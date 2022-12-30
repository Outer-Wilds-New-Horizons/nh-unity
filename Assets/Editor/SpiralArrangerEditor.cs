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
