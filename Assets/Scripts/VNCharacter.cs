using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "VN/Character")]
public class VNCharacter : ScriptableObject
{
    [Header("Identification")]
    public string characterIdentifier = "";

    [Header("GFX")]
    public string defaultCharacterName = "Character";
    public string overrideName = "";
    public Color characterColor = Color.white;
    public CharacterPose[] poses;

    public string DisplayName {
        get {
            if(overrideName == "") {
                return defaultCharacterName;
            }
            else {
                return overrideName;
            }
        }
        set {
            overrideName = value;
        }
    }

    //Methods
    public Sprite GetPose(string _poseName) {
        for (int i = 0; i < poses.Length; i++) {
            if (poses[i].poseName == _poseName) {
                return poses[i].pose;
            }
        }

        Debug.LogWarning("No Valid Pose with name " + _poseName + " was found for " + defaultCharacterName);
        return null;
    }

    public void OnValidate() {
        if(poses != null) {
            for (int i = 0; i < poses.Length; i++) {
                if (poses[i].poseName == "" && poses[i].pose != null) {
                    poses[i].poseName = poses[i].pose.name;
                }
            }
        }
    }

}

[System.Serializable]
public struct CharacterPose
{
    public string poseName;
    public Sprite pose;
}
