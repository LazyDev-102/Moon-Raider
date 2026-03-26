using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.IO;
using UnityEngine.SceneManagement;

using Rewired;

//#if UNITY_STANDALONE
//using EasySteamLeaderboard;
//#endif

public enum Menu
{
    Main = 0,
    Mailchimp = 1,
    Mission = 2,
    Mode = 3,
    Slot = 4,
    Leaders = 5,
    Options = 6,
    Confirm = 7,
    Exit = 8
}

public enum Mission
{
    Classic = 0,
    Bonus = 1
}


[RequireComponent(typeof(CharacterController))]
public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    //Active option and bool to check if main menu is active
    private int option = 0;
    private Menu menu = Menu.Main;
    private Menu previous = Menu.Main;

    public Text version;
    public Text GamePadRequirement;

    //Check to move the menu using the keys or only the arrows
    [SerializeField, Tooltip("Check to move the menu using the keys or only the arrows")]
    public bool useKeys = true;

    //Check if using parallax or not
    public bool useParallax = true;

    public Image Fade;

    //Scenes animation
    [SerializeField, Tooltip("Animation speed in seconds")]
    public float animSpeed;

    //Option quantity
    [SerializeField, Tooltip("Introduce all the options in your menu")]
    public string[] options;

    [SerializeField, HideInInspector]
    public Text menuText;
    [SerializeField]
    public GameObject[] activeBackground;

    //Arrows
    public Image ArrowR;
    public Image ArrowL;

    //Menu bar gameobject
    [SerializeField, HideInInspector]
    public GameObject menuBar;

    //Backgrounds Controller
    [SerializeField, HideInInspector]
    public GameObject backgroundsController;

    //Sounds
    [Header("Sounds")]
    [Space(10)]
    public AudioClip Select;
    public AudioClip SceneSelect;
    private AudioSource Audio;

    //Events
    [SerializeField]
    public UnityEvent[] Events;

    //Exit Menu
    [SerializeField, HideInInspector]
    public GameObject exitMenu;

    //Missions menu
    [SerializeField, HideInInspector]
    public ModeMenu missionsMenu;

    //Options menu
    public GameObject optionsMenu;
    public ModeMenu modeMenu;
    public SlotMenu slotMenu;
    public ModeMenu leaderboardMenu;
    public ModeMenu mailchimpMenu;
    //public MailChimp mailchimp;

    private int playerId = 0;
    private Player player;
    private CharacterController cc;
    private bool canMove = true;
    private bool canSelect = true;
    private bool canBack = true;
    private bool canExit = true;
    private bool wasLocked = true;

    void Start()
    {
        Debug.Log("Aspect ratio is " + Camera.main.aspect);

        //ReInput.Reset();
        version.text = Application.version;

#if UNITY_TVOS
        UnityEngine.tvOS.Remote.allowExitToHome = true;
#endif

        Cursor.visible = true;

        Audio = gameObject.GetComponent<AudioSource>();
        instance = this;

        Fade.enabled = false;

        // Reset global vars
        GlobalVariables.LoadSaved = false;
        GlobalVariables.LevelIndex = -1;
        GlobalVariables.ForceLevelNumber = -1;
        GlobalVariables.SavedLevelIndex = -1;
        GlobalVariables.SaveSlot = -1;
        GlobalVariables.StartHealth = 5;
        GlobalVariables.StartEnergy = 0;
        GlobalVariables.WorldIndex = 1;
        GlobalVariables.IsBonus = false;
        GlobalVariables.direction = DirectionEnum.Forwards;
        GlobalVariables.CanMelee = false;
        GlobalVariables.MaxWeapon = 0;
        GlobalVariables.MaxHealth = 5;
        GlobalVariables.TwoPlayer = false;
        GlobalVariables.SavedSpecialGems = 0;

        SoundManager.Instance.ResetMusicPosition();

        // Get the character controller
        cc = GetComponent<CharacterController>();

        //Changes the text corresponding option
        menuText.text = options[0];

//#if UNITY_IOS || UNITY_ANDROID || UNITY_TVOS
//        // No gaem center, options, exit on mobile / tvOS      
//        string[] neweropts = { options[0], options[1] };
//        options = neweropts;
//        UnityEvent[] newEvents = { Events[0], Events[1] };
//        Events = newEvents;
//#elif UNITY_STANDALONE
//        // No achivements overlay
//        string[] newopts = { options[0], options[1], options[3], options[4] };
//        options = newopts;
//        UnityEvent[] newEvents = { Events[0], Events[1], Events[3], Events[4] };
//        Events = newEvents;
//#endif

        JoystickManager.Instance.Reboot();
        JoystickManager.Instance.Mode = JoystickManager.JoystickMode.MainMenu;
        JoystickManager.Instance.Reset();
        admanager.instance.ShowGenericVideoAd();
    }


    void Update()
    {
        //if (GlobalVariables.GameLocked)
        //{
        //    //if (GlobalVariables.SubscriptionService)
        //        menuText.text = "Please activate Google Play Pass";
        //    //else
        //    //    menuText.text = "Game Locked";

        //    ArrowL.enabled = false;
        //    ArrowR.enabled = false;
        //    return;
        //}
        //else
        //{
            menuText.text = options[option];
        //}

        wasLocked = GlobalVariables.GameLocked;

        if (JoystickManager.Instance.playerOne == null)
        {
            Debug.Log("Player one missing");
            return;
        }

        if (menu == Menu.Main)
        {
#if UNITY_TVOS
            if(JoystickManager.Instance.JoystickConnected)
            {
                menuBar.SetActive(true);
                GamePadRequirement.enabled = false;
            }
            else
            {
                menuBar.SetActive(false);
                GamePadRequirement.enabled = true;
            }
#endif

            //Deactivate arrows
            //If the option is less than 1 left arrow deactivated
            if (option < 1)
            {
                ArrowL.enabled = false;
            }
            else
            {
                ArrowL.enabled = true;
            }

            //If the option is the last option deactivate right arrow
            if (option >= options.Length - 1)
            {
                ArrowR.enabled = false;
            }
            else
            {
                ArrowR.enabled = true;
            }

            float moveX = JoystickManager.Instance.playerOne.GetAxis("Nav X");

            if (canMove)
            {
                if (moveX >= 0.25f)
                {
                    moveRight();
                    canMove = false;
                }
                else if (moveX <= -0.25f)
                {
                    moveLeft();
                    canMove = false;
                }
            }
            else
            {
                if (Mathf.Abs(moveX) < 0.25f)
                    canMove = true;
            }

            bool select = JoystickManager.Instance.playerOne.GetButtonDown("Select");

            if (select && canSelect)
            {
                pressEnter();
                canSelect = false;
            }
            else
            {
                if (!select)
                    canSelect = true;
            }

#if UNITY_ANDROID
            bool AndroidBack = Input.GetKey(KeyCode.Escape);
            if (AndroidBack && previous == Menu.Main && canExit)
            {
                canExit = false;
                exitMenuOpen();
            }
            else
            {
                if (!AndroidBack)
                {
                    canExit = true;
                    previous = Menu.Main;
                }
            }
#endif

        }
        else if (menu == Menu.Slot)
        {
            if (slotMenu.Warning.activeSelf == true)
            {
                // SELECT
                bool select = JoystickManager.Instance.playerOne.GetButtonDown("Select");

                if (select && canSelect)
                {
                    startGame();
                    canSelect = false;
                }
                else
                {
                    if (!select)
                        canSelect = true;
                }

                // BACK
                bool back = JoystickManager.Instance.playerOne.GetButtonDown("Back");

#if UNITY_ANDROID
                bool AndroidBack = Input.GetKey(KeyCode.Escape);
                back |= AndroidBack;
#endif

                if (back && canBack)
                {
                    hideWarning();
                }
                else
                {
                    if (!back)
                        canBack = true;
                }
            }
            else
            {
                // HIGHLIGHT         
                float moveY = JoystickManager.Instance.playerOne.GetAxis("Nav Y");

                if (canMove)
                {
                    if (moveY >= 0.25f)
                    {
                        slotMenu.HighlightedSlot--;

                        if (slotMenu.HighlightedSlot < 0)
                            slotMenu.HighlightedSlot = 2;

                        slotMenu.HighlightSlot();

                        canMove = false;
                    }
                    else if (moveY <= -0.25f)
                    {
                        slotMenu.HighlightedSlot++;

                        if (slotMenu.HighlightedSlot > 2)
                            slotMenu.HighlightedSlot = 0;

                        slotMenu.HighlightSlot();

                        canMove = false;
                    }
                }
                else
                {
                    if (Mathf.Abs(moveY) < 0.25f)
                        canMove = true;
                }

                // SELECT
                bool select = JoystickManager.Instance.playerOne.GetButtonDown("Select");

                if (select && canSelect)
                {
                    if (slotMenu.HighlightedSlot == 0)
                        selectSlotA();
                    else if (slotMenu.HighlightedSlot == 1)
                        selectSlotB();
                    else if (slotMenu.HighlightedSlot == 2)
                        selectSlotC();

                    canSelect = false;
                }
                else
                {
                    if (!select)
                        canSelect = true;
                }

                // BACK
                bool back = JoystickManager.Instance.playerOne.GetButtonDown("Back");

#if UNITY_ANDROID
                bool AndroidBack = Input.GetKey(KeyCode.Escape);
                back |= AndroidBack;
#endif

                if (back && canBack)
                {
                    slotMenuClose();
                    canBack = false;
                }
                else
                {
                    if (!back)
                        canBack = true;
                }
            }
        }
        else if (menu == Menu.Exit)
        {
            // SELECT
            bool select = JoystickManager.Instance.playerOne.GetButtonDown("Select");

            if (select && canSelect)
            {
                exitGame();
                canSelect = false;
            }
            else
            {
                if (!select)
                    canSelect = true;
            }

            // BACK
            bool back = JoystickManager.Instance.playerOne.GetButtonDown("Back");

#if UNITY_ANDROID
            bool AndroidBack = Input.GetKey(KeyCode.Escape);
            back |= AndroidBack;
#endif

            if (back && canBack)
            {
                exitMenuClose();
                canBack = false;
            }
            else
            {
                if (!back)
                    canBack = true;
            }
        }
        else if (menu == Menu.Options)
        {
            // SELECT
            bool select = JoystickManager.Instance.playerOne.GetButtonDown("Select");

            if (select && canSelect)
            {
                openOptions();
                canSelect = false;
            }
            else
            {
                if (!select)
                    canSelect = true;
            }

            // BACK
            bool back = JoystickManager.Instance.playerOne.GetButtonDown("Back");

#if UNITY_ANDROID
            bool AndroidBack = Input.GetKey(KeyCode.Escape);
            back |= AndroidBack;
#endif

            if (back && canBack)
            {
                closeOptions();
                canBack = false;
            }
            else
            {
                if (!back)
                    canBack = true;
            }
        }
    }

    //Initiate
    private void initiate()
    {
        //If use parallax is active then instantiate the parallax main bck
        //Else instantiate the normal background
        menu = Menu.Main;
        menuBar.SetActive(true);
    }

    //Press enter or click on option 
    public void pressEnter()
    {
        //if (GlobalVariables.GameLocked)
        //    return;

        Debug.Log("Selected option is " + options[option]);
        menuText.color = new Color(1, 1, 0, 1);

        StartCoroutine(PressEnter(option));
        //Debug.Log(option);
    }

    protected IEnumerator PressEnter(int option)
    {
        yield return new WaitForSeconds(0.05f);

        Events[option].Invoke();
    }

