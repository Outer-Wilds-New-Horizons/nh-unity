using UnityEngine;
using UnityEditor;

public class NomaiTextArcEditor : EditorWindow
{
    [MenuItem("Tools/Nomai Text Arc Builder")]
    static void CreateReplaceWithPrefab()
    {
        EditorWindow.GetWindow<NomaiTextArcEditor>();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Place Arc"))
        {
			NomaiTextArcBuilder.Place();
        }
    }
}