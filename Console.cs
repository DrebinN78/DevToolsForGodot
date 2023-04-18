using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using ImGuiNET;
[AttributeUsage(AttributeTargets.Method)]
public class ConsoleCommand : Attribute
{
    public string ttip;
    public string pptip;
    public ConsoleCommand()
    {

    }
    public ConsoleCommand(string tooltip)
    {
        ttip = tooltip;
    }
    public ConsoleCommand(string tooltip, string parampacktip)
    {
        ttip = tooltip;
        pptip = parampacktip;
    }
}
[AttributeUsage(AttributeTargets.Property)]
public class ConsoleValue : Attribute
{
    public string ttip;
    public string vttip;
    public ConsoleValue()
    {
    }
    public ConsoleValue(string tooltip)
    {
        ttip = tooltip;
    }
    public ConsoleValue(string tooltip, string valueTooltip)
    {
        ttip = tooltip;
        vttip = valueTooltip;
    }
}
namespace DevToolsForGodot
{
    public partial class Console : Node
    {
        public enum CommandHistoryTheme
        {
            //Used for common text
            Default,
            //Used user input was processed succesfully
            Success,
            //Used when the command implementation leads to an error
            Failure,
            //Used when the user input leads to an error
            Error
        }
        struct Command
        {
            public string name;
            public string tooltip;
            public Action<object[]> wrappedMethod;
        }
        struct Value
        {
            public string name;
            public Type valueType;
            public string tooltip;
            public string valueTooltip;
            public Action<object> wrappedSetMethod;
            public object wrappedGetMethod;
        }
        const string consoleAction = "ToggleDevConsole";
        static Key consoleHotkey = Key.Semicolon;
        static double previousTimeScale;
        static string currentConsoleInput = "";
        //T0 is Exit Code, T1 is Command
        static Dictionary<string, CommandHistoryTheme> consoleHistory = new();
        static List<Command> availableCommands = new();
        static List<Value> availableValues = new();
        static bool active = false;
        static System.Numerics.Vector4 consoleFailure = new(255, 0, 0, 255);
        static System.Numerics.Vector4 consoleError = new(255, 0, 0, 255);
        static System.Numerics.Vector4 consoleSuccess = new(0, 255, 0, 255);
        static System.Numerics.Vector4 consoleDefault = new(255, 255, 255, 255);
        public override void _EnterTree()
        {
            AddDevToolConsoleHotKey();
            FetchAllConsoleItems();
        }
        public override void _ExitTree()
        {
            RemoveDevToolConsoleHotKey();
        }
        public override void _Process(double delta)
        {
            if (!OS.HasFeature("editor") || !active) return;
            //ImGui.ShowDemoWindow();
            ImGui.Begin("Godot Console");
            ImGui.PushItemWidth(-1);
            if (ImGui.InputText("", ref currentConsoleInput, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoHorizontalScroll))
            {
                List<string> splitConsoleInput = new List<string>(currentConsoleInput.Split(' '));
                string order = splitConsoleInput[0];
                splitConsoleInput.RemoveAt(0);
                List<object> parampack = new();
                foreach (var param in splitConsoleInput)
                    parampack.Add(param);
                consoleHistory.Add("> " + currentConsoleInput, CommandHistoryTheme.Default);
                currentConsoleInput = "";
                Execute(order, parampack.ToArray());
            }
            ImGui.PopItemWidth();
            ImGui.BeginChild("Scrolling");
            foreach (var entry in consoleHistory)
            {
                switch (entry.Value)
                {
                    case CommandHistoryTheme.Success: ImGui.TextColored(consoleSuccess, entry.Key); break;
                    case CommandHistoryTheme.Failure: ImGui.TextColored(consoleFailure, entry.Key); break;
                    case CommandHistoryTheme.Default: ImGui.TextColored(consoleDefault, entry.Key); break;
                }
            }
            ImGui.EndChild();
            ImGui.End();

        }
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed(consoleAction))
                active = !active;
        }
        public void Execute(string input, object[] pack)
        {
            foreach (var command in availableCommands)
            {
                if (input == command.name)
                {

                    command.wrappedMethod.Invoke(pack);
                    consoleHistory.Add(" ", CommandHistoryTheme.Default);
                    return;
                }

            }
            if (pack.Length == 1)
            {
                foreach (var value in availableValues)
                {
                    if (value.name == input)
                    {
                        value.wrappedSetMethod.Invoke(Convert.ChangeType(pack[0], value.valueType));
                    }
                }
            }
            consoleHistory.Add(input + "does not exist", CommandHistoryTheme.Failure);
        }
        public static void AddConsoleHistoryEntry(string entry, CommandHistoryTheme theme) => consoleHistory.Add(entry, theme);
        [ConsoleCommand("Clears console history")]
        static void Clear(object[] pack) => consoleHistory.Clear();
        [ConsoleCommand("Writes in console the desired output")]
        static void Echo(object[] pack)
        {
            string exit = "";
            foreach (object param in pack)
            {
                exit += param.ToString();
            }
            AddConsoleHistoryEntry(exit, CommandHistoryTheme.Default);
        }
        [ConsoleCommand("Displays tooltip of the selected itea")]
        static void Help(object[] pack)
        {
            foreach (var command in availableCommands)
            {
                if (command.name == pack[0].ToString())
                {
                    AddConsoleHistoryEntry(command.tooltip, CommandHistoryTheme.Default);
                    return;
                }
            }
            foreach (var value in availableValues)
            {
                if (value.name == pack[0].ToString())
                {
                    AddConsoleHistoryEntry(value.tooltip, CommandHistoryTheme.Default);
                    return;
                }
            }
            AddConsoleHistoryEntry("Could not find info for '" + pack[0] + "'", CommandHistoryTheme.Default);
        }
        void AddDevToolConsoleHotKey()
        {
            InputMap.AddAction(consoleAction);
            InputEventKey ev = new();
            ev.Keycode = consoleHotkey;
            InputMap.ActionAddEvent(consoleAction, ev);
        }
        void RemoveDevToolConsoleHotKey() => InputMap.EraseAction(consoleAction);
        void FetchAllConsoleItems()
        {
            List<Type> typeList = new();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
                typeList.AddRange(assembly.GetTypes());
            GD.Print("Type count : " + typeList.Count);
            foreach (Type type in typeList)
            {
                //Command Fetch Loop
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
                {
                    if (!Attribute.IsDefined(method, typeof(ConsoleCommand), true)) continue;
                    Command cmd = new();
                    cmd.name = method.Name;
                    cmd.wrappedMethod = delegate (object[] parampack) { method.Invoke(null, new object[] { parampack }); };
                    object[] attrs = method.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        if (attr is ConsoleCommand commandAttr)
                            cmd.tooltip = commandAttr.ttip;
                    }
                    availableCommands.Add(cmd);
                }
                //Value Fetch Loop
                foreach (var property in type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
                {
                    if (!Attribute.IsDefined(property, typeof(ConsoleValue), true)) continue;
                    Value value = new();

                    var setMethod = property.GetSetMethod();
                    var getMethod = property.GetGetMethod();
                    value.name = property.Name;
                    value.wrappedGetMethod = delegate () { return getMethod.Invoke(null, null); };
                    value.wrappedSetMethod = delegate (object value) { setMethod.Invoke(null, new object[] { value }); };
                    object[] attrs = property.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        if (attr is ConsoleValue valueAttr)
                        {
                            value.valueType = property.PropertyType;
                            value.tooltip = valueAttr.ttip;
                            value.valueTooltip = valueAttr.vttip;
                        }

                    }
                    availableValues.Add(value);
                }
            }
        }
    }
}


public static class TestClass
{
    [ConsoleValue("Test int")]
    static int TestInt { get; set; } = 100;
}