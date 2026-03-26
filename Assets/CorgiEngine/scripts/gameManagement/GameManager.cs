using UnityEngine;
using System.Collections;
/// <summary>
/// The game manager is a persistent singleton that handles points and time
/// </summary>
public class GameManager : PersistentHumbleSingleton<GameManager>
{		
	/// the current number of game points
	public int Points { get; private set; }
    public int SpecialPoints { get; private set; }

    public float PointPercentage
    {
        get
        {
            return (float)GameManager.Instance.Points / (float)GameManager.Instance.Player.BehaviorParameters.MaxGems;
        }
    }

	/// the current time scale
	public float TimeScale { get; private set; }
	/// true if the game is currently paused
	public bool Paused { get; set; } 
	/// true if the player is not allowed to move (in a dialogue for example)
	public bool CanMove = false;
	/// the current player
	public CharacterBehavior Player { get; set; }
    public CharacterBehavior PlayerTwo { get; set; }

    public AudioClip PointsUpSfx;
	public AudioClip PointsDownSfx;

    public PickerTileMap Grid;

    // storage
    protected float _savedTimeScale = 1f;

    private bool canPause = false;

    Coroutine AddRoutine;

    /// <summary>
    /// this method resets the whole game manager
    /// </summary>
    public virtual void Reset()
	{
        Player = null;
		Points = 0;
		TimeScale = 1f;
		Paused = false;
		CanMove = false;
		GUIManager.Instance.RefreshPoints ();
	}
    private void Start()
    {
        //admanager.instance.ShowGenericVideoAd();

    }
    void Update()
    {
#if UNITY_ANDROID
        bool AndroidBack = Input.GetKey(KeyCode.Escape);
        bool AndroidMenu = Input.GetKey(KeyCode.Menu);

        if (canPause && (AndroidBack || AndroidMenu))
        {
            canPause = false;
            Instance.Pause();
        }
        else if (!AndroidBack && !AndroidMenu)
        {
            canPause = true;
        }
#endif
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!Instance.Paused && !hasFocus)
            Instance.Pause();
    }

    public IEnumerator BeamOut(float delay, bool autoBeamIn = false)
    {
        yield return new WaitForSeconds(delay);

        if (!autoBeamIn)
        {
            var np = GameManager.Instance.PlayerTwo.transform.position;
            var beamFab = Resources.Load("FX/AphidBeam") as GameObject;
            var beamObj = Instantiate(beamFab, new Vector3(np.x, np.y - 0.5f, 0), Quaternion.identity);

            float time = beamObj.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).length;
            Destroy(beamObj, time);
        }

        if(Instance.PlayerTwo.gameObject != null)
            Destroy(Instance.PlayerTwo.gameObject, 0.01f);

        GlobalVariables.TwoPlayer = false;
        GameManager.Instance.PlayerTwo.OnInvisible(false);
        GameManager.Instance.PlayerTwo = null;
        GUIManager.Instance.SetHealthTwoActive(false);

        if (autoBeamIn)
            StartCoroutine(BeamIn(0.015f));
    }


    public IEnumerator BeamIn(float delay)
    {
        yield return new WaitForSeconds(delay);

        var averyPrefab = Resources.Load("PlayableCharacters/Aphid") as GameObject;
        GameObject aphidObj = Instantiate(averyPrefab, GameManager.Instance.Player.transform.position, gameObject.transform.rotation);
        aphidObj.transform.parent = aphidObj.transform.parent;
        GameManager.Instance.PlayerTwo = aphidObj.GetComponent<CharacterBehavior>();

        GameManager.Instance.PlayerTwo.Health = 5;
        GUIManager.Instance.SetHealthTwoActive(true);
        GlobalVariables.TwoPlayer = true;
        GameManager.Instance.PlayerTwo.PlayBeamSound();

        var np = GameManager.Instance.PlayerTwo.transform.position;

        var beamFab = Resources.Load("FX/AphidBeam") as GameObject;
        var beamObj = Instantiate(beamFab, new Vector3(np.x, np.y - 1, 0), Quaternion.identity);

        float time = beamObj.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).length;
        Destroy(beamObj, time);
    }

    public virtual void AddSpecialPoints(int pointsToAdd)
    {
        if (pointsToAdd == 1)
        {
            SpecialPoints += pointsToAdd;

            if (SpecialPoints > Instance.Player.BehaviorParameters.MaxSpecialGems)
                SpecialPoints = Instance.Player.BehaviorParameters.MaxSpecialGems;

            if (SpecialPoints < 0)
                SpecialPoints = 0;

            GUIManager.Instance.RefreshSpecialPoints(true);
            GUIManager.Instance.Amulet.PlaySound();
        }
        else
        {
            float d = 0.05f;
            for(int i = 0; i < pointsToAdd; i++)
            {
                if (i % 8 == 0)
                {
                    GUIManager.Instance.Amulet.PlaySound();
                    StartCoroutine(AddSpecialPoint(d * (i + 1), true));
                }
                else
                {
                    StartCoroutine(AddSpecialPoint(d * (i + 1), false));
                }

                if (GameManager.Instance.SpecialPoints >= 200)
                    break;
            }
        }
    }

    protected virtual IEnumerator AddSpecialPoint(float delay, bool shouldFlicker)
    {
        yield return new WaitForSeconds(delay);

        SpecialPoints += 1;

        GUIManager.Instance.RefreshSpecialPoints(shouldFlicker);
    }

    /// <summary>
    /// Adds the points in parameters to the current game points.
    /// </summary>
    /// <param name="pointsToAdd">Points to add.</param>
    public virtual void AddPoints(int pointsToAdd)
	{
        if (AddRoutine == null)
            AddRoutine = StartCoroutine(AddPoint(pointsToAdd));
        else
            AddPointsInstant(pointsToAdd);
	}


    public virtual void AddPointsInstant(int pointsToAdd)
    {
		Points += pointsToAdd;

		if (Points > Instance.Player.BehaviorParameters.MaxGems)
			Points = Instance.Player.BehaviorParameters.MaxGems;

		if (Points < 0)
			Points = 0;

        GUIManager.Instance.RefreshPoints();
	}


	protected virtual IEnumerator AddPoint(int points)
	{
        int startPoints = Points;
        Points += points;

        if (Points > Instance.Player.BehaviorParameters.MaxGems)
            Points = Instance.Player.BehaviorParameters.MaxGems;

        if (Points < 0)
            Points = 0;

        while (startPoints < Points)
		{
			yield return new WaitForSeconds (0.1f);

			int step = 1;
			if (Points - startPoints > 10)
				step = 10;

            startPoints += step;

			GUIManager.Instance.DisplayPoints();
		}

        yield return new WaitForSeconds(0.1f);

        GUIManager.Instance.RefreshPoints();

        AddRoutine = null;
	}
	
	/// <summary>
	/// use this to set the current points to the one you pass as a parameter
	/// </summary>
	/// <param name="points">Points.</param>
	public virtual void SetPoints(int points)
	{
		Points = points;
		GUIManager.Instance.RefreshPoints ();
	}

    public virtual void SetSpecialPoints(int points)
    {
        SpecialPoints = points;
        GUIManager.Instance.RefreshSpecialPoints(false);
    }

    /// <summary>
    /// sets the timescale to the one in parameters
    /// </summary>
    /// <param name="newTimeScale">New time scale.</param>
    public virtual void SetTimeScale(float newTimeScale)
	{
		_savedTimeScale = Time.timeScale;
		Time.timeScale = newTimeScale;
	}
	
	/// <summary>
	/// Resets the time scale to the last saved time scale.
	/// </summary>
	public virtual void ResetTimeScale()
	{
		Time.timeScale = _savedTimeScale;
	}
	
	/// <summary>
	/// Pauses the game or unpauses it depending on the current state
	/// </summary>
	public virtual void Pause()
	{	
		// if time is not already stopped		
		if (Time.timeScale > 0.0f)
		{
            Debug.Log("Pausing");
			Cursor.visible = true;
			Instance.SetTimeScale(0.0f);
			Instance.Paused = true;
			GUIManager.Instance.SetPause(true);

        }
        else
		{
            UnPause();
		}		
	}

    /// <summary>
    /// Unpauses the game
    /// </summary>
    public virtual void UnPause()
    {
        Debug.Log("Unpausing");
        Cursor.visible = false;
        Instance.ResetTimeScale();
        Instance.Paused = false;
        if (GUIManager.Instance!= null)
        { 
            GUIManager.Instance.SetPause(false);
        }
    }
	
	/// <summary>
	/// Freezes the character.
	/// </summary>
	public virtual void FreezeCharacter()
	{
		Player.SetHorizontalMove(0);
		Player.SetVerticalMove(0);
        Player.Controller.SetHorizontalForce(0);
        Player.Controller.SetVerticalForce(0);
		Player.shoot.ShootStop();

        GUIManager.Instance.SetMobileControlsActive(false);

        if (PlayerTwo != null)
        {
            PlayerTwo.SetHorizontalMove(0);
            PlayerTwo.SetVerticalMove(0);
            PlayerTwo.Controller.SetHorizontalForce(0);
            PlayerTwo.Controller.SetVerticalForce(0);
            PlayerTwo.shoot.ShootStop();
        }

		Instance.CanMove = false;
	}	

	public virtual void ThawCharacter()
	{
		Player.SetHorizontalMove(0);
		Player.SetVerticalMove(0);
		Instance.CanMove = true;

        GUIManager.Instance.SetMobileControlsActive(true);
    }
}
