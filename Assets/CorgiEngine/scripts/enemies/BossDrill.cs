using UnityEngine;
using System.Collections;


public class BossDrill : Boss
{
    protected Animator _animator;
    protected SpriteRenderer _sprite;
    protected Health _health;
    protected EnemyController _controller;
    protected AIReact _react;

    public float SideForce = 8;
    public float JumpForce = 8;
    public AudioClip LandSfx;
    public AudioClip RotateSfx;
    public GameObject Base;

    public Health Top, Left, Right, Bottom;
    public GameObject CollisionEffect;

    private bool wasGrounded = false;
    private BoxCollider2D _boxCollider;

    public float RotatonSpeed = 0;
    private float rotationTarget = 0;

    private int phase = 0;
    private bool canRotate = false;

    private bool awake = false;
    private bool ready = false;
    private bool wasDead = false;

    CameraController sceneCamera;


    // Use this for initialization
    void Start()
    {
        if (CheckDefeated())
            return;

        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _sprite = GetComponent<SpriteRenderer>();
        _controller = GetComponent<EnemyController>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _react = GetComponent<AIReact>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_react.Reacting && !awake)
        {
            awake = true;

            CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

            if (sceneCamera != null)
                sceneCamera.FreezeAt(new Vector3(-3, -20, 0));

            SoundManager.Instance.PlayBossMusic();

            CloseWalls();

            StartCoroutine(GUIManager.Instance.SlideBarsOut(3f));

            // Don't need anymore
            _react.enabled = false;

            StartCoroutine(Ready());

            return;
        }

        if (!ready)
            return;

        // Ground Slam
        if (_controller.State.IsGrounded && !wasGrounded)
        {
            if (LandSfx != null)
                SoundManager.Instance.PlaySound(LandSfx, transform.position);

            if (CollisionEffect != null)
                Instantiate(CollisionEffect, transform.position + 6*Vector3.down + _controller.Speed.normalized.x * Vector3.right, transform.rotation);

                Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
            sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

            if (sceneCamera != null)
                sceneCamera.Shake(ShakeParameters);

            StartCoroutine(Jump(0.1f));
        }

        wasGrounded = _controller.State.IsGrounded;

        if (_controller.State.IsCollidingLeft)
            _controller.SetHorizontalForce(SideForce);
        else if (_controller.State.IsCollidingRight)
            _controller.SetHorizontalForce(-SideForce);
        else
        {
            if(_controller.Speed.x == 0)
            {
                if(transform.position.x > GameManager.Instance.Player.transform.position.x)
                    _controller.SetHorizontalForce(-SideForce);
                else
                    _controller.SetHorizontalForce(SideForce);
            }
        }

        float rotation = Base.transform.rotation.eulerAngles.z;

        if (rotation > 0)
            rotation -= 360;

        bool noBottom = Bottom.CurrentHealth <= 0 && rotation == 0;
        bool noLeft = Left.CurrentHealth <= 0 && rotation == -90;
        bool noTop = Top.CurrentHealth <= 0 && rotation == -180;
        bool noRight = Right.CurrentHealth <= 0 && rotation == -270;

        if ((noBottom && phase == 0) || (noLeft && phase == 1) || (noTop && phase == 2) || (noRight && phase == 3))
        {
            _boxCollider.offset = new Vector2(0, -2);

            phase += 1;

            // Schedule a rotation
            StartCoroutine(Rotate(2));
        }
        else
        {
            if (!canRotate)
            {
                if(phase == 4)
                {
                    phase = 5;

                    // Kaboom!
                    GetComponent<Health>().TakeDamage(1000, gameObject, false);
                    StartCoroutine(Dead(3f));
                }
                return;
            }

            if (rotation > rotationTarget)
            {
                Base.transform.Rotate(0, 0, RotatonSpeed * Time.deltaTime);
            }
            else if (rotation <= rotationTarget)
            {
                Base.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationTarget));
                _boxCollider.offset = new Vector2(0, -4);

                // Stop any funkiness that might result from falling
                if (transform.position.y < -20)
                    transform.position = new Vector3(transform.position.x, -20, transform.position.z);

                canRotate = false;
            }
        }
    }


    public virtual IEnumerator Dead(float duration)
    {
        yield return new WaitForSeconds(duration);

        Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
        sceneCamera.Shake(ShakeParameters);

        OpenWalls();
        sceneCamera.SetTarget(GameManager.Instance.Player.transform);

        SoundManager.Instance.PlayRegularMusic();
    }


    public virtual IEnumerator Ready()
    {
        yield return new WaitForSeconds(1);

        Say(0);

        yield return new WaitForSeconds(1);

        SuckUpGems();

        yield return new WaitForSeconds(1);

        ready = true;
    }


    public virtual IEnumerator Rotate(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (RotateSfx != null)
            SoundManager.Instance.PlaySound(RotateSfx, transform.position);

        rotationTarget -= 90;
        canRotate = true;
    }


    public virtual IEnumerator Jump(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_controller.State.IsCollidingBelow)
        {
            _controller.SnapToFloor = false;
            _controller.SetVerticalForce(JumpForce);

            yield return new WaitForSeconds(0.25f);

            _controller.SnapToFloor = true;
        }
    }
}