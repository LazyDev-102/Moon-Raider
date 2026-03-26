using UnityEngine;
using System.Collections;
[RequireComponent(typeof(BoxCollider2D))]

/// <summary>
/// Add this class to an empty component. It will automatically add a boxcollider2d, set it to "is trigger". Then customize the dialogue zone
/// through the inspector.
/// </summary>

public class DialogueZone : MonoBehaviour 
{	
	[Header("Dialogue Look")]
	/// the color of the text background.
	public Color TextBackgroundColor=Color.black;
	/// the color of the text
	public Color TextColor=Color.white;
	/// if true, the dialogue box will have a small, downward pointing arrow
	public bool ArrowVisible=true;
	
	[Space(10)]
	
	[Header("Dialogue Speed (in seconds)")]
	/// the duration of the in and out fades
	public float FadeDuration = 0.2f;
	/// the time between two dialogues 
	public float TransitionTime = 0.2f;
		
	[Space(10)]	
	[Header("Dialogue Position")]
	/// the distance from the top of the box collider the dialogue box should appear at
	public float DistanceFromTop = 0;
	
	[Space(10)]	
	[Header("Button handled or auto playing ?")]
	/// true if the dialogue box is button handled - note that this will prevent the player from moving while all the messages have not been read
	public bool ButtonHandled = true;
	public bool CanMoveWhileTalking = true;
	public bool ShouldCelebrate = true;
	public bool ShowInventory = false;
	[Header("Only if the dialogue is button handled :")]
	public bool AlwaysShowPrompt = true;
	/// duration of the message. only considered if the box is not button handled
	[Header("Only if the dialogue is not button handled :")]
	[Range (1, 100)]
	public float MessageDuration = 3f;
	
	[Space(10)]
	
	[Header("Activations")]
	/// true if can be activated more than once
	public bool ActivableMoreThanOnce = true;
	/// if the zone is activable more than once, how long should it remain inactive between up times ?
	[Range (1, 100)]
	public float InactiveTime = 2f;
	public AudioClip DialogueSound;
	public AudioClip DeactivateSound;
	public AudioClip EvaporateSound;

	[Space(10)]

	/// the dialogue lines
	[Multiline]
	public string[] Dialogue;

    /// private variables
	//protected DialogueBar _dialogueBar;
    protected BoxCollider2D _boxCollider;
    protected bool _activated=false;
    protected bool _playing=false;
    protected int _currentIndex;
    protected bool _activable=true;
    protected GameObject _buttonA;
    protected CharacterBehavior _character;
	protected Animator _animator;

	private Vector3 _targetPosition;

    /// <summary>
    /// Initializes the dialogue zone
    /// </summary>
    protected virtual void Start () 
	{			
		_boxCollider = (BoxCollider2D)GetComponent<BoxCollider2D>();
		_animator = GetComponentInParent<Animator>();
		_currentIndex=0;

		//_dialogueBar = GUIManager.Instance.Dialogue;

		_targetPosition = transform.parent.gameObject.transform.position;
				
		if (AlwaysShowPrompt)
			ShowPrompt();
	}
		
	protected virtual void Update()
	{
		/*Vector3 pos = transform.parent.gameObject.transform.position;

		float x = Mathf.Lerp (pos.x, _targetPosition.x, 10*Time.deltaTime);
		float y = Mathf.Lerp (pos.y, _targetPosition.y, 10*Time.deltaTime);
		float z = Mathf.Lerp (pos.z, _targetPosition.z, 10*Time.deltaTime);

		transform.parent.gameObject.transform.position = new Vector3 (x, y, z);*/
	}

	/// <summary>
	/// When triggered, either by button press or simply entering the zone, starts the dialogue
	/// </summary>
	public virtual void StartDialogue()
	{
		// if the button A prompt is displayed, we hide it
		if (_buttonA != null)
			Destroy(_buttonA);
	
		// if the dialogue zone has no box collider, we do nothing and exit
		if (_boxCollider == null)
			return;	
		
		// if the zone has already been activated and can't be activated more than once.
		if (_activated && !ActivableMoreThanOnce)
			return;
			
		// if the zone is not activable, we do nothing and exit
		if (!_activable)
			return;
		
		// if the player can't move while talking, we notify the game manager
		if (!CanMoveWhileTalking)
		{
			GameManager.Instance.FreezeCharacter();
		}
									
		// if it's not already playing, we'll initialize the dialogue box
		if (!_playing)
		{	
			//_dialogueBar.SlideIn();

			// the dialogue is now playing
			_playing = true;
		}
		// we start the next dialogue
		StartCoroutine(PlayNextDialogue());
	}

    /// <summary>
    /// Plays the next dialogue in the queue
    /// </summary>
    protected virtual IEnumerator PlayNextDialogue()
	{		
		// if this is not the first message
		if (_currentIndex != 0)
		{
			// we wait for the specified transition time before playing the next dialogue
			yield return new WaitForSeconds(TransitionTime);
		}	
		
		// if we've reached the last dialogue line, we show options (if they exist)
		if (_currentIndex >= Dialogue.Length) {

			if (ShowInventory) {
				//_dialogueBar.TalkZone = this;
				//_dialogueBar.FadeIn (FadeDuration);
			}
			else {
				Close ();

				_currentIndex = 0;

				_boxCollider.enabled = false;

				// we set activated to true as the dialogue zone has now been turned on		
				_activated = true;

				if ((ButtonHandled) && (_character != null)) {				
					_character.BehaviorState.InDialogueZone = false;
					_character.BehaviorState.CurrentDialogueZone = null;
				}
			}

			yield break;
		} else {
			if (DialogueSound != null)
				SoundManager.Instance.PlaySound(DialogueSound,transform.position);
		}
	
		// every dialogue box starts with it fading in
		//_dialogueBar.StartCoroutine(_dialogueBar.Speak(Dialogue[_currentIndex]));

		_currentIndex++;
		
		// if the zone is not button handled, we start a coroutine to autoplay the next dialogue
		if (!ButtonHandled)
		{
			StartCoroutine(AutoNextDialogue());
		}
	}

