using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(CharacterBehavior))]
[CanEditMultipleObjects]

/// <summary>
/// Adds custom labels to the CorgiController inspector
/// </summary>

public class CharacterBehaviorInspector : Editor 
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
		CharacterBehavior behavior = (CharacterBehavior)target;
		if (behavior.BehaviorState!=null)
		{
			EditorGUILayout.LabelField("LadderClimbing",behavior.BehaviorState.LadderClimbing.ToString());
			EditorGUILayout.LabelField("LadderColliding",behavior.BehaviorState.LadderColliding.ToString());
			EditorGUILayout.LabelField("LadderTopColliding",behavior.BehaviorState.LadderTopColliding.ToString());
			EditorGUILayout.LabelField("LadderClimbingSpeed",behavior.BehaviorState.LadderClimbingSpeed.ToString());
		}
		DrawDefaultInspector();
		
		
	}
}

