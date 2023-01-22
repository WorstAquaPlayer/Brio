﻿using Brio.Core;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;

namespace Brio.Game.GPose;

public class GPoseService : ServiceBase<GPoseService>
{
    public GPoseState GPoseState { get; private set; }
    public bool IsInGPose => GPoseState == GPoseState.Inside || FakeGPose;
    public bool FakeGPose { get; set; } = false;

    public delegate void OnGPoseStateDelegate(GPoseState gposeState);
    public event OnGPoseStateDelegate? OnGPoseStateChange;

    private delegate void ExitGPoseDelegate(IntPtr addr);
    private Hook<ExitGPoseDelegate>? _exitGPoseHook = null;

    private delegate bool EnterGPoseDelegate(IntPtr addr);
    private Hook<EnterGPoseDelegate>? _enterGPoseHook = null;

    public override unsafe void Start()
    {
        GPoseState = Dalamud.PluginInterface.UiBuilder.GposeActive ? GPoseState.Inside : GPoseState.Outside;


        // TODO: Move to clientstructs
        // Track: https://github.com/aers/FFXIVClientStructs/pull/291
        var framework = Framework.Instance();
        if(framework == null)
            throw new Exception("Framework not found");

        var uiModule = framework->GetUiModule();
        if(uiModule == null)
            throw new Exception("Could not get UI module");

        var enterGPoseAddress = (nint)uiModule->vfunc[75];
        if(enterGPoseAddress == 0)
            throw new Exception("Could not get EnterGPose address");

        var exitGPoseAddress = (nint)uiModule->vfunc[76];
        if(exitGPoseAddress == 0)
            throw new Exception("Could not get ExitGPose address");

        _enterGPoseHook = Hook<EnterGPoseDelegate>.FromAddress(enterGPoseAddress, EnteringGPoseDetour);
        _enterGPoseHook.Enable();

        _exitGPoseHook = Hook<ExitGPoseDelegate>.FromAddress(exitGPoseAddress, ExitingGPoseDetour);
        _exitGPoseHook.Enable();

        base.Start();
    }

    private void ExitingGPoseDetour(IntPtr addr)
    {
        HandleGPoseChange(GPoseState.Exiting);
        _exitGPoseHook!.Original.Invoke(addr);
        HandleGPoseChange(GPoseState.Outside);
    }

    private bool EnteringGPoseDetour(IntPtr addr)
    {
        bool didEnter = _enterGPoseHook!.Original.Invoke(addr);
        if(didEnter)
            HandleGPoseChange(GPoseState.Inside);

        return didEnter;
    }

    private void HandleGPoseChange(GPoseState state)
    {
        GPoseState = state;
        OnGPoseStateChange?.Invoke(state);
    }

    public override void Dispose()
    {
        _exitGPoseHook?.Dispose();
        _enterGPoseHook?.Dispose();
    }
}

public enum GPoseState
{
    Inside,
    Exiting,
    Outside
}
