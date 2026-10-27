using System;
using System.Collections;
using UnityEngine;

public class SleepSequenceController : MonoBehaviour
{
    [SerializeField] private CanvasGroup blackOutUI;
    [SerializeField] private Transform awakeTransform;
    [SerializeField] private float fadeDuration;
    
    private bool isSleeping;
    private GameObject blackOutUIGameObject;

    private void OnEnable()
    {
        EventBus<OnPlayerPassedOutEvent>.Subscribe(HandlePassOut);
    }

    private void OnDisable()
    {
        EventBus<OnPlayerPassedOutEvent>.Unsubscribe(HandlePassOut);
    }

    private void Start()
    {
        blackOutUIGameObject = blackOutUI.gameObject;
        LeanTween.reset();
    }

    private void HandlePassOut(OnPlayerPassedOutEvent evt)
    {
        if (isSleeping) return;
        isSleeping = true;
        blackOutUIGameObject.SetActive(true);
        EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Static});
        StartFadeOut(awakeTransform, false, 0.1f);
        
    }

    public void HandleSleep()
    {
        if (isSleeping) return;
        isSleeping = true;
        blackOutUIGameObject.SetActive(true);
        EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Static});
        StartFadeOut(awakeTransform, true, fadeDuration / 2);
    }

    private void StartFadeOut(Transform targetTransform, bool raiseSleepEvent, float fadeDuration)
    {
        LeanTween.cancel(blackOutUIGameObject);
        LeanTween.value(blackOutUIGameObject, 0, 1, fadeDuration  / 2)
            .setOnUpdate((float value) =>
            {
                blackOutUI.alpha = value;
            })
            .setOnComplete(() =>
            {
                blackOutUI.alpha = 1;

                if (raiseSleepEvent)
                {
                    EventBus<OnSleepEvent>.Raise(new OnSleepEvent());
                }
                
                EventBus<OnRequestTeleportEvent>.Raise(new OnRequestTeleportEvent(){AwakeTransform = targetTransform});
                StartFadeIn();
            });
    }

    private void StartFadeIn()
    {
        LeanTween.cancel(blackOutUIGameObject);
        LeanTween.value(blackOutUIGameObject,1, 0, fadeDuration / 2)
            .setEase(LeanTweenType.easeInOutExpo)
            .setOnUpdate((float value) =>
            {
                blackOutUI.alpha = value;
            })
            .setOnComplete(() =>
            {
                blackOutUI.alpha = 0;
                
                EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Player});
                blackOutUIGameObject.SetActive(false);
                isSleeping = false;
            });
    }
}

