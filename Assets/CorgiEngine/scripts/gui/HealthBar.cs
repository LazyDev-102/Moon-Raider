using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Manages the health bar
/// </summary>
public class HealthBar : MonoBehaviour 
{
    public Image Frame;
    public Image Segment1;
	public Image Segment2;
	public Image Segment3;
	public Image Segment4;
	public Image Segment5;
	public Image Segment6;
	public Image Segment7;
	public Image Segment8;
	public Image Segment9;

	private Image[] segments = { null, null, null, null, null, null, null, null, null };
	private Sprite[] details;
    private Sprite[] frames;

    private int oldCap = -1;
	private int oldHealth = -1;

	protected CharacterBehavior _character;

	private float nextWarnTime;

    public enum PlayerRole
    {
        PlayerOne,
        PlayerTwo
    }

    public PlayerRole PlayerNum = PlayerRole.PlayerOne;

    /// <summary>
    /// Initialization, gets the player
    /// </summary>
    protected virtual void Start()
	{
        if(PlayerNum == PlayerRole.PlayerOne)
		    _character = GameManager.Instance.Player;
        else
            _character = GameManager.Instance.PlayerTwo;

        frames = Resources.LoadAll<Sprite>("HUD");
        details = Resources.LoadAll <Sprite> ("small_details");

		//Debug.Log ("S1: " + Segment1);

		nextWarnTime = Time.time;

		segments [0] = Segment1;
		segments [1] = Segment2;
		segments [2] = Segment3;
		segments [3] = Segment4;
		segments [4] = Segment5;
		segments [5] = Segment6;
		segments [6] = Segment7;
		segments [7] = Segment8;
		segments [8] = Segment9;
	}

	public virtual IEnumerator Flicker(Color color)
    {
		Frame.color = color;
		yield return new WaitForSeconds(0.18f);
		Frame.color = Color.white;
		yield return new WaitForSeconds(0.18f);
		Frame.color = color;
		yield return new WaitForSeconds(0.18f);
		Frame.color = Color.white;
	}

    /// <summary>
    /// Every frame, sets the foreground sprite's width to match the character's health.
    /// </summary>
    protected virtual void Update()
	{
        if (_character == null)
        {
            if (PlayerNum == PlayerRole.PlayerOne)
                _character = GameManager.Instance.Player;
            else
                _character = GameManager.Instance.PlayerTwo;
            return;
        }

        if (oldHealth == _character.Health && oldCap == _character.BehaviorParameters.MaxHealth)
        {
			if (_character.Health == 1 && Time.time > nextWarnTime)
			{
				nextWarnTime = Time.time + 3;
				StartCoroutine(Flicker(Color.red));
			}
			return;
        }

        for (int i = 0; i < 9; i++)
        {
            if (segments[i] == null)
                continue;

            segments[i].sprite = details [48];
			segments[i].color = Color.clear;
		}

		int cap = Mathf.CeilToInt (_character.BehaviorParameters.MaxHealth);

        //Debug.Log("Cap = " + cap);

        for (int i = 0; i < cap; i++)
        {
            if (segments[i] == null)
                continue;

            segments[i].color = Color.white;
        }

        //Debug.Log("Health = " + _character.Health);

        for (int i = 0; i < _character.Health; i++)
        {
            if (segments[i] == null)
                continue;

            segments[i].sprite = details[49];
        }

        if(PlayerNum == PlayerRole.PlayerOne)
            Frame.sprite = frames[cap - 5];

		if (cap != oldCap)
			StartCoroutine(Flicker(Color.green));

        oldHealth = _character.Health;
		oldCap = _character.BehaviorParameters.MaxHealth;
	}
	
}
