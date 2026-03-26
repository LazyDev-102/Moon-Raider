using UnityEngine;
using System.Collections;
using JulienFoucher;

public class BossGuard : Boss
{
	public AudioClip LandSfx;
	public AudioClip TauntSfx;
	public AudioClip AttackSfx;
    public AudioClip ChargeAttackSfx;
    public AudioClip ChargeErrorSfx;

    public GameObject LandEffect;
    public float ChargeSpeed = 12;

    private AISimpleWalk _walk;
	private Animator _animator;
	private Rigidbody2D _rigid;
	private EnemyController _controller;
	private BoxCollider2D _spearCollider;
    private BoxCollider2D _spearUpCollider;
    protected Health _health;

	private bool _started = false;
	private bool _wasStarted = false;
	private bool _wasHurt = false;
	private bool _wasDead = false;

    private bool _canChangeDirection = true;

	private CameraController sceneCamera;
    private Vector3 oldOffset;

    private enum StageEnum { NotStarted, Phase1, Phase2, Phase3 };
    StageEnum phase = StageEnum.Phase1;
    StageEnum oldPhase = StageEnum.NotStarted;
    int hurtCounter = 0;
    int wallCounter = 0;

    AISayThings sayThings;
    bool waitForPhase2 = false;
    Coroutine popupRoutine;
    bool CanCount = true;


    // Use this for initialization
    void Start ()
	{
        if (CheckDefeated())
            return;

        _animator = GetComponent<Animator> ();
		_walk = GetComponent<AISimpleWalk> ();
		_rigid = GetComponent<Rigidbody2D> ();
		_controller = GetComponent<EnemyController> ();
		_spearUpCollider = GetComponents<BoxCollider2D> () [1];
        _spearCollider = GetComponents<BoxCollider2D>()[2];
        _health = GetComponent<Health> ();
        sayThings = GetComponent<AISayThings>();

        sceneCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraController> ();

		_walk.Disable();

		_health.MinDamageThreshold = 100;

		// Create spikes
		var spikePrefab = Resources.Load ("Obstacles/PopupSpikes") as GameObject;

		for (int i = 0; i < 12; i++) {
			float left = -15 + 2 * i;
			GameObject spike = Instantiate (spikePrefab, new Vector3 (left, -16.5f, 11), Quaternion.identity);
			spike.transform.parent = transform.parent;
		}
    }


	void DropIn()
	{
        GameManager.Instance.FreezeCharacter();

        CloseWalls ();

		SoundManager.Instance.PlayBossMusic();

		StartCoroutine (Drop (1.25f));
	}

	public virtual IEnumerator Drop(float duration)
	{
		yield return new WaitForSeconds (duration);

        oldOffset = sceneCamera.CameraOffset;
        sceneCamera.CameraOffset = 4 * Vector3.down;
        sceneCamera.SetTarget(transform);

		_controller.Parameters.Gravity = -80;
	}

	void Landed()
	{
		sceneCamera.FreezeAt (transform.position);

        if (LandSfx != null)
			SoundManager.Instance.PlaySound(LandSfx, transform.position);

        GetComponent<SpriteTrail>().enabled = false;

		Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
		sceneCamera.Shake(ShakeParameters);

        StartCoroutine(TalkSmack(0));

        StartCoroutine(PopupSpikes(0.001f));
		StartCoroutine (Taunt(0.25f));
		StartCoroutine (FreezeMiddle(2.0f));
		StartCoroutine (Attack(3.5f));

        StartCoroutine(GUIManager.Instance.SlideBarsOut(2f));
    }

	public virtual IEnumerator Taunt(float duration)
	{
		yield return new WaitForSeconds (duration);

        sceneCamera.CameraOffset = oldOffset;

        GameManager.Instance.ThawCharacter ();

		if (TauntSfx != null)
			SoundManager.Instance.PlaySound(TauntSfx, transform.position);
	}

	public virtual IEnumerator FreezeMiddle(float duration)
	{
		yield return new WaitForSeconds (duration);

		sceneCamera.FreezeAt (new Vector3 (-4, -13, 0));
	}

	public virtual IEnumerator Attack(float duration)
	{
		yield return new WaitForSeconds (duration);

        if (!_animator.GetBool("Charging"))
        {
            if (AttackSfx != null)
                SoundManager.Instance.PlaySound(AttackSfx, transform.position);

            _spearUpCollider.enabled = false;

            _animator.SetBool("PlayerOverhead", false);
            _animator.SetBool("Pause", false);
            _animator.SetBool("Attack", true);

            _health.MinDamageThreshold = 1;

            _walk.TurnAtWalls = true;
            _walk.Walk();
        }
	}

	public virtual IEnumerator OverheadAttack(float duration)
	{
		yield return new WaitForSeconds (duration);
		_spearUpCollider.enabled = true;
	}


