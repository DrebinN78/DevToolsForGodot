#if TOOLS
using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using ImGuiNET;
namespace DevToolsForGodot
{
    public partial class PreferenceWindow : DevTool, IDevToolInfo
    {
        public string toolname { get => "DevToolsForGodot Preference Window"; }
        public string tooltip { get => "Centralise preferences for the DevToolsForGodot Suite"; }
        public string category { get => DevTools.prefsToolCat; }

        static Dictionary<Type, List<FieldInfo>> preferenceFields = new();


        public override void ToolSetup()
        {
            FetchPreferences();
        }

        public override void ToolLoop()
        {
            //ImGui.ShowDemoWindow();
            ImGui.Begin("Dev Tools For Godot Preference Window");
            if (ImGui.BeginTabBar(""))
            {
                foreach (var toolCategory in preferenceFields.Keys)
                {
                    if (ImGui.BeginTabItem(toolCategory.Name))
                    {
                        foreach (var preference in preferenceFields[toolCategory])
                        {
                            switch (preference.GetValue(null))
                            {
                                case int a: ImGui.InputInt(preference.Name, ref a); preference.SetValue(null, a); break;
                                case bool b: ImGui.Checkbox(preference.Name, ref b); preference.SetValue(null, b); break;
                                case Key key:
                                    {
                                        var items = Enum.GetValues(typeof(Key));
                                        Key current_item = key;
                                        if (ImGui.BeginCombo(preference.Name, key.ToString())) // The second parameter is the label previewed before opening the combo.
                                        {
                                            foreach (Key keyCode in items)
                                            {
                                                bool is_selected = (current_item == key); // You can store your selection however you want, outside or inside your objects
                                                if (ImGui.Selectable(keyCode.ToString(), is_selected))
                                                    current_item = keyCode;
                                                if (is_selected)
                                                    ImGui.SetItemDefaultFocus();   // You may set the initial focus when opening the combo (scrolling + for keyboard navigation support)
                                            }
                                            ImGui.EndCombo();
                                        }
                                    }
                                    ImGui.BeginCombo(preference.Name, key.ToString()); foreach (var keyCode in Enum.GetValues(typeof(Key))) ImGui.Selectable(keyCode.ToString()); ImGui.EndCombo(); break;
                            }
                        }
                        ImGui.EndTabItem();
                    }
                }

            }
            ImGui.End();

        }

        public static void FetchPreferences()
        {
            DevToolHubPreferences();

            foreach (Type toolType in DevTools.availableTools)
            {
                preferenceFields.Add(toolType, new());
                foreach (var field in toolType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (!Attribute.IsDefined(field, typeof(Preference), true)) continue;
                    preferenceFields[toolType].Add(field);
                }
            }
        }

        static void DevToolHubPreferences()
        {
            //DevToolsHub section
            preferenceFields.Add(typeof(DevToolsHub), new());

            //tools hotkey
            preferenceFields[typeof(DevToolsHub)].Add(typeof(DevToolsHub).GetField("devtoolHotkey", BindingFlags.Static | BindingFlags.NonPublic));
        }




    }
}
#endif