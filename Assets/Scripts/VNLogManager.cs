using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VNLogManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject logWindowRoot;
    [SerializeField] TextMeshProUGUI logDisplay;


    [HideInInspector]public string logs;
    
    //Methods
    public void SwitchLogWindow(){
        bool activate = !logWindowRoot.activeInHierarchy;

        if (activate) {
            logDisplay.text = logs;
        }
        logWindowRoot.SetActive(activate);
    }

    public void InsertNewLog(string _log) {
        logs += _log + "\n";
    }

    public void InsertNewLog(Dialogue _log) {
        string extra = _log.speakerName != "" ? ": " : "";
        string line = "<color=#" + ColorUtility.ToHtmlStringRGBA(_log.messageColor) + ">" + _log.speakerName + extra + _log.content + "</color>";
        InsertNewLog(line);
    }

}
