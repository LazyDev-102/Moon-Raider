using UnityEngine;
using System.Collections;

public class Plug : MonoBehaviour, IPlayerRespawnListener
{
	public float DistanceFromTop = 1.0f;
	public AudioClip PullUpSoundEffect;
    public LayerMask DoorMask;

    public bool UnplugOnAwake = false;

	protected GameObject _buttonA;
	protected BoxCollider2D _boxCollider;
	protected Coroutine _coroutine;
	protected Collider2D _playerCollider;

    private Switchable _door;
	private Animator _animator;

    public int SaveIndex = 0;

    private bool scanned = false;


    public virtual void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();

        if(_buttonA != null)
            _buttonA.SetActive(!UnplugOnAwake);
    }

    protected virtual void Start ()
	{
        StartCoroutine(SetPulse(0.1f));

//		Debug.Log ("Plug at x: " +  transform.position.x +  " y: " + transform.position.y);
	}

    protected virtual IEnumerator SetPulse(float duration)
    {
        yield return new WaitForSeconds(duration);

        ScanForCable(transform.position, 0, true);

        yield return new WaitForSeconds(duration);

        if (UnplugOnAwake)
            Pull(false);
    }

    protected virtual IEnumerator Scan(float duration, bool instant = false) 
	{
		yield return new WaitForSeconds(duration);

		ScanForCable (transform.position, 0, false, instant);
	}

	bool ScanForSwitchable(Vector2 raycastOrigin, bool instant = false)
	{
		//if (_door != null) {
		//	Debug.Log ("Door already set!");
		//	return;
		//}

		float ScanDist = 2;

		// Scan for door. If none, scan for cable
		// Doors are only down or up
		
		RaycastHit2D[] doorDown = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.down, ScanDist, DoorMask, true, Color.yellow);

		for(int i = 0; i < doorDown.Length; i++)
		{
			_door = doorDown[i].collider.gameObject.GetComponent<Switchable>();

			if (_door != null)
				break;
//			Debug.Log ("--> " + doorDown[i].collider.gameObject.name + " DN at " + raycastOrigin.x + " " + raycastOrigin.y);
		}

		if (_door == null) 
		{
			RaycastHit2D[] doorUp = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.up, ScanDist, DoorMask, true, Color.yellow);

			for(int i = 0; i < doorUp.Length; i++)
			{
				_door = doorUp[i].collider.gameObject.GetComponent<Switchable> ();

				if (_door != null)
					break;

				//Debug.Log ("--> " + doorUp[i].collider.gameObject.name + " UP at " + raycastOrigin.x + " " + raycastOrigin.y);
			}
		}

        if (_door == null)
        {
            RaycastHit2D[] doorRight = CorgiTools.CorgiRaycastAll(raycastOrigin, Vector2.right, ScanDist, DoorMask, true, Color.yellow);

            for (int i = 0; i < doorRight.Length; i++)
            {
                _door = doorRight[i].collider.gameObject.GetComponent<Switchable>();

                if (_door != null)
                    break;
            }
        }

        if (_door == null)
        {
            RaycastHit2D[] doorLeft = CorgiTools.CorgiRaycastAll(raycastOrigin, Vector2.left, ScanDist, DoorMask, true, Color.yellow);

            for (int i = 0; i < doorLeft.Length; i++)
            {
                _door = doorLeft[i].collider.gameObject.GetComponent<Switchable>();

                if (_door != null)
                    break;
            }
        }


        if (_door != null) {

            if (instant)
            {
                _door.StartCoroutine(_door.InstantOpen(0.1f));
            }
            else
            {
                _door.Flicker();
                _door.StartCoroutine(_door.Open(1.0f));
                StartCoroutine(GameManager.Instance.Player.FreezeThawEnemies(false, true, 2.25f));
            }

            return _door.KeepScanning;
        }

		return true;
	}


	void ScanForCable(Vector2 raycastOrigin, int depth, bool enable = false, bool instant = false)
	{
        scanned = true;

		if (depth > 50) {
			Debug.Log ("Max scan depth!");
			return;
		}

        //		Debug.Log (depth);

        bool shouldScan = true;
        if(!enable)
	    	shouldScan = ScanForSwitchable (raycastOrigin, instant);

		if (!shouldScan)
			return;

		float ScanDist = 1.25f;

		var cableMask = 1 << LayerMask.NameToLayer ("Foreground");

		RaycastHit2D[] cableDown = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.down, ScanDist, cableMask, true, Color.yellow);
		for (int i = 0; i < cableDown.Length; i++) 
        {
			if (cableDown[i]) 
            {
                Cord cord = cableDown[i].collider.GetComponent<Cord>();

                if (cord != null) 
                {
                    if (enable)
                    {
                        if (!cord.Energized)
                        {
                            cord.SetPulse(0);
                            ScanForCable(cableDown[i].collider.transform.position, depth + 1, true, false);
                        }
                    }
                    else
                    {
                        if (instant)
                            cord.InstantOff();
                        else
                            cord.Flicker();
                        ScanForCable(cableDown[i].collider.transform.position, depth + 1, false, instant);
                    }
				}
			}
		}

		RaycastHit2D[] cableRight = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.right, ScanDist, cableMask, true, Color.yellow);
		for (int i = 0; i < cableRight.Length; i++) 
        {
			if (cableRight[i]) 
            {
                Cord cord = cableRight[i].collider.GetComponent<Cord>();

                if (cord != null)
                {
                    if (enable)
                    {
                        if (!cord.Energized)
                        {
                            cord.SetPulse(1);
                            ScanForCable(cableRight[i].collider.transform.position, depth + 1, true, false);
                        }
                    }
                    else
                    {
                        if (instant)
                            cord.InstantOff();
                        else
                            cord.Flicker();
                        ScanForCable(cableRight[i].collider.transform.position, depth + 1, false, instant);
                    }
                }
			}
		}

		RaycastHit2D[] cableUp = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.up, ScanDist, cableMask, true, Color.yellow);
		for (int i = 0; i < cableUp.Length; i++) 
        {
			if (cableUp[i]) 
            {
                Cord cord = cableUp[i].collider.GetComponent<Cord>();

                if (cord != null)
                {
                    if (enable)
                    {
                        if (!cord.Energized)
                        {
                            cord.SetPulse(2);
                            ScanForCable(cableUp[i].collider.transform.position, depth + 1, true, false);
                        }
                    }
                    else
                    {
                        if (instant)
                            cord.InstantOff();
                        else
                            cord.Flicker();
                        ScanForCable(cableUp[i].collider.transform.position, depth + 1, false, instant);
                    }
                }
            }
		}

		RaycastHit2D[] cableLeft = CorgiTools.CorgiRaycastAll (raycastOrigin, Vector2.left, ScanDist, cableMask, true, Color.yellow);
		for (int i = 0; i < cableLeft.Length; i++) 
        {
			if (cableLeft[i]) 
            {
                Cord cord = cableLeft[i].collider.GetComponent<Cord>();

                if (cord != null)
                {
                    if (enable)
                    {
                        if (!cord.Energized)
                        {
                            cord.SetPulse(3);
                            ScanForCable(cableLeft[i].collider.transform.position, depth + 1, true, false);
                        }
                    }
                    else
                    {
                        if (instant)
                            cord.InstantOff();
                        else
                            cord.Flicker();
                        ScanForCable(cableLeft[i].collider.transform.position, depth + 1, false, instant);
                    }
                }
			}
		}
	}


	virtual public void Pull(bool instant = false)
	{
        if (_boxCollider != null)
        {
            if (!_boxCollider.enabled)
                return;
        }

        if(!instant)
            StartCoroutine(GameManager.Instance.Player.FreezeThawEnemies(true, true, 0.5f));

        LevelVariables.switchState[SaveIndex] = true;

        if (_boxCollider != null)
            _boxCollider.enabled = false;

        if(instant)
        {
            StartCoroutine(Pulled(0.01f, true));
            StartCoroutine(Scan(0.1f, true));
        }
        else
        {
            StartCoroutine(Pulled(0.5f, false));
            StartCoroutine(Scan(0.5f, false));
        }
	}


	protected virtual IEnumerator Pulled(float duration, bool instant) 
	{
		yield return new WaitForSeconds(duration);

		if(PullUpSoundEffect != null && !instant)
			SoundManager.Instance.PlaySound(PullUpSoundEffect,transform.position);

        if(_animator != null)
		    CorgiTools.UpdateAnimatorBool(_animator,"Pulled",true);
	}

	/// <summary>
	/// Triggered when something collides with the plant leaves
	/// </summary>
	/// <param name="collider">Other.</param>
	public virtual void OnTriggerEnter2D (Collider2D collider) 
	{
		if (GetComponent<SpriteRenderer> ().enabled == false || UnplugOnAwake) {
			return;
		}

		_playerCollider = collider;

		// if what's colliding with the coin ain't a characterBehavior, we do nothing and exit
		CharacterBehavior behavior = collider.GetComponent<CharacterBehavior>();
		if (behavior == null)
			return;

		ShowPrompt ();
		behavior.OnPlug (this);

#if UNITY_IOS || UNITY_ANDROID
        GUIManager.Instance.ContextualButton.enabled = true;
#endif
    }

	public virtual void OnTriggerExit2D (Collider2D collider) 
	{
		_playerCollider = collider;

		// if what's colliding with the coin ain't a characterBehavior, we do nothing and exit
		CharacterBehavior behavior = collider.GetComponent<CharacterBehavior>();
		if (behavior == null)
			return;

		HidePrompt ();
		behavior.OffPlug (this);

#if UNITY_IOS || UNITY_ANDROID
        GUIManager.Instance.ContextualButton.enabled = false;
#endif
    }

	/// <summary>
	/// Shows the button A prompt.
	/// </summary>
	protected virtual void ShowPrompt()
	{
		// we add a blinking A prompt to the top of the zone
		if (_buttonA == null) {
            if (Application.platform == RuntimePlatform.Switch)
                _buttonA = (GameObject)Instantiate(Resources.Load("GUI/ButtonX"));
            else if (Application.platform == RuntimePlatform.PS4)
                _buttonA = (GameObject)Instantiate(Resources.Load("GUI/ButtonTriangle"));
            else
            {
#if UNITY_IOS || UNITY_ANDROID
                _buttonA = (GameObject)Instantiate(Resources.Load("GUI/ButtonY"));
#else
                if (JoystickManager.Instance.JoystickConnected == true)
                    _buttonA = (GameObject)Instantiate(Resources.Load("GUI/ButtonY"));
                else
                    _buttonA = (GameObject)Instantiate(Resources.Load("GUI/ButtonDown"));
#endif
            }

            _buttonA.transform.position = new Vector2 (_boxCollider.bounds.center.x, _boxCollider.bounds.max.y + DistanceFromTop); 
			_buttonA.transform.parent = transform;
			_buttonA.GetComponent<SpriteRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);
		}

		if (_coroutine != null) {
			StopCoroutine (_coroutine);
			_coroutine = null;
		}
		_coroutine = StartCoroutine (CorgiTools.FadeSprite (_buttonA.GetComponent<SpriteRenderer> (), 0.2f, new Color (1f, 1f, 1f, 1f)));
	}

	protected virtual void HidePrompt()
	{
		if (_buttonA != null) {
			if (_coroutine != null) {
				StopCoroutine (_coroutine);
				_coroutine = null;
			}
			_coroutine = StartCoroutine (CorgiTools.FadeSprite (_buttonA.GetComponent<SpriteRenderer> (), 1.0f, new Color (1f, 1f, 1f, 0f)));	
		}
	}

	/// <summary>
	/// When the player respawns, we reinstate the object
	/// </summary>
	/// <param name="checkpoint">Checkpoint.</param>
	/// <param name="player">Player.</param>
	public virtual void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint, CharacterBehavior player)
	{
		gameObject.SetActive(true);
	}
}
