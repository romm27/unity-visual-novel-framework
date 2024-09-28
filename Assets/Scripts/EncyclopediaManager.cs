using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Keep all the encyclopedia code in this file, this is supposed to be independent from VN3!

public class EncyclopediaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Encyclopedia encyclopedia;

}


[CreateAssetMenu(fileName = "New Encycopledia", menuName = "VN/Encycopledia")]
public class Encyclopedia : ScriptableObject {
    [Header("Data")]
    public EncyclopediaEntry[] content;

    [Header("Settings")]
    public EncyclopediaSection[] sections;
    public bool locked = false;


}


[System.Serializable]
public struct EncyclopediaEntry {
    public string identifier;
    public int section;
    public TextAsset content;
}

[System.Serializable]
public struct EncyclopediaSection {
    [Header("Settings")]
    public string displayName; //<---- also used as identifier
    public Sprite tabIcon;
    public bool locked;
}