using UnityEngine;
using System.Collections;
/// <summary>
/// Coin manager
/// </summary>
/// 
 
public enum DirectionEnum{Backwards, Forwards};

public class Gateway : MonoBehaviour
{
	public AudioClip Sound;
	public AudioClip CloseSound;
	public DirectionEnum Direction = DirectionEnum.Forwards; 
	public bool IsBonus = false;
    public bool Reverse = false;
    public bool Underwater = false;

    private bool open = false;
	private bool locked = false;
	private bool transporting = false;
	private bool occupied = false;
    private bool backwards;
    public BoxCollider2D openTrigger;
    public BoxCollider2D enterTrigger;

    public GameObject Barrier;

    CameraController sceneCamera;



    protected void Update()
    {

    }

    protected void Start()
    {
        GUIManager.Instance.Fader.gameObject.SetActive(true);

        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        sceneCamera.FreezeAt(GetComponentsInChildren<SpriteRenderer>()[3].transform.position, true);

        StartCoroutine(SpawnPlayer());
    }

    public IEnumerator SpawnPlayer()
    {
        yield return new WaitForSeconds(0.025f);
        Unlock();

		//barrier = Barrier.GetComponent<BoxCollider2D>();
		backwards = (!Reverse && transform.localScale.x == -1) || (Reverse && transform.localScale.x == 1);

        Direction = DirectionEnum.Forwards;
        if (backwards)
            Direction = DirectionEnum.Backwards;

        Debug.Log(GlobalVariables.ForceLevelNumber + " " + GlobalVariables.LastLevelNumber);

        if (GameManager.Instance.Player == null)
        {
            locked = LevelVariables.bonusVisited && IsBonus && GameManager.Instance.Grid.LevelNumber < 1000;

            if (backwards && GlobalVariables.direction == DirectionEnum.Forwards ||
                !backwards && GlobalVariables.direction == DirectionEnum.Backwards ||
                IsBonus)
            {
                if (IsBonus && !GlobalVariables.IsBonus || !IsBonus && GlobalVariables.IsBonus)
                {
                    Debug.Log("Bonus ignore");
                }
                else
                {
                    Transform spawnPoint = GetComponentsInChildren<SpriteRenderer>()[3].transform;

                    float offset = backwards ? -0.5f : 0.5f;

                    var prefab = Resources.Load("PlayableCharacters/Ava") as GameObject;
                    var obj = Instantiate(prefab, spawnPoint.position + offset * Vector3.right, Quaternion.identity);
                    sceneCamera.FreezeAt(spawnPoint.position + offset * Vector3.right, true);

                    CharacterBehavior player = obj.GetComponent<CharacterBehavior>();
                    GameManager.Instance.CanMove = false;

                    GameManager.Instance.Player = player;
                    GameManager.Instance.Player.Health = GlobalVariables.StartHealth;
                    GameManager.Instance.SetPoints(GlobalVariables.StartEnergy);
                    GameManager.Instance.SetSpecialPoints(GlobalVariables.SavedSpecialGems);

                    LevelManager.Instance.grid.player = player;
                    LevelManager.Instance.Player = player;
                    LevelManager.Instance.Camera.Setup();

                    // PLAYER TWO
                    if (GlobalVariables.TwoPlayer && GlobalVariables.PlayerTwoHealth > 0)
                    {
                        var averyPrefab = Resources.Load("PlayableCharacters/Aphid") as GameObject;
                        GameObject aphidObj = Instantiate(averyPrefab, GameManager.Instance.Player.transform.position, gameObject.transform.rotation);
                        aphidObj.transform.parent = gameObject.transform.parent;
                        GameManager.Instance.PlayerTwo = aphidObj.GetComponent<CharacterBehavior>();

                        GameManager.Instance.PlayerTwo.Health = GlobalVariables.PlayerTwoHealth;
                        GUIManager.Instance.SetHealthTwoActive(true);
                    }
                    else
                    {
                        GUIManager.Instance.SetHealthTwoActive(false);
                    }

                    GameManager.Instance.Player.Enter();

                    if (GameManager.Instance.PlayerTwo != null)
                        GameManager.Instance.PlayerTwo.Enter();

                    CheckPoint checkpoint = GetComponent<CheckPoint>();
                    checkpoint.SpawnPlayer(player, spawnPoint);
                    checkpoint.transform.parent = transform;

                    if (GameManager.Instance.PlayerTwo != null)
                    {
                        checkpoint.SpawnPlayer(GameManager.Instance.PlayerTwo, spawnPoint);
                        checkpoint.transform.parent = transform;
                    }

                    CheckOpen();
                }

                if (GameManager.Instance.Player)
                {
                    GameManager.Instance.Player.BehaviorParameters.MaxGems = GlobalVariables.MaxGems;
                    GUIManager.Instance.DisplayPoints();

                    if (GUIManager.Instance != null)
                        GUIManager.Instance.FaderOn(false, 1f);
                }
            }
        }
    }
       

