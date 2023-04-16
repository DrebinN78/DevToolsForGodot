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
        const string consoleAction = "ToggleDevConsole";
        public override void _EnterTree()
        {
            AddAutoloadSingleton("DevTools", "res://addons/devtoolsforgodot/DevTools.tscn");
            InputMap.AddAction(consoleAction);
            InputEventKey ev = new();
            ev.Keycode = Key.Quoteleft;
            InputMap.ActionAddEvent(consoleAction, ev);
        }

        public override void _ExitTree()
        {
            RemoveAutoloadSingleton("DevTools");
            InputMap.EraseAction(consoleAction);
        }
    }
}
#endif