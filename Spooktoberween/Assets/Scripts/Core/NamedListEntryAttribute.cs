using UnityEngine;

public class NamedListEntryAttribute : PropertyAttribute
{
    public string nameProperty;

    public NamedListEntryAttribute(string NameProperty)
    {
        nameProperty = NameProperty;
    }
}
