using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class CreditsController : MonoBehaviour {

    public EasyEndCredits endCredits;
    public Image bgImage;
    public Image fadeImage;
    public Sprite[] images;

    private bool isFadeOut;
    private float fadeOutTime;
    private float changeBgTime;
    private Sprite changeSprite;

    private int playerId = 0;
    private Player player;
    private CharacterController cc;
    private bool canStop = true;

    AudioSource audio;
    bool joyconnected = false;

    private bool isChangeBg { get { return changeSprite != null; } }

    // Use this for initialization
    void Start () {
        Play();


        float delay = GetComponents<AudioSource>()[0].clip.length;

        StartCoroutine(PlaySongTwo(delay + 1));

	}

    public void EndNow()
    {
        Initiate.Fade("MainMenu", Color.black, 1.0f);
    }

    public virtual IEnumerator PlaySongTwo(float duration)
    {
        yield return new WaitForSeconds(duration);

        GetComponents<AudioSource>()[1].Play();

        float delay = GetComponents<AudioSource>()[1].clip.length + 2;

        yield return new WaitForSeconds(delay);

        Initiate.Fade("MainMenu", Color.black, 1.0f);
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
    void Update () {

        if (ReInput.isReady)
        {
            if (player == null)
                Setup();

            bool select = player.GetButton("Stop");

            if (select && canStop)
            {
                Initiate.Fade("MainMenu", Color.black, 1.25f);
                canStop = false;
            }
        }


        if (isFadeOut) {
            fadeOutTime += Time.deltaTime;
            if (fadeOutTime > 3.0f) {
                float alpha = Mathf.Min(1.0f, (fadeOutTime - 3.0f) / 3.0f);
                fadeImage.color = new Color(0, 0, 0, alpha);
                if (alpha == 1.0f) {
                    endCredits.Stop();
                }
            } else if (fadeOutTime > 1.0f) {
                endCredits.scrollSpeed = 0.0f;
            } else {
                endCredits.scrollSpeed = (1.0f - fadeOutTime) * 30.0f;
            }
        }
        if (isChangeBg) {
            changeBgTime += Time.deltaTime;
            if (changeBgTime > 2.0f) {
                bgImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                changeSprite = null;
            } else if (changeBgTime > 1.0f) {
                bgImage.sprite = changeSprite;
                bgImage.color = new Color(1.0f, 1.0f, 1.0f, changeBgTime - 1.0f);
            } else {
                bgImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - changeBgTime);
            }
        }
	}

    public void Play() {
        bgImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        bgImage.sprite = images[0];
        fadeImage.color = new Color(0, 0, 0, 0);
        isFadeOut = false;
        fadeOutTime = 0.0f;
        changeBgTime = 0.0f;
        changeSprite = null;
        //endCredits.scrollSpeed = 30.0f;
        endCredits.Play();
    }

    public void changeBgImame(Sprite sprite, bool first = false) {
        changeSprite = sprite;
        changeBgTime = first ? 1.0f : 0.0f;
    }

    public void FadeOut() {
        isFadeOut = true;
        fadeOutTime = 0.0f;
    }

    public void onCreditEvent(string eventName) {
        switch (eventName) {
        case "image1":
            changeBgImame(images[0], true);
            break;
        case "image2":
            changeBgImame(images[1]);
            break;
        case "image3":
            changeBgImame(images[2]);
            break;
        case "image4":
            changeBgImame(images[3]);
            break;
        case "image5":
            changeBgImame(images[4]);
            break;
        case "image6":
            changeBgImame(images[5]);
            break;
        case "finish":
            FadeOut();
            break;
        }
    }
}
