using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class VNBackgroundManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image background;
    [SerializeField] Image backgroundFade;

    public static float fadeDuration = 1f;
    [HideInInspector] public string backgroundName = "";


    private float fadeRate = 0f;

    public void Update() {
        if (fadeRate != 0) {
            Color temp = backgroundFade.color;
            if (temp.a <= 0f) {
                fadeRate = 0;
                return;
            }
            else {
                temp.a -= fadeRate * Time.deltaTime;
                backgroundFade.color = temp;
            }
        }
    }


    //Methods
    public void SetBackground(string _backgroundName, bool _fadeTo = false) {
        backgroundName = _backgroundName;
        if (_fadeTo) {
            StartBackgroundFadeInto(backgroundName);
        }
        else {
            SnapBackground(_backgroundName);
        }
    }

    private void SnapBackground(string _backgroundName) {
        CancelFade();
        for (int i = 0; i < VNDirector.instance.ActiveArcDataPack.sceneBackgrounds.Length; i++) {
            if (VNDirector.instance.ActiveArcDataPack.sceneBackgrounds[i].name == _backgroundName) {
                background.sprite = VNDirector.instance.ActiveArcDataPack.sceneBackgrounds[i];
                return;
            }
        }
        Debug.LogError("No valid background called " + _backgroundName + " exists");
    }


    private void StartBackgroundFadeInto(string _fadeToBackground) {
        backgroundFade.sprite = background.sprite;
        backgroundFade.color = Color.white;

        fadeRate = 1f / fadeDuration;

        for (int i = 0; i < VNDirector.instance.ActiveArcDataPack.sceneBackgrounds.Length; i++) {
            if (VNDirector.instance.ActiveArcDataPack.sceneBackgrounds[i].name == _fadeToBackground) {
                background.sprite = VNDirector.instance.ActiveArcDataPack.sceneBackgrounds[i];
                return;
            }
        }
        Debug.LogError("No valid background called " + _fadeToBackground + " exists");
    }

    public void CancelFade() {
        fadeRate = 0;
        Color temp = new Color(1f, 1f, 1f, 0f);
        backgroundFade.color = temp;
    }
}
