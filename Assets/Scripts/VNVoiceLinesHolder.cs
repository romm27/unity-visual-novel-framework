using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Board", menuName = "VN/Audio Board")]
public class VNVoiceLinesHolder : ScriptableObject
{
    [Header("Data")]
    public AudioClip[] audioClips;

 }
