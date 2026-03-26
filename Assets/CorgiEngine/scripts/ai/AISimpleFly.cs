using UnityEngine;
using System.Collections;

public class AISimpleFly : MonoBehaviour,IPlayerRespawnListener
{
	/// The speed of the agent
	public float Speed;
	protected float _orgSpeed;

	/// The initial direction
	public bool GoesRightInitially = true;

	// private stuff
	protected EnemyController _controller;
	protected Vector2 _direction;
	protected Vector2 _startPosition;
	protected Vector2 _initialDirection;
	protected Vector3 _initialScale;

	/// <summary>
	/// Initialization
	/// </summary>
	protected virtual void Awake()
	{
		// we get the CorgiController2D component
		_controller = GetComponent<EnemyController>();
		// initialize the start position
		_startPosition = transform.position;
		// initialize the direction
		_direction = GoesRightInitially ? Vector2.right : -Vector2.right;
		_initialDirection = _direction;
		_initialScale = transform.localScale;
		_orgSpeed = Speed;
	}

	public void Fly()
	{
		Speed = _orgSpeed;
	}

	public void Disable()
	{
		Speed = 0;
		_orgSpeed = 0;
	}

	/// <summary>
	/// Every frame, moves the agent and checks if it can shoot at the player.
	/// </summary>
	protected virtual void Update () 
	{

		// moves the agent in its current direction
		_controller.SetHorizontalForce(_direction.x * Speed);
		CheckForWalls();
	}

	/// <summary>
	/// Checks for a wall and changes direction if it meets one
	/// </summary>
	protected virtual void CheckForWalls()
	{
		if (Speed == 0)
			return;

		// if the agent is colliding with something, make it turn around
		if ((_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight))
		{
			ChangeDirection();
		}
	}

	/// <summary>
	/// Changes the agent's direction and flips its transform
	/// </summary>
	public virtual void ChangeDirection()
	{
		_direction = -_direction;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	/// <summary>
	/// When the player respawns, we reinstate this agent.
	/// </summary>
	/// <param name="checkpoint">Checkpoint.</param>
	/// <param name="player">Player.</param>
	public virtual void onPlayerRespawnInThisCheckpoint (CheckPoint checkpoint, CharacterBehavior player)
	{
		_direction = _initialDirection;
		transform.localScale= _initialScale;
		transform.position=_startPosition;
		gameObject.SetActive(true);
	}
}

