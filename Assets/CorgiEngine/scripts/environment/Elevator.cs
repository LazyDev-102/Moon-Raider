using UnityEngine;
using System.Collections;

/// <summary>
/// TODO DESCRIPTION
/// </summary>
public class Elevator : MonoBehaviour 
{
	/// the time (in seconds) before the fall of the platform
	public float TimeBeforeFall = 2f;
	/// 
	public float FallSpeed = 2f;

	// private stuff
	protected Vector2 _newPosition;
	protected BoxCollider2D _bounds;

	/// <summary>
	/// Initialization
	/// </summary>
	protected virtual void Start()
	{
		_bounds = GameObject.FindGameObjectWithTag("LevelBounds").GetComponent<BoxCollider2D>();
	}

	/// <summary>
	/// This is called every frame.
	/// </summary>
	protected virtual void FixedUpdate()
	{		
		if (FallSpeed == 0)
			return;

		if (TimeBeforeFall < 0)
		{
			_newPosition = new Vector2(0, -FallSpeed*Time.deltaTime);

			transform.Translate(_newPosition,Space.World);

			if (transform.position.y < _bounds.bounds.min.y) {
				FallSpeed = 0;
			} else {
				Vector2 raycastOrigin = new Vector2(transform.position.x,transform.position.y - 1);		
				RaycastHit2D raycast = CorgiTools.CorgiRayCast(raycastOrigin,-Vector2.down,4,1<<LayerMask.NameToLayer("Platforms"),true,Color.gray);

				if (raycast) {
					FallSpeed = 0;
				}
			}
		}
	}
		

	/// <summary>
	/// Triggered when a CorgiController touches the platform
	/// </summary>
	/// <param name="controller">The corgi controller that collides with the platform.</param>

	public virtual void OnTriggerStay2D(Collider2D collider)
	{
		CorgiController controller = collider.GetComponent<CorgiController>();

		if (controller == null) {
			FallSpeed = 0;
			return;
		}

		if (TimeBeforeFall > 0)
		{
			if (collider.transform.position.y > transform.position.y)
			{
				TimeBeforeFall -= Time.deltaTime;
			}
		}	
	}
}
