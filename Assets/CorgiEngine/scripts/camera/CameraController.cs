using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Camera))]
/// <summary>
/// The Corgi Engine's Camera Controller. Handles camera movement, shakes, player follow.
/// </summary>
public class CameraController : MonoBehaviour 
{
	
	/// True if the camera should follow the player
	public bool FollowsPlayer{get;set;}
	
	[Space(10)]	
	[Header("Distances")]
	/// How far ahead from the Player the camera is supposed to be		
	public float HorizontalLookDistance = 3;
	/// Vertical Camera Offset	
	public Vector3 CameraOffset;
	/// Minimal distance that triggers look ahead
	public float LookAheadTrigger = 4;

	public GameObject Background;
    public SpriteRenderer BackgroundMask;

    [Space(10)]	
	[Header("Movement Speed")]
	/// How fast the camera goes back to the Player
	public float ResetSpeed = 0.5f;
	/// How fast the camera moves
	public float CameraSpeed = 0.3f;
	private float _cameraSpeed;
	
	[Space(10)]	
	[Header("Camera Zoom")]
	[Range (1, 20)]
	public float MinimumZoom = 5f;
	[Range (1, 20)]
	public float MaximumZoom = 5f;	
	public float ZoomSpeed = 0.4f;

    public bool LockX = false;
	
	// Private variables
	public Transform OverrideTarget;
	protected Transform _target;
    protected CorgiController _targetController;
	protected Animator _targetAnimator;
	protected LevelLimits _levelBounds;

    protected float _xMin;
    protected float _xMax;
    protected float _yMin;
    protected float _yMax;	 
	
	protected float _offsetZ;
	protected Vector3 _lastTargetPosition;
    protected Vector3 _currentVelocity;
	protected Vector3 _lookAheadPos;

	protected float _shakeIntensity;
    protected float _shakeDecay;
    protected float _shakeDuration;
	
	protected float _currentZoom;	
	protected Camera _camera;

    protected Vector3 _lookDirectionModifier = Vector3.zero;
    protected Vector3 _twoPlayerModifier = Vector3.zero;

	private bool freezeCamera = false;
	private bool snap = true;

	private Vector3 _freezePosition;

    private float _manualUpDownLookDistance = 4;

    private bool idle = true;
    private float idleTime = 0;
    private float idleTimeLimit = 0.66f;
    private float oldPlayerDir = 0;

    private Color _targetColor;

    private float zoomAdjust = 1.1f;

	/// <summary>
	/// Initialization
	/// </summary>
	protected virtual void Start()
	{
        _targetColor = Color.clear;
    }


    public void Dim()
    {
        _targetColor = Color.black;
    }


    public void Setup()
	{
		Debug.Log("Setting up camera");
		OverrideTarget = null;

		// we get the camera component
		_camera = GetComponent<Camera>();

		_cameraSpeed = CameraSpeed;
	
		// We make the camera follow the player
		FollowsPlayer = true;

        _currentZoom = zoomAdjust * MinimumZoom;

        if (GameManager.Instance.Player == null) 
        {
			Debug.Log("ERROR: No player for camera to look at!");
            return;
        }

		// player and level bounds initialization
		_target = GameManager.Instance.Player.transform;

        if (_target == null)
        {
			Debug.Log("ERROR: No target transform!");
            return;
        }

		if (_target.GetComponent<CorgiController>() == null)
		{
			Debug.Log("ERROR: Target has no controller!");
			return;
		}
		
		_targetController = _target.GetComponent<CorgiController>();
		_targetAnimator = _target.GetComponentsInChildren<Animator> ()[0];
		_levelBounds = GameObject.FindGameObjectWithTag("LevelBounds").GetComponent<LevelLimits>();

		// we store the target's last position
		_lastTargetPosition = _target.position;
		_offsetZ = (transform.position - _target.position).z;
		transform.parent = null;


        _lookDirectionModifier = Vector3.zero;
        _twoPlayerModifier = Vector3.zero;

        Zoom();
    }

