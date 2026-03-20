using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerPreviewModel : MonoBehaviour
{
    public List<GameObject> playerModels;
    public List<GameObject> kartModels;

    [SerializeField] private float minRotateSpeed = 1f;
    [SerializeField] private float maxRotateSpeed = 5f;
    [SerializeField] private float boostRotateSpeed = 1f;
    [SerializeField] private float deccelRotate = 0.3f;

    private float rotateSpeed = 1f;

    private int currentChar = 0;
    private int currentKart = 0;

    void Update()
    {
        rotateSpeed -= deccelRotate * Time.deltaTime;
        rotateSpeed = Mathf.Clamp(rotateSpeed, minRotateSpeed, maxRotateSpeed);

        transform.Rotate(transform.up * rotateSpeed * Time.deltaTime);
    }

    public void ChangeCharacterModel(int modelID)
    {
        playerModels[currentChar].SetActive(false);
        currentChar = modelID;
        playerModels[currentChar].SetActive(true);
        rotateSpeed += boostRotateSpeed;
    }

    public void ChangeKartModel(int modelID)
    {
        kartModels[currentKart].SetActive(false);
        currentKart = modelID;
        kartModels[currentKart].SetActive(true);
        rotateSpeed += boostRotateSpeed;
    }

}
