using UnityEngine;
using SpeechBubbleManager = VikingCrew.Tools.UI.SpeechBubbleManager;
using VikingCrewDevelopment.Demos;

public class AISayThings : MonoBehaviour
{
    [System.Serializable]
    public class ThingToSay
    {
        [Multiline]
        public string thingToSay;
        public SpeechBubbleManager.SpeechbubbleType type;
    }

    public ThingToSay[] thingsToSay;

    [Header("Leave as null if you just want center of character to emit speechbubbles")]
    public Transform mouth;
    public float timeBetweenSpeak = 5f;
    public bool doTalkOnYourOwn = true;
    private float timeToNextSpeak;

    private int thingToSayNext = 0;


    // Use this for initialization
    void Start()
    {
        timeToNextSpeak = timeBetweenSpeak;
    }

    // Update is called once per frame
    void Update()
    {
        //timeToNextSpeak -= Time.deltaTime;

        //if (doTalkOnYourOwn && timeToNextSpeak <= 0 && thingsToSay.Length > 0)
        //    SaySomething();
    }

    public void SaySomething(int index = -1, int duration = 3)
    {
        if (thingToSayNext >= thingsToSay.Length && index == -1)
            thingToSayNext = 0;
        else
            thingToSayNext = index;

        string message = thingsToSay[thingToSayNext].thingToSay;
        SpeechBubbleManager.SpeechbubbleType type = thingsToSay[thingToSayNext].type;

        if(index == -1)
            thingToSayNext++;
        SaySomething(message, type, duration);
    }

    public void SaySomething(string message, SpeechBubbleManager.SpeechbubbleType speechbubbleType, int duration)
    {
        if (mouth == null)
            SpeechBubbleManager.Instance.AddSpeechBubble(transform, message, speechbubbleType, duration);
        else
            SpeechBubbleManager.Instance.AddSpeechBubble(mouth, message, speechbubbleType, duration);

        timeToNextSpeak = timeBetweenSpeak;
    }

}