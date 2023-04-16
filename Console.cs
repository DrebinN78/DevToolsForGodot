using Godot;
using System;
using System.Collections.Generic;
using ImGuiNET;

public partial class DeveloppmentConsole : Node
{
    double previousTimeScale;
    string currentConsoleInput = "";
    Dictionary<string, string> commandResults = new();
    bool active = false;
    System.Numerics.Vector4 consoleError = new(255, 0, 0, 255);
    System.Numerics.Vector4 consoleSuccess = new(0, 255, 0, 255);
    public override void _Process(double delta)
    {
        if (!OS.HasFeature("editor") || !active) return;
        Engine.TimeScale = 0;
        //ImGui.ShowDemoWindow();
        ImGui.Begin("Lowkey Console");
        ImGui.PushItemWidth(-1);
        if (ImGui.InputText("", ref currentConsoleInput, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoHorizontalScroll))
        {
            string command = currentConsoleInput;
            currentConsoleInput = "";
            bool result = Execute(command);
            if (command != "clear")
                commandResults.Add(command, Answer(command, result));
        }
        ImGui.PopItemWidth();
        ImGui.BeginChild("Scrolling");
        foreach (var command in commandResults)
        {
            ImGui.Text("> " + command.Key);
            if (command.Value.Length == 0) continue;
            if (command.Value[0] == '0')
                ImGui.TextColored(consoleError, command.Value.Substr(1, command.Value.Length));
            else
                ImGui.TextColored(consoleSuccess, command.Value.Substr(1, command.Value.Length));
            ImGui.Text("");
        }
        ImGui.EndChild();
        ImGui.End();

    }
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("toggledevconsole"))
        {
            active = !active;
            if (active)
            {
                previousTimeScale = Engine.TimeScale;
            }
            else
            {
                Engine.TimeScale = previousTimeScale;
            }
        }


    }
    public bool Execute(string command)
    {
        switch (command)
        {
            case "clear":
                commandResults.Clear();
                return true;
            case "quitgame":
                GetTree().Quit();
                return true;
            default:
                return false;
        }
    }
    public string Answer(string command, bool result)
    {
        //preface console message with 1 for success, 0 for failure
        switch (command)
        {
            case "clear":
                return "1";
            case "quitgame":
                return "1";
            default:
                return "0'" + command + "' command has not been implemented";
        }
    }
}