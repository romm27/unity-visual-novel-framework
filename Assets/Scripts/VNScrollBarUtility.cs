using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VNScrollBarUtility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Scrollbar scrollbar;

    //Methods
    public void Start() {
        scrollbar.value = 0;
    }

}
