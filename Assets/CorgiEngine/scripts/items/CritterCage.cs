using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class CritterCage : Switchable
{
    public GameObject Cage;
    public GameObject CritterRocket;
    public GameObject Reward;

    public float RocketSpeed = 3;

    public AudioClip CheerSound;
    public AudioClip TakeOffSound;

    SpriteRenderer critter;
    Animator critterAnimator;
    Animator cageAnimator;
    Flickers flicker;
    AISayThings sayThings;
    AIReact react;

    bool TakeOff = false;
    bool opened = false;
    GameObject rocket;
    float rocketSpeed = 1f;
    bool reacted = false;


    // Use this for initialization
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        critter = GetComponent<SpriteRenderer>();
        critterAnimator = GetComponent<Animator>();
        cageAnimator = Cage.GetComponent<Animator>();
        flicker = Cage.GetComponent<Flickers>();

        sayThings = GetComponent<AISayThings>();
        react = GetComponent<AIReact>();

        react.enabled = !LevelVariables.critterFound;
        critter.enabled = !LevelVariables.critterFound;
        Cage.SetActive(!LevelVariables.critterFound);
        GetComponent<BoxCollider2D>().enabled = !LevelVariables.critterFound;
    }

    // Update is called once per frame
    void Update()
    {
        if(TakeOff)
        {
            rocket.transform.Translate(new Vector3(0, rocketSpeed * Time.deltaTime, 0));
            rocketSpeed += RocketSpeed * Time.deltaTime;

            if (rocketSpeed > 15)
                rocketSpeed = 15;
        }

        if(!reacted && react.Reacting)
        {
            reacted = true;
            sayThings.SaySomething(0, 3);
        }
    }


    override public void Flicker()
    {
        flicker.Flicker();
    }


    override public IEnumerator Open(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!opened)
        {
            opened = true;
            LevelVariables.critterFound = true;

            //string achievement = "mr.achievements.critterfound" + GlobalVariables.WorldIndex + "_" + GlobalVariables.LevelIndex;
            //LevelManager.Instance.SaveAchievement(achievement);

            AnalyticsEvent.Custom("critter_found", new Dictionary<string, object>
            {
                { "level", GlobalVariables.LevelIndex },
                { "world", GlobalVariables.WorldIndex },
                { "time_elapsed", Time.timeSinceLevelLoad }
            });

            critterAnimator.SetBool("Celebrate", true);
            cageAnimator.SetBool("Disable", true);
            react.enabled = false;

            if (CheerSound != null)
                SoundManager.Instance.PlaySound(CheerSound, transform.position);

            sayThings.SaySomething(1, 3);

            yield return new WaitForSeconds(duration + 1);

            Flicker();

            critter.enabled = false;
            cageAnimator.SetBool("Detach", true);

            if (TakeOffSound != null)
                SoundManager.Instance.PlaySound(TakeOffSound, transform.position);

            rocket = Instantiate(CritterRocket, transform.position, transform.rotation);
            rocket.GetComponent<Animator>().SetBool("Celebrate", true);

            TakeOff = true;

            yield return new WaitForSeconds(0.5f);

            int maskLayer = LayerMask.NameToLayer("Enemies");
            gameObject.layer = maskLayer;
            Cage.layer = maskLayer;
            Reward.SetActive(true);
            GetComponent<BoxCollider2D>().enabled = false;

            yield return new WaitForSeconds(10);

            TakeOff = false;
            CritterRocket.SetActive(false);
        }
    }
}
