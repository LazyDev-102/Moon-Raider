using UnityEngine;
using System.Collections;
/// <summary>
/// Add this component to a CorgiController2D and it will try to kill your player on sight.
/// </summary>
public class AISimpleWalk : MonoBehaviour, IPlayerRespawnListener
{
    /// The speed of the agent
    public float Speed;
	protected float _orgSpeed;
    public float SpeedRandomizer = 0;

	/// The initial direction
	public bool GoesRightInitially = true;

    public float JumpForce = 0;
    public float JumpRate = 0;
    public int JumpDamage = 0;
    public GameObject JumpLandEffect;
    //public AudioClip JumpLandSound;

    /// If set to true, the agent will try and avoid falling
    public bool AvoidFalling = false;
	public bool TurnAtWalls = true;
	public bool NeedsGroundToChangeDirection = true;
	public bool ShouldFlip = true;
	public LayerMask maskLayer = 0;
    public bool AlwaysJumping = false;

    public bool Grounded = true;

    public float WallOffset = 0;

	// private stuff
	protected EnemyController _controller;
	protected BoxCollider2D _collider;
	protected SpriteRenderer _renderer;
    protected Vector2 _direction;
    protected Vector2 _startPosition;
    protected Vector2 _initialDirection;
    protected Vector3 _initialScale;
    protected Vector3 _holeDetectionOffset;
    protected float _holeDetectionRayLength = 0.5f;
    protected float _wallDetectionRayLength = 0.5f;

	private bool _needsToThink = false;
    private bool _started = false;
    private CameraController sceneCamera;

    public bool IsColliding = false;

    public Vector2 Direction
    {
        get
        {
            return _direction;
        }
    }

    public void SetOrgSpeed(float s)
    {
        _orgSpeed = s;
    }

    /// <summary>
    /// Initialization
    /// </summary>
    protected virtual void Awake()
    {
		// we get the CorgiController2D component
		_controller = GetComponent<EnemyController>();
		_collider = GetComponents<BoxCollider2D>()[0];
		_renderer = GetComponent<SpriteRenderer> ();

		// initialize the start position
		_startPosition = transform.position;

        // initialize the direction
        _direction = GoesRightInitially ? Vector2.right : Vector2.left;

        _holeDetectionRayLength = _renderer.bounds.size.y / 2 + 0.05f;
        _wallDetectionRayLength = _renderer.bounds.size.x / 2 + 0.05f + WallOffset;

        _initialDirection = _direction;
        _initialScale = transform.localScale;
		_holeDetectionOffset = new Vector3(1, 0.5f, 0);
        Speed += Random.Range(-SpeedRandomizer, SpeedRandomizer);
		_orgSpeed = Speed;

        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
    }


    public virtual void Start()
    {
        StartCoroutine(Startup());
    }


    public virtual IEnumerator Jump(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_controller.State.IsCollidingBelow)
        {
            _controller.SnapToFloor = false;

            if(_needsToThink)
                _controller.SetVerticalForce(JumpForce);

            yield return new WaitForSeconds(2 * JumpRate);

            _controller.SnapToFloor = true;
        }

