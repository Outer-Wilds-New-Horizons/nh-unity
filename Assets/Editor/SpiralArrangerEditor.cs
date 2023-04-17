using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static NomaiTextArcBuilder;

[CustomEditor(typeof(SpiralArranger))]
public class SpiralArrangerEditor : Editor 
{
    public override void OnInspectorGUI() 
    {
        SpiralArranger myTarget = (SpiralArranger)target;
        
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
            var overlap = myTarget.Overlap();
            if (overlap.x < 0) return;

            var success = myTarget.AttemptOverlapResolution(overlap);
            for(var i = 0; i < 10; i++) myTarget.Step();
        }
    }
}
