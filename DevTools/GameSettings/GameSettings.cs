#if TOOLS
using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using ImGuiNET;
namespace DevToolsForGodot
{
    public partial class GameSettings : DevTool, IDevToolInfo
    {
        public string toolname { get => "Game Settings Window"; }
        public string tooltip { get => "Allows you to change generic game settings such as render resolution or locale"; }
        public string category { get => DevTools.prefsToolCat; }

        //static uint renderHeight = 600;
        //static uint renderWidth = 800;
        //static uint renderRefreshRate = 60;




        public override void ToolSetup()
        {
        }

        public override void ToolLoop()
        {
            //ImGui.ShowDemoWindow();
            ImGui.Begin("Dev Tools For Godot Preference Window");
            if (ImGui.BeginTabBar(""))
            {
                if (ImGui.BeginTabItem("Rendering"))
                {

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Localisation"))
                {

                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            ImGui.End();

        }

        static void SetRenderWidth(uint width) => ProjectSettings.SetSetting("display/window/size/width", width);
        static void SetRenderHeight(uint height) => ProjectSettings.SetSetting("display/window/size/height", height);
    }
}
#endif