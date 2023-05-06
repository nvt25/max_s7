using API.LogEvent;
using API.RemoteConfig;
#if USE_FIREBASE_REMOTE
using Firebase.RemoteConfig;
#endif
#if USE_AOA
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace API.Ads
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Ins;

        public AdsSetting androidSetting;

        public AdsSetting iosSetting;

        private AdsSetting curSetting;

        public bool isUseFireBase = false;

        public string androidFirebaseKey = "test_android";

        public string iosFirebaseKey = "test_ios";

        private bool IsAdsInited = false;

        private bool isAdsLeaveApp = false;

        public bool IsAdsLeaveApp
        {
            get => isAdsLeaveApp;
            set => isAdsLeaveApp = value;
        }

        private bool isAOAOff = false;

        public bool IsAOAOff => isAOAOff;

        private bool IsHideBanner = false;

        private int numInterShowed;

        private int numRewardShowed;

        private void Awake()
        {
            if (Ins == null)
            {
                Ins = this;
                DontDestroyOnLoad(transform.root.gameObject);
                if (isUseFireBase)
                {
#if USE_FIREBASE_REMOTE
                    StartCoroutine(WaitInitRemoveConfig());
#else
                string data = PlayerPrefs.GetString(StaticClass.ADS_SETTING, "O");
                if (data.Equals("O"))
                {
                    curSetting = StaticClass.IsAndroid() ? androidSetting : iosSetting;
                    PlayerPrefs.SetString(StaticClass.ADS_SETTING, JsonUtility.ToJson(curSetting));
                }
                else
                {
                    curSetting = JsonUtility.FromJson<AdsSetting>(data);
                }
                InitAds();
#endif
                }
                else
                {
                    string data = PlayerPrefs.GetString(StaticClass.ADS_SETTING, "O");
                    if (data.Equals("O"))
                    {
                        curSetting = StaticClass.IsAndroid() ? androidSetting : iosSetting;
                        PlayerPrefs.SetString(StaticClass.ADS_SETTING, JsonUtility.ToJson(curSetting));
                    }
                    else
                    {
                        curSetting = JsonUtility.FromJson<AdsSetting>(data);
                    }
                    InitAds();
                }
            }
            else
            {
                if (Ins != this)
                {
                    Destroy(transform.root.gameObject);
                }
            }
        }

#if USE_FIREBASE_REMOTE
        private IEnumerator WaitInitRemoveConfig()
        {
            yield return new WaitUntil(() => RemoteConfigManager.Ins);
            RemoteConfigManager.Ins.OnFetchComplete += OnFirebaseInitComplete;
        }
#endif

        private void Start()
        {
            numInterShowed = PlayerPrefs.GetInt("NumInterShowed", 0);
            numRewardShowed = PlayerPrefs.GetInt("NumRewardShowed", 0);
        }

#region Firebase
        private void OnFirebaseInitComplete()
        {
#if USE_FIREBASE_REMOTE
            string data = FirebaseRemoteConfig.DefaultInstance.GetValue(GetKey()).StringValue;
            data = data.CheckCorrect();
            Debug.Log("Key " + AdManager.Ins.GetKey() + ": " + data);
            if (data.Equals("O"))
            {
                data = PlayerPrefs.GetString(StaticClass.ADS_SETTING, "O");
                if (data.Equals("O"))
                {
                    curSetting = StaticClass.IsAndroid() ? androidSetting : iosSetting;
                    PlayerPrefs.SetString(StaticClass.ADS_SETTING, JsonUtility.ToJson(curSetting));
                }
                else
                {
                    curSetting = JsonUtility.FromJson<AdsSetting>(data);
                }
            }
            else
            {
                curSetting = JsonUtility.FromJson<AdsSetting>(data);
                PlayerPrefs.SetString(StaticClass.ADS_SETTING, data);
            }
            InitAds();
#endif
        }
