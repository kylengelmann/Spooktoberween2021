using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(NamedListEntryAttribute))]
public class NamedListEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        NamedListEntryAttribute EntryAttribute = (NamedListEntryAttribute)attribute;
        
        string labelString = "";
        SerializedProperty nameProp = property.FindPropertyRelative(EntryAttribute.nameProperty);
        if (nameProp != null)
        {
            switch(nameProp.propertyType)
            {
                case SerializedPropertyType.String:
                    labelString = nameProp.stringValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    if(nameProp.objectReferenceValue)
                    {
                        labelString = nameProp.objectReferenceValue.name;
                    }
                    break;
                case SerializedPropertyType.Enum:
                    int typeValue = nameProp.enumValueIndex;
                    if (nameProp.enumDisplayNames.Length > typeValue && typeValue >= 0)
                    {
                        labelString = nameProp.enumDisplayNames[typeValue];
                    }
                    break;
            }

            if(!string.IsNullOrWhiteSpace(labelString))
            {
                label = new GUIContent(labelString);
            }
        }

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }
}
