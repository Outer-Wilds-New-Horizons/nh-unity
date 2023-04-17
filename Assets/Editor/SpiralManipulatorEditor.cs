using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpiralManipulator))]
public class SpiralManipulatorInspector : Editor 
{
    public override void OnInspectorGUI() 
    {
        SpiralManipulator myTarget = (SpiralManipulator)target;

        var newPoint = EditorGUILayout.IntSlider(myTarget._parentPointIndex, SpiralManipulator.MIN_PARENT_POINT, SpiralManipulator.MAX_PARENT_POINT);
        if (newPoint != myTarget._parentPointIndex) 
        {
            myTarget._parentPointIndex = newPoint;
            SpiralManipulator.PlaceChildOnParentPoint(myTarget, myTarget.parent, newPoint);
        }
        
        if (GUILayout.Button("Mirror"))
        {
            myTarget.Mirror();
            myTarget.UpdateChildren();
        }

        if (GUILayout.Button("Add Child"))
        {
            myTarget.AddChild();
        }
    }
}
