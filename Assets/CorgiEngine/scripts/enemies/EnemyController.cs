using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;
using System.Collections.Generic;
[RequireComponent(typeof(BoxCollider2D))]

// DISCLAIMER : this controller's been built from the ground up for the Corgi Engine. It takes clues and inspirations from various methods and articles freely 
// available online. Special thanks to @prime31 for his talent and patience, Yoann Pignole, Mysteriosum and Sebastian Lague, among others for their great articles
// and tutorials on raycasting. If you have questions or suggestions, feel free to contact me at unitysupport@reuno.net

/// <summary>
/// The character controller that handles the character's gravity and collisions.
/// It requires a Collider2D and a rigidbody to function.
/// </summary>
public class EnemyController : MonoBehaviour 
{
	/// the various states of our character
	public CorgiControllerState State { get; protected set; }
	/// the initial parameters
	public CorgiControllerParameters DefaultParameters;
	/// the current parameters
	public CorgiControllerParameters Parameters{get{return _overrideParameters ?? DefaultParameters;}}

	[Space(10)]	
	[Header("Collision Masks")]
	/// The layer mask the platforms are on
	public LayerMask PlatformMask=0;
    public bool SnapToFloor = true;
	/// The layer mask the moving platforms are on
	public LayerMask MovingPlatformMask=0;
	/// The layer mask the one way platforms are on
	public LayerMask EdgeColliderPlatformMask=0;
	/// gives you the object the character is standing on
	public GameObject StandingOn { get; protected set; }	
	/// the current velocity of the character
	public Vector2 Speed { get{ return _speed; } }

	[Space(10)]	
	[Header("Raycasting")]
	public bool UseSpeedForXRays = true;
	public int NumberOfHorizontalRays = 8;
	public int NumberOfVerticalRays = 8;	
	public float RayOffset=0.05f; 

	public Vector3 ColliderCenter {get
		{
			Vector3 colliderCenter = Vector3.Scale(transform.localScale, _boxCollider.offset);
			return colliderCenter;
		}}
	public Vector3 ColliderPosition {get
		{
			Vector3 colliderPosition = transform.position + ColliderCenter;
			return colliderPosition;
		}}
	public Vector3 ColliderSize {get
		{
			Vector3 colliderSize = Vector3.Scale(transform.localScale, _boxCollider.size);
			return colliderSize;
		}}
	public Vector3 BottomPosition {get
		{
			Vector3 colliderBottom = new Vector3(ColliderPosition.x,ColliderPosition.y - (ColliderSize.y / 2),ColliderPosition.z);
			return colliderBottom;
		}}

    public bool FreezeY { get; set; }

	// parameters override storage
	protected CorgiControllerParameters _overrideParameters;
	// private local references			
	protected Vector2 _speed;
	protected float _fallSlowFactor;
	protected Vector2 _externalForce;
	protected Vector2 _newPosition;
	protected Transform _transform;
	protected BoxCollider2D _boxCollider;
	protected GameObject _lastStandingOn;
	protected LayerMask _platformMaskSave;
	protected float _movingPlatformsCurrentGravity;

	protected const float _largeValue=500000f;
	protected const float _smallValue=0.0001f;
	protected const float _obstacleHeightTolerance=0.05f;
	protected const float _movingPlatformsGravity=-150;

	// rays parameters
	protected Rect _rayBoundsRectangle;

	protected List<RaycastHit2D> _contactList;

	/// <summary>
	/// initialization
	/// </summary>
	protected virtual void Awake()
	{
		_transform=transform;
		_boxCollider = (BoxCollider2D)GetComponent<BoxCollider2D>();
		_contactList = new List<RaycastHit2D>();
		State = new CorgiControllerState();

		// we add the edge collider platform and moving platform masks to our initial platform mask so they can be walked on	
		_platformMaskSave = PlatformMask;	
		PlatformMask |= EdgeColliderPlatformMask;
		PlatformMask |= MovingPlatformMask;

        State.Reset();
		SetRaysParameters();
	}

    protected virtual void Start()
    {

    }

