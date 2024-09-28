using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VNCharacterDisplay : MonoBehaviour
{
    [Header("References")]
    public Image displayGFX;
    [SerializeField] Image fadeInEffectGFX;
    public RectTransform rectTransform;
    [SerializeField] RectTransform fadeInPoseRect;

    [Header("Data")]
    public string owner = "";
    public string pose = "";

    [HideInInspector] public Vector2 defaultSize;

    //Defined
    public static float screenSizeCorrectionX = 400f;
    public static float screenSizeCorrectionY = 220f;

    //For Pose Change
    private float endTime;
    private float duration = 0;
    private float changeRate;

    public Sprite smoothSprite{
        get {
            return displayGFX.sprite;
        }
        set {
            FadeInto(value, VNDirector.instance.characterManager.poseTransitionDuration);
        }
    }

    public static Vector2 V2ScreenSizeCorrection {
        get {
            return new Vector2(screenSizeCorrectionX, screenSizeCorrectionY);
        }
    }

    public bool IsFlippedX {
        get {
            return rectTransform.localScale.x < 0;
        }
    }


    public void Update() {
        if(duration > 0) {
            if (Time.time <= endTime) {
                fadeInEffectGFX.color -= new Color(0f, 0f, 0f, 1f) * Time.deltaTime * changeRate;
            }
            else {
                fadeInEffectGFX.color = Color.white * 0;
                duration = 0;
                fadeInPoseRect.localScale = Vector3.one;
            }
        }
    }

    public Vector2 GetV2Position {
        get {
            Vector2 temp = rectTransform.localPosition;
            temp.x /= screenSizeCorrectionX;
            temp.y /= screenSizeCorrectionY;
            return temp;
        }
    }

    public Vector2 LocalPosition {
        get {
            return rectTransform.localPosition;
        }
        set {
            rectTransform.localPosition = value;
        }
    }
    //Methods
    public Vector2 ToV2Position(Vector2 _position) {
        Vector2 temp = _position;
        temp.x /= screenSizeCorrectionX;
        temp.y /= screenSizeCorrectionY;
        return temp;
    }

    public void SetXPosition(float _x) {

        Vector3 temp = rectTransform.localPosition;
        temp.x = _x * (screenSizeCorrectionX);
        rectTransform.localPosition = temp;
    }
    public void SetYPosition(float _y) {
        Vector3 temp = rectTransform.localPosition;
        temp.y = _y * (screenSizeCorrectionY);
        rectTransform.localPosition = temp;
    }
    public void SetPosition(Vector2 _pos) {
        SetXPosition(_pos.x);
        SetYPosition(_pos.y);
    }
    public void SetYPositionAbsolute(float _y) {
        Vector3 temp = rectTransform.localPosition;
        temp.y = _y;
        rectTransform.localPosition = temp;
    }

    public void SetScale(float _scale) {
        rectTransform.sizeDelta = new Vector2(defaultSize.x * _scale, defaultSize.y * _scale);
    }

    public void FadeInto(Sprite _fadeTo, float _duration) {
        fadeInEffectGFX.sprite = displayGFX.sprite;
        fadeInEffectGFX.color = new Color(1f, 1f, 1f, 1f);
        //UnityEngine.Debug.Log(fadeInEffectGFX.color);
        displayGFX.sprite = _fadeTo;
        endTime = Time.time + _duration;
        duration = _duration;
        changeRate = 1f / _duration;
        //Debug.Log(duration + " " + changeRate + " " + startedChangingAt);
    }

    //Runs when an unexpected deletion happens
    public void OnCancel() {
        
    }

    public void FlipX(bool _flip, bool _smooth) {
        float setValue = _flip ? -1 : 1;

        //Debug.Log(_smooth);

        if (_flip != IsFlippedX) {

            if (_smooth) {
                fadeInPoseRect.localScale = new Vector3(-1f, 1f, 1f);
                FadeInto(smoothSprite, VNDirector.instance.characterManager.poseTransitionDuration);
            }
            rectTransform.localScale = new Vector3(setValue, 1, 1);
        }
    }
    

}
