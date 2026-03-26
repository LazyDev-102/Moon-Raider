using UnityEngine;
using System.Collections;
using JulienFoucher;

public class BossWasp : Boss
{
    public float FlutterSpeed = 4;
    public bool StartBars = true;

    public AudioClip SweepSound;

    public GameObject King;

    public GameObject Left;
    public GameObject Right;
    public GameObject Center;

    public int Number = 0;

    CameraController sceneCamera;
    AIReact _react;
    Animator _animator;
    Health _health;
    AIShootOnSight _shoot;
    AISayThings _sayThings;

    bool FightOver = false;

    /**
     * New pattern:
     * Stage 1
     *  - Sweep right to left in an upside down parabola
     *  - King shoots 3 times from right to left
     *  - Sweep left to right in an upside down parabola
     *  - King shoots 3 times from right to left
     * 
     * Stage 2
     *  - Sweep right to left in a sine wave pattern// zs 
     *  - Sweep let to right in a sine wave pattern
     *  - Wasp shoots spray 3 times from right to left
     *  
     * Stage 4
     *  - Dead bug King is the remains
     * */

    enum Stage
    {
        Wait,
        ParabolaSweep,
        Shoot,
        BackToSweep,
        WaitToSweep,
        SineSweep,
        Spray,
        Dead
    }

    enum SweepPhase
    {
        NoSweep,
        SweepRightToLeft,
        SweepLeftToRight
    }

    Stage _stage;
    SweepPhase _phase;

    Vector3 left = new Vector3(-15f, 27f, 8f);
    Vector3 middle = new Vector3(-5f, 20f, 8);
    Vector3 right = new Vector3(5f, 27f, 8f);

    float top;
    float initialFlutter;
    float d = 0;

    bool _hurt = false;
    bool _wasHurt = false;
    bool _wasReacting = false;
    bool _wasDead = false;
    bool _hasSucked = false;
    bool inFiringPos = false;
    float stopY = 0;


    // this for initialization
    void Start()
    {
        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        _react = GetComponent<AIReact>();
        _health = GetComponent<Health>();
        _animator = GetComponent<Animator>();
        _shoot = GetComponent<AIShootOnSight>();
        _shoot.enabled = false;

        _sayThings = GetComponent<AISayThings>();

        _animator.SetLayerWeight(0, 0);
        _animator.SetLayerWeight(1, 0);
        _animator.SetLayerWeight(Number, 1);

        top = transform.position.y;

        Right.transform.position = right;
        Left.transform.position = left;
        Center.transform.position = middle;

        Right.transform.parent = null;
        Left.transform.parent = null;
        Center.transform.parent = null;

        Right.SetActive(false);
        Left.SetActive(false);
        Center.SetActive(false);

        _stage = Stage.Wait;
        _phase = SweepPhase.NoSweep;

        _health = GetComponent<Health>();
        initialFlutter = FlutterSpeed + Number;

        if (Number == 1)
        {
            _health.Remains = null;
            _health.popResult = Health.PopEnum.None;
        }

        // Stick off screen for dramatic entrance
        if(StartBars)
            transform.Translate(8 * Vector3.up);
    }


    IEnumerator StartFight(float delay)
    {
        yield return new WaitForSeconds(delay);

        _stage = Stage.ParabolaSweep;

        if (transform.position.x > -5)
            _phase = SweepPhase.SweepRightToLeft;
        else
            _phase = SweepPhase.SweepLeftToRight;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.Player == null)
            return;

        _hurt = _animator.GetBool("Hurt");

