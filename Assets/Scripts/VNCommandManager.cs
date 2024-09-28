using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

//WARNING NOT ALL COMMAND LINES ARE YET IMPLEMENTED!
public class VNCommandManager : MonoBehaviour
{
    [Header("Queued Commands")]
    public Queue<VNCommand> queuedCommands = new Queue<VNCommand>();

    //Methods
    public VNCommand GetNextCommand() {
        if(queuedCommands.Count == 0) { return null; }
        return queuedCommands.Dequeue();
    }
}
[System.Serializable]
public class VNCommand
{

    public int lineNumber;
    public virtual bool SkipInput {
        get {
            return true;
        }
        set {
            Debug.LogError("Can't do that!");
        }
    }
    //Always Override These
    public virtual void Generate() {}

    public virtual void Execute() {
        Debug.LogWarning("Null Command");
    }

    public virtual void ExecuteOnSkip() { }

    public virtual string ToCommandLine() {
        Debug.LogError("NO TOCOMMANDLINE IMPLEMENTED! FOR " + this.ToString());
        return "";
    }

    public virtual string ToJson() {
        return JsonUtility.ToJson(this);
    }
}

public class VNCommandDialogue : VNCommand
{
    public override bool SkipInput {
        get {
            return false;
        }
        set {
            Debug.LogError("Can't do that!");
        }
    }

    public Dialogue dialogue;
    public void Generate(string _speakerIdentificator, string _dialogue) {
        VNCharacter character = VNDirector.instance.characterManager.GetCharacter(_speakerIdentificator);

        dialogue.speakerName = character.DisplayName;
        dialogue.messageColor = character.characterColor;
        dialogue.content = _dialogue;
        //Debug.Log(dialogue.speakerName);
    }

    public override void Execute() {
        Dialogue dialogueTemp = dialogue;
        dialogueTemp.content = VNDirector.instance.variableManager.ReplaceStringReferences(dialogue.content);
        VNDirector.instance.dialogueBoxManager.DisplayDialogue(dialogueTemp);
        VNDirector.instance.logManager.InsertNewLog(dialogueTemp);
        VNDirector.instance.voiceManager.PlayCurrentLine();
    }
    public override void ExecuteOnSkip() {
        VNDirector.instance.voiceManager.OnSkippedLine();
    }

    public override string ToCommandLine() {
        return dialogue.speakerName + ":" + dialogue.content;
    }

    //public override string ExtraInfoForDebug() {
        //string temp = "speakerName:" + dialogue.speakerName + "\n";
        //temp += "content: " + dialogue.content + "\n";
        //temp += "lineId" + dialogue.lineId;
        
        //return temp;
    //}
}


public class VNCommandSystemNarratorDialogue : VNCommandDialogue
{
    public override bool SkipInput {
        get {
            return false;
        }
        set {
            Debug.LogError("Can't do that!");
        }
    }
    public void Generate(string _dialogue) {
        dialogue.speakerName = "";
        dialogue.messageColor = VNDialogueBoxManager.systemNarratorColor;
        dialogue.content = _dialogue;
    }

    public override void Execute() {
        Dialogue dialogueTemp = dialogue;
        dialogueTemp.content = VNDirector.instance.variableManager.ReplaceStringReferences(dialogue.content);
        VNDirector.instance.dialogueBoxManager.DisplayDialogue(dialogueTemp);
        VNDirector.instance.logManager.InsertNewLog(dialogueTemp);
        VNDirector.instance.voiceManager.PlayCurrentLine();
    }
    public override void ExecuteOnSkip() {
        VNDirector.instance.voiceManager.OnSkippedLine();
    }
    public override string ToCommandLine() {
        return "system_narrator:" + dialogue.content;
    }

    //public override string ExtraInfoForDebug() {
    //    string temp = "speakerName:" + dialogue.speakerName + "\n";
    //    temp += "content: " + dialogue.content + "\n";
    //    temp += "lineId" + dialogue.lineId;

    //    return temp;
    //}
}

public class VNCommandLoadScene : VNCommand
{
    public override bool SkipInput => false;   //Setting this to true breaks the first dialogue of the next scene!?!??!?!?!??!

    [SerializeField]private string sceneName;

    public void Generate(string _sceneName) {
        sceneName = _sceneName;
    }

    public override void Execute() {
        VNDirector.instance.LoadSceneFromCurrentArc(sceneName);
    }
    public override string ToCommandLine() {
        return "scene =" + sceneName;
    }

    //public override string ExtraInfoForDebug() {
    //    string temp = "sceneID " + sceneName + "\n";
    //    return temp;
    //}
}

public class VNCommandChangeBackground : VNCommand {
    public override bool SkipInput => true;

    [SerializeField]private string backgroundName;

    public void Generate(string _backgroundName) {
        backgroundName = _backgroundName;
    }