#if UNITY_IOS
    bool isItchio = false;
#elif DISABLESTEAMWORKS
    bool isItchio = true;
#else
        bool isItchio = false;
#endif

    //Function to go foward in the menu
    public void moveRight()
    {
        if (option < options.Length - 1)
        {
            option = option + 1;

            //if (isItchio && option == 2)
            //    option++;

            menuText.text = options[option];

            Audio.clip = Select;
            Audio.Play();
        }
    }

    //Function to go back in the menu
    public void moveLeft()
    {
        if (option > 0)
        {
            option = option - 1;

//            if (isItchio && option == 2)
 //               option--;

            menuText.text = options[option];

            Audio.clip = Select;
            Audio.Play();
        }
    }

    //New Game event
    public void newGame()
    {
        GlobalVariables.LoadSaved = false;
        //openMissions();
        openSlots();
    }

    //Continue
    public void selectScene()
    {
        GlobalVariables.LoadSaved = true;
        openSlots();
    }

    public void showAchievements()
    {
#if UNITY_IOS
        Debug.Log("Nothing to see here");
#endif
    }

    //Opens the exit menu
    public void exitMenuOpen()
    {
        var animEx = exitMenu.GetComponent<Animation>();
        exitMenu.transform.SetAsLastSibling();
        animEx.Play("Fade In");
        menu = Menu.Exit;
    }

    //Closes the exit menu
    public void exitMenuClose()
    {
        Audio.clip = Select;
        Audio.Play();

        var animEx = exitMenu.GetComponent<Animation>();
        animEx.Play("Fade out");
        menu = Menu.Main;
        menuText.color = Color.white;
    }

    //Exit Game
    public void exitGame()
    {
        Application.Quit();
    }

    public void startGame()
    {
#if UNITY_TVOS
        String saveData = PlayerPrefs.GetString("SavedGame" + GlobalVariables.SaveSlot, "");

        if (saveData != "")
        {
            if (GlobalVariables.LoadSaved)
            {
                PickerSave savedGame = JsonUtility.FromJson<PickerSave>(saveData);
                GlobalVariables.LevelIndex = savedGame.SavedLevelIndex;
                GlobalVariables.WorldIndex = savedGame.SavedWorldIndex;
            }
            else
            {
                PlayerPrefs.SetString("SavedGame" + GlobalVariables.SaveSlot, "");

                // Clear out all per-level save data
                for (int w = 1; w <= 10; w++)
                {
                    for (int l = 0; l < 8; l++)
                    {
                        PlayerPrefs.SetString("SavedLevel" + GlobalVariables.SaveSlot + "_" + w + "_" + l, "");
                    }
                }

                GlobalVariables.LevelIndex = 0;
                GlobalVariables.WorldIndex = 1;
            }
        }
        else
        {
            PlayerPrefs.SetString("SavedGame" + GlobalVariables.SaveSlot, "");
            GlobalVariables.LevelIndex = 0;
            GlobalVariables.WorldIndex = 1;
        }
#else
        // If we're starting a new game and a file exists in this slot, delete it
        var filePath = Application.persistentDataPath + "/savedgame" + GlobalVariables.SaveSlot + ".json";
		if (File.Exists(filePath))
		{
			if (GlobalVariables.LoadSaved)
			{
				PickerSave savedGame = JsonUtility.FromJson<PickerSave>(File.ReadAllText(filePath));
				GlobalVariables.LevelIndex = savedGame.SavedLevelIndex;
                GlobalVariables.WorldIndex = savedGame.SavedWorldIndex;
            }
			else
			{
                GlobalVariables.LevelIndex = 0;
                GlobalVariables.WorldIndex = 1;

                File.Delete(filePath);

                // Clear out all per-level save data
                for(int w = 1; w <= 10; w++)
                {
                    for (int l = 0; l < 8;
                        l++)
                    {
                        var levelPath = Application.persistentDataPath + "/savedlevel" + GlobalVariables.SaveSlot + "_" + w + "_" + l + ".json";

                        if (File.Exists(levelPath))
                            File.Delete(levelPath);
                    }
                }
			}
		}
        else
        {
            GlobalVariables.LevelIndex = 0;
            GlobalVariables.WorldIndex = 1;
        }
#endif


#if DEMOMODE
        if (GlobalVariables.WorldIndex > 2)
        {
            Initiate.Fade("DemoEnd", Color.black, 1.0f);
            return;
        }
#endif
        Debug.Log("Opening World " + GlobalVariables.WorldIndex + " From " + GlobalVariables.LevelIndex);

        if (GlobalVariables.WorldIndex != -1 && GlobalVariables.WorldIndex < 11)
        {
            Initiate.Fade("World" + GlobalVariables.WorldIndex, Color.black, 1.0f);
        }
        else
        {
            // Could change this to a menu with all levels as an option?
            Initiate.Fade("EndStory", Color.black, 1.0f);
        }

    }


    public void openMissions()
    {
        RectTransform rt = missionsMenu.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.75f);

        missionsMenu.GetComponent<Animation>().Play("Fade In");
        menu = Menu.Mission;
        previous = Menu.Mode;
    }


    public void missionsClose()
    {
        var animEx = missionsMenu.GetComponent<Animation>();
        animEx.Play("Fade out");

        menu = previous;
        menuText.color = Color.white;
    }

    public void selectClassicMissions()
    {
        Audio.clip = Select;
        Audio.Play();

        missionsClose();
        openMode();
    }


    public void hideWarning()
    {
        slotMenu.Warning.SetActive(false);
    }


    public void openSlots()
    {
#if UNITY_TVOS
        UnityEngine.tvOS.Remote.allowExitToHome = false;
#endif
        Fade.color = new Color(0, 0, 0, 0);
        Fade.enabled = true;

        StartCoroutine(FadeInImage(0.005f, Fade, 0, 0.75f));
        slotMenu.GetComponent<Animation>().Play("Fade In");
        previous = menu;
        menu = Menu.Slot;
        RectTransform rt = slotMenu.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.675f);
        slotMenu.transform.SetAsLastSibling();
        slotMenu.HighlightedSlot = 0;
        slotMenu.HighlightSlot();
        slotMenu.Refresh();
    }

    public void SelectSlot()
    {
        if (GlobalVariables.LoadSaved)
        {
            // Load game, filled slot
            if (isSlotFilled())
                startGame();
            // Load Game, empty slot
            else
            {
                GlobalVariables.LoadSaved = false;
                startGame();
            }
        }
        else
        {
            // New game, filled slot
            if (isSlotFilled())
                slotMenu.ShowWarning();
            // New game, empty slot
            else
                startGame();
        }
    }

    public void selectSlotA()
    {
        Audio.clip = Select;
        Audio.Play();

        GlobalVariables.SaveSlot = 0;

        SelectSlot();
    }

    public void selectSlotB()
    {
        Audio.clip = Select;
        Audio.Play();

        GlobalVariables.SaveSlot = 1;

        SelectSlot();
    }

    public void selectSlotC()
    {
        Audio.clip = Select;
        Audio.Play();

        GlobalVariables.SaveSlot = 2;

        SelectSlot();
    }

    private bool isSlotFilled()
    {
#if UNITY_TVOS
        String saveData = PlayerPrefs.GetString("SavedGame" + GlobalVariables.SaveSlot, "");

        return (saveData != "");
#else
        var filePath = Application.persistentDataPath + "/savedgame" + GlobalVariables.SaveSlot + ".json";
		return File.Exists(filePath);
#endif
	}


    public void slotMenuClose()
    {
        slotMenu.CloseText.color = new Color(1, 1, 0, 1);
        StartCoroutine(ReallyClose(0.1f));
    }

    protected IEnumerator ReallyClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(FadeOutImage(0.005f, Fade, Fade.color.a));

        Audio.clip = Select;
		Audio.Play();

		RectTransform rt = slotMenu.GetComponent<RectTransform>();
		rt.pivot = new Vector2(0.5f, 0.25f);

		var animEx = slotMenu.GetComponent<Animation>();
		animEx.Play("Fade out");

		menu = previous;
        previous = Menu.Slot;
        menuText.color = Color.white;
    }

	public void openMode()
	{
#if UNITY_TVOS
        UnityEngine.tvOS.Remote.allowExitToHome = false;
#endif

		modeMenu.GetComponent<Animation>().Play("Fade In");
		menu = Menu.Mode;
		previous = Menu.Mission;
		RectTransform rt = modeMenu.GetComponent<RectTransform>();
		rt.pivot = new Vector2(0.5f, 0.7f);
		modeMenu.transform.SetAsLastSibling();

		modeMenu.HighlightedSlot = 0;
		modeMenu.HighlightSlot();
	}

	public void selectCasualMode()
	{
		Audio.clip = Select;
		Audio.Play();

		GlobalVariables.SavedLevelIndex = 0;
		openSlots();
	}

	public void selectRogueMode()
	{
		Audio.clip = Select;
		Audio.Play();

		GlobalVariables.SavedLevelIndex = 0;
		openSlots();
	}

	public void selectSuperRogueMode()
	{
		Audio.clip = Select;
		Audio.Play();

		GlobalVariables.SavedLevelIndex = 0;
		openSlots();
	}

	public void modeMenuClose()
	{
#if UNITY_TVOS
        UnityEngine.tvOS.Remote.allowExitToHome = true;
#endif

		Audio.clip = Select;
		Audio.Play();

		RectTransform rt = modeMenu.GetComponent<RectTransform>();
		rt.pivot = new Vector2(0.5f, 0.25f);

		var animEx = modeMenu.GetComponent<Animation>();
		animEx.Play("Fade out");
        menu = Menu.Mission;

        openMissions();
	}

	//Open Options
	public void openOptions()
	{
#if UNITY_TVOS
        UnityEngine.tvOS.Remote.allowExitToHome = false;
#endif

		Audio.clip = Select;
		Audio.Play();

        Fade.color = new Color(0, 0, 0, 0);
        Fade.enabled = true;
        StartCoroutine(FadeInImage(0.005f, Fade, 0, 0.75f));

        RectTransform rt = optionsMenu.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.65f);

        optionsMenu.gameObject.GetComponent<Animation>().Play("Fade In");
		menu = Menu.Options;
		optionsMenu.transform.SetAsLastSibling();
	}

	//Close Options
	public void closeOptions()
	{
#if UNITY_TVOS
        UnityEngine.tvOS.Remote.allowExitToHome = true;
#endif

		Audio.clip = Select;
		Audio.Play();

        StartCoroutine(FadeOutImage(0.005f, Fade, Fade.color.a));

        optionsMenu.gameObject.GetComponent<Animation>().Play("Fade out");
		menu = Menu.Main;
	}

	public void openLeaderboardMenu()
    {      
		leaderboardMenu.GetComponent<Animation>().Play("Fade In");
		menu = Menu.Leaders;
		previous = Menu.Leaders;
		RectTransform rt = leaderboardMenu.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.7f);
        modeMenu.transform.SetAsLastSibling();

		leaderboardMenu.HighlightedSlot = 0;
		leaderboardMenu.HighlightSlot();
    }

    public void selectCasualLeaders()
    {
        Audio.clip = Select;
        Audio.Play();

		GlobalVariables.Leaderboard = "Casual - Best Time";
		Initiate.Fade("Leaderboard", Color.black, 1.0f);
    }

    public void selectRogueLeaders()
    {
        Audio.clip = Select;
        Audio.Play();

		GlobalVariables.Leaderboard = "Rogue - Best Time";
		Initiate.Fade("Leaderboard", Color.black, 1.0f);
    }

    public void selectSuperRogueLeaders()
    {
        Audio.clip = Select;
        Audio.Play();
        
		GlobalVariables.Leaderboard = "Super Rogue - Best Time";
		Initiate.Fade("Leaderboard", Color.black, 1.0f);
    }

    public void leaderMenuClose()
    {
        Audio.clip = Select;
        Audio.Play();

        RectTransform rt = modeMenu.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.25f);

		var animEx = leaderboardMenu.GetComponent<Animation>();
        animEx.Play("Fade out");
        menu = Menu.Main;
    }

    protected IEnumerator FadeInImage(float delay, Image image, float alpha, float targetAlpha)
    {
        yield return new WaitForSeconds(delay);

        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

        //Debug.Log("new alpha is " + alpha);

        if (alpha < targetAlpha)
            StartCoroutine(FadeInImage(delay, image, alpha + 0.025f, targetAlpha));
    }

    protected IEnumerator FadeOutImage(float delay, Image image, float alpha)
    {
        yield return new WaitForSeconds(delay);

        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

        if (alpha > 0)
            StartCoroutine(FadeOutImage(delay, image, alpha - 0.025f));
        else
            image.enabled = false;
    }

    public void openLeaderboard()
	{
//#if UNITY_IOS
		//KTGameCenter.SharedCenter().ShowLeaderboard();
//#elif UNITY_STANDALONE
//		openLeaderboardMenu();
//#endif
	}

	private void OnDestroy()
	{      
		//ReInput.Reset();
	}
}

