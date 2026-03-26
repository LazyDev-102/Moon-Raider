using UnityEngine;
using System.Collections;

public class Masher : Switchable
{
    public float MashSpeed = 1f;
    public float OpenSpeed = 1f;
    public bool AlwaysDamages = false;

    public AudioClip MashSound;
    public AudioClip OpenSound;
    public AudioClip SlamSound;

    private AIReact _react;
    private bool closing = false;
    private bool opening = false;
    private bool holding = false;
    private bool booting = false;

    public float MoveDistance = 1.75f;

    public GameObject top;
    public GameObject bottom;

    public GiveDamageToPlayer topDamage;
    public GiveDamageToPlayer bottomDamage;
    public bool DamagesWhileOpening = false;
    public float DamageDelay = 0.5f;

    public GameObject CollisionEffect;
    public bool IsSwitchable = false;
    private bool switchedOn = true;

    private Vector3 maxTopPos;
    private Vector3 maxBotPos;
    private Vector3 orgTopPos;
    private Vector3 orgBotPos;

    public int DamageToGive = 5;

    public SpriteRenderer TopLight;
    public SpriteRenderer BottomLight;

    private Sprite[] details;
    private bool topDamageEnabled = false, bottomDamageEnabled = false;


    // Use this for initialization
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        details = Resources.LoadAll<Sprite>("small_details");

        _react = GetComponent<AIReact>();

        if (transform.localScale.y == -1)
        {
            top = bottom;
            bottom = null;
        }

        if (top != null)
        {
            orgTopPos = top.transform.position;

            if (transform.rotation.z == 0)
                maxTopPos = top.transform.position + MoveDistance * Vector3.down;
            else
            {
                SpriteRenderer sprite = top.GetComponent<SpriteRenderer>();
                sprite.flipX = true;
                maxTopPos = top.transform.position + MoveDistance * Vector3.left;
            }
        }

        if (bottom != null)
        {
            orgBotPos = bottom.transform.position;

            if (transform.rotation.z == 0)
                maxBotPos = bottom.transform.position + MoveDistance * Vector3.up;
            else
            {
                maxBotPos = bottom.transform.position + MoveDistance * Vector3.right;
                SpriteRenderer sprite = bottom.GetComponent<SpriteRenderer>();
                sprite.flipX = true;
            }

            if (TopLight != null)
                TopLight.sprite = details[86];

            if (BottomLight != null)
                BottomLight.sprite = details[86];

            //Debug.Log("Rotation is " + transform.rotation.z + " and target is " + maxBotPos + " from " + orgBotPos);
        }

        if (!AlwaysDamages)
        {
            if (topDamage != null)
                topDamage.DamageToGive = 0;

            if (bottomDamage != null)
                bottomDamage.DamageToGive = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!switchedOn && !opening)
            return;

        if(_react.Reacting && !opening && !closing && !holding && !booting)
        {
            booting = true;

            if (TopLight != null)
                TopLight.sprite = details[85];

            if (BottomLight != null)
                BottomLight.sprite = details[85];

            StartCoroutine(Collapse(0.5f));
        }

        if(closing)
        {
            if (TopLight != null)
                TopLight.sprite = details[84];

            if (BottomLight != null)
                BottomLight.sprite = details[84];

            float y = MashSpeed * Time.deltaTime;

            if (top != null)
            {
                if (top.transform.position.y > maxTopPos.y && transform.rotation.z == 0)
                {
                    Vector2 newDown = y * Vector2.down;
                    top.transform.Translate(newDown, Space.World);

                    if (topDamage != null)
                    {
                        if (topDamage.DamageToGive == 0 && !topDamageEnabled)
                        {
                            StartCoroutine(EnableDamage(topDamage, DamageDelay));
                            topDamageEnabled = true;
                        }
                    }
                }
                else if (top.transform.position.x > maxTopPos.x && transform.rotation.z != 0)
                {
                    Vector2 newDown = y * Vector2.left;
                    top.transform.Translate(newDown, Space.World);

                    if (topDamage != null)
                    {
                        if (topDamage.DamageToGive == 0 && !topDamageEnabled)
                        {
                            StartCoroutine(EnableDamage(topDamage, DamageDelay));
                            topDamageEnabled = true;
                        }
                    }
                }
                else
                {
                    top.transform.position = maxTopPos;

                    if (bottom == null)
                        Slam();
                }
            }

            if (bottom != null)
            {
                if (bottom.transform.position.y < maxBotPos.y && transform.rotation.z == 0)
                {
                    Vector2 newUp = y * Vector2.up;
                    bottom.transform.Translate(newUp, Space.World);

                    if (bottomDamage != null)
                    {
                        if (bottomDamage.DamageToGive == 0 && !bottomDamageEnabled)
                        {
                            StartCoroutine(EnableDamage(bottomDamage, DamageDelay));
                            bottomDamageEnabled = true;
                        }
                    }
                }
                else if (bottom.transform.position.x < maxBotPos.x && transform.rotation.z != 0)
                {
                    Vector2 newUp = y * Vector2.right;
                    bottom.transform.Translate(newUp, Space.World);

                    if (bottomDamage != null)
                    {
                        if (bottomDamage.DamageToGive == 0 && !bottomDamageEnabled)
                        {
                            StartCoroutine(EnableDamage(bottomDamage, DamageDelay));
                            bottomDamageEnabled = true;
                        }
                    }
                }
                else
                {
                    bottom.transform.position = maxBotPos;
                    Slam();
                }
            }
        }

