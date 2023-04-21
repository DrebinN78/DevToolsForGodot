#if TOOLS
using Godot;
using System;
using System.Runtime.Remoting;
using System.Reflection;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace DevToolsForGodot
{
    public partial class DevToolsHub : Node
    {
        const string undocumentedToolCategoryName = "Uncategorized";
        public static bool devToolLayerActive;
        public static Dictionary<string, List<DevTool>> toolNodeList = new();
        public static Dictionary<string, bool> toolTabsList = new();
        public static string activeToolTab = "UNINITIALIZED";
        const string devtoolAction = "ToggleDevConsole";
        static Key devtoolHotkey = Key.Semicolon;
        public override void _Ready()
        {
            AddDevToolLayerHotKey();
            SpawnAllTools();
        }
        public override void _ExitTree()
        {
            RemoveDevToolConsoleHotKey();
        }
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed(devtoolAction))
            {
                devToolLayerActive = !devToolLayerActive;
            }

        }
        public override void _Process(double delta)
        {
            if (!devToolLayerActive) return;
            ImGuiLoop();
        }
        void ImGuiLoop()
        {
            ImGui.Begin("Godot Dev Tool Suite Hub");
            if (ImGui.BeginTabBar(""))
            {
                foreach (var toolCat in toolTabsList.Keys)
                {
                    if (ImGui.BeginTabItem(toolCat))
                    {
                        foreach (var tool in toolNodeList[toolCat])
                        {
                            if (tool is IDevToolInfo toolInfo)
                            {
                                if (toolInfo.category != toolCat) continue;
                                ImGui.Checkbox(toolInfo.toolname, ref tool.active);
                                if (ImGui.IsItemHovered())
                                    ImGui.SetTooltip(toolInfo.tooltip);
                            }
                            else
                            {
                                if (toolCat == undocumentedToolCategoryName)
                                    ImGui.Checkbox(tool.GetInstanceId().ToString(), ref tool.active);
                            }

                        }
                        ImGui.EndTabItem();
                    }
                }
                ImGui.EndTabBar();
            }
            ImGui.End();
        }
        void AddDevToolLayerHotKey()
        {
            InputMap.AddAction(devtoolAction);
            InputEventKey ev = new();
            ev.Keycode = devtoolHotkey;
            InputMap.ActionAddEvent(devtoolAction, ev);
        }
        void RemoveDevToolConsoleHotKey() => InputMap.EraseAction(devtoolAction);
        void SpawnAllTools()
        {
            foreach (Type tool in DevTools.availableTools)
            {
                DevTool node = (DevTool)Activator.CreateInstance(null, tool.ToString()).Unwrap();
                if (node is IDevToolInfo toolInfo)
                {
                    if (toolNodeList.ContainsKey(toolInfo.category))
                        toolNodeList[toolInfo.category].Add(node);
                    else
                        toolNodeList.Add(toolInfo.category, new List<DevTool>() { node });
                }
                else
                {
                    if (toolNodeList.ContainsKey(undocumentedToolCategoryName))
                        toolNodeList[undocumentedToolCategoryName].Add(node);
                    else
                        toolNodeList.Add(undocumentedToolCategoryName, new List<DevTool>() { node });
                }
                AddChild(node);
            }
            foreach (var key in toolNodeList.Keys)
            {
                if (toolTabsList.Count == 0)
                    toolTabsList.Add(key, true);
                else
                    toolTabsList.Add(key, false);
            }

        }
    }
}

#endif