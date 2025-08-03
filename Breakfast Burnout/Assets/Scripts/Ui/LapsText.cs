using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LapsText : MonoBehaviour
{
    public TMP_Text textRef;
    public CheckpointSystem checkRef;

    // Update is called once per frame
    void Update()
    {
        textRef.text = "LAP: " + (checkRef.currentPlayerLap + 1) + "/" + checkRef.lapsRequiredToWin;

    }
}
