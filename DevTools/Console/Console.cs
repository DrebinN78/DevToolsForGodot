#if TOOLS
using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using ImGuiNET;
namespace DevToolsForGodot
{
    public partial class Console : DevTool, IDevToolInfo
    {
        public string toolname { get => "Console"; }
        public string tooltip { get => "Allows user to execute commands and read and change variable values"; }
        public string category { get => DevTools.developerToolCat; }
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
        struct Variable
        {
            public string name;
            public Type valueType;
            public string tooltip;
            public string valueTooltip;
            public Action<object> wrappedSetMethod;
            public object wrappedGetMethod;
        }
        static string currentConsoleInput = "";
        //T0 is Exit Code, T1 is Command
        static Dictionary<string, CommandHistoryTheme> consoleHistory = new();
        static List<Command> availableCommands = new();
        static List<Variable> availableVariables = new();
        static System.Numerics.Vector4 consoleFailure = new(255, 0, 0, 255);
        static System.Numerics.Vector4 consoleError = new(255, 0, 0, 255);
        static System.Numerics.Vector4 consoleSuccess = new(0, 255, 0, 255);
        static System.Numerics.Vector4 consoleDefault = new(255, 255, 255, 255);
        public override void ToolSetup()
        {
            FetchAllConsoleItems();
        }

        public override void ToolLoop()
        {

            //ImGui.ShowDemoWindow();
            ImGui.Begin("Console");
            /*
                        if (ImGui.BeginMenuBar())
                        {
                            if (ImGui.BeginMenu("Menu"))
                            {
                                ImGui.EndMenu();
                            }
                            if (ImGui.BeginMenu("Examples"))
                            {
                                ImGui.MenuItem("Main menu bar");
                                ImGui.MenuItem("Console");
                                ImGui.MenuItem("Log");
                                ImGui.MenuItem("Simple layout");
                                ImGui.MenuItem("Property editor");
                                ImGui.MenuItem("Long text display");
                                ImGui.MenuItem("Auto-resizing window");
                                ImGui.MenuItem("Constrained-resizing window");
                                ImGui.MenuItem("Simple overlay");
                                ImGui.MenuItem("Manipulating window title");
                                ImGui.MenuItem("Custom rendering");
                                ImGui.EndMenu();
                            }
                            if (ImGui.BeginMenu("Help"))
                            {
                                ImGui.MenuItem("Metrics");
                                ImGui.MenuItem("Style Editor");
                                ImGui.MenuItem("About ImGui");
                                ImGui.EndMenu();
                            }
                            ImGui.EndMenuBar();
                        }
                        */
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
                foreach (var value in availableVariables)
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
            foreach (var value in availableVariables)
            {
                if (value.name == pack[0].ToString())
                {
                    AddConsoleHistoryEntry(value.tooltip, CommandHistoryTheme.Default);
                    return;
                }
            }
            AddConsoleHistoryEntry("Could not find info for '" + pack[0] + "'", CommandHistoryTheme.Default);
        }
        void FetchAllConsoleItems()
        {
            List<Type> typeList = new();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                typeList.AddRange(assembly.GetTypes());
            foreach (Type type in typeList)
            {
                //Command Fetch Loop
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
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
                foreach (var property in type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (!Attribute.IsDefined(property, typeof(ConsoleValue), true)) continue;
                    Variable variable = new();

                    var setMethod = property.GetSetMethod();
                    var getMethod = property.GetGetMethod();
                    variable.name = property.Name;
                    variable.wrappedGetMethod = delegate () { return getMethod.Invoke(null, null); };
                    variable.wrappedSetMethod = delegate (object value) { setMethod.Invoke(null, new object[] { value }); };
                    object[] attrs = property.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        if (attr is ConsoleValue valueAttr)
                        {
                            variable.valueType = property.PropertyType;
                            variable.tooltip = valueAttr.ttip;
                            variable.valueTooltip = valueAttr.vttip;
                        }

                    }
                    availableVariables.Add(variable);
                }
            }
        }
    }
}
#endif