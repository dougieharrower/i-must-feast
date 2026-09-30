using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TreelineGeneratorAdvanced))]
public class TreelineGeneratorAdvancedEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var gen = (TreelineGeneratorAdvanced)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Generator Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate (Use Seed)"))
        {
            gen.GenerateWithSeed();
            EditorUtility.SetDirty(gen);
        }
        if (GUILayout.Button("New Seed + Generate"))
        {
            Undo.RecordObject(gen, "Change Seed");
            gen.randomSeed = Random.Range(int.MinValue, int.MaxValue);
            gen.GenerateWithSeed();
            EditorUtility.SetDirty(gen);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Generated"))
        {
            gen.ClearGenerated();
            EditorUtility.SetDirty(gen);
        }
#if UNITY_EDITOR
        if (GUILayout.Button("Bake to New Parent"))
        {
            gen.BakeToNewParent();
        }
#endif
        EditorGUILayout.EndHorizontal();
    }
}
