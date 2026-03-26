using UnityEngine;
using System.Collections;

/// <summary>
/// Add this component to a CorgiController2D and it will try to kill your player on sight.
/// </summary>
public class AIMoveOnSight : MonoBehaviour 
{
	
	/// The speed of the agent
	public float Speed=3f;
	/// The maximum distance at which the AI can see the player
	public float ViewDistance = 10f;
    /// The character is facing right by default
    public bool CharacterFacingRight = true;
    /// the horizontal distance from the player at which the agent will stop moving. Between that distance and the walk distance, the agent will slow down progressively
    public float StopDistance = 1f;

    protected float _canFireIn;
    protected Vector2 _direction;
    protected float _distance;
    protected CorgiController _controller;
    protected Animator _animator;
    protected int _facingModifier;

	/// initialization
	protected virtual void Start () 
	{
		// we get the CorgiController2D component
		_controller = GetComponent<CorgiController>();
		// we get the character's animator
		_animator = GetComponent<Animator>();
		_direction=Vector2.right;
        if (CharacterFacingRight)
            _facingModifier = -1;
        else
            _facingModifier = 1;
	}

    /// Every frame, check for the player and try and kill it
    protected virtual void Update () 
	{
		bool hit=false;
        _distance = 0;
		// we cast a ray to the left of the agent to check for a Player
		Vector2 raycastOrigin = new Vector2(transform.position.x,transform.position.y+(transform.localScale.y/2));		
		RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin,-Vector2.right,ViewDistance,1<<LayerMask.NameToLayer("Player"),true,Color.gray);
		// if we see a player
		if (raycast)
		{
			hit=true;
			// we change direction
            _direction = -Vector2.right;
            _distance= raycast.distance;
		}
		
		// we cast a ray to the right of the agent to check for a Player
		raycastOrigin = new Vector2(transform.position.x,transform.position.y+(transform.localScale.y/2));		
		raycast = CorgiTools.CorgiRayCast(raycastOrigin,Vector2.right,ViewDistance,1<<LayerMask.NameToLayer("Player"),true,Color.gray);
		if (raycast)
		{
			hit=true;
            _direction = Vector2.right;
            _distance = raycast.distance;
		}
		

		// if the ray has hit the player, we move the agent in that direction
        if ((hit) &&  (_distance > StopDistance))
            _controller.SetHorizontalForce(_direction.x * Speed);
        else
            _controller.SetHorizontalForce(0);


        if (_direction == Vector2.right)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * _facingModifier, transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * _facingModifier, transform.localScale.y, transform.localScale.z);

        // updates the animator if it's not null
        if (_animator != null)
            _animator.SetFloat("Speed", Mathf.Abs(_controller.Speed.x));

	}
	
}
