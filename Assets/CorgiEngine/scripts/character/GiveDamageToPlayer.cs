using UnityEngine;
using System.Collections;
/// <summary>
/// Add this class to an object that should give damage to the player when colliding with it
/// </summary>
public class GiveDamageToPlayer : MonoBehaviour 
{
	/// The amount of health to remove from the player's health
	public int DamageToGive = 10;
    public bool DiesOnContact = false;
    public bool HurtEnemies = false;
    public bool HurtsPlayer = true;

    // storage		
    protected Vector2
		_lastPosition,
		_velocity;
	
	/// <summary>
	/// During last update, we store the position and velocity of the object
	/// </summary>
	public virtual void FixedUpdate () 
	{
		_velocity = (_lastPosition - (Vector2)transform.position) /Time.deltaTime;
		_lastPosition = transform.position;
	}
	
	/// <summary>
	/// When a collision with the player is triggered, we give damage to the player and knock it back
	/// </summary>
	/// <param name="collider">what's colliding with the object.</param>
	public virtual void OnTriggerEnter2D(Collider2D collider)
	{
        Health enemy = collider.GetComponent<Health>();
        if (HurtEnemies && enemy != null)
        {
            enemy.TakeDamage(DamageToGive, gameObject);
            return;
        }

        CharacterBehavior player = collider.GetComponent<CharacterBehavior>();
        if (player == null)
			return;

        if (!HurtsPlayer)
            return;

        //Debug.Log("Giving Damage " + DamageToGive);

        var controller = player.GetComponent<CorgiController>();
        var totalVelocity = controller.Speed + _velocity;

        if (!player.BehaviorState.MeleeEnergized)
        {
            controller.SetForce(new Vector2(
                -1 * Mathf.Sign(totalVelocity.x) * Mathf.Clamp(Mathf.Abs(totalVelocity.x) * 5, 10, 20),
                -1 * Mathf.Sign(totalVelocity.y) * Mathf.Clamp(Mathf.Abs(totalVelocity.y) * 2, 0, 15)
                ));

            player.TakeDamage(DamageToGive, gameObject);
        }

        if(DiesOnContact)
        {
            Health health = GetComponent<Health>();

            if(health != null)
            {
                health.TakeDamage(health.CurrentHealth, collider.gameObject, false);
            }
        }
    }

    public virtual void OnTriggerStay2D(Collider2D collider)
    {
        OnTriggerEnter2D(collider);
    }


    void KnockBack()
    {

        //		var controller = player.GetComponent<CorgiController>();
        //		var totalVelocity = controller.Speed + _velocity;

        //        controller.SetForce(new Vector2(
        //			-1*Mathf.Sign(totalVelocity.x) * Mathf.Clamp(Mathf.Abs(totalVelocity.x) * 5,10,20),
        //			-1*Mathf.Sign(totalVelocity.y) * Mathf.Clamp(Mathf.Abs(totalVelocity.y) * 2,0,15)			
        //			));
    }
}
