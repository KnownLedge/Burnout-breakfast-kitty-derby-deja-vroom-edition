using UnityEngine;

public class CameraScript : MonoBehaviour
{
    //Camera script, should handle any behaviours and events for the camera.

    //But breakfast burnout currently doesn't really have any that aren't player related, so this script only handles triggering an underwater effect

    [Header("Refernces")]
    public GameObject waterCover; //Object to cover screen with for water effect

    [Header("Underwater")]
    [Tooltip("Color of water areas if the trigger doesn't already apply a color")]
    public Color defaultWaterColor = new Color(0,0,1,0.2f);
    private int waterCount = 0; //Tally of how many objects have triggered "underwater" state


    public void triggerUnderWater()
    {
        waterCount++;
        waterCover.GetComponent<MeshRenderer>().material.color = defaultWaterColor;
        waterCover.SetActive(true);
    }
    //Override for unique color
    public void triggerUnderWater(Color waterColor)
    {
        waterCount++;
        waterCover.GetComponent<MeshRenderer>().material.color = waterColor;
        waterCover.SetActive(true);
    }

    public void deActivateUnderWater()
    {
        waterCount--;
        if (waterCount <= 0)
        {
            waterCount = 0;
            waterCover.SetActive(false);
        }
    }
}