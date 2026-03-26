using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
public class admanager : MonoBehaviour
{

    public static admanager instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }
    public bool AdmobPriorityInter, UnityPriorityInter, AdmobPriorityRewarded, UnityPriorityRewarded;
    // Start is called before the first frame update
    void Start()
    {
        //AdmobAds.instance.SetUserConsent(true);
        //AdmobAds.instance.SetCCPAConsent(true);
        //AdmobAds.instance.requestInterstital();
        //AdmobAds.instance.loadRewardVideo();

    }
    void ShowUnityBannerAd()
    {
        //StartCoroutine(AdmobAds.instance.ShowBannerWhenInitialized());

    }
    public void ShowGenericVideoAd()
    {
        if (AdmobPriorityInter)
        {
            if (AdmobAds.instance.interstitial.CanShowAd())
            {
                showVideoAd(); // admob inter ad

            }
            else
            {
                AdmobAds.instance.showUnityInterstitialAd();
            }
        }
        else if (UnityPriorityInter)
        {
            if (Advertisement.isInitialized)
            {

                AdmobAds.instance.showUnityInterstitialAd();
            }
            else
            {

                showVideoAd();
            }

        }


    }
    public void ShowRewardedVideAdGeneric(int i)
    {
        PlayerPrefs.SetInt("RewardKey", i);
        if (UnityPriorityRewarded)
        {
            if (Advertisement.isInitialized)
            {
                AdmobAds.instance.ShowUnityRewardedVideo();

            }
            else
            {
                showRewardedVideoAd();
            }

        }
        if (AdmobPriorityRewarded)
        {
            if (AdmobAds.instance.rewardedAd.CanShowAd())
            {
                showRewardedVideoAd();

            }
            else
            {
                AdmobAds.instance.ShowUnityRewardedVideo();
            }

        }


        //AdmobAds.instance.OnUnityAdsDidFinish("rewardedVideo", ShowResult showResult);
    }

    public void showbannerbottomLeft()
    {

        AdmobAds.instance.reqBannerAdBottomLeft();
    }
    public void showbannerbottomRight()
    {

        AdmobAds.instance.reqBannerAdBottomRight();
    }
    public void showbannerTopRight()
    {

        AdmobAds.instance.reqBannerAdTopRight();
    }
    public void showbannerTopLeft()
    {
        AdmobAds.instance.reqBannerAdTopLeft();

    }
    public void showBoxBanner(int i)
    {
        if (i == 0)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.CenterLeft;
        if (i == 1)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.CenterRight;
        if (i == 2)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.Top;
        if (i == 3)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.Bottom;
        if (i == 4)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.bottomleft;
        if (i == 5)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.bottomRight;
        if (i == 6)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.TopLeft;
        if (i == 7)
            AdmobAds.instance.boxbannerpos = BannerBoxPos.TopRight;

        AdmobAds.instance.reqBannerAdBox();
    }
    public void hideBottomLeftBanner()
    {
        AdmobAds.instance.hideBannerBottomLeft();
    }
    public void hideBottomRightBanner()
    {
        AdmobAds.instance.hideBannerBottomRight();
    }
    public void hideTopLeftBanner()
    {
        AdmobAds.instance.hideBannerTopLeft();
    }
    public void hideTopRightBanner()
    {
        AdmobAds.instance.hideBannerTopRight();
    }
    public void hideBoxBanner()
    {
        AdmobAds.instance.hidereqBannerAdBox();
    }

    void showVideoAd()
    {
        AdmobAds.instance.ShowAdmobInterstitialAd();

    }
    void showRewardedVideoAd()
    {
        AdmobAds.instance.showAdmobRewardedVideoAd();
        //AdmobAds.instance.HandleUserEarnedReward();

    }


    public void rewardOfRewardedVideo()
    {
        if (PlayerPrefs.GetInt("RewardKey") == 0)
        {
            //give extra day

        }
        if (PlayerPrefs.GetInt("RewardKey") == 1)
        {


        }
        if (PlayerPrefs.GetInt("RewardKey") == 2)
        {
            //give extra day

        }
        if (PlayerPrefs.GetInt("RewardKey") == 3)
        {
            //give extra day

        }
        if (PlayerPrefs.GetInt("RewardKey") == 4)
        {
            //give extra day

        }
        if (PlayerPrefs.GetInt("RewardKey") == 5)
        {
            //give extra day

        }

    }

}
