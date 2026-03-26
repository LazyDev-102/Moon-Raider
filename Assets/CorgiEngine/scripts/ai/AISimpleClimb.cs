using UnityEngine;
using System.Collections;
/// <summary>
/// Add this component to a CorgiController2D and it will try to kill your player on sight.
/// </summary>
public class AISimpleClimb : MonoBehaviour,IPlayerRespawnListener
{
	/// The speed of the agent
	public float Speed;
	protected float _orgSpeed;

	/// The initial direction
	public bool GoesRightInitially = true;
	/// If set to true, the agent will try and avoid falling
	public bool AvoidFalling = false;
	public bool ShouldFlip = true;

	// private stuff
	protected EnemyController _controller;
	protected Vector2 _direction;
	protected Vector2 _startPosition;
	protected Vector2 _initialDirection;
	protected Vector3 _initialScale;
	protected Vector3 _holeDetectionOffset;
	protected float _holeDetectionRayLength = 1f;

    bool canChange = true;

    /// <summary>
    /// Initialization
    /// </summary>
    protected virtual void Awake()
	{
		// we get the CorgiController2D component
		_controller = GetComponent<EnemyController> ();
		// initialize the start position
		_startPosition = transform.position;
		// initialize the direction
		GoesRightInitially = (transform.localScale.y == 1);
		_direction = GoesRightInitially ? Vector2.up : -Vector2.up;
		_initialDirection = _direction;
		_initialScale = transform.localScale;
		_holeDetectionOffset = new Vector3(-0.25f*transform.localScale.x, 1f, 0);
		_orgSpeed = Speed;
	}

	public void Climb()
	{
		Speed = _orgSpeed;
	}

	public void Disable()
	{
		Speed = 0;
		_orgSpeed = 0;
	}

	protected virtual void Start()
	{
		if (GetComponent<SpriteRenderer>().flipY)
			_holeDetectionOffset = new Vector3(-0.25f*transform.localScale.x, -1f, 0);
	}

	/// <summary>
	/// Every frame, moves the agent and checks if it can shoot at the player.
	/// </summary>
	protected virtual void Update () 
	{
		CheckForWalls();
		if (AvoidFalling)
		{
			CheckForHoles();
		}

		// moves the agent in its current direction
        _controller.SetVerticalForce(-_direction.y * Speed);
	}

	/// <summary>
	/// Checks for a wall and changes direction if it meets one
	/// </summary>
	protected virtual void CheckForWalls()
	{
		// BROKEN
		// if the agent is colliding with something, make it turn around
		if ((_direction.y <= 0 && _controller.State.IsCollidingAbove) || (_direction.y >= 0 && _controller.State.IsCollidingBelow))
		{
			ChangeDirection();
		}
	}

	/// <summary>
	/// Checks for holes 
	/// </summary>
	protected virtual void CheckForHoles()
	{
		Vector2 raycastOrigin = new Vector2(transform.position.x + _direction.x*_holeDetectionOffset.x - (transform.localScale.x / 2), transform.position.y + transform.localScale.y*_holeDetectionOffset.y);
		RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, transform.localScale.x * Vector2.left, _holeDetectionRayLength, (1 << LayerMask.NameToLayer("Platforms")) | (1 << LayerMask.NameToLayer("OneWayPlatforms")), true, Color.yellow);

		// if the raycast doesn't hit anything
		if (!raycast)
		{
			// we change direction
			ChangeDirection();
		}
	}

	/// <summary>
	/// Changes the agent's direction and flips its transform
	/// </summary>
	public virtual void ChangeDirection()
	{
        if (!canChange)
            return;

        canChange = false;
        StartCoroutine(EnableChange(0.25f));

        _direction = -_direction;

		if(ShouldFlip)
			transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
	}


    public virtual IEnumerator EnableChange(float duration)
    {
        yield return new WaitForSeconds(duration);

        canChange = true;
    }

    /// <summary>
    /// When the player respawns, we reinstate this agent.
    /// </summary>
    /// <param name="checkpoint">Checkpoint.</param>
    /// <param name="player">Player.</param>
    public virtual void onPlayerRespawnInThisCheckpoint (CheckPoint checkpoint, CharacterBehavior player)
	{
		_direction = _initialDirection;
		transform.localScale = _initialScale;
		transform.position = _startPosition;
		gameObject.SetActive(true);
	}

	protected virtual void OnTriggerEnter2D(Collider2D collider)
	{
		ChangeDirection();
	}

}