        switch (_stage)
        {
            case Stage.Wait:

                if (StartBars)
                {
                    if (_react.Reacting && !_wasReacting)
                    {
                        _wasReacting = true;
                        Vector3 pos = new Vector3(-5, 22, 0);
                        sceneCamera.FreezeAt(pos);
                        sceneCamera.LockX = true;

                        _sayThings.SaySomething(0, 5);

                        SoundManager.Instance.PlayBossMusic();
                        CloseWalls();
                        StartCoroutine(GUIManager.Instance.SlideBarsOut(5f));
                        StartCoroutine(StartFight(6));
                    }

                    if (_wasReacting)
                    {
                        Vector2 delta = Move(new Vector3(transform.position.x, top - 1.5f, transform.position.z));
                        if (delta.magnitude < 1)
                        {
                            if (!_hasSucked)
                            {
                                SuckUpGems();
                                _hasSucked = true;
                            }
                        }
                    }
                }
                else
                {
                    transform.Translate(8 * Vector3.up);

                    _stage = Stage.ParabolaSweep;
                    _phase = SweepPhase.NoSweep;
                }

                break;

            case Stage.Dead:

                if(!_wasDead)
                {
                    var king = Instantiate(King, transform.position, Quaternion.identity);
                    king.GetComponent<EnemyController>().SetVerticalForce(16);
                    king.GetComponent<BossKing>().Number = Number;
                    _wasDead = true;
                }

                if (!FightOver)
                {
                    Vector2 delta = Move(new Vector3(transform.position.x, 21f, transform.position.z));

                    if (delta.magnitude <= 0.125f)
                    {
                        FightOver = true;
                        //OpenWalls();
                    }
                }
                break;

            case Stage.ParabolaSweep:
                Sweep();
                break;

            case Stage.Shoot:
                Shoot();
                break;

            case Stage.BackToSweep:
                ResetSweep(0.9f);
                break;

            case Stage.SineSweep:
                Sweep();
                break;

            case Stage.Spray:
                Shoot();
                break;
        }
        //Debug.Log("Stage: " + _stage + " Phase: " + _phase);
    }


    float CurveParabola(float x)
    {
        float y = 0.035f*(Mathf.Pow((x - sceneCamera.transform.position.x), 2)) + 20;

        //var marker = Instantiate(Center, new Vector3(x, y, 0),Quaternion.identity);
        //marker.GetComponent<SpriteRenderer>().color = Color.magenta;

        return y;
    }


    void ResetSweep(float delay)
    {
        if(_health.CurrentHealth <= 0)
        {
            _stage = Stage.Dead;
            return;
        }

        Vector2 delta = Move(new Vector3(transform.position.x, stopY, transform.position.z));

        if (delta.magnitude == 0 && _stage != Stage.WaitToSweep)
        {
            _stage = Stage.WaitToSweep;

            // What phase were we last in? Turn around and fire!
            if (transform.position.x < middle.x)
                _phase = SweepPhase.SweepLeftToRight;
            else
                _phase = SweepPhase.SweepRightToLeft;

            StartCoroutine(StartSineSweep(delay));
        }
    }


    public virtual IEnumerator StartSineSweep(float delay)
    {
        if (_health.CurrentHealth <= 0)
        {
            yield return new WaitForSeconds(0.01f);
            _stage = Stage.Dead;
        }
        else
        {
            yield return new WaitForSeconds(delay);

            d = 0;
            _stage = Stage.SineSweep;
            _wasHurt = false;
        }
    }


    void Sweep()
    {
        if (_health.CurrentHealth <= 0)
        {
            _stage = Stage.Dead;
            return;
        }

        d += FlutterSpeed * Time.deltaTime;

        switch (_phase)
        {
            case SweepPhase.NoSweep:
                Vector2 delta = Move(new Vector3(transform.position.x, 21, transform.position.z));

                if (delta.magnitude == 0)
                {
                    if (transform.position.x > -1)
                        _phase = SweepPhase.SweepRightToLeft;
                    else
                        _phase = SweepPhase.SweepLeftToRight;
                }

                break;

            case SweepPhase.SweepRightToLeft:
                float RnextX = Mathf.Lerp(right.x, left.x, d);
                float RnextY = CurveParabola(transform.position.x);
                Vector2 rDelta = Move(new Vector3(RnextX, RnextY, transform.position.z));

                if (rDelta.magnitude == 0)
                {
                    d = 0;
                    stopY = transform.position.y;

                    if (_hurt && !_wasHurt)
                    {
                        _wasHurt = true;

                        if (_stage == Stage.ParabolaSweep)
                            _stage = Stage.Shoot;
                        else if (_stage == Stage.SineSweep)
                            _stage = Stage.Spray;
                    }
                    else
                    {
                        _phase = SweepPhase.SweepLeftToRight;
                    }
                }

                break;

            case SweepPhase.SweepLeftToRight:

                float LnextX = Mathf.Lerp(left.x, right.x, d);
                float LnextY = CurveParabola(transform.position.x);
                Vector2 lDelta = Move(new Vector3(LnextX, LnextY, transform.position.z));

                if (lDelta.magnitude == 0)
                {
                    d = 0;
                    stopY = transform.position.y;

                    if (_hurt && !_wasHurt)
                    {
                        _wasHurt = true;

                        if (_stage == Stage.ParabolaSweep)
                            _stage = Stage.Shoot;
                        else if (_stage == Stage.SineSweep)
                            _stage = Stage.Spray;
                    }
                    else
                    {
                        _phase = SweepPhase.SweepRightToLeft;
                    }
                }

                break;
        }

        //if (_phase != oldPhase && SweepSound != null)
            //SoundManager.Instance.PlaySound(SweepSound, transform.position);
    }


    Vector2 Move(Vector3 target)
    {
        //Debug.Log(_phase + " " + (transform.position - target).sqrMagnitude);

        float diff = 0.25f;

        float x = FlutterSpeed * Time.deltaTime;

        float deltaX = transform.position.x - target.x;

        if (deltaX > 0)
            x = -x;

        if (Mathf.Abs(deltaX) < diff)
            x = 0;

        if (_stage != Stage.Dead)
        {
            if (_phase == SweepPhase.SweepLeftToRight)
                transform.localScale = new Vector2(-1, 1);
            else if (_phase == SweepPhase.SweepRightToLeft)
                transform.localScale = new Vector2(1, 1);
        }

        float y = 0.5f * FlutterSpeed * Time.deltaTime;

        float deltaY = transform.position.y - target.y;

        if (deltaY > 0)
            y = -y;

        if (Mathf.Abs(deltaY) < diff)
            y = 0;

        Vector2 newPosition = new Vector2(x, y);

        transform.Translate(newPosition, Space.World);

        return newPosition;
    }


    void Shoot()
    {
        if (_hurt && !_wasHurt && inFiringPos)
        {
            if (_health.CurrentHealth > 0)
                _stage = Stage.BackToSweep;
            else
                _stage = Stage.Dead;

            _shoot.StopShooting();
            _shoot.enabled = false;
            inFiringPos = false;
            _wasHurt = true;
            return;
        }

        // What phase were we last in? Turn around and fire!
        if (transform.position.x < middle.x)
            _phase = SweepPhase.SweepLeftToRight;
        else
            _phase = SweepPhase.SweepRightToLeft;

        if (!inFiringPos)
        {
            Vector2 delta = Move(new Vector3(transform.position.x, left.y - 6, transform.position.z));
            inFiringPos = (delta.magnitude == 0);
        }
        else
        {
            if (!_shoot.enabled)
            {
                _shoot.enabled = true;
                StartCoroutine(StartShooting());
            }
        }
    }

    public virtual IEnumerator StartShooting()
    {
        yield return new WaitForSeconds(0.9f);

        _wasHurt = false;
    }
}
