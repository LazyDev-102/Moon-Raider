using UnityEngine;
using System.Collections;
using JulienFoucher;

public class Fireball : MonoBehaviour
{
	Vector3 _orgPosition;
	Vector3 _topPosition;
	SpriteRenderer _renderer;
	Animator _animator;
    SpriteTrail _trail;

	bool _up = true, _oldUp = true;
	GameObject _splash = null;
	float height = 9.0f;

    public float zPos = 1f;
    public AudioClip LaunchSfx;


    void Awake()
	{
		_renderer = gameObject.GetComponentInChildren<SpriteRenderer> ();
		_animator = GetComponentInChildren<Animator> ();
        _trail = GetComponentInChildren<SpriteTrail>();

    }

	void Start()
	{
		_orgPosition = transform.position + 4 * Vector3.down;
        _orgPosition.z = zPos;
        _topPosition = _orgPosition + height * Vector3.up;
	}

	// Update is called once per frame
	void Update ()
	{
		_oldUp = _up;

		float delta = (_topPosition.y - transform.position.y) * (_topPosition.y - transform.position.y) + 12f;

		// Safety cap
		if (delta > height + 12f)
			delta = height + 12f;

		float y = 0.5f*delta * Time.deltaTime;

		if (_up) {
			_up = (transform.position.y <= _topPosition.y);

            if (transform.position.y > _orgPosition.y + 3)
                _trail.enabled = true;
        } 
        else {
			
			y = -y;
			_up = (transform.position.y < _orgPosition.y);

			if (transform.position.y < _orgPosition.y + 3)
				Splash ();
		}

		Vector3 newPosition = new Vector3 (0, y, 0);
		transform.Translate (newPosition, Space.World);

		CorgiTools.UpdateAnimatorFloat(_animator,"SpeedY", y / Time.deltaTime);
		_renderer.flipY = !_up;
	}


    void Splash()
	{
		if (_splash != null)
			return;

		if (LaunchSfx != null)
			SoundManager.Instance.PlaySound (LaunchSfx, transform.position);

        _trail.enabled = false;

        var splashPrefab = Resources.Load ("FX/LavaSplash") as GameObject;
		_splash = Instantiate (splashPrefab, _orgPosition + 3f * Vector3.up, Quaternion.identity);
		_splash.transform.parent = transform.parent;
		Destroy (_splash, 0.583f);
	}
}