    public override void Execute() {
        VNDirector.instance.backgroundManager.SetBackground(backgroundName);
    }
    public override string ToCommandLine() {
        return "background_snap = " + backgroundName;
    }
}


public class VNCommandFadeIntoNewBackground : VNCommand
{
    public override bool SkipInput => true;

    [SerializeField]private string newBackground;

    public void Generate(string _newBackground) {
        newBackground = _newBackground;
    }

    public override void Execute() {
        VNDirector.instance.backgroundManager.SetBackground(newBackground, true);
    }
    public override string ToCommandLine() {
        return "background = " + newBackground;
    }
}


public class VNCommandModifyFadeDuration : VNCommand
{
    public override bool SkipInput => true;

    [SerializeField]private float newDuration;

    public void Generate(float _newDuration) {
        newDuration = _newDuration;
    }

    public override void Execute() {
        VNBackgroundManager.fadeDuration = newDuration;
    }

}

public class VNCommandSoundChange : VNCommand
{
    public override bool SkipInput => true;
   

    [SerializeField]private VNAudioManager.SoundCommandType soundCommandType;
    [SerializeField]private string soundName;


    public override void Execute() {
        if (soundName != "none") {
            switch (soundCommandType) {
                case VNAudioManager.SoundCommandType.Effect:
                    VNDirector.instance.audioManager.PlayDynamicAudio(soundName);
                    break;
                default:
                    VNDirector.instance.audioManager.SetAudioForChannel(soundName, soundCommandType);
                    break;
            }
        }
        else {
            VNDirector.instance.audioManager.SilenceAudioForChannel(soundCommandType);
        }
    }

    public void Generate(string _soundName, VNAudioManager.SoundCommandType _soundType) {
        base.Generate();
        soundCommandType = _soundType;
        soundName = _soundName;
    }

    public override string ToCommandLine() {
        return "play_sound = " + soundName;
    }
}

public class VNCommandChangeCharacterDisplayProperties : VNCommand
{
    public override bool SkipInput => true;

    public enum PropertyType { X,Y,Scale,Pose, Hide, FlipX, FlipY, SmoothlyFlipX, SmoothFlipY, SetCharacterName }
    [SerializeField] private PropertyType propertyType;
    [SerializeField] string target;
    [SerializeField] float value1;
    [SerializeField] string value2;

    public void Generate(PropertyType _propertyType, string _target, float _value1 = 0, string _value2 = "") {
        propertyType = _propertyType;
        target = _target;
        value1 = _value1;
        value2 = _value2;
    }

    public override void Execute() {
        switch(propertyType) {
            case PropertyType.X:
                VNDirector.instance.characterManager.SetCharacterDisplayX(target, value1);
                return;
            case PropertyType.Y:
                VNDirector.instance.characterManager.SetCharacterDisplayY(target, value1);
                return;
            case PropertyType.Scale:
                VNDirector.instance.characterManager.SetCharacterDisplayScale(target, value1);
                return;
            case PropertyType.Pose:
                VNDirector.instance.characterManager.SetCharacterPose(target, value2);
                return;
                case PropertyType.Hide:
                VNDirector.instance.characterManager.DestroyCharacterPose(target);
                return;
            case PropertyType.FlipX:
                VNDirector.instance.characterManager.FlipCharacterX(target, value2, false);
                return;
            case PropertyType.SmoothlyFlipX:
                VNDirector.instance.characterManager.FlipCharacterX(target, value2, true);
                return;
            case PropertyType.SetCharacterName:
                VNDirector.instance.characterManager.SetCharacterName(target, value2);
                return;
        }
    }
    public override string ToCommandLine() {
        switch (propertyType) {
            case PropertyType.X:
                //VNDirector.instance.characterManager.SetCharacterDisplayX(target, value1);
                return target + ".x = " + value1;
            case PropertyType.Y:
                //VNDirector.instance.characterManager.SetCharacterDisplayY(target, value1);
                return target + ".y = " + value1;
            case PropertyType.Scale:
                //VNDirector.instance.characterManager.SetCharacterDisplayScale(target, value1);
                return base.ToCommandLine();
            case PropertyType.Pose:
                //VNDirector.instance.characterManager.SetCharacterPose(target, value2);
                return target + ".pose = " + value2;
            case PropertyType.Hide:
                //VNDirector.instance.characterManager.DestroyCharacterPose(target);
                return target + ".pose = hide";
            case PropertyType.FlipX:
                //VNDirector.instance.characterManager.FlipCharacterX(target, value2, false);
                return target + ".flipxsnap = " + value2;
            case PropertyType.SmoothlyFlipX:
                //VNDirector.instance.characterManager.FlipCharacterX(target, value2, true);
                return target + ".flipx = " + value2;
            case PropertyType.SetCharacterName:
                //VNDirector.instance.characterManager.FlipCharacterX(target, value2, true);
                return target + "name = " + value2;
            default:
                return base.ToCommandLine();
        }
    }
}

