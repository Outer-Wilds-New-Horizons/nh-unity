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
            SpiralManipulator.PlaceChildOnParentPoint(myTarget.gameObject, myTarget.parent.gameObject, newPoint);
        }
        
        if (GUILayout.Button("Mirror"))
        {
            myTarget.Mirror();
            foreach(var child in myTarget.children) 
            {
                SpiralManipulator.PlaceChildOnParentPoint(child.gameObject, myTarget.gameObject, child._parentPointIndex);
            }
        }

        if (GUILayout.Button("Add Child"))
        {
            myTarget.AddChild();
        }
    }
}
