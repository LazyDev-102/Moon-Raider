using UnityEngine;
using System.Collections;
using JulienFoucher;

public class BossKing : Boss
{
    public AudioClip BeamSound;
    public AudioClip DashSound;
    public AudioClip LandSound;
    public AudioClip DeadSound;

    public GameObject Beam;
    public GameObject Beam2;
    public GameObject BeamEnd;

    public GameObject Wasp;
    public GameObject DischargeEffect;

    public int Number = 0;

    EnemyController _controller;
    Animator _animator;
    Health _health;
    AISimpleWalk _walk;
    AIShootOnSight _ai;

    bool _wasDead = false;

    CameraController sceneCamera;

    enum Stage
    {
        Jump,
        Fall,
        Bazooka,
        Shield,
        Beam,
        Dash,
        Dead
    }

    Stage _stage = Stage.Jump;


    int ShieldAttackCount = 0;


    // Use this for initialization
    void Start()
    {
        _controller = GetComponent<EnemyController>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _walk = GetComponent<AISimpleWalk>();
        _ai = GetComponent<AIShootOnSight>();

        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        _walk.enabled = false;
        _ai.enabled = false;

        Beam.SetActive(false);
        Beam2.SetActive(false);
        BeamEnd.SetActive(false);

        Beam.GetComponent<GiveDamageToPlayer>().DamageToGive = 4;
        Beam2.GetComponent<GiveDamageToPlayer>().DamageToGive = 4;
        BeamEnd.GetComponent<GiveDamageToPlayer>().DamageToGive = 4;

        if (DashSound != null)
            SoundManager.Instance.PlaySound(DashSound, transform.position);
        _animator.SetBool("DashUp", true);
        GetComponent<SpriteTrail>().enabled = true;

        StartCoroutine(StartFight(2.5f));
    }


    IEnumerator StartFight(float delay)
    {
        yield return new WaitForSeconds(delay);

        _stage = Stage.Fall;
        GetComponent<SpriteTrail>().enabled = false;

        if (Number == 0)
        {
            _animator.SetBool("DashUp", false);
            StartCoroutine(Hover(0.1f));
        }
        else
        {
            GetComponent<SpriteRenderer>().flipY = true;
        }
    }


    IEnumerator Hover(float delay)
    {
        yield return new WaitForSeconds(delay);

        LayerMask maskLayer = 1 << LayerMask.NameToLayer("Enemies");
        RaycastHit2D[] circles = Physics2D.CircleCastAll(transform.localPosition, 800.0f, Vector2.right, 0.0f, maskLayer);

        //Debug.Log(circles.Length);

        if (circles.Length > 1)
        {
            _controller.FreezeY = true;
            StartCoroutine(Hover(0.75f));
        }
        else
        {
            _controller.FreezeY = false;
            SuckUpGems();
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.Player == null || _wasDead)
            return;

        switch (_stage)
        {
            case Stage.Jump:
                _controller.SetVerticalForce(16);
                break;

            case Stage.Fall:
                if (_controller.State.IsGrounded)
                {
                    GetComponent<SpriteRenderer>().flipY = false;
                    _animator.SetBool("DashUp", false);

                    if (LandSound != null)
                        SoundManager.Instance.PlaySound(LandSound, transform.position);
                    _stage = Stage.Bazooka;

                    if(Number == 1)
                        PopupSpikes();
                }
                break;

            case Stage.Shield:
                Shield();
                break;

            case Stage.Bazooka:
                Bazooka();
                break;
        }

        _animator.SetFloat("Speed", Mathf.Abs(_controller.Speed.x));
        _animator.SetFloat("SpeedY", _controller.Speed.y);
        _animator.SetBool("Grounded", _controller.State.IsGrounded);
    }


    void Bazooka()
    {
        _ai.enabled = true;
        _health.MinDamageThreshold = 1;

        if (_animator.GetBool("Hurt"))
        {
            _health.MinDamageThreshold = 10;
            _stage = Stage.Shield;
        }
    }


