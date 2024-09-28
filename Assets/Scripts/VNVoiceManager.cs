using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VNVoiceManager : MonoBehaviour
{
    [Header("Data")]
    public int currentLine = 0; 
    public Queue<QueuedDialogue> queuedDialogues = new Queue<QueuedDialogue>();
    public QueuedDialogue currentDialogue;

    [Header("Development")]
    public bool creatingAudioTrack = false;
    [SerializeField] private List<AudioClip> workspace = new List<AudioClip>(); //<- use to help with keeping track of voice lines
    [TextArea(20,20)][SerializeField] private string currentDialogueBox;

    [Header("Voice Splitter")]
    [SerializeField] bool developerVoiceSplitter = false;
    [SerializeField][TextArea(20, 20)] private string voiceLinesScript;

    public bool VoicesAvailable {
        get {
            return VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene().Length > 0;
        }
    }

    public AudioClip CurrentClip {
        get {
            if(VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene().Length == 0) {
                return null;
            }

            return VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene()[currentLine];
        }
    }

    //Methods

    public void LoadLinesForCurrentScene() {
        if (creatingAudioTrack) {
            workspace = new List<AudioClip>(VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene());
        }

        if (developerVoiceSplitter) {
            List<VNCommand> commands = new List<VNCommand>(VNDirector.instance.interpreter.PullScriptFromDataPack(0));
            int voiceId = 0;
            string speaker = "system_narrator";
            string finalDialogue = "";

            for (int i = 0; i < commands.Count; i++) {
                if (commands[i] is VNCommandDialogue || commands[i] is VNCommandSystemNarratorDialogue) {
                    if (commands[i] is VNCommandDialogue) {
                        VNCommandDialogue dialogue = commands[i] as VNCommandDialogue;
                        finalDialogue = dialogue.dialogue.content;
                        speaker = dialogue.dialogue.speakerName;
                    }
                    if (commands[i] is VNCommandSystemNarratorDialogue) {
                        VNCommandSystemNarratorDialogue narratorDialogue = commands[i] as VNCommandSystemNarratorDialogue;
                        finalDialogue = narratorDialogue.dialogue.content;
                        speaker = narratorDialogue.dialogue.speakerName;
                    }
                    voiceLinesScript += voiceId.ToString() + "- " + speaker + ": " + finalDialogue + '\n';
                    voiceId++;
                }
            }
        }
    }

    public void ResetLineCount() {
        currentLine = 0;
    }


    public void PlayCurrentLine() {
        if(VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene() == null) {
            return;
        }

        if (VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene().Length > 0 && currentLine < VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene().Length && currentLine <= VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene().Length) {
            if (VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene()[currentLine] != null) {
                VNDirector.instance.audioManager.PlayInChannel(VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene()[currentLine], 4);
            }
        }
        if (creatingAudioTrack) {
            currentDialogueBox = VNDirector.instance.dialogueBoxManager.CurrentDialogueContent;
            if(currentLine > VNDirector.instance.ActiveArcDataPack.GetVoicesForActiveScene().Length) {
                workspace.Add(null);
            }
        }
        currentLine++;
    }

    public void OnSkippedLine() {
        currentLine++;
    }
}


[System.Serializable]
public struct QueuedDialogue
{
    public int dialogueLine;
    public AudioClip clip;
}
