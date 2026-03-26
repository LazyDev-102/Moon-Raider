using UnityEngine;
using System.Collections;

public class Moth : MonoBehaviour
{
	private Animator animator;
	private AIReact reaction;
	bool flying = false;
	Vector2 orgPosition;
	private SpriteRenderer lightRenderer;
	private Light light;

	public float FlutterSpeed = 1f;

	private bool _inAir = false;
    public LayerMask mask;

    public virtual void Awake()
	{
		animator = gameObject.GetComponent<Animator> ();

		reaction = gameObject.GetComponent<AIReact> ();

		// Some real light
		light = gameObject.GetComponent<Light> ();

		// Fake light shadow
		lightRenderer = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
	}

	void Start()
	{
		orgPosition = transform.position;

		RaycastHit2D wall = CorgiTools.CorgiRayCast (transform.position, Vector3.down, 0.5f, mask, true, Color.yellow);

		if (!wall) 
		{
			_inAir = true;
		}
	}

	void FixedUpdate()
	{
		if (reaction.Reacting) 
		{
			flying = true;

			float randX = Random.Range (-4, 4) * FlutterSpeed * Time.deltaTime;
			float randY = Random.Range (0, 8) * FlutterSpeed * Time.deltaTime;

            RaycastHit2D roof = CorgiTools.CorgiRayCast(transform.position + 0.5f*Vector3.up, Vector3.up, 0.5f, mask, true, Color.yellow);

            if (roof)
                randY = 0;

            Vector2 newPosition = new Vector2 (randX, randY);

			transform.Translate (newPosition, Space.World);
		} 
		else 
		{
            RaycastHit2D wall = CorgiTools.CorgiRayCast(transform.position, Vector3.down, 0.05f, mask, true, Color.yellow);

			if (!wall) 
			{
				float x = FlutterSpeed * Time.deltaTime;
				float y = -FlutterSpeed * Time.deltaTime;
				if (transform.localPosition.x > orgPosition.x) {
					x = -x;
				}	

				Vector2 newPosition = new Vector2 (x, y);
				transform.Translate (newPosition, Space.World);
			} 
			else 
			{
				flying = _inAir;
			}
		}

		bool showFlying = flying || _inAir;

		lightRenderer.enabled = showFlying;
		light.enabled = showFlying;

		if (animator != null)
			animator.SetBool("Flying", showFlying);
	}
}