#endregion
        private void InitAds()
        {
            Debug.Log("InitAds");
            string timeData = PlayerPrefs.GetString("LastTimeRefocusShow", "O");
            if (timeData == "O")
            {
                lastTimeRefocusShow = DateTime.Now.AddDays(-1);
            }
            else
            {
                lastTimeRefocusShow = DateTime.ParseExact(timeData, "O", CultureInfo.InvariantCulture);
            }
            lastTimeShowFull = Time.time - curSetting.TimeBetweenShowFull;
#if USE_AOA
            
            if (curSetting.IsUseAppOpenAd)
            {
                MobileAds.Initialize(status =>
                {
                    RequestAppOpen();
                });
            }
#endif

            Debug.Log("AdsInited");
#if USE_APPLOVIN_ADS
            if (curSetting.IsUseApplovin)
            {
                if (!string.IsNullOrEmpty(curSetting.SDK_KEY_APPLOVIN))
                {
                    MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
                    {
                        RequestRewardBasedVideo();
                        if (isAutoLoadFull)
                        {
                            RequestFull();
                            //RequestFullReward();
                        }
                        RequestApplovinBanner();
                        if (curSetting.IsBannerInStart && !IsHideBanner)
                        {
                            Debug.Log("IsBannerInStart");
                            ShowBanner();
                        }
                        IsAdsInited = true;
                    };
                    MaxSdk.SetSdkKey(curSetting.SDK_KEY_APPLOVIN);
                    MaxSdk.InitializeSdk();
                }
            }
            else
            {
                if (curSetting.IsBannerInStart && !IsHideBanner)
                {
                    Debug.Log("IsBannerInStart");
                    ShowBanner();
                }
                IsAdsInited = true;
            }
#else
            if (curSetting.IsBannerInStart && !IsHideBanner)
            {
                Debug.Log("IsBannerInStart");
                ShowBanner();
            }
            IsAdsInited = true;
#endif

        }
#region Banner
        private int numClickBanner = 0;
#if UNITY_2020_3_OR_NEWER
#else
        private bool isOpeningBanner;
#endif

        private bool bannerLoaded = false;
        /// <summary>
        /// Show banner ads
        /// </summary>
        public void ShowBanner()
        {
            if (!CanShowAds() || !curSetting.IsShowBanner)
            {
                Debug.Log("Can't Show Banner");
                return;
            }
            if (!CanRequestBanner())
            {
                Debug.Log("Can't Request Banner");
                return;
            }
            //return;
            if (!curSetting.IsUseApplovin)
            {
                return;
            }
            bannerLoaded = false;
#if USE_APPLOVIN_ADS
            Debug.Log("ShowBanner");
            MaxSdk.ShowBanner(curSetting.BANNER_ID_APPLOVIN);
#endif

        }
        /// <summary>
        /// Hide banner ads
        /// </summary>
        public void HideBanner()
        {
            if (!IsAdsInited)
            {
                IsHideBanner = true;
                return;
            }
#if USE_APPLOVIN_ADS
            if (curSetting.IsUseApplovin)
            {
                MaxSdk.HideBanner(curSetting.BANNER_ID_APPLOVIN);
            }
#endif
        }
        private void RequestApplovinBanner()
        {
            if (!CanShowAds())
            {
                Debug.Log("RequestCan'tShow");
                return;
            }

            if (!CanRequestBanner())
            {
                Debug.Log("RequestCan'tRequest");
                return;
            }

#if USE_APPLOVIN_ADS

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
            MaxSdk.CreateBanner(curSetting.BANNER_ID_APPLOVIN, MaxSdkBase.BannerPosition.BottomCenter);
            Color bannerBgColor;
            switch (curSetting.BannerColor)
            {
                case BannerColor.NoColor:
                    bannerBgColor = new Color(1f, 1f, 1f, 0f);
                    break;
                default:
                    bannerBgColor = Color.black;
                    break;
            }
            // Set background or background color for banners to be fully functional.
            MaxSdk.SetBannerBackgroundColor(curSetting.BANNER_ID_APPLOVIN, bannerBgColor);
#endif
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (LogEventManager.Ins)
            {
                LogEventManager.Ins.OnApplovinAdsRevenuePaid(adInfo, Adformat.banner, curSetting.BannerEventToken);
            }
        }