	public void SetTarget(Transform target)
    {
        _target = target;
	}

	public void SetSnap()
	{
		snap = true;
	}


	public void FreezeAt(Vector3 freezePos, bool instant = false)
	{
		FollowsPlayer = false;
		_freezePosition = freezePos;

#if UNITY_IOS || UNITY_ANDROID
        _freezePosition = freezePos + Vector3.down;
#endif

        if (instant)
            transform.position = freezePos;
    }

	public virtual IEnumerator SpeedUp(float duration)
	{
		yield return new WaitForSeconds (duration);
		FollowsPlayer = true;
        SetTarget(GameManager.Instance.Player.transform);
        RestoreSpeed();
    }

	public void RestoreSpeed()
	{
		CameraSpeed = _cameraSpeed;
	}

    void UpdateFrozenCamera()
	{
		Vector3 newFreezePos = Vector3.SmoothDamp(transform.position, _freezePosition, ref _currentVelocity, 4*CameraSpeed);

		float fposX = Mathf.Clamp(newFreezePos.x, _xMin, _xMax);
		float fposY = Mathf.Clamp(newFreezePos.y, _yMin, _yMax);
		float fposZ = transform.position.z;

		Vector3 newFCameraPosition = new Vector3(fposX, fposY, fposZ);
		Vector3 fshakeFactorPosition = new Vector3(0, 0, 0);

		// If shakeDuration is still running.
		if (_shakeDuration > 0)
		{
			fshakeFactorPosition = Random.insideUnitSphere * _shakeIntensity * _shakeDuration * 0.75f;
			_shakeDuration -= _shakeDecay * Time.deltaTime;
		}

		fshakeFactorPosition.x = 0;
		fshakeFactorPosition.y *= 2;

		Vector3 newFreezeCameraPosition = newFCameraPosition + fshakeFactorPosition;
		newFreezeCameraPosition.z = 0;

		// We move the actual transform
		transform.position = newFreezeCameraPosition;
		return;
	}


    void UpdateBackgroundColor()
	{
		if (BackgroundMask != null)
		{
			Color bgColor = BackgroundMask.color;
			float r = Mathf.Lerp(bgColor.r, _targetColor.r, Time.deltaTime);
			float g = Mathf.Lerp(bgColor.g, _targetColor.g, Time.deltaTime);
			float b = Mathf.Lerp(bgColor.b, _targetColor.b, Time.deltaTime);
			float a = Mathf.Lerp(bgColor.a, _targetColor.a, Time.deltaTime);

			BackgroundMask.color = new Color(r, g, b, a);
		}

        if(Background != null)
		    Background.transform.position = new Vector3(Background.transform.position.x, 0.75f * transform.position.y - 16, Background.transform.position.z);
	}


    /// <summary>
    /// Every frame, we move the camera if needed
    /// </summary>
    protected virtual void FixedUpdate () 
	{
		UpdateBackgroundColor();

        // if the camera is not supposed to follow the player, we do nothing
        if (!FollowsPlayer) 
        {
			UpdateFrozenCamera();
			return;
		}

		float xMoveDelta = (_target.position - _lastTargetPosition).x;

		bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadTrigger;

		if (updateLookAheadTarget)
		{
			_lookAheadPos = HorizontalLookDistance * Vector3.right * Mathf.Sign(xMoveDelta);
		}
		else
		{
			_lookAheadPos = Vector3.MoveTowards(_lookAheadPos, Vector3.zero, Time.deltaTime * ResetSpeed);
		}

        if (GlobalVariables.TwoPlayer && GameManager.Instance.PlayerTwo != null)
        {
            Vector2 offset = 0.25f * (GameManager.Instance.PlayerTwo.transform.position - GameManager.Instance.Player.transform.position);
            _twoPlayerModifier = Vector2.ClampMagnitude(offset, 12);
        }
        else
        {
            _twoPlayerModifier = Vector2.zero;
        }

		Vector3 aheadTargetPos = _target.position + _lookAheadPos + Vector3.forward * _offsetZ + _lookDirectionModifier + CameraOffset + _twoPlayerModifier;

		Vector3 newCameraPosition = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref _currentVelocity, CameraSpeed);

