using UnityEngine;
using System.Collections;
/// <summary>
/// Add this component to a CorgiController2D and it will try to kill your player on sight.
/// </summary>
public class AIOmniWalk : MonoBehaviour, IPlayerRespawnListener
{
    /// The speed of the agent
    public float Speed;
    /// The initial direction
    public bool GoesRightInitially = true;
    /// If set to true, the agent will try and avoid falling
    public bool AvoidFalling = false;

    public bool RandomMovement = false;

    public bool ShouldFlip = true;

    public LayerMask wallMask;
    public Vector4 Offsets = Vector4.zero; // NESW

    public float LookDistance = 0;

    // private stuff
    protected EnemyController _controller;
    protected Vector2 _direction;
    protected Vector2 _startPosition;
    protected Vector2 _initialDirection;
    protected Vector3 _initialScale;
    protected Vector3 _holeDetectionOffset;
    private float _rotation;
    private Quaternion _turn;
    private float _height = 0;
    float _speed;
    private bool _canRandom = true;
    private BoxCollider2D _box;

    bool canUpdate = false;
    float initDelay = 0.025f;

    public bool shouldInit = true;

    public float OrgSpeed
    {
        get
        {
            return _speed;
        }
    }

    public Vector2 Direction
    {
        get
        {
            return _direction;
        }
    }

    float orgGravityScale = 0;

    /// <summary>
    /// Initialization
    /// </summary>
    protected virtual void Awake()
    {
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        orgGravityScale = GetComponent<Rigidbody2D>().gravityScale;
        GetComponent<Rigidbody2D>().gravityScale = 0;

        if (shouldInit)
            StartCoroutine(Init(initDelay));
    }

    public virtual IEnumerator Init(float duration)
    {
        yield return new WaitForSeconds(duration);

        _box = GetComponent<BoxCollider2D>();

        PickerOffset piggerOffset = GetComponent<PickerOffset>();

        // we get the CorgiController2D component
        _controller = GetComponent<EnemyController>();

        _height = GetComponent<SpriteRenderer>().bounds.size.y + 0.5f;

        if (LookDistance == 0)
            LookDistance = _height;
            
        _speed = Speed;

        RaycastHit2D rDown = CorgiTools.CorgiRayCast(transform.position, Vector2.down, _height, wallMask, true, Color.yellow);
        if (rDown)
        {
            _box.offset = new Vector2(_box.offset.x, _box.offset.y - Offsets.z);
            transform.Translate(Offsets.z * Vector3.down);
        }
        else
        {
            GetComponent<BoxCollider2D>().size = 0.99f*Vector2.one;
            RaycastHit2D rUp = CorgiTools.CorgiRayCast(transform.position, Vector2.up, _height, wallMask, true, Color.yellow);
            if (rUp)
            {
                transform.Rotate(new Vector3(0, 0, 180));
                _box.offset = new Vector2(_box.offset.x, _box.offset.y - Offsets.x);
                transform.Translate(Offsets.x * Vector3.up);
            }
            else
            {
                RaycastHit2D rLeft = CorgiTools.CorgiRayCast(transform.position, Vector2.left, _height, wallMask, true, Color.yellow);
                if (rLeft)
                {
                    if (piggerOffset != null)
                    {
                        transform.position = new Vector3(transform.position.x, transform.position.y - piggerOffset.Offset.y, transform.position.z);
                    }

                    _box.offset = new Vector2(_box.offset.x, _box.offset.y - Offsets.w);
                    GetComponent<SpriteRenderer>().flipX = false;
                    transform.Translate(Offsets.w * Vector3.right);
                    transform.Rotate(new Vector3(0, 0, 270));
                }
                else
                {
                    RaycastHit2D rRight = CorgiTools.CorgiRayCast(transform.position, Vector2.right, _height, wallMask, true, Color.yellow);
                    if (rRight)
                    {
                        if (piggerOffset != null)
                        {
                            transform.position = new Vector3(transform.position.x, transform.position.y - piggerOffset.Offset.y, transform.position.z);
                        }

                        _box.offset = new Vector2(_box.offset.x, _box.offset.y - Offsets.y);
                        GetComponent<SpriteRenderer>().flipX = false;
                        transform.Translate(Offsets.y * Vector3.left);
                        transform.Rotate(new Vector3(0, 0, 90));
                    }
                }
            }
        }

        // initialize the start position
        _startPosition = transform.position;

        // initialize the direction
        _rotation = transform.rotation.eulerAngles.z;

        AIShootOnSight _shoot = GetComponent<AIShootOnSight>();

        if (_shoot != null)
        {
            _shoot.AimAngle = _rotation - 90;
        }

        _turn = Quaternion.Euler(0, 0, _rotation);

        _direction = GoesRightInitially ? _turn * Vector2.right : _turn * Vector2.left;

        _initialDirection = _direction;
        _initialScale = transform.localScale;

        _holeDetectionOffset = Vector3.right;

        GetComponent<Rigidbody2D>().gravityScale = orgGravityScale;

        canUpdate = true;
    }