        if (AlwaysJumping)
        {
            StartCoroutine(Jump(JumpRate));
        }
    }


    public virtual IEnumerator Startup()
    {
        yield return new WaitForSeconds(0.25f);

        CheckForConveyor();
        _started = true;

        if (JumpForce > 0)
            StartCoroutine(Jump(JumpRate));
    }

    public void SetNeedsToThink(bool state)
	{
		_needsToThink = state;
	}

	public bool Active()
	{
		return _needsToThink;
	}

	public void FlipDir()
	{
		_direction = -_direction;
	}

	public void Walk()
	{
		Speed = _orgSpeed;
		_needsToThink = true;
	}

	public void Disable()
	{
		Speed = 0;
		_needsToThink = false;
        _controller.SetHorizontalForce(0);
    }

    public void HardDisable()
    {
        Disable();
        TurnAtWalls = false;
        AvoidFalling = false;
        ShouldFlip = false;

        AIShootOnSight _shoot = GetComponent<AIShootOnSight>();

        if (_shoot != null)
            _shoot.StopToShoot = false;
    }


    public virtual IEnumerator ResetColliding(float duration)
    {
        yield return new WaitForSeconds(duration);
        IsColliding = false;
    }


    /// <summary>
    /// Every frame, moves the agent and checks if it can shoot at the player.
    /// </summary>
    protected virtual void FixedUpdate () 
	{
        if (!_started)
        {
            _controller.SetHorizontalForce(0);
            return;
        }

        // moves the agent in its current direction
        if(Speed > 0)
            _controller.SetHorizontalForce(_direction.x * Speed);

		if (_needsToThink) 
		{
			if (TurnAtWalls) {
				CheckForWalls ();
			}

			CheckForHoles ();
		}
    }

    /// <summary>
    /// Checks for a wall and changes direction if it meets one
    /// </summary>
    protected virtual void CheckForWalls()
    {
		if (Speed == 0)
			return;

        Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y);
        RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, _direction.x * Vector2.right, _wallDetectionRayLength, maskLayer, true, Color.cyan);

        //Debug.Log (_controller.State.IsCollidingLeft + " " + _controller.State.IsCollidingRight);

        // if the agent is colliding with something, make it turn around
        if (raycast)
        {
            IsColliding = true;
            StartCoroutine(ResetColliding(0.25f));
            ChangeDirection("there's a wall.");
        }
    }

    /// <summary>
    /// Checks for holes 
    /// </summary>
    protected virtual void CheckForHoles()
    {
        Grounded = false;

        Vector2 raycastOrigin = new Vector2(transform.position.x + _direction.x*_collider.bounds.size.x/2, transform.position.y);
		RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, transform.localScale.y * Vector2.down, _holeDetectionRayLength, maskLayer, true, Color.magenta);
        
		// if the raycast doesn't hit anything
        if (!raycast)
        {
            // we change direction
            if(AvoidFalling)
                ChangeDirection("there's a hole.");
        }
        else
        {
            Grounded = true;

            if (_controller.State.WasGroundedLastFrame && JumpDamage > 0 && _needsToThink)
            {
                Health h = raycast.collider.gameObject.GetComponent<Health>();
                if (h)
                {
                    h.TakeDamage(JumpDamage, raycast.collider.gameObject);
                }

                //if (JumpLandSound != null)
                    //SoundManager.Instance.PlaySound(JumpLandSound, transform.position);

                //if (JumpLandEffect != null)
                    //Instantiate(JumpLandEffect, raycast.point + _controller.RayOffset*Vector2.down, transform.rotation);
            }
        }
    }

	protected virtual void CheckForConveyor()
	{
		Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y);
		RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, transform.localScale.y * Vector2.down, _holeDetectionRayLength, maskLayer, true, Color.green);

		// if the raycast doesn't hit anything
		if (raycast)
		{
			if (raycast.collider.GetComponent<Conveyor> () != null) { 
				//Debug.Log ("Conveyor detected!");
				Speed *= 3;
			}
		}
	}

    /// <summary>
    /// Changes the agent's direction and flips its transform
    /// </summary>
    public virtual void ChangeDirection(string reason = "")
    {
        //if (reason != "")
            //Debug.Log(gameObject.name + " is turning around because " + reason);

		// Only turn around on the ground
		RaycastHit2D raycast = CorgiTools.CorgiRayCast(transform.position, Vector2.down, 1.25f*_collider.bounds.size.y, maskLayer, true, Color.yellow);

        // if the raycast doesn't hit anything
        if (!raycast && NeedsGroundToChangeDirection)
        {
            //Debug.Log(gameObject.name + " Can't turn around because it's in the air");
            return;
        }

        _direction = -_direction;

		if(ShouldFlip)
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
		//transform.localScale = _initialScale;
		transform.position = _startPosition;
		gameObject.SetActive(true);
	}

}
