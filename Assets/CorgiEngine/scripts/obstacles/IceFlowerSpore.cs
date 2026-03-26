using UnityEngine;
using System.Collections;

public class IceFlowerSpore : MonoBehaviour
{
    public AudioClip LatchSound;

    bool canStick = false;
    bool down = true;
    float rx = 0, ry = 0;
    bool playerWasDead = false;

    float FallSpeed = 0.75f;
    protected Vector2 _upPosition, _downPosition, _newPosition, _oldPosition;

    CorgiController controller;
    PitchChanger pc;

    // Use this for initialization
    void Start()
    {
        pc = GetComponent<PitchChanger>();

        float time = (float)Random.Range(0, 3) / 10.0f;
        StartCoroutine(EnableStick(time));
    }


    private void resetUpDown()
    {
        if(rx == 0)
            rx = (float)Random.Range(-5, 5) / 10.0f;

        if(ry == 0)
            ry = (float)Random.Range(-8, -5) / 10.0f;

        transform.localPosition = new Vector3(rx, ry, transform.localPosition.z);

        _upPosition = transform.position + 0.1f * Vector3.up;
        _downPosition = transform.position + 0.1f * Vector3.down;
    }


    protected virtual void FixedUpdate()
    {
        if(!playerWasDead)
        {
            bool isDead = GameManager.Instance.Player.BehaviorState.IsDead;

            if(isDead)
            {
                Poof();
                playerWasDead = true;
                return;
            }
        }

        if (transform.parent == null)
            return;
            
        if(_oldPosition.y != transform.parent.position.y)
            resetUpDown();

        _oldPosition = transform.parent.position;

        if (down)
        {
            _newPosition = new Vector2(0, -FallSpeed * Time.deltaTime);

            if (transform.position.y < _downPosition.y)
            {
                down = false;
            }
        }
        else
        {
            _newPosition = new Vector2(0, FallSpeed * Time.deltaTime);

            if (transform.position.y > _upPosition.y)
            {
                down = true;
            }
        }

        transform.Translate(_newPosition, Space.World);
    }


    IEnumerator EnableStick(float duration)
    {
        yield return new WaitForSeconds(duration);

        canStick = true;
    }


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        Underwater water = collider.GetComponent<Underwater>();

        if (water != null)
            Poof();

        if (!canStick)
            return;

        // we check that the object colliding with the water is actually a corgi controller and a character
        CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
        if (character != null && transform.parent != collider.transform)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponents<CircleCollider2D>()[1].enabled = false;

            controller = collider.GetComponent<CorgiController>();
            controller.Parameters.MaxVelocity.x -= 1;

            if (controller.Parameters.MaxVelocity.x < 1)
                controller.Parameters.MaxVelocity.x = 1;

            controller.Parameters.MaxVelocity.y -= 1;

            if (controller.Parameters.MaxVelocity.y < 15)
                controller.Parameters.MaxVelocity.y = 15;

            transform.parent = character.transform;
            character.Permissions.MeleeAttackEnabled = false;
            character.BehaviorState.CoveredInSpores = true;

            StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.cyan));

            resetUpDown();

            if (LatchSound != null)
            {
                if (pc == null)
                    SoundManager.Instance.PlaySound(LatchSound, transform.position);
                else
                    SoundManager.Instance.PlaySound(LatchSound, transform.position, false, 1 + pc.Pitch);
            }

            canStick = false;
        }
    }

    void Poof()
    {
        // Just assume all spores are cleared
        if (controller != null)
        {
            controller.Parameters.MaxVelocity = controller.OriginalParameters.MaxVelocity;

            CharacterBehavior character = controller.GetComponent<CharacterBehavior>();
            if (character != null)
                character.Permissions.MeleeAttackEnabled = true;
        }

        SpriteExploder se = GetComponent<SpriteExploder>();

        if (se != null)
            se.ExplodeSprite();

        transform.parent = null;
    }
}
