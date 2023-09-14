using UnityEngine;

public class playerMusicBounceTimer : MonoBehaviour
{
    float timer = 0;
    // Update is called once per frame
    void LateUpdate()
    {
        timer+=Time.deltaTime;
    }
    public float reset()
    {
        float t = timer;
        timer = 0;
        return t;
    }
}