	public void Close()
	{
		//_dialogueBar.SlideOut();

		if (ShouldCelebrate) {
			CorgiTools.UpdateAnimatorBool (_animator, "Celebrate", true);

			StartCoroutine (Deactivate ());
		} else {
			StartCoroutine (Reactivate ());
			GameManager.Instance.ThawCharacter ();
		}
	}

    protected virtual IEnumerator AutoNextDialogue()
	{
		// we wait for the duration of the message
		yield return new WaitForSeconds(MessageDuration);
		StartCoroutine(PlayNextDialogue());
	}

    protected virtual IEnumerator Reactivate()
	{
		// we let the player move again
		if (!CanMoveWhileTalking)
		{
			GameManager.Instance.CanMove = true;
		}
		yield return new WaitForSeconds(InactiveTime);
		_boxCollider.enabled = true;
		_activable = true;
		_currentIndex = 0;
		_playing = false;
		if (AlwaysShowPrompt)
			ShowPrompt();
	}

	protected virtual IEnumerator Deactivate()
	{
		yield return new WaitForSeconds(InactiveTime);

		if (EvaporateSound!=null)
			SoundManager.Instance.PlaySound(EvaporateSound,transform.position);

		CorgiTools.UpdateAnimatorBool (_animator, "Evaporate", true);
		_targetPosition = _character.transform.position;
		_targetPosition.y -= 1;
		yield return new WaitForSeconds(0.5f);

		_character.BehaviorState.CritterUp = true;

		GetComponentInParent<SpriteRenderer> ().enabled = false;

		if (DeactivateSound!=null)
			SoundManager.Instance.PlaySound(DeactivateSound,transform.position);

		yield return new WaitForSeconds(0.75f);

		_character.BehaviorState.CritterUp = false;

		GameManager.Instance.ThawCharacter ();

		Destroy (transform.parent.gameObject);

		//gameObject.SetActive(false);
	}

    /// <summary>
    /// Shows the button A prompt.
    /// </summary>
    protected virtual void ShowPrompt()
	{
		// we add a blinking A prompt to the top of the zone
		_buttonA = (GameObject)Instantiate(Resources.Load("GUI/ButtonY"));			
		_buttonA.transform.position=new Vector2(_boxCollider.bounds.center.x,_boxCollider.bounds.max.y+DistanceFromTop); 
		_buttonA.transform.parent = transform;
		_buttonA.GetComponent<SpriteRenderer>().material.color=new Color(1f,1f,1f,0f);
		StartCoroutine(CorgiTools.FadeSprite(_buttonA.GetComponent<SpriteRenderer>(),0.2f,new Color(1f,1f,1f,1f)));	
	}

    /// <summary>
    /// Hides the button A prompt.
    /// </summary>
    protected virtual IEnumerator HidePrompt()
	{	
		StartCoroutine(CorgiTools.FadeSprite(_buttonA.GetComponent<SpriteRenderer>(),0.2f,new Color(1f,1f,1f,0f)));	
		yield return new WaitForSeconds(0.3f);
		Destroy(_buttonA);
	}

	/// <summary>
	/// Triggered when something collides with the dialogue zone
	/// </summary>
	/// <param name="collider">Something colliding with the water.</param>
	public virtual void OnTriggerEnter2D(Collider2D collider)
	{
		// we check that the object colliding with the water is actually a corgi controller and a character
		CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
		if (character==null)
			return;		
		CorgiController controller = collider.GetComponent<CorgiController>();
		if (controller==null)
			return;	
		if (character.tag!="Player")
			return;	
			
		_character = character;
		
		// if the dialogue zone is button handled, we change the character state	
		if (ButtonHandled)
		{
			// if we're not already showing the prompt and if the zone can be activated, we show it
			if ((_buttonA == null) && _activable && !_playing)
				ShowPrompt();	
						
			_character.BehaviorState.InDialogueZone = true;
			_character.BehaviorState.CurrentDialogueZone = this;
		}
		else
		{
			if (!_playing)
			{
				// if it's not button handled, we start the dialogue instantly
				StartDialogue();
			}
		}
	}
		
	/// <summary>
	/// Triggered when something exits the water
	/// </summary>
	/// <param name="collider">Something colliding with the dialogue zone.</param>
	public virtual void OnTriggerExit2D(Collider2D collider)
	{
		// we check that the object colliding with the water is actually a corgi controller and a character
		CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
		if (character==null)
			return;		
		CorgiController controller = collider.GetComponent<CorgiController>();
		if (controller==null)
			return;
		if (character.tag!="Player")
			return;
						
		if (ButtonHandled)
		{
			if ((_buttonA!=null) && !AlwaysShowPrompt)
				StartCoroutine(HidePrompt());	
		}
		
		_character.BehaviorState.InDialogueZone=false;
		_character.BehaviorState.CurrentDialogueZone=null;
		

	}
}
