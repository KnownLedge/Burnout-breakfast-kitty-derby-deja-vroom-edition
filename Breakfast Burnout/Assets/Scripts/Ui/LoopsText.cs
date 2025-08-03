using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LoopsText : MonoBehaviour
{
    public TMP_Text textRef;

    // Update is called once per frame
    void Update()
    {
        textRef.text = "|" + Collectable.totalLoops + "|";


    }
}
