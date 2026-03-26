using UnityEngine;
using System.Collections;

public class Wheel : MonoBehaviour
{
    public GameObject WheelObject;
    public int WheelSpeed = 1;
    public AudioClip SlamSound;
    public AudioClip RevSound;

    SpriteRenderer _wheel;
    SpriteRenderer _pilot;
    Animator _animator;
    AISimpleWalk _aiWalk;
    Health _health;
    Flickers _flicker;
    AIReact _react;

    float oldDir = 1;
    bool dead = false;
    private CameraController sceneCamera;

    float OrgSpeed = 0;

    // Use this for initialization
    void Start()
    {
        _pilot = GetComponent<SpriteRenderer>();
        _wheel = WheelObject.GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _aiWalk = GetComponent<AISimpleWalk>();
        _health = GetComponent<Health>();
        _flicker = WheelObject.GetComponent<Flickers>();
        _react = GetComponent<AIReact>();

        oldDir = _aiWalk.Direction.x;
        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        OrgSpeed = _aiWalk.Speed;

        _pilot.flipX = false;
    }

    // Update is called once per frame
    void Update()
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

        if (_health.CurrentHealth <= 0 && !dead)
        {
            dead = true;
            WheelObject.GetComponent<SpriteExploder>().ExplodeSprite();
        }
    }
}
