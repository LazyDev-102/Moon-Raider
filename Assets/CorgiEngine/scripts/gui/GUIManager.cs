using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/// <summary>
/// Handles all GUI effects and changes
/// </summary>
public class GUIManager : MonoBehaviour 
{
    private Sprite[] sprites;

	/// the game object that contains the heads up display (avatar, health, points...)
	public GameObject HUD;
	/// the pause screen game object
	public GameObject PauseScreen;	
	/// the time splash gameobject
	public GameObject TimeSplash;
	/// The mobile buttons
	public GameObject Buttons;
	/// The mobile movement pad
	public GameObject Pad;
    public GameObject Analog;
    /// the points counter
    //public Text PointsText;
    /// the pause button
    public Image PauseButton;
    public Image ContextualButton;
    public Image MeleeButton;

    /// the screen used for all fades
    public Image Fader;

    public Image Digit0;
    public Image Digit1;

    private Image DS0;
    private Image DS1;

    /// the jetpack bar
    public GameObject JetPackBar;

    public GameObject CinemaBarTop;
    public GameObject CinemaBarBottom;

    // Dialogue overlay
    public ChatBox ChatBar;
    public HealthBar HealthBar;
    public HealthBar HealthBarTwo;
    public GoalMeter Amulet;
    public Text HealthBarTwoNotice;
    public Text PauseLevelNumber;

    protected static GUIManager _instance;
	
	// Singleton pattern
	public static GUIManager Instance
	{
		get
		{
			if(_instance == null)
				_instance = FindObjectOfType<GUIManager>();
			return _instance;
		}
	}


    public float BarAnimationSpeed = 10;
    private bool BarsShowing = false;
    public Coroutine PlayerTwoFlickerRoutine;

    /// <summary>
    /// Initialization
    /// </summary>
    protected virtual void Start()
	{
        Debug.Log("GUI Manager start: " + Camera.main.aspect);

        sprites = Resources.LoadAll<Sprite>("digits");

        DS0 = Digit0.GetComponent<Image>();
        DS1 = Digit1.GetComponent<Image>();

        // Adjust for iPad
        if (Camera.main.aspect < 1.5f)
        {
            Analog.transform.localScale = 0.5f * Vector3.one;
            RectTransform rt = (RectTransform)Analog.transform;
            rt.anchoredPosition = new Vector3(160, 160, 0);

            Buttons.transform.localScale = 0.4f * Vector3.one;
            RectTransform brt = (RectTransform)Buttons.transform;
            brt.anchoredPosition = new Vector3(-132, 180, 0);
        }

        ContextualButton.enabled = false;

        RefreshPoints();
        Amulet.DisplayPoints(false);
        SetHealthTwoActive(false);
        HealthBarTwoNotice.enabled = false;
    }


    public void SetHealthTwoActive(bool state)
    {
        HealthBarTwo.Frame.color = Color.white;
        HealthBarTwo.gameObject.SetActive(state);
    }


    public IEnumerator FlickerPlayerTwoNotice()
    {
        HealthBarTwoNotice.enabled = true;

        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(0.25f);
            HealthBarTwoNotice.enabled = false;
            yield return new WaitForSeconds(0.25f);
            HealthBarTwoNotice.enabled = true;
        }

        yield return new WaitForSeconds(0.25f);
        HealthBarTwoNotice.enabled = false;

