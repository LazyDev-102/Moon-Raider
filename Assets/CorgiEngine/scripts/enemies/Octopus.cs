using UnityEngine;
using System.Collections;

public class Octopus : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sprite;
    private AIReact reaction;
    private Rigidbody2D rigid;
    private BoxCollider2D collider;
    bool flying = false;
    Vector2 orgPosition;

    public float FlutterSpeed = 1f;
    public bool Hatched = true;

    private bool started = false;
    private bool sinking = false;
    private Coroutine sinkRoutine = null;
    private bool thinking = true;

    public virtual void Awake()
    {
        orgPosition = transform.position;

        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        reaction = GetComponent<AIReact>();
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();

        if (!Hatched)
        {
            rigid.gravityScale = 0.25f;
        }
    }

    void Start()
    {
        var mask = 1 << LayerMask.NameToLayer("Platforms");
    }

    void FixedUpdate()
    {
        if (reaction.Reacting && thinking)
        {
            if (!Hatched)
            {
                Hatched = true;
                animator.SetBool("Hatched", true);
                rigid.gravityScale = 0;
            }

            // This thing is only following the player, so skip the expensive "GetComponent"
            CharacterBehavior behavior = GameManager.Instance.Player;

            if (behavior == null)
                return;

            if (!behavior.BehaviorState.Swimming)
                return;

            flying = true;

            float x = FlutterSpeed * Time.deltaTime;

            float deltaX = transform.position.x - reaction.Target.transform.position.x;

            if (deltaX > 0)
                x = -x;

            sprite.flipX = (x > 0 && Mathf.Abs(deltaX) > 1);

            float y = FlutterSpeed * Time.deltaTime;

            float deltaY = transform.position.y - reaction.Target.transform.position.y + 1;

            if (deltaY > 0)
                y = -y;

            if (Mathf.Abs(deltaY) < 0.5)
                y = 0;

            Vector2 newPosition = new Vector2(x, y);

            transform.Translate(newPosition, Space.World);

            rigid.gravityScale = 0;
            sinking = false;
            started = true;
        }
        else
        {
            if (!sinking)
            {
                if (sinkRoutine == null && started)
                    sinkRoutine = StartCoroutine(Sink(1f));
            }
            else
            {
                if (transform.localPosition.y > (orgPosition.y + 0.1f))
                {
                    float x = FlutterSpeed * Time.deltaTime;
                    float y = -FlutterSpeed * Time.deltaTime;
                    if (transform.localPosition.x > orgPosition.x)
                    {
                        x = -x;
                    }

                    Vector2 newPosition = new Vector2(x, y);
                    transform.Translate(newPosition, Space.World);
                }
            }
        }
    }

    protected virtual IEnumerator Sink(float duration)
    {
        yield return new WaitForSeconds(duration);

        sinking = true;
        rigid.gravityScale = 0.05f;
    }

    public void EnterWater()
    {
        thinking = true;
    }

    public void ExitWater()
    {
        thinking = false;
    }
}
