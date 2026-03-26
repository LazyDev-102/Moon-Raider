using UnityEngine;
using System.Collections;

public class BossSquid : Boss
{
    public float TrackSpeed = 6;
    public float EvadeSpeed = 10;
    public GameObject Bubble;

    private enum StageEnum { Wait, Track, Ink, Bubble, Lay, Dead };
    private StageEnum _stage = StageEnum.Wait;
    private int _stageCount = 1;
    private int _inkTick = 0;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Health _health;
    private AIReact _react;


    private GameObject babyPrefab;
    private GameObject inkPrefab;

    private bool _wasHurt = false;
    private bool _wasEvading = false;
    private bool _wasLaying = false;
    private bool _wasStarted = false;
    private bool _wasDead = false;
    private bool _wasWaiting = false;

    private CameraController sceneCamera;

    // Use this for initialization
    void Start()
    {
        if (CheckDefeated())
            return;

        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _health = GetComponent<Health>();
        _react = GetComponent<AIReact>();

        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        babyPrefab = Resources.Load("Enemies/BabySquid") as GameObject;
        inkPrefab = Resources.Load("Weapons/Ink") as GameObject;

        Bubble.SetActive(false);
    }

    IEnumerator StopWaiting(float duration)
    {
        yield return new WaitForSeconds(duration);      

        SoundManager.Instance.PlayBossMusic();

        Vector3 lockPos = new Vector3(17f, -7f, -10f);

        sceneCamera.LockX = true;
        sceneCamera.FreezeAt(lockPos);
        yield return new WaitForSeconds(0.5f);
        sceneCamera.transform.position = new Vector3(lockPos.x, sceneCamera.transform.position.y, sceneCamera.transform.position.z);
        StartCoroutine(sceneCamera.SpeedUp(0.1f));

        CloseWalls();
        SuckUpGems();

        //Debug.Log("Slide bars out and thaws player");
        StartCoroutine(GUIManager.Instance.SlideBarsOut(3f));
        StartCoroutine(ThawPlayer(3.5f));

        yield return new WaitForSeconds(4f);

        _stage = StageEnum.Track;
        _wasWaiting = true;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        bool dead = _animator.GetBool("Dying");
        if (dead && !_wasDead)
        {
            SoundManager.Instance.PlayRegularMusic();
            OpenWalls();
            _wasDead = true;
            _stage = StageEnum.Dead;
            return;
        }


        // Can only get hurt while tracking
        if (_stage == StageEnum.Wait)
        {
            if (!_wasStarted && _react.Reacting)
            {
                _wasStarted = true;
                StartCoroutine(StopWaiting(0.1f));
            }
        }
        else if (_stage == StageEnum.Track)
        {
            if (_wasWaiting)
            {
                _health.MinDamageThreshold = 1;

                bool hurt = _animator.GetBool("Hurt");
                if (hurt && !_wasHurt)
                {
                    _wasHurt = true;
                    _stage = StageEnum.Ink;
                    return;
                }

                // Track
                Track();
            }
        }
        else if (_stage == StageEnum.Ink)
        {
            _health.MinDamageThreshold = 100;

            _wasHurt = false;

            if (!_wasEvading)
            {
                _animator.SetBool("Inking", true);
                StartCoroutine(StopInking(1f));
            }

            _wasEvading = true;

            Evade();
        }
        else if (_stage == StageEnum.Bubble)
        {
            _health.MinDamageThreshold = 100;

            Bubble.SetActive(true);

            if (!_wasLaying)
            {
                _animator.SetBool("Inking", false);
                StartCoroutine(Lay());
            }
            _wasLaying = true;

        }
        else if (_stage == StageEnum.Lay)
        {
            _health.MinDamageThreshold = 100;
        }
    }


    // Track the player
    void Track()
    {
        Swim(TrackSpeed);
    }


    // Turn in the opposite direction of player while inking
    void Evade()
    {
        Swim(-EvadeSpeed);
        Ink();
    }


    // Spray ink
    void Ink()
    {
        if (_stage != StageEnum.Ink)
            return;

        _inkTick++;

        if (_inkTick == 4)
        {
            GameObject ink = Instantiate(inkPrefab, gameObject.transform.position, gameObject.transform.rotation);
            ink.transform.parent = gameObject.transform.parent;
            _inkTick = 0;
        }
    }


    IEnumerator ThawPlayer(float duration)
    {
        yield return new WaitForSeconds(duration);
        GameManager.Instance.ThawCharacter();
    }


    IEnumerator StopInking(float duration)
    {
        yield return new WaitForSeconds(duration);
        _stage = StageEnum.Bubble;
        _wasEvading = false;
    }


    // Swimming
    private void Swim(float speed)
    {
        /**CharacterBehavior behavior = reaction.Target.GetComponent<CharacterBehavior>();*/

        // This thing is only following the player, so skip the expensive "GetComponent"
        CharacterBehavior behavior = GameManager.Instance.Player;

        if (behavior == null)
            return;

        if (!behavior.BehaviorState.Swimming)
            return;

        float x = speed * Time.deltaTime;

        float deltaX = transform.position.x - behavior.transform.position.x;

        if (deltaX > 0)
            x = -x;

        _sprite.flipX = (x < 0 && Mathf.Abs(deltaX) > 1);

        float y = speed * Time.deltaTime;

        float deltaY = transform.position.y - behavior.transform.position.y;

        if (deltaY > 0)
            y = -y;

        // Don't fly out of water
        if (transform.position.y > -1.5f && y > 0)
            y = 0;
        else if (transform.position.y < -11f && y < 0)
            y = 0;

        Vector2 newPosition = new Vector2(x, y);

        transform.Translate(newPosition, Space.World);
    }


    IEnumerator Lay()
    {
        yield return new WaitForSeconds(2f);

        _animator.SetBool("Laying", true);

        int t = 3 * _stageCount;

        for (var o = 0; o < t; o++)
        {
            StartCoroutine(LayEgg(0.5f * o, o == (t - 1)));
        }
    }


    IEnumerator LayEgg(float duration, bool isLast)
    {
        yield return new WaitForSeconds(duration);

        GameObject baby = Instantiate(babyPrefab, gameObject.transform.position, gameObject.transform.rotation);
        baby.transform.parent = gameObject.transform.parent;

        if (isLast)
        {
            StartCoroutine(NextStage());
        }
    }


    IEnumerator NextStage()
    {
        yield return new WaitForSeconds(2f);

        _animator.SetBool("Laying", false);

        _stageCount++;

        _stage = StageEnum.Track;
        _wasLaying = false;

        Bubble.SetActive(false);
    }
}