        if(opening)
        {
            CheckOpen();
        }
    }


    IEnumerator EnableDamage(GiveDamageToPlayer masher, float duration)
    {
        yield return new WaitForSeconds(duration);

        masher.DamageToGive = DamageToGive;
    }


    void CheckOpen()
    {
        if (TopLight != null)
            TopLight.sprite = details[86];

        if (BottomLight != null)
            BottomLight.sprite = details[86];

        float y = OpenSpeed * Time.deltaTime;

        if (top != null)
        {
            if (top.transform.position.y < orgTopPos.y && transform.rotation.z == 0)
            {
                Vector2 newDown = y * Vector2.up;
                top.transform.Translate(newDown, Space.World);

                if (!DamagesWhileOpening && topDamage != null && !AlwaysDamages)
                {
                    topDamage.DamageToGive = 0;
                    topDamageEnabled = false;
                }
            }
            else if (top.transform.position.x < orgTopPos.x && transform.rotation.z != 0)
            {
                Vector2 newDown = y * Vector2.right;
                top.transform.Translate(newDown, Space.World);

                if (!DamagesWhileOpening && topDamage != null && !AlwaysDamages)
                {
                    topDamage.DamageToGive = 0;
                    topDamageEnabled = false;
                }
            }
            else
            {
                top.transform.position = orgTopPos;

                if (bottom == null)
                {
                    opening = false;
                    StartCoroutine(Hold(2f));
                }

                if (IsSwitchable)
                {
                    StartCoroutine(Thaw(1.5f));
                }
            }
        }

        if (bottom != null)
        {
            if (bottom.transform.position.y > orgBotPos.y && transform.rotation.z == 0)
            {
                Vector2 newUp = y * Vector2.down;
                bottom.transform.Translate(newUp, Space.World);

                if (!DamagesWhileOpening && bottomDamage != null && !AlwaysDamages)
                {
                    bottomDamage.DamageToGive = 0;
                    bottomDamageEnabled = false;
                }
            }
            else if (bottom.transform.position.x > orgBotPos.x && transform.rotation.z != 0)
            {
                Vector2 newUp = y * Vector2.left;
                bottom.transform.Translate(newUp, Space.World);

                if (!DamagesWhileOpening && bottomDamage != null && !AlwaysDamages)
                {
                    bottomDamage.DamageToGive = 0;
                    bottomDamageEnabled = false;
                }
            }
            else
            {
                bottom.transform.position = orgBotPos;
                opening = false;

                if (IsSwitchable)
                {
                    StartCoroutine(Thaw(1.5f));
                }
                else
                {
                    StartCoroutine(Hold(2f));
                }
            }
        }
    }


    void Slam()
    {
        closing = false;
        booting = false;
        holding = true;

        Vector3 ShakeParameters = new Vector3(0.5f, 0.75f, 1f);
        cam.Shake(ShakeParameters);

        if (SlamSound != null)
            SoundManager.Instance.PlaySound(SlamSound, transform.position);

        if (CollisionEffect != null)
        {
            if (bottom != null)
                Instantiate(CollisionEffect, maxBotPos + Vector3.up, transform.rotation);
            else if(top != null)
                Instantiate(CollisionEffect, maxTopPos + Vector3.down, transform.rotation);
        }

        StartCoroutine(Expand(1f));
    }


    IEnumerator Collapse(float duration)
    {
        yield return new WaitForSeconds(duration);

        closing = true;

        if (MashSound != null)
            SoundManager.Instance.PlaySound(MashSound, transform.position);
    }


    IEnumerator Hold(float duration)
    {
        yield return new WaitForSeconds(duration);

        holding = false;

        if(top != null)
            top.isStatic = true;

        if(bottom != null)
            bottom.isStatic = true;
    }


    IEnumerator Expand(float duration)
    {
        yield return new WaitForSeconds(duration);

        opening = true;
        if (OpenSound != null)
            SoundManager.Instance.PlaySound(OpenSound, transform.position);

        if (top != null)
            top.isStatic = false;

        if (bottom != null)
            bottom.isStatic = false;
    }


    override public IEnumerator Open(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (IsSwitchable)
        {
            switchedOn = false;
            _react.enabled = false;

            StartCoroutine(Freeze(0.01f, transform));

            CheckOpen();
        }
    }

    override public IEnumerator Close(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (IsSwitchable)
        {
            switchedOn = true;
            _react.enabled = true;
        }
    }

    override public void Flicker()
    {
        // Override me
        return;
    }


    override public IEnumerator InstantOpen(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (IsSwitchable)
        {
            switchedOn = false;
            _react.enabled = false;
        }
    }
}
