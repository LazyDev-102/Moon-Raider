using UnityEngine;
using System.Collections;

/// <summary>
/// Add this component to a CorgiController2D and it will try to kill your player on sight.
/// </summary>
public class AIInteriorClimb : MonoBehaviour
{
	/// The speed of the agent
	public float Speed;
	protected float _orgSpeed;

	/// The initial direction
	public bool GoesRightInitially = true;

	// private stuff
	protected Vector2 _direction;
	private Vector2 _lastDirection;
	private bool againstLeft = false, againstRight = false, againstUp = false, againstDown = false, farRight = false, farLeft = false;

	public bool OnCeiling {
		get {
			return againstUp;
		}
	}

	public bool OnFloor {
		get {
			return againstDown;
		}
	}

	public bool OnRightWall {
		get {
			return againstRight;
		}
	}

	public bool OnLeftWall {
		get {
			return againstLeft;
		}
	}

	public bool NearLeft {
		get {
			return farLeft;
		}
	}

	public bool NearRight {
		get {
			return farRight;
		}
	}

	/// <summary>
	/// Initialization
	/// </summary>
	protected virtual void Awake()
	{
		// initialize the direction
		_direction = GoesRightInitially ? Vector2.right : Vector2.left;

		_orgSpeed = Speed / 10;
	}

	protected virtual void Start()
	{

	}

	public bool MovingRight() {
		return (_direction.x > 0);
	}

	public void ChangeDirection() 
	{
		GoesRightInitially = !GoesRightInitially;
		_direction = GoesRightInitially ? Vector2.right : Vector2.left;
		_lastDirection = new Vector2 (-_lastDirection.x, -_lastDirection.y);
	}

	public void Disable()
	{
		Speed = 0;
		_orgSpeed = 0;
	}

	/// <summary>
	/// Every frame, moves the agent and checks if it can shoot at the player.
	/// </summary>
	protected virtual void Update () 
	{
		Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y);

		float vectorLengthH = 2f, vectorLengthV = 2f;
		float rotation = transform.rotation.eulerAngles.z;

		if (rotation == 90f || rotation == 270f) {
			vectorLengthH = 2f;
			vectorLengthV = 2f;
		}
			
		var mask = (1 << LayerMask.NameToLayer ("Platforms")) | (1 << LayerMask.NameToLayer ("OneWayPlatforms"));

		// Check sides
		RaycastHit2D raycastLeft = CorgiTools.CorgiRayCast (raycastOrigin, Vector2.left, vectorLengthH, mask, true, Color.red);
		againstLeft = raycastLeft.collider != null;

		RaycastHit2D raycastRight = CorgiTools.CorgiRayCast (raycastOrigin, Vector2.right, vectorLengthH, mask, true, Color.green);
		againstRight = raycastRight.collider != null;

		RaycastHit2D raycastFarLeft = CorgiTools.CorgiRayCast (raycastOrigin, Vector2.left, 3*vectorLengthH, mask, true, Color.red);
		farLeft = raycastFarLeft.collider != null;

		RaycastHit2D raycastFarRight = CorgiTools.CorgiRayCast (raycastOrigin, Vector2.right, 3*vectorLengthH, mask, true, Color.green);
		farRight = raycastFarRight.collider != null;

		// Check up / down
		RaycastHit2D raycastDown = CorgiTools.CorgiRayCast (raycastOrigin, Vector2.down, vectorLengthV, mask, true, Color.yellow);
		againstDown = raycastDown.collider != null;

		RaycastHit2D raycastUp = CorgiTools.CorgiRayCast (raycastOrigin, Vector2.up, vectorLengthV, mask, true, Color.blue);
		againstUp = raycastUp.collider != null;

//		Debug.Log (againstUp + " " + againstRight + " " + againstDown + " " + againstLeft + " " + rotation);

		if (againstDown) {
			// Do nothing
			if (againstUp) {
				Debug.Log ("Stuck");
			} 
			// Bottom-left corner
			else if (againstLeft) {

				if (rotation == 0f) {
					// If I'm walking into a corner, I need to turn
					transform.Rotate (new Vector3 (0, 0, _direction.x*90f));
				} 
				// If I'm on the wall, crawl up
				else if (rotation == 270f) {
					if (_direction.x < 0)
						_lastDirection = new Vector3 (0f, -_direction.x * _orgSpeed, 0f);
					else
						transform.Rotate (new Vector3 (0, 0, _direction.x*90f));
				}
			}
			// Bottom-right corner
			else if (againstRight) {

				if (rotation == 0f) {
					// If I'm walking into a corner, I need to turn
					transform.Rotate (new Vector3 (0, 0, _direction.x*90f));
					_lastDirection = new Vector3 (-_direction.x * _orgSpeed, 0f, 0f);
				}
				// If I'm on the wall, crawl up
				else if (rotation == 90f) {
					if(_direction.x > 0)
						_lastDirection = new Vector3 (_direction.x * _orgSpeed, 0f, 0f);
					else
						transform.Rotate (new Vector3 (0, 0, _direction.x*90f));
				}
			} else {
				_lastDirection = new Vector3 (_direction.x * _orgSpeed, 0f, 0f);
			}
		} else if (againstUp) {
			if (againstDown) {
				Debug.Log ("Stuck");
			}
			// Upper-left corner
			else if (againstLeft) {
				if (rotation == 270f) {
					// If I'm walking into a corner, I need to turn
					if (_direction.x < 0)
						transform.Rotate (new Vector3 (0, 0, _direction.x*90f));
					else
						_lastDirection = new Vector3 (0f, _direction.x * _orgSpeed, 0f);
				} else if (rotation == 180f) {
					if (_direction.x < 0)
						_lastDirection = new Vector3 (_direction.x * _orgSpeed, 0f, 0f);
					else
						transform.Rotate (new Vector3 (0, 0, _direction.x * 90f));
				}
			}
			// Upper-right corner
			else if (againstRight) {
				// If I'm walking into a corner, I need to turn
				transform.Rotate (new Vector3 (0, 0, _direction.x*90f));
			} else {
				_lastDirection = new Vector3 (_direction.x * _orgSpeed, 0f, 0f);
			}
		}

		transform.Translate(_lastDirection);
	}
}
