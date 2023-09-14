using UnityEngine;

public class sineMovement : MonoBehaviour
{
    public float sinMultiplier = 5;
    public float sinMax = 0.5f;
    public float offset = 0;
    public bool globalTime = true,useBaseY = false;
    float timeActive = 0,baseY = 0;
    void OnEnable()
    {
        baseY = transform.localPosition.y;
    }
    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0)
        {
            float lineWidth;
            if(globalTime)
            {
                lineWidth = Mathf.Clamp(sinMax*Mathf.Sin(((Time.timeSinceLevelLoad)*sinMultiplier)+offset),-sinMax,sinMax);
            }
            else
            {
                lineWidth = Mathf.Clamp(sinMax*Mathf.Sin(((timeActive)*sinMultiplier)+offset),-sinMax,sinMax);
                timeActive+=Time.deltaTime;
            }
            if(useBaseY)
            transform.localPosition = new Vector3(transform.localPosition.x,lineWidth+transform.localPosition.y,transform.localPosition.z);
            else transform.localPosition = new Vector3(transform.localPosition.x,lineWidth+baseY,transform.localPosition.z);
        }
    }
    void OnDisable()
    {
        timeActive = 0;
    }
}
