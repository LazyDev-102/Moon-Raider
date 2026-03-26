using UnityEngine;
using System.Collections;

namespace UnitySampleAssets.CrossPlatformInput
{
    public class PressureTouch : MonoBehaviour
    {
        float touchPressure;
        public string Name;

        void Update()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                touchPressure = Input.GetTouch(i).pressure;

                if (touchPressure <= 0.665f)
                {
                    CrossPlatformInputManager.SetButtonDown(Name);
                    break;
                }
            }
        }
    }
}
