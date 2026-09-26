using UnityEngine;

public class PlayerCollisionEvent : MonoBehaviour
{
    private PlayerMovement playerMove;
    void Start()
    {
        playerMove = GetComponentInParent<PlayerMovement>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        playerMove.playerOBJOnCollisionEnter(collision);
    }
}
