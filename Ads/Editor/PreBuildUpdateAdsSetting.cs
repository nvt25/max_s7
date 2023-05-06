using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PreBuildUpdateAdsSetting : MonoBehaviour
{
    private const string AppLovinSettingsExportPath = "MaxSdk/Resources/AppLovinSettings.asset";
    private const string AdsSettingPath = "AdsSetting/AdSetting.asset";
    [MenuItem("Test/TestApplovinAds")]
    public static void UpdateApplovinSetting()
    {
        var settingsFileName = GetAppLovinSettingsAssetPath();
        Debug.LogError("Setting File Name " + settingsFileName + " " + File.Exists(settingsFileName));
        if (!File.Exists(settingsFileName))
        {
            Debug.LogError("Asset Not Found");
            return;
        }

        var obj = AssetDatabase.LoadAssetAtPath(settingsFileName, Type.GetType("AppLovinSettings, MaxSdk.Scripts.IntegrationManager.Editor"));
        AppLovinSettings appLovinSettings = (AppLovinSettings)obj;
        Debug.Log("Id" + appLovinSettings.AdMobAndroidAppId + " " + appLovinSettings.AdMobIosAppId);
        string settingFilePath = Path.Combine("Assets", AdsSettingPath);
        AdSetting adSetting = AssetDatabase.LoadAssetAtPath<AdSetting>(settingFilePath);
        appLovinSettings.AdMobAndroidAppId = adSetting.AndroidAdmobId;
        appLovinSettings.AdMobIosAppId = adSetting.IosAdmobId;
        EditorUtility.SetDirty(appLovinSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static string GetAppLovinSettingsAssetPath()
    {
        // Since the settings asset is generated during compile time, the asset label will contain platform specific path separator. So, use platform specific export path.  
        var assetLabel = "l:al_max_export_path-" + AppLovinSettingsExportPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var guids = AssetDatabase.FindAssets(assetLabel);
        var defaultPath = Path.Combine("Assets", AppLovinSettingsExportPath);
        if(guids.Length > 0)
        {
            Debug.LogError("If");
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }
        else
        {
            assetLabel = "l:al_max_export_path-" + AppLovinSettingsExportPath.Replace(Path.AltDirectorySeparatorChar, '\\');
            Debug.LogError("Else");
            guids = AssetDatabase.FindAssets(assetLabel);
            return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : defaultPath;
        }
    }
}
