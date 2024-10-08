using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

// VNScreenEffectManager does nothing in the current version, this is handled by the VNBackgroundManager.
public class VNScreenEffectManager : MonoBehaviour
{
    [Header("References")]
    public Canvas uiCanvas;


    [Header("Data")]
    List<VNScreenEffect> effectsInExecution = new List<VNScreenEffect>();

    [Header("Unity Events")]
    public UnityEvent onScreenEffectOver;


    //Methods
    public void OnScreenEffectOverMethod(VNScreenEffect _effect) {
        onScreenEffectOver?.Invoke();
        effectsInExecution.Remove(_effect);
    }
}


public class VNScreenEffect
{
    public bool ranStartCheck = false;

    protected EffectArgument[] effectArguments;

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

    //Base


    //Available For Modification
    public virtual void OnEffectStart() { }

    public virtual void OnEffectOver() {
        VNDirector.instance.screenEffectManager.OnScreenEffectOverMethod(this);    
    }
}



//============================================================Screen Effect Classes=========================================

//Character Slides from start to destionation in duration time
public class VNScreenEffectDipTo : VNScreenEffect
{
    //Uses arguments:
    //duration v1
    //dip_to

    float speed;
    string fadeTo;

    private float endTime;

    public override void InsertArguments(string[] _arguments) {
        base.InsertArguments(_arguments);
    }

    public override void OnEffectStart() {
        float fadeTo = GetEffectArgumentValues("fade_to").x;
        float duration = GetEffectArgumentValues("duration").x;

        speed = 1f / duration;
        endTime = Time.time + duration;
    }
    public override void OnEffectOver() {
        //if (target != null) {
        //    target.SetPosition(GetEffectArgumentValues("destination"));
       // }
        base.OnEffectOver();
    }
}
