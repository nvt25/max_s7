using API.Ads;
#if USE_FIREBASE_REMOTE
using Firebase.Extensions;
using Firebase.RemoteConfig;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace API.RemoteConfig
{
    public class RemoteConfigManager : MonoBehaviour
    {
        public static RemoteConfigManager Ins;
        /// <summary>
        /// Call after firebase remote config fetch complete;
        /// </summary>
        public Action OnFetchComplete;
        public bool IsFetchComplete;
        public bool IsUseRemoteConfig;
        // Start is called before the first frame update
        Dictionary<string, object> defaults = new Dictionary<string, object>();
        public bool WaitSetDefaultData = false;
        private void Awake()
        {
            if (Ins == null)
            {
                Ins = this;
                DontDestroyOnLoad(transform.root.gameObject);
#if USE_FIREBASE_REMOTE
                Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        OnFirebaseInitCompleted();
                    }
                    else
                    {
                        Debug.LogError("FirebaseInitFalse");
                    }
                });
#endif
            }
            else if(Ins != this)
            {
                Destroy(transform.root.gameObject);
            }
        }
        void Start()
        {

        }

        private void OnFirebaseInitCompleted()
        {
            Debug.Log("FirebaseInitComplete");
#if USE_FIREBASE_MESS
        FirebaseMessaging.TokenReceived += OnTokenRecevied;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
#endif

            StartCoroutine(IEWaitSetDefaultData());
        }

        private IEnumerator IEWaitSetDefaultData()
        {
            yield return new WaitUntil(() => WaitSetDefaultData);
            yield return new WaitUntil(() => AdManager.Ins);
            InitDefault();
        }

        private void InitDefault()
        {
#if USE_FIREBASE_REMOTE
            string adsSetting = PlayerPrefs.GetString(StaticClass.ADS_SETTING, "O");
            defaults.Add(AdManager.Ins.GetKey(), adsSetting);
            defaults.Add(AdManager.Ins.GetKey() + "_more_game_link", StaticClass.MoreGameLink);
            defaults.Add(AdManager.Ins.GetKey() + "_notification_setting", "O");
            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
            {
                FetchData();
            });
#endif
        }

        private void FetchData()
        {
#if USE_FIREBASE_REMOTE
            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            fetchTask.ContinueWithOnMainThread(FetchComplete);
#endif
        }

#if USE_FIREBASE_REMOTE
        private void FetchComplete(Task fetchTask)
        {
            Debug.Log("Firebase Fetch Complete");
            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread((Task t) => SetData());
                    return;
            }
            SetData();
        }
#endif
        /// <summary>
        /// Set data after remote config fetch value complete
        /// </summary>
        private void SetData()
        {
#if USE_FIREBASE_REMOTE
            IsFetchComplete = true;
            OnFetchComplete?.Invoke();
            string data = FirebaseRemoteConfig.DefaultInstance.GetValue(AdManager.Ins.GetKey() + "_more_game_link").StringValue;
            data = data.CheckCorrect();
            Debug.Log("Key " + AdManager.Ins.GetKey() + "_more_game_link: " + data);
            if (!string.IsNullOrEmpty(data))
            {
                StaticClass.MoreGameLink = data;
            }
#endif
        }
        /// <summary>
        /// Set firebase remote config default value(only effect if call before start)
        /// </summary>
        /// <param name="key">Remote config key</param>
        /// <param name="defaultValue">Remote config default value</param>
        public void SetDefault(string key, object defaultValue)
        {
            defaults.Add(key, defaultValue);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(RemoteConfigManager))]
    public class RemoteConfigCustomEditor: Editor
    {
        private RemoteConfigManager configManager;
        private void OnEnable()
        {
            configManager = (RemoteConfigManager)target;
        }

        public override void OnInspectorGUI()
        {
            configManager.IsUseRemoteConfig = EditorGUILayout.Toggle("Use Remote Config", configManager.IsUseRemoteConfig);
            configManager.WaitSetDefaultData = EditorGUILayout.Toggle("Wait Set Default Data", configManager.WaitSetDefaultData);
            if (GUILayout.Button("Save"))
            {
                SetUpDefineSymbolsForGroup(StaticClass.USE_FIREBASE_REMOTE, configManager.IsUseRemoteConfig);
                AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(configManager);
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
