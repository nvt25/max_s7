using Google.Play.Review;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RateAppExt : MonoBehaviour
{
    ReviewManager reviewManager = new ReviewManager();

    private static RateAppExt instance = null;

    bool isShowingIAR = false;

    PlayReviewInfo _playReviewInfo;

    bool isReviewAvailable = false;

    public static RateAppExt Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gObject = new GameObject(string.Format("(Singleton) {0}", typeof(RateAppExt).Name));
                instance = gObject.AddComponent<RateAppExt>();
                DontDestroyOnLoad(gObject);
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod]
    public static void FirstLoadReview()
    {
        Instance.RequestReviewFlow();
    }

    public void ShowInAppReview()
    {
        if (isShowingIAR) return;
        if (!isReviewAvailable)
        {
            RequestReviewFlow();
        }
        StartCoroutine(IEShowReviewFlow());
    }

    public void RequestReviewFlow()
    {
        StartCoroutine(IERequestReviewFlow());
    }

    IEnumerator IERequestReviewFlow()
    {
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error == ReviewErrorCode.NoError)
        {
            _playReviewInfo = requestFlowOperation.GetResult();
            isReviewAvailable = true;
            Debug.Log("Request successfull");
        }
        else
        {
            isReviewAvailable = false;
            Debug.Log($"Request review fail{requestFlowOperation.Error}");
        }
    }
    IEnumerator IEShowReviewFlow()
    {
        yield return new WaitUntil(() => isReviewAvailable);
        isShowingIAR = true;
        var launchFlowOperation = reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null;
        isReviewAvailable = false;
        RequestReviewFlow();
        if (launchFlowOperation.Error == ReviewErrorCode.NoError)
        {
            Debug.Log("Show review success full");
            isShowingIAR = false;
            PlayerPrefs.SetInt(StaticClass.RATE, 1);
        }
        else
        {
            Debug.Log($"Fail to show review: {launchFlowOperation.Error}");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ShowInAppReview();
        }
    }
}
