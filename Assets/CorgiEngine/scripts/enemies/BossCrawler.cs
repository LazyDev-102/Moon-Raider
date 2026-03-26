using UnityEngine;
using System.Collections;

public class BossCrawler : Boss
{
	public Projectile Projectile;
	public AudioClip AimFx;
	public AudioClip ShootFx;
	public AudioClip LaserFx;

	public AudioClip StartFx;
	public AudioClip StopFx;
	public AudioClip MoveFx;

	public float Speed = 3;
	public float CannonFireRate = 0.333f;

	private AudioSource _laserFX;
	private AudioSource _loopSound;

	private Animator _animator;
	private Animator _clawAnimator;
	private SpriteRenderer _sprite;
	private SpriteRenderer _laser;
	private BoxCollider2D _laserLeft;
	private BoxCollider2D _laserRight;
	private SpriteRenderer _splash;
	private AIInteriorClimb _ai;
	private Health _health;
	private bool _started = false;
	private bool _wasHurt = false;

	private bool firing = false;
	private bool _laserFiring = false;

	private int fireCount = 0;

	private CameraController sceneCamera;
	private bool _onWall = false;
	private bool _firingCannon = false;
	private bool _firingLasers = false;
	private Coroutine fireRoutine;
	private bool wasDead = false;

    private bool alreadyThawed = false;

	private enum StageEnum{LeftDescend, LeftFire, LeftAscend, RightDescend, RightFire, RightAscend, OnCeiling};
	private StageEnum _state;
	protected Vector2 _direction;

	Vector3 orgPosition;
	Vector3 orgScale;
	Quaternion orgRotation;

	// Use this for initialization
	void Start ()
	{
        if (CheckDefeated())
            return;

        _animator = GetComponentsInChildren<Animator> ()[0];
		_clawAnimator = GetComponentsInChildren<Animator> ()[1];
		_sprite = GetComponent<SpriteRenderer> ();
		_laser = GetComponentsInChildren<SpriteRenderer> ()[2];
		_splash = GetComponentsInChildren<SpriteRenderer> ()[3];
		_ai = GetComponent<AIInteriorClimb> ();
		_health = GetComponent<Health> ();

		_laserLeft = GetComponentsInChildren<BoxCollider2D> ()[1];
		_laserRight = GetComponentsInChildren<BoxCollider2D> ()[2];

		sceneCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraController> ();

		_laser.enabled = false;
		_laserLeft.enabled = false;
		_laserRight.enabled = false;

		_state = StageEnum.LeftDescend;
		_splash.enabled = false;

		_direction = Vector2.zero;

		orgPosition = transform.position;
		orgScale = transform.localScale;
		orgRotation = transform.rotation;

        SetStagePosition ();
    }

	void SetStagePosition()
	{
		_health.MinDamageThreshold = 1;

		if (_state == StageEnum.LeftDescend) {
			transform.position = orgPosition;
			transform.localScale = orgScale;
			transform.rotation = orgRotation;
		} 
		else if (_state == StageEnum.RightDescend) {
			transform.position = new Vector3 (transform.position.x - 16, transform.position.y - 4, transform.position.z);
			transform.localScale = new Vector3 (transform.localScale.x, -transform.localScale.y, transform.localScale.z);
		} 
		else if (_state == StageEnum.OnCeiling) {
			transform.position = new Vector3 (6, -20, transform.position.z);
			transform.localScale = new Vector3 (1, 1, 1);

			if(transform.rotation.eulerAngles.z == 90)
				transform.Rotate (new Vector3 (0, 0, 90));
			else
				transform.Rotate (new Vector3 (0, 0, 180));

			StartCoroutine (TurnAround (3f));
		}
	}

	// Close walls above
	void ChangeWallCheck()
	{
		if (_state == StageEnum.LeftDescend || _state == StageEnum.RightDescend) {
			ChangeWalls (true, Vector3.up, Vector3.left, Color.red);
            ChangeWalls(false, Vector3.down, Vector3.left, Color.red);
        } 
		else if (_state == StageEnum.LeftAscend || _state == StageEnum.RightAscend) {
			ChangeWalls (false, Vector3.up, Vector3.left, Color.green);
			ChangeWalls (true, Vector3.down, Vector3.left, Color.green);
		} 
		else if (_state == StageEnum.OnCeiling) {
			ChangeWalls (false, Vector3.left, Vector3.up, Color.blue);
			ChangeWalls (false, Vector3.right, Vector3.up, Color.blue);
		}
	}

