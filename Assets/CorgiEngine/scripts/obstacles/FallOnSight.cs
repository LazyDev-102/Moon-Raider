using UnityEngine;
using System.Collections;

public class FallOnSight : MonoBehaviour
{
    public LayerMask FloorMask;
    public LayerMask PlayerMask;
    public AudioClip ShakeSound;
    public AudioClip ImpactSound;
    public GameObject ImpactFX;
    public bool DieOnImpact = false;
    public bool ShakeBeforeFall = false;
    public bool ResetXOnFall = false;
    public bool StopFallOnPlayer = false;

    float distance = 0;
    bool shakeLeft = true;
    float ox = 0;

    private Rigidbody2D rigid;
    private SpriteRenderer sprite;
    private Animator animator;

    public enum Stage
    {
        Waiting = 0,
        Shaking = 1,
        Falling = 2,
        Resetting = 3
    }

    Stage stage = Stage.Waiting;

    PitchChanger pc;

    bool canUpdate = false;

    // Use this for initialization
    void Start()
    {
        ox = transform.position.x;

        animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody2D>();
        rigid.velocity = new Vector2(0, 0);
        rigid.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        sprite = GetComponent<SpriteRenderer>();
        pc = GetComponent<PitchChanger>();

        StartCoroutine(Init(0.125f));
    }


    public virtual IEnumerator Init(float duration)
    {
        yield return new WaitForSeconds(duration);

        // First figure out how far down to scan
        RaycastHit2D raycast = Physics2D.Raycast(transform.position + 2 * Vector3.down, Vector2.down, 20, FloorMask);

        if (raycast)
            distance = (transform.position.y - raycast.point.y);

        canUpdate = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.Player == null || !canUpdate)
            return;

        if (stage == Stage.Waiting)
        {
            Vector3 offset = Vector3.left;

            if (GameManager.Instance.Player.transform.position.x > transform.position.x)
                offset = Vector3.right;

            // First figure out how far down to scan
            RaycastHit2D raycast = CorgiTools.CorgiRayCast(transform.position + offset, Vector2.down, distance, PlayerMask, false, Color.red);

            if (raycast)
            {
                stage = Stage.Shaking;
                SoundManager.Instance.PlaySound(ShakeSound, transform.position, false, 1 + pc.Pitch);

                if (ShakeBeforeFall)
                    StartCoroutine(SetStage(0.35f, Stage.Falling));
                else
                    StartCoroutine(SetStage(0.1f, Stage.Falling));
            }
        }
        else if (stage == Stage.Shaking)
        {
            if (ShakeBeforeFall)
            {
                if (shakeLeft)
                {
                    shakeLeft = false;
                    transform.position = new Vector3(ox - 0.1f, transform.position.y - 0.0125f, transform.position.z);
                }
                else
                {
                    shakeLeft = true;
                    transform.position = new Vector3(ox + 0.1f, transform.position.y - 0.0125f, transform.position.z);
                }
            }
        }
    }


    public virtual IEnumerator SetStage(float duration, Stage s)
    {
        yield return new WaitForSeconds(duration);

        stage = s;

        if (stage == Stage.Falling)
        {
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
            transform.rotation = Quaternion.Euler(Vector3.zero);

            var omni = GetComponent<AIOmniWalk>();

            if (omni != null)
                omni.Speed = 3* omni.Speed;

            if(ResetXOnFall)
                transform.position = new Vector3(ox, transform.position.y, transform.position.z);

            if (animator != null)
                animator.SetBool("Landed", true);
        }
    }


    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (stage != Stage.Falling)
            return;

        if (collider.gameObject != GameManager.Instance.Player.gameObject || StopFallOnPlayer)
        {
            rigid.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

            if (DieOnImpact)
                Destroy(gameObject);

            if (ImpactFX != null)
            {
                var splash = Instantiate(ImpactFX, transform.position + Vector3.up, Quaternion.identity);
                splash.transform.parent = transform.parent;
            }

            if (ImpactSound != null)
            {
                if (pc == null)
                    SoundManager.Instance.PlaySound(ImpactSound, transform.position);
                else
                    SoundManager.Instance.PlaySound(ImpactSound, transform.position, false, 1 + pc.Pitch);
            }
        }
    }
}
