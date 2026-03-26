using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;
using JulienFoucher;

/// <summary>
/// This class will pilot the CorgiController component of your character.
/// This is where you'll implement all of your character's game rules, like jump, dash, shoot, stuff like that.
/// </summary>
public class CharacterBehavior : MonoBehaviour, CanTakeDamage
{
    public enum PlayerRole
    {
        PlayerOne,
        PlayerTwo
    }

    public PlayerRole PlayerNum = PlayerRole.PlayerOne;

    /// the boxcollider2D that will be used to check if the character's head is colliding with anything (used when crouched mostly)
    public BoxCollider2D HeadCollider;
    public GameObject MeleeColider;

    /// the current health of the character
    public int Health { get; set; }
    public float PartialDamage { get; set; }
    private float _partialDamageRate = 0;
    private Color _targetColor = Color.white;

    private bool wasSwimming = false;

    /// the various states of the character
    public CharacterBehaviorState BehaviorState { get; protected set; }
    /// the default parameters of the character
    public CharacterBehaviorParameters DefaultBehaviorParameters;
    /// the current behavior parameters (they can be overridden at times)
    public CharacterBehaviorParameters BehaviorParameters { get { return _overrideBehaviorParameters ?? DefaultBehaviorParameters; } }
    /// the permissions associated to the character
    public CharacterBehaviorPermissions Permissions;

    public int MaxSpecialIndex = 1;

    public bool TakesDamage = true;

    [Space(10)]
    [Header("Effects")]
    /// the effect that will be instantiated everytime the character touches the ground
    public Animator HitGround;

    [Space(10)]
    [Header("Sounds")]
    // the sound to play when the player jumps
    public AudioClip PlayerJumpSfx;
    // the sound to play when the player gets hit
    public AudioClip PlayerHitSfx;
    public AudioClip PlayerDeadSfx;
    public AudioClip PlayerDrainSfx;
    public AudioClip PlayerPickupSfx;
    public AudioClip PlayerPowerupSfx;
    public AudioClip PlayerMeleeSfx;
    public AudioClip PlayerMeleeErrorSfx;

    public bool Offscreen
    {
        get
        {
            return _isOffscreen;
        }
    }

    public bool HasFlyUpgrade
    {
        get
        {
            return PlayerNum == PlayerRole.PlayerTwo && BehaviorState.CanMelee && !BehaviorState.CoveredInSpores;
        }
    }


    public bool CanControl
    {
        get
        {
            return !_isOffscreen;
        }
    }

    /// is true if the character can jump
    public bool JumpAuthorized
    {
        get
        {
            if (BehaviorState.Swimming)
                return true;

            if ((BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhere) || (BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhereAnyNumberOfTimes))
                return true;

            if (BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpOnGround)
                return _controller.State.IsGrounded;

            return false;
        }
    }

    // associated gameobjects and positions
    protected CameraController _sceneCamera;
    protected CorgiController _controller;

    protected Animator _animator;
    protected CharacterShoot _shoot;
    protected Color _initialColor;
    protected Vector3 _initialScale;

    // storage for overriding behavior parameters
    protected CharacterBehaviorParameters _overrideBehaviorParameters;
    // storage for original gravity and timer
    protected float _originalGravity;

    // the current normalized horizontal speed
    protected float _normalizedHorizontalSpeed;

    // pressure timed jumps
    protected float _jumpButtonPressTime = 0;
    protected bool _jumpButtonPressed = false;
    protected bool _jumpButtonReleased = false;

    // time for offscreen
    protected float _offscreenStartTime = 0;
    protected Coroutine _offscreenReturnRoutine;
    protected bool _isOffscreen = false;

    // true if the player is facing right
    protected bool _isFacingRight = true;

    // INPUT AXIS
    protected float _horizontalMove;
    protected float _verticalMove;
    protected float _intendedHorizontalMove;

    private Color _defaultAmbientColor;
    private Color _targetAmbientColor;

    // GUN OFFSETS
    //	private Vector2[] _gunOffset;
    //	private SpriteRenderer _gun;
    private SpriteRenderer _ava;
    private SpriteRenderer _fx;
    private SpriteRenderer _bubble;
    private SpriteTrail _bubbleTrail;
    private SpriteTrail _trail;

    // FIGHT ME!!!
    private FightMusic fightMusic;

    // Plug I'm standing on
    private Plug _plug;
    private bool _wasSpinning;

    private bool _hiccupping = false;
    private Coroutine _hiccupRoutine = null;

    private Flickers _flickerPrimary;
    private Flickers _flickerSecondary;

    private int _specialIndex = 0;
    private bool wasMelee = false;


    public float moveX
    {
        get
        {
            return _normalizedHorizontalSpeed;
        }
    }

    public float IntendedMoveX
    {
        get
        {
            return _intendedHorizontalMove;
        }
    }

    public CharacterShoot shoot
    {
        get
        {
            return _shoot;
        }
    }

    public Animator animator
    {
        get
        {
            return _animator;
        }
    }

    public float HorizontalMove
    {
        get
        {
            return _horizontalMove;
        }
    }

    public float VerticalMove
    {
        get
        {
            return _verticalMove;
        }
    }

    public CorgiController Controller
    {
        get
        {
            return _controller;
        }
    }

    public Vector2 Speed
    {
        get
        {
            return _controller.Speed;
        }
    }


