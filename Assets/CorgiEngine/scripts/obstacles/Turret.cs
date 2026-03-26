using UnityEngine;
using System.Collections;

public class Turret : Switchable
{
	AIReact _react;
	Animator _animator;
	public float Speed = 4;
	public SpriteRenderer WeaponAttachment;
	public float FireDelay = 1;
	public float BurstDelay = 1;
	public float BurstCount = 3;
	public Projectile TheProjectile;

	public AudioClip SoundEffect = null;
	public AudioClip WarnSoundEffect = null;

	public AudioClip OffSoundEffect;

	private bool _shooting = false;
	private bool _couldShoot = false;
	private Vector2 _direction; 
	float ShootAngle = 0;
	private int burst = 0;
	private Coroutine shooting;

	private bool armed = true;
	private SpriteRenderer _turret;

    private GameObject TargetGameObject;


    public SpriteRenderer Emitter
    {
        get
        {
            return _turret;
        }
    }


	// Use this for initialization
	void Start ()
	{
		_animator = GetComponentInChildren<Animator> ();
		_react = GetComponent<AIReact> ();
		_turret = GetComponentsInChildren<SpriteRenderer> ()[1];
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        _shooting = false;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		bool targeting = false;
		float targetRotation = 0;

		if (!_react.Reacting)
        {
			_shooting = false;
			targeting = true;
            TargetGameObject = null;
		}
		else 
		{
            if (TargetGameObject == null)
                TargetGameObject = _react.Target;

            float y = TargetGameObject.transform.position.y;
			if (y > transform.position.y)
				targeting = true;

			if (armed) 
			{	
				float x = TargetGameObject.transform.position.x;

				if (x < (transform.position.x - 2))
					targetRotation = -45;
                else if (x > (transform.position.x + 2))
					targetRotation = 45;
			}
		}

		float currentRotation = _turret.transform.rotation.eulerAngles.z;

		if (currentRotation > 180)
			currentRotation -= 360;

        //Debug.Log (_react.Reacting + " " + currentRotation + " -> " + targetRotation);

        _turret.transform.rotation = Quaternion.Euler(0, 0, targetRotation);

        if (targetRotation > 0) 
        {
        	ShootAngle = targetRotation;
        } 
        else if (targetRotation < 0) 
        {
        	ShootAngle = targetRotation + 180;
        } 
        else 
        {
        	ShootAngle = 90;
        }

        _direction = new Vector2((float)Mathf.Cos(Mathf.Deg2Rad*ShootAngle),-(float)Mathf.Sin(Mathf.Deg2Rad*ShootAngle));

		_shooting = !targeting && armed;

		if (_shooting && !_couldShoot) 
		{
			if (WarnSoundEffect != null)
				SoundManager.Instance.PlaySound (WarnSoundEffect, transform.position);
			
			StartCoroutine (Shoot (FireDelay));
		} 
		else if (!_shooting && _couldShoot) {
			_shooting = false;
            
            if (shooting != null)
			    StopCoroutine (shooting);
		}

		_couldShoot = _shooting;
	}

	public virtual IEnumerator Shoot(float duration)
	{
		yield return new WaitForSeconds (duration);

        if (_shooting && armed) 
		{
			burst++;
			Vector2 firePos = transform.position;

			if (WeaponAttachment != null)
				firePos = WeaponAttachment.transform.position;

			if (SoundEffect != null)
				SoundManager.Instance.PlaySound (SoundEffect, transform.position);

			Quaternion rotation = Quaternion.Euler (0, 0, ShootAngle);
			Projectile projectile = (Projectile)Instantiate (TheProjectile, firePos, rotation);
			projectile.Initialize (gameObject, _direction, 5 * _direction, false);

			CorgiTools.UpdateAnimatorBool(_animator,"Shooting",true);

			if (burst < BurstCount)
				StartCoroutine (Shoot (FireDelay));
			else {
				CorgiTools.UpdateAnimatorBool(_animator,"Shooting",false);
				burst = 0;
				shooting = StartCoroutine (Shoot (BurstDelay));
			}
		}
	}

	void StopShooting()
	{
		_shooting = false;
        TargetGameObject = null;
        CorgiTools.UpdateAnimatorBool(_animator,"Shooting",false);
	}


    public void SetArmed(bool a)
    {
        armed = a;
    }


	override public IEnumerator Close(float duration)
	{
		yield return new WaitForSeconds (duration);

		armed = true;

		//Debug.Log ("Enabling turret");
	}

	override public IEnumerator Open(float duration)
	{
		yield return new WaitForSeconds (duration);

		//Debug.Log ("Disabling turret");

		armed = false;
		_shooting = false;

		CorgiTools.UpdateAnimatorBool(_animator,"Off",true);

		if (OffSoundEffect != null)
			SoundManager.Instance.PlaySound (OffSoundEffect, transform.position);

        if(shooting != null)
		    StopCoroutine (shooting);

        StartCoroutine(Freeze(0.01f, transform));
        StartCoroutine(Thaw(1.5f));
    }

	override public void Flicker()
	{
		// Override me
	}

    override public IEnumerator InstantOpen(float duration)
    {
        yield return new WaitForSeconds(duration);

        armed = false;
        _shooting = false;

        CorgiTools.UpdateAnimatorBool(_animator, "Off", true);
    }
}

