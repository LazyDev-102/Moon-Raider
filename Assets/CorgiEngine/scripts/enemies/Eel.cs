using UnityEngine;
using System.Collections;

public class Eel : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sprite;
    private AIReact reaction;
    private Rigidbody2D rigid;
    private GiveDamageToPlayer damage;
    Vector2 orgPosition;

    public float FlutterSpeed = 1.5f;

    private bool thinking = true;

    private bool angry = false;
    private LayerMask mask;


    public virtual void Awake()
    {
        orgPosition = transform.position;

        animator = gameObject.GetComponent<Animator>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        reaction = gameObject.GetComponent<AIReact>();
        rigid = gameObject.GetComponent<Rigidbody2D>();
        damage = gameObject.GetComponent<GiveDamageToPlayer>();
    }


    void Start()
    {
        mask = 1 << LayerMask.NameToLayer("Platforms");
    }


    void FixedUpdate()
    {
        if(!angry)
        { 
            if (animator.GetBool("Hurt"))
            {
                angry = true;
                FlutterSpeed += 2f;
                damage.DamageToGive += 2; 
            }
        }

        if (reaction.Reacting && thinking)
        {
            /**CharacterBehavior behavior = reaction.Target.GetComponent<CharacterBehavior>();*/

            // This thing is only following the player, so skip the expensive "GetComponent"
            CharacterBehavior behavior = GameManager.Instance.Player;

            if (behavior == null)
                return;

            if (!behavior.BehaviorState.Swimming)
                return;

            float x = FlutterSpeed * Time.deltaTime;

            float deltaX = transform.position.x - reaction.Target.transform.position.x;

            if (deltaX > 0)
                x = -x;

            if(x < 0)
                transform.localScale = new Vector2(1, 1);
            else
                transform.localScale = new Vector2(-1, 1);

            float y = FlutterSpeed * Time.deltaTime;

            float deltaY = transform.position.y - reaction.Target.transform.position.y;

            if (deltaY > 0)
                y = -y;

            if (Mathf.Abs(deltaY) < 0.5)
                y = 0;

            Vector2 newPosition = new Vector2(x, y);

            transform.Translate(newPosition, Space.World);

            rigid.gravityScale = 0;
        }
        else
        {
            // Pace back and forth
            RaycastHit2D raycast = CorgiTools.CorgiRayCast(transform.position, transform.localScale.x * Vector2.right, 6, mask, true, Color.magenta);

            if (raycast)
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);

            float x = -transform.localScale.x *  FlutterSpeed * Time.deltaTime;
            float y = -FlutterSpeed * Time.deltaTime / 10;

            Vector2 newPosition = new Vector2(x, y);

            transform.Translate(newPosition, Space.World);
        }
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