	protected virtual void FixedUpdate()
	{
		Vector3 _newPosition = Speed * _direction * Time.deltaTime;
		transform.Translate(_newPosition, Space.World);
	}

	void ChangeWalls(bool activate, Vector3 dir, Vector3 perp, Color color)
	{
		var mask = (1 << LayerMask.NameToLayer ("Foreground")) | (1 << LayerMask.NameToLayer ("Platforms"));

		RaycastHit2D[] platforms1 = CorgiTools.CorgiRaycastAll (transform.position + 2*dir - perp, dir, 8f, mask, true, color);
		RaycastHit2D[] platforms2 = CorgiTools.CorgiRaycastAll (transform.position + 2*dir + perp, dir, 8f, mask, true, color);

		for (int i = 0; i < platforms1.Length; i++) 
		{
			var plat = platforms1 [i];

			if (plat.distance > 0) 
			{
				BossWall wallL = plat.collider.GetComponent<BossWall> ();

				if (wallL) 
				{
					wallL.Order = 0.25f;

					if (activate)
						wallL.Activate ();
					else
						wallL.Deactivate ();
				} 
			}
		}

		for (int i = 0; i < platforms2.Length; i++) 
		{
			var plat = platforms2 [i];

			if (plat.distance > 0) 
			{
				BossWall wallR = plat.collider.GetComponent<BossWall> ();

				if (wallR) 
				{
					wallR.Order = 0.5f;

					if (activate)
						wallR.Activate ();
					else
						wallR.Deactivate ();
				}
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{           
		bool reacting = _animator.GetBool ("Reacting");

		if (reacting && !_started) 
		{
			_started = true;

			CloseWalls ();
			SoundManager.Instance.PlayBossMusic ();

			sceneCamera.FreezeAt (new Vector3(-14, -24, 10));
			return;
		}

		if (_started && !wasDead) 
		{
			if (_health.CurrentHealth <= 0) {
				_sprite.color = Color.red;
				_health.StartCoroutine(_health.Flicker());
			}

			// Turn off
			bool dead = _animator.GetBool("Dead");
			if (dead && !wasDead) 
			{
				wasDead = true;

				if( _loopSound != null)
					_loopSound.Stop ();

                if (_laserFX != null)
                    _laserFX.Stop();

                if (StopFx != null)
					SoundManager.Instance.PlaySound(StopFx, transform.position);

				SoundManager.Instance.PlayRegularMusic ();

				OpenWalls ();

				return;
			}

			ChangeWallCheck ();
			CheckForWalls ();

			if (_state == StageEnum.LeftFire || _state == StageEnum.RightFire) 
			{
                if (!_firingCannon)
					StartCoroutine (FireCannon (0.25f));
            } 
			else if (_state == StageEnum.LeftDescend) 
			{
				if (!_onWall)
					_direction = Vector3.down;
				else 
				{
					if( _loopSound != null)
						_loopSound.Stop ();

					if (StopFx != null)
						SoundManager.Instance.PlaySound (StopFx, transform.position);

					_health.MinDamageThreshold = 1;
					_direction = Vector3.zero;
					_firingCannon = false;
					_state = StageEnum.LeftFire;
				}
			} 
			else if (_state == StageEnum.RightDescend) 
			{
				if (!_onWall)
					_direction = Vector3.down;
				else 
				{
					if( _loopSound != null)
						_loopSound.Stop ();

					if (StopFx != null)
						SoundManager.Instance.PlaySound (StopFx, transform.position);

					_health.MinDamageThreshold = 1;
					_direction = Vector3.zero;
					_firingCannon = false;
					_state = StageEnum.RightFire;
				}
			} 
			else if (_state == StageEnum.LeftAscend)
			{
				_health.MinDamageThreshold = 100;

				if (!_onWall)
				{
					_direction = Vector3.up;

					if (_wasHurt)
					{
						CorgiTools.UpdateAnimatorBool(_animator, "Hurt", false);
						CorgiTools.UpdateAnimatorBool(_animator, "Aiming", false);
						_wasHurt = false;
					}
				}
				else
				{
					_state = StageEnum.RightDescend;

					_direction = Vector3.down;

					SetStagePosition();
				}
			} 
			else if (_state == StageEnum.RightAscend)
			{
				_health.MinDamageThreshold = 100;

				if (!_onWall)
				{
					_direction = Vector3.up;

					if (_wasHurt)
					{
						CorgiTools.UpdateAnimatorBool(_animator, "Hurt", false);
						CorgiTools.UpdateAnimatorBool(_animator, "Aiming", false);
						_wasHurt = false;
					}
				}
				else
				{
					_state = StageEnum.OnCeiling;
					_direction = Vector3.zero;

					SetStagePosition();
				}
			} 
			else if (_state == StageEnum.OnCeiling)
			{
				_health.MinDamageThreshold = 1;

				if (_onWall)
                {
                    _direction = -_direction;
                    _health.MinDamageThreshold = 1;
                }
                else
                {
                    float rx = Mathf.Round(transform.position.x);
                    // Weapon on between -21 and -7, off otherwise
                    if (rx == -21 && _direction.x > 0 ||
                        rx == -7 && _direction.x < 0)
                    {
                        if (!_laserFiring)
                            StartCoroutine(PrepLasers(1.0f));
                    }
                    else if (rx < -21 || rx > -7)
                    {
                        CorgiTools.UpdateAnimatorBool(_clawAnimator, "Shooting", false);

                        if (_laserFiring)
                            StartCoroutine(StopLasers(0.01f));
                    }
                }
			}

			bool hurt = _animator.GetBool ("Hurt");
			if (hurt && !_wasHurt) 
			{
				_wasHurt = true;
				StartCoroutine(StopShoot(0f));
			}
		}
	}

	protected virtual void CheckForWalls()
	{
		var mask = 1 << LayerMask.NameToLayer ("Platforms");
		RaycastHit2D platformUp = CorgiTools.CorgiRayCast (transform.position, Vector3.up, 2f, mask, true, Color.yellow);
		RaycastHit2D platformDown = CorgiTools.CorgiRayCast (transform.position, Vector3.down, 2f, mask, true, Color.yellow);

		RaycastHit2D platformLeft = CorgiTools.CorgiRayCast (transform.position, Vector3.left, 3f, mask, true, Color.yellow);
		RaycastHit2D platformRight = CorgiTools.CorgiRayCast (transform.position, Vector3.right, 3f, mask, true, Color.yellow);
			
		_onWall = false;
		if (_state == StageEnum.RightAscend && platformUp) {
			_onWall = true;
		} 
		else if (_state == StageEnum.RightDescend && platformDown) {
			_onWall = true;
		} 
		else if (_state == StageEnum.LeftAscend && platformUp) {
			_onWall = true;
		} 
		else if (_state == StageEnum.LeftDescend && platformDown) {
			_onWall = true;
		}
		else if (_state == StageEnum.OnCeiling && platformLeft && _direction.x < 0) {
			_onWall = true;
		}
		else if (_state == StageEnum.OnCeiling && platformRight && _direction.x > 0) {
			_onWall = true;
		}
	}

	public virtual IEnumerator TurnAround(float delay)
	{
		yield return new WaitForSeconds (delay);

		if (_direction == Vector2.zero)
			_direction = Vector2.right;

		_direction = -_direction;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	public virtual IEnumerator PrepLasers(float duration)
	{
		_laserFiring = true;
		Vector2 oldDir = _direction;
		_direction = Vector2.zero;

		yield return new WaitForSeconds (duration);

		CorgiTools.UpdateAnimatorBool (_clawAnimator, "Shooting", true);

		StartCoroutine (FireLasers (0.417f, oldDir));
	}

	public virtual IEnumerator FireLasers(float duration, Vector2 dir)
	{
		yield return new WaitForSeconds (duration);

		_direction = dir;

		if (LaserFx != null)
			_laserFX = SoundManager.Instance.PlaySound(LaserFx,transform.position,true);

		_laser.enabled = true;
		_laserLeft.enabled = true;
		_laserRight.enabled = true;

		_splash.enabled = true;
	}

	public virtual IEnumerator StopLasers(float delay)
	{
		_laserFiring = false;

		yield return new WaitForSeconds (delay);

		_laserFX.Stop ();

        _laser.enabled = false;
		_laserLeft.enabled = false;
		_laserRight.enabled = false;

		_splash.enabled = false;
	}

	public virtual IEnumerator FireCannon(float duration)
	{
		_firingCannon = true;

        yield return new WaitForSeconds (duration);

        // Stop
        //		_ai.enabled = false;

        // temp
        if (_state == StageEnum.LeftFire && !alreadyThawed)
        {
            alreadyThawed = true;
            StartCoroutine(ThawCharacter());

            // Fire!
            firing = true;
            StartCoroutine(Aim(1.5f));
            fireRoutine = StartCoroutine(Shoot(1.8f));
        }
        else if(_state == StageEnum.RightFire)
        {
            // Fire!
            firing = true;
            StartCoroutine(Aim(0));
            fireRoutine = StartCoroutine(Shoot(0.3f));
        }
	}

	public virtual IEnumerator StopShoot(float delay)
	{
		yield return new WaitForSeconds (delay);

		firing = false;
		CorgiTools.UpdateAnimatorBool (_animator, "Firing", false);
		CorgiTools.UpdateAnimatorBool (_animator, "Aiming", false);
		StopCoroutine (fireRoutine);
		StartCoroutine (ChangeState (2f));
	}

	public virtual IEnumerator ChangeState(float delay)
	{
		yield return new WaitForSeconds (delay);

		_wasHurt = false;
		_health.MinDamageThreshold = 100;

		if (_state == StageEnum.LeftFire) 
		{
			if (StartFx != null)
				SoundManager.Instance.PlaySound (StartFx, transform.position);

			StartCoroutine (StartLoopSound (1.25f));

			_state = StageEnum.LeftAscend;
			StartCoroutine(TurnAround (1));
		}
		else if (_state == StageEnum.RightFire) 
		{
			if (StartFx != null)
				SoundManager.Instance.PlaySound (StartFx, transform.position);

			StartCoroutine (StartLoopSound (1.25f));

			_state = StageEnum.RightAscend;
			StartCoroutine(TurnAround (1));
		}
	}

	public virtual IEnumerator Aim(float delay)
	{
		yield return new WaitForSeconds (delay);

		if (AimFx != null)
			SoundManager.Instance.PlaySound (AimFx, transform.position);	

		CorgiTools.UpdateAnimatorBool (_animator, "Aiming", true);
	}

	public virtual IEnumerator Shoot(float delay)
	{
		yield return new WaitForSeconds (delay);

		// we play the shooting sound
		if (ShootFx != null)
			SoundManager.Instance.PlaySound (ShootFx, transform.position);	

		Vector2 firePos = transform.position;

		Projectile projectile = (Projectile)Instantiate (Projectile, firePos, transform.rotation);

		if (_state == StageEnum.LeftFire)
			projectile.Initialize (gameObject, Vector2.left, 10 * Vector2.left, false);
		else
			projectile.Initialize (gameObject, Vector2.right, 10 * Vector2.right, false);

		if (firing && fireCount < 2) 
		{
			fireCount++;
			CorgiTools.UpdateAnimatorBool (_animator, "Aiming", false);
			CorgiTools.UpdateAnimatorBool (_animator, "Firing", true);
			fireRoutine = StartCoroutine (Shoot (CannonFireRate));
		} 
		else 
		{
			fireCount = 0;
			CorgiTools.UpdateAnimatorBool (_animator, "Firing", false);
			StartCoroutine (Aim (1.7f));
			fireRoutine = StartCoroutine (Shoot (2f));
		}
	}

	public virtual IEnumerator StartLoopSound(float duration)
	{
		yield return new WaitForSeconds (duration);

		if (MoveFx != null) {
			if (_loopSound != null)
				_loopSound.Stop ();
			_loopSound = SoundManager.Instance.PlaySound (MoveFx, transform.position, true);
		}
	}


    public virtual IEnumerator ThawCharacter()
    {
        yield return new WaitForSeconds(0.1f);

        SuckUpGems();
        StartCoroutine(GUIManager.Instance.SlideBarsOut(0.75f));

        yield return new WaitForSeconds(1f);

        GameManager.Instance.ThawCharacter();
    }
}