#if USE_APPLOVIN_ADS
        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            numClickBanner++;
            //LogEventManager.Ins.OnAdClickLogEvent("Banner", "Admob", numClickBanner);
            isAdsLeaveApp = true;
            if (!CanRequestBanner())
            {
                HideBanner();
            }
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            bannerLoaded = true;
        }

        private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Debug.Log("FailLoadBanner");
            bannerLoaded = false;
        }
#endif

        private bool CanRequestBanner()
        {
            return numClickBanner < curSetting.MaxClickBanner;
        }

#endregion

#region Full
        public bool isAutoLoadFull;

        private int curRetryFull;

        private int maxFullRetryCount = 3;

        private float lastTimeShowFull;

        private int numClickFull = 0;

        private UnityAction OnFullClosed;

        private int curCallNetworkTypeFull = 0;

        private int curCallNumFull = 0;
        
        /// <summary>
        /// Show full ads
        /// </summary>
        /// <param name="location">The location where the ads show</param>
        /// <param name="callback">When ads close</param>
        public void ShowFull(string location, UnityAction callback = null)
        {
            OnFullClosed = callback;
            if (!IsAdsInited)
            {
                StartCoroutine(CompleteMethodfull());
                return;
            }
            //#if UNITY_EDITOR
            //            if (callback != null)
            //            {
            //                callback.Invoke();
            //            }

            //            return;
            //#endif
            LogEventManager.Ins.OnCallShowInterstitialAds(MaxSdk.IsInterstitialReady(curSetting.FULL_ID_APPLOVIN), location);
            if (CanShowAds() && CanShowFull())
            {
                if (!curSetting.IsUseApplovin)
                {
                    StartCoroutine(CompleteMethodfull());
                    return;
                }
                if (MaxSdk.IsInterstitialReady(curSetting.FULL_ID_APPLOVIN))
                {
                    isAdsLeaveApp = true;
                    lastTimeShowFull = Time.time;
                    MaxSdk.ShowInterstitial(curSetting.FULL_ID_APPLOVIN);
                    //LogShowFull
                    //LogEventManager.Ins.OnAdShowLogEvent(location, "full");
                }
                else
                {
                    StartCoroutine(CompleteMethodfull());
                }
            }
            else
            {
                StartCoroutine(CompleteMethodfull());
            }
        }

        private void RequestFull()
        {
            if (!CanRequestFull())
            {
                return;
            }
#if USE_APPLOVIN_ADS
            if (curSetting.IsUseApplovin)
            {
                if (MaxSdk.IsInterstitialReady(curSetting.FULL_ID_APPLOVIN))
                {
                    return;
                }
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;

                MaxSdk.LoadInterstitial(curSetting.FULL_ID_APPLOVIN);

            }
#endif
        }

        private bool CanRequestFull()
        {
            return numClickFull < curSetting.MaxClickFull && CanShowAds();
        }

        private bool CanShowFull()
        {
            return Time.time - lastTimeShowFull >= curSetting.TimeBetweenShowFull;

        }
#if USE_APPLOVIN_ADS
        private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (LogEventManager.Ins)
            {
                LogEventManager.Ins.OnApplovinAdsRevenuePaid(adInfo, Adformat.interstitial, curSetting.InterstitialEventToken);
            }
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            curRetryFull = 0;
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (isAutoLoadFull)
            {
                MaxSdk.LoadInterstitial(curSetting.FULL_ID_APPLOVIN);
            }
            StartCoroutine(CompleteMethodfull());
            if (numInterShowed < 3)
            {
                numInterShowed++;
                PlayerPrefs.SetInt("NumInterShowed", numInterShowed);
                if (numInterShowed == 3)
                {
                    LogEventManager.Ins.OnInterstitialNumReach(curSetting.InterstitialImpToken, numInterShowed);
                }
            }
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            isAdsLeaveApp = true;
            numClickFull++;
            //LogEventManager.Ins.OnAdClickLogEvent("Interstitial", "Admob", numClickFull);
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Debug.Log("Interstitial AppLovin load failed");
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            curRetryFull++;
            float retryDelay = Mathf.Pow(2, Math.Min(6, curRetryFull));
            Invoke(nameof(WaitLoadFull), retryDelay);
        }

        private void WaitLoadFull()
        {
            MaxSdk.LoadInterstitial(curSetting.FULL_ID_APPLOVIN);
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Debug.LogError(errorInfo.Message);
            if (isAutoLoadFull)
                MaxSdk.LoadInterstitial(curSetting.FULL_ID_APPLOVIN);
            StartCoroutine(CompleteMethodfull());
        }
