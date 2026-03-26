using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine.Advertisements;
using System.Collections;
public enum UserConsent
{
    Unset = 0,
    Accept = 1,
    Deny = 2
}
public enum BannerBoxPos // your custom enumeration
{
    bottomleft,
    bottomRight,
    TopLeft, TopRight, Center, Top, Bottom, CenterLeft, CenterRight
};
public class AdmobAds : MonoBehaviour,IUnityAdsInitializationListener,IUnityAdsLoadListener,IUnityAdsShowListener 
{
    public string GameID = "ca-app-pub-9496460709444277~8576776327";
    public string UnityAdId = "3711077";
    string mySurfacingId = "rewardedVideo";
    string mySurfacingIdbanner = "banner";
    public string myInterId = "video";
    // Sample ads
    public string bannerAdId1 = "ca-app-pub-3940256099942544/6300978111";
    public string bannerAdId2 = "ca-app-pub-3940256099942544/6300978111";
    public string bannerAdId3 = "ca-app-pub-3940256099942544/6300978111";
    public string bannerAdId4 = "ca-app-pub-3940256099942544/6300978111";
    public string bannerAdId5 = "ca-app-pub-3940256099942544/6300978111";
    public string InterstitialAdID = "ca-app-pub-3940256099942544/1033173712";
    public string rewarded_Ad_ID = "ca-app-pub-3940256099942544/5224354917";

