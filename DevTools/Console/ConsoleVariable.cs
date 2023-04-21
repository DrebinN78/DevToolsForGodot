using System;
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