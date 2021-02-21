using UnityEngine;
using Core.Gui.Canvases;

[DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    enum TargetPlatforms
    {
        All, Android, IOS,
    }

    RectTransform rectTransform;
#if UNITY_EDITOR //Debug
    [SerializeField]
    bool useEmulated;
#endif
    Rect lastSafeArea;

    [SerializeField]
    TargetPlatforms platforms = TargetPlatforms.All;

    [SerializeField]
    bool fitHorizontaly = true;

    [SerializeField]
    bool fitVerticaly = true;

    void Update()
    {        
        TryUpdateSafeArea();
    }

    bool CorrectPlatform()
    {
        if (Application.isEditor || platforms == TargetPlatforms.All)
            return true;
#if UNITY_IOS
        return platforms == TargetPlatforms.IOS;
#elif UNITY_ANDROID
        return platforms == TargetPlatforms.Android;
#endif

        return false;
    }

    /// <summary>
    /// https://docs.unity3d.com/ScriptReference/Screen-safeArea.html
    /// https://habr.com/ru/company/pixonic/blog/351184/
    /// </summary>
    private void TryUpdateSafeArea()
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        if (!CorrectPlatform())
            return;

        var screenSafeArea = ProvideSafeArea();

        if (lastSafeArea == screenSafeArea && rectTransform.rect == screenSafeArea)
            return;

        lastSafeArea = screenSafeArea;
        var anchorMin = screenSafeArea.position;
        var anchorMax = screenSafeArea.position + screenSafeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;

        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }

    Rect ProvideSafeArea()
    {
        var safeArea = Screen.safeArea;
#if UNITY_EDITOR
        if (useEmulated || StaticCanvas.Instance.EmulateIPhoneXSafeArea)
            safeArea = StaticCanvas.Instance.IPhoneXSafeAreaRect;
#endif
        if(!fitHorizontaly)
        {
            safeArea.x = 0f;
            safeArea.width = Screen.width;
        }

        if (!fitVerticaly)
        {
            safeArea.y = 0f;
            safeArea.height = Screen.height;
        }

        return safeArea;
    }
}