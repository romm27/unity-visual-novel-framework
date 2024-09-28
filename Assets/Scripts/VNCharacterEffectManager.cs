using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

public class VNCharacterEffectManager : MonoBehaviour
{
    [Header("References")]
    public Canvas uiCanvas;

    [Header("Unity Events")]
    public UnityEvent OnCharacterEffectOver = new UnityEvent();

    [Header("Settings")]
    public float scaleX = 800f;
    public float scaleY = 600f;

    private List<VNCharacterEffect> currentEffectsInExecution = new List<VNCharacterEffect>();

    public void Update() {
        if(currentEffectsInExecution.Count > 0) {
            for (int i = 0; i < currentEffectsInExecution.Count; i++) {
                currentEffectsInExecution[i].WhileExecuting();
            }
        }
    }


    //Methods
    public void StartEffect(VNCharacterEffect _effect, VNCharacterDisplay _target) {
        currentEffectsInExecution.Add(_effect);
        _effect.SetTarget(_target);
    }

    public void OnCharacterEffectOverMethod(VNCharacterEffect _effect) {
        OnCharacterEffectOver?.Invoke();
        currentEffectsInExecution.Remove(_effect);
    }

    public void OnPlayerInput() {
        if (currentEffectsInExecution.Count > 0) {
            for (int i = 0; i < currentEffectsInExecution.Count; i++) {
                if (currentEffectsInExecution[i].ranStartCheck) {
                    currentEffectsInExecution[i].OnEffectOver();
                }
            }
        }
    }

    //IMPORTED CODE
    public static Rect RectTransformToScreenSpace(RectTransform transform) {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }
    //
}

public class VNCharacterEffect
{
    public bool ranStartCheck = false;

    protected EffectArgument[] effectArguments;
    protected VNCharacterDisplay target;

    public virtual void InsertArguments(string[] _arguments) {
        effectArguments = EffectArgumentUtilities.FromStringToArgument(_arguments);
    }

    public Vector2 GetEffectArgumentValues(string _argument) {
        for (int i = 0; i < effectArguments.Length; i++) {
            if (effectArguments[i].argument == _argument) {
                return new Vector2(effectArguments[i].value1, effectArguments[i].value2);
            }
        }
        Debug.LogError("No valid argument called " + _argument + " was found!");
        return Vector2.zero;
    }

    public bool EffectArgumentExists(string _argument) {
        for (int i = 0; i < effectArguments.Length; i++) {
            if (effectArguments[i].argument == _argument) {
                return true;
            }
        }

        return false;
    }

    //Internal
    public void WhileExecuting() {
        //Start
        if (!ranStartCheck) {
            ranStartCheck = true;
            OnEffectStart();
        }

        //Update
        if (target != null) {
            OnEffectUpdate();
        }
        else {
            //Unload this instance
            VNDirector.instance.characterEffectManager.OnCharacterEffectOverMethod(this);
        }
    }

    //Base

    public void SetTarget(VNCharacterDisplay _target) {
        target = _target;
    }


    //Available For Modification
    public virtual void OnEffectStart() { }

    public virtual void OnEffectUpdate() { }

    public virtual void OnEffectOver() {
        VNDirector.instance.characterEffectManager.OnCharacterEffectOverMethod(this);
    }
}






//============================================================Character Effect Classes=========================================

//Character Slides from start to destionation in duration time
public class VNCharacterEffectSlideTo : VNCharacterEffect
{
    //Uses arguments:
    //start v2 (optional)
    //destination v2
    //duration v1

    float speed;
    Vector2 dir;

    private float endTime;

    public override void InsertArguments(string[] _arguments) {
        base.InsertArguments(_arguments);
    }

    public override void OnEffectStart() {
        Vector2 start = EffectArgumentExists("start")? GetEffectArgumentValues("start") * VNCharacterDisplay.V2ScreenSizeCorrection : target.LocalPosition;
        Vector2 destination = GetEffectArgumentValues("destination") * VNCharacterDisplay.V2ScreenSizeCorrection;
        float duration = GetEffectArgumentValues("duration").x;

        speed = ((Vector2.Distance(start, destination)) / (duration));
        dir = (destination - start).normalized;
        target.LocalPosition = start;
        endTime = Time.time + duration;
    }