        PlayerTwoFlickerRoutine = null;
    }


    /// <summary>
    /// Sets the HUD active or inactive
    /// </summary>
    /// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
    public virtual void SetHUDActive(bool state)
    {
        if (HUD != null)
        { 
            HUD.SetActive(state);
        }
        //if (PointsText!= null)
        //{ 
        //    PointsText.enabled = state;
        //}
    }

    /// <summary>
    /// Sets the avatar active or inactive
    /// </summary>
    /// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
    public virtual void SetAvatarActive(bool state)
    {
        if (HUD != null)
        {
            HUD.SetActive(state);
        }
    }

    public virtual void SetMobileControlsActive(bool state)
	{
#if UNITY_IOS || UNITY_ANDROID
        Pad.SetActive(state);
		Buttons.SetActive(state);
		PauseButton.enabled = state;
#else
        Pad.SetActive(false);
        Buttons.SetActive(false);
        PauseButton.enabled = false;
#endif
    }

	/// <summary>
	/// Sets the pause.
	/// </summary>
	/// <param name="state">If set to <c>true</c>, sets the pause.</param>
	public virtual void SetPause(bool state)
	{
        if (GlobalVariables.ForceLevelNumber > 1000)
            PauseLevelNumber.text = "ZONE " + GlobalVariables.WorldIndex + "-B";
        else
            PauseLevelNumber.text = "ZONE " + GlobalVariables.WorldIndex + "-" + (GlobalVariables.LevelIndex + 1);

        if (PauseScreen!= null)
    		PauseScreen.SetActive(state);
    }

	/// <summary>
	/// Sets the jetpackbar active or not.
	/// </summary>
	/// <param name="state">If set to <c>true</c>, sets the pause.</param>
	public virtual void SetJetpackBar(bool state)
	{
        if (JetPackBar != null)
        { 
		    JetPackBar.SetActive(state);
        }
    }

	/// <summary>
	/// Sets the time splash.
	/// </summary>
	/// <param name="state">If set to <c>true</c>, turns the timesplash on.</param>
	public virtual void SetTimeSplash(bool state)
	{
        if (TimeSplash != null)
        {
            TimeSplash.SetActive(state);
        }
	}

    /// <summary>
    /// Sets the text to the game manager's points.
    /// </summary>
    public virtual void RefreshPoints()
    {
        DisplayPoints();
    }

    public virtual void RefreshSpecialPoints(bool shouldFlicker)
    {
        Amulet.DisplayPoints(shouldFlicker);
    }

    public virtual void DisplayPoints()
    {
        if (GameManager.Instance.Player == null)
            return;

        if (DS0 == null)
        {
            DS0 = Digit0.GetComponent<Image>();
            DS1 = Digit1.GetComponent<Image>();
        }

        float percent = (float)GameManager.Instance.Points / (float)GameManager.Instance.Player.BehaviorParameters.MaxGems;
        int roundedPoints = (int)(100 * percent);
        if (roundedPoints > 99)
            roundedPoints = 99;

        if (roundedPoints < 1)
        {
            DS0.sprite = sprites[0];
            DS1.sprite = (percent == 0) ? sprites[0] : sprites[1];
        }
        else if (roundedPoints < 10)
        {
            DS0.sprite = sprites[0];
            DS1.sprite = sprites[roundedPoints];
        }
        else if (roundedPoints < 100)
        {
            int ones = roundedPoints % 10;
            int tens = (roundedPoints - ones) / 10;

            DS0.sprite = sprites[tens];
            DS1.sprite = sprites[ones];
        }

        if (GameManager.Instance.Player != null)
        {
            if (percent > 0.9f)
            {
                DS0.color = Color.green;
                DS1.color = Color.green;
            }
            else if(percent < 0.25f)
            {
                DS0.color = Color.red;
                DS1.color = Color.red;
            }
            else if (percent >= 0.25 && percent < 0.5f)
            {
                DS0.color = Color.yellow;
                DS1.color = Color.yellow;
            }
            else
            {
                DS0.color = Color.white;
                DS1.color = Color.white;
            }
        }
    }
	
	/// <summary>
	/// Fades the fader in or out depending on the state
	/// </summary>
	/// <param name="state">If set to <c>true</c> fades the fader in, otherwise out if <c>false</c>.</param>
	public virtual void FaderOn(bool state ,float duration)
	{
        if (Fader!= null)
        { 
		    Fader.gameObject.SetActive(true);
            if (state)
                StartCoroutine(CorgiTools.FadeImage(Fader, duration, new Color(0, 0, 0, 1f)));
            else
                StartCoroutine(CorgiTools.FadeImage(Fader, duration, new Color(0, 0, 0, 0f)));
        }
    }


    public IEnumerator SlideBarsIn()
    {
        float startX = 0;
        float topStartY = 200;
        float bottomStartY = -200;
        float deltaY = 4;

        Image topBar = CinemaBarTop.GetComponent<Image>();
        Image bottomBar = CinemaBarBottom.GetComponent<Image>();

        GameManager.Instance.Player.TakesDamage = false;

        for (int i = 0; i < 40; i++)
        {
            topBar.rectTransform.anchoredPosition = new Vector2(startX, topStartY - i * deltaY);
            bottomBar.rectTransform.anchoredPosition = new Vector2(startX, bottomStartY + i * deltaY);
            yield return new WaitForSeconds(BarAnimationSpeed);
        }

        BarsShowing = true;
    }


    public IEnumerator SlideBarsOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        float startX = 0;
        Image topBar = CinemaBarTop.GetComponent<Image>();
        Image bottomBar = CinemaBarBottom.GetComponent<Image>();

        GameManager.Instance.ThawCharacter();

        if (BarsShowing)
        {
            float topStartY = 40;
            float bottomStartY = -40;
            float deltaY = 4;

            for (int i = 0; i < 40; i++)
            {
                topBar.rectTransform.anchoredPosition = new Vector3(startX, topStartY + i * deltaY, 0);
                bottomBar.rectTransform.anchoredPosition = new Vector2(startX, bottomStartY - i * deltaY);
                yield return new WaitForSeconds(BarAnimationSpeed);
            }

            BarsShowing = false;
            GameManager.Instance.Player.TakesDamage = true;
        }
    }

}
