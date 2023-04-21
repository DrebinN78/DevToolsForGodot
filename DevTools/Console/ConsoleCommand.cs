using System;
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