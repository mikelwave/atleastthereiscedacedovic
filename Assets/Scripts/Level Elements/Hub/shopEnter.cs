using UnityEngine;

public class shopEnter : MonoBehaviour
{
    public Transform cam,player;
    public float xPos = 0;
    public Vector3 leftPos,rightPos;

    public void switchCam()
    {
        if(player.position.x<xPos)
        {
            cam.position = leftPos;
        }
        else cam.position = rightPos;
    }
}
