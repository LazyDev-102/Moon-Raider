using UnityEngine;
using System.Collections;

/// <summary>
/// Adds this class to a body of water. It will handle splash effects on entering/exiting, and allow the player to jump out of it.
/// </summary>
public class Quicksand : MonoBehaviour 
{

    /// <summary>
    /// Triggered when something collides with the water
    /// </summary>
    /// <param name="collider">Something colliding with the water.</param>
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        // we check that the object colliding with the water is actually a corgi controller and a character
        CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
        if (character != null)
        {
            CorgiController controller = collider.GetComponent<CorgiController>();
            if (controller == null)
                return;

            if (controller.Speed.y < 0)
            {
                controller.Parameters.MaxVelocity = 0.5f*controller.OriginalParameters.MaxVelocity;
            }
        }
    }


    protected virtual void OnTriggerExit2D(Collider2D collider)
	{
		// we check that the object colliding with the water is actually a corgi controller and a character
		CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
        if (character != null)
        {
            CorgiController controller = collider.GetComponent<CorgiController>();
            if (controller == null)
                return;

            if (controller.Speed.y > 0)
            {
                controller.Parameters.MaxVelocity = controller.OriginalParameters.MaxVelocity;
            }
        }
    }
}
