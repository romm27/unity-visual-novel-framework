using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Security.Cryptography;

public class VNChoiceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject choiceUI;
    [SerializeField] TextMeshProUGUI[] choiceText;

    [Header("Temp Data")]
    public VNCommand[] choiceCommands;


    int placedChoices = 0;

    public bool ChoiceInProgress {
        get {
            return choiceUI.activeInHierarchy;
        }
    }


    //Get Active Prompts for save file
    public string[] ChoiceFlavor {
        get {
            string[] temp = new string[placedChoices];
            for (int i = 0; i < temp.Length; i++) {
                temp[i] = choiceText[i].text;
            }
            return temp;
        }
    }

    public string[] ChoiceCommands {
        get {
            string[] temp = new string[placedChoices];
            for (int i = 0; i < temp.Length; i++) {
                temp[i] = choiceCommands[i].ToCommandLine();
            }
            return temp;
        }
    }

    //Methods
    public void Awake() {
        choiceCommands = new VNCommand[choiceText.Length];
    }

    public void CreateChoice(string _prompt) {
        //Create Prompt
        VNCommandSystemNarratorDialogue temp = new VNCommandSystemNarratorDialogue();
        temp.Generate(_prompt);
        temp.Execute();
    }

    public void FinishChoice() {
        choiceUI.SetActive(true);
        for (int i = 0; i < placedChoices; i++) {
            choiceText[i].transform.parent.gameObject.SetActive(true);
        }
    }

    public void InsertChoice(string _choiceText, VNCommand _consequence) {
        //Debug.Log(_consequence.ToString());
        choiceText[placedChoices].text = _choiceText;
        choiceCommands[placedChoices] = _consequence;
        placedChoices++;
    }

    public void OnChoice(int _id) {
        choiceCommands[_id].Execute();
        ClearChoiceCache();

        //Continue
        VNDirector.instance.InputNextMessage();
    }

    public void ClearChoiceCache() {
        for (int i = 0; i < choiceText.Length; i++) {
            choiceText[i].transform.parent.gameObject.SetActive(false);
        }

        //Clear
        placedChoices = 0;
        choiceUI.SetActive(false);
        choiceCommands = new VNCommand[choiceText.Length];
    }

}

public struct QueuedVoiceLine
{

}
