#if TOOLS
using Godot;
using Godot.Collections;
using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Reflection;
using System.Numerics;

namespace DevToolsForGodot
{
    [Tool]
    public partial class DevTools : EditorPlugin
    {
        public const string developerToolCat = "Developer Tools";
        public const string sceneToolCat = "Scene Tools";
        public const string prefsToolCat = "Preferences";
        const string devtoolScenePath = "res://addons/devtoolsforgodot/Scenes/DevTools.tscn";
        const string devtoolWindowScenePath = "res://addons/devtoolsforgodot/Scenes/DevToolDock.tscn";
        public static List<Type> availableTools { get => FetchAllTools(); }
        Control devtoolSettingsWindow;
        public override void _EnterTree()
        {
            AddDevToolSceneToAutoLoad();
            AddDevToolsSettingsWindow();
            FetchAllTools();
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
        static List<Type> FetchAllTools()
        {
            List<Type> toolList = new List<Type>();
            List<Type> typeList = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
                typeList.AddRange(assembly.GetTypes());
            foreach (Type type in typeList)
            {
                if (type.IsSubclassOf(typeof(DevTool)))
                    toolList.Add(type);
            }
            return toolList;

        }
    }
}
#endif