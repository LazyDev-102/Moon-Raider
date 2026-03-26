using UnityEngine;
using System.Collections;
/// <summary>
/// Add this script to a CharacterBehavior+CorgiController2D object to make it follow (or try to follow) the GameObject with a "Player" tag.
/// So far the Follower will move horizontally towards the player, and use a jetpack to reach it, or jump above obstacles.
/// </summary>
public class AIFollower : MonoBehaviour 
{
	/// true if the agent should follow the player
	public bool AgentFollowsPlayer{get;set;}
	
	/// the minimum horizontal distance between the agent and the player at which the agent should start running
	public float RunDistance = 10f;
	/// the maximum horizontal distance between the agent and the player at which the agent should walk at normal speed
	public float WalkDistance = 5f;
	/// the horizontal distance from the player at which the agent will stop moving. Between that distance and the walk distance, the agent will slow down progressively
	public float StopDistance = 1f;
	/// the minimum vertical distance at which the agent will start jetpacking if the target is above it
	public float JetpackDistance = 0.2f;
	
	// private stuff 
	protected Transform _target;
    protected CharacterBehavior _playerComponent;
    protected CorgiController _controller;
    //protected CharacterJetpack _jetpack;
    protected float _speed;
    protected float _direction;

    /// <summary>
    /// Initialization
    /// </summary>
    protected virtual void Start () 
	{
		// we get the player from its tag
		_target=GameManager.Instance.Player.transform;
		// we get its components
		_playerComponent=(CharacterBehavior)GetComponentInParent<CharacterBehavior>();
		_controller=(CorgiController)GetComponentInParent<CorgiController>();
		//_jetpack = (CharacterJetpack)GetComponentInParent<CharacterJetpack>();
		// we make the agent start following the player
		AgentFollowsPlayer=true;
	}

    /// <summary>
    /// Every frame, we make the agent move towards the player
    /// </summary>
    protected virtual void Update () 
	{
		// if the agent is not supposed to follow the player, we do nothing.
		if (!AgentFollowsPlayer)
			return;
			
		// if the Follower doesn't have the required components, we do nothing.
		if ( (_playerComponent==null) || (_controller==null) )
			return;
		
		// we calculate the distance between the target and the agent
		float distance = Mathf.Abs(_target.position.x - transform.position.x);
				
		// we determine the direction	
		_direction = _target.position.x>transform.position.x ? 1f : -1f;
		
		// depending on the distance between the agent and the player, we set the speed and behavior of the agent.
		if (distance>RunDistance)
		{
			// run
			_speed=1;
			_playerComponent.RunStart();
		}
		else
		{
			_playerComponent.RunStop();
		}
		if (distance<RunDistance && distance>WalkDistance)
		{
			// walk
			_speed=1;
		}
		if (distance<WalkDistance && distance>StopDistance)
		{
			// walk slowly
			_speed=distance/WalkDistance;
		}
		if (distance<StopDistance)
		{
			// stop
			_speed=0f;
		}
		
		// we make the agent move
		_playerComponent.SetHorizontalMove(_speed*_direction);
		
		// if there's an obstacle on the left or on the right of the agent, we make it jump. If it's moving, it'll jump over the obstacle.
		if (_controller.State.IsCollidingRight || _controller.State.IsCollidingLeft)
		{
			_playerComponent.JumpStart();
		}
		
		// if the follower is equipped with a jetpack
		/*if (_jetpack!=null)
		{
			// if the player is above the agent + a magic factor, we make the agent start jetpacking
			if (_target.position.y>transform.position.y+JetpackDistance)
			{
				_jetpack.JetpackStart();
			}
			else
			{
				_jetpack.JetpackStop();
			}
		}*/
	}
}
