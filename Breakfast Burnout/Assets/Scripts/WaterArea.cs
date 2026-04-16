using UnityEngine;

public class WaterArea : MonoBehaviour
{
    [Tooltip("Whether this water will use the next variables color for the water effect, or just use the color defined in the camera")]
    public bool useUniqueWaterColor = false;
    [Tooltip("Unique color the camera will be tinted when inside this object, requires above value to be true to be used")]
    public Color uniqueWaterColor = new Color(1, 1, 1, 0.2f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CameraScript>())
        {
            if (useUniqueWaterColor)
            {
                other.GetComponent<CameraScript>().triggerUnderWater(uniqueWaterColor);
            }
            else
            {
                other.GetComponent<CameraScript>().triggerUnderWater();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CameraScript>())
        {
            other.GetComponent<CameraScript>().deActivateUnderWater();
        }
    }
}