		Vector3 shakeFactorPosition = new Vector3(0, 0, 0);

		// If shakeDuration is still running.
		if (_shakeDuration > 0)
		{
			shakeFactorPosition = Random.insideUnitSphere * _shakeIntensity * _shakeDuration;
			_shakeDuration -= _shakeDecay * Time.deltaTime;
		}
		newCameraPosition = newCameraPosition + shakeFactorPosition;

		// Clamp to level boundaries
		float posX = Mathf.Clamp(newCameraPosition.x, _xMin, _xMax);
		float posY = Mathf.Clamp(newCameraPosition.y, _yMin, _yMax);
		float posZ = newCameraPosition.z;

        // Lock for vertical-only sections
        if (LockX)
            posX = transform.position.x;

		// We move the actual transform
		transform.position = new Vector3(posX, posY, posZ);

		_lastTargetPosition = _target.position;
    }
	
	/// <summary>
	/// Handles the zoom of the camera based on the main character's speed
	/// </summary>
	protected virtual void Zoom()
	{
        if(_targetController == null) 
        {
            Debug.Log("No target controller!");
            return;
        }

		//float characterSpeed = Mathf.Abs(_targetController.Speed.x);
		//float currentVelocity = 0f;

		float aspect = Mathf.Max (_camera.aspect, 1.25f);

		float zoomScale = Mathf.Max (1.77777777778f / aspect, 1f);

        _currentZoom = zoomAdjust * Mathf.Max(zoomScale * MaximumZoom, MinimumZoom);

        //		Debug.Log (aspect + " " + zoomScale * MaximumZoom + " " + MinimumZoom);

        //		Debug.Log (_currentZoom + " " + zoomScale);

        _camera.orthographicSize = _currentZoom;
		GetLevelBounds();
	}

    /// <summary>
    /// Gets the levelbounds coordinates to lock the camera into the level
    /// </summary>
    protected virtual void GetLevelBounds()
	{
		// camera size calculation (orthographicSize is half the height of what the camera sees.
		float cameraHeight = Camera.main.orthographicSize * 2f;		
		float cameraWidth = cameraHeight * Camera.main.aspect;

        _xMin = _levelBounds.LeftLimit + (cameraWidth / 2);
        _xMax = _levelBounds.RightLimit - (cameraWidth / 2);
        _yMin = _levelBounds.BottomLimit + (cameraHeight / 2);
        _yMax = _levelBounds.TopLimit - (cameraHeight / 2);
    }
	
	/// <summary>
	/// Use this method to shake the camera, passing in a Vector3 for intensity, duration and decay
	/// </summary>
	/// <param name="shakeParameters">Shake parameters : intensity, duration and decay.</param>
	public virtual void Shake(Vector3 shakeParameters)
	{
        float scalar = 0.5f;

		// Mobile needs bigger shakes
#if UNITY_IOS || UNITY_ANDROID
		scalar = 1;
#endif

		_shakeIntensity = scalar * shakeParameters.x;
		_shakeDuration = shakeParameters.y;
		_shakeDecay = shakeParameters.z;
	}

    /// <summary>
    /// Moves the camera up
    /// </summary>
    public virtual void LookUp()
	{
        if (FollowsPlayer)
            _lookDirectionModifier = new Vector3(0,_manualUpDownLookDistance,0);
	}


    /// <summary>
    /// Moves the camera down
    /// </summary>
    public virtual void LookDown()
	{
        if (FollowsPlayer && !GameManager.Instance.Player.BehaviorState.CanPickup && !GameManager.Instance.Player.BehaviorState.Pickup)
            _lookDirectionModifier = new Vector3(0,-_manualUpDownLookDistance, 0);
	}

    /// <summary>
    /// Resets the look direction modifier
    /// </summary>
    public virtual void ResetLookUpDown()
	{	
		_lookDirectionModifier = new Vector3(0,0,0);
	}
	
	
}