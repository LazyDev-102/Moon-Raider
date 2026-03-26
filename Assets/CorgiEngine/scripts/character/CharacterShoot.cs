using UnityEngine;
using System.Collections;
using JulienFoucher;

/// <summary>
/// Add this class to a character so it can shoot projectiles
/// </summary>
public class CharacterShoot : MonoBehaviour 
{
	/// the initial weapon owned by the character
	public Weapon [] Weapons;

    public AudioClip RapidStartSound;

    public int WeaponIndex = 0;
    public int MaxWeaponIndex = 0;

    public int LastWeaponIndex
    {
        get
        {
            return Weapons.Length - 1;
        }
    }
	
	/// the position the weapon will be attached to
	public Transform WeaponAttachment;
    public Transform WeaponAttachmentUp;
    public Transform WeaponAttachmentSwim;

    public SpriteRenderer muzzleFlash;
	public float OffsetY = 0.0f;

	private float gunXMagnitude = 0.0f;
	private float gunYMagnitude = 0.0f;

    protected Weapon _weapon;
    protected float _fireTimer;

    protected float _horizontalMove;
    protected float _verticalMove;

    protected CharacterBehavior _characterBehavior;
    protected CorgiController _controller;

	private float _orgGrav;
	private float nextFire = 0.0F;
	private bool ShootingUp = false;
    private bool RapidFire = false;
    private bool WarmingUpRapidFire = false;
    private Coroutine StartRapidFire = null;


	// Initialization
	protected virtual void Start () 
	{
		// initialize the private vars
		_characterBehavior = GetComponent<CharacterBehavior>();
		_controller = GetComponent<CorgiController>();

		_orgGrav = _controller.DefaultParameters.Gravity;

		// filler if the WeaponAttachment has not been set
		if (WeaponAttachment == null)
			WeaponAttachment = transform;

		// we set the initial weapon
        ChangeWeapon(Weapons[WeaponIndex]);
	}


    protected virtual void FixedUpdate()
    {
        if (_characterBehavior.BehaviorState.Firing)
            TryShoot();
    }

	
	/// <summary>
	/// Makes the character shoot once.
	/// </summary>
	public virtual void ShootOnce()
	{
		// if the Shoot action is enabled in the permissions, we continue, if not we do nothing. If the player is dead we do nothing.
		if (!_characterBehavior.Permissions.ShootEnabled || _characterBehavior.BehaviorState.IsDead)
			return;

		// if the character can't fire, we do nothing		
		if (!_characterBehavior.BehaviorState.CanShoot)
		{			
			// we just reset the firing direction (this happens when the character gets on a ladder for example.
			_characterBehavior.BehaviorState.FiringDirection = 3;
			return;		
		}
		
		// if the character is not in a position where it can move freely, we do nothing.
		if (!_characterBehavior.BehaviorState.CanMoveFreely)
			return;

		if (Time.time < nextFire)
			return;
		
		// we fire a projectile and reset the fire timer
		FireProjectile();	
		_fireTimer = 0;	
	}

    /// <summary>
    /// Causes the character to start shooting
    /// </summary>
    public virtual void ShootStart()
    {
        // if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
        if (!_characterBehavior.Permissions.ShootEnabled || _characterBehavior.BehaviorState.IsDead)
            return;

        // if the character can't fire, we do nothing		
        if (!_characterBehavior.BehaviorState.CanShoot)
        {
            // we just reset the firing direction (this happens when the character gets on a ladder for example.
            _characterBehavior.BehaviorState.FiringDirection = 3;

            if (_characterBehavior.BehaviorState.NumberOfJumpsLeft == 0)
                _characterBehavior.BehaviorState.WantsToShoot = true;

            return;
        }

        // if the character is not in a position where it can move freely, we do nothing.
        if (!_characterBehavior.BehaviorState.CanMoveFreely)
            return;

        // firing state reset								
        _characterBehavior.BehaviorState.FiringStop = false;
        _characterBehavior.BehaviorState.Firing = true;

        //_weapon.SetGunFlamesEmission(true);
        //_weapon.SetGunShellsEmission (true);

        if(RapidFire)
            nextFire = Time.time + 0.75f * _weapon.FireRate;
        else
            nextFire = Time.time + _weapon.FireRate;

        TryShoot();
    }

