using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class VNInterpreter : MonoBehaviour
{
    [Header("Settings")]
    public string systemNarratorIdentifier = "system_narrator";

    public void Awake() {
        
    }


    //Methods
    public List<VNCommand> PullScriptFromDataPack(int _script) {
        return PullScriptFromDataPackStartingAt(_script, 0);
    }

    public List<VNCommand> PullScriptFromDataPackStartingAt(int _script, int _startingAt) {
        return InterpretScriptStartingAtLine(VNDirector.instance.ActiveArcDataPack.sceneScripts[_script].text.Split('\n'), _startingAt);
    }

    public VNCommand InterpretLine(string _commandLine) {
        string[] temp = new string[1];
        temp[0] = _commandLine;
        return InterpretScript(temp)[0];
    }

    public List<VNCommand> InterpretScript(string[] _script) {

        //Parse Script
        List<string> parsedScript = new List<string>();

        for (int i = 0; i < _script.Length; i++) {
            string finalLine = "";

            if (_script[i].Contains('#')) {
                finalLine = _script[i].Split('#')[0];
            }
            else {
                finalLine = _script[i].Trim();
            }

            if (finalLine == string.Empty) {
                continue;
            }

            parsedScript.Add(finalLine.Trim());
        }
        //Parsing Done
        List<VNCommand> finalCommands = new List<VNCommand>();

        for (int i = 0; i < parsedScript.Count; i++) {
            string parsedLine = parsedScript[i];

            //for dialogue========================================
            if (parsedLine.Contains(':')) {
                string dialogueTarget = parsedLine.Split(':', 2)[0].Trim();
                string content = parsedLine.Split(":", 2)[1].Trim();

                if (dialogueTarget != "system_narrator") {
                    VNCommandDialogue temp = new VNCommandDialogue();
                    temp.Generate(dialogueTarget, content); // <------------------- Add Character Name Puller once it is implemented
                    finalCommands.Add(temp);
                    continue;
                }

                else {
                    VNCommandSystemNarratorDialogue temp = new VNCommandSystemNarratorDialogue();
                    temp.Generate(content);
                    finalCommands.Add(temp);
                    continue;
                }
            }
            //=====================================================

            //For IFs
            if(parsedLine.Contains("if[")) {
                VNCommandIf temp = new VNCommandIf();

                string test = parsedLine.Split('[')[1].Split(']')[0];
                temp.Generate(test);
                finalCommands.Add(temp);
                continue;
            }

            if (parsedLine.Contains("endif")) {
                VNCommandEndif temp = new VNCommandEndif();
                temp.Generate();
                finalCommands.Add(temp);
                continue;
            }

            //Choice Prompt 1
            if (parsedLine.Contains("endchoiceprompt")) {
                VNCommandEndOfChoicePrompt temp = new VNCommandEndOfChoicePrompt();
                temp.Generate();
                finalCommands.Add(temp);
                continue;
            }

            //======================================================

            parsedLine = parsedLine.Replace('"'.ToString(), string.Empty);

            if(parsedLine == string.Empty) { //<- for some reason the parsed line is empty here investigate a better fix later
                continue;
            }

            //Debug.Log(parsedLine);
            string command = parsedLine.Split('=', 2)[0].Trim();
            string argument = parsedLine.Split('=', 2)[1].Trim();
            

            //Choice Prompt 2
            if (command.Contains("newchoiceprompt")) {
                VNCommandStartNewChoicePrompt temp = new VNCommandStartNewChoicePrompt();
                temp.Generate(argument);
                finalCommands.Add(temp);
                continue;
            }

            if (command.Contains("addchoice")) {
                string realArgument = argument.Split('>', 2)[0];
                string consequenceCommandLine = argument.Split('>', 2)[1];
                VNCommandInsertChoice temp = new VNCommandInsertChoice();

                VNCommand consequence = InterpretLine(consequenceCommandLine);

                temp.Generate(realArgument, consequence);
                finalCommands.Add(temp);
                continue;
            }

            //For Variables
            if (command.Contains("INT_")) {
                command = command.Replace("{", "").Replace("}", "").Trim(); //<-- replace with prettier method later
                argument = argument.Replace("{", "").Replace("}", "").Trim(); //<-- replace with prettier method later
                if (command.Contains('+') || command.Contains('-') || command.Contains('*') || command.Contains('/') ) {
                    VNCommandIntVariableOperation temp = new VNCommandIntVariableOperation();
                    temp.Generate(command, argument);
                    finalCommands.Add(temp);
                    continue;
                }
                else {
                    VNCommandSetVariableInt temp = new VNCommandSetVariableInt();
                    temp.Generate(command, int.Parse(argument));
                    finalCommands.Add(temp);
                    continue;
                }
            }
            if (command.Contains("STRING_")) {
                command = command.Replace("{", "").Replace("}", "").Trim(); //<-- replace with prettier method later
                argument = argument.Replace("{", "").Replace("}", "").Trim(); //<-- replace with prettier method later
                VNCommandSetVariableString temp = new VNCommandSetVariableString();
                temp.Generate(command, argument);
                finalCommands.Add(temp);
                continue;
            }

            //For . Commands(property change)
            if (parsedLine.Contains('.')) {
                //Debug.Log(parsedLine);

                string target = command.Split('.')[0].Trim();
                string property = command.Split('.')[1].Trim();

                if(target == "background" && property == "fade_duration") {
                    VNCommandModifyFadeDuration modifyTemp = new VNCommandModifyFadeDuration();
                    modifyTemp.Generate(float.Parse(argument, CultureInfo.InvariantCulture));
                    finalCommands.Add(modifyTemp);
                    continue;
                }

                //For Character effects
                if (property == "playeffect") {
                    VNCommandCreateCharacterEffect effectTemp = new VNCommandCreateCharacterEffect();

                    string[] arguments;

                    //Debugging Script
                    if(!argument.Contains('[') || !argument.Contains(']')) {
                        Debug.LogError("No [ or ] operators found in playeffect declaration, perhaps you used the wrong wrappers? - () instead of []");
                    }

                    arguments = argument.Split('[')[1].Replace("]", "").Split(';');

                    //Define Each Effect here
                    switch (argument.Split('[')[0]) {
                        case "slide_to":
                            effectTemp.Generate(target, arguments, new VNCharacterEffectSlideTo());
                            break;
                        case "fade_in":
                            effectTemp.Generate(target, arguments, new VNCharacterEffectFadeIn());
                            break;
                        case "fade_out":
                            effectTemp.Generate(target, arguments, new VNCharacterEffectFadeOut());
                            break;
                        default:
                            Debug.LogError("No valid Character effect defined for " + argument);
                            break;
                    }

                    finalCommands.Add(effectTemp);
                    continue;
                }

                //Debug.Log(parsedLine + " target:" + target);

                VNCommandChangeCharacterDisplayProperties temp = new VNCommandChangeCharacterDisplayProperties();

                //For Each Command
                if(property == "x") {
                    temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.X, target, float.Parse(argument, CultureInfo.InvariantCulture));
                }
                if (property == "y") {
                    temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.Y, target, float.Parse(argument, CultureInfo.InvariantCulture));
                }
                if (property == "scale") {
                    temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.Scale, target, float.Parse(argument, CultureInfo.InvariantCulture));
                }
                if (property == "pose") {
                    if (argument == "hide") {
                        temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.Hide, target);
                    }
                    else {
                        temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.Pose, target, 0, argument);
                    }
                }
                if (property == "flipxsnap") {
                    temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.FlipX, target, 0f, argument);
                }
                if (property == "flipx") {
                    temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.SmoothlyFlipX, target, 0f, argument);
                }
                if (property == "name") {
                    temp.Generate(VNCommandChangeCharacterDisplayProperties.PropertyType.SetCharacterName, target, 0f, argument);
                }

                finalCommands.Add(temp);
                continue;
            }

            //Scene Property Set Commands
            //Instantly Change Background
            if (command == "background_snap") {
                VNCommandChangeBackground temp = new VNCommandChangeBackground();
                temp.Generate(argument);
                finalCommands.Add(temp);
                continue;
            }
            //Start Background Fade to
            if (command == "background") {
                VNCommandFadeIntoNewBackground temp = new VNCommandFadeIntoNewBackground();

                temp.Generate(argument);
                finalCommands.Add(temp);
                continue;
            }

            //Define Variables
            if (command == "defineint") {
                VNCommandDefineVar temp = new VNCommandDefineVar();
                temp.Generate(argument, 0);
                finalCommands.Add(temp);
                continue;
            }
            if (command == "definestring") {
                VNCommandDefineVar temp = new VNCommandDefineVar();
                temp.Generate(argument, 1);
                finalCommands.Add(temp);
                continue;
            }

            //Forget Variables with identifier
            if (command == "clearvar") {
                VNCommandClearVarWith temp = new VNCommandClearVarWith();
                temp.Generate(argument);
                finalCommands.Add(temp);
                continue;
            }

            //Load Scene
            if (command == "scene") {
                VNCommandLoadScene temp = new VNCommandLoadScene();

                temp.Generate(argument);
                finalCommands.Add(temp);
                continue;
            }

            //Audio Set Commands
            if (command == "play_sound") {
                VNCommandSoundChange temp = new VNCommandSoundChange();
                temp.Generate(argument, VNAudioManager.SoundCommandType.Effect);
                finalCommands.Add(temp);
                continue;
            }
            if (command == "background_music") {
                VNCommandSoundChange temp = new VNCommandSoundChange();
                temp.Generate(argument, VNAudioManager.SoundCommandType.Music);
                finalCommands.Add(temp);
                continue;
            }
            if (command == "ambient_sound") {
                VNCommandSoundChange temp = new VNCommandSoundChange();
                temp.Generate(argument, VNAudioManager.SoundCommandType.Ambient);
                finalCommands.Add(temp);
                continue;
            }
            //VOICE TO IMPLEMENT

            //No Valid Command
            Debug.LogWarning("Couldn't parse line: " + (int)(i + 1) + ": " + parsedScript[i]);
        }

        //Set Line Numbers
        for(int j = 0; j < finalCommands.Count; j++) {
            finalCommands[j].lineNumber = j;
        }

        return finalCommands;
    }


    public List<VNCommand> InterpretScriptStartingAtLine(string[] _script, int _startingPoint) {
        List<VNCommand> temp = InterpretScript(_script);

        if (_startingPoint <= 0) {
            return temp;
        }


        List<VNCommand> final = new List<VNCommand>();

        for (int i = _startingPoint; i < temp.Count; i++) {
            final.Add(temp[i]);
        }
        return final;
    }


    //Methods

}
