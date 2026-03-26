using UnityEngine;

public class CheckForLicense : MonoBehaviour
{
    private const string m_PublicKey_Base64 = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsHWKMt5dY2e3+/jOziTMThVLvaMnK8aai/MzlvTKfIgGQDJuJZOf7pKJui+CtavnXaeX4nGoLR3ioopb8hw8CmvW3dJei3x68YTQgn9jETEBkjkQZnSCN+clQKvxUeZlzNFrpqvAxn02wCnaootQX+AoQt/ZSRYj1n5AWWP9R06mYnD+4h1QgYexNKnoq5J8MrSChZ9Yl9VMfwTiRwpDpPclO3qBy2m6xcdU4Bed8HwARPBTBLDLG5U1EBCv9QoE856hWdX4eXduMZb661z072pGuvNSCR5Qa47ErfC9Ya6/JXcaGSrCwOc6nheOi2KxZyyO1TEVb/2O2C3JokIdUwIDAQAB";

    static CheckForLicense s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }


    class LicensingServiceCallback : AndroidJavaProxy
    {
        private CheckForLicense m_CheckLicenseButton;

        public LicensingServiceCallback(CheckForLicense checkLicenseButton)
            : base("com.google.licensingservicehelper.LicensingServiceCallback") {
            m_CheckLicenseButton = checkLicenseButton;
        }

        public void allow(string payloadJson)
        {
            Debug.Log("Google Play Pass: allow access");
            m_CheckLicenseButton.processResponse(true, payloadJson);
        }

        public void dontAllow(AndroidJavaObject pendingIntent)
        {
            Debug.Log("Google Play Pass: deny access");
            m_CheckLicenseButton.processResponse(false, "Deny access");
            m_CheckLicenseButton.m_LicensingHelper.Call("showPaywall", pendingIntent);
            m_CheckLicenseButton.m_Activity.Call("finish");
        }

        public void applicationError(string errorMessage)
        {
            Debug.Log("Google Play Pass: application error");
            m_CheckLicenseButton.processResponse(false, "Application error: " + errorMessage);
        }
    }

    private AndroidJavaObject m_Activity;
    private AndroidJavaObject m_LicensingHelper;

    System.Action<bool> m_OnDone;

    void Start()
    {
        bool runningOnAndroid = new AndroidJavaClass("android.os.Build").GetRawClass() != System.IntPtr.Zero;
        
        if (!runningOnAndroid) {
            Debug.Log("Google Play Pass: Not on Android");
            GlobalVariables.SubscriptionService = false;
            GlobalVariables.GameLocked = false;
            return;
        }

        GlobalVariables.SubscriptionService = true;

        m_Activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        m_LicensingHelper = new AndroidJavaObject("com.google.licensingservicehelper.LicensingServiceHelper", m_Activity, m_PublicKey_Base64);
    }

    public static void CheckLicense(System.Action<bool> OnDone)
    {
        s_instance.CheckLicenseHelper(OnDone);
    }

    private void CheckLicenseHelper(System.Action<bool> OnDone)
    {
        m_OnDone = OnDone;
        m_LicensingHelper.Call("checkLicense", new LicensingServiceCallback(this));
    }

    private void processResponse(bool isAllowed, string result)
    {
        m_OnDone?.Invoke(isAllowed);
    }
}