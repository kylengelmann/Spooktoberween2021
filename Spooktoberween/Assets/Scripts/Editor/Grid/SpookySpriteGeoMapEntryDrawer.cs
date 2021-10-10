using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SpookySpriteGeoMapEntryAttribute))]
public class SpookySpriteGeoMapEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty geoProp = property.FindPropertyRelative("geo");
        if(geoProp != null)
        {
            GameObject geo = (GameObject)geoProp.objectReferenceValue;
            if(geo)
            {
                label = new GUIContent(geo.name);
            }
        }

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }
}
