using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VNVariableManager : MonoBehaviour
{
    [Header("Data")]
    public List<VNVariableInt> loadedInts = new List<VNVariableInt>();
    public List<VNVariableString> loadedStrings = new List<VNVariableString>();

    [Header("Settings")]
    [SerializeField] TextAsset variableInitialisation;


    public void Start() {
        if(variableInitialisation != null && loadedInts.Count + loadedStrings.Count == 0) {
            string[] temp = variableInitialisation.text.Split('\n');

            Queue<VNCommand> commands = new Queue<VNCommand>(VNDirector.instance.interpreter.InterpretScript(temp));

            while (commands.Count > 0) {
                commands.Dequeue().Execute();
            }
        } 
    }

    //Methods
    public bool IfCheck(string _argument) {
        int type = 0; //0 - ==, 1 - <, 2 >

        int value1;
        int value2;

        if (_argument.Contains("==")) {
            type = 0;
        }
        if (_argument.Contains("<")) {
            type = 1;
        }
        if (_argument.Contains(">")) {
            type = 2;
        }

        switch (type) {
            default:
                Debug.Log("Error on If Manager");
                return false;
            case 0:
                string temp = _argument.Replace("==", "=");
                value1 = int.Parse(temp.Split('=')[0]);
                value2 = int.Parse(temp.Split("=")[1]);
                return value1 == value2;
            case 1:
                value1 = int.Parse(_argument.Split('<')[0]);
                value2 = int.Parse(_argument.Split("<")[1]);
                return value1 < value2;
            case 2:
                value1 = int.Parse(_argument.Split('>')[0]);
                value2 = int.Parse(_argument.Split(">")[1]);
                return value1 > value2;
        }
    }

    public int ReturnOperation(string _argument) {
        int type = 0; //0 - +; 1 - -; 2 - *; 3 - /;

        int value1;
        int value2;

        if (_argument.Contains("+")) {
            type = 1;
        }
        if (_argument.Contains("_")) {
            type = 2;
        }
        if (_argument.Contains("*")) {
            type = 3;
        }
        if (_argument.Contains("/")) {
            type = 4;
        }


        switch (type) {
            default:
                Debug.Log("Error on Value Manager");
                return 0;
            case 0: //<-- No Operation
                return int.Parse(_argument);
            case 1:
                //string temp = _argument.Replace("==", "=");
                value1 = int.Parse(_argument.Split('+')[0]);
                value2 = int.Parse(_argument.Split("+")[1]);
                return value1 + value2;
            case 2:
                value1 = int.Parse(_argument.Split('-')[0]);
                value2 = int.Parse(_argument.Split("-")[1]);
                return value1 - value2;
            case 3:
                value1 = int.Parse(_argument.Split('*')[0]);
                value2 = int.Parse(_argument.Split("*")[1]);
                return value1 * value2;
            case 4:
                value1 = int.Parse(_argument.Split('/')[0]);
                value2 = int.Parse(_argument.Split("/")[1]);
                return value1 / value2;
        }
    }

    //INT
    public void SetVariable(string _identifier, int _value) {
        int value;
        int index;

        if (IntVariableExists(_identifier, out value, out index)) {
            loadedInts[index] = new VNVariableInt(_identifier, _value);
        }
        else {
            loadedInts.Add(new VNVariableInt(_identifier, _value));
        }
    }

    public int GetVariableInt(string _identifier) {
        int temp;
        int temp1;

        if (IntVariableExists(_identifier, out temp, out temp1)) {
            return temp;
        }
        else {
            return 0;
            //Debug.LogError("No valid Int Variable with identifier " + _identifier + " Is loaded!");
            //return 0;
        }
    }

    public bool IntVariableExists(string _identifier) {
        for (int i = 0; i < loadedInts.Count; i++) {
            if (loadedInts[i].variableIdentifier == _identifier) {
                return true;
            }
        }
        return false;
    }
    public bool IntVariableExists(string _identifier, out int _value, out int _index) {
        for (int i = 0; i < loadedInts.Count; i++) {
            if (loadedInts[i].variableIdentifier == _identifier) {
                _value = loadedInts[i].value;
                _index = i;
                return true;
            }
        }
        _value = 0;
        _index= 0;
        return false;
    }


    //STRING
    public string GetVariableString(string _identifier) {
        string temp;
        int temp1;

        if (StringVariableExists(_identifier, out temp, out temp1)) {
            return temp;
        }
        else {
            //Debug.LogError("No valid Int Variable with identifier " + _identifier + " Is loaded!");
            return "";
        }
    }

    //Defining Variables
    public void DefineInt(string _identifier) {
        if (!IntVariableExists(_identifier)) {
            SetVariable(_identifier, 0);
        }   
    }

    public void DefineString(string _identifier) {
        if (!StringVariableExists(_identifier)) {
            SetVariable(_identifier, "");
        }
    }

    public void ClearVarsThatContain(string _target) {
        List<int> toRemove = new List<int>();

        //Ints
        List<VNVariableInt> newIntList = new List<VNVariableInt>();
        for(int i = 0; i < loadedInts.Count; i++) {
            if (loadedInts[i].variableIdentifier.Contains(_target)) {
                toRemove.Add(i);
            }
        }
        for(int i = 0; i < loadedInts.Count; i++){
            if (!toRemove.Contains(i)) {
                newIntList.Add(loadedInts[i]);
            }
        }
        loadedInts = newIntList;
        toRemove.Clear();

        //Strings
        List<VNVariableString> newStringList = new List<VNVariableString>();
        for (int i = 0; i < loadedStrings.Count; i++) {
            if (loadedStrings[i].variableIdentifier.Contains(_target)) {
                toRemove.Add(i);
            }
        }
        for (int i = 0; i < loadedStrings.Count; i++) {
            if (!toRemove.Contains(i)) {
                newStringList.Add(loadedStrings[i]);
            }
        }
        loadedStrings = newStringList;
        toRemove.Clear();
    }

    //Settings Variables
    public void SetVariable(string _identifier, string _value) {
        string value;
        int index;

        if (StringVariableExists(_identifier, out value, out index)) {
            loadedStrings[index] = new VNVariableString(_identifier, _value);
        }
        else {
            loadedStrings.Add(new VNVariableString(_identifier, _value));
        }
    }

    public bool StringVariableExists(string _identifier) {
        for (int i = 0; i < loadedInts.Count; i++) {
            if (loadedStrings[i].variableIdentifier == _identifier) {
                return true;
            }
        }
        return false;
    }
    public bool StringVariableExists(string _identifier, out string _value, out int _index) {
        for (int i = 0; i < loadedStrings.Count; i++) {
            if (loadedStrings[i].variableIdentifier == _identifier) {
                _value = loadedStrings[i].value;
                _index = i;
                return true;
            }
        }
        _value = "";
        _index = 0;
        return false;
    }


    public string ReplaceIntReferences(string _text) {
        string temp = _text;
        for (int i = 0; i < loadedInts.Count; i++) {
            temp = temp.Replace(loadedInts[i].variableIdentifier, loadedInts[i].value.ToString());
        }
        return temp;
    }

    public string ReplaceStringReferences(string _text) {
        if (!_text.Contains("STRING_")) {
            return _text;
        }

        string temp = _text;
        for (int i = 0; i < loadedStrings.Count; i++) {
            temp = temp.Replace(loadedStrings[i].variableIdentifier, loadedStrings[i].value);
        }
        return temp;
    }

}

[System.Serializable]
public struct VNVariableInt
{
    public string variableIdentifier;
    public int value;

    public VNVariableInt(string _identifier, int _value) {
        this.variableIdentifier = _identifier;
        this.value = _value;
    }
}

[System.Serializable]
public struct VNVariableString
{
    public string variableIdentifier;
    public string value;

    public VNVariableString(string _identifier, string _value) {
        this.variableIdentifier = _identifier;
        this.value = _value;
    }
}
