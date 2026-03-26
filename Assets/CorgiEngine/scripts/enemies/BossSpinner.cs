using UnityEngine;
using System.Collections;

public class BossSpinner : Boss
{
	public float JumpForce = 8f;
	public AudioClip LandSfx;

	public AudioClip BootSfx;
	public AudioClip ActiveSfx;
	public AudioClip StartSfx;
	public AudioClip StopSfx;
    public AudioClip SpinSfx;

    protected Animator _animator;
	protected SpriteRenderer _sprite;
	protected AISimpleWalk _walk;
	protected Health _health;
	protected EnemyController _controller;

	private int hurtStage = 0;
	private bool wasHurt = false;
	private bool awake = false;
    private bool wasPoweringUp = false;
	private bool wasGrounded = false;
	private bool wasReacting = false;
	private bool wasDying = false;
	private bool wasDead = false;
	private AudioSource _loopSound;


	// Use this for initialization
	void Start ()
	{
        if (CheckDefeated())
            return;

        _animator = GetComponent<Animator> ();
		_walk = GetComponent<AISimpleWalk> ();
		_health = GetComponent<Health> ();
		_sprite = GetComponent<SpriteRenderer> ();
		_controller = GetComponent<EnemyController> ();

		_health.MinDamageThreshold = 100;

		_walk.Disable();
	}


    // Update is called once per frame
    void Update()
    {
        // Turn on
        bool reacting = _animator.GetBool("Reacting");

        if (reacting && !awake)
        {
            awake = true;

            CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

            if (sceneCamera != null)
                sceneCamera.FreezeAt(transform.position);

            SoundManager.Instance.PlayBossMusic();

            CloseWalls();

            StartCoroutine(Powerup(2f));
            StartCoroutine(GUIManager.Instance.SlideBarsOut(6f));
        }

        wasReacting = reacting;

        // Hurt
        bool hurt = _animator.GetBool("Hurt");

        if (hurt && !wasHurt)
        {
            hurtStage++;
            _animator.SetInteger("HurtStage", hurtStage);

            if (_loopSound != null)
                _loopSound.Stop();

            if (StopSfx != null)
                SoundManager.Instance.PlaySound(StopSfx, transform.position);

            _health.MinDamageThreshold = 100;
            StartCoroutine(Activate(1.9f));

            if (hurtStage == 1)
                _walk.Speed += 8;
            else if (hurtStage == 2)
            {
                _walk.Speed -= 4;
                StartCoroutine(Jump(1.5f));
            }
            else if (hurtStage == 3)
            {
                _walk.Speed += 4;
            }
        }

        wasHurt = hurt;

        // Ground Slam
        if (_controller.State.IsGrounded && !wasGrounded)
        {
            if (LandSfx != null)
                SoundManager.Instance.PlaySound(LandSfx, transform.position);

            Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
            CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

            if (sceneCamera != null)
                sceneCamera.Shake(ShakeParameters);

            if (hurtStage >= 2)
                StartCoroutine(Jump(0.1f));
        }

        wasGrounded = _controller.State.IsGrounded;

        bool dying = _animator.GetBool("Dying");

        if (dying && !wasDying)
        {
            StartCoroutine(PlayHurt(1f));
        }

        wasDying = dying;

        // Turn off
        bool dead = _animator.GetBool("Dead");

        if (dead && !wasDead)
        {
            if (_loopSound != null)
                _loopSound.Stop();

            if (StopSfx != null)
                SoundManager.Instance.PlaySound(StopSfx, transform.position);

            SoundManager.Instance.PlayRegularMusic();

            OpenWalls();
        }

        wasDead = dead;
    }


    public virtual IEnumerator Powerup(float duration)
    {
        yield return new WaitForSeconds(duration);

        _animator.SetBool("PoweringUp", true);

        Say(0, (int)duration + 1);

        yield return new WaitForSeconds(duration);

        //SuckUpGems();

        StartCoroutine(ComeOnline(2.25f));
    }


    public virtual IEnumerator ComeOnline(float duration)
	{
        yield return new WaitForSeconds(duration);

        CorgiTools.UpdateAnimatorBool (_animator, "Active", true);

		if (BootSfx != null)
			SoundManager.Instance.PlaySound(BootSfx, transform.position);

		_sprite.color = Color.red;
		_health.StartCoroutine(_health.Flicker());

        StartCoroutine (Blades (0.75f));

		StartCoroutine (Activate (3f));
	}

	public virtual IEnumerator Blades(float duration)
	{
		yield return new WaitForSeconds (duration);

		if (ActiveSfx != null)
			SoundManager.Instance.PlaySound(ActiveSfx, transform.position);
	}

	public virtual IEnumerator Jump(float duration)
	{
		yield return new WaitForSeconds (duration);

        if (_controller.State.IsCollidingBelow)
        {
            _controller.SnapToFloor = false;
            _controller.SetVerticalForce(JumpForce);

            yield return new WaitForSeconds(0.25f);

            _controller.SnapToFloor = true;
        }
	}

	public virtual IEnumerator PlayHurt(float duration)
	{
		//Debug.Log ("Playing really hurt");
		yield return new WaitForSeconds (duration);
		_health.PlayReallyHurt ();

		yield return new WaitForSeconds (duration);
		_health.PlayReallyHurt ();

		yield return new WaitForSeconds (duration);
		_health.PlayReallyHurt ();
	}

	public virtual IEnumerator Activate(float duration)
	{
		yield return new WaitForSeconds (duration);

		_health.MinDamageThreshold = 0;
		_walk.Walk ();

		if (StartSfx != null)
			SoundManager.Instance.PlaySound (StartSfx, transform.position);

		StartCoroutine (StartLoopSound (1.25f));
		GameManager.Instance.ThawCharacter();
	}

	public virtual IEnumerator StartLoopSound(float duration)
	{
		yield return new WaitForSeconds (duration);

		if (SpinSfx != null) {
			if (_loopSound != null)
				_loopSound.Stop ();
			_loopSound = SoundManager.Instance.PlaySound (SpinSfx, transform.position, true);
		}
	}
}

