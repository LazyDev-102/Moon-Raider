using UnityEngine;
using System.Collections;

public class Door : Switchable
{
	public AudioClip CloseSoundEffect;
	public AudioClip OpenSoundEffect;

	public float AutoOpenTime = 0;
	public float AutoCloseTime = 0;

	private bool opening = false;
	private bool closing = false;
	private Vector2 orgPos;
	private float speed = 0.125f;

	private SpriteRenderer _onSprite;
	private SpriteRenderer _offSprite;
	private SpriteRenderer _capSprite;


    // Use this for initialization

    public virtual void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        Sprite[] doors = Resources.LoadAll<Sprite>("doors");

        orgPos = transform.position;

        _onSprite = GetComponent<SpriteRenderer>();
        _offSprite = GetComponentsInChildren<SpriteRenderer>()[1];
        _capSprite = GetComponentsInChildren<SpriteRenderer>()[2];

        // Change door colors on certain worlds
        PickerTileMap grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<PickerTileMap>();
        int world = GlobalVariables.WorldIndex;

        if (world == 2)
        {
            _offSprite.sprite = doors[0];
            _onSprite.sprite = doors[1];
            _capSprite.sprite = doors[4];
        }
    }

    void Start()
    {
        if (AutoOpenTime > 0)
			StartCoroutine (Open (AutoOpenTime));

		if(AutoCloseTime > 0)
			StartCoroutine (Close (AutoCloseTime));
	}

	override public void Flicker()
	{
		_capSprite.transform.parent = null;

		float tick = 0.083f;

		StartCoroutine(Off(4*tick));
		StartCoroutine(On(5*tick));
		StartCoroutine(Off(6*tick));
	}

	public virtual IEnumerator On(float duration)
	{
		yield return new WaitForSeconds (duration);

		_onSprite.enabled = true;
	}

	public virtual IEnumerator Off(float duration)
	{
		yield return new WaitForSeconds (duration);

		_onSprite.enabled = false;
	}

	override public IEnumerator Open(float duration)
	{
		yield return new WaitForSeconds (duration);

		if(OpenSoundEffect != null)
			SoundManager.Instance.PlaySound(OpenSoundEffect,transform.position);

		GetComponent<SpriteRenderer> ().enabled = false;
		closing = false;
		opening = true;
        
		StartCoroutine(Freeze(0.01f, transform));
	}


	override public IEnumerator Close(float duration)
	{
		yield return new WaitForSeconds (duration);

		if(CloseSoundEffect != null)
			SoundManager.Instance.PlaySound(CloseSoundEffect,transform.position);

		GetComponent<SpriteRenderer> ().enabled = true;
		opening = false;
		closing = true;
	}

	// Update is called once per frame
	void Update ()
	{
		if (opening) {
			transform.Translate (new Vector3 (0, transform.localScale.y*speed, 0));

			if (Mathf.Abs(transform.position.y - orgPos.y) >= 4.4f)
			{
				opening = false;
				StartCoroutine(Thaw(0.5f));
			}
		}

		if (closing) {
			transform.Translate (new Vector3 (0, -transform.localScale.y*speed, 0));

			if(Mathf.Abs(transform.position.y - orgPos.y) <= 0)
				closing = false;
		}
	}

    override public IEnumerator InstantOpen(float duration)
    {
        yield return new WaitForSeconds(duration);

        _capSprite.transform.parent = null;
        transform.Translate(new Vector3(0, 3.6f* transform.localScale.y, 0));
    }
}

