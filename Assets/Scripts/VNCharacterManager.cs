using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VNCharacterManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform characterRoot;
    [SerializeField] VNCharacterDisplay activeDisplayPrefab;

    [Header("Scene")]
    public List<ActiveCharacterDisplay> activeCharacterDisplays = new List<ActiveCharacterDisplay>();

    [Header("Local Effects Settings")]
    public float poseTransitionDuration = 0.3f; //use 0f for disabled;



    //Methods
    public VNCharacter GetCharacter(string _characterIdentificator) {
        for (int i = 0; i < VNDirector.instance.ActiveArcDataPack.sceneCharacters.Length; i++) {
            if (VNDirector.instance.ActiveArcDataPack.sceneCharacters[i].characterIdentifier == _characterIdentificator) {
                return VNDirector.instance.ActiveArcDataPack.sceneCharacters[i];
            }
        }

        Debug.LogError("No Character with identificator: " + _characterIdentificator + " exists");
        return null;
    }

    public VNCharacterDisplay GetCharacterDisplay(string _characterIdentificator, bool _debug = true) {
        for (int i = 0; i < activeCharacterDisplays.Count; i++) {
            if (activeCharacterDisplays[i].characterIdentifier == _characterIdentificator) {
                return activeCharacterDisplays[i].display;
            }
        }
        if (_debug) {
            Debug.LogError("No active display for " + _characterIdentificator + " was found!");
        }

        return null;
    }

    public bool CharacterDisplayIsActive(string _characterIdentificator) {
        return GetCharacterDisplay(_characterIdentificator, false) != null;
    }

    public void SetCharacterDisplayX(string _characterIdentificator, float _value) {
        VNCharacterDisplay temp = GetCharacterDisplay(_characterIdentificator);
        temp.SetXPosition(_value);
    }

    public void SetCharacterDisplayY(string _characterIdentificator, float _value) {
        VNCharacterDisplay temp = GetCharacterDisplay(_characterIdentificator);
        temp.SetYPosition(_value);
    }

    public void SetCharacterDisplayScale(string _characterIdentificator, float _value) {
        VNCharacterDisplay temp = GetCharacterDisplay(_characterIdentificator);
        temp.SetScale(_value);
    }

    public void SetCharacterPose(string _characterIdentificator, string _poseName, bool _smooth = true) {
        bool newSprite = false;

        if (!CharacterDisplayIsActive(_characterIdentificator)) {
            activeCharacterDisplays.Add(new ActiveCharacterDisplay(_characterIdentificator, Instantiate(activeDisplayPrefab, characterRoot)));
            newSprite = true;
        }

        VNCharacterDisplay temp = GetCharacterDisplay(_characterIdentificator);
        temp.owner = _characterIdentificator;
        temp.pose = _poseName;

        if (newSprite || !_smooth) {
            temp.displayGFX.sprite = GetCharacter(_characterIdentificator).GetPose(_poseName);
        }
        else {
            temp.smoothSprite = GetCharacter(_characterIdentificator).GetPose(_poseName);
        }
    }
    

    public void DestroyCharacterPose(string _characterIdentificator) {
        if (CharacterDisplayIsActive(_characterIdentificator)) {
            ActiveCharacterDisplay temp = new ActiveCharacterDisplay();

            for (int i = 0; i < activeCharacterDisplays.Count; i++) {
                if (activeCharacterDisplays[i].characterIdentifier== _characterIdentificator) {
                    temp = activeCharacterDisplays[i];
                }
            }

            Destroy(temp.display.gameObject);
            activeCharacterDisplays.Remove(temp);
        }
    }

    public void SetCharacterName(string _identifier, string _name) {
        GetCharacter(_identifier).DisplayName = _name;
    }

    public void FlipCharacterX(string _identifier,string _flip, bool _smooth) {
        bool willFlip = _flip == "yes";
        FlipCharacterX(_identifier, willFlip, _smooth);
    }

    public void FlipCharacterX(string _identifier, bool _flip, bool _smooth) {
        GetCharacterDisplay(_identifier).FlipX(_flip, _smooth);
    }

    public void CancelAllLocalCharacterEffects() {
        for (int i = 0; i < activeCharacterDisplays.Count; i++) {
            activeCharacterDisplays[i].display.OnCancel();
        }
    }

    public void DestroyAllCharactersDisplays() {
        CancelAllLocalCharacterEffects();
        List<string> activeCharacters = new List<string>();
        for (int i = 0; i < activeCharacterDisplays.Count; i++) {
            activeCharacters.Add(activeCharacterDisplays[i].characterIdentifier);
        }

        for (int i = 0; i < activeCharacters.Count; i++) {
            DestroyCharacterPose(activeCharacters[i]);
        }
    }
}

[System.Serializable]
public struct ActiveCharacterDisplay
{
    public string characterIdentifier;
    public VNCharacterDisplay display;

    public ActiveCharacterDisplay(string _identificator, VNCharacterDisplay _display) {
        characterIdentifier = _identificator;
        display = _display;
    }
}
