using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(CorgiController))]
[CanEditMultipleObjects]

/// <summary>
/// Adds custom labels to the CorgiController inspector
/// </summary>

public class CorgiControllerInspector : Editor 
{
	
	void onEnable()
	{
		// nothing
	}
	
	/// <summary>
	/// When inspecting a Corgi Controller, we add to the regular inspector some labels, useful for debugging
	/// </summary>
	public override void OnInspectorGUI()
	{
		CorgiController controller = (CorgiController)target;
		if (controller.State!=null)
		{
			EditorGUILayout.LabelField("Grounded",controller.State.IsGrounded.ToString());
			EditorGUILayout.LabelField("Falling",controller.State.IsFalling.ToString());
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Colliding Left",controller.State.IsCollidingLeft.ToString());
			EditorGUILayout.LabelField("Colliding Right",controller.State.IsCollidingRight.ToString());
			EditorGUILayout.LabelField("Colliding Above",controller.State.IsCollidingAbove.ToString());
			EditorGUILayout.LabelField("Colliding Below",controller.State.IsGrounded.ToString());
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Slope Angle",controller.State.SlopeAngle.ToString());
		}
		DrawDefaultInspector();
		
		
	}
}

