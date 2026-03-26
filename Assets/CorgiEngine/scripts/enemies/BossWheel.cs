using UnityEngine;
using System.Collections;

public class BossWheel : Boss
{
    public GameObject WheelObject;
    public int WheelSpeed = 1;
    public AudioClip SlamSound;
    public AudioClip RevSound;
    public AudioClip LandSfx;
    public float FallSpeed = 9;

    SpriteRenderer _wheel;
    SpriteRenderer _pilot;
    Animator _animator;
    AISimpleWalk _aiWalk;
    Health _health;
    Flickers _flicker;
    AIReact _react;
    EnemyController _controller;
    AISayThings sayThings;

    float oldDir = 1;
    bool dead = false;
    private bool landed = false;
    private bool attacking = false;
    private bool started = false;
    private bool wasHurt = false;
    private bool wasDead = false;
    private bool canFall = false;
    private CameraController sceneCamera;

    float oldGravity = 0;
    float initdelay = 0.25f;
    float OrgSpeed = 0;

    // Use this for initialization
    void Start()
    {
        if (CheckDefeated())
            return;

        _pilot = GetComponent<SpriteRenderer>();
        _wheel = WheelObject.GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _aiWalk = GetComponent<AISimpleWalk>();
        _aiWalk.enabled = false;
        _health = GetComponent<Health>();
        _flicker = WheelObject.GetComponent<Flickers>();
        _react = GetComponent<AIReact>();
        _controller = GetComponent<EnemyController>();
        _controller.enabled = false;
        sayThings = GetComponent<AISayThings>();

        oldDir = _aiWalk.Direction.x;
        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        OrgSpeed = _aiWalk.Speed;
    }


    public virtual IEnumerator DropIn(float duration)
    {
        yield return new WaitForSeconds(duration);

        oldGravity = _controller.Parameters.Gravity;
        _controller.Parameters.Gravity = 0;
        StartCoroutine(Drop(1.25f));
    }

    public virtual IEnumerator Drop(float duration)
    {
        yield return new WaitForSeconds(duration);

        _controller.Parameters.Gravity = oldGravity;
        sceneCamera.OverrideTarget = transform;

        canFall = true;
    }


    void Update()
    {
        if (wasDead)
            return;

        if (!landed)
            Fall();
        else
        {
            bool dead = _animator.GetBool("Dying");

            if (dead)
            {
                Destroy(WheelObject);
                wasDead = true;
                StartCoroutine(Dead(1f));
                return;
            }

            bool hurt = _animator.GetBool("Hurt");

            if(hurt && !wasHurt)
            {
                wasHurt = true;
                _health.MinHurtDamage = 100;
                StartCoroutine(CanTakeDamage());
            }

            if (attacking)
                Attack();
        }
    }


    public virtual IEnumerator Dead(float duration)
    {
        yield return new WaitForSeconds(duration);

        Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
        sceneCamera.Shake(ShakeParameters);

        sceneCamera.OverrideTarget = null;

        OpenWalls();

        SoundManager.Instance.PlayRegularMusic();
    }


    void Fall()
    {
        if(!started)
        {
            if(_react.Reacting)
            {
                started = true;
                CloseWalls();

                GameManager.Instance.FreezeCharacter();
                SoundManager.Instance.PlayBossMusic();
                StartCoroutine(DropIn(1.5f));
            }

            return;
        }

        if (landed || !canFall)
            return;

        transform.localScale = Vector3.one;
        transform.Translate(FallSpeed * Time.deltaTime * Vector3.down, Space.World);

        // Scan for floor and stop right oe it once you see it
        if (transform.position.y <= -9)
        {
            landed = true;

            sceneCamera.OverrideTarget = null;
            sceneCamera.FreezeAt(transform.position + Vector3.left);

            StartCoroutine(Bootup());

            transform.position = new Vector3(transform.position.x, -9, transform.position.z);

            if (LandSfx != null)
                SoundManager.Instance.PlaySound(LandSfx, transform.position);

            Vector3 ShakeParameters = new Vector3(0.7f, 0.75f, 1.5f);
            sceneCamera.Shake(ShakeParameters);
        }
    }



    IEnumerator CanTakeDamage()
    {
        yield return new WaitForSeconds(0.5f);

        wasHurt = false;
        _health.MinHurtDamage = 5;
    }



    IEnumerator Bootup()
    {
        yield return new WaitForSeconds(0.5f);

        sayThings.SaySomething(0, 2);

        yield return new WaitForSeconds(2.5f);

        sayThings.SaySomething(1, 2);

        yield return new WaitForSeconds(2.5f);

        StartCoroutine(GUIManager.Instance.SlideBarsOut(1f));

        attacking = true;

        _aiWalk.Walk();
        _aiWalk.enabled = true;

        _controller.enabled = true;
    }


    // Update is called once per frame
    void Attack()
    {
        float multiplier = 1f;
        if(_react.Reacting)
        {
            bool targetIsAhead = ((_react.Target.transform.position.x > transform.position.x)  && _aiWalk.Direction.x > 0)
                            ||  ((_react.Target.transform.position.x < transform.position.x)  &&  _aiWalk.Direction.x < 0);

            if(targetIsAhead)
                multiplier = 2f;
        }

        _aiWalk.Speed = multiplier*OrgSpeed;

        Vector3 rotation = new Vector3(0, 0, -_aiWalk.Direction.x);
        _wheel.transform.Rotate(multiplier * WheelSpeed * Time.deltaTime * rotation);

        if(oldDir != _aiWalk.Direction.x)
        {
            _animator.SetFloat("Direction", oldDir);

            oldDir = _aiWalk.Direction.x;

            Vector3 ShakeParameters = new Vector3(0.35f, 0.5f, 1f);
            sceneCamera.Shake(ShakeParameters);

            if (SlamSound != null)
                SoundManager.Instance.PlaySound(SlamSound, transform.position);

            if (RevSound != null)
                SoundManager.Instance.PlaySound(RevSound, transform.position);
        }

        if (_health.Flickering && !_flicker.Flickering)
        {
            _flicker.Flicker();
        }

        if (_health.CurrentHealth == 0 && !dead)
        {
            dead = true;
            WheelObject.GetComponent<SpriteExploder>().ExplodeSprite();
        }
    }
}
