using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Arc", menuName = "VN/Arc")]
public class VNChapterDataPack : ScriptableObject
{
    [Header("Info Display")]
    public string chapterName = "default";

    [Header("Data Packs")]
    public TextAsset[] sceneScripts;
    public VNVoiceLinesHolder[] sceneVoices;
    public VNCharacter[] sceneCharacters;
    public Sprite[] sceneBackgrounds;
    public AudioClip[] sceneAudioLoops;
    public AudioClip[] sceneAudioEffects;

    //Methods
    public AudioClip[] GetVoicesForActiveScene() {
        if(sceneVoices == null) {
            return null;
        }

        VNVoiceLinesHolder temp = sceneVoices[VNDirector.currentSceneId];
        if(temp != null) {
            return temp.audioClips;
        }
        else {
            return new AudioClip[0];
        }
    }
}
