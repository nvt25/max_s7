using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace API.Ads
{
    [CustomEditor(typeof(AdManager))]
    public class CustomerAdsManagerEditor : Editor
    {
        private AdManager adManager;
        AnimBool showExtraAndroid;
        AnimBool showExtraIOS;

        private void OnEnable()
        {
            adManager = (AdManager)target;
            showExtraAndroid = new AnimBool(false);
            showExtraAndroid.valueChanged.AddListener(Repaint);

            showExtraIOS = new AnimBool(false);
            showExtraIOS.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 150;
            #region ANDROID

            //Android
            EditorGUIUtility.labelWidth = 60;
            showExtraAndroid.target = EditorGUILayout.Toggle("Android", showExtraAndroid.target, CustomGUI.FoldOutHeaderGUIStyle);

            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            if (EditorGUILayout.BeginFadeGroup(showExtraAndroid.faded))
            {
                adManager.androidSetting.IsShowBanner = EditorGUILayout.Toggle("Show Banner", adManager.androidSetting.IsShowBanner);
                GUILayout.Space(5);
                if (adManager.androidSetting.IsShowBanner)
                {
                    adManager.androidSetting.BannerColor = (BannerColor)EditorGUILayout.EnumPopup("Banner Type", adManager.androidSetting.BannerColor);
                    GUILayout.Space(5);
                    adManager.androidSetting.IsBannerInStart = EditorGUILayout.Toggle("Start Show Banner", adManager.androidSetting.IsBannerInStart);
                    GUILayout.Space(5);
                }
                adManager.androidSetting.IsUseApplovin = EditorGUILayout.Toggle("Use Applovin Ads", adManager.androidSetting.IsUseApplovin);
                if (adManager.androidSetting.IsUseApplovin)
                {
                    adManager.androidSetting.SDK_KEY_APPLOVIN = EditorGUILayout.TextField("ID APP", adManager.androidSetting.SDK_KEY_APPLOVIN);
                    adManager.androidSetting.BANNER_ID_APPLOVIN = EditorGUILayout.TextField("ID Banner", adManager.androidSetting.BANNER_ID_APPLOVIN.Trim());
                    adManager.androidSetting.FULL_ID_APPLOVIN = EditorGUILayout.TextField("ID Show Full", adManager.androidSetting.FULL_ID_APPLOVIN.Trim());
                    adManager.androidSetting.FULL_REWARD_ID_APPLOVIN = EditorGUILayout.TextField("ID Show Full Reward", adManager.androidSetting.FULL_REWARD_ID_APPLOVIN.Trim());
                    adManager.androidSetting.REWARD_ID_APPLOVIN = EditorGUILayout.TextField("ID Video", adManager.androidSetting.REWARD_ID_APPLOVIN.Trim());
                }
                adManager.androidSetting.BannerEventToken = EditorGUILayout.TextField("Banner Event Token", adManager.androidSetting.BannerEventToken);
                adManager.androidSetting.InterstitialEventToken = EditorGUILayout.TextField("Interstitial Event Token", adManager.androidSetting.InterstitialEventToken);
                adManager.androidSetting.RewardedEventToken = EditorGUILayout.TextField("Rewarded Event Token", adManager.androidSetting.RewardedEventToken);
                adManager.androidSetting.InterstitialImpToken = EditorGUILayout.TextField("Interstitial Impression Event Token", adManager.androidSetting.InterstitialImpToken);
                adManager.androidSetting.RewardedVideoImpToken = EditorGUILayout.TextField("Rewarded Video Impression Event Token", adManager.androidSetting.RewardedVideoImpToken);
                GUILayout.Space(5); adManager.androidSetting.IsUseAppOpenAd = EditorGUILayout.Toggle("Use App Open Ads", adManager.androidSetting.IsUseAppOpenAd);
                if (adManager.androidSetting.IsUseAppOpenAd)
                {
                    adManager.androidSetting.ID_TIER_1 = EditorGUILayout.TextField("ID App Open Ads 1", adManager.androidSetting.ID_TIER_1.Trim());
                    adManager.androidSetting.ID_TIER_2 = EditorGUILayout.TextField("ID App Open Ads 2", adManager.androidSetting.ID_TIER_2.Trim());
                    adManager.androidSetting.ID_TIER_3 = EditorGUILayout.TextField("ID App Open Ads 3", adManager.androidSetting.ID_TIER_3.Trim());
                }
                GUILayout.Space(5);
                adManager.androidSetting.IsRefocusShowAds = EditorGUILayout.Toggle("Refocus Show Ads", adManager.androidSetting.IsRefocusShowAds);
                if (adManager.androidSetting.IsRefocusShowAds)
                {
                    GUILayout.Space(5);
                    adManager.androidSetting.TimeBetweenRefocusShow = EditorGUILayout.IntField("Time Refocus Show Full", adManager.androidSetting.TimeBetweenRefocusShow);
                }
                GUILayout.Space(5);
                adManager.androidSetting.TimeBetweenShowFull = EditorGUILayout.IntField("Time Interstitial", adManager.androidSetting.TimeBetweenShowFull);
                GUILayout.Space(5);
                adManager.androidSetting.MaxClickBanner = EditorGUILayout.IntField("Max Click Banner", adManager.androidSetting.MaxClickBanner);
                adManager.androidSetting.MaxClickFull = EditorGUILayout.IntField("Max Click Full", adManager.androidSetting.MaxClickFull);
                adManager.androidSetting.MaxClickVideo = EditorGUILayout.IntField("Max Click Video", adManager.androidSetting.MaxClickVideo);
                if (adManager.isUseFireBase)
                {
                    GUILayout.Space(20);
                    EditorGUILayout.BeginHorizontal();
                    adManager.androidFirebaseKey = GUILayout.TextField(adManager.androidFirebaseKey);
                    if (GUILayout.Button("Copy Android"))
                    {
                        EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(adManager.androidSetting);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndHorizontal();
            //end

            #endregion

            #region IOS

            //IOS
            EditorGUIUtility.labelWidth = 60;
            showExtraIOS.target = EditorGUILayout.Toggle("IOS", showExtraIOS.target, CustomGUI.FoldOutHeaderGUIStyle);

            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            if (EditorGUILayout.BeginFadeGroup(showExtraIOS.faded))
            {
                adManager.iosSetting.IsShowBanner = EditorGUILayout.Toggle("Show Banner", adManager.iosSetting.IsShowBanner);
                GUILayout.Space(5);
                if (adManager.iosSetting.IsShowBanner)
                {
                    adManager.iosSetting.BannerColor = (BannerColor)EditorGUILayout.EnumPopup("Banner Type", adManager.iosSetting.BannerColor);
                    GUILayout.Space(5);
                    adManager.iosSetting.IsBannerInStart = EditorGUILayout.Toggle("Start Show Banner", adManager.iosSetting.IsBannerInStart);
                    GUILayout.Space(5);
                }
                adManager.iosSetting.IsUseApplovin = EditorGUILayout.Toggle("Use Applovin Ads", adManager.iosSetting.IsUseApplovin);
                if (adManager.iosSetting.IsUseApplovin)
                {
                    adManager.iosSetting.SDK_KEY_APPLOVIN = EditorGUILayout.TextField("ID APP", adManager.iosSetting.SDK_KEY_APPLOVIN);
                    adManager.iosSetting.BANNER_ID_APPLOVIN = EditorGUILayout.TextField("ID Banner", adManager.iosSetting.BANNER_ID_APPLOVIN.Trim());
                    adManager.iosSetting.FULL_ID_APPLOVIN = EditorGUILayout.TextField("ID Show Full", adManager.iosSetting.FULL_ID_APPLOVIN.Trim());
                    adManager.iosSetting.FULL_REWARD_ID_APPLOVIN = EditorGUILayout.TextField("ID Show Full Reward", adManager.iosSetting.FULL_REWARD_ID_APPLOVIN.Trim());
                    adManager.iosSetting.REWARD_ID_APPLOVIN = EditorGUILayout.TextField("ID Video", adManager.iosSetting.REWARD_ID_APPLOVIN.Trim());
                }
                adManager.iosSetting.BannerEventToken = EditorGUILayout.TextField("Banner Event Token", adManager.iosSetting.BannerEventToken);
                adManager.iosSetting.InterstitialEventToken = EditorGUILayout.TextField("Interstitial Event Token", adManager.iosSetting.InterstitialEventToken);
                adManager.iosSetting.RewardedEventToken = EditorGUILayout.TextField("Rewarded Event Token", adManager.iosSetting.RewardedEventToken);
                adManager.iosSetting.InterstitialImpToken = EditorGUILayout.TextField("Interstitial Impression Event Token", adManager.iosSetting.InterstitialImpToken);
                adManager.iosSetting.RewardedVideoImpToken = EditorGUILayout.TextField("Rewarded Video Impression Event Token", adManager.iosSetting.RewardedVideoImpToken);
                GUILayout.Space(5);
                adManager.iosSetting.IsUseAppOpenAd = EditorGUILayout.Toggle("Use App Open Ads", adManager.iosSetting.IsUseAppOpenAd);
                if (adManager.iosSetting.IsUseAppOpenAd)
                {
                    adManager.iosSetting.ID_TIER_1 = EditorGUILayout.TextField("ID App Open Ads 1", adManager.iosSetting.ID_TIER_1.Trim());
                    adManager.iosSetting.ID_TIER_2 = EditorGUILayout.TextField("ID App Open Ads 2", adManager.iosSetting.ID_TIER_2.Trim());
                    adManager.iosSetting.ID_TIER_3 = EditorGUILayout.TextField("ID App Open Ads 3", adManager.iosSetting.ID_TIER_3.Trim());
                }
                GUILayout.Space(5);
                adManager.iosSetting.IsRefocusShowAds = EditorGUILayout.Toggle("Refocus Show Ads", adManager.iosSetting.IsRefocusShowAds);
                if (adManager.iosSetting.IsRefocusShowAds)
                {
                    GUILayout.Space(5);
                    adManager.iosSetting.TimeBetweenRefocusShow = EditorGUILayout.IntField("Time Refocus Show Full", adManager.iosSetting.TimeBetweenRefocusShow);
                }
                GUILayout.Space(5);
                adManager.iosSetting.TimeBetweenShowFull = EditorGUILayout.IntField("Time Interstitial", adManager.iosSetting.TimeBetweenShowFull);
                GUILayout.Space(5);
                adManager.iosSetting.MaxClickBanner = EditorGUILayout.IntField("Max Click Banner", adManager.iosSetting.MaxClickBanner);
                adManager.iosSetting.MaxClickFull = EditorGUILayout.IntField("Max Click Full", adManager.iosSetting.MaxClickFull);
                adManager.iosSetting.MaxClickVideo = EditorGUILayout.IntField("Max Click Video", adManager.iosSetting.MaxClickVideo);
                if (adManager.isUseFireBase)
                {
                    GUILayout.Space(20);
                    EditorGUILayout.BeginHorizontal();
                    adManager.iosFirebaseKey = GUILayout.TextField(adManager.iosFirebaseKey);
                    if (GUILayout.Button("Copy IOS"))
                    {
                        EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(adManager.iosSetting);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            adManager.isAutoLoadFull = EditorGUILayout.Toggle("Auto Load Interstitial", adManager.isAutoLoadFull);
            //end


            #endregion
            GUILayout.Space(10);
            adManager.isUseFireBase = EditorGUILayout.Toggle("Use Firebase Remote Config", adManager.isUseFireBase);

            GUILayout.Space(10);
            if (GUILayout.Button("Save"))
            {

#if UNITY_ANDROID
                SetUpDefineSymbolsForGroup(StaticClass.USE_APPLOVIN_ADS, adManager.androidSetting.IsUseApplovin);
                SetUpDefineSymbolsForGroup(StaticClass.USE_AOA, adManager.androidSetting.IsUseAppOpenAd);
#elif UNITY_IOS
                SetUpDefineSymbolsForGroup(StaticClass.USE_APPLOVIN_ADS, adManager.iosSetting.IsUseApplovin);
                SetUpDefineSymbolsForGroup(StaticClass.USE_AOA, adManager.iosSetting.IsUseAppOpenAd);
#endif

                AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(adManager);
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
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

        private AddRequest addRequest;

        private RemoveRequest removeRequest;

        private void AddPackage()
        {
            addRequest = Client.Add("com.unity.ads");
            EditorApplication.update += AddProgress;
        }

        private void AddProgress()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                    Debug.Log("Installed: " + addRequest.Result.packageId);
                else if (addRequest.Status >= StatusCode.Failure)
                    Debug.Log(addRequest.Error.message);

                EditorApplication.update -= AddProgress;
            }
        }

        private void RemovePackage()
        {
            removeRequest = Client.Remove("com.unity.ads");

            EditorApplication.update += RemoveProgress;
        }

        private void RemoveProgress()
        {
            if (removeRequest.IsCompleted)
            {
                if (removeRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Removed: " + removeRequest.PackageIdOrName);
                }
                else if (removeRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(removeRequest.Error.message);
                }
                EditorApplication.update -= RemoveProgress;
            }
        }
    }

    public class CustomGUI : EditorWindow
    {
        static GUIStyle m_foldout_header_gui_style;
        public static GUIStyle FoldOutHeaderGUIStyle
        {
            get
            {
                if (m_foldout_header_gui_style == null)
                {


                    m_foldout_header_gui_style = new GUIStyle(EditorStyles.foldout);
                    m_foldout_header_gui_style.fontSize = 14;

                    m_foldout_header_gui_style.padding.top = -2;
                    m_foldout_header_gui_style.padding.left = 16;
                }

                return m_foldout_header_gui_style;
            }
        }
    }
}
