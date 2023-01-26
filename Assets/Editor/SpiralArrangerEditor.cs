using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NomaiTextArcArranger))]
public class SpiralArrangerEditor : Editor 
{
    SerializedProperty m_minX;
    SerializedProperty m_maxX;
    SerializedProperty m_minY;
    SerializedProperty m_maxY;

    void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        m_minX = serializedObject.FindProperty("minX");
        m_maxX = serializedObject.FindProperty("maxX");
        m_minY = serializedObject.FindProperty("minY");
        m_maxY = serializedObject.FindProperty("maxY");
    }

    public override void OnInspectorGUI() 
    {
        NomaiTextArcArranger myTarget = (NomaiTextArcArranger)target;
        
        GUILayout.Label("Bounds");
        GUILayout.BeginHorizontal();
            GUILayout.Label("X");
            EditorGUILayout.PropertyField(m_minX, GUIContent.none);
            EditorGUILayout.PropertyField(m_maxX, GUIContent.none);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
            GUILayout.Label("Y");
            EditorGUILayout.PropertyField(m_minY, GUIContent.none);
            EditorGUILayout.PropertyField(m_maxY, GUIContent.none);
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();



        if (GUILayout.Button("Backtrack"))
        {
            myTarget.Backtrack();
        }

        if (GUILayout.Button("Step"))
        {
            myTarget.Step();
        }
        
        if (GUILayout.Button("Step 10"))
        {
            for(var i = 0; i < 10; i++) myTarget.Step();
        }
        
        if (GUILayout.Button("Overlap Check"))
        {
            Debug.Log("Overlap found: " + myTarget.Overlap());
        }

        if (GUILayout.Button("Attempt to fix overlap"))
        {
            for (var k = 0; k < myTarget.spirals.Count*2; k++) 
            {
                var overlap = myTarget.Overlap();
                if (overlap.x < 0) return;

                var indexMirrored = myTarget.AttemptOverlapResolution(overlap);
                Debug.Log("Mirrored spiral " + indexMirrored);
                for(var i = 0; i < 10; i++) myTarget.Step();
            }

            Debug.Log("Overlap resolution failed!");
        }
    }
}
