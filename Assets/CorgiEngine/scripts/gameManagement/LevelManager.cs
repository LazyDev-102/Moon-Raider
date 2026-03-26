using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_STANDALONE && !DISABLESTEAMWORKS
using Steamworks;
#elif UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif

/// <summary>
/// Spawns the player, and 
/// </summary>
public class LevelManager : MonoBehaviour
{
	/// Singleton
	public static LevelManager Instance { get; private set; }		
	/// the prefab you want for your player
	[Header("Prefabs")]
	/// Debug spawn	
	public CheckPoint DebugSpawn;
	/// the elapsed time since the start of the level
	public TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}
	
	[Space(10)]
	[Header("Intro and Outro durations")]
	/// duration of the initial fade in
	public float IntroFadeDuration=1f;
	/// duration of the fade to black at the end of the level
	public float OutroFadeDuration=1f;
	[Space(10)]
	public PickerTileMap grid;

    // private stuff
    protected CharacterBehavior _player;
    protected List<CheckPoint> _checkpoints;
    protected int _currentCheckPointIndex;
    protected DateTime _started;
    protected int _savedPoints;
    protected CameraController _cameraController ;

	public CameraController Camera
	{
		get 
		{
			return _cameraController;
		}
	}

	public CharacterBehavior Player
	{
		get
		{
			return _player;
		}

		set
		{
			_player = value;
		}      
	}

    public bool ShowCursor = false;


	/// <summary>
	/// On awake, instantiates the player
	/// </summary>
	public virtual void Awake()
	{
		
		Instance = this;

		Application.targetFrameRate = 60;

        Cursor.visible = ShowCursor;

        _player = GameManager.Instance.Player;

		if (_player == null) {
			Debug.Log ("Warning: No player!");
		}

#if UNITY_IOS
		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
		Social.localUser.Authenticate(success => {
			if (success)
				Debug.Log("Game center authenticated");
			else
				Debug.Log("Failed to authenticate game center");
		});
#endif
	}

	/// <summary>
	/// Initialization
	/// </summary>
	public virtual void Start()
	{
		// storage
		_savedPoints=GameManager.Instance.Points;
		_checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(t => t.transform.position.x).ToList();
		_currentCheckPointIndex = _checkpoints.Count > 0 ? 0 : -1;
		_started = DateTime.UtcNow;

		// we get the camera				
		_cameraController = FindObjectOfType<CameraController>();

		// we get the list of spawn points
		var listeners = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerRespawnListener>();
		foreach(var listener in listeners)
		{
			for (var i = _checkpoints.Count - 1; i>=0; i--)
			{
				var distance = ((MonoBehaviour) listener).transform.position.x - _checkpoints[i].transform.position.x;
				if (distance<0)
					continue;
				
				_checkpoints[i].AssignObjectToCheckPoint(listener);
				break;
			}
		}
	}

	/// <summary>
	/// Every frame we check for checkpoint reach
	/// </summary>
	public virtual void Update()
	{
		/*var isAtLastCheckPoint = _currentCheckPointIndex + 1 >= _checkpoints.Count;
		if (isAtLastCheckPoint)
			return;
		
		var distanceToNextCheckPoint = _checkpoints[_currentCheckPointIndex+1].transform.position.x - _player.transform.position.x;
		if (distanceToNextCheckPoint>=0)
			return;
		
		_checkpoints[_currentCheckPointIndex].PlayerLeftCheckPoint();
		
		_currentCheckPointIndex++;
		_checkpoints[_currentCheckPointIndex].PlayerHitCheckPoint();
		
		_savedPoints = GameManager.Instance.Points;
		_started = DateTime.UtcNow;*/
	}


    public void SaveAchievement(string achievement)
    {
#if UNITY_IOS
		Social.ReportProgress(achievement, 100, (result) => {
			Debug.Log(result ? "Reported achievement" : "Failed to report achievement");
		});
#elif UNITY_STANDALONE
#if !DISABLESTEAMWORKS
        if (SteamManager.Initialized)
        {
            Steamworks.SteamUserStats.SetAchievement(achievement);
            SteamUserStats.StoreStats();
        }
#endif
#endif
	}

    static public void SetWorldIndex()
    {

        GlobalVariables.WorldIndex = (int)Mathf.Floor((GlobalVariables.SavedLevelIndex - 1) / 7) + 1;

        Debug.Log("Set world number to " + GlobalVariables.WorldIndex);
    }


    /// <summary>
    /// Gets the player to the specified level
    /// </summary>
    /// <param name="levelName">Level name.</param>
    public virtual void GotoLevel(string levelName)
	{		
        if (GUIManager.Instance!= null)
        { 
    		GUIManager.Instance.FaderOn(true,OutroFadeDuration);
        }
        StartCoroutine(GotoLevelCo(levelName));
    }

    /// <summary>
    /// Waits for a short time and then loads the specified level
    /// </summary>
    /// <returns>The level co.</returns>
    /// <param name="levelName">Level name.</param>
    protected virtual IEnumerator GotoLevelCo(string levelName)
	{
        if (_player!= null)
        { 
    		_player.Disable();
        }


        if (Time.timeScale > 0.0f)
        { 
            yield return new WaitForSeconds(OutroFadeDuration);
        }
        GameManager.Instance.UnPause();

		if (string.IsNullOrEmpty (levelName))
			SceneManager.LoadScene ("StartScreen", LoadSceneMode.Single);
		else
			SceneManager.LoadScene (levelName, LoadSceneMode.Single);
		
	}

	public void RestartLevel()
    {
        GameManager.Instance.UnPause();

        GlobalVariables.LoadSaved = true;
        
        Initiate.Fade("World" + GlobalVariables.WorldIndex, Color.black, 1.0f);
    }

    public void EndStory()
    {
        GameManager.Instance.UnPause();

        GlobalVariables.LoadSaved = true;

        Initiate.Fade("EndStory", Color.black, 1.0f);
    }


    /// <summary>
    /// Kills the player.
    /// </summary>
    public virtual void KillPlayer()
	{
		StartCoroutine(KillPlayerCo());
	}

    /// <summary>
    /// Coroutine that kills the player, stops the camera, resets the points.
    /// </summary>
    /// <returns>The player co.</returns>
    protected virtual IEnumerator KillPlayerCo()
	{
		_player.Kill();
		_cameraController.FollowsPlayer = false;

		yield return new WaitForSeconds(2f);
		
		_cameraController.FollowsPlayer = true;

//		if (_currentCheckPointIndex!=-1)
//			_checkpoints[_currentCheckPointIndex].SpawnPlayer(_player);

		_started = DateTime.UtcNow;
		GameManager.Instance.SetPoints(_savedPoints);

		LevelManager.Instance.GotoLevel ("World" + GlobalVariables.WorldIndex);
	}


    public void BackToMain()
    {
        LevelManager.Instance.GotoLevel("MainMenu");
    }

    public void GoToCredits()
    {
        LevelManager.Instance.GotoLevel("EndCredits");
    }

    public void GoToKickstarter()
    {
        Application.OpenURL("https://www.kickstarter.com/projects/cascadiagames/moon-raider-fast-paced-2d-action-platform-game?ref=bhbcql");
    }

    public void GoToDemo()
    {
        Cursor.visible = true;

        LevelManager.Instance.GotoLevel("DemoEnd");
    }
}

