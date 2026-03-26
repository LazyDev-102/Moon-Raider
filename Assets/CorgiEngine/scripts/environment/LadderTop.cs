using UnityEngine;
using System.Collections;
/// <summary>
/// Add this component to the top of a ladder to make sure the character knows when it hits the top of the ladder.
/// </summary>
public class LadderTop : MonoBehaviour 
{

	/// <summary>
	/// Triggered while another collider (usually the player) is colliding with this component.
	/// </summary>
	/// <param name="collider">the other collider.</param>
	protected virtual void OnTriggerStay2D(Collider2D collider)
	{		
		// if the other collider isn't a CharacterBehavior, we do nothing
		CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
		if (character==null)
			return;
		// if the other collider isn't a CorgiController, we do nothing
		CorgiController controller = collider.GetComponent<CorgiController>();
		if (controller==null)
			return;
		// while the character is colliding with the top of the ladder, we set the corresponding state to True.
		character.BehaviorState.LadderTopColliding=true;		
	}

    protected virtual void OnTriggerExit2D(Collider2D collider)
	{
		// if the other collider isn't a CharacterBehavior, we do nothing
		CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
		if (character==null)
			return;
		// if the other collider isn't a CorgiController, we do nothing		
		CorgiController controller = collider.GetComponent<CorgiController>();
		if (controller==null)
			return;
		// the character is not colliding with the top of the ladder anymore, we set the corresponding state to false.
		character.BehaviorState.LadderTopColliding=false;
		
		
	}
}
