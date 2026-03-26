using UnityEngine;
using System.Collections;

public class Bat : MonoBehaviour
{
	private Animator animator;
	private SpriteRenderer sprite;
	private AIReact reaction;
	bool flying = false;
	Vector2 orgPosition;
    public bool StopAtWater = true;

	public float FlutterSpeed = 1f;
    public float YOffset = 0;

	private bool _inAir = false;

	public virtual void Awake()
	{
		orgPosition = transform.position;

		animator = gameObject.GetComponent<Animator> ();
		sprite = gameObject.GetComponent<SpriteRenderer> ();
		reaction = gameObject.GetComponent<AIReact> ();
	}

	void Start()
	{
		var mask = 1 << LayerMask.NameToLayer ("Platforms");

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
            /**CharacterBehavior behavior = reaction.Target.GetComponent<CharacterBehavior>();*/

            // This thing is only following the player, so skip the expensive "GetComponent"
            CharacterBehavior behavior = GameManager.Instance.Player;

            if (behavior == null)
                return;

            if (behavior.BehaviorState.Swimming || reaction.Target == null)
                return;

            flying = true;

			float x = FlutterSpeed * Time.deltaTime;

			float deltaX = transform.position.x - reaction.Target.transform.position.x;

			if(deltaX > 0)
				x = -x;

			sprite.flipX = (x < 0 && Mathf.Abs(deltaX) > 1);

			float y = FlutterSpeed * Time.deltaTime;

			float deltaY = (transform.position.y - YOffset) - reaction.Target.transform.position.y + 1;

			if(deltaY > 0)
				y = -y;

			// Only fly up at water
			if (animator.GetBool ("Underwater") && StopAtWater)
				y = Mathf.Abs (y);

			if (Mathf.Abs (deltaY) < 0.5)
				y = 0;

			Vector2 newPosition = new Vector2 (x, y);

			transform.Translate (newPosition, Space.World);
		} 
		/*else 
		{
			if (transform.localPosition.y > (orgPosition.y + 0.1f)) 
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
		}*/

		bool showFlying = flying || _inAir;

		if (animator != null)
			animator.SetBool("Flying", showFlying);
	}
}