    public virtual IEnumerator EnableChangeDirection(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!_animator.GetBool("Taunting"))
            _canChangeDirection = true;
    }


	// Update is called once per frame
	void FixedUpdate ()
	{
        if (_wasDead)
            return;

		bool reacting = _animator.GetBool ("Reacting");
		if (reacting && !_started) {
			_started = true;
			DropIn ();
			return;
		}

		_animator.SetBool ("Grounded", _controller.State.IsGrounded);

		// Land
		if (_started && !_wasStarted && _controller.State.IsGrounded) {
			_wasStarted = _started;
			Landed ();
			return;
		}

        // Check for player and swipe!
        if (_started && _wasStarted && _controller.State.IsGrounded)
        {
            bool dying = _animator.GetBool("Dying");

            if (dying && !_wasDead)
            {
                _wasDead = true;

                _walk.enabled = false;
                GetComponent<AIReact>().enabled = false;
                //GetComponent<BossGuard>().enabled = false;

                StartCoroutine(Dead(1f));
            }
            else
            {
                _spearCollider.enabled = _animator.GetBool("Attack");

                if (phase == StageEnum.Phase1)
                    Phase1();
                else if (phase == StageEnum.Phase2)
                    Phase2();
                else if (phase == StageEnum.Phase3)
                    Phase3();
            }			
		}
	}


    void Phase1()
    {
        if (waitForPhase2 && transform.position.x < -8)
        {
            StopCoroutine(popupRoutine);

            oldPhase = StageEnum.Phase1;
            phase = StageEnum.Phase2;
            waitForPhase2 = false;

            if (_canChangeDirection)
            {
                if (transform.localScale.x == -1 && transform.position.x < GameManager.Instance.Player.transform.position.x ||
                    transform.localScale.x == 1 && transform.position.x > GameManager.Instance.Player.transform.position.x)
                {
                    _walk.ChangeDirection();
                    //transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
                }
            }

            _walk.enabled = false;
            _animator.SetBool("Hurt", true);
            _controller.SetHorizontalForce(0);

            return;
        }

        if (_canChangeDirection)
        {
            var mask = 1 << LayerMask.NameToLayer("Player");
            RaycastHit2D player = CorgiTools.CorgiRayCast(transform.position, Vector3.up, 2f, mask, true, Color.yellow);

            if (player && _animator.GetBool("PlayerOverhead") == false)
            {
                _animator.SetBool("PlayerOverhead", true);
                _animator.SetBool("Attack", false);

                _walk.Disable();

                _canChangeDirection = false;
                StartCoroutine(EnableChangeDirection(0.5f));
                StartCoroutine(OverheadAttack(0.333f));
                StartCoroutine(TurnAround(0.667f));
                StartCoroutine(Attack(0.667f));
            }
            else
            {
                // look higher and turn around if you see him
                var himask = 1 << LayerMask.NameToLayer("Player");
                RaycastHit2D hiplayer = CorgiTools.CorgiRayCast(transform.position, Vector3.up, 8f, himask, true, Color.yellow);

                if (hiplayer && _animator.GetBool("Pause") == false)
                {
                    _animator.SetBool("Pause", true);
                    _animator.SetBool("Attack", false);

                    _walk.Disable();

                    _canChangeDirection = false;
                    StartCoroutine(EnableChangeDirection(0.5f));
                    StartCoroutine(TurnAround(0.25f));
                    StartCoroutine(Attack(0.25f));
                }
            }
        }

        bool hurt = _animator.GetBool("Hurt");
        bool dying = _animator.GetBool("Dying");

        if (hurt && !_wasHurt && !dying && hurtCounter < 3)
        {
            _health.MinDamageThreshold = 100;

            _wasHurt = true;
            _walk.Speed += 1;

            hurtCounter++;

            _walk.enabled = false;
            _controller.SetHorizontalForce(0);
            popupRoutine = StartCoroutine(PopupSpikes(1.5f));

            if (hurtCounter >= 3)
                waitForPhase2 = true;
        }
    }

    void Phase2()
    {
        if(oldPhase == StageEnum.Phase1)
        {
            oldPhase = StageEnum.Phase2;

            StartCoroutine(GUIManager.Instance.SlideBarsIn());

            _animator.SetBool("Attack", false);
            _animator.SetBool("Taunting", true);
            _canChangeDirection = false;

            GameManager.Instance.FreezeCharacter();
            _health.MinDamageThreshold = 100;

            if (transform.localScale.x == 1)
                _walk.ChangeDirection();

            StartCoroutine(TalkSmack(3));
            StartCoroutine(ChargeUp(9));
        }
    }


    void Phase3()
    {
        if (oldPhase == StageEnum.Phase2)
        {
            oldPhase = StageEnum.Phase3;

            GetComponent<SpriteTrail>().enabled = true;
            GetComponent<GiveDamageToPlayer>().DamageToGive = 4;

            GameManager.Instance.ThawCharacter();

            StartCoroutine(ChargeAttack());
        }

        // Stop dizzy
        if (_animator.GetBool("Vulnerable") == false && CanCount)
        {
            if (_walk.IsColliding)
            {
                wallCounter++;
                CanCount = false;
                StartCoroutine(EnableCanCount());

                if (wallCounter % 3 == 0)
                {
                    if (ChargeErrorSfx != null)
                        SoundManager.Instance.PlaySound(ChargeErrorSfx, transform.position);

                    //_walk.ChangeDirection();
                    _walk.Disable();
                    _animator.SetBool("Vulnerable", true);
                    _health.MinDamageThreshold = 1;
                    wallCounter = 0;

                    StartCoroutine(Invulernable(3));
                }
            }
        }
    }

    public virtual IEnumerator EnableCanCount()
    {
        yield return new WaitForSeconds(0.5f);

        CanCount = true;
    }

    public virtual IEnumerator ChargeAttack()
    {
        yield return new WaitForSeconds(2f);

        if (ChargeAttackSfx != null)
            SoundManager.Instance.PlaySound(ChargeAttackSfx, transform.position);

        _walk.Speed = ChargeSpeed;
        _walk.SetOrgSpeed(ChargeSpeed);
        _walk.enabled = true;
        _health.MinDamageThreshold = 100;

        _animator.SetBool("Attack", true);
    }

    public virtual IEnumerator Invulernable(float duration)
    {
        yield return new WaitForSeconds(duration);

        _walk.Speed = ChargeSpeed;
        _walk.SetOrgSpeed(ChargeSpeed);
        _walk.Walk();
        _health.MinDamageThreshold = 100;
        _animator.SetBool("Vulnerable", false);

        if (ChargeAttackSfx != null)
            SoundManager.Instance.PlaySound(ChargeAttackSfx, transform.position);
    }


    public virtual IEnumerator ChargeUp(float duration)
    {
        yield return new WaitForSeconds(duration);

        SuckUpGems();

        yield return new WaitForSeconds(1);

        _animator.SetBool("Charging", true);
        StartCoroutine(GUIManager.Instance.SlideBarsOut(1f));

        yield return new WaitForSeconds(1);

        GetComponent<SpriteRenderer>().flipX = false;
        phase = StageEnum.Phase3;
        _health.MinDamageThreshold = 1;
    }



    public virtual IEnumerator TurnAround(float duration)
	{
		yield return new WaitForSeconds (duration);

		_walk.ChangeDirection ();
	}

	public virtual IEnumerator Dead(float duration)
	{
		yield return new WaitForSeconds (duration);

		Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
		sceneCamera.Shake(ShakeParameters);

		OpenWalls ();
        sceneCamera.SetTarget(GameManager.Instance.Player.transform);

		SoundManager.Instance.PlayRegularMusic ();
	}

	public virtual IEnumerator PopupSpikes(float duration)
	{
		yield return new WaitForSeconds (duration);

        StartCoroutine(ResumeWalk(1f));

        Instantiate(LandEffect, transform.position + 0.5f * Vector3.up, transform.rotation);

        if (LandSfx != null)
			SoundManager.Instance.PlaySound(LandSfx, transform.position);

		Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
		sceneCamera.Shake(ShakeParameters);

		var maskLayer = 1 << LayerMask.NameToLayer ("Enemies");

		RaycastHit2D[] circles = Physics2D.CircleCastAll (transform.localPosition + 2 * Vector3.down, 24.0f, Vector2.right, 0.0f, maskLayer);

//		Debug.Log ("Spikes: " + circles.Length);

		for(int i = 0; i < circles.Length; i++)
		{
			var circle = circles [i];

			var wall = circle.collider.gameObject.GetComponent<PopupSpike> ();

			if (wall != null) {
				float time = Mathf.Abs(wall.transform.position.x - transform.position.x) / 32;
				wall.Cap = 5*time*time + 0.75f;
				wall.StartCoroutine (wall.Popup (time));
			}
		}

		_wasHurt = false;
		_health.MinDamageThreshold = 1;
	}


    public virtual IEnumerator ResumeWalk(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (phase == StageEnum.Phase3)
            _walk.Speed = ChargeSpeed;
        _walk.enabled = true;
    }

    public IEnumerator TalkSmack(float duration)
    {
        yield return new WaitForSeconds(0.1f);

        if (phase == StageEnum.Phase1)
        {
            sayThings.SaySomething(0, 3);
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = (transform.position.x > GameManager.Instance.Player.transform.position.x);

            sayThings.SaySomething(1, 3);

            yield return new WaitForSeconds(duration);

            sayThings.SaySomething(2, 3);

            yield return new WaitForSeconds(duration);

            sayThings.SaySomething(3, 3);

            if (TauntSfx != null)
                SoundManager.Instance.PlaySound(TauntSfx, transform.position);
        }
    }
}

