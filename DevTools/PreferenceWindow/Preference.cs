using System;

[AttributeUsage(AttributeTargets.Field)]
public class Preference : Attribute
{
    public string ttip;
}