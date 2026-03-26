using UnityEngine;
using System.Collections;

public class Drip : MonoBehaviour
{
	private SpriteRenderer drenderer;
	private Rigidbody2D rigid;
	Vector2 orgPosition, hitpoint;
	public float FallSpeed = -12f;
	public bool Hot = false;

    private bool waiting = false;
	private bool resetting = false;


    // Use this for initialization
    public virtual void Start()
	{
		gameObject.name = "Drip";

        orgPosition = transform.position;
		orgPosition.y += 0.5f;

		drenderer = GetComponentInChildren<SpriteRenderer>();

		if (GlobalVariables.WorldIndex == 9)
			drenderer.color = Color.green;

		rigid = GetComponent<Rigidbody2D> ();

        rigid.velocity = new Vector2(0, 0);
        rigid.constraints = RigidbodyConstraints2D.FreezePositionY;

        StartCoroutine(StopFalling(null));
    }

	public void OnTriggerEnter2D(Collider2D collider)
	{
		if (resetting)
			return;

		resetting = true;

        // Reset drip
        if (rigid != null)
        {
            rigid.velocity = new Vector2(0, 0);
            rigid.constraints = RigidbodyConstraints2D.FreezePositionY;
        }

        if (drenderer != null)
		    drenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0f);

		// Splash
		GameObject splashObject;

		if(Hot)
			splashObject = new GameObject ("lavaSplash");
		else
			splashObject = new GameObject ("waterSplash");

		splashObject.transform.parent = gameObject.transform.parent;
		splashObject.layer = 6;

		Vector2 point = transform.position;
		splashObject.transform.position = new Vector3 (point.x, point.y + 0.75f, 0);

		SpriteRenderer splashRenderer = splashObject.AddComponent<SpriteRenderer>() as SpriteRenderer;

		if (GlobalVariables.WorldIndex == 9)
			splashRenderer.color = Color.green;

		splashRenderer.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		splashRenderer.sortingLayerName = "Between";
		splashRenderer.transform.parent = splashObject.transform;

		Animator splashAnimator = splashObject.AddComponent<Animator> ();
		if(Hot)
			splashAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/LavaSplash");
		else
			splashAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/WaterSplash");

		StartCoroutine(StopFalling(splashObject));
	}

	void FixedUpdate()
	{
		if (waiting) {
			float alpha = Mathf.Lerp (drenderer.material.color.a, 1.0f, Time.deltaTime);
			drenderer.material.color = new Color(1.0f, 1.0f, 1.0f, alpha);
		}
	}

	public virtual IEnumerator StopFalling(GameObject toDelete)
	{
		yield return new WaitForSeconds (0.583f);

		waiting = true;
		transform.position = orgPosition;

        if (toDelete != null)
		    Destroy (toDelete);
		StartCoroutine (Reset ());
	}

	public virtual IEnumerator Reset()
	{
		yield return new WaitForSeconds (Random.Range(1.0f,3.0f));
		StartCoroutine (StopWaiting ());
	}

	public virtual IEnumerator StopWaiting()
	{		
		yield return new WaitForSeconds (Random.Range(1f,3.0f));
		waiting = false;
		resetting = false;
		rigid.constraints = RigidbodyConstraints2D.None;
        rigid.velocity = new Vector2(0, -1f);
    }
}

