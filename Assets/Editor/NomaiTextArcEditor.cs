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
        if (GUILayout.Button("Place New Conversation"))
        {
			var root = NomaiTextArcArranger.Place();
            root.gameObject.name = "Spiral 0";
        }
        
        if (GUILayout.Button("Place Random Conversation"))
        {
			var root = NomaiTextArcArranger.Place().GetComponent<SpiralManipulator>();
            root.gameObject.name = "Spiral 0";
            generateChildren(root);
        }
    }

    private void generateChildren(SpiralManipulator root, int depth = 0) {
        if (depth > 2) return;
        else if (depth > 0 && Random.value < 0.5f) return;

        for(var i = 0; i < Random.Range(1, 3); i++) 
        {
            var child = root.AddChild();
            generateChildren(child, depth+1);
        }
    }
}
