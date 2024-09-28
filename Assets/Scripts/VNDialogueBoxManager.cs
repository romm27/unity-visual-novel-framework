using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class VNDialogueBoxManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform mainTransform;
    [SerializeField] TextMeshProUGUI dialogueBox;
    [SerializeField] TextMeshProUGUI speakerName;
    [SerializeField] Transform hideOnNarratorDialogue;
    [SerializeField] GameObject nameDisplayOBJ;
    [SerializeField] GameObject toolboxOBJ;

    [Header("Data")]
    [SerializeField] DialogueBoxTransform[] dialogueBoxTransformPresets;

    [Header("Dialogue Controls")]
    public bool autoPlay = false;
    public bool skipSpaces = true;

    [Header("Events")]
    public UnityEvent OnLastCharacter = new UnityEvent();

    public string CurrentDialogueContent {
        get {
            return dialogueBox.text;
        }
    }

    [HideInInspector] public Dialogue currentDialogue;

    public static Color systemNarratorColor = Color.white;


    public void Start() {
        ApplyDialogueBoxPreset("default");
    }

    //Methods
    public void DisplayDialogue(Dialogue _dialogue, bool _instant = false) {
        currentDialogue = _dialogue;

        hideOnNarratorDialogue.gameObject.SetActive(_dialogue.speakerName != "");

        dialogueBox.color = _dialogue.messageColor;
        speakerName.color = _dialogue.messageColor;

        dialogueBox.text = _dialogue.content;
        speakerName.text = _dialogue.speakerName;

        if (!_instant) {
            dialogueBox.maxVisibleCharacters = 0;
            VNDirector.instance.playingDialogue = true;
            StartCoroutine(OnCharacterDisplayed());
        }
        else {
            dialogueBox.maxVisibleCharacters = _dialogue.content.Length;
        }
    }


    public IEnumerator OnCharacterDisplayed() {
        yield return new WaitForSeconds(VNDirector.instance.globalOptions.timeToNextCharacter);
        dialogueBox.maxVisibleCharacters++;

        //check for space
        if (skipSpaces && dialogueBox.text.Length > dialogueBox.maxVisibleCharacters) {
            if (dialogueBox.text[dialogueBox.maxVisibleCharacters] == ' ') {
                dialogueBox.maxVisibleCharacters++;
            }
        }

        //Next Character
        if(dialogueBox.maxVisibleCharacters < dialogueBox.text.Length) {
            StartCoroutine(OnCharacterDisplayed());
        }
        else {
            VNDirector.instance.playingDialogue = false;
            OnLastCharacterFunction();
        }
    } 
    
    public void ApplyDialogueBoxPreset(string _idententifier) {
        foreach(DialogueBoxTransform index in dialogueBoxTransformPresets) {
            if(index.identifier == _idententifier) {
                ApplyDialogueBoxPreset(index);
                return;
            }
        }
    }

    //Need to fix this, it does nothing right now
    public void ApplyDialogueBoxPreset(DialogueBoxTransform _preset) {
        mainTransform.rect.Set(_preset.rect.x, _preset.rect.y, _preset.rect.width, _preset.rect.height);
        toolboxOBJ.SetActive(!_preset.hideButtonBox);
        nameDisplayOBJ.SetActive(!_preset.hideName);
    }


    //Called Methods
    public void OnLastCharacterFunction() {
        //Base do not override
        OnLastCharacter?.Invoke();
    }

    public void CheckForAutoPlay() {
        if (autoPlay) {
            StartCoroutine(OnAutoPlay());
        }
    }

    public void ToggleAutoPlay() {
        if (autoPlay) {
            autoPlay = false;
        }
        else {
            autoPlay = true;
            if (!VNDirector.instance.playingDialogue) {
                VNDirector.instance.InputNextMessage();
            }
        }
    }


    public IEnumerator OnAutoPlay() {
        //float soundExtra = VNDirector.instance.voiceManager.CurrentClip != null ? VNDirector.instance.voiceManager.CurrentClip.length : 0f;
        float soundExtra = 0f;
        yield return new WaitForSeconds(VNDirector.instance.globalOptions.autoPlayTimeBetweenMessages + soundExtra);
        if (autoPlay) {
            VNDirector.instance.InputNextMessage();
        }
    }

}

[System.Serializable]
public struct Dialogue
{
    public string speakerName;
    public string content;
    public Color messageColor;
    public int lineId;
}

[System.Serializable]
public struct DialogueBoxTransform
{
    [Header("Data")]
    public bool hideName;
    public bool hideButtonBox;
    public string identifier;
    public Rect rect;
}