#endif
        private IEnumerator CompleteMethodfull()
        {
            Debug.Log("StartCompleteMethod");
            yield return null;
            if (OnFullClosed != null)
            {
                OnFullClosed();
                OnFullClosed = null;
            }
        }
        #endregion

        #region AppOpen
#if USE_AOA
        private int tierIndex = 1;

        private AppOpenAd appOpen;

        private DateTime loadTime;

        private bool showFirstOpen = false;

        private bool isShowingAd = false;

        private UnityAction OnAOAClosed;

        private void RequestAppOpen()
        {
            if (!CanShowAds())
                return;
            string id = curSetting.ID_TIER_1;
            if (tierIndex == 2)
                id = curSetting.ID_TIER_2;
            else if (tierIndex == 3)
                id = curSetting.ID_TIER_3;

            Debug.Log("Start request Open App Ads Tier " + tierIndex);

            AdRequest request = new AdRequest.Builder().Build();

            AppOpenAd.LoadAd(id, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
            {
                if (error != null)
                {
                    // Handle the error.
                    Debug.LogFormat("Failed to load the ad. (reason: {0}), tier {1}", error.LoadAdError.GetMessage(), tierIndex);
                    tierIndex++;
                    if (tierIndex <= 3)
                        RequestAppOpen();
                    else
                        tierIndex = 1;
                    return;
                }
                Debug.Log("OpenAdsLoaded");
                // App open ad is loaded.
                appOpen = appOpenAd;
                tierIndex = 1;
                loadTime = DateTime.UtcNow;
                if (!showFirstOpen)
                {
                    ShowAppOpen("first_open");
                    isAOAOff = true;
                    showFirstOpen = true;
                }
            }));
        }
#endif
        public void ShowAppOpen(string pos, UnityAction callback = null)
        {
            if (!CanShowAds())
                return;
#if USE_AOA
            OnAOAClosed = callback;
#if UNITY_EDITOR
            if (callback != null)
            {
                callback.Invoke();
            }

            return;
#endif
            if (appOpen != null && !isShowingAd)
            {
                Debug.Log("ShowOpenAds");
                appOpen.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
                appOpen.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
                appOpen.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
                appOpen.OnAdDidRecordImpression += HandleAdDidRecordImpression;
                appOpen.OnPaidEvent += HandlePaidEvent;
                isAdsLeaveApp = true;
                appOpen.Show();
                return;
            }
            else
            {
                OnAOAClosed?.Invoke();
            }
#else
            ShowFull(pos, callback);
#endif
        }
