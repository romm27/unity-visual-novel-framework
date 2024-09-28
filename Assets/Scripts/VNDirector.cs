using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class VNDirector : MonoBehaviour
{
    [Header("Insider References")]
    public VNDialogueBoxManager dialogueBoxManager;
    public VNCommandManager commandManager;
    public VNInterpreter interpreter;
    public VNCharacterManager characterManager;
    public VNBackgroundManager backgroundManager;
    public VNLogManager logManager;
    public VNAudioManager audioManager;
    public VNCharacterEffectManager characterEffectManager;
    [HideInInspector] public VNScreenEffectManager screenEffectManager; //<- background manager doing it's job right now, decide if it is gonna be scrapped or not later
    public VNVoiceManager voiceManager;
    public VNChoiceManager choiceManager;
    public VNVariableManager variableManager;
    public VNSaveManager saveManager;

    [Header("References")]
    [SerializeField] GameObject dialogueBoxSwitchTransform;
    public VNSettings globalOptions;

    [Header("Estates")]
    public bool playingDialogue = false;
    public bool skippingCommands = false;

    [Header("Events")]
    public UnityEvent OnCommandFinished = new UnityEvent();
    public UnityEvent OnPlayerNextMessageInput = new UnityEvent();
    public UnityEvent OnMessageSkip = new UnityEvent();
    public UnityEvent OnNewScriptLoaded = new UnityEvent();

    [Header("Data")]
    public VNChapterDataPack[] arcs;

    [Header("Settings")]
    [SerializeField] bool debugMessages = false;

    [Header("Analytics")]
    public float sessionPlayTime = 0;

    [Header("Debug Generated Commands")]
    [SerializeField] List<string> generatedCommandLines = new List<string>();

    private int executedCommands = 0;

    public static int currentSceneId = 0;
    public static int currentArc = 0;

    public static string firstScriptName = "pilot";

    public static VNDirector instance;

    private bool displayedEndOfScene = false;

    [HideInInspector] public int lastExecutedLineNumber;
    [HideInInspector] public string sceneName;

    public VNChapterDataPack ActiveArcDataPack {
        get {
            return arcs[currentArc];
        }
    }

    public bool ShowUI {
        get {
            return dialogueBoxSwitchTransform.activeInHierarchy;
        }
        set {
            dialogueBoxSwitchTransform.SetActive(value);
        }
    }

    public void Awake() {
        instance = this;
    }

    public void Start() {
        //DEBUG Setup
        LoadScene(0, 0);
    }

    public void Update() {
        //Debug.Log(currentArc + "|" + currentSceneId);
        //INPUT CHECKS
        //Check for Next Message
        if(Input.GetKeyDown(KeyCode.Space)) {
            InputNextMessage();
        }
        //Check for Toggle Box
        if (Input.GetKeyDown(KeyCode.Tab)) {
            dialogueBoxSwitchTransform.SetActive(!dialogueBoxSwitchTransform.activeInHierarchy);
        }

        sessionPlayTime += Time.deltaTime;
    }

    //Methods
    public void LoadScene(string _sceneName, int _arc, bool _autoPlay = true, int _startingPoint = 0) {
        executedCommands = _startingPoint;
        sceneName = _sceneName;
        commandManager.queuedCommands = new Queue<VNCommand>();


     

        if (_startingPoint == 0) {
            VNDirector.instance.voiceManager.ResetLineCount(); //<----- if not starting from the beggining it must be set manually for now!
        }

        for (int i = 0; i < arcs[_arc].sceneScripts.Length; i++) {
            if (arcs[_arc].sceneScripts[i].name == _sceneName) {
                commandManager.queuedCommands = new Queue<VNCommand>(interpreter.PullScriptFromDataPackStartingAt(i, _startingPoint));

                if (debugMessages) {
                    Queue<VNCommand> duplicate = new Queue<VNCommand>(interpreter.PullScriptFromDataPackStartingAt(i, _startingPoint));
                    for (int j = 0; j < commandManager.queuedCommands.Count; j++) {
                        generatedCommandLines.Add(duplicate.Dequeue().ToCommandLine());
                    }
                }

                OnNewScriptLoaded?.Invoke();
                if (_autoPlay) {
                    InputNextMessage();
                }
                currentSceneId = i;
                voiceManager.LoadLinesForCurrentScene();
                return;
            }
        }
        Debug.LogError("No valid script named " + _sceneName + " Is in datapack.");
    }

    public void LoadScene(int _sceneId, int _arc, bool _autoPlay = true, int _startingPoint = 0) {
        LoadScene(arcs[_arc].sceneScripts[_sceneId].name, _arc, _autoPlay, _startingPoint);
    }

    public void LoadSceneFromCurrentArc(string _sceneName, bool _autoPlay = true, int _startingPoint = 0) {
        LoadScene(_sceneName, currentArc, _autoPlay, _startingPoint);
    }

    public void LoadArc(int _arc) {
        currentArc = _arc;
    }

    public void InputNextMessage() {
        //Check for skips
        if (choiceManager.ChoiceInProgress) {
            return;
        }

        if (playingDialogue) {
            OnMessageSkip.Invoke();
        }

        VNCommand command = commandManager.GetNextCommand();

        if (command != null) {
            lastExecutedLineNumber = command.lineNumber;
            bool skippingThisCommand = skippingCommands;
            bool gambiarraFix = command.SkipInput;

            if (debugMessages) {
                string not = skippingThisCommand ? "NOT" : "";
                string automatically = gambiarraFix ? "Automatically " : "";
                //Debug.Log(command.lineNumber + " | " + command.SkipInput);
                Debug.Log("Now " + not + automatically + "executing:" + executedCommands + " - " + command.GetType().ToString() + " - Json:" + command.ToJson());
            }

            //Execute Command
            if (!skippingThisCommand) {
                command.Execute();
            }
            else {
                command.ExecuteOnSkip();
            }
            executedCommands++;

            if (gambiarraFix || skippingThisCommand) {
                //Debug.Log(command.lineNumber + " " + command.ToCommandLine());
                //Debug.Log(command.ToString() + " | " + command.SkipInput);
                InputNextMessage();
            }

            OnPlayerNextMessageInput?.Invoke();
        }
        else if(!displayedEndOfScene){
            //End of scene
            Debug.Log("End of Scene");
            displayedEndOfScene = true;
        }
    }

}


