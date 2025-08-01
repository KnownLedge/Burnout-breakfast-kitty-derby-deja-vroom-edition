using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class PositionText : MonoBehaviour
{

    public Image frontImage;
    public Image backImage;
    [SerializeField] private Sprite[] Sprites;
    private float transitionTimer = 0f;
    public float transitionTime = 2f;
    private float percentage = 0f;

    private int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) {
            count++;
            updatePosText(count);
        }
        transitionTimer += Time.deltaTime;
        percentage = transitionTimer / transitionTime;
        if(percentage > 1)
        {
            percentage = 1;
            frontImage.sprite = backImage.sprite;
        }
        backImage.fillAmount = percentage;
        frontImage.fillAmount = 1 - percentage;
        
    }

    public void updatePosText(int Position)
    {
        backImage.sprite = Sprites[Position - 1];
        transitionTimer = 0f;
    }

}
