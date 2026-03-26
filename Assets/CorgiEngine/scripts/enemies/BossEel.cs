using UnityEngine;
using System.Collections;

public class BossEel : Boss
{
    public float TrackSpeed = 6;
    public float EvadeSpeed = 10;

    private enum StageEnum { Wait, Track, Zap, Dead };
    private StageEnum _stage = StageEnum.Wait;
    private int _stageCount = 0;
    private int _inkTick = 0;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private Health _health;
    private AIReact _react;
    private GiveDamageToPlayer _damage;

    private GameObject zapPrefab;

    private bool _wasHurt = false;
    private bool _wasEvading = false;
    private bool _wasZapping = false;
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
        _damage = GetComponent<GiveDamageToPlayer>();

        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        zapPrefab = Resources.Load("Weapons/ElectroBall") as GameObject;
    }

    IEnumerator StopWaiting(float duration)
    {
        yield return new WaitForSeconds(duration);

        Vector3 lockPos = new Vector3(17f, -7f, -10f);

        sceneCamera.LockX = true;
        sceneCamera.FreezeAt(lockPos);
        yield return new WaitForSeconds(0.5f);
        sceneCamera.transform.position = new Vector3(lockPos.x, sceneCamera.transform.position.y, sceneCamera.transform.position.z);
        StartCoroutine(sceneCamera.SpeedUp(0.1f));

        SoundManager.Instance.PlayBossMusic();
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
                    //Debug.Log("Hurt! Time to zap!");
                    _wasHurt = true;
                    _stage = StageEnum.Zap;
                    return;
                }

                // Track
                Track();
            }
        }
        else if (_stage == StageEnum.Zap)
        {
            _health.MinDamageThreshold = 100;

            if (!_wasZapping)
            {
                _wasZapping = true;
                _animator.SetBool("Zap", true);
                StartCoroutine(Zap());
            }
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
        Zap();
    }


    IEnumerator ThawPlayer(float duration)
    {
        yield return new WaitForSeconds(duration);
        GameManager.Instance.ThawCharacter();
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
        else if (transform.position.y < -11.5f && y < 0)
            y = 0;

        Vector2 newPosition = new Vector2(x, y);

        transform.Translate(newPosition, Space.World);
    }


    IEnumerator Zap()
    {
        yield return new WaitForSeconds(0.75f);

        var magicPrefab = Resources.Load("FX/MagicRing") as GameObject;

        Vector2[] angles = {
            new Vector2 (0, 1),
            new Vector2 (0, -1),
            new Vector2 (1, 0),
            new Vector2 (-1, 0),
            new Vector2 (1, 1),
            new Vector2 (1, -1),
            new Vector2 (-1, 1),
            new Vector2 (-1, -1)
        };

        for (var n = 0; n < 8; n++)
        {
            Vector3 offset = Vector3.zero;

            if (n == 0)
                offset = Vector3.up;
            else if (n == 1)
                offset = Vector3.down;
            else if (n == 2)
                offset = Vector3.right;
            else if (n == 3)
                offset = Vector3.left;

            GameObject zapObj = Instantiate(zapPrefab, gameObject.transform.position + 2 * offset, gameObject.transform.rotation);
            zapObj.transform.parent = gameObject.transform.parent;

            MagicRing mr = zapObj.GetComponent<MagicRing>();
            mr.Direction = angles[n];
        }

        StartCoroutine(NextStage());
    }


    IEnumerator NextStage()
    {
        yield return new WaitForSeconds(1.25f);

        _animator.SetBool("Zap", false);

        _stageCount++;

        Debug.Log("Zap! Stage is now " + _stageCount + " and health is " + _health.CurrentHealth);

        _animator.SetLayerWeight(0, 0);
        _animator.SetLayerWeight(1, 0);
        _animator.SetLayerWeight(2, 0);

        _animator.SetLayerWeight(_stageCount, 1);

        TrackSpeed += 0.5f;
        _damage.DamageToGive += 1;

        _wasHurt = false;
        _wasZapping = false;

        _stage = StageEnum.Track;
    }
}