#if USE_AOA
        private void HandlePaidEvent(object sender, AdValueEventArgs e)
        {
            Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
               e.AdValue.CurrencyCode, e.AdValue.Value);
        }

        private void HandleAdDidRecordImpression(object sender, EventArgs e)
        {
            Debug.Log("Recorded ad impression");
        }

        private void HandleAdDidPresentFullScreenContent(object sender, EventArgs e)
        {
            Debug.Log("Displayed app open ad");
            isShowingAd = true;
        }

        private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs e)
        {
            Debug.LogFormat("Failed to present the ad (reason: {0})", e.AdError.GetMessage());
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            appOpen = null;
            RequestAppOpen();
            StartCoroutine(CompleteMethodAOA());
        }

        private void HandleAdDidDismissFullScreenContent(object sender, EventArgs e)
        {
            Debug.Log("Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            appOpen = null;
            isShowingAd = false;
            RequestAppOpen();
            StartCoroutine(CompleteMethodAOA());
        }

        private IEnumerator CompleteMethodAOA()
        {
            Debug.Log("StartCompleteMethod");
            yield return null;
            if (OnAOAClosed != null)
            {
                OnAOAClosed();
                OnAOAClosed = null;
            }
        }
#endif
        #endregion


        #region Reward
        private int numClickReward = 0;

        private bool triggerCompleteMethod;

        private int currentRetryRewardedVideo;

        private int maxRetryCount = 3;

        private UnityAction<bool> OnCompleteMethod;

        private int curCallNetworkTypeVideo = 0;

        private int curCallNumVideo = 0;
        /// <summary>
        /// Show reward ads
        /// </summary>
        /// <param name="location">The location where the ads show</param>
        /// <param name="callback">When ads close</param>
        public void ShowRewardedVideo(string location, UnityAction<bool> callback)
        {
            triggerCompleteMethod = true;
            OnCompleteMethod = callback;
//#if UNITY_EDITOR
//            if (callback != null)
//            {
//                callback(true);
//            }

//            return;
//#endif

            LogEventManager.Ins.OnCallShowRewardedVideoAds(MaxSdk.IsRewardedAdReady(curSetting.REWARD_ID_APPLOVIN), location);
            if (!curSetting.IsUseApplovin)
            {
                callback(true);
                return;
            }
            if (IsRewardVideoApplovinAvailable())
            {
                isAdsLeaveApp = true;
                MaxSdk.ShowRewardedAd(curSetting.REWARD_ID_APPLOVIN);
                //LogShowReward
                //LogEventManager.Ins.OnAdShowLogEvent(location, "reward");
                return;
            }
            else
            {
                MaxSdk.LoadRewardedAd(curSetting.REWARD_ID_APPLOVIN);
            }
            callback(false);
        }
        /// <summary>
        /// Check if reward video ads is loaded
        /// </summary>
        /// <returns>Is ads loaded</returns>
        public bool IsRewardVideoAvailable()
        {
            if (IsRewardVideoApplovinAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsRewardVideoApplovinAvailable()
        {
#if USE_APPLOVIN_ADS
            return MaxSdk.IsRewardedAdReady(curSetting.REWARD_ID_APPLOVIN);
#endif
            return false;
        }

        private void RequestRewardBasedVideo()
        {
            if (!CanRequestVideo())
            {
                return;
            }
#if USE_APPLOVIN_ADS
            if (!curSetting.IsUseApplovin || MaxSdk.IsRewardedAdReady(curSetting.REWARD_ID_APPLOVIN))
            {
                return;
            }

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

            MaxSdk.LoadRewardedAd(curSetting.REWARD_ID_APPLOVIN);
#endif
        }

        
        IEnumerator CompleteMethodRewardedVideo(bool val)
        {
            yield return null;
            if (OnCompleteMethod != null)
            {
                OnCompleteMethod(val);
                OnCompleteMethod = null;
            }
        }

        private bool CanRequestVideo()
        {
            return numClickReward < curSetting.MaxClickVideo;
        }

        #region AppLovinRewardCallback

#if USE_APPLOVIN_ADS
        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (LogEventManager.Ins)
            {
                LogEventManager.Ins.OnApplovinAdsRevenuePaid(adInfo, Adformat.video_rewarded, curSetting.RewardedEventToken);
            }
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("ShowReward");
        }

        public void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MaxSdk.LoadRewardedAd(curSetting.REWARD_ID_APPLOVIN);
            if (triggerCompleteMethod)
            {
                StartCoroutine(CompleteMethodRewardedVideo(false));
            }
        }
        public void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            triggerCompleteMethod = false;
            MaxSdk.LoadRewardedAd(curSetting.REWARD_ID_APPLOVIN);
            StartCoroutine(CompleteMethodRewardedVideo(true));
            if (numRewardShowed < 3)
            {
                numRewardShowed++;
                PlayerPrefs.SetInt("NumRewardShowed", numRewardShowed);
                if (numRewardShowed == 3)
                {

                    LogEventManager.Ins.OnRewardedVideoNumReach(curSetting.RewardedVideoImpToken, numRewardShowed);
                }
            }
        }
        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            isAdsLeaveApp = true;
            numClickReward++;
            //LogEventManager.Ins.OnAdClickLogEvent("Video", "Admob", numClickReward);
        }

        public void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Debug.Log("Rewarded ad failed to load with error code: " + errorInfo.Code);
            if (currentRetryRewardedVideo < maxRetryCount)
            {
                currentRetryRewardedVideo++;
                MaxSdk.LoadRewardedAd(curSetting.REWARD_ID_APPLOVIN);
            }
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            Debug.Log("Rewarded ad failed to display with error code: " + errorInfo.Code);
            MaxSdk.LoadRewardedAd(curSetting.REWARD_ID_APPLOVIN);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            currentRetryRewardedVideo = 0;
        }
