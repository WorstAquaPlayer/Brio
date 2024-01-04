﻿using Brio.Capabilities.Posing;
using Brio.Game.Posing;
using Brio.UI.Controls.Stateless;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace Brio.UI.Controls.Editors;
internal class BoneIKEditor
{
    public static void Draw(BonePoseInfo poseInfo, PosingCapability posing)
    {
        bool didChange = false;

        var ik = poseInfo.DefaultIK;

        if(ImGui.Checkbox("Enabled", ref ik.Enabled))
        {
            didChange |= true;
        }

        ImGui.SameLine();

        if(ImBrio.FontIconButtonRight("snapshot", FontAwesomeIcon.CameraRetro, 1.1f, "Apply IK Snapshot", poseInfo.Parent.HasIKStacks))
            posing.SnapshotIK();

        using(ImRaii.Disabled(!ik.Enabled))
        {

            if(ImGui.Checkbox("Enforce Constraints", ref ik.EnforceConstraints))
            {
                didChange |= true;
            }

            if(ImGui.SliderInt("Depth", ref ik.Depth, 1, 20))
            {
                didChange |= true;
            }

            if(ImGui.SliderInt("Iterations", ref ik.Iterations, 1, 20))
            {
                didChange |= true;
            }
        }

        if(didChange)
        {
            poseInfo.DefaultIK = ik;
        }
    }
}