public class VNCommandCreateCharacterEffect : VNCommand
{
    public override bool SkipInput => true;

    string target;
    VNCharacterEffect effect;
  

    public void Generate(string _target, string[] _arguments, VNCharacterEffect _characterEffect) {
        target = _target;
        effect = _characterEffect;
        effect.InsertArguments(_arguments);
    }

    public override void Execute() {
        VNCharacterDisplay display = VNDirector.instance.characterManager.GetCharacterDisplay(target, true);

        VNDirector.instance.characterEffectManager.StartEffect(effect, display);
    }
}


public class VNCommandSetVariableInt : VNCommand
{
    public override bool SkipInput => true;
    string identifier;
    int value;

    public void Generate(string _identifier, int _value) {
        identifier = _identifier;
        value = _value;
    }

    public override void Execute() {
        VNDirector.instance.variableManager.SetVariable(identifier, value);
    }

    public override string ToCommandLine() {
        return identifier + " = " + value;
    }
}

public class VNCommandSetVariableString : VNCommand
{
    public override bool SkipInput => true;
    string identifier;
    string value;

    public void Generate(string _identifier, string _value) {
        identifier = _identifier;
        value = _value;
    }

    public override void Execute() {
        VNDirector.instance.variableManager.SetVariable(identifier, value);
    }

    public override string ToCommandLine() {
        return identifier + " = " + value;
    }
}

public class VNCommandIntVariableOperation : VNCommand
{
    public override bool SkipInput => true;
    string variableIdentifier;
    string operation;

    public void Generate(string _variableIdentifier, string _operation) {
        variableIdentifier = _variableIdentifier;
        operation = _operation;
    }

    public override void Execute() {
        Debug.Log(operation);
        VNDirector.instance.variableManager.SetVariable(variableIdentifier, VNDirector.instance.variableManager.ReturnOperation(operation));
    }

    public override string ToCommandLine() {
        return variableIdentifier + operation;
    }
}


public class VNCommandIf : VNCommand
{
    public override bool SkipInput => true;
    string argument;

    public void Generate(string _test) {
        argument = _test;
    }

    public override void Execute() {
        string test = VNDirector.instance.variableManager.ReplaceIntReferences(argument);
        if (!VNDirector.instance.variableManager.IfCheck(test)) {
            VNDirector.instance.skippingCommands = true;
        }
    }

    public override string ToCommandLine() {
        return "if[" + argument + "]";
    }
}

public class VNCommandDefineVar: VNCommand {
    public override bool SkipInput => true;
    string identifier;
    int type;

    public void Generate(string _identifier, int _type) { //_type: 0 - int; 1 - string
        identifier = _identifier;
        type = _type;
    }

    public override void Execute() {
        switch (type) {
            case 0:
                VNDirector.instance.variableManager.DefineInt(identifier);
                break;
            case 1:
                VNDirector.instance.variableManager.DefineString(identifier);
                break;
            default:
                Debug.LogWarning("Could not define " + identifier + " because it has no valid type!");
                break;
        }
    }
}

public class VNCommandClearVarWith : VNCommand {
    public override bool SkipInput => true;
    public string identifier;

    public void Generate(string _identifier) {
        identifier = _identifier;
    }

    public override void Execute() {
        VNDirector.instance.variableManager.ClearVarsThatContain(identifier);
    }
}

public class VNCommandEndif : VNCommand
{
    public override bool SkipInput => true;

    public override void Generate() {}

    public override void Execute() {
        VNDirector.instance.skippingCommands = false;
    }
    public override void ExecuteOnSkip() {
        VNDirector.instance.skippingCommands = false;
    }

    public override string ToCommandLine() {
        return "endif";
    }
}


public class VNCommandStartNewChoicePrompt : VNCommand
{
    public override bool SkipInput => true;
    string prompt;

    public void Generate(string _prompt) {
        prompt = _prompt;
    }

    public override void Execute() {
        VNDirector.instance.choiceManager.CreateChoice(prompt);
    }

    public override string ToCommandLine() {
        return "newchoiceprompt =" + prompt;
    }
}

public class VNCommandInsertChoice : VNCommand
{
    public override bool SkipInput => true;
    VNCommand consequence;
    string prompt;

    public void Generate(string _prompt, VNCommand _consequence) {
        prompt = _prompt;
        consequence = _consequence;
    }

    public override void Execute() {
        VNDirector.instance.choiceManager.InsertChoice(prompt, consequence);
    }

    public override string ToCommandLine() {
        return "addchoice = " + prompt + " > {" + consequence.ToCommandLine() + "}";
    }
}

public class VNCommandEndOfChoicePrompt : VNCommand
{
    public override bool SkipInput => true;

    public override void Generate() { }

    public override void Execute() {
        VNDirector.instance.choiceManager.FinishChoice();
    }

    public override string ToCommandLine() {
        return "endchoiceprompt";
    }
}