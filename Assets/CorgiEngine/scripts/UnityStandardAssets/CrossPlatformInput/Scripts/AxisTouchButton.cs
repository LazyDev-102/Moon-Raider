using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnitySampleAssets.CrossPlatformInput;

public class AxisTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    // designed to work in a pair with another axis touch button
    // (typically with one having -1 and one having 1 axisValues)
    public string axisName = "Horizontal";                  // The name of the axis
    public float axisValue = 1;                             // The axis that the value has
    public float responseSpeed = 3;                         // The speed at which the axis touch button responds
    public float returnToCentreSpeed = 3;                   // The speed at which the button will return to its centre

    public GameObject PadZone;
   // public GameObject DeadZone;
    public GameObject LeftArrow;
    public GameObject RightArrow;
    public GameObject UpArrow;
    public GameObject DownArrow;

    private RectTransform Pad;
    private RectTransform Dead;
    private RectTransform Left;
    private RectTransform Right;
    private RectTransform Up;
    private RectTransform Down;

    private Image UpImage;
    private Image DownImage;
    private Image LeftImage;
    private Image RightImage;

    private Color OnColor;
    private Color OffColor;


    private Camera GUICamera;

    private AxisTouchButton pairedWith;                      // Which button this one is paired with

    protected static CharacterBehavior _player;

    void Start()
    {
#if UNITY_STANDALONE || UNITY_TVOS
        enabled = false;
#endif

        //Debug.Log("AXIS ENABLED");
        Pad = PadZone.GetComponent<RectTransform>();
        //Dead = DeadZone.GetComponent<RectTransform>();
        Left = LeftArrow.GetComponent<RectTransform>();
        Right = RightArrow.GetComponent<RectTransform>();
        Up = UpArrow.GetComponent<RectTransform>();
        Down = DownArrow.GetComponent<RectTransform>();

        UpImage = UpArrow.GetComponent<Image>();
        DownImage = DownArrow.GetComponent<Image>();

        LeftImage = LeftArrow.GetComponent<Image>();
        RightImage = RightArrow.GetComponent<Image>();
    }


    void OnEnable()
    {
        checkPlayer();

        OnColor = new Color(1.0f, 1.0f, 1.0f, 0.375f);
        OffColor = new Color(1.0f, 1.0f, 1.0f, 0.125f);

        GUICamera = GUIManager.Instance.GetComponent<Camera>();

        FindPairedButton();
    }

    void FindPairedButton()
    {
        // find the other button witch which this button should be paired
        // (it should have the same axisName)
        var otherAxisButtons = FindObjectsOfType(typeof(AxisTouchButton)) as AxisTouchButton[];

        if (otherAxisButtons != null)
        {
            for (int i = 0; i < otherAxisButtons.Length; i++)
            {
                if (otherAxisButtons[i].axisName == axisName && otherAxisButtons[i] != this)
                {
                    pairedWith = otherAxisButtons[i];
                    //Debug.Log("PAIR FOUND");
                }
            }
        }
    }

    void OnDisable()
    {
        //Debug.Log("AXIS DISABLED");
        // The object is disabled so remove it from the cross platform input system
        //axis.Remove();
    }

    bool checkPlayer()
    {
#if UNITY_STANDALONE || UNITY_TVOS
        return true;
#else 
        if (_player == null)
        {
            if (GameManager.Instance.Player != null)
            {
                if (GameManager.Instance.Player.GetComponent<CharacterBehavior>() != null)
                {
                    _player = GameManager.Instance.Player;
                    return (_player != null);
                }
            }
            else
            {
                //Debug.Log("NULL player!");
                if(GameObject.FindWithTag("Player") != null)
                    GameManager.Instance.Player = GameObject.FindWithTag("Player").GetComponent<CharacterBehavior>();
                _player = GameManager.Instance.Player;
                return (_player != null);
            }
        }

        return true;
#endif
    }


    void HandleArrows(PointerEventData data)
    {
#if UNITY_STANDALONE || UNITY_TVOS
        return;
#else
        //if (pairedWith == null)
        //{
        //    FindPairedButton();
        //}

        if (!GameManager.Instance.CanMove)
        {
            _player.SetVerticalMove(0);
            _player.SetHorizontalMove(0);
            LeftImage.color = OffColor;
            RightImage.color = OffColor;
            return;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(Pad, data.position, data.pressEventCamera))
        {
            Vector2 guiPOS = Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Pad, data.position, GUICamera, out guiPOS);

            float width = Pad.rect.width / 2;
            float height = Pad.rect.height / 2;

            float h = Mathf.Clamp(2 * guiPOS.x / width, -1, 1);
            float v = Mathf.Clamp(2 * guiPOS.y / height, -1, 1);

            // Debug.Log(guiPOS + " vs " + h + " " + v); 

            float deadzone = 0.25f;
            Vector2 stickInput = new Vector2(h, v);
            if (stickInput.magnitude < deadzone)
                stickInput = Vector2.zero;
            else
                stickInput = stickInput.normalized * ((stickInput.magnitude - deadzone) / (1 - deadzone));

            float trueY = 0;
            // Ignore up / down analog unless swimming or meleeing
            if (!_player.BehaviorState.Swimming && !_player.BehaviorState.MeleeEnergized)
                trueY = 0;
            else
                trueY = stickInput.y;

            _player.SetHorizontalMove(stickInput.x);

            if (stickInput.x > 0.1f)
            {
                LeftImage.color = OffColor;
                RightImage.color = OnColor;
            }
            else if (stickInput.x < -0.1f)
            {
                LeftImage.color = OnColor;
                RightImage.color = OffColor;
            }
            else
            {
                LeftImage.color = OffColor;
                RightImage.color = OffColor;
            }

            if (trueY > 0.1f)
            {
                DownImage.color = OffColor;
                UpImage.color = OnColor;
            }
            else if (trueY < -0.1f)
            {
                DownImage.color = OnColor;
                UpImage.color = OffColor;
            }
            else
            {
                DownImage.color = OffColor;
                UpImage.color = OffColor;
            }
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(Left, data.position, data.pressEventCamera))
        {
            _player.SetVerticalMove(0);
            _player.SetHorizontalMove(-1);
            LeftImage.color = OnColor;
            RightImage.color = OffColor;
            DownImage.color = OffColor;
            UpImage.color = OffColor;
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(Right, data.position, data.pressEventCamera))
        {
            _player.SetVerticalMove(0);
            _player.SetHorizontalMove(1);
            LeftImage.color = OffColor;
            RightImage.color = OnColor;
            DownImage.color = OffColor;
            UpImage.color = OffColor;
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(Up, data.position, data.pressEventCamera))
        {
            _player.SetVerticalMove(1);
            _player.SetHorizontalMove(0);
            LeftImage.color = OffColor;
            RightImage.color = OffColor;
            DownImage.color = OffColor;
            UpImage.color = OnColor;
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(Down, data.position, data.pressEventCamera))
        {
            _player.SetVerticalMove(-1);
            _player.SetHorizontalMove(0);
            LeftImage.color = OffColor;
            RightImage.color = OffColor;
            DownImage.color = OnColor;
            UpImage.color = OffColor;
        }
        else
        {
            _player.SetVerticalMove(0);
            _player.SetHorizontalMove(0);
            LeftImage.color = OffColor;
            RightImage.color = OffColor;
            DownImage.color = OffColor;
            UpImage.color = OffColor;
        }
#endif
    }


    public void OnPointerDown(PointerEventData data)
    {
        HandleArrows(data);
    }

    public void OnPointerUp(PointerEventData data)
    {
#if UNITY_IOS || UNITY_ANDROID
        _player.SetVerticalMove(0);
        _player.SetHorizontalMove(0);
        LeftImage.color = OffColor;
        RightImage.color = OffColor;
        DownImage.color = OffColor;
        UpImage.color = OffColor;
#endif
    }

    public void OnDrag(PointerEventData data)
    {
        HandleArrows(data);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        HandleArrows(data);
    }

    public void OnEndDrag(PointerEventData data)
    {
#if UNITY_IOS || UNITY_ANDROID
        _player.SetVerticalMove(0);
        _player.SetHorizontalMove(0);
#endif
    }
}