    public BannerBoxPos boxbannerpos = BannerBoxPos.bottomleft;
    public BannerView bannerAdBottomLeft, bannerAdBottomRight, bannerAdTopLeft, bannerAdTopRight, BannerAdBox;
    public InterstitialAd interstitial;
    public RewardedAd rewardedAd;
    private const string userConsent = "UserConsent";
    private const string ccpaConsent = "CcpaConsent";
    private const string removeAds = "RemoveAds";
    private static bool initialized;
    public static AdmobAds instance;
    bool IsReady;
    private bool unityRewarded;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);


        MobileAds.Initialize(initStatus => {
            requestInterstital();
            loadRewardVideo();

        });
        Advertisement.Initialize(UnityAdId,false,this);
        initialized = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        //Advertisement.AddListener(this);
        //MobileAds.Initialize(GameID);
        // unity banner callling 	//StartCoroutine(ShowBannerWhenInitialized());

    }
    //set user consents and get user consents 

    public void SetUserConsent(bool accept)
    {
        if (accept == true)
        {
            PlayerPrefs.SetInt(userConsent, (int)UserConsent.Accept);
        }
        else
        {
            PlayerPrefs.SetInt(userConsent, (int)UserConsent.Deny);
        }
        if (initialized == true)
        {
            UpdateUserConsent();
        }
    }
    public void SetCCPAConsent(bool accept)
    {
        if (accept == true)
        {
            PlayerPrefs.SetInt(ccpaConsent, (int)UserConsent.Accept);
        }
        else
        {
            PlayerPrefs.SetInt(ccpaConsent, (int)UserConsent.Deny);
        }
        if (initialized == true)
        {
            UpdateUserConsent();
        }
    }

    public bool CanShowAds()
    {
        if (!PlayerPrefs.HasKey(removeAds))
        {
            return true;
        }
        else
        {
            if (PlayerPrefs.GetInt(removeAds) == 0)
            {
                return true;
            }
        }
        return false;
    }
    public void RemoveAds(bool remove)
    {
        if (remove == true)
        {
            PlayerPrefs.SetInt(removeAds, 1);
            //if banner is active and user bought remove ads the banner will automatically hide
            hideBannerBottomLeft();
            hideBannerBottomRight();
            hideBannerTopLeft();
            hideBannerTopRight();
        }
        else
        {
            PlayerPrefs.SetInt(removeAds, 0);
        }
    }
    private void UpdateUserConsent()
    {
        UpdateConsent(GetConsent(userConsent), GetConsent(ccpaConsent));
    }
    public void UpdateConsent(UserConsent consent, UserConsent ccpaConsent)
    {

    }
    private UserConsent GetConsent(string fileName)
    {
        if (!ConsentWasSet(fileName))
            return UserConsent.Unset;
        return (UserConsent)PlayerPrefs.GetInt(fileName);
    }

    private bool ConsentWasSet(string fileName)
    {
        return PlayerPrefs.HasKey(fileName);
    }
    public UserConsent GetUserConsent()
    {
        return GetConsent(userConsent);
    }
    public bool UserConsentWasSet()
    {
        return PlayerPrefs.HasKey(userConsent);
    }

    public bool CCPAConsentWasSet()
    {
        return PlayerPrefs.HasKey(ccpaConsent);
    }
    //end consents

    #region rewarded Video Ads
    public void loadRewardVideo()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest.Builder().Build();

        // send the request to load the ad.
        RewardedAd.Load(rewarded_Ad_ID, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                rewardedAd = ad;
                RegisterEventHandlers(rewardedAd);
            });
    }
    public void showAdmobRewardedVideoAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                admanager.instance.rewardOfRewardedVideo();
                Debug.Log(String.Format( reward.Type, reward.Amount));
            });
        }
    }
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            loadRewardVideo();
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
    public IEnumerator ShowBannerWhenInitialized()
    {
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.5f);
        }
        if (CanShowAds())
        {
            Advertisement.Banner.Show(mySurfacingIdbanner);
        }
        Advertisement.Banner.SetPosition(BannerPosition.CENTER);
    }
    public void showUnityInterstitialAd()
    {
        if (CanShowAds())
        {
            // Check if UnityAds ready before calling Show method:
            if (Advertisement.isInitialized)
            {
                Debug.Log("testing");
                Advertisement.Show(myInterId);
            }
            else
            {
                Debug.Log("Interstitial ad not ready at the moment! Please try again later!");
            }
        }


    }
    //unity rewarded ADs

        public void ShowUnityRewardedVideo()
    {
        // Check if UnityAds ready before calling Show method:
        if (Advertisement.isInitialized)
        {
            Debug.Log("unity ads is running");
            Advertisement.Show(mySurfacingId);
        }
        else
        {
            Debug.Log("Rewarded video is not ready at the moment! Please try again later!");
        }
    }

    // Implement IUnityAdsListener interface methods:

    #endregion

    #region banner

    public void reqBannerAdBottomLeft()
    {
        if (bannerAdBottomLeft == null)
        {
            if (bannerAdBottomLeft != null)
            {
                hideBannerBottomLeft();
            }

            // Create a 320x50 banner at top of the screen
            bannerAdBottomLeft = new BannerView(bannerAdId1, AdSize.Banner, AdPosition.BottomLeft);
        }
        var adRequest = new AdRequest.Builder()
              .AddKeyword("unity-admob-sample")
              .Build();
        ListenToAdEvents();
        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        bannerAdBottomLeft.LoadAd(adRequest);
    }
    private void ListenToAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        bannerAdBottomLeft.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerAdBottomLeft.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerAdBottomLeft.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        bannerAdBottomLeft.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        bannerAdBottomLeft.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerAdBottomLeft.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerAdBottomLeft.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerAdBottomLeft.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
  public void hideBannerBottomLeft()
    {
        if (bannerAdBottomLeft != null)
        {
            //bannerAdBottomLeft.Destroy();
            bannerAdBottomLeft.Destroy();
            bannerAdBottomLeft = null;
        }
        //}
    }
    public void reqBannerAdBottomRight()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.

        if (bannerAdBottomRight == null)
        {
            if (bannerAdBottomRight != null)
            {
                hideBannerBottomRight();
            }

            // Create a 320x50 banner at top of the screen
            bannerAdBottomRight = new BannerView(bannerAdId2, AdSize.Banner, AdPosition.BottomRight);
        }
        var adRequest = new AdRequest.Builder()
               .AddKeyword("unity-admob-sample")
               .Build();
        ListenToAdEvents2();
        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        bannerAdBottomRight.LoadAd(adRequest);
 }
    private void ListenToAdEvents2()
    {
        // Raised when an ad is loaded into the banner view.
        bannerAdBottomRight.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerAdBottomRight.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerAdBottomRight.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        bannerAdBottomRight.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        bannerAdBottomRight.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerAdBottomRight.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerAdBottomRight.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerAdBottomRight.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    public void hideBannerBottomRight()
    {
        if (bannerAdBottomRight != null)
        {
            bannerAdBottomRight.Destroy();
          //  bannerAdBottomRight.Destroy();
            bannerAdBottomRight = null;
        }
    }
    public void reqBannerAdTopLeft()
    {
        Debug.Log("Creating banner view");

        // create an instance of a banner view first.
        if (bannerAdTopLeft == null)
        {
            if (bannerAdTopLeft != null)
            {
                hideBannerTopLeft();
            }

            // Create a 320x50 banner at top of the screen
            bannerAdTopLeft = new BannerView(bannerAdId3, AdSize.Banner, AdPosition.TopLeft);
        }
        // create our request used to load the ad.
        var adRequest = new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();
        ListenToAdEvents3();
        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        bannerAdTopLeft.LoadAd(adRequest);
 }
    private void ListenToAdEvents3()
    {
        // Raised when an ad is loaded into the banner view.
        bannerAdTopLeft.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerAdTopLeft.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerAdTopLeft.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        bannerAdTopLeft.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        bannerAdTopLeft.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerAdTopLeft.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerAdTopLeft.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerAdTopLeft.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    public void hideBannerTopLeft()
    {
        if (bannerAdTopLeft != null)
        {
            //bannerAdTopLeft.Destroy();
            bannerAdTopLeft.Destroy();
            bannerAdTopLeft = null;
        }
    }
    public void reqBannerAdTopRight()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (bannerAdTopRight == null)
        {
            if (bannerAdTopRight != null)
            {
                hideBannerTopRight();
            }

            // Create a 320x50 banner at top of the screen
            bannerAdTopRight = new BannerView(bannerAdId3, AdSize.Banner, AdPosition.TopRight);
        }

        var adRequest = new AdRequest.Builder()
           .AddKeyword("unity-admob-sample")
           .Build();
        ListenToAdEvents4();
        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        bannerAdTopRight.LoadAd(adRequest);
 }
    private void ListenToAdEvents4()
    {
        // Raised when an ad is loaded into the banner view.
        bannerAdTopRight.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerAdTopRight.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerAdTopRight.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        bannerAdTopRight.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        bannerAdTopRight.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerAdTopRight.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerAdTopRight.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerAdTopRight.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    public void hideBannerTopRight()
    {
        if (bannerAdTopRight != null)
        {

            bannerAdTopRight.Destroy();
            bannerAdTopRight = null;
        }
    }

    public void reqBannerAdBox()
    {
        if (CanShowAds())
        {
            if (BannerAdBox == null)
            {
                if (boxbannerpos == BannerBoxPos.bottomleft)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.BottomLeft);
                else if (boxbannerpos == BannerBoxPos.bottomRight)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.BottomRight);
                else if (boxbannerpos == BannerBoxPos.TopLeft)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.TopLeft);
                else if (boxbannerpos == BannerBoxPos.TopRight)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.TopRight);
                else if (boxbannerpos == BannerBoxPos.Center)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.Center);
                else if (boxbannerpos == BannerBoxPos.Top)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.Top);
                else if (boxbannerpos == BannerBoxPos.Bottom)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.Bottom);
                else if (boxbannerpos == BannerBoxPos.CenterLeft)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.BottomLeft);
                else if (boxbannerpos == BannerBoxPos.CenterRight)
                    BannerAdBox = new BannerView(bannerAdId5, AdSize.MediumRectangle, AdPosition.BottomRight);
                // Called when an ad request has successfully loaded.
                ListenToAdEvents5();

                AdRequest request = new AdRequest.Builder().Build();

                BannerAdBox.LoadAd(request);
            }
            else
            {

                hidereqBannerAdBox();
                reqBannerAdBox();
            }
        }
    }
    private void ListenToAdEvents5()
    {
        // Raised when an ad is loaded into the banner view.
        BannerAdBox.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + BannerAdBox.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        BannerAdBox.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        BannerAdBox.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        BannerAdBox.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        BannerAdBox.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        BannerAdBox.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        BannerAdBox.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    public void hidereqBannerAdBox()
    {
        if (BannerAdBox != null)
        {

            BannerAdBox.Hide();
            BannerAdBox = null;
        }
 }
    #endregion

    #region interstitial

    public void requestInterstital()
    {

        // Clean up the old ad before loading a new one.
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest.Builder().Build();

        // send the request to load the ad.
        InterstitialAd.Load(InterstitialAdID, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());
                interstitial = ad;
                RegisterEventHandlers(interstitial);
            });
    }

    public void ShowAdmobInterstitialAd()
    {
        if (interstitial != null && interstitial.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            interstitial.Show();
        }
        else
        {
            requestInterstital();
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            requestInterstital();
            Debug.Log("Interstitial ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    public void OnInitializationComplete()
    {
        Debug.LogError("Unity Ads initialization Completed");
        Advertisement.Load(mySurfacingId, this);
        Advertisement.Load(myInterId, this);
        // throw new NotImplementedException();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        IsReady = true;
        //  throw new NotImplementedException();
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
      
        // throw new NotImplementedException();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError("Unity Ads loading failed " + placementId + " " + error + " " + message);
        IsReady = false;
        //  throw new NotImplementedException();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
     
        
        //  throw new NotImplementedException();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
       
        
        // throw new NotImplementedException();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
      
        
        // throw new NotImplementedException();
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED && unityRewarded)
        {
            Debug.Log("i got the reward from unity");
            unityRewarded = false;
            admanager.instance.rewardOfRewardedVideo();
        }
        else if (showCompletionState == UnityAdsShowCompletionState.SKIPPED)
        {
            Debug.LogError("The " + placementId + " did not finish due to skip.");
        }
        else if (showCompletionState == UnityAdsShowCompletionState.UNKNOWN)
        {
            Debug.LogError("The " + placementId + " did not finish due to an Unknown error.");
        }
        if (placementId == myInterId)
        {
            Debug.LogError("Load Unity Inter");
            Advertisement.Load(myInterId, this);
        }
        if (placementId == mySurfacingId)
        {
            Debug.LogError("Load Unity Reward");
            Advertisement.Load(mySurfacingId, this);
        }

        //  throw new NotImplementedException();
    }


    #endregion

    #region adDelegates










    #endregion

}
