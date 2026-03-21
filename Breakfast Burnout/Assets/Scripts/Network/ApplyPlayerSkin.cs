using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ApplyPlayerSkin : MonoBehaviour
{
    public int kartid = 1;
    public int catid = 1;

    public PlayerMovement playerRef;

    public List<GameObject> karts;
    public List<GameObject> cats;

    public GameObject currentKart;
    public GameObject currentCat;
    void Start()
    {
       // updateAppearance(kartid, catid);
    }

    public void updateAppearance(int kartSkin, int catSkin)
    {
        GameObject newKart = Instantiate(karts[kartSkin]);
        newKart.transform.SetParent(currentKart.transform.parent,false);
        currentKart.SetActive(false);
        currentKart = newKart;

        GameObject newCat = Instantiate(cats[catSkin]);
        newCat.transform.SetParent(currentCat.transform.parent,false);
        currentCat.SetActive(false);
        currentCat = newCat;

        if (currentKart.GetComponent<VehicleInfo>())
        {
            VehicleInfo newVI = currentKart.GetComponent<VehicleInfo>();
            playerRef.VI.frontSect = newVI.frontSect;
            playerRef.VI.rearSect = newVI.rearSect;

            playerRef.VI.frontWheels = newVI.frontWheels;
            playerRef.VI.rearWheels = newVI.rearWheels;
            playerRef.VI.cat = currentCat.transform;
        }
        
    }
}
