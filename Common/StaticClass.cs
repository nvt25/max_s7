using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
public class StaticClass
{
    public static readonly string USE_APPLOVIN_ADS = "USE_APPLOVIN_ADS";

    public static readonly string USE_FIREBASE_ANA = "USE_FIREBASE_ANA";

    public static readonly string USE_AOA = "USE_AOA";

    public static readonly string USE_FIREBASE_REMOTE = "USE_FIREBASE_REMOTE";

    public static readonly string USE_FACEBOOK = "USE_FACEBOOK";

    public static readonly string USE_ADJUST = "USE_ADJUST";

    public static readonly string USE_IAP = "USE_IAP";

    public static readonly string REMOVE_ADS = "RemoveAds";

    public static readonly string RATE = "IsRate";

    public static readonly string ADS_SETTING = "AdsSetting";

    public static readonly string ALARM_SETTING = "AlarmSetting";

    public static readonly string MORE_GAME = "MoreGame";

    public static string MoreGameLink
    {
        get => PlayerPrefs.GetString(MORE_GAME, "");
        set => PlayerPrefs.SetString(MORE_GAME, value);
    }

    public static bool IsAndroid()
    {
#if UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }

    public static bool IsIOS()
    {
#if UNITY_IOS
        return true;
#else
        return false;
#endif
    }

    public static bool IsIpad()
    {
        float Aspect = Camera.main.aspect;
        return Aspect >= 2224f / 1668f && Aspect < 1136f / 640f;
    }

    public static void RateApp()
    {
#if UNITY_ANDROID
        RateAppExt.Instance.ShowInAppReview();
#else
        Device.RequestStoreReview();
        PlayerPrefs.SetInt(RATE, 1);
#endif
    }

    public void ShareApp()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            //AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            //AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + imagePath);
            //intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
            //intentObject.Call<AndroidJavaObject>("setType", "image/png");

            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),
                "https://play.google.com/store/apps/details?id=" + Application.identifier);
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "");
            currentActivity.Call("startActivity", jChooser);
        }
    }

    public static void MoreGame()
    {
        if (string.IsNullOrEmpty(MoreGameLink))
        {
            RateApp();
        }
        else
        {
            Application.OpenURL(MoreGameLink);
        }
    }
}

public static class MyAPIExtention
{
    public static string CheckCorrect(this string input)
    {
        if (input == null)
            return string.Empty;
        return input.Replace("\r", string.Empty);
    }
}