#endif

#endregion
#endregion

#region FullReward
        //#if USE_APPLOVIN_ADS
        //        private RewardedInterstitialAd rewardFull;
        //#endif
        //        private bool IsRewardFullLoaded = false;

        //        private UnityAction<bool> ShowFullOnCompleteMethod;

        //        private bool TriggerShowFullCompleteMethod;

        //        private void RequestFullReward()
        //        {
        //            if (!CanRequestFull())
        //            {
        //                return;
        //            }
        //#if USE_APPLOVIN_ADS
        //            if (curSetting.IsUseAdmob)
        //            {
        //                if (rewardFull != null || IsRewardFullLoaded)
        //                {
        //                    return;
        //                }

        //                if (rewardFull != null)
        //                {
        //#if UNITY_2020_3_OR_NEWER
        //                    rewardFull.Destroy();
        //#endif
        //                }

        //                AdRequest request = new AdRequest.Builder().Build();
        //                Debug.Log("FullRewardId " + curSetting.FULL_REWARD_ID_ADMOB);
        //                RewardedInterstitialAd.LoadAd(curSetting.FULL_REWARD_ID_ADMOB, request, adLoadCallback);
        //            }
        //#endif
        //        }
        //        /// <summary>
        //        /// Show full reward ads
        //        /// </summary>
        //        /// <param name="location">The location where the ads show</param>
        //        /// <param name="callback">When ads close</param>
        //        public void ShowFullReward(string location, UnityAction<bool> callback)
        //        {
        //            TriggerShowFullCompleteMethod = true;
        //            ShowFullOnCompleteMethod = callback;
        //#if UNITY_EDITOR
        //            if (callback != null)
        //            {
        //                callback(true);
        //            }

        //            return;
        //#endif
        //#if USE_APPLOVIN_ADS
        //            if (IsRewardFullLoaded && rewardFull != null)
        //            {
        //                isAdsLeaveApp = true;
        //                rewardFull.Show(HandlePaidEvent);
        //                return;
        //            }
        //#endif
        //            callback(false);
        //        }
        //#if USE_APPLOVIN_ADS

        //#if UNITY_2020_3_OR_NEWER
        //        private void adLoadCallback(RewardedInterstitialAd ad, AdFailedToLoadEventArgs error)
        //        {
        //            if (error == null)
        //            {
        //                IsRewardFullLoaded = true;
        //                rewardFull = ad;
        //                //rewardFull.OnPaidEvent += HandlePaidEvent;
        //                rewardFull.OnAdDidDismissFullScreenContent += HandleAdDidDismiss;
        //            }
        //            else
        //            {
        //                Debug.Log("Fail To Load Full Reward " + error.LoadAdError.GetMessage());
        //            }
        //        }
        //#else
        //        private void adLoadCallback(RewardedInterstitialAd ad, string error)
        //        {
        //            if (error == null)
        //            {
        //                IsRewardFullLoaded = true;
        //                rewardFull = ad;
        //                //rewardFull.OnPaidEvent += HandlePaidEvent;
        //                rewardFull.OnAdDidDismissFullScreenContent += HandleAdDidDismiss;
        //            }
        //            else
        //            {
        //                Debug.Log("Fail To Load Full Reward " + error);
        //            }
        //        }
        //#endif

        //        private void HandlePaidEvent(Reward reward)
        //        {
        //            Debug.Log("HandlePaid");
        //            TriggerShowFullCompleteMethod = false;
        //            StartCoroutine(CompleteMethodRewardedFull(true));
        //        }

        //        private void HandleAdDidDismiss(object sender, EventArgs args)
        //        {
        //            Debug.Log("Dismiss");
        //            if (TriggerShowFullCompleteMethod)
        //                StartCoroutine(CompleteMethodRewardedFull(false));
        //            IsRewardFullLoaded = false;
        //            rewardFull = null;
        //            RequestFullReward();
        //        }
        //#endif
        //        IEnumerator CompleteMethodRewardedFull(bool val)
        //        {
        //            Debug.Log("RunCompleteReward");
        //            yield return null;
        //            Debug.Log("AfterYield");
        //            if (ShowFullOnCompleteMethod != null)
        //            {
        //                Debug.Log("RunComplete");
        //                ShowFullOnCompleteMethod(val);
        //                ShowFullOnCompleteMethod = null;
        //            }
        //        }