    void PopupSpikes()
    {
        var maskLayer = 1 << LayerMask.NameToLayer("Safe");

        RaycastHit2D[] circles = Physics2D.CircleCastAll(transform.localPosition, 64.0f, Vector2.right, 0.0f, maskLayer);

        //Debug.Log ("Spikes: " + circles.Length);

        for (int i = 0; i < circles.Length; i++)
        {
            var circle = circles[i];

            var wall = circle.collider.gameObject.GetComponent<EnergySpike>();

            if (wall != null)
            {
                float time = Mathf.Abs(wall.transform.position.x - transform.position.x) / 24;
                wall.StartCoroutine(wall.Popup(time));
            }
        }
    }


    IEnumerator StartBeam(float delay)
    {
        yield return new WaitForSeconds(delay);

        _walk.Disable();

        float dir = -transform.localScale.x;

        if (BeamSound != null)
            SoundManager.Instance.PlaySound(BeamSound, transform.position);

        // Measure the distance to the wall
        RaycastHit2D raycast = CorgiTools.CorgiRayCast(Beam.transform.position, dir*Vector2.right, 64, 1 << LayerMask.NameToLayer("Platforms"), true, Color.red);

        if (raycast)
        {
            float dist = Mathf.Abs(Beam.transform.position.x - raycast.point.x);

            float width = Beam2.GetComponent<SpriteRenderer>().bounds.size.x;

            Beam2.transform.localScale = new Vector2(dist / width, 1);
            Beam2.transform.position = new Vector3(Beam.transform.position.x + dir * (dist / 2 - 2), Beam2.transform.position.y, Beam2.transform.position.z);
            BeamEnd.transform.position = new Vector3(raycast.point.x - dir * 3, BeamEnd.transform.position.y, BeamEnd.transform.position.z);
            Beam.SetActive(true);
            Beam2.SetActive(true);
            BeamEnd.SetActive(true);
        }
    }


    IEnumerator StopBeam(float delay)
    {
        yield return new WaitForSeconds(delay);

        Beam.SetActive(false);
        Beam2.SetActive(false);
        BeamEnd.SetActive(false);

        _animator.SetBool("StandUp", false);
        _animator.SetBool("Deflect", false);

        _stage = Stage.Bazooka;
    }


    void Shield()
    {
        if (Beam.activeSelf || _wasDead)
            return;

        _ai.enabled = false;

        float px = GameManager.Instance.Player.transform.position.x;

        float x = transform.position.x;
        float s = transform.localScale.x;

        if ((px > x && s == -1) || (px < x && s == 1))
        {
            _walk.Disable();
            _health.MinDamageThreshold = 10;

            if(_health.DeflectionCount >= 6)
            {
                if (ShieldAttackCount == 0)
                {
                    ShieldAttackCount++;
                    _stage = Stage.Beam;
                    _animator.SetBool("StandUp", true);
                    _health.DeflectionCount = 0;

                    if (SpeakSfx != null)
                        SoundManager.Instance.PlaySound(SpeakSfx, transform.position);

                    // Fire beam!
                    StartCoroutine(StartBeam(0.5f));
                    StartCoroutine(StopBeam(1.25f));
                }
                else
                {
                    if (Number < 1)
                    {
                        if (DashSound != null)
                            SoundManager.Instance.PlaySound(DashSound, transform.position);

                        _animator.SetBool("DashUp", true);
                        GetComponent<SpriteTrail>().enabled = true;
                        _stage = Stage.Jump;
                        StartCoroutine(SwapInWasp(1.5f));
                    }
                    else
                    {
                        if (_health.CurrentHealth > 0)
                        {
                            Instantiate(DischargeEffect, transform.position + 0.5f * Vector3.up, transform.rotation);

                            _wasDead = true;
                            if (DeadSound != null)
                                SoundManager.Instance.PlaySound(DeadSound, transform.position);
                            SoundManager.Instance.PlayRegularMusic();

                            _health.MinHurtDamage = 0;
                            _health.TakeDamage(1000, gameObject, false);                            
                        }
                    }
                }
            }
        }
        else
        {
            _walk.ChangeDirection();
            _health.MinDamageThreshold = 1;
        }
    }



    IEnumerator SwapInWasp(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Wasp != null)
        {
            var w = Instantiate(Wasp, new Vector3(1, 34, 8), Quaternion.identity);

            BossWasp bw = w.GetComponent<BossWasp>();

            if (bw != null)
                bw.StartBars = false;

            bw.Number = Number + 1;
        }

        Destroy(gameObject);
    }
}