    /// <summary>
    /// Initializes this instance of the character
    /// </summary>
    protected virtual void Awake()
    {
        BehaviorState = new CharacterBehaviorState();
        _sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        _controller = GetComponent<CorgiController>();
        _shoot = GetComponent<CharacterShoot>();

        _flickerPrimary = GetComponentsInChildren<Flickers>()[0];
        _flickerSecondary = GetComponentsInChildren<Flickers>()[1];

        _initialScale = transform.localScale;

        float c = 0.75f;
        _defaultAmbientColor = new Color(c, c, c, 1f);
        _targetAmbientColor = _defaultAmbientColor;

        if (GetComponent<Renderer>() != null)
            _initialColor = GetComponent<Renderer>().material.color;

        _ava = GetComponentsInChildren<SpriteRenderer>()[0];
        _fx = GetComponentsInChildren<SpriteRenderer>()[1];
        _bubble = GetComponentsInChildren<SpriteRenderer>()[2];
        _bubbleTrail = _bubble.GetComponent<SpriteTrail>();
        _trail = GetComponentInChildren<SpriteTrail>();
    }

    /// <summary>
    /// </summary>
    protected void Start()
    {
        Cursor.visible = false;

        // we get the animator
        _animator = GetComponentsInChildren<Animator>()[0];

        // if the width of the character is positive, then it is facing right.
        _isFacingRight = transform.localScale.x > 0;

        _originalGravity = _controller.Parameters.Gravity;

        // we initialize all the controller's states with their default values.
        BehaviorState.Initialize();
        BehaviorState.NumberOfJumpsLeft = BehaviorParameters.NumberOfJumps;
        BehaviorState.CanJump = true;

        // Load state
        if (PlayerNum == PlayerRole.PlayerOne)
            BehaviorParameters.MaxHealth = GlobalVariables.MaxHealth;
        else
            BehaviorParameters.MaxHealth = 5;

        BehaviorParameters.MaxGems = GlobalVariables.MaxGems;
        BehaviorParameters.MaxWeapon = GlobalVariables.MaxWeapon;
        BehaviorState.CanMelee = GlobalVariables.CanMelee;

        // Lock into running
        BehaviorState.Running = true;
        BehaviorParameters.MovementSpeed = BehaviorParameters.RunSpeed;

        if (PlayerNum == PlayerRole.PlayerOne)
        {
            _shoot.WeaponIndex = GameManager.Instance.Player.BehaviorParameters.MaxWeapon;
            var gameManagers = GameObject.Find("GameManagers");
            BackgroundMusic bmusic = gameManagers.GetComponent<BackgroundMusic>();
            fightMusic = bmusic.fightMusic;
            SoundManager.Instance.FadeIn();

            GameManager.Instance.CanMove = false;
        }
        else
        {
            _shoot.WeaponIndex = GameManager.Instance.PlayerTwo.BehaviorParameters.MaxWeapon;
        }

        _shoot.MaxWeaponIndex = _shoot.WeaponIndex;
        _shoot.ChangeWeapon(_shoot.Weapons[_shoot.MaxWeaponIndex]);


        // Intro chat
        if (GlobalVariables.LevelIndex == 0 && GlobalVariables.WorldIndex == 1 && GlobalVariables.direction == DirectionEnum.Forwards && PlayerNum == PlayerRole.PlayerOne)
            StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.Intro));
        else if (GlobalVariables.LevelIndex == 1 && GlobalVariables.WorldIndex == 2 && GlobalVariables.direction == DirectionEnum.Forwards && PlayerNum == PlayerRole.PlayerOne)
            StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.TeachMelee));
    }


    public void PlayBeamSound()
    {
        if (PlayerNum == PlayerRole.PlayerTwo)
        {
            if (PlayerPowerupSfx != null)
                SoundManager.Instance.PlaySound(PlayerPowerupSfx, transform.position);
        }
    }


    public void FlickerSecondary()
    {
        _flickerSecondary.Flicker();
    }


    public void Enter()
    {
        Color newColor = new Color(0f, 0f, 0f, 0f);
        _sceneCamera.SetSnap();

        StartCoroutine(FreezeThawEnemies(true, false, 0.125f));
        StartCoroutine(FreezeThawEnemies(false, false, 0.75f));
    }

    public void Exit()
    {
        _sceneCamera.Dim();
        SoundManager.Instance.FadeOut();
        StartCoroutine(DelayedDisable(0.5f));
    }

    IEnumerator DelayedDisable(float duration)
    {
        yield return new WaitForSeconds(duration);

        Disable();
    }

    /// <summary>
    /// This is called every frame.
    /// </summary>
    protected virtual void Update()
    {
        if (BehaviorState.IsDead)
        {
            // Ambient Color
            float r = Mathf.Lerp(RenderSettings.ambientLight.r, _targetAmbientColor.r, Time.deltaTime);
            float g = Mathf.Lerp(RenderSettings.ambientLight.g, _targetAmbientColor.g, Time.deltaTime);
            float b = Mathf.Lerp(RenderSettings.ambientLight.b, _targetAmbientColor.b, Time.deltaTime);

            RenderSettings.ambientLight = new Color(r, g, b, 1.0f);
            return;
        }

        if (_animator.GetBool("SpinJumping") == true && _controller.Speed.y > 0)
            BehaviorState.NumberOfJumpsLeft = 0;
        else if (_animator.GetBool("SpinJumping") == false && _wasSpinning)
            BehaviorState.NumberOfJumpsLeft = 1;

        _wasSpinning = _animator.GetBool("SpinJumping");

        BehaviorState.DoubleJumping = !_controller.State.IsGrounded && (BehaviorParameters.NumberOfJumps - BehaviorState.NumberOfJumpsLeft) == 2;

        _trail.enabled = BehaviorState.MeleeEnergized;

        //BehaviorState.CanShoot = (BehaviorState.Swimming || BehaviorState.NumberOfJumpsLeft > 0 || _companion != null) && BehaviorState.Hurting == false && BehaviorState.Pickup == false;

        if (PlayerNum == PlayerRole.PlayerOne)
            BehaviorState.CanShoot = (BehaviorState.Swimming || !BehaviorState.DoubleJumping) && !BehaviorState.Hurting && !BehaviorState.Pickup;
        else
            BehaviorState.CanShoot = !BehaviorState.Hurting && !BehaviorState.Pickup;

        _bubble.enabled = BehaviorState.Swimming;
        _bubbleTrail.enabled = BehaviorState.Swimming;

        // we handle horizontal and vertical movement	
        HorizontalMovement();
        if (GameManager.Instance.CanMove)
            VerticalMovement();

        // if the character is not firing, we reset the firingStop state.
        if (!BehaviorState.Firing)
            BehaviorState.FiringStop = false;

        if (BehaviorState.Firing && !BehaviorState.CanShoot)
        {
            _shoot.ShootStop();
            BehaviorState.WantsToShoot = true;
        }
    }


    public void PlayMeleeErrorSound()
    {
        if (PlayerMeleeErrorSfx != null)
            SoundManager.Instance.PlaySound(PlayerMeleeErrorSfx, transform.position);
    }

    /// <summary>
    /// This is called once per frame, after Update();
    /// </summary>
    protected virtual void FixedUpdate()
    {
#if UNITY_IOS || UNITY_ANDROID
        GUIManager.Instance.MeleeButton.enabled = BehaviorState.CanMelee;
#endif

        if (PlayerNum == PlayerRole.PlayerTwo && _isOffscreen)
        {
            return;
        }

        if (BehaviorState.MeleeEnergized)
        {
            if (GameManager.Instance.CanMove)
            {
                // Rubber band gem use
                float percent = (float)GameManager.Instance.Points / (float)GameManager.Instance.Player.BehaviorParameters.MaxGems;

                if(percent > 0.25f)
                    GameManager.Instance.AddPointsInstant(-3);
                else if(percent <= 0.25f && percent > 0.1f)
                    GameManager.Instance.AddPointsInstant(-2);
                else
                    GameManager.Instance.AddPointsInstant(-1);
            }

            if (!wasMelee)
            {
                if (PlayerMeleeSfx != null)
                    SoundManager.Instance.PlaySound(PlayerMeleeSfx, transform.position);
            }
        }

        if (HasFlyUpgrade && BehaviorState.NumberOfJumpsLeft == 0 && Speed.y >= 0 && !BehaviorState.Swimming)
            GameManager.Instance.AddPointsInstant(0);
        else
            GravityActive(!BehaviorState.MeleeEnergized || BehaviorState.DoubleJumping);

        if (GameManager.Instance.Points == 0 && BehaviorState.MeleeEnergized)
        {
            BehaviorState.MeleeEnergized = false;

            if (PlayerNum == PlayerRole.PlayerOne)
                StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.yellow));
            else
                StartCoroutine(GUIManager.Instance.HealthBarTwo.Flicker(Color.yellow));

            PlayMeleeErrorSound();

            JumpStop();
        }

        MeleeColider.SetActive(BehaviorState.MeleeEnergized);

        // If we mash the fire button while double jumping, go ahead and start it automatically when the jump ends.
        if (BehaviorState.WantsToShoot && !BehaviorState.DoubleJumping && BehaviorState.CanShoot)
        {
            BehaviorState.WantsToShoot = false;
            _shoot.ShootOnce();
            _shoot.ShootStart();
        }

        // if the character became grounded this frame, we reset the doubleJump flag so he can doubleJump again
        if (_controller.State.JustGotGrounded)
        {
            BehaviorState.NumberOfJumpsLeft = BehaviorParameters.NumberOfJumps;

            if (Time.time - _jumpButtonPressTime < 0.05f)
            {
                JumpStart();
            }
        }
        else
        {
            if (_controller.State.IsFalling && BehaviorState.NumberOfJumpsLeft > 1)
            {
                BehaviorState.NumberOfJumpsLeft = 1;
            }
        }

        PartialDamage += _partialDamageRate;

        if (PartialDamage > 1)
        {
            PartialDamage -= 1;
            TakeDamage(1, null, false);
            _ava.color = Color.white;
        }
        else if (PartialDamage <= 0)
        {
            PartialDamage = 0;
            _ava.color = Color.white;
        }
        else
        {
            float r = Mathf.Lerp(Color.white.r, _targetColor.r, PartialDamage);
            float g = Mathf.Lerp(Color.white.g, _targetColor.g, PartialDamage);
            float b = Mathf.Lerp(Color.white.b, _targetColor.b, PartialDamage);

            _ava.color = new Color(r, g, b, 1);
        }

        // we send our various states to the animator.      
        UpdateAnimator();

        wasMelee = BehaviorState.MeleeEnergized;
    }

    public bool Grounded()
    {
        return _controller.State.IsGrounded;
    }

    /// <summary>
    /// This is called at Update() and sets each of the animators parameters to their corresponding State values
    /// </summary>
    protected virtual void UpdateAnimator()
    {
        if (BehaviorState.IsDead)
            return;

        if (_animator != null)
        {
            CorgiTools.UpdateAnimatorBool(_animator, "Grounded", _controller.State.IsGrounded);
            CorgiTools.UpdateAnimatorFloat(_animator, "Speed", Mathf.Abs(_controller.Speed.x));
            CorgiTools.UpdateAnimatorBool(_animator, "Jumping", !_controller.State.IsGrounded && _controller.Speed.y > 0);
            CorgiTools.UpdateAnimatorBool(_animator, "Running", BehaviorState.Running);
            CorgiTools.UpdateAnimatorBool(_animator, "Dashing", BehaviorState.Dashing);
            CorgiTools.UpdateAnimatorBool(_animator, "Crouching", BehaviorState.Crouching);
            CorgiTools.UpdateAnimatorBool(_animator, "LookingUp", BehaviorState.LookingUp);
            CorgiTools.UpdateAnimatorBool(_animator, "Diving", BehaviorState.Diving);
            CorgiTools.UpdateAnimatorBool(_animator, "Pickup", BehaviorState.Pickup);
            CorgiTools.UpdateAnimatorBool(_animator, "FiringStop", BehaviorState.FiringStop);
            CorgiTools.UpdateAnimatorBool(_animator, "Firing", BehaviorState.Firing);
            CorgiTools.UpdateAnimatorInteger(_animator, "FiringDirection", BehaviorState.FiringDirection);
            CorgiTools.UpdateAnimatorBool(_animator, "MeleeAttacking", BehaviorState.MeleeEnergized);
            CorgiTools.UpdateAnimatorBool(_animator, "Hurting", BehaviorState.Hurting);
            CorgiTools.UpdateAnimatorBool(_animator, "DoubleJumping", BehaviorState.DoubleJumping);

            if (!_isOffscreen)
            {
                CorgiTools.UpdateAnimatorBool(_animator, "Underwater", BehaviorState.Swimming);
                CorgiTools.UpdateAnimatorFloat(_animator, "vSpeed", _controller.Speed.y);
                CorgiTools.UpdateAnimatorBool(_animator, "Flying", HasFlyUpgrade && BehaviorState.NumberOfJumpsLeft == 0 && GameManager.Instance.Points > 0);
            }

            if (_controller.Speed.y < 0)
                CorgiTools.UpdateAnimatorBool(_animator, "SpinJumping", false);
        }
    }


    /// <summary>
    /// Sets the horizontal move value.
    /// </summary>
    /// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
    public virtual void SetHorizontalMove(float value)
    {
        _intendedHorizontalMove = value;

        if ((_controller.State.IsCollidingRight && _controller.movementDir() >= 0 && value > 0) ||
        (_controller.State.IsCollidingLeft && _controller.movementDir() <= 0 && value < 0)
        || BehaviorState.LookingUp)
        {
            _horizontalMove = 0;
        }
        else
        {
            if (BehaviorState.MeleeEnergized)
                _horizontalMove = 1.5f * value;
            else
            {
                _horizontalMove = value;
            }

            if (value > 0.65f || value < -0.65f)
                BehaviorState.LookingUp = false;
        }
    }

    /// <summary>
    /// Sets the vertical move value.
    /// </summary>
    /// <param name="value">Vertical move value, between -1 and 1
    public virtual void SetVerticalMove(float value)
    {
        _verticalMove = value;

        if (BehaviorState.MeleeEnergized)
        {
            if (value >= 0.65)
            {
                _controller.SetHorizontalForce(0);
                _controller.SetVerticalForce(15f * value);
            }
            else if (value <= -0.65)
            {
                _controller.SetHorizontalForce(0);
                _controller.SetVerticalForce(25f * value);
            }
            else
            {
                if (!BehaviorState.DoubleJumping)
                {
                    _controller.SetVerticalForce(0);
                }
                else
                {
                    JumpStop();
                }
            }

            BehaviorState.LookingUp = false;
        }
        else
        {
            if (BehaviorState.Swimming)
            {
                _controller.SetVerticalForce(5 * _verticalMove);

                if (VerticalMove > 0.125f)
                    _controller.NoFall();
                else
                    JumpStop();
            }
            else
            {
                if (_controller.State.IsGrounded)
                {
                    _shoot.SetVerticalMove(value);

                    if (_verticalMove >= 0.65 || _verticalMove <= -0.65)
                        _controller.SetHorizontalForce(0);
                }
                else
                {
                    _shoot.SetVerticalMove(0);
                }
            }
        }
    }

    /// <summary>
    /// Called at Update(), handles horizontal movement
    /// </summary>
    protected virtual void HorizontalMovement()
    {
        // if movement is prevented, we exit and do nothing
        if (!BehaviorState.CanMoveFreely)
            return;

        // If the value of the horizontal axis is positive, the character must face right.
        if (_horizontalMove > 0.65f)
        {
            BehaviorState.LookingUp = false;

            if (PlayerNum == PlayerRole.PlayerOne)
                _sceneCamera.ResetLookUpDown();

            _normalizedHorizontalSpeed = _horizontalMove;
            if (!_isFacingRight)
                Flip();
        }
        // If it's negative, then we're facing left
        else if (_horizontalMove < -0.65f)
        {
            BehaviorState.LookingUp = false;

            if (PlayerNum == PlayerRole.PlayerOne)
                _sceneCamera.ResetLookUpDown();

            _normalizedHorizontalSpeed = _horizontalMove;
            if (_isFacingRight)
                Flip();
        }
        else
        {
            _normalizedHorizontalSpeed = 0;
        }

        // we pass the horizontal force that needs to be applied to the controller.
        if (!BehaviorState.LookingUp)
        {
            var movementFactor = _controller.State.IsGrounded ? _controller.Parameters.SpeedAccelerationOnGround : _controller.Parameters.SpeedAccelerationInAir;
            if (BehaviorParameters.SmoothMovement && !_controller.State.IsGrounded)
                _controller.SetHorizontalForce(Mathf.Lerp(_controller.Speed.x, _normalizedHorizontalSpeed * BehaviorParameters.MovementSpeed, Time.deltaTime * movementFactor));
            else
                _controller.SetHorizontalForce(_normalizedHorizontalSpeed * BehaviorParameters.MovementSpeed);
        }
    }

    /// <summary>
    /// Called at Update(), handles vertical movement
    /// </summary>
    protected virtual void VerticalMovement()
    {
        if (BehaviorState.Swimming || BehaviorState.MeleeEnergized || !_controller.State.IsGrounded)
        {
            BehaviorState.LookingUp = false;
            return;
        }

        if (_verticalMove > 0.75f)
        {
            BehaviorState.LookingUp = true;

            if (PlayerNum == PlayerRole.PlayerOne && Mathf.Abs(_horizontalMove) < 0.65f)
                _sceneCamera.LookUp();
        }
        else if (_verticalMove < -0.75f)
        {
            BehaviorState.LookingUp = false;

            if (PlayerNum == PlayerRole.PlayerOne && Mathf.Abs(_horizontalMove) < 0.65f)
                _sceneCamera.LookDown();
        }
        else
        {
            BehaviorState.LookingUp = false;

            if (PlayerNum == PlayerRole.PlayerOne)
                _sceneCamera.ResetLookUpDown();
        }

        // Manages the ground touching effect
        if (_controller.State.JustGotGrounded)
        {
            if (HitGround != null)
            {
                Vector3 hitPos = _controller.BottomPosition;
                hitPos += Vector3.up;
                var poof = Instantiate(HitGround, hitPos, transform.rotation);
                Destroy(poof.gameObject, 0.5f);
            }

            _sceneCamera.RestoreSpeed();
        }
    }

    /// <summary>
    /// Use this method to force the controller to recalculate the rays, especially useful when the size of the character has changed.
    /// </summary>
    public virtual void RecalculateRays()
    {
        _controller.SetRaysParameters();
    }

    /// <summary>
    /// Causes the character to start running.
    /// </summary>
    public virtual void RunStart()
    {
        // if the Run action is enabled in the permissions, we continue, if not we do nothing
        if (!Permissions.RunEnabled)
            return;

        // if the character is not in a position where it can move freely, we do nothing.
        if (!BehaviorState.CanMoveFreely)
            return;

        // if the player presses the run button and if we're on the ground and not crouching and we can move freely, 
        // then we change the movement speed in the controller's parameters.
        if (BehaviorState.Swimming)
        {
            BehaviorParameters.MovementSpeed = BehaviorParameters.SwimmingSpeed;
        }
        else
        {
            BehaviorState.Running = true;
        }
    }

    /// <summary>
    /// Causes the character to stop running.
    /// </summary>
    public virtual void RunStop()
    {
        // if the run button is released, we revert back to the walking speed.
        BehaviorParameters.MovementSpeed = BehaviorParameters.WalkSpeed;
        BehaviorState.Running = false;
    }

    /// <summary>
    /// Causes the character to start jumping.
    /// </summary>
    public virtual void JumpStart()
    {

        // if the Jump action is enabled in the permissions, we continue, if not we do nothing. If the player is dead, we do nothing.
        if (!Permissions.JumpEnabled || !JumpAuthorized || BehaviorState.IsDead || _controller.State.IsCollidingAbove || BehaviorState.MeleeEnergized)
            return;

        if (BehaviorState.Swimming)
        {
            _controller.SetVerticalForce(5);
            _controller.NoFall();
            return;
        }

        if (HasFlyUpgrade && BehaviorState.NumberOfJumpsLeft == 0)
            BehaviorState.NumberOfJumpsLeft = 1;

        _jumpButtonPressTime = Time.time;
        _jumpButtonPressed = true;
        _jumpButtonReleased = false;

        // we check if the character can jump without conflicting with another action
        BehaviorState.CanJump = (_controller.State.IsGrounded || BehaviorState.LadderClimbing || BehaviorState.NumberOfJumpsLeft > 0);

        // if the player can't jump, we do nothing. 
        if ((!BehaviorState.CanJump) && !(BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhereAnyNumberOfTimes))
            return;

        if (_controller.State.IsGrounded)
        {
            if (HitGround != null)
            {
                Vector3 hitPos = _controller.BottomPosition;
                hitPos += Vector3.up;
                var poof = Instantiate(HitGround, hitPos, transform.rotation);
                Destroy(poof.gameObject, 0.5f);
            }
        }

        // we decrease the number of jumps left
        BehaviorState.NumberOfJumpsLeft = BehaviorState.NumberOfJumpsLeft - 1;
        BehaviorState.LadderClimbing = false;
        BehaviorState.CanMoveFreely = true;
        GravityActive(true);

        // we play the jump sound
        if (PlayerJumpSfx != null)
            SoundManager.Instance.PlaySound(PlayerJumpSfx, transform.position);

        float scale = 2f;
        if (HasFlyUpgrade && BehaviorState.NumberOfJumpsLeft == 0 && GameManager.Instance.Points > 0)
            scale = 0.5f;

        if (BehaviorState.NumberOfJumpsLeft != 0)
            _controller.SetVerticalForce(Mathf.Sqrt(scale * BehaviorParameters.JumpHeight * Mathf.Abs(_controller.Parameters.Gravity)));
        else
        {
            // Boost horizontal speed // scale was 1.75f
            float y = Mathf.Sqrt(scale * BehaviorParameters.JumpHeight * Mathf.Abs(_controller.Parameters.Gravity));
            _controller.SetForce(new Vector2(0, y));
        }

        if (HasFlyUpgrade && BehaviorState.NumberOfJumpsLeft == 0 && GameManager.Instance.Points > 0 && !BehaviorState.Swimming)
        {
            if (PlayerMeleeSfx != null)
                SoundManager.Instance.PlaySound(PlayerMeleeSfx, transform.position);

            GravityActive(false);
        }
    }

    /// <summary>
    /// Causes the character to stop jumping.
    /// </summary>
    public virtual void JumpStop()
    {
        if (HasFlyUpgrade && BehaviorState.NumberOfJumpsLeft == 0)
            GravityActive(true);

        //Debug.Log("Hard jump stop");
        _jumpButtonPressed = false;
        _jumpButtonReleased = true;
        _controller.ResetGravity();
    }


    public virtual void OnPlug(Plug plug)
    {
        _plug = plug;
        BehaviorState.CanPickup = true;
    }

    public virtual void OffPlug(Plug plug)
    {
        if (_plug == plug)
        {
            _plug = null;
        }
        BehaviorState.CanPickup = false;
    }


    /// <summary>
    /// Pulls a plug fuse out of the ground
    /// </summary>
    public void Pickup()
    {
        if (!_controller.State.IsGrounded)
            return;

        if (BehaviorState.CanPickup)
        {
            if (BehaviorState.Pickup)
                return;

            _shoot.ShootStop();

            if (PlayerPickupSfx != null)
                SoundManager.Instance.PlaySound(PlayerPickupSfx, transform.position);

            if (_plug != null)
            {
                // Open door
                _plug.Pull();
                _plug = null;

                BehaviorState.CanPickup = false;
                GameManager.Instance.CanMove = false;
                BehaviorState.Pickup = true;
                SetHorizontalMove(0);
                StartCoroutine(StopPickup());
            }
        }
    }


    protected virtual IEnumerator StopPickup()
    {
        yield return new WaitForSeconds(BehaviorParameters.PickupDuration);
        BehaviorState.Pickup = false;
        GameManager.Instance.CanMove = true;
        SetHorizontalMove(0);
    }


    public virtual IEnumerator Powerup()
    {
        if (PlayerPowerupSfx != null)
            SoundManager.Instance.PlaySound(PlayerPowerupSfx, transform.position);

        GameManager.Instance.FreezeCharacter();
        CorgiTools.UpdateAnimatorBool(_animator, "Powerup", true);
        yield return new WaitForSeconds(0.583f);
        GameManager.Instance.ThawCharacter();
        CorgiTools.UpdateAnimatorBool(_animator, "Powerup", false);
    }


    /// <summary>
    /// Activates or desactivates the gravity for this character only.
    /// </summary>
    /// <param name="state">If set to <c>true</c>, activates the gravity. If set to <c>false</c>, turns it off.</param>
    protected virtual void GravityActive(bool state)
    {
        if (state == true)
        {
            if (_controller.Parameters.Gravity == 0)
            {
                _controller.Parameters.Gravity = _originalGravity;
                _controller.ResetGravity();
                _controller.SetVerticalForce(-0.1f);
            }
        }
        else
        {
            if (_controller.Parameters.Gravity != 0)
                _originalGravity = _controller.Parameters.Gravity;
            _controller.Parameters.Gravity = 0;
        }
    }

    /// <summary>
    /// makes the character colliding again with layer 12 (Projectiles) and 13 (Enemies)
    /// </summary>
    /// <returns>The layer collision.</returns>
    protected virtual IEnumerator ResetLayerCollision(float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics2D.IgnoreLayerCollision(9, 12, false);
        Physics2D.IgnoreLayerCollision(9, 13, false);
        BehaviorState.Hurting = false;
        _targetAmbientColor = _defaultAmbientColor;
    }

    /// <summary>
    /// Kills the character, sending it in the air
    /// </summary>
    public virtual void Kill()
    {
        if (BehaviorState.IsDead)
            return;

        // we set its dead state to true
        BehaviorState.IsDead = true;

        //		_gun.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
        _animator.Play("AvaDeath");
        _targetAmbientColor = Color.white;

        // we make it ignore the collisions from now on
        //_controller.CollisionsOff ();
        //GetComponent<Collider2D> ().enabled = false;

        // we set its health to zero (useful for the healthbar)
        Health = 0;

        //_controller.SetForce(new Vector2(0,10));
        StartCoroutine(ReallyKill(0.458f));
    }

    protected virtual IEnumerator ReallyKill(float delay)
    {
        yield return new WaitForSeconds(delay);

        _animator.enabled = false;

        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }

        // we send it in the air
        _controller.ResetParameters();
        ResetParameters();

        GameManager.Instance.Player = null;
    }

    /// <summary>
    /// Called to disable the player (at the end of a level for example. 
    /// It won't move and respond to input after this.
    /// </summary>
    public virtual void Disable()
    {
        enabled = false;
        _controller.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    /// <summary>
    /// Makes the player respawn at the location passed in parameters
    /// </summary>
    /// <param name="spawnPoint">The location of the respawn.</param>
    public virtual void RespawnAt(Transform spawnPoint, bool flip)
    {
        // we raise it from the dead (if it was dead)
        BehaviorState.IsDead = false;

        Vector3 verticalOffset = Vector3.zero;

        if (flip)
            Flip();

        float offset = flip ? -0.5f : 0.5f;

        transform.position = spawnPoint.position + verticalOffset + offset * Vector3.right;

        StartCoroutine(EnableMove(flip));

        //Debug.Log(transform.position);

        //		Health = BehaviorParameters.MaxHealth;
    }


    public IEnumerator EnableMove(bool flip)
    {
        yield return new WaitForSeconds(0.0625f);

        // we re-enable its 2D collider
        GetComponent<Collider2D>().enabled = true;

        // Scoot player into the gate
        if (flip)
        {
            SetHorizontalMove(1f);
        }
        else
        {
            SetHorizontalMove(-1f);
        }

        yield return new WaitForSeconds(0.3f);

        // we make it handle collisions again
        _controller.CollisionsOn();

        GameManager.Instance.CanMove = true;
    }


    public virtual void TakePartialDamage(float rate, Color targetColor)
    {
        _partialDamageRate = rate;
        _targetColor = targetColor;
    }


    /// <summary>
    /// Called when the player takes damage
    /// </summary>
    /// <param name="damage">The damage applied.</param>
    /// <param name="instigator">The damage instigator.</param>
    public virtual void TakeDamage(int damage, GameObject instigator, bool melee = false)
    {
        if (BehaviorState.IsDead || Health <= 0 || damage == 0 || !TakesDamage || BehaviorState.MeleeEnergized)
            return;

        // we prevent the character from colliding with layer 12 (Projectiles) and 13 (Enemies)
        Physics2D.IgnoreLayerCollision(9, 12, true);
        Physics2D.IgnoreLayerCollision(9, 13, true);

        BehaviorState.Hurting = true;

        StartCoroutine(ResetLayerCollision(0.5f));

        // we decrease the character's health by the damage
        Health -= damage;

        GameManager.Instance.AddPointsInstant(-50);

        if (GameManager.Instance.Points > 0)
            Gem.popIntoMiniGems(gameObject, 5);

        _flickerPrimary.Flicker();

        if (PlayerNum == PlayerRole.PlayerOne)
            StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.red));
        else
            StartCoroutine(GUIManager.Instance.HealthBarTwo.Flicker(Color.red));

        if (Health <= 0)
        {
            _isOffscreen = false;
            BehaviorState.IsDead = true;
            CorgiTools.UpdateAnimatorBool(_animator, "Dead", true);

            _shoot.ShootStop();


            if (PlayerDeadSfx != null)
                SoundManager.Instance.PlaySound(PlayerDeadSfx, transform.position);

            if (PlayerNum == PlayerRole.PlayerTwo)
            {
                StartCoroutine(Gem.popIntoMagic(gameObject, 0.01f));
                StartCoroutine(HideSprite(0.015f));
                GameManager.Instance.PlayerTwo = null;
                GUIManager.Instance.SetHealthTwoActive(false);
                Destroy(gameObject, 0.5f);
                return;
            }

            StartCoroutine(Gem.popIntoMagic(gameObject, 0.467f));
            StartCoroutine(HideSprite(0.6f));

            SoundManager.Instance.StopAllMusic();
            GameManager.Instance.FreezeCharacter();

            CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            if (sceneCamera != null)
            {
                Vector3 ShakeParameters = new Vector3(1, 0.5f, 1f);
                sceneCamera.Shake(ShakeParameters);
                sceneCamera.FreezeAt(transform.position);
            }

            _sceneCamera.Dim();

            AnalyticsEvent.Custom("player_death", new Dictionary<string, object>
            {
                { "level", GlobalVariables.LevelIndex },
                { "world", GlobalVariables.WorldIndex },
                { "player", (PlayerNum == PlayerRole.PlayerOne) ? "1" : "2" },
                { "time_elapsed", Time.timeSinceLevelLoad }
            });

            StartCoroutine(KillMe(1.667f));
        }
        else
        {
            // we play the sound the player makes when it gets hit
            if (PlayerHitSfx != null)
                SoundManager.Instance.PlaySound(PlayerHitSfx, transform.position);

            // Let's handle the fight music. If she's hurt, crank it up!
            if (fightMusic != null)
            {
                fightMusic.Play();
            }

            TakesDamage = false;
            StartCoroutine(ResumeTakingDamage());
        }
    }

    public IEnumerator HideSprite(float delay)
    {
        yield return new WaitForSeconds(delay);

        _ava.enabled = false;
    }

    /// <summary>
    /// Called when the character gets health (from a stimpack for example)
    /// </summary>
    /// <param name="health">The health the character gets.</param>
    /// <param name="instigator">The thing that gives the character health.</param>
    public virtual void GiveHealth(int health, GameObject instigator)
    {
        // this function adds health to the character's Health and prevents it to go above MaxHealth.
        Health = Mathf.Min(Health + health, BehaviorParameters.MaxHealth);

        PartialDamage = 0;

        if (PlayerNum == PlayerRole.PlayerOne)
            StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.green));
        else
            StartCoroutine(GUIManager.Instance.HealthBarTwo.Flicker(Color.green));
    }

    /// <summary>
    /// Flips the character and its dependencies (jetpack for example) horizontally
    /// </summary>
    public virtual void Flip()
    {
        // Flips the character horizontally
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        _isFacingRight = transform.localScale.x > 0;

        if (_shoot != null)
        {
            _shoot.Flip();
        }
    }


    public void SetAnimatorLayer()
    {
        for (int i = 0; i < 9; i++)
        {
            _animator.SetLayerWeight(i, 0);
        }
        _animator.SetLayerWeight(_shoot.WeaponIndex + _specialIndex * 3, 1);
    }


    public virtual void ResetParameters()
    {
        _overrideBehaviorParameters = DefaultBehaviorParameters;
    }

    /// <summary>
    /// Called when the character collides with something else
    /// </summary>
    /// <param name="other">The other collider.</param>
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (!BehaviorState.MeleeEnergized)
            return;

        Health enemy = collider.GetComponent<Health>();
        if (enemy != null)
        {
            enemy.TakeDamage(20, gameObject, true);
            return;
        }

        /*var parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
        if (parameters == null)
            return;
        // if the other collider has behavior parameters, we override ours with them.
        _overrideBehaviorParameters = parameters.BehaviorParameters;*/
    }

    /// <summary>
    /// Called when the character is colliding with something else
    /// </summary>
    /// <param name="other">The other collider.</param>
    public virtual void OnTriggerStay2D(Collider2D collider)
    {
    }

    /// <summary>
    /// Called when the character exits a collider
    /// </summary>
    /// <param name="collider">The other collider.</param>
    public virtual void OnTriggerExit2D(Collider2D collider)
    {
        /*var parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
        if (parameters == null)
            return;

        // if the other collider had behavior parameters, we reset ours
        _overrideBehaviorParameters = null;*/
    }


   


    public void OnVisible()
    {
        if(_offscreenReturnRoutine != null)
        {
            StopCoroutine(_offscreenReturnRoutine);
            _offscreenReturnRoutine = null;
        }
    }


    public void OnInvisible(bool UseTimer = true)
    {
        // Start a timer. If player two is lost for too long, fly back in ala tails
        if (PlayerNum == PlayerRole.PlayerTwo && isActiveAndEnabled)
        {
            if (_offscreenReturnRoutine == null)
            {
                shoot.ShootStop();

                if (UseTimer)
                {
                    _offscreenStartTime = Time.time;
                    _offscreenReturnRoutine = GameManager.Instance.StartCoroutine(GameManager.Instance.BeamOut(5f, true));
                }
            }
        }
    }

    public void Sparkle()
    {
        if (_fx != null)
        {
            _fx.GetComponent<Animator>().Play("Sparkle", 0, 0);
        }
    }

    public void Hiccup(float time)
    {
        if (!_hiccupping)
        {
            _hiccupping = true;
            if (_hiccupRoutine != null)
                StopCoroutine(_hiccupRoutine);
            _hiccupRoutine = StartCoroutine(Heekup(time));
        }
    }

    protected IEnumerator KillMe(float delay)
    {
        yield return new WaitForSeconds(delay);
        _ava.color = Color.clear;
        LevelManager.Instance.KillPlayer();

        if (PlayerNum == PlayerRole.PlayerTwo)
            GlobalVariables.TwoPlayer = false;
    }

    public IEnumerator Heekup(float delay)
    {
        // Is this causing collision bugs?
        //Time.timeScale = 0.35f; //Stop time flow

        yield return new WaitForSeconds(delay);

        //Time.timeScale = 1.0f; //Recover time flow

        _hiccupping = false;
    }

    public void EnterWater(float damage, Color color)
    {
        // Don't enter water if flying in from offscreen --- wait for that to be done first.
        if (_isOffscreen)
            return;

        if(PlayerNum == PlayerRole.PlayerTwo)
            CorgiTools.UpdateAnimatorBool(_animator, "Flying", false);

        BehaviorState.Swimming = true;

        _controller.SlowFall(BehaviorParameters.SwimmingSlowFallFactor);
        _controller.SlowJump(BehaviorParameters.SwimmingSlowJumpFactor);

        if (damage != 0)
            TakePartialDamage(damage, color);

        BehaviorParameters.JumpRestrictions = CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhereAnyNumberOfTimes;

        // Just assume all spores are cleared
        _controller.Parameters.MaxVelocity = _controller.OriginalParameters.MaxVelocity;
        Permissions.MeleeAttackEnabled = true;
        BehaviorState.CoveredInSpores = false;

        if (PlayerNum == PlayerRole.PlayerOne)
            _sceneCamera.ResetLookUpDown();
    }

    public void ExitWater(float waterExitForce)
    {
        Debug.Log(tag + " exiting water");

        if (_isOffscreen)
            CorgiTools.UpdateAnimatorBool(_animator, "Flying", true);

       // Just assume all spores are cleared
       _controller.Parameters.MaxVelocity = _controller.OriginalParameters.MaxVelocity;

        BehaviorState.Swimming = false;

        _controller.SlowFall(0f);
        _controller.SlowJump(0f);
        _controller.ResetGravity();

        // when the character is not colliding with the water anymore, we reset its various water related states
        BehaviorParameters.JumpRestrictions = CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhere;

        _partialDamageRate = -0.1f;

        // we also push it up in the air        
        //      Splash (character.transform.position);
        BehaviorState.NumberOfJumpsLeft = BehaviorParameters.NumberOfJumps;

        if (waterExitForce != 0)
            _controller.SetVerticalForce(Mathf.Abs(waterExitForce));
    }


    public void Drain()
    {
        _animator.SetBool("Draining", true);

        if (PlayerDrainSfx != null)
            SoundManager.Instance.PlaySound(PlayerDrainSfx, transform.position);

        Say(0);

        StartCoroutine(StopDraining(0.25f));
    }

    public virtual IEnumerator StopDraining(float duration)
    {
        yield return new WaitForSeconds(duration);

        _animator.SetBool("Draining", false);
    }

    public void Say(int something = 0, int duration = 3)
    {
        AISayThings sayThings = GetComponent<AISayThings>();

        if (sayThings != null)
            sayThings.SaySomething(something, duration);
    }


    public IEnumerator FreezeThawEnemies(bool freeze = true, bool affectSelf = true, float duration = 1f)
    {
        yield return new WaitForSeconds(duration);

        if (affectSelf)
        {
            if (freeze)
                GameManager.Instance.FreezeCharacter();
            else
                GameManager.Instance.ThawCharacter();
        }

        var maskLayer = 1 << LayerMask.NameToLayer("Enemies");
        RaycastHit2D[] circles = Physics2D.CircleCastAll(transform.localPosition, 400.0f, Vector2.right, 0.0f, maskLayer);

        for (int i = 0; i < circles.Length; i++)
        {
            var circle = circles[i];

            var optimize = circle.collider.gameObject.GetComponent<Optimize>();

            if (optimize != null)
            {
                optimize.SetState(!freeze);
            }
        }
    }


    public IEnumerator ResumeTakingDamage(float duration = 1f)
    {
        yield return new WaitForSeconds(duration);

        TakesDamage = true;
    }

    void OnDestroy()
    {
        if(PlayerNum == PlayerRole.PlayerTwo)
        {
            Debug.Log("Avery gone");
            GlobalVariables.TwoPlayer = false;

            if(GameManager.Instance != null)
                GameManager.Instance.PlayerTwo = null;

            if(GUIManager.Instance != null)
                GUIManager.Instance.SetHealthTwoActive(false);
        }
    }
}
