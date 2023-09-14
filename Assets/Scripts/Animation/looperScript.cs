using UnityEngine;

public class looperScript : MonoBehaviour
{
    public float loopSpeedX = 1,loopSpeedY = 0;
    public float maxOffsetX = 24,maxOffsetY = 10;
    public bool scrollX = true, scrollY = true;
    void Start()
    {
        maxOffsetX += transform.localPosition.x;
        maxOffsetY += transform.localPosition.y;        
    }
    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0)
        {
            if(scrollX)
            {
                transform.localPosition+=new Vector3(loopSpeedX,0,transform.localPosition.z);
                if(maxOffsetX>0&&transform.localPosition.x>=maxOffsetX||maxOffsetX<=0&&transform.localPosition.x<=maxOffsetX)
                    transform.localPosition-=new Vector3(maxOffsetX,0,0);
            }
            if(scrollY)
            {
                transform.localPosition+=new Vector3(0,loopSpeedY,transform.localPosition.z);
                if(maxOffsetY>0&&transform.localPosition.y>=maxOffsetY||maxOffsetY<=0&&transform.localPosition.y<=maxOffsetY)
                    transform.localPosition-=new Vector3(0,maxOffsetY,0);
            }
        }
    }
}
