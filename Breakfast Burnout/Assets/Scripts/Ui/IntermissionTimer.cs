using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntermissionTimer : MonoBehaviour
{

    public RaceTimer timeRef;
    public FruitLoopsFill flFill;
    public int state = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timeRef.gameTime >= 0 && state == 0)
        {
            flFill.shouldFill = true;
            state = 1;
        }
        else if (state == 1 && flFill.isFill == true) {
            SceneManager.LoadScene("RACESCENE");
        
        }
    }
}