	/// <summary>
	/// Use this to add force to the character
	/// </summary>
	/// <param name="force">Force to add to the character.</param>
	public virtual void AddForce(Vector2 force)
	{
		_speed += force;	
		_externalForce += force;
	}

	/// <summary>
	///  use this to set the horizontal force applied to the character
	/// </summary>
	/// <param name="x">The x value of the velocity.</param>
	public virtual void AddHorizontalForce(float x)
	{
		_speed.x += x;
		_externalForce.x += x;
	}

	/// <summary>
	///  use this to set the vertical force applied to the character
	/// </summary>
	/// <param name="y">The y value of the velocity.</param>
	public virtual void AddVerticalForce(float y)
	{
		_speed.y += y;
		_externalForce.y += y;
	}

	/// <summary>
	/// Use this to set the force applied to the character
	/// </summary>
	/// <param name="force">Force to apply to the character.</param>
	public virtual void SetForce(Vector2 force)
	{
		_speed = force;
		_externalForce = force;	
	}

	/// <summary>
	///  use this to set the horizontal force applied to the character
	/// </summary>
	/// <param name="x">The x value of the velocity.</param>
	public virtual void SetHorizontalForce (float x)
	{
		_speed.x = x;
		_externalForce.x = x;
	}

	/// <summary>
	///  use this to set the vertical force applied to the character
	/// </summary>
	/// <param name="y">The y value of the velocity.</param>
	public virtual void SetVerticalForce (float y)
	{
		_speed.y = y;
		_externalForce.y = y;

	}

	/// <summary>
	/// Every frame, we apply the gravity to our character, then check using raycasts if an object's been hit, and modify its new position 
	/// accordingly. When all the checks have been done, we apply that new position. 
	/// </summary>
	protected virtual void FixedUpdate()
	{
        if(_contactList != null)
		    _contactList.Clear();

		_speed.y += (Parameters.Gravity + _movingPlatformsCurrentGravity) * Time.deltaTime;

		if (_fallSlowFactor != 0)
		{
			_speed.y *= _fallSlowFactor;
		}

        // we make sure the velocity doesn't exceed the MaxVelocity specified in the parameters
        // for some reason mathf.clamp didn't work, so did this manually
        if (_speed.x > Parameters.MaxVelocity.x)
            _speed.x = Parameters.MaxVelocity.x;
        else if ((_speed.x < -Parameters.MaxVelocity.x))
            _speed.x = -Parameters.MaxVelocity.x;

        if (_speed.y > Parameters.MaxVelocity.y)
            _speed.y = Parameters.MaxVelocity.y;
        else if ((_speed.y < -Parameters.MaxVelocity.y))
            _speed.y = -Parameters.MaxVelocity.y;

        if (FreezeY)
            _speed = Vector2.zero;

        _newPosition = _speed * Time.deltaTime;

		State.WasGroundedLastFrame = State.IsCollidingBelow;
		State.WasTouchingTheCeilingLastFrame = State.IsCollidingAbove;
		State.Reset(); 

		SetRaysParameters();

		CastRaysToTheSides();
		CastRaysBelow();	
		CastRaysAbove();

		_transform.Translate(_newPosition, Space.World);

		SetRaysParameters();

		// we compute the new speed
		if (Time.deltaTime > 0)
			_speed = _newPosition / Time.deltaTime;

		// we change states depending on the outcome of the movement
		if( !State.WasGroundedLastFrame && State.IsCollidingBelow )
			State.JustGotGrounded = true;

		//if (State.IsCollidingLeft || State.IsCollidingRight || State.IsCollidingBelow || State.IsCollidingRight)
		//{
		//	OnCorgiColliderHit();
		//}

		_externalForce.x = 0;
		_externalForce.y = 0;
	}

