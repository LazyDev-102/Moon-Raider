using UnityEngine;
using System.Collections;

public class Laser : Switchable
{
	public bool OnTimer = true;
	public float FireDuration = 2.0f;
	public float FireRate = 4.0f;
	public Animator FadeEffect;

    public LayerMask WallMask;
    public bool AlwaysUpdate = false;

    public AudioClip SoundEffect;
	public AudioClip ChargeSoundEffect;
	public AudioClip OffSoundEffect;

	private Animator _animator;
	private bool _active = false;
	private float _maxLaserRange = 80.0f;

	private Animator _beam;
	private Animator _splash;
	private Animator _smoke;

	private SpriteRenderer _beamSprite;
	private SpriteRenderer _splashSprite;
	private SpriteRenderer _smokeSprite;

	private BoxCollider2D _damage;
	private bool _on = false;
	private bool _hasSwitch = false;
    private float _pitch = 0;
    private bool _deactivated = false;

    private float laserScale = 1f;
    bool _canUpdate = false;


    // Use this for initialization
    public virtual void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        _animator = GetComponentsInChildren<Animator>()[0];
        _beam = GetComponentsInChildren<Animator>()[1];
        _splash = GetComponentsInChildren<Animator>()[2];
        _smoke = GetComponentsInChildren<Animator>()[3];

        _beamSprite = _beam.GetComponent<SpriteRenderer>();
        _splashSprite = _splash.GetComponent<SpriteRenderer>();
        _smokeSprite = _smoke.GetComponent<SpriteRenderer>();

        _damage = _beam.GetComponent<BoxCollider2D>();

        _beamSprite.enabled = false;
        _splashSprite.enabled = false;
        _smokeSprite.enabled = false;
        _damage.enabled = false;

        _hasSwitch = (GetComponent<AIReact>() == null);

        if (!_hasSwitch)
            laserScale = 0.5f;

