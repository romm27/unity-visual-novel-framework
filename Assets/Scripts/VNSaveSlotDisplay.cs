using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VNSaveSlotDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image thumbnail;
    [SerializeField] TextMeshProUGUI chapterName;
    [SerializeField] TextMeshProUGUI lastDialogue;
    [SerializeField] TextMeshProUGUI playTimeDisplay;
    [SerializeField] Button deleteButton;
    [SerializeField] Button overwriteButton;
    [SerializeField] Sprite emptyFileSprite;

    //Methods
    public void UpdateSlotDisplay(string _chapter, string _lastDialogue, int _playTime, Sprite _thumbnail, Color _dialogueColor) {

        chapterName.text = _chapter;
        lastDialogue.text = _lastDialogue;
        lastDialogue.color = _dialogueColor;
        playTimeDisplay.text = "Play Time: " + FormatToClock(_playTime);
        thumbnail.sprite = _thumbnail;
        deleteButton.gameObject.SetActive(true);
        overwriteButton.gameObject.SetActive(true);

    }

    public void DisplayAsEmpty() {
        chapterName.text = "Click here to save!";
        lastDialogue.text = "";
        playTimeDisplay.text = "";
        thumbnail.sprite = emptyFileSprite;
        deleteButton.gameObject.SetActive(false);
        overwriteButton.gameObject.SetActive(false);
    }

    //Methods
    public string FormatToClock(float _playTime) {
        string temp = "A Few Seconds";

        if (_playTime > 60) {
            int hour = Mathf.FloorToInt(_playTime / (60 * 60));
            int min = Mathf.FloorToInt(_playTime / 60) % 60;

            int day = Mathf.FloorToInt(hour / 24);

            temp = day + "d " + hour % 24 + "h " + min + "m ";
        }

        return temp;
    }
}
