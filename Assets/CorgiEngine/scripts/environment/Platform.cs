using UnityEngine;
using System.Collections;

/// <summary>
/// TODO DESCRIPTION
/// </summary>
public class Platform : MonoBehaviour 
{
	public float FallSpeed = 0.5f;

	// private stuff
	protected Vector2 _upPosition, _downPosition, _orgPosition, _newPosition;

	bool down = true;

	private void resetUpDown ()
	{
		FallSpeed = 0.5f;
		_upPosition = transform.position + 0.2f*Vector3.up;
		_downPosition = transform.position + 0.2f*Vector3.down;
	}

	/// <summary>
	/// Initialization
	/// </summary>
	protected virtual void Start()
	{
		_orgPosition = transform.position;
		resetUpDown ();
	}

	/// <summary>
	/// This is called every frame.
	/// </summary>
	protected virtual void FixedUpdate()
	{
		if (down) {
			_newPosition = new Vector2 (0, -FallSpeed * Time.deltaTime);

			if (transform.position.y < _downPosition.y) {
				down = false;
			}
		} else {
			_newPosition = new Vector2 (0, FallSpeed * Time.deltaTime);

			if (transform.position.y > _upPosition.y) {
				down = true;
			}
		}

		transform.Translate(_newPosition,Space.World);
	}


	/// <summary>
	/// Triggered when a CorgiController touches the platform
	/// </summary>
	/// <param name="controller">The corgi controller that collides with the platform.</param>

	public virtual void OnTriggerEnter2D(Collider2D collider)
	{
		CorgiController controller = collider.GetComponent<CorgiController>();

		if (controller == null) {
			return;
		}

		if (!down) {
			down = true;
			FallSpeed = 1f;
			_downPosition = _orgPosition + 0.3f * Vector2.down;
			StartCoroutine(DropDown(0.25f));
		}
	}


	public virtual void OnTriggerExit2D(Collider2D collider)
	{
		CorgiController controller = collider.GetComponent<CorgiController>();

		if (controller == null) {
			return;
		}

		resetUpDown ();
	}

	public virtual IEnumerator DropDown(float duration)
	{
		yield return new WaitForSeconds (duration);
		resetUpDown ();
	}
}
