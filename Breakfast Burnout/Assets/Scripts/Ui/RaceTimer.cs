using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceTimer : MonoBehaviour
{
    public float gameTime = 0f;
    public int roundedTime = 0;
    public string outputTime = string.Empty;

    public TMP_Text textRef;
    private float translatedTime;
    private int hours;
    private int minutes;
    private float mile; //mileseconds
    private float second;

    public float offSet = 0f;
    public int direction = 1;

    void Start()
    {
        gameTime = offSet;
    }

    // Update is called once per frame
    void Update()
    {
        gameTime += Time.deltaTime;
        
        roundedTime = Mathf.RoundToInt(gameTime);
        minutes = Mathf.Abs((roundedTime / 60)); //get total minutes spent
        second = Mathf.Abs((roundedTime % 60)); // get remainder seconds from what isn't a minute
        hours = (minutes / 60); // Get hours by divided minutes by 60
        minutes = Mathf.Abs((minutes % 60)); //Get remainder minutes off of calculation for hours
        mile = Mathf.Abs((Mathf.RoundToInt(gameTime * 100) % 60)); 


        outputTime = MakeString(minutes) + ":" + MakeString(second) + ":" + MakeString(mile);

        if (textRef != null)
        {
            textRef.text = "TIME " + outputTime;
        }

    }

    private string MakeString(int value)
    {
        if (value < 10)
        {
            return "0" + value.ToString();
        }
        else
        {
            return value.ToString();
        }
    }

    private string MakeString(float value) //Overide
    {
        if (value < 10)
        {
            return "0" + value.ToString();
        }
        else
        {
            return value.ToString();
        }
    }


}
