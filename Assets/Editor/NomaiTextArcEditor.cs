using UnityEngine;
using UnityEditor;
using static NomaiTextArcBuilder;

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
        
        if (GUILayout.Button("Place Random Conversation"))
        {
			var root = NomaiTextArcBuilder.Place().GetComponent<NomaiTextArcBuilder.SpiralManipulator>();
            generateChildren(root);
        }
    }

    private void generateChildren(NomaiTextArcBuilder.SpiralManipulator root, int depth = 0) {
        if (depth > 2) return;
        else if (depth > 0 && Random.value < 0.5f) return;

        for(var i = 0; i < Random.Range(1, 3); i++) 
        {
            var child = root.AddChild();
            generateChildren(child, depth+1);
        }
    }
}