    public override void OnEffectUpdate() {

        if(Time.time >= endTime) {
            OnEffectOver();
        }
        else {
            target.LocalPosition += dir * Time.deltaTime * speed;
        }
    }
    public override void OnEffectOver() {
        if (target != null) {
            target.SetPosition(GetEffectArgumentValues("destination"));
        }
        base.OnEffectOver();
    }
}


//Character Fades in for the duration specified
public class VNCharacterEffectFadeIn : VNCharacterEffect
{
    //Uses arguments:
    //start v2 (optional)
    //destination v2
    //duration v1

    private float rate;

    public override void InsertArguments(string[] _arguments) {
        base.InsertArguments(_arguments);
    }

    public override void OnEffectStart() {
        float duration = EffectArgumentExists("duration") ? GetEffectArgumentValues("duration").x : 1f;
        rate = 1f / duration;

        target.displayGFX.color = new Color(0, 0, 0, 0);
    }

    public override void OnEffectUpdate() {
        if (target.displayGFX.color.a >= 1f) {
            OnEffectOver();
        }
        else {
            target.displayGFX.color += new Color(1f, 1f, 1f, 1f) * rate * Time.deltaTime;
        }
    }
    public override void OnEffectOver() {
        target.displayGFX.color = new Color(1f, 1f, 1f, 1f);
        base.OnEffectOver();
    }
}


//Character Fades out for the duration specified
public class VNCharacterEffectFadeOut : VNCharacterEffect
{
    //Uses arguments:
    //start v2 (optional)
    //destination v2
    //duration v1

    private float rate;

    public override void InsertArguments(string[] _arguments) {
        base.InsertArguments(_arguments);
    }

    public override void OnEffectStart() {
        float duration = EffectArgumentExists("duration") ? GetEffectArgumentValues("duration").x : 1f;
        rate = 1f / duration;

        target.displayGFX.color = new Color(1f, 1f, 1f, 1f);
    }

    public override void OnEffectUpdate() {
        if (target.displayGFX.color.a <= 0) {
            OnEffectOver();
        }
        else {
            target.displayGFX.color -= new Color(1f, 1f, 1f, 1f) * rate * Time.deltaTime;
        }
    }
    public override void OnEffectOver() {
        VNDirector.instance.characterManager.DestroyCharacterPose(target.owner);
        base.OnEffectOver();
    }
}

//Character Fades into a new pose
public class VNCharacterEffectPodeIn : VNCharacterEffect
{
    //Uses arguments:
    //start v2 (optional)
    //destination v2
    //duration v1

    private float rate;

    public override void InsertArguments(string[] _arguments) {
        base.InsertArguments(_arguments);
    }

    public override void OnEffectStart() {
        float duration = EffectArgumentExists("duration") ? GetEffectArgumentValues("duration").x : 1f;
        rate = 1f / duration;

        target.displayGFX.color = new Color(1f, 1f, 1f, 1f);
    }

    public override void OnEffectUpdate() {
        if (target.displayGFX.color.a <= 0) {
            OnEffectOver();
        }
        else {
            target.displayGFX.color -= new Color(1f, 1f, 1f, 1f) * rate * Time.deltaTime;
        }
    }
    public override void OnEffectOver() {
        VNDirector.instance.characterManager.DestroyCharacterPose(target.owner);
        base.OnEffectOver();
    }
}



//Arguments
public struct EffectArgument
{
    public string argument;
    public float value1, value2;

    public EffectArgument(string argument, float value1, float value2) {
        this.argument = argument;
        this.value1 = value1;
        this.value2 = value2;
    }
}
public static class EffectArgumentUtilities
{
    //Methods
    public static EffectArgument[] FromStringToArgument(string[] _arguments) {
        EffectArgument[] effectArguments = new EffectArgument[_arguments.Length];

        for (int i = 0; i < _arguments.Length; i++) {
            //Debug.Log(_arguments[i]);
            EffectArgument temp = new EffectArgument();
            temp.argument = _arguments[i].Split('=')[0].Trim();

            if (_arguments[i].Contains('(')) { //double value
                string line = _arguments[i].Split('=')[1].Replace("(", "").Replace(")", "");

                temp.value1 = float.Parse(line.Split(',')[0].Trim(), CultureInfo.InvariantCulture);
                temp.value2 = float.Parse(line.Split(',')[1].Trim(), CultureInfo.InvariantCulture);
            }
            else { //single value
                temp.value1 = float.Parse(_arguments[i].Split('=')[1].Trim(), CultureInfo.InvariantCulture);
            }

            effectArguments[i] = temp;
        }

        return effectArguments;
    }
}