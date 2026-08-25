using System.Collections;
using UnityEngine;

public class SleepSequenceController : MonoBehaviour
{
    [SerializeField] private CanvasGroup blackOutUI;
    [SerializeField] private float fadeDuration;
    
    private bool isSleeping;
    private GameObject blackOutUIGameObject;

    private void Start()
    {
        blackOutUIGameObject = blackOutUI.gameObject;
        LeanTween.reset();
    }

    public void HandleSleep(Transform targetTransform)
    {
        if (isSleeping) return;
        isSleeping = true;
        blackOutUIGameObject.SetActive(true);
        EventBus<ChangeActionMap>.Raise(new ChangeActionMap(){MapType = ActionMapType.Static});
        StartFadeOut(targetTransform);
    }

    private void StartFadeOut(Transform targetTransform)
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
                
                EventBus<OnSleepEvent>.Raise(new OnSleepEvent());
                EventBus<OnRequestTeleportEvent>.Raise(new OnRequestTeleportEvent(){AwakeTransform = targetTransform});
                // StartCoroutine(WaitCoroutine());
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

    private IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        StartFadeIn();
    }
}