	public void CheckOpen()
	{
		var shouldOpen = IsBonus;

		if (!shouldOpen)
			shouldOpen = GlobalVariables.direction != Direction;

		open = shouldOpen;
		transporting = shouldOpen;
		occupied = shouldOpen;

        UpdateAnimator();
    }

    // Switch this to ienumerator
	public void Lock()
	{
		locked = true;
        int maskLayer = LayerMask.NameToLayer("Platforms");
        Barrier.layer = maskLayer;

        UpdateAnimator ();
	}

	public void Unlock()
	{
		locked = false;
        int maskLayer = LayerMask.NameToLayer("Safe");
        Barrier.layer = maskLayer;

        UpdateAnimator ();
	}

	public virtual void OnTriggerExit2D (Collider2D collider)
	{
        CharacterBehavior character = collider.GetComponent<CharacterBehavior>();

        if (character != null && open) {
			occupied = false;
            StartCoroutine (Close (0.25f, character));
        }
    }

	/// <summary>
	/// Triggered when something collides with the coin
	/// </summary>
	/// <param name="collider">Other.</param>
	public virtual void OnTriggerEnter2D (Collider2D collider)
	{
		if (occupied)
		{
			Debug.Log("Door is occupied");
			return;
		}

        if (locked)
        {
            Debug.Log("Door is locked");
            UpdateAnimator();
            return;
        }

        if (collider.IsTouching(openTrigger) && !open) 
		{
			Open();
		} 
		else 
		{
			if (collider.GetComponent<CharacterBehavior> () != null && !transporting && open && collider.IsTouching(enterTrigger))
			{
				transporting = true;

				BoxCollider2D box = GetComponent<BoxCollider2D> ();
				box.enabled = false;

				if (GameManager.Instance.Player.Health <= 0)
				{
					LevelManager.Instance.RestartLevel();
					return;
				}

				GameManager.Instance.Player.Exit ();

				if (IsBonus) 
				{
					int LevelNumber = LevelManager.Instance.grid.Levels [GlobalVariables.LevelIndex];

                    GlobalVariables.IsBonus = true;

                    if (Direction == DirectionEnum.Forwards)
                        LevelManager.Instance.grid.NextLevel();
                    else if (Direction == DirectionEnum.Backwards)
                        LevelManager.Instance.grid.PrevLevel();
                }
				else 
				{
                    GlobalVariables.IsBonus = false;

                    if (Direction == DirectionEnum.Forwards)
						LevelManager.Instance.grid.NextLevel ();
					else if (Direction == DirectionEnum.Backwards)
						LevelManager.Instance.grid.PrevLevel ();
				}
			}
            else if(collider.GetComponent<AISimpleWalk>() != null)
			{
				collider.GetComponent<AISimpleWalk>().ChangeDirection();
			}
		}
	}

	public void Open()
	{
        if (open)
            return;

        open = true;

        if (Sound != null)
			SoundManager.Instance.PlaySound (Sound, transform.position);

		UpdateAnimator ();
	}

	private void UpdateAnimator() 
	{
		Animator _animator = GetComponent<Animator> ();
		if (_animator != null) {
			_animator.SetBool ("Open", open);
			_animator.SetBool ("Locked", locked);
		}
	}

	protected virtual IEnumerator Close(float duration, CharacterBehavior character)
	{
		yield return new WaitForSeconds (duration);

        if (locked)
            Lock();

        if (open)
        {
            if (CloseSound != null)
                SoundManager.Instance.PlaySound(CloseSound, transform.position);

            if (character != null)
                character.SetHorizontalMove(0);

            ReallyClose();
        }
	}

	public void ReallyClose()
	{
		open = false;
		transporting = false;

		//barrier.enabled = true;

		if (GlobalVariables.WorldIndex == 1 && GlobalVariables.LevelIndex == 0 && Direction == DirectionEnum.Backwards)
			Lock ();

		UpdateAnimator ();
	}

}
