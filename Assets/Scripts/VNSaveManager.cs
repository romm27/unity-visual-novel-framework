using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class VNSaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject saveWindow;
    [SerializeField] GameObject promptWindow;
    [SerializeField] TextMeshProUGUI promptText;

    [Header("Save Elements")]
    [SerializeField] VNSaveSlotDisplay[] slots;

    [Header("Windows Settings")]
    public int currentPage;
    public int pageCount;

    [Header("Settings")]
    public bool saveScreenShotToSaveFile = true;
    public Vector2Int screenshotSize = new Vector2Int(320, 180);


    //Prompt Markers
    private int operationType = 0; //0 - overwrite; 1 - delete
    private int targetSlot = 0;


    public string GetSaveFilePath(string _fileName) {
        //Debug.Log(Application.dataPath + GlobalSettings.savePath + _fileName + ".json");
        string path = Application.persistentDataPath + GlobalSettings.savePath;
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        return path + '/' + _fileName + ".json";
    }

    //Methods

    public void SaveToHardDrive(string _fileName) {
        string json = JsonUtility.ToJson(GenerateSaveFile());
        File.WriteAllText(GetSaveFilePath(_fileName), json);
        //Debug.Log(GetSaveFilePath(_fileName));
    }
    public void LoadFromHardDrive(string _fileName) {
        LoadJson(File.ReadAllText(GetSaveFilePath(_fileName)));
    }

    public void LoadJson(string _json) {
        LoadFile(JsonUtility.FromJson<SaveFile>(_json));
    }

    public SaveFile GenerateSaveFile() {
        SaveFile temp = new SaveFile();

        //Save basic data
        temp.logs = VNDirector.instance.logManager.logs;
        temp.background = VNDirector.instance.backgroundManager.backgroundName;
        temp.lastExecutedCommandline = VNDirector.instance.lastExecutedLineNumber;
        temp.dialogueInDisplay = VNDirector.instance.dialogueBoxManager.currentDialogue;
        temp.sceneId = VNDirector.currentSceneId;
        temp.sceneName = VNDirector.instance.sceneName;
        temp.arcId = VNDirector.currentArc;
        temp.currentDialogueLine = VNDirector.instance.voiceManager.currentLine;
        temp.sessionPlayTime = Mathf.FloorToInt(VNDirector.instance.sessionPlayTime);

        //Choice
        temp.choiceInProgress = VNDirector.instance.choiceManager.ChoiceInProgress;
        if (temp.choiceInProgress) {
            temp.choiceFlavor = VNDirector.instance.choiceManager.ChoiceFlavor;
            temp.choiceCommands = VNDirector.instance.choiceManager.ChoiceCommands;
            temp.choicePrompt = VNDirector.instance.dialogueBoxManager.CurrentDialogueContent;
        }

        //Audio
        temp.loopingMusic = VNDirector.instance.audioManager.CurrentLoopingMusic;
        temp.loopingAmbiance = VNDirector.instance.audioManager.CurrentLoopingAmbiance;

        //Save Screen Elements
        List<CharacterDisplayData> list = new List<CharacterDisplayData>();
        for (int i = 0; i < VNDirector.instance.characterManager.activeCharacterDisplays.Count; i++) {
            CharacterDisplayData character = new CharacterDisplayData();
            character.identifier = VNDirector.instance.characterManager.activeCharacterDisplays[i].characterIdentifier;
            character.pose = VNDirector.instance.characterManager.activeCharacterDisplays[i].display.pose;
            character.pos = VNDirector.instance.characterManager.activeCharacterDisplays[i].display.GetV2Position;

            list.Add(character);
        }
        temp.characterDisplayData = list.ToArray();

        //Save Dynamic Character Settings
        List<CharacterDynamicData> characterData = new List<CharacterDynamicData>();
        for (int i = 0; i < VNDirector.instance.ActiveArcDataPack.sceneCharacters.Length; i++) {
            if (VNDirector.instance.ActiveArcDataPack.sceneCharacters[i].DisplayName != VNDirector.instance.ActiveArcDataPack.sceneCharacters[i].defaultCharacterName) {
                CharacterDynamicData tempDyn = new CharacterDynamicData();
                tempDyn.characterIdentifier = VNDirector.instance.ActiveArcDataPack.sceneCharacters[i].characterIdentifier;
                tempDyn.displayName = VNDirector.instance.ActiveArcDataPack.sceneCharacters[i].DisplayName;
                characterData.Add(tempDyn);
            }
        }
        temp.characterDynamicData = characterData.ToArray();

        //Save Variables
        temp.ints = VNDirector.instance.variableManager.loadedInts.ToArray();
        temp.strings = VNDirector.instance.variableManager.loadedStrings.ToArray();

        //Screen
        Texture2D tempTexture = CaptureCameraRenderAsTexture(screenshotSize.x, screenshotSize.y);
        //temp.screen = tempTexture.GetRawTextureData();

        temp.screenColors = tempTexture.GetPixels32();

        return temp;
    }

    public void LoadFile(SaveFile _file) {
        //Clear Old Stuff
        VNDirector.instance.characterManager.DestroyAllCharactersDisplays();
        VNDirector.instance.variableManager.loadedInts = new List<VNVariableInt>();
        VNDirector.instance.variableManager.loadedStrings = new List<VNVariableString>();
        VNDirector.instance.choiceManager.ClearChoiceCache();

        //Load Basic Data
        VNDirector.instance.logManager.logs = _file.logs;
        VNDirector.instance.backgroundManager.SetBackground(_file.background);
        VNDirector.instance.voiceManager.currentLine = _file.currentDialogueLine;
        VNDirector.instance.sessionPlayTime = _file.sessionPlayTime;

        //Load Audio
        if (_file.loopingMusic != string.Empty) {
            VNDirector.instance.audioManager.PlayInChannel(VNDirector.instance.audioManager.GetAudio(_file.loopingMusic, 1), 0);
        }
        if (_file.loopingAmbiance != string.Empty) {
            VNDirector.instance.audioManager.PlayInChannel(VNDirector.instance.audioManager.GetAudio(_file.loopingAmbiance, 1), 2);
        }

        //Load Dynamic Character Settings
        List<CharacterDynamicData> characterData = new List<CharacterDynamicData>();
        foreach (CharacterDynamicData index in _file.characterDynamicData) {
            VNDirector.instance.characterManager.SetCharacterName(index.characterIdentifier, index.displayName);
        }

        if (_file.choiceInProgress) {
            VNDirector.instance.choiceManager.CreateChoice(_file.choicePrompt);
            for (int i = 0; i < _file.choiceFlavor.Length; i++) {
                VNCommand temp = VNDirector.instance.interpreter.InterpretLine(_file.choiceCommands[i]);
                VNDirector.instance.choiceManager.InsertChoice(_file.choiceFlavor[i], temp);
            }
            VNDirector.instance.choiceManager.FinishChoice();
        }
        else {
            VNDirector.instance.dialogueBoxManager.DisplayDialogue(_file.dialogueInDisplay, true);
        }

        //Load Screen Elements
        for (int i = 0; i < _file.characterDisplayData.Length; i++) {
            VNDirector.instance.characterManager.SetCharacterPose(_file.characterDisplayData[i].identifier, _file.characterDisplayData[i].pose, false);
            VNDirector.instance.characterManager.SetCharacterDisplayX(_file.characterDisplayData[i].identifier, _file.characterDisplayData[i].pos.x);
            VNDirector.instance.characterManager.SetCharacterDisplayY(_file.characterDisplayData[i].identifier, _file.characterDisplayData[i].pos.y);
            VNDirector.instance.characterManager.FlipCharacterX(_file.characterDisplayData[i].identifier, _file.characterDisplayData[i].flipped, false);
        }

        //Set Variables
        for (int i = 0; i < _file.ints.Length; i++) {
            VNDirector.instance.variableManager.SetVariable(_file.ints[i].variableIdentifier, _file.ints[i].value);
        }
        for (int i = 0; i < _file.strings.Length; i++) {
            VNDirector.instance.variableManager.SetVariable(_file.strings[i].variableIdentifier, _file.strings[i].value);
        }

        //Reload Commands
        VNDirector.instance.LoadArc(_file.arcId);
        VNDirector.instance.LoadSceneFromCurrentArc(_file.sceneName, false, _file.lastExecutedCommandline + 1);
    }


    //UI Methods
    public int GetRealScreenSlotId(int _localSlot) {
        return (currentPage * slots.Length) + _localSlot;
    }

    public void SwitchSaveWindow() {
        bool activate = !saveWindow.activeInHierarchy;
        if(activate) {
            for (int i = 0; i < slots.Length; i++) {
                UpdateSaveSlotDisplay(i);
            }
        }

        saveWindow.SetActive(activate);
    }


    //File Control
    public void SaveToSlot(int _slot) {
        SaveFile temp = GenerateSaveFile();
        SaveToHardDrive("saveslot" + GetRealScreenSlotId(_slot));
    }

    public void DeleteSlot(int _slot) {
        File.Delete(GetSaveFilePath("saveslot" + GetRealScreenSlotId(_slot)));
    }

    public void LoadSaveSlot(int _slot) {
        int realSlot = GetRealScreenSlotId(_slot);
        LoadFromHardDrive("saveslot" + realSlot);
    }

    public bool SaveFileSlotExists(int _slot) {
        int realSlot = GetRealScreenSlotId(_slot);
        //Debug.Log(realSlot);
        return File.Exists(GetSaveFilePath("saveslot" + realSlot));
    }


    public void UpdateSaveSlotDisplay(int _slot) {
        bool fileExists = SaveFileSlotExists(_slot);

        if (fileExists) {
            string json = "saveslot" + GetRealScreenSlotId(_slot);
            string final = GetSaveFilePath(json);
            if (File.Exists(final)) {
                SaveFile temp = JsonUtility.FromJson<SaveFile>(File.ReadAllText(final));
                //Sprite load = ConvertByteArrayToSprite(temp.screen, screenshotSize.x, screenshotSize.y);
                Texture2D text2D = new Texture2D(screenshotSize.x, screenshotSize.y);
                text2D.SetPixels32(temp.screenColors);
                text2D.Apply();
                Sprite load = Sprite.Create(text2D, new Rect(0, 0, screenshotSize.x, screenshotSize.y), Vector2.one * 0.5f);
                slots[_slot].UpdateSlotDisplay(VNDirector.instance.arcs[temp.arcId].chapterName, temp.dialogueInDisplay.content, temp.sessionPlayTime, load, temp.dialogueInDisplay.messageColor);
            }
        }
        else {
            slots[_slot].DisplayAsEmpty();
        }
    }


    public Texture2D CaptureCameraRenderAsTexture(int _width, int _height) {
        bool showUiTemp = VNDirector.instance.ShowUI;

        VNDirector.instance.ShowUI = false;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = new RenderTexture(_width, _height, 24);
        mainCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        mainCamera.Render();

        // Create a new texture and read the render texture data into it
        Texture2D screenshotTexture = new Texture2D(_width, _height, TextureFormat.RGB24, false);
        screenshotTexture.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
        screenshotTexture.Apply();

        // Reset everything
        mainCamera.targetTexture = null;
        RenderTexture.active = currentRT;
        Destroy(renderTexture);

        VNDirector.instance.ShowUI = showUiTemp;

        return screenshotTexture;
    }

    public Sprite CaptureCameraRenderAsSprite(int _width, int _height) {
        Texture2D ScreenshotTexture = CaptureCameraRenderAsTexture(_width, _height);
        Sprite temp = Sprite.Create(ScreenshotTexture, new Rect(0, 0, _width, _height), Vector2.one * 0.5f);
        
        return temp;
    }

    public Sprite ConvertByteArrayToSprite(byte[] _byteArray, int _width, int _height) {
        Texture2D texture = new Texture2D(_width, _height);
        Debug.Log(_byteArray.Length);
        //texture.LoadImage(_byteArray);

        //texture.SetPixelData<byte>(_byteArray, 0, 0);


        //texture.LoadRawTextureData();
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, _width, _height), new Vector2(0.5f, 0.5f));

        return sprite;
    }

    //Button Control
    public void OnClickSaveSlot(int _slot) {
        //if(Input.GetKeyDown(KeyCode.Mouse0)) {
        if (SaveFileSlotExists(_slot)) {
            DisplayPrompt("Are you sure that you want to load Slot " + _slot.ToString() + "? \n Unsaved data will be lost.", _slot, 2);
        }
        else {
            SaveToSlot(_slot);
            UpdateSaveSlotDisplay(_slot);
        }
    }

    public void OnClickDeleteButton(int _slot) {
        if (SaveFileSlotExists(_slot)) {
            DisplayPrompt("Are you sure that you want to delete Slot " + _slot.ToString() + "?", _slot, 1);
        }
    }

    public void OnClickOverwrite(int _slot) {
        if (SaveFileSlotExists(_slot)) {
            DisplayPrompt("Are you sure that you want to overwrite Slot " + _slot.ToString() + "?", _slot, 0);
        }
    }


    //Prompt Control
    public void DisplayPrompt(string _prompt, int _slot, int _operationType) {
        promptText.text = _prompt;
        operationType = _operationType;
        targetSlot = _slot;
        promptWindow.SetActive(true);
    }

    public void CancelPrompt() {
        promptWindow.SetActive(false);
    }

    public void ConfirmPrompt() {
        switch (operationType) {
            case 0:              //Overwrite
                SaveToSlot(targetSlot);
                break;
            case 1:              //Delete
                DeleteSlot(targetSlot);
                break;
            case 2:              //Delete
                LoadSaveSlot(targetSlot);
                SwitchSaveWindow();
                break;
        }
        UpdateSaveSlotDisplay(targetSlot);
        promptWindow.SetActive(false);
    }
}

    [System.Serializable]
public class SaveFile
{
    public string logs;
    public string sceneName;
    public int arcId;
    public int sceneId;
    public int lastExecutedCommandline;
    public int currentDialogueLine;
    public int sessionPlayTime;
    public string background;
    public Dialogue dialogueInDisplay;
    public CharacterDisplayData[] characterDisplayData;
    public CharacterDynamicData[] characterDynamicData;


    //Variables
    public VNVariableInt[] ints;
    public VNVariableString[] strings;

    //Choice
    public bool choiceInProgress;
    public string choicePrompt;
    public string[] choiceFlavor;
    public string[] choiceCommands;

    //Sound
    public string loopingMusic;
    public string loopingAmbiance;

    //Screen
    //public byte[] screen;
    public Color32[] screenColors;
}

[System.Serializable]
public struct CharacterDisplayData
{
    public string identifier;
    public string pose;
    public Vector2 pos;
    public bool flipped;
}

[System.Serializable]
public struct CharacterDynamicData
{
    public string characterIdentifier;
    public string displayName;
}
    
