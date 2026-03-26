using UnityEngine;
using System.Collections;

public class BossWall : MonoBehaviour
{
	public bool Activated = false;
	public AudioClip ActivateSfx;
	public AudioClip DeactivateSfx;
	public float Order = 0.5f;

	private SpriteRenderer _sprite;

	public int SaveIndex = 0;


    // Use this for initialization
    void Start ()
	{
		_sprite = GetComponent<SpriteRenderer> ();

		if (Activated) {
			_sprite.color = new Color (1f, 1f, 1f, 1f);
            gameObject.isStatic = true;
            gameObject.layer = LayerMask.NameToLayer ("Platforms");
		}
		else {
			_sprite.color = new Color (1f, 1f, 1f, 0f);
            gameObject.isStatic = false;
            gameObject.layer = LayerMask.NameToLayer ("Foreground");
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void Activate(bool playSound = true)
	{
		if (Activated)
			return;

		Activated = true;
		gameObject.layer = LayerMask.NameToLayer ("Platforms");

		StartCoroutine (ReallyActivate (Order,playSound));
	}

	protected virtual IEnumerator ReallyActivate(float duration, bool playSound)
	{
		yield return new WaitForSeconds (duration);

		_sprite.color = new Color (1f, 1f, 1f, 1f);
        gameObject.isStatic = true;

        GetComponent<Flickers> ().Flicker ();

		if (ActivateSfx != null && playSound)
			SoundManager.Instance.PlaySound(ActivateSfx, transform.position);

		Vector3 ShakeParameters = new Vector3(0.5f,0.5f,1f);
		CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

		if (sceneCamera != null)
			sceneCamera.Shake(ShakeParameters);
	}

	public void Deactivate(bool playSound = true)
	{
		if (!Activated)
			return;

		Activated = false;

		StartCoroutine (ReallyDeactivate (Order, playSound));
	}

	protected virtual IEnumerator ReallyDeactivate(float duration, bool playSound)
	{
		yield return new WaitForSeconds (duration);

		var flicker = GetComponent<Flickers> ();
		flicker.Flicker ();

		yield return new WaitForSeconds (flicker.FlickerSpeed*(float)flicker.FlickerCount);

		gameObject.layer = LayerMask.NameToLayer ("Foreground");
		_sprite.color = new Color (1f, 1f, 1f, 0f);
        gameObject.isStatic = false;


        if (DeactivateSfx != null && playSound)
			SoundManager.Instance.PlaySound(DeactivateSfx, transform.position);

        LevelVariables.blockState[SaveIndex] = true;
    }
}