#endregion

#region RefocusAds
        private DateTime lastTimeRefocusShow;

        private void OnApplicationPause(bool pause)
        {
            if (curSetting == null)
            {
                return;
            }
            if (!curSetting.IsRefocusShowAds)
            {
                return;
            }
            if ((DateTime.Now - lastTimeRefocusShow).TotalSeconds < curSetting.TimeBetweenRefocusShow)
            {
                return;
            }
            if (!pause)
            {
                if (isAdsLeaveApp)
                {
                    isAdsLeaveApp = false;
                }
                else
                {
                    if (curSetting.IsUseAppOpenAd)
                    {
                        ShowAppOpen("app_unpause");
                    }
                    else
                    {
                        ShowFull("app_unpause");
                    }
                    UpdateLastRefocusTime();
                }
            }
        }

        private void UpdateLastRefocusTime()
        {
            lastTimeRefocusShow = DateTime.Now;
            PlayerPrefs.SetString("LastTimeRefocusShow", lastTimeRefocusShow.ToString("O"));
        }
#endregion
        /// <summary>
        /// Get firebase remote config key
        /// </summary>
        /// <returns></returns>
        public string GetKey()
        {
#if UNITY_ANDROID
            return androidFirebaseKey;
#else
            return iosFirebaseKey;
#endif
        }
#region RemoveAds
        /// <summary>
        /// Check if can show Full or Banner
        /// </summary>
        /// <returns></returns>
        public bool CanShowAds()
        {
            return PlayerPrefs.GetInt(StaticClass.REMOVE_ADS, 0) == 0;
        }
        /// <summary>
        /// Remove ads
        /// </summary>
        public void RemoveAds()
        {
            PlayerPrefs.SetInt(StaticClass.REMOVE_ADS, 1);
            HideBanner();
        }
#endregion
    }

    [System.Serializable]
    public class AdsSetting
    {
        //AdmobSetting
        public bool IsUseApplovin;
        public string SDK_KEY_APPLOVIN = "";
        public string BANNER_ID_APPLOVIN = "";
        public string FULL_REWARD_ID_APPLOVIN = "";
        public string FULL_ID_APPLOVIN = "";
        public string REWARD_ID_APPLOVIN = "";
        //AdjustEventId
        public string BannerEventToken;
        public string InterstitialEventToken;
        public string RewardedEventToken;
        public string InterstitialImpToken;
        public string RewardedVideoImpToken;
        //AOASetitng
        public bool IsUseAppOpenAd;
        public string ID_TIER_1 = "";
        public string ID_TIER_2 = "";
        public string ID_TIER_3 = "";
        //BannerSetting
        public bool IsShowBanner;
        public bool IsBannerInStart;
        public BannerColor BannerColor;
        public int MaxClickBanner;
        //FullSetting
        public int TimeBetweenRefocusShow = 5;
        public int TimeBetweenShowFull = 20;
        public int MaxClickFull = 3;
        //RewardSetting
        public int MaxClickVideo = 3;
        public bool IsRefocusShowAds = true;
    }

    [System.Serializable]
    public enum BannerColor
    {
        Black,
        NoColor
    }
}
