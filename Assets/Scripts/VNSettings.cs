using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New VN Settings", menuName = "Settings/VN Settings")]
public class VNSettings : ScriptableObject
{
    [Header("Audio Settings")]
    public float mainVolume = 1.0f;
    public float musicVolume = 1.0f;
    public float ambientVolume = 1.0f;
    public float voiceVolume = 1.0f;

    [Header("Dialogue Settings")]
    public float timeToNextCharacter = 0.025f;
    public float autoPlayTimeBetweenMessages = 2.5f;
}
