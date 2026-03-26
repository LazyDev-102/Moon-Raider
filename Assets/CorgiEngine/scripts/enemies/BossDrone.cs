using UnityEngine;
using System.Collections;
using JulienFoucher;

public class BossDrone : Boss
{
    public LayerMask maskLayer;
    private Vector2 speed = Vector2.zero;

    private CameraController sceneCamera;

    private Animator _animator;
    private Rigidbody2D _rigid;
    private Health _health;
    private AIReact _react;
    private Turret _turret;
    private SpriteTrail _trail;
  

    private bool _started = false;
    private bool _wasHurt = false;
    private bool _wasDead = false;
    private bool _canFlip = false;

    private bool _ramming = false;
    private int _orgThresh = 0;
    private float _orgSpeed = 0;

    void Start()
    {
        if (CheckDefeated())
            return;

        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _react = GetComponent<AIReact>();
        _turret = GetComponent<Turret>();
        _trail = GetComponent<SpriteTrail>();

        _orgThresh = _health.MinDamageThreshold;

        _trail.enabled = false;

        transform.localScale = Vector3.one;
    }

    void DropIn()
    {
        CloseWalls();

        SoundManager.Instance.PlayBossMusic();

        StartCoroutine(Drop(1.25f));
    }


    public virtual IEnumerator Drop(float duration)
    {
        yield return new WaitForSeconds(duration);

        sceneCamera.SetTarget(transform);

        speed.y = -10;

        yield return new WaitForSeconds(1.25f);

        speed.y = -4;

        sceneCamera.FreezeAt(new Vector3(-3, -5f, 0));

        yield return new WaitForSeconds(0.225f);

        speed.y = 2;

        yield return new WaitForSeconds(0.25f);

        speed.y = 0;

        SuckUpGems();
        StartCoroutine(GUIManager.Instance.SlideBarsOut(0.75f));

        yield return new WaitForSeconds(1.25f);

        speed.x = -6;

        _canFlip = true;

        GameManager.Instance.ThawCharacter();
    }


    public virtual IEnumerator StopDrop(float duration)
    {
        yield return new WaitForSeconds(duration);

        speed.y = 0;
    }


    protected virtual void FixedUpdate()
    {
        Vector3 _newPosition = speed * Time.deltaTime;
        transform.Translate(_newPosition, Space.World);

        _health.MinDamageThreshold = (speed.y == 0) ? 0 : 100;
    }


    void Update()
    {
        bool reacting = _animator.GetBool("Reacting");
        if (reacting && !_started)
        {
            _started = true;
            _react.radius = 8;
            _react.ReactSfx = null;
            DropIn();
            return;
        }

        if (_started)
        {
            bool hurt = _animator.GetBool("Hurt");
            bool dying = _animator.GetBool("Dying");

            // Hurt
            if (hurt && !_wasHurt && !dying)
            {
                //_health.MinDamageThreshold = 100;
                _wasHurt = true;

                StartCoroutine(StartAttack(0.833f));
            }

            // Turn around
            if (CheckForWalls())
            {
                transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
                speed.x = -speed.x;
            }

            // Death
            if (dying && !_wasDead)
            {
                _wasDead = true;

                _react.enabled = false;

                _turret.SetArmed(false);
                _turret.enabled = false;
                _turret.Emitter.enabled = false;

                speed = Vector2.zero;

                StartCoroutine(Dead(4f));
            }
        }
    }


    public virtual IEnumerator StartAttack(float duration)
    {
        yield return new WaitForSeconds(duration);

        _wasHurt = false;

        // Ram!!!
        _ramming = true;
        _trail.enabled = true;
        speed.x = -transform.localScale.x * 12;

        _turret.SetArmed(false);

        _animator.SetBool("Attack", true);
        _animator.SetBool("Hurt", false);

        speed.y = -2;

        StartCoroutine(StopAtack(3f));
    }


    public virtual IEnumerator StopAtack(float duration)
    {
        yield return new WaitForSeconds(duration);

        _ramming = false;
        speed.x = -transform.localScale.x * 6;
        _trail.enabled = false;

        _turret.SetArmed(true);
        _animator.SetBool("Attack", false);

        speed.y = 2;

        StartCoroutine(StopDrop(3));
    }


    public virtual IEnumerator Dead(float duration)
    {
        yield return new WaitForSeconds(duration);

        Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
        sceneCamera.Shake(ShakeParameters);

        OpenWalls();

        sceneCamera.OverrideTarget = null;

        SoundManager.Instance.PlayRegularMusic();
    }


    bool CheckForWalls()
    {
        if (!_canFlip)
            return false;

        float dist = _ramming ? 4 : 2;

        RaycastHit2D raycast = CorgiTools.CorgiRayCast(transform.position, transform.localScale.x * Vector2.left, dist, maskLayer, true, Color.green);

        // if the raycast doesn't hit anything
        if (raycast)
        {
            return true;
        }

        return false;
    }
}
