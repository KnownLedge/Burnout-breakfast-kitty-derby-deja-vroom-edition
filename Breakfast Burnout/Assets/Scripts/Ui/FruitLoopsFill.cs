using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FruitLoopsFill : MonoBehaviour
{
    private Image image;
    public bool fillOnAwake = false;
    public bool shouldFill = false;

    private float fillTimer = 1f;

    public bool isFill = false;

    void Start()
    {
        image = GetComponent<Image>();
        if (fillOnAwake)
        {
          shouldFill = !shouldFill;  
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldFill)
        {
            image.fillAmount += fillTimer * Time.deltaTime;
            if(image.fillAmount >= 1)
                isFill = true;
        }
        else
        {
            image.fillAmount -= fillTimer * Time.deltaTime;
            if (image.fillAmount <= 0)
                isFill = false;
        }
    }
}
