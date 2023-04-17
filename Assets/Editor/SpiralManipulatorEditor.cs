using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static NomaiTextArcBuilder;

[CustomEditor(typeof(SpiralManipulator))]
public class SpiralManipulatorInspector : Editor 
{
    public override void OnInspectorGUI() 
    {
        SpiralManipulator myTarget = (SpiralManipulator)target;

        var newPoint = EditorGUILayout.IntSlider(myTarget._parentPointIndex, NomaiTextArcBuilder.MIN_PARENT_POINT, NomaiTextArcBuilder.MAX_PARENT_POINT);
        if (newPoint != myTarget._parentPointIndex) 
        {
            myTarget._parentPointIndex = newPoint;
            SpiralManipulator.PlaceChildOnParentPoint(myTarget.gameObject, myTarget.parent.gameObject, newPoint);
        }
        
        if (GUILayout.Button("Mirror"))
        {
            myTarget.Mirror();
        }

        if (GUILayout.Button("Add Child"))
        {
            myTarget.AddChild();
        }
    }
}
