using UnityEngine;

public class NetworkInfo : MonoBehaviour
{
    public static bool PLAYING_ONLINE = false;


    public void setOffline()
    {
        PLAYING_ONLINE = false;
    }

    public void setOnline()
    {
        PLAYING_ONLINE = true;
    }
}
