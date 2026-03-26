using UnityEngine;
using System.Collections;

public class ElectroCoil : MonoBehaviour
{
	public float FireRate = 3;
	public float FireDuration = 2;

	public AudioClip SoundEffect;
	public AudioClip ChargeSoundEffect;
	public AudioClip OffSoundEffect;

	public bool Reacting = false;
	private bool _active = false;
	private bool _isEmitter = false;
	private AIReact _react;
	private int failsafe = 0;

	private SpriteRenderer _tipSpark;
	private ElectroCoil _partner;

	private GameObject[] _arcs = {null, null, null, null, null};

	// Use this for initialization
	void Start ()
	{
		_react = GetComponent<AIReact> ();
		_tipSpark = GetComponentsInChildren<SpriteRenderer> () [1];
        _tipSpark.enabled = false;

        StartCoroutine(Bootup(0.5f));

    }

    public IEnumerator Bootup(float duration)
    {
        yield return new WaitForSeconds(duration);

        float rotation = transform.rotation.eulerAngles.z;

        if (rotation == 180)
            StartCoroutine(ScanForPartner(0.5f, Vector3.up, rotation));
        else if (rotation == 90)
            StartCoroutine(ScanForPartner(0.5f, Vector3.right, rotation));
    }

    public IEnumerator ScanForPartner(float duration, Vector3 dir, float rotation)
	{
		yield return new WaitForSeconds (duration);

		Scan (duration, dir, rotation);

	}

	void Scan(float duration, Vector3 dir, float rotation)
	{
		_isEmitter = true;

		var maskLayer = 1 << LayerMask.NameToLayer ("Platforms");

		RaycastHit2D[] raycasts = CorgiTools.CorgiRaycastAll (transform.position,dir,20,maskLayer,true,Color.white);	

		for(int r = 0; r < raycasts.Length; r++)
		{
			RaycastHit2D raycast = raycasts [r];

			var potential = raycast.collider.GetComponent<ElectroCoil> ();
			if (potential != null) {
				if (potential != this)
					_partner = raycast.collider.GetComponent<ElectroCoil> ();
			} else {
				continue;
			}

			Vector3 pos = transform.position + 2*dir;
			Vector3 tPos = raycast.collider.transform.position - 2*dir;

			while (pos.x <= tPos.x && pos.y <= tPos.y && failsafe < 5) 
			{
				var arcPrefab = Resources.Load ("Obstacles/ElectricArc") as GameObject;
				var arc = GameObject.Instantiate (arcPrefab, pos, Quaternion.Euler(0, 0, rotation + 90));
				arc.transform.parent = transform.parent;

				arc.SetActive(false);

				_arcs [failsafe] = arc;

				pos += 2 * dir;
				failsafe++;
			}
		}

		//if (_partner == null)
		//	Debug.Log ("No partner found!");
	}
	
	// Update is called once per frame
	void Update ()
	{
		Reacting = _react.Reacting;

		if (!_isEmitter || _partner == null)
			return;

		Reacting = Reacting || _partner.Reacting;

		if (Reacting && !_active) {
			StartCoroutine (Fire (FireRate));
		}
	}

	public void TipOn(bool on)
	{
		_tipSpark.enabled = on;
	}

	protected  IEnumerator Fire(float rate)
	{
		if (rate == 0)
		{
			rate = FireRate;
		}

		if (OffSoundEffect != null)
			SoundManager.Instance.PlaySound (OffSoundEffect, transform.position);

		_active = true;

		yield return new WaitForSeconds (rate);

		_tipSpark.enabled = true;

		if(_partner != null)
			_partner.TipOn (true);

		if (ChargeSoundEffect != null)
			SoundManager.Instance.PlaySound (ChargeSoundEffect, transform.position);

		yield return new WaitForSeconds (0.667f);

		for (int i = 0; i < failsafe; i++)
			_arcs [i].SetActive(true);


		AudioSource onSound = null;
		if (SoundEffect != null)
			onSound = SoundManager.Instance.PlaySound (SoundEffect, transform.position);

		yield return new WaitForSeconds (FireDuration);

		for (int i = 0; i < failsafe; i++)
			_arcs [i].SetActive(false);

		_active = false;
		_tipSpark.enabled = false;

		if(_partner != null)
			_partner.TipOn (false);
		
		if(onSound != null)
			onSound.Stop ();
	}
}