	/// <summary>
	/// Casts rays to the sides of the character, from its center axis.
	/// If we hit a wall/slope, we check its angle and move or not according to it.
	/// </summary>
	protected virtual void CastRaysToTheSides() 
	{			
		float movementDirection = 1;

		if (UseSpeedForXRays) {
			if ((_speed.x <= 0) || (_externalForce.x < 0))
				movementDirection = -1;
		} else {
			if (transform.localScale.x == 1)
				movementDirection = -1;
		}

		float horizontalRayLength = Mathf.Abs(_speed.x*Time.deltaTime) + _rayBoundsRectangle.width/2 + RayOffset*2;

		Vector2 horizontalRayCastFromBottom = new Vector2(_rayBoundsRectangle.center.x,
			_rayBoundsRectangle.yMin+_obstacleHeightTolerance);	

		Vector2 horizontalRayCastToTop = new Vector2(	_rayBoundsRectangle.center.x,
			_rayBoundsRectangle.yMax-_obstacleHeightTolerance);				

		RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfHorizontalRays];	

		for (int i = 0; i < NumberOfHorizontalRays; i++)
		{	
			Vector2 rayOriginPoint = Vector2.Lerp(horizontalRayCastFromBottom, horizontalRayCastToTop, (float)i / (float)(NumberOfHorizontalRays-1));

            if (State.WasGroundedLastFrame && i == 0)
                hitsStorage[i] = CorgiTools.CorgiRayCast(rayOriginPoint, movementDirection * Vector2.right, horizontalRayLength, PlatformMask, true, Color.red);
            else
                hitsStorage[i] = CorgiTools.CorgiRayCast(rayOriginPoint, movementDirection * Vector2.right, horizontalRayLength, PlatformMask & ~EdgeColliderPlatformMask, true, Color.red);

            if (hitsStorage[i].distance > 0)
			{
				float hitAngle = Mathf.Abs(Vector2.Angle(hitsStorage[i].normal, Vector2.up));		

				State.SlopeAngle = hitAngle;	

				if (hitAngle > Parameters.MaximumSlopeAngle)
				{
					if (_speed.y <= 0) 
					{
						if (movementDirection <= 0)
							State.IsCollidingLeft = true;
						else
							State.IsCollidingRight = true;
					}

					State.SlopeAngleOK = false;

					if (State.IsCollidingLeft)
					{
						_newPosition.x = 0;
					}
					else if(State.IsCollidingRight)
					{						
						_newPosition.x = 0;
					}					

					_contactList.Add(hitsStorage[i]);
					_speed = new Vector2(0, _speed.y);
					break;
				}
			}						
		}
	}

	/// <summary>
	/// Every frame, we cast a number of rays below our character to check for platform collisions
	/// </summary>
	protected virtual void CastRaysBelow()
	{
		if (_newPosition.y < -_smallValue)
			State.IsFalling = true;
		else
			State.IsFalling = false;

        if ((Parameters.Gravity > 0) && (!State.IsFalling) || _externalForce.y > 0)
			return;

		float rayLength = _rayBoundsRectangle.height/2 + RayOffset; 		
		if (_newPosition.y < 0)
		{
			rayLength += Mathf.Abs(_newPosition.y);
		}

        Vector2 verticalRayCastFromLeft = new Vector2(_rayBoundsRectangle.xMin + _newPosition.x, _rayBoundsRectangle.center.y + RayOffset);
        Vector2 verticalRayCastToRight = new Vector2(_rayBoundsRectangle.xMax + _newPosition.x, _rayBoundsRectangle.center.y + RayOffset);

        RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		float smallestDistance = _largeValue; 
		int smallestDistanceIndex = 0; 						
		bool hitConnected = false; 		

		for (int i = 0; i < NumberOfVerticalRays; i++)
		{
            Vector2 rayOriginPoint = Vector2.Lerp(verticalRayCastFromLeft, verticalRayCastToRight, (float)i / (float)(NumberOfVerticalRays - 1));

            // if ((_newPosition.y > 0) && (!State.WasGroundedLastFrame))
            //    hitsStorage[i] = CorgiTools.CorgiRayCast(rayOriginPoint, Vector2.down, rayLength, PlatformMask & ~EdgeColliderPlatformMask, true, Color.blue);
            //else
            hitsStorage[i] = CorgiTools.CorgiRayCast(rayOriginPoint, Vector2.down, rayLength, PlatformMask, true, Color.blue);

            if ((Mathf.Abs(hitsStorage[smallestDistanceIndex].point.y - verticalRayCastFromLeft.y)) <  _smallValue)
			{
				break;
			}

			if (hitsStorage[i])
			{
				hitConnected = true;
				if (hitsStorage[i].distance < smallestDistance)
				{
					smallestDistanceIndex = i;
					smallestDistance = hitsStorage[i].distance;
				}
			}								
		}

		if (hitConnected)
		{

			StandingOn = hitsStorage[smallestDistanceIndex].collider.gameObject;

			//_contactList.Add(hitsStorage[smallestDistanceIndex]);

			// if the character is jumping onto a (1-way) platform but not high enough, we do nothing
			if (!State.WasGroundedLastFrame && smallestDistance < _rayBoundsRectangle.size.y/2 && StandingOn.layer == LayerMask.NameToLayer("OneWayPlatforms"))
			{
				State.IsCollidingBelow = false;
				return;
			}

			State.IsFalling = false;			
			State.IsCollidingBelow = true;

            if (SnapToFloor)
            {
                _newPosition.y = -Mathf.Abs(hitsStorage[smallestDistanceIndex].point.y - verticalRayCastFromLeft.y) + _rayBoundsRectangle.height / 2 + RayOffset;

                if (_externalForce.y > 0)
                {
                    _newPosition.y += _externalForce.y * Time.deltaTime;
                    State.IsCollidingBelow = false;
                }

                //if (!State.WasGroundedLastFrame && _speed.y > 0)
                //{
                //    _newPosition.y += _speed.y * Time.deltaTime;
                //}

                if (Mathf.Abs(_newPosition.y) < _smallValue)
                    _newPosition.y = 0;
            }

			// we check if the character is standing on a moving platform
			/*PathFollow movingPlatform = hitsStorage[smallestDistanceIndex].collider.GetComponent<PathFollow>();
			State.OnAMovingPlatform = false;
			if (movingPlatform != null)
			{
				_movingPlatformsCurrentGravity = _movingPlatformsGravity;
				State.OnAMovingPlatform = true;
				_transform.Translate(movingPlatform.CurrentSpeed*Time.deltaTime);
				_newPosition.y = 0;					
			}
			else
			{
				_movingPlatformsCurrentGravity = 0;
			}*/

			State.OnAMovingPlatform = false;
			// Check for conveyor belt
			Conveyor conveyor = hitsStorage[smallestDistanceIndex].collider.GetComponent<Conveyor>();

			if (conveyor != null) {
				State.OnAMovingPlatform = true;
				_transform.Translate (conveyor.AddedSpeedX * Vector3.right);
			}
		}
		else
		{
			_movingPlatformsCurrentGravity = 0;
			State.IsCollidingBelow = false;
		}	
	}

	/// <summary>
	/// If we're in the air and moving up, we cast rays above the character's head to check for collisions
	/// </summary>
	protected virtual void CastRaysAbove()
	{
		if (_newPosition.y < 0)
			return;

		float rayLength = State.IsGrounded?RayOffset : _newPosition.y*Time.deltaTime;
		rayLength+=_rayBoundsRectangle.height/2;

		bool hitConnected=false; 

		Vector2 verticalRayCastStart=new Vector2(_rayBoundsRectangle.xMin+_newPosition.x,
			_rayBoundsRectangle.center.y);	
		Vector2 verticalRayCastEnd=new Vector2(	_rayBoundsRectangle.xMax+_newPosition.x,
			_rayBoundsRectangle.center.y);	

		RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfVerticalRays];
		float smallestDistance=_largeValue; 

		for (int i = 0; i< NumberOfVerticalRays; i++)
		{							
			Vector2 rayOriginPoint = Vector2.Lerp(verticalRayCastStart,verticalRayCastEnd,(float)i/(float)(NumberOfVerticalRays-1));
			hitsStorage[i] = CorgiTools.CorgiRayCast (rayOriginPoint,Vector2.up,rayLength,PlatformMask & ~EdgeColliderPlatformMask,true,Color.green);	


			if (hitsStorage[i])
			{
				hitConnected = true;
				if (hitsStorage[i].distance<smallestDistance)
				{
					smallestDistance = hitsStorage[i].distance;
				}
			}	

		}	

		if (hitConnected)
		{
			_speed.y=0;
			_newPosition.y = smallestDistance - _rayBoundsRectangle.height/2   ;

			if ( (State.IsGrounded) && (_newPosition.y<0) )
			{
				_newPosition.y=0;
			}

			State.IsCollidingAbove=true;

			if (!State.WasTouchingTheCeilingLastFrame)
			{
				_newPosition.x=0;
				_speed = new Vector2(0, _speed.y);
			}
		}	
	}

	/// <summary>
	/// Creates a rectangle with the boxcollider's size for ease of use and draws debug lines along the different raycast origin axis
	/// </summary>
	public virtual void SetRaysParameters() 
	{		

		_rayBoundsRectangle = new Rect(_boxCollider.bounds.min.x + 0.1f,
			_boxCollider.bounds.min.y,
			_boxCollider.bounds.size.x - 0.1f,
			_boxCollider.bounds.size.y);	

		Debug.DrawLine(new Vector2(_rayBoundsRectangle.center.x,_rayBoundsRectangle.yMin),new Vector2(_rayBoundsRectangle.center.x,_rayBoundsRectangle.yMax));  
		Debug.DrawLine(new Vector2(_rayBoundsRectangle.xMin,_rayBoundsRectangle.center.y),new Vector2(_rayBoundsRectangle.xMax,_rayBoundsRectangle.center.y));



	}


	/// <summary>
	/// Disables the collisions for the specified duration
	/// </summary>
	/// <param name="duration">the duration for which the collisions must be disabled</param>
	public virtual IEnumerator DisableCollisions(float duration)
	{
		// we turn the collisions off
		CollisionsOff();
		// we wait for a few seconds
		yield return new WaitForSeconds (duration);
		// we turn them on again
		CollisionsOn();
	}

	public virtual void ResetMovingPlatformsGravity()
	{
		_movingPlatformsCurrentGravity=0f;
	}

	public virtual void CollisionsOn()
	{
		PlatformMask=_platformMaskSave;
		PlatformMask |= EdgeColliderPlatformMask;
		PlatformMask |= MovingPlatformMask;
	}

	public virtual void CollisionsOff()
	{
		PlatformMask=0;
	}

	public virtual void ResetParameters()
	{
		_overrideParameters = DefaultParameters;
	}

	public virtual void SlowFall(float factor)
	{
		_fallSlowFactor=factor;
	}

	// Events


	/// <summary>
	/// triggered when the character's raycasts collide with something 
	/// </summary>
	protected virtual void OnCorgiColliderHit() 
	{
		foreach (RaycastHit2D hit in _contactList )
		{			
			Rigidbody2D body = hit.collider.attachedRigidbody;
			if (body == null || body.isKinematic)
				return;

			Vector3 pushDir = new Vector3(_externalForce.x, 0, 0);

			body.velocity = pushDir.normalized * Parameters.Physics2DPushForce;		
		}		
	}

	/// <summary>
	/// triggered when the character enters a collider
	/// </summary>
	/// <param name="collider">the object we're colliding with.</param>
	protected virtual void OnTriggerEnter2D(Collider2D collider)
	{

		CorgiControllerPhysicsVolume2D parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
		if (parameters == null)
			return;
		// if the object we're colliding with has parameters, we apply them to our character.
		_overrideParameters = parameters.ControllerParameters;
	}

	/// <summary>
	/// triggered while the character stays inside another collider
	/// </summary>
	/// <param name="collider">the object we're colliding with.</param>
	protected virtual void OnTriggerStay2D( Collider2D collider )
	{
	}

	/// <summary>
	/// triggered when the character exits a collider
	/// </summary>
	/// <param name="collider">the object we're colliding with.</param>
	protected virtual void OnTriggerExit2D(Collider2D collider)
	{		
		CorgiControllerPhysicsVolume2D parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
		if (parameters == null)
			return;

		// if the object we were colliding with had parameters, we reset our character's parameters
		_overrideParameters = null;
	}


}