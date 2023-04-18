#if TOOLS
using Godot;
using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace DevToolsForGodot
{
    [Tool]
    public partial class DevTools : EditorPlugin
    {

        const string devtoolScenePath = "res://addons/devtoolsforgodot/DevTools.tscn";
        const string devtoolWindowScenePath = "res://addons/devtoolsforgodot/DevTools.tscn";
        Control devtoolSettingsWindow;
        public override void _EnterTree()
        {
            AddDevToolSceneToAutoLoad();
            AddDevToolsSettingsWindow();
        }

        public override void _ExitTree()
        {
            RemoveDevToolSceneFromAutoLoad();
            RemoveDevToolsSettingsWindow();
        }

        void AddDevToolSceneToAutoLoad() => AddAutoloadSingleton("DevTools", devtoolScenePath);
        void RemoveDevToolSceneFromAutoLoad() => RemoveAutoloadSingleton("DevTools");
        void AddDevToolsSettingsWindow()
        {
            devtoolSettingsWindow = (Control)GD.Load<PackedScene>(devtoolWindowScenePath).Instantiate();
            AddControlToDock(DockSlot.LeftUl, devtoolSettingsWindow);
        }
        void RemoveDevToolsSettingsWindow()
        {
            RemoveControlFromDocks(devtoolSettingsWindow);
            devtoolSettingsWindow.Free();
        }


    }
}
#endif