    void TryShoot()
    {
        _fireTimer += Time.deltaTime;

        float r = _weapon.FireRate;

        if (RapidFire)
            r = 0.75f * _weapon.FireRate;

        if (_fireTimer > r)
		{
            FireProjectile();

            _fireTimer = 0; // reset timer for fire rate

            if (!RapidFire)
            {
                if (!WarmingUpRapidFire && GameManager.Instance.Points > 0)
                {
                    WarmingUpRapidFire = true;
                    StartRapidFire = StartCoroutine(EnableRapidFire(1.25f));
                }
            }
            else
            {
                if (GameManager.Instance.Points > 0)
                    GameManager.Instance.AddPointsInstant(-1);
                else
                {
                    StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.yellow));
                    StopRapidFire();
                }
            }
        }		
	}
	
	/// <summary>
	/// Causes the character to stop shooting
	/// </summary>
	public virtual void ShootStop()
	{
        _characterBehavior.BehaviorState.WantsToShoot = false;

        // if the Shoot action is enabled in the permissions, we continue, if not we do nothing
        if (!_characterBehavior.Permissions.ShootEnabled)
			return;

		// if the character can't fire, we do nothing		
		if (!_characterBehavior.BehaviorState.CanShoot)
		{			
			// we just reset the firing direction (this happens when the character gets on a ladder for example.
			_characterBehavior.BehaviorState.FiringDirection=3;
            _characterBehavior.BehaviorState.FiringStop = true;
            _characterBehavior.BehaviorState.Firing = false;
            return;		
		}

		_characterBehavior.BehaviorState.FiringStop = true;		
		_characterBehavior.BehaviorState.Firing = false;

        // reset the firing direction
        _characterBehavior.BehaviorState.FiringDirection = 3;

        StopRapidFire();
    }

    protected IEnumerator EnableRapidFire(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (RapidStartSound != null)
            SoundManager.Instance.PlaySound(RapidStartSound, transform.position);

        RapidFire = true;
    }

    void StopRapidFire()
    {
        if(StartRapidFire != null)
            StopCoroutine(StartRapidFire);
        RapidFire = false;
        StartRapidFire = null;
        WarmingUpRapidFire = false;
    }
	
	/// <summary>
	/// Changes the character's current weapon to the one passed as a parameter
	/// </summary>
	/// <param name="newWeapon">The new weapon.</param>
	public virtual void ChangeWeapon(Weapon newWeapon)
	{
        // if the character already has a weapon, we make it stop
        if (_weapon != null)
        {
			ShootStop();
		}
        // weapon instaniation
        _weapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position, WeaponAttachment.transform.rotation);
        _weapon.transform.parent = transform;
        // we turn off the gun's emitters.
        _weapon.SetGunFlamesEmission(false);
        _weapon.SetGunShellsEmission(false);

        gunXMagnitude = Mathf.Abs(_weapon.ProjectileFireLocation.localPosition.x);
        gunYMagnitude = Mathf.Abs(_weapon.ProjectileFireLocation.localPosition.y);
    }

    /// <summary>
    /// Fires one of the current weapon's projectiles
    /// </summary>
    protected virtual void FireProjectile () 
	{
        if (!_characterBehavior.Permissions.ShootEnabled || _characterBehavior.BehaviorState.IsDead)
            return;
		
		// if the projectile fire location has not been set, we do nothing and exit
		if (_weapon.ProjectileFireLocation == null)
			return;

		StartCoroutine (RestoreFall ());

		bool _isFacingRight = transform.localScale.x > 0;

		// we calculate the angle based on the buttons the player is pressing to determine the direction of the shoot.									
		float angle = 0;

		if(!ShootingUp)
			angle = _isFacingRight ? 90f : -90f;

		Vector2 direction = Vector2.up;

        // TODO: Make sure the local offset is flipped on the X
        Vector3 projectileOrigin = WeaponAttachment.position;

        if(_characterBehavior.BehaviorState.LookingUp)
        {
            projectileOrigin = WeaponAttachmentUp.position;
        }
        else if(_characterBehavior.BehaviorState.Swimming)
        {
            projectileOrigin = WeaponAttachmentSwim.position;
        }

        // We set the animation depending on where the character is shooting

        // if shooting up
        if (ShootingUp)
			_characterBehavior.BehaviorState.FiringDirection = 1;
		else
			_characterBehavior.BehaviorState.FiringDirection = 3;
		
		// we move the ProjectileFireLocation according to the angle
		bool _facingRight = transform.localScale.x > 0;
		float horizontalDirection = _facingRight ? 1f : -1f;
		
		// we instantiate the projectile at the projectileFireLocation's position.		
        float[] angleOffset = { 0, 0, 0, 0, 0 };

        if (ShootingUp)
        {
            if(_facingRight)
            {
                angleOffset[1] = 45;
                angleOffset[2] = 270;
            }
            else
            {
                angleOffset[1] = 315;
                angleOffset[2] = 90;
            }
        }
        else
        {
            if (_facingRight)
            {
                angleOffset[1] = 45;
                angleOffset[2] = 180;
            }
            else
            {
                angleOffset[1] = 225;
                angleOffset[2] = 0;
            }
        }

        for (int i = 0; i < _weapon.ProjectileCount; i++)
        {
            // we rotate the gun towards the direction
            direction = Quaternion.Euler(0, 0, -angle + angleOffset[i]) * direction;
            _weapon.GunRotationCenter.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle * horizontalDirection + 90f + angleOffset[i]));

            Projectile projectile = Instantiate(_weapon.Projectile[i], projectileOrigin, _weapon.ProjectileFireLocation.rotation); 
            projectile.Initialize(gameObject, direction, _controller.Speed, RapidFire);

            if (RapidFire)
            {
                projectile.Speed = 1.5f * projectile.Speed;
                SpriteRenderer sprite = projectile.GetComponentInChildren<SpriteRenderer>();
                sprite.color = RapidFire ? new Color(1, 0.75f, 0.75f) : Color.white;
                projectile.GetComponentsInChildren<SpriteTrail>()[0].enabled = false;
                projectile.GetComponentsInChildren<SpriteTrail>()[1].enabled = true;
            }
        }

		// Lower gravity while shooting
		_controller.Parameters.Gravity = _orgGrav/2;

		// Kickback
		if(!ShootingUp)
		{
			_controller.AddHorizontalForce(-5 * horizontalDirection);
		}	
	}

    protected  IEnumerator HideFlash()
	{
		yield return new WaitForSeconds(0.1f);
		muzzleFlash.enabled = false;
	}

	protected  IEnumerator RestoreFall()
	{
		// Lower gravity while shooting
		yield return new WaitForSeconds(0.1f);
		_controller.Parameters.Gravity = _orgGrav;
	}

    protected virtual Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle) 
	{
		
		angle = angle*(Mathf.PI/180f);
		var rotatedX = Mathf.Cos(angle) * (point.x - pivot.x) - Mathf.Sin(angle) * (point.y-pivot.y) + pivot.x;
		var rotatedY = Mathf.Sin(angle) * (point.x - pivot.x) + Mathf.Cos(angle) * (point.y - pivot.y) + pivot.y;
		return new Vector3(rotatedX,rotatedY,0);		
	}

	/// <summary>
	/// Sets the horizontal move value.
	/// </summary>
	/// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
	public virtual void SetHorizontalMove(float value)
	{
		_horizontalMove = value;
	}
	
	/// <summary>
	/// Sets the vertical move value.
	/// </summary>
	/// <param name="value">Vertical move value, between -1 and 1
	public virtual void SetVerticalMove(float value)
	{
		_verticalMove = value;
		ShootingUp = _verticalMove >= 0.75f && (_controller.State.IsGrounded && !_characterBehavior.BehaviorState.Swimming && !_characterBehavior.BehaviorState.MeleeEnergized);
	}
	
	public virtual void Flip()
	{
		if (!_weapon)
			return;
	}
}
