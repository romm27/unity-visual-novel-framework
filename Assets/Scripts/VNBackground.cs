using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Background", menuName = "VN/Background")]
public class VNBackground : ScriptableObject
{
    [Header("Data")]
    public string identifier;
    public Sprite background;
}
