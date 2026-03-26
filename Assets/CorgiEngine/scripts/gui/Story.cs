using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Rewired;

[RequireComponent(typeof(CharacterController))]
public class Story : MonoBehaviour
{
	public static Story instance;
	public Text storyText;
    public float LineTime = 3f;
    public float FadeTime = 0.5f;
    public GameObject[] panels;
    public Image Fader;
    public AudioClip EndSound;

    private int playerId = 0;
    private Player player;
    private CharacterController cc;
    private bool canStop = true;

 

    AudioSource music;
    bool joyconnected = false;

    // Use this for initialization
    void Start()
    {
        Cursor.visible = false;

        instance = this;
        music = GetComponent<AudioSource>();

        if (ReInput.isReady)
		{
			// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
			Setup();
		}
        else
        {
            joyconnected = false;
        }

        Fader.color = Color.black;
        for (int i = 0; i < panels.Length; i++)
        {
            StartCoroutine(FadeOutImage(Fader, 1, 0.01f + i * LineTime));
            StartCoroutine(SetPanel(panels[i], 0.01f + i * LineTime));
            StartCoroutine(FadeInImage(Fader, 0, 0.01f + (i+1) * LineTime - 2f * FadeTime));
        }

        StartCoroutine(End(panels.Length * LineTime - FadeTime));

        if (music != null)
            StartCoroutine(FadeOut(music, (panels.Length - 1) * LineTime + 0.5f*LineTime, 0.5f*LineTime));
        else
            Debug.Log("No audio to fade out!");

        //Debug.Log(ReInput.controllers.joystickCount + " joysticks present at Start");
    }


    void Setup()
	{
        if (joyconnected)
            return;

        joyconnected = ReInput.controllers.IsJoystickAssigned(0);

        //Debug.Log("Setup Rewired");
        player = ReInput.players.GetPlayer(playerId);

        player.controllers.maps.LoadMap(ControllerType.Keyboard, 0, 2, 0, true);
        player.controllers.maps.LoadMap(ControllerType.Joystick, 0, 2, 0, true);
	}


    // Update is called once per frame
    void Update()
    {
		if (ReInput.isReady)
		{
			if (player == null)
				Setup();

			bool select = player.GetButton("Stop");

			if (select && canStop)
			{
                if (music != null)
                    StartCoroutine(FadeOut(music, 0.05f, 0.75f));
                else
                    Debug.Log("No audio to fade out!");

                StartCoroutine(End(0.8f));
				canStop = false;
			}
		}
    }

	protected IEnumerator End(float delay)
	{
		yield return new WaitForSeconds(delay);

        if (GlobalVariables.WorldIndex != 11)
        {
            if (EndSound != null)
                SoundManager.Instance.PlaySound(EndSound, transform.position);

            Initiate.Fade("MainMenu", Color.white, 1.25f);
        }
        else
        {
            Initiate.Fade("EndCredits", Color.black, 1.25f);
        }
    }


    public IEnumerator FadeOut(AudioSource audioSource, float delay, float FadeTime)
    {
        yield return new WaitForSeconds(delay);

        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }
        audioSource.Stop();
    }

    public void Stop()
	{
        StartCoroutine(End(0.1f));
	}

	protected IEnumerator SetPanel(GameObject panel, float delay)
	{
		yield return new WaitForSeconds(delay);

        for (int i = 0; i < panels.Length; i++)
            panels[i].SetActive(false);

        panel.SetActive(true);
    }

    protected IEnumerator FadeInImage(Image image, float alpha, float addedDelay)
    {
        yield return new WaitForSeconds(FadeTime / 80 + addedDelay);

        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

        if (alpha < 1)
            StartCoroutine(FadeInImage(image, alpha + 0.025f, 0));
    }

    protected IEnumerator FadeOutImage(Image image, float alpha, float addedDelay)
    {
        yield return new WaitForSeconds(FadeTime / 80 + addedDelay);

        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

        if (alpha > 0)
            StartCoroutine(FadeOutImage(image, alpha - 0.025f, 0));
    }
}