        var pc = GetComponent<PitchChanger>();
        if (pc != null)
            _pitch = pc.Pitch;

    }

    void Start()
    {
        StartCoroutine(Init(0.025f));

		if (_animator.GetBool ("Reacting") || _hasSwitch) {
			StartCoroutine (Fire (0.5f));
		}
	}

    public virtual IEnumerator Init(float duration)
    {
        yield return new WaitForSeconds(duration);

        UpdateLaser();
    }


    void UpdateLaser()
    {
        Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y);

        float rotation = transform.rotation.eulerAngles.z;

        if (rotation == 0f)
        {
            RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, Vector2.down, _maxLaserRange, WallMask, true, Color.red);

            if (raycast)
            {
                float distance = transform.position.y - raycast.point.y;
                _beam.transform.localScale = new Vector3(laserScale, laserScale*distance, laserScale);
                _beam.transform.position = new Vector3(_beam.transform.position.x, _animator.transform.position.y - distance / 2, _beam.transform.position.z);
                _splash.transform.position = new Vector3(_beam.transform.position.x, _beam.transform.position.y - distance / 2 + 0.5f, _beam.transform.position.z);
                _smoke.transform.position = new Vector3(_beam.transform.position.x, _beam.transform.position.y - distance / 2 + 1.0f, _beam.transform.position.z);
            }
        }
        else if (rotation == 90f)
        {
            RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, Vector2.right, _maxLaserRange, WallMask, true, Color.red);

            if (raycast)
            {
                float distance = transform.position.x - raycast.point.x;
                _beam.transform.localScale = new Vector3(laserScale, laserScale * distance, laserScale);
                _beam.transform.position = new Vector3(_animator.transform.position.x - distance / 2, _beam.transform.position.y, _beam.transform.position.z);
                _splash.transform.position = new Vector3(_beam.transform.position.x - distance / 2 - 0.5f, _beam.transform.position.y, _beam.transform.position.z);
                _smoke.transform.position = new Vector3(_beam.transform.position.x - distance / 2, _beam.transform.position.y + 1.0f, _beam.transform.position.z);
                _smokeSprite.transform.localRotation = Quaternion.Euler(0, 0, 270f);
            }
        }
        else if (rotation == 180f)
        {
            RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, Vector2.up, _maxLaserRange, WallMask, true, Color.red);

            if (raycast)
            {
                float distance = raycast.point.y - transform.position.y;
                _beam.transform.localScale = new Vector3(laserScale, laserScale * distance, laserScale);
                _beam.transform.position = new Vector3(_beam.transform.position.x, _animator.transform.position.y + distance / 2, _beam.transform.position.z);
                _splash.transform.position = new Vector3(_beam.transform.position.x, _beam.transform.position.y + distance / 2 - 0.5f, _beam.transform.position.z);
                _smoke.transform.position = new Vector3(_beam.transform.position.x, _beam.transform.position.y + distance / 2 + 1.5f, _beam.transform.position.z);
                _smokeSprite.flipY = true;
            }
        }
        else if (rotation == 270f)
        {
            RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin, Vector2.left, _maxLaserRange, WallMask, true, Color.red);

            if (raycast)
            {
                float distance = raycast.point.x - transform.position.x;
                _beam.transform.localScale = new Vector3(laserScale, laserScale * distance, laserScale);
                _beam.transform.position = new Vector3(_animator.transform.position.x + distance / 2, _beam.transform.position.y, _beam.transform.position.z);
                _splash.transform.position = new Vector3(_beam.transform.position.x + distance / 2 + 0.5f, _beam.transform.position.y, _beam.transform.position.z);
                _smoke.transform.position = new Vector3(_beam.transform.position.x + distance / 2, _beam.transform.position.y + 1.0f, _beam.transform.position.z);
                _smokeSprite.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            }
        }

        _canUpdate = true;
    }


	void Update()
	{
        if (!_canUpdate)
            return;

		if (!_on)
		{
			_on = _animator.GetBool ("Reacting");

            if (_on && !_active && !_hasSwitch) {
				StartCoroutine (Fire (0.1f));
			}
		}
		else
		{
            if (AlwaysUpdate)
                UpdateLaser();

			_on = _animator.GetBool ("Reacting");
		}

        //Debug.Log(_on + " " + _active + " " + _hasSwitch);
    }


	protected  IEnumerator Fire(float rate)
	{
		if (rate == 0)
		{
			rate = FireRate;
		}

		if (OffSoundEffect != null && !_deactivated)
			SoundManager.Instance.PlaySound (OffSoundEffect, transform.position, false, 1f + _pitch);

        yield return new WaitForSeconds (rate);

        if (!_deactivated)
        {
            _active = true;

            CorgiTools.UpdateAnimatorBool(_animator, "On", _active);

            if (ChargeSoundEffect != null)
                SoundManager.Instance.PlaySound(ChargeSoundEffect, transform.position);

            yield return new WaitForSeconds(0.667f);
            _beamSprite.enabled = _active;
            _splashSprite.enabled = _active;
            _smokeSprite.enabled = _active;
            _damage.enabled = _active;

            AudioSource onSound = null;
            if (SoundEffect != null)
                onSound = SoundManager.Instance.PlaySound(SoundEffect, transform.position, false, 1f + _pitch);

            if (OnTimer)
            {
                yield return new WaitForSeconds(FireDuration);
                TurnOff();
                onSound.Stop();
            }
        }
	}

	protected void TurnOff()
	{
		_active = false;

		CorgiTools.UpdateAnimatorBool(_animator,"On", _active);

		_beamSprite.enabled = _active;
		_splashSprite.enabled = _active;
		_smokeSprite.enabled = _active;
		_damage.enabled = _active;

		if (FadeEffect != null) {
			var fade = Instantiate(FadeEffect,_beam.transform.position,_beam.transform.rotation);

			Destroy (fade.gameObject, 0.333f);
		}

		if(OnTimer && _on && !_hasSwitch)
			StartCoroutine (Fire (0));
	}


	override public IEnumerator Close(float duration)
	{
		yield return new WaitForSeconds (duration);

		StartCoroutine (Fire (0));
    }

	override public IEnumerator Open(float duration)
	{
		yield return new WaitForSeconds (duration);

        StartCoroutine(Freeze(0.01f, transform));
        StartCoroutine(DoTurnOff(0.5f));
        StartCoroutine(Thaw(1.5f));
    }

    public IEnumerator DoTurnOff(float duration)
    {
        yield return new WaitForSeconds(duration);
        TurnOff();
    }

	override public void Flicker()
	{
		// Override me
	}

    override public IEnumerator InstantOpen(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_hasSwitch)
        {
            _deactivated = true;
            TurnOff();
        }
    }
}

