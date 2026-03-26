using UnityEngine;
using System.Collections;

/// <summary>
/// Add this component to a CorgiController2D and it will try to kill your player on sight.
/// </summary>
public class AIShootOnSight : MonoBehaviour 
{
	
	/// The fire rate (in seconds)
	public float FireRate = 1;
	public float FireDelay = 0.1f;
    public float YOffset = 0;

	/// The kind of projectile shot by the agent
	public Projectile Projectile;
	/// The maximum distance at which the AI can shoot at the player
	public float ShootDistance = 10f;
	public bool StopToShoot = true;
	public float AimAngle = 0.0f;
	public float ShootAngle = 0.0f;
    public int Burst = 1;

	public AudioClip SoundEffect = null;
	public AudioClip WarnSoundEffect = null;

    public bool isShooting 
    {
        get
        {
            return _firing;
        }
    }

	// private stuff
	protected float _canFireIn;
    protected Vector2 _direction;
	protected EnemyController _controller;
    protected Bat _bat;
	protected Animator _animator;
	protected AISimpleWalk _walk;
    protected AIOmniWalk _omni;
	protected AISimpleFly _fly;
	protected AISimpleClimb _climb;
	protected bool _shooting = false;
	protected SpriteRenderer _fireLocation;
	protected SpriteRenderer _sprite;
	private bool _firing = false;

	private Coroutine shootingRoutine;

    LayerMask mask;

    public SpriteRenderer FireLocation;

    /// initialization
    protected virtual void Start () 
	{
		// we get the CorgiController2D component
		_controller = GetComponent<EnemyController>();

        if (_controller == null)
            _bat = GetComponent<Bat>();

        _animator = GetComponent<Animator> ();
		_walk = GetComponent<AISimpleWalk> ();
        _omni = GetComponent<AIOmniWalk>();
        _fly = GetComponent<AISimpleFly> ();
		_climb = GetComponent<AISimpleClimb> ();

        mask = 1 << LayerMask.NameToLayer("Player");

        if (GetComponentsInChildren<SpriteRenderer>().Length > 1 && FireLocation == null)
            _fireLocation = GetComponentsInChildren<SpriteRenderer>()[1];
        else
            _fireLocation = FireLocation;

		_sprite = GetComponent<SpriteRenderer>();
    }

	public void CancelShoot()
	{
		if(shootingRoutine != null)
			StopCoroutine (shootingRoutine);
	}

    /// Every frame, check for the player and try and kill it
    protected virtual void FixedUpdate ()
	{
		// fire cooldown
		if ((_canFireIn -= Time.deltaTime) > 0 || _shooting) {
			return;
		}

		float flip = 1;
		if (_sprite.flipX)
			flip = -1;

        _direction = new Vector2(flip * transform.localScale.x * (float)Mathf.Cos(Mathf.Deg2Rad * AimAngle), transform.localScale.y * -(float)Mathf.Sin(Mathf.Deg2Rad * AimAngle));

        // we cast a ray in front of the agent to check for a Player
        Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y + YOffset);
        RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, _direction, ShootDistance, mask, true, Color.red);

        if (!raycast)
			return;

		_canFireIn = FireRate;
		_shooting = true;

        // if the ray has hit the player, we fire a projectile
        CorgiTools.UpdateAnimatorBool(_animator, "Shooting", true);

        if (StopToShoot)
		{
			if (_walk != null) {
                _walk.Disable();
			}

            if(_omni != null) {
                _omni.Speed = 0;
            }

			if (_fly != null) {
				_fly.Speed = 0;
			}

			if (_climb != null) {
				_climb.Speed = 0;
			}
		}

		shootingRoutine = StartCoroutine (Shoot ());
	}

	public virtual IEnumerator Shoot()
	{
		if (WarnSoundEffect != null)
			SoundManager.Instance.PlaySound (WarnSoundEffect, transform.position);
		
		yield return new WaitForSeconds (FireDelay);

		Vector2 firePos = transform.position + YOffset*Vector3.up;

		if (_fireLocation != null)
			firePos = _fireLocation.transform.position;

		if (SoundEffect != null)
			SoundManager.Instance.PlaySound (SoundEffect, transform.position);

		float flip = 1;
		if (_sprite.flipX)
			flip = -1;

		Vector2 d = new Vector2(flip * transform.localScale.x * (float)Mathf.Cos(Mathf.Deg2Rad * (ShootAngle + AimAngle)), transform.localScale.y * -(float)Mathf.Sin(Mathf.Deg2Rad * (ShootAngle + AimAngle)));

        _firing = true;
        StartCoroutine(ClearFiring());

        for (int i = 0; i < Burst; i++)
        {
            Projectile projectile = (Projectile)Instantiate(Projectile, firePos + i*_direction*0.25f, transform.rotation);

            if(_controller != null)
                projectile.Initialize(gameObject, d, _controller.Speed, false);
            else
            {
                if(_bat)
                    projectile.Initialize(gameObject, d, new Vector2(_bat.FlutterSpeed, 0), false);
                else
                    projectile.Initialize(gameObject, d, Vector2.zero, false);
            }
        }

		if (_animator != null) {
			StartCoroutine (StopShooting ());
		}
	}

	public void Reset()
	{
		CorgiTools.UpdateAnimatorBool(_animator,"Shooting",false);
		_shooting = false;
	}

    public virtual IEnumerator ClearFiring()
    {
        yield return new WaitForSeconds(0.1f);
        _firing = false;
    }

	public virtual IEnumerator StopShooting()
	{
		yield return new WaitForSeconds (FireRate);
		Reset ();

		if (StopToShoot)
		{
			if (_walk != null) {
				_walk.Walk ();
			}

            if(_omni != null) {
                _omni.Walk();
            }

			if (_fly != null) {
				_fly.Fly ();
			}

			if (_climb != null) {
				_climb.Climb ();
			}
		}
	}
}
