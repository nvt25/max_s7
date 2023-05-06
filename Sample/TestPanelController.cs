using API.Ads;
using API.IAP;
using API.LogEvent;
using API.RemoteConfig;
using API.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPanelController : BaseUIComp
{
    public Text RemoteText;
    public Button ShowBanner;
    public Button HideBanner;
    public Button ShowFull;
    public Button ShowReward;
    public Button ShowFullReward;
    public Button RemoveAds;
    public Text AdsText;
    public Button LogStartLevel;
    public Button LogEndLevel;
    public Button LogUseItem;
    public Text LogText;
    private float levelStartTime;
    // Start is called before the first frame update
    void Start()
    {
        RemoteConfigManager.Ins.OnFetchComplete += OnFirebaseFetchComplete;
        ShowBanner.onClick.AddListener(OnShowBannerClick);
        HideBanner.onClick.AddListener(OnHideBannerClick);
        ShowFull.onClick.AddListener(OnShowFullClick);
        ShowReward.onClick.AddListener(OnShowRewardClick);
        ShowFullReward.onClick.AddListener(OnShowFullRewardClick);
        RemoveAds.onClick.AddListener(OnRemoveAdsClick);
        LogStartLevel.onClick.AddListener(OnLogStartLevelClick);
        LogEndLevel.onClick.AddListener(OnLogEndLevelClick);
        LogUseItem.onClick.AddListener(OnLogUseItemClick);
    }

    private void OnFirebaseFetchComplete()
    {
        RemoteText.text = "FetchComplete";
    }

    private void OnShowFullRewardClick()
    {
        AdsText.text = "Click Show Full Reward";
        //AdManager.Ins.ShowFullReward("show_full_reward_button", (IsComplete) => 
        //{
        //    Debug.Log("RunCompleteCheck");
        //    if (IsComplete)
        //    {
        //        Debug.Log("CompleteTrue");
        //        AdsText.text = "Show Full Reward Close With Reward";
        //    }
        //    else
        //    {
        //        Debug.Log("CompleteFalse");
        //        AdsText.text = "Show Full Reward Fail Or Close";
        //    }
        //});
    }

    private void OnShowBannerClick()
    {
        AdsText.text = "Click Show Banner";
        AdManager.Ins.ShowBanner();
    }

    private void OnHideBannerClick()
    {
        AdsText.text = "Click Hide Banner";
        AdManager.Ins.HideBanner();
    }

    private void OnShowFullClick()
    {
        AdsText.text = "Click Show Full";
        AdManager.Ins.ShowFull("show_full_button", () =>
        {
            AdsText.text = "Show Full Close";
        });
    }

    private void OnShowRewardClick()
    {
        AdsText.text = "Click Show Reward";
        AdManager.Ins.ShowRewardedVideo("show_full_button", (IsComplete) =>
        {
            if (IsComplete)
            {
                AdsText.text = "Show Reward Close With Reward";
            }
            else
            {
                AdsText.text = "Show Reward Fail Or Close";
            }
        });
    }

    private void OnRemoveAdsClick()
    {
#if UNITY_EDITOR
#if USE_IAP
        BaseIAPManager.Ins.PurchaseButtonClick("removeads", (IsPurchase) =>
        {
            if (IsPurchase)
            {
                AdsText.text = "Ads Removed";
                AdManager.Ins.RemoveAds();
            }
            else
            {
                AdsText.text = "Cancel Purchase";
            }
        });
        return;
#else
        AdsText.text = "Ads Removed";
        AdManager.Ins.RemoveAds();
#endif
#endif
        AdsText.text = "Ads Removed";
        AdManager.Ins.RemoveAds();
    }

    private void OnLogStartLevelClick()
    {
        LogText.text = "Log Start Level Event";
        levelStartTime = Time.time;
        if (LogEventManager.Ins)
        {
            LogEventManager.Ins.OnLevelStartLogEvent("test");
        }
    }

    private void OnLogEndLevelClick()
    {
        LogText.text = "Log End Level Event";
        if (LogEventManager.Ins)
        {
            LogEventManager.Ins.OnLevelCompleteLogEvent("test", Time.time - levelStartTime);
        }
    }

    private void OnLogUseItemClick()
    {
        LogText.text = "Log Use Item Event";
        if (LogEventManager.Ins)
        {
            LogEventManager.Ins.OnUseItemLog("test", "test");
        }
    }
}
