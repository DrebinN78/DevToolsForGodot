#if TOOLS
using Godot;
using System;

namespace DevToolsForGodot
{
    public partial class DevTool : Node
    {
        public bool active = false;
        public sealed override void _EnterTree() => ToolSetup();
        public sealed override void _Process(double time)
        {
            if (DevToolsHub.devToolLayerActive && active)
                ToolLoop();
        }
        public sealed override void _ExitTree() { }
        public sealed override void _Input(InputEvent @event) { }
        public virtual void ToolSetup() { }
        public virtual void ToolLoop() { }
    }
}
#endif