    //void OnBecameVisible()
    //{
    //    enabled = true;
    //    _controller.enabled = true; // redundancy
    //}

    //void OnBecameInvisible()
    //{
    //    enabled = false;
    //}

    /// <summary>
    /// Every frame, moves the agent and checks if it can shoot at the player.
    /// </summary>
    protected virtual void Update()
    {
        if (!canUpdate)
            return;
            
        float r = Mathf.Round(transform.rotation.eulerAngles.z);

        if (r != _rotation)
        {
            //Debug.Log(">>>>> Changing " + _rotation + " to " + r);
            _rotation = r;
            _turn = Quaternion.Euler(0, 0, _rotation);
        }

        CheckForWalls();
        if (AvoidFalling)
        {
            CheckForHoles();
        }

        if (_rotation == 0f || _rotation == 180f)
        {
            _controller.SetHorizontalForce(_direction.x * Speed);
        }
        else
        {
            _controller.SetVerticalForce(-_direction.y * Speed);
        }

        if (RandomMovement && _canRandom)
            ChangeDirection();
    }


    public void Stop()
    {
        _controller.SetHorizontalForce(0);
        _controller.SetVerticalForce(0);
    }

    public void Walk()
    {
        Speed = _speed;
    }


    /// <summary>
    /// Checks for a wall and changes direction if it meets one
    /// </summary>
    protected virtual void CheckForWalls()
    {
        Vector3 vect = 0.5f * Vector3.down;

        if (_rotation == 0.0f)
        {
            RaycastHit2D wall = CorgiTools.CorgiRayCast(transform.position, Vector2.right * _direction.x, LookDistance, wallMask, true, Color.cyan);
            if (wall)
                ChangeDirection();
        }
        else if (_rotation == 180.0f)
        {
            RaycastHit2D wall = CorgiTools.CorgiRayCast(transform.position, Vector2.right * _direction.x, LookDistance, wallMask, true, Color.cyan);
            if (wall)
                ChangeDirection();
        }
        else
        {
            RaycastHit2D wall = CorgiTools.CorgiRayCast(transform.position + _turn * vect, _turn * Vector2.left * _direction.x, LookDistance, wallMask, true, Color.cyan);
            if (wall)
                ChangeDirection();
        }
    }

    /// <summary>
    /// Checks for holes 
    /// </summary>
    protected virtual void CheckForHoles()
    {
        Vector2 holeDir = Vector2.down;
        Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y);

        if (_rotation == 180.0f)
        {
            holeDir = Vector2.up;
            raycastOrigin += _direction.x * Vector2.right;
        }
        else if (_rotation == 90.0f)
        {
            holeDir = Vector2.right;
            raycastOrigin += _direction.y * Vector2.down;
        }
        else if (_rotation == 270.0f)
        {
            holeDir = Vector2.left;
            raycastOrigin += _direction.y * Vector2.down;
        }
        else
        {
            raycastOrigin += _direction.x * Vector2.right;
        }

        RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, holeDir, 0.65f*_height, wallMask, true, Color.yellow);
        if (!raycast)
        {
            // we change direction
            ChangeDirection();
        }
    }

    /// <summary>
    /// Changes the agent's direction and flips its transform
    /// </summary>
    protected virtual void ChangeDirection()
    {
        _direction = -_direction;

        if(ShouldFlip)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        if (_canRandom)
        {
            _canRandom = false;
            StartCoroutine(ResetRandom(Random.Range(1, 5) / 2));
        }
    }

    public virtual IEnumerator ResetRandom(float duration)
    {
        yield return new WaitForSeconds(duration);

        _canRandom = true;
    }

    /// <summary>
    /// When the player respawns, we reinstate this agent.
    /// </summary>
    /// <param name="checkpoint">Checkpoint.</param>
    /// <param name="player">Player.</param>
    public virtual void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint, CharacterBehavior player)
    {
        _direction = _initialDirection;
        transform.localScale = _initialScale;
        transform.position = _startPosition;
        gameObject.SetActive(true);
    }

}
