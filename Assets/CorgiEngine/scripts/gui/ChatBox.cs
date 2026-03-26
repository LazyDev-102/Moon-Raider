using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChatBox : MonoBehaviour
{
    public Sprite[] Sprites;
    public float AnimationSpeed;
    public Image Box;
    public Image Bar;
    public Text ChatName;
    public Text ChatText;
    public string[] ThingsToSay;

    public AudioClip SlideUpSound;
    public AudioClip SlideDownSound;

    public AudioClip Jargon1Sound;
    public AudioClip Jargon2Sound;

    bool Showing;

    string shootButton, jumpButton, meleeButton;

    public enum SpeakMode { Intro, MoreHealth, MoreGems, MoreDamage, NewMelee, TeachMelee, SpecialGems, SpecialGemsMax };

    Image Amulet;

    float startX
    {
        get
        {
            return Bar.rectTransform.anchoredPosition.x;
        }
    }

    public IEnumerator SlideUp(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (SlideUpSound != null)
            SoundManager.Instance.PlaySound(SlideUpSound, transform.position);

        float startY = -70;
        float deltaY = 3.5f;

        for(int i = 0; i < 41; i++)
        {
            Bar.rectTransform.anchoredPosition = new Vector3(startX, startY + i * deltaY, 0);
            //Amulet.rectTransform.anchoredPosition = new Vector3(Amulet.rectTransform.anchoredPosition.x, startY + i * deltaY - 70, 0);
            yield return new WaitForSeconds(AnimationSpeed);
        }
    }


    public IEnumerator SlideDown()
    {
        if (Showing)
        {
            if (SlideDownSound != null)
                SoundManager.Instance.PlaySound(SlideDownSound, transform.position);

            float startY = 70;
            float deltaY = -3.5f;

            for (int i = 0; i < 41; i++)
            {
                Bar.rectTransform.anchoredPosition = new Vector3(startX, startY + i * deltaY, 0);
                //Amulet.rectTransform.anchoredPosition = new Vector3(Amulet.rectTransform.anchoredPosition.x, startY + i * deltaY - 70, 0);
                yield return new WaitForSeconds(AnimationSpeed);
            }

            Showing = false;
        }
    }


    public IEnumerator Smile()
    {
        yield return new WaitForSeconds(AnimationSpeed);
        Box.sprite = Sprites[0];
    }


    public IEnumerator Chat(float delay, int index, string[] varName, string[] value, int start = 0)
    {
        yield return new WaitForSeconds(delay);

        string thingToSay = ThingsToSay[index];

        for (int i = 0; i < varName.Length; i++)
        {
           //Debug.Log("Replacing " + "{ " + varName[i] + " } with " + value[i]);
            thingToSay = thingToSay.Replace("{ " + varName[i] + " }", value[i]);
        }

        ChatText.text = thingToSay;

        if (index % 2 == 0)
        {
            if (Jargon1Sound != null)
                SoundManager.Instance.PlaySound(Jargon1Sound, transform.position);
        }
        else
        {
            if (Jargon2Sound != null)
                SoundManager.Instance.PlaySound(Jargon2Sound, transform.position);
        }

        for (int h = 1; h < 8; h++)
        {
            for (int i = start; i < (start + 5); i++)
            {
                Box.sprite = Sprites[i];
                yield return new WaitForSeconds(2*AnimationSpeed);
            }
        }
    }


    // Use this for initialization
    void Start()
    {
        if (Application.platform == RuntimePlatform.Switch)
        {
            shootButton = "Y";
            jumpButton = "B";
            meleeButton = "A";
        }
        else if (Application.platform == RuntimePlatform.PS4)
        {
            shootButton = "square";
            jumpButton = "X";
            meleeButton = "circle";
        }
        else
        {
#if UNITY_IOS || UNITY_ANDROID
                shootButton = "X";
                jumpButton = "A";
                meleeButton = "B";
#elif UNITY_STANDALONE
            if (JoystickManager.Instance.JoystickConnected == true)
            {
                shootButton = "X";
                jumpButton = "A";
                meleeButton = "B";
            }
            else
            {
                shootButton = "C or J";
                jumpButton = "SPACE";
                meleeButton = "V or K";
            }
#else
            shootButton = "X";
            jumpButton = "A";
            meleeButton = "B";
#endif
        }

        Amulet = GUIManager.Instance.Amulet.Frame;
    }


    public IEnumerator ChatStart(float delay, SpeakMode mode)
    {
        yield return new WaitForSeconds(delay);

        string inputType = "button";

#if UNITY_STANDALONE
        if (JoystickManager.Instance.JoystickConnected == false)
            inputType = "key";
#endif

        if (!Showing && ThingsToSay.Length > 0)
        {
            Showing = true;

            ChatName.text = "Dr. Cavor:";
            Box.sprite = Sprites[0];

            if (mode == SpeakMode.Intro)
            {
                ChatText.text = ThingsToSay[0];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform             
                string[] variables = new string[] { "input", "inputType" };
                string[] values1 = new string[] { jumpButton, inputType };
                string[] values2 = new string[] { shootButton, inputType };
                string[] values3 = new string[] { "", "" };

                StartCoroutine(Chat(0.1f, 0, variables, values3));
                StartCoroutine(Chat(3, 1, variables, values1)); // jump
                StartCoroutine(Chat(6, 2, variables, values2)); // shoot
                StartCoroutine(Chat(9, 3, variables, values3));

                StartCoroutine(SlideUp(12));
            }
            else if (mode == SpeakMode.MoreDamage)
            {
                int startIndex = 8;

                ChatText.text = ThingsToSay[startIndex];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform
                string[] variables = new string[] { "na" };
                string[] values = new string[] { "" };

                StartCoroutine(Chat(0.1f, startIndex, variables, values));

                StartCoroutine(SlideUp(4));
            }
            else if (mode == SpeakMode.MoreGems)
            {
                int startIndex = 7;

                ChatText.text = ThingsToSay[startIndex];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform
                string[] variables = new string[] { "na" };
                string[] values = new string[] { "" };

                StartCoroutine(Chat(0.1f, startIndex, variables, values));

                StartCoroutine(SlideUp(4));
            }
            else if (mode == SpeakMode.MoreHealth)
            {
                int startIndex = 4;

                ChatText.text = ThingsToSay[startIndex];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform
                string[] variables = new string[] { "na" };
                string[] values = new string[] { "" };

                StartCoroutine(Chat(0.1f, startIndex, variables, values));

                StartCoroutine(SlideUp(4));
            }
            else if (mode == SpeakMode.NewMelee)
            {
                int startIndex = 5;

                ChatText.text = ThingsToSay[startIndex];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform
                string[] variables = new string[] { "input", "inputType" };
                string[] values1 = new string[] { "", "" };
                string[] values2 = new string[] {meleeButton, inputType };

                StartCoroutine(Chat(0.1f, startIndex, variables, values1));
                StartCoroutine(Chat(3f, startIndex + 1, variables, values2));

                StartCoroutine(SlideUp(6));
            }
            else if (mode == SpeakMode.TeachMelee)
            {
                int startIndex = 9;

                ChatText.text = ThingsToSay[startIndex];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform
                string[] variables = new string[] { "input", "inputType" };
                string[] values = new string[] { meleeButton, inputType };

                StartCoroutine(Chat(0.1f, startIndex, variables, values));

                StartCoroutine(SlideUp(4));
            }
            else if (mode == SpeakMode.SpecialGems)
            {
                int startIndex = 10;

                ChatText.text = ThingsToSay[startIndex];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform
                string[] variables = new string[] { "total" };
                string[] values = new string[] { "" + GameManager.Instance.Player.BehaviorParameters.MaxSpecialGems };

                StartCoroutine(Chat(0.1f, startIndex, variables, values));

                StartCoroutine(SlideUp(4));
            }
            else if (mode == SpeakMode.SpecialGemsMax)
            {
                int startIndex = 11;

                ChatText.text = ThingsToSay[startIndex];

                StartCoroutine(SlideDown());

                // TODO: Change the button text per platform
                string[] variables = new string[] { "na" };
                string[] values = new string[] { "" };

                StartCoroutine(Chat(0.1f, startIndex, variables, values));
                StartCoroutine(Chat(3f, startIndex + 1, variables, values));

                StartCoroutine(SlideUp(7));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
