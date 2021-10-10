#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpookyTilemap))]
[CanEditMultipleObjects]
public class SpookyTilemapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate Geo"))
        {
            SpookyTilemap spookyTilemap = (SpookyTilemap)serializedObject.targetObject;
            if (spookyTilemap)
            {
                spookyTilemap.GenerateGeo();
            }
        }
        DrawDefaultInspector();
    }
}
#endif