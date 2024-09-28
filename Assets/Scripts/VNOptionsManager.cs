using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VNOptionsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Slider mastersoundSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider ambientSlider;
    [SerializeField] Slider voiceSlider;
    [SerializeField] Slider dialogueSpeedSlider;
    [SerializeField] GameObject optionsTransform;
    [SerializeField] VNSettings globalSettings;


    public void Start() {
        GetSettings();
    }

    //Methods
    public void SwitchOptionsMenu() {
        optionsTransform.SetActive(!optionsTransform.activeInHierarchy);
    }

    public void UpdateSettings() {
        globalSettings.mainVolume = mastersoundSlider.value;
        globalSettings.musicVolume = musicSlider.value;
        globalSettings.voiceVolume = voiceSlider.value;
        globalSettings.ambientVolume = ambientSlider.value;
        globalSettings.timeToNextCharacter = dialogueSpeedSlider.maxValue - dialogueSpeedSlider.value;
        VNDirector.instance.audioManager.UpdateVolume();
    }

    public void GetSettings() {
        mastersoundSlider.value = globalSettings.mainVolume;
        musicSlider.value = globalSettings.musicVolume;
        voiceSlider.value = globalSettings.voiceVolume;
        ambientSlider.value = globalSettings.ambientVolume;
        dialogueSpeedSlider.value = dialogueSpeedSlider.maxValue - globalSettings.timeToNextCharacter;
    }
}
