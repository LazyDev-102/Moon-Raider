using UnityEngine;
using System.Collections;

public class GUIScalar : MonoBehaviour
{
    public float DesktopScale = 0.65f;
    public float MobileScale = 1.0f;


    // Use this for initialization
    void Start()
    {
#if UNITY_IOS || UNITY_ANDROID
		GetComponent<RectTransform>().localScale = MobileScale * Vector3.one;
#else
        GetComponent<RectTransform>().localScale = DesktopScale * Vector3.one;
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}
