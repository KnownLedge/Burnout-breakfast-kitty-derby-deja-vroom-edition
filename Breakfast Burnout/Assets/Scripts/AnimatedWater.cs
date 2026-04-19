using UnityEngine;

public class AnimatedWater : MonoBehaviour
{

    public float speedX = 0f;
    public float speedY = 4f;
    private float curX;
    private float curY;

    void Start()
    {
        curX = GetComponent<Renderer>().material.mainTextureOffset.x;
        curY = GetComponent<Renderer>().material.mainTextureOffset.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curX += Time.deltaTime * speedX;
        curY += Time.deltaTime * speedY;
        GetComponent<Renderer>().material.SetTextureOffset("_BaseMap", new Vector2(curX, curY));
    }
}
