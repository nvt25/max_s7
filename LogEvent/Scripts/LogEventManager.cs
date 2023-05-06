#if USE_FACEBOOK
using Facebook.Unity;
#endif
#if USE_FIREBASE_ANA
using Firebase.Analytics;
using GoogleMobileAds.Api;
#endif
#if USE_ADJUST
using com.adjust.sdk;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace API.LogEvent
{
    public class LogEventManager : MonoBehaviour
    {
        public static LogEventManager Ins;

        public bool IsUseFirebaseLogEvent;

        public bool IsUseAdjust;

        public bool IsUseFacebook;

        public bool IsUseDebug;

        private void Awake()
        {
            if (Ins == null)
            {
                Ins = this;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                if (Ins != this)
                {
                    Destroy(transform.root.gameObject);
                }
            }
            Debug.unityLogger.logEnabled = IsUseDebug;
        }

        private void OnEnable()
        {
#if USE_FACEBOOK
            if (!FB.IsInitialized)
            {
                FB.Init();
            }
#endif
        }
        /// <summary>
        /// Log event when IAP purchase complete
        /// </summary>
        /// <param name="Name">IAP pack name</param>
        /// <param name="Id">IAP pack id</param>
        /// <param name="price">IAP pack price</param>
        /// <param name="Currency">IAP pack price currency</param>
//        public void OnInAppPurchaseCompleteLogEvent(string Name, string Id, double price, string Currency)
//        {
//#if USE_ADJUST

//#endif
//            LogFirebaseEvent("iap_pack_" + Id + "_purchase_complete");
//        }
        /// <summary>
        /// Log event when ads show
        /// </summary>
        /// <param name="Location">Location where ads show</param>
        /// <param name="AdsType">The ads type show (Banner, Full, Reward)</param>
        /// <param name="AdNetworkName">The ads network show (Admob, Unity)</param>
//        public void OnAdShowLogEvent(string Location, string AdsType)
//        {
//#if USE_ADJUST

//#endif
//            LogFirebaseEvent("show_applovin_" + AdsType + "_ads_at_" + Location);
//        }
        /// <summary>
        /// Log event when ads click
        /// </summary>
        /// <param name="AdsType">The ads type show (Banner, Full, Reward)</param>
        /// <param name="AdNetworkName">The ads network show (Admob, Unity)</param>
        /// <param name="numClick">Num ads type click</param>
//        public void OnAdClickLogEvent(string AdsType, string AdNetworkName, int numClick)
//        {
//#if USE_ADJUST

//#endif
//            LogFirebaseEvent("click_" + AdNetworkName + "_" + AdsType + "_ads_click_time_" + numClick);
//        }
        /// <summary>
        /// Log event when level complete
        /// </summary>
        /// <param name="levelName">Level complete name</param>
        /// <param name="playDuration">Time play level</param>
        public void OnLevelCompleteLogEvent(string levelName, double playDuration)
        {
#if USE_ADJUST

#endif
#if USE_FIREBASE_ANA
            FirebaseAnalytics.LogEvent("level_" + levelName + "_complete", "play_time", playDuration);
#endif
        }
        /// <summary>
        /// Log event when level start
        /// </summary>
        /// <param name="levelName">Level start name</param>
        public void OnLevelStartLogEvent(string levelName)
        {
#if USE_ADJUST

#endif
            LogFirebaseEvent("level_" + levelName + "_start");
        }
        /// <summary>
        /// Log event when use item
        /// </summary>
        /// <param name="ItemName">Use item name</param>
        /// <param name="levelName">Level use item name</param>
        public void OnUseItemLog(string ItemName, string levelName)
        {
            LogFirebaseEvent("use_" + ItemName + "_level_" + levelName);
        }

        public void OnApplovinAdsRevenuePaid(MaxSdkBase.AdInfo adInfo, Adformat adFormat, string adjustEventId)
        {
#if USE_FIREBASE_ANA
            Parameter[] AdParameters = {
                new Firebase.Analytics.Parameter("ad_platform", "applovin"),
                new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
                new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
                new Firebase.Analytics.Parameter("currency","USD"),
                new Firebase.Analytics.Parameter("value", adInfo.Revenue),
                new Firebase.Analytics.Parameter("placement", adInfo.Placement),
                new Firebase.Analytics.Parameter("country_code", MaxSdk.GetSdkConfiguration().CountryCode),
                new Firebase.Analytics.Parameter("ad_format", adFormat.ToString())
            };
            FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
#endif
#if USE_ADJUST
            AdjustEvent adjustEvent = new AdjustEvent(adjustEventId);
            adjustEvent.addCallbackParameter("ad_source", adInfo.NetworkName);
            adjustEvent.setRevenue(adInfo.Revenue, "USD");
            Adjust.trackEvent(adjustEvent);
#endif
        }

        /// <summary>
        /// Log custom firebase event
        /// </summary>
        /// <param name="EventName">Event name</param>
        public void LogFirebaseEvent(string EventName, Dictionary<string, string> parameters = null)
        {
#if USE_FIREBASE_ANA
            if (parameters != null)
            {
                List<Parameter> TemParam = new List<Parameter>();
                foreach (KeyValuePair<string, string> param in parameters)
                {
                    TemParam.Add(new Parameter(param.Key, param.Value));
                }
                FirebaseAnalytics.LogEvent(EventName, TemParam.ToArray());
            }
            else
            {
                FirebaseAnalytics.LogEvent(EventName);
            }
#endif
        }
        /// <summary>
        /// Log event when user pass numReach Interstitial impression
        /// </summary>
        /// <param name="adjustEventId">Adjust event id</param>
        /// <param name="numReach">Num impression reached</param>
        public void OnInterstitialNumReach(string adjustEventId, int numReach)
        {
#if USE_FIREBASE_ANA
            FirebaseAnalytics.LogEvent("impdau_inter_passed", new Parameter("ImpdauPassed", numReach));
#endif
#if USE_ADJUST
            AdjustEvent adjustEvent = new AdjustEvent(adjustEventId);
            adjustEvent.addCallbackParameter("ImpdauPassed", numReach.ToString());
            Adjust.trackEvent(adjustEvent);
#endif
        }
        /// <summary>
        /// Log event when app try to show Interstitial ads
        /// </summary>
        /// <param name="HasAds"></param>
        /// <param name="placement"></param>
        public void OnCallShowInterstitialAds(bool HasAds, string placement)
        {
#if USE_FIREBASE_ANA
            FirebaseAnalytics.LogEvent("show_interstitial_ads", new Parameter[] { new Parameter("has_ads", HasAds.ToString()), new Parameter("placement", placement) });
#endif
        }
        /// <summary>
        /// Log event when user pass numReach Rewarded Video impression
        /// </summary>
        /// <param name="adjustEventId">Adjust event id</param>
        /// <param name="numReach">Num impression reached</param>
        public void OnRewardedVideoNumReach(string adjustEventId, int numReach)
        {
#if USE_FIREBASE_ANA
            FirebaseAnalytics.LogEvent("impdau_reward_passed", new Parameter("ImpdauPassed", numReach));
#endif
#if USE_ADJUST
            AdjustEvent adjustEvent = new AdjustEvent(adjustEventId);
            adjustEvent.addCallbackParameter("ImpdauPassed", numReach.ToString());
            Adjust.trackEvent(adjustEvent);
#endif
        }
        /// <summary>
        /// Log event when app try to show Rewarded Video ads
        /// </summary>
        /// <param name="HasAds"></param>
        /// <param name="placement"></param>
        public void OnCallShowRewardedVideoAds(bool HasAds, string placement)
        {
#if USE_FIREBASE_ANA
            FirebaseAnalytics.LogEvent("show_rewarded_ads", new Parameter[] { new Parameter("has_ads", HasAds.ToString()), new Parameter("placement", placement) });
#endif
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LogEventManager))]
    public class RemoteConfigCustomEditor : Editor
    {
        private LogEventManager logManager;
        private void OnEnable()
        {
            logManager = (LogEventManager)target;
        }

        public override void OnInspectorGUI()
        {
            logManager.IsUseFirebaseLogEvent = EditorGUILayout.Toggle("Use Firebase Analytics", logManager.IsUseFirebaseLogEvent);
            GUILayout.Space(5);
            logManager.IsUseAdjust = EditorGUILayout.Toggle("Use Adjust Analytics", logManager.IsUseAdjust);
            GUILayout.Space(5);
            logManager.IsUseFacebook = EditorGUILayout.Toggle("Use Facebook", logManager.IsUseFacebook);
            GUILayout.Space(5);
            logManager.IsUseDebug = EditorGUILayout.Toggle("Use Debug", logManager.IsUseDebug);
            if (GUILayout.Button("Save"))
            {
                SetUpDefineSymbolsForGroup(StaticClass.USE_FIREBASE_ANA, logManager.IsUseFirebaseLogEvent);
                SetUpDefineSymbolsForGroup(StaticClass.USE_ADJUST, logManager.IsUseAdjust);
                SetUpDefineSymbolsForGroup(StaticClass.USE_FACEBOOK, logManager.IsUseFacebook);
                AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(logManager);
            }
        }
        private void SetUpDefineSymbolsForGroup(string key, bool enable)
        {
            //Debug.Log(enable);
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            // Only if not defined already.
            if (defines.Contains(key))
            {
                if (enable)
                {
                    Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + key + "</b> <i>Scripting Define Symbol</i>.");
                    return;
                }
                else
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines.Replace(key, "")));

                    return;
                }
            }
            else
            {
                // Append
                if (enable)
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + key));
            }
        }
    }
#endif
}

public enum Adformat
{
    banner,
    interstitial,
    video_rewarded
}