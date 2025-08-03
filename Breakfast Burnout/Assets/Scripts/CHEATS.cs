using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHEATS : MonoBehaviour
{
    public int loopRequirement = 80;
    public enum CheatState {Fair, Cheat}
    public static CheatState state = CheatState.Fair;

    public AudioSource audio;
    public AudioClip wrong;
    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (other.tag == "Player" && Collectable.totalLoops >= loopRequirement)
        {
            Collectable.totalLoops -= loopRequirement;
            state = CheatState.Cheat;
            audio.Play();
        }else if(other.tag == "Player")
        {
            audio.PlayOneShot(wrong);
        }
    }
}
