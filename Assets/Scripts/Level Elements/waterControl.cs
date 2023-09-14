using System.Collections;
using UnityEngine;

public class waterControl : MonoBehaviour
{
    public float drainSpeed = 0.05f, raiseSpeed = 0.001f;
    public int waterStages = 6;
    int currentStage = 1;
    int waitFrames = 0;
    int tickInt = 0;
    public int drainTime = 10;
    movingPlatformScript platScript;
    public AudioClip[] soundClips;
    Coroutine cor;
    GameData data;
    Transform platform;
    // Start is called before the first frame update
    void Start()
    {
        platScript = GetComponent<movingPlatformScript>();
        platform = transform.GetChild(0).transform;
        data = GameObject.Find("_GM").GetComponent<GameData>();
    }
    IEnumerator drainEvent()
    {
        //Debug.Log("water timer started.");
        while(waitFrames>0)
        {
            if(waitFrames>=300&&waitFrames%30==0
             ||waitFrames<300 &&waitFrames>=150&&waitFrames%20==0
             ||waitFrames<150&&waitFrames>=60&&waitFrames%10==0
             ||waitFrames<60&&waitFrames>=15&&waitFrames%5==0)
            {
                if(tickInt==0)
                data.playUnlistedSound(soundClips[2]);
                else data.playUnlistedSound(soundClips[3]);

                tickInt++;
                if(tickInt>1)tickInt = 0;
            }
            if(platScript.moving)
            {
               //Debug.Log(Mathf.Clamp(drainSpeed*(platform.position.y-platScript.CurrentTarget.position.y)/2,drainSpeed,0.2f));
                platScript.movementSpeed = Mathf.Clamp(drainSpeed*(platform.position.y-platScript.CurrentTarget.position.y),drainSpeed,0.5f);
            }
            waitFrames--;
            if(waitFrames==5)data.playSoundStatic(72);
            yield return new WaitUntil(()=>Time.timeScale!=0);
            yield return 0;
        }
        //data.playSoundStatic(72);
        //Debug.Log("water coming up");
        //if(platform.position.y<cam.position.y-20)platform.position = new Vector3(platform.position.x,cam.position.y-20,platform.position.z);
        platScript.movementSpeed = raiseSpeed*currentStage;
        data.playUnlistedSound(soundClips[0]);
        platScript.switchPoint(2);
        tickInt = 0;
    }
    public void drainWater()
    {
        //Debug.Log("water draining");
        if(waitFrames<=0)
        {
            waitFrames = 60*drainTime;
            data.playUnlistedSound(soundClips[1]);
            currentStage=Mathf.Clamp(currentStage+1,0,waterStages);
            platScript.movementSpeed = drainSpeed;
            platScript.switchPoint(1);
            if(cor!=null)StopCoroutine(cor);
            cor = StartCoroutine(drainEvent());
        }
        else
        {
            waitFrames = 60*drainTime;
        }
    }
    public void activateWater()
    {
        //Debug.Log("water coming up");
        //if(platform.position.y<cam.position.y-10)platform.position = new Vector3(platform.position.x,cam.position.y-10,platform.position.z);
        platScript.movementSpeed = raiseSpeed*currentStage;
        data.playUnlistedSound(soundClips[0]);
        platScript.switchPoint(2);
        tickInt = 0;
    }
    public void deactivateWater()
    {
        if(cor!=null)StopCoroutine(cor);
        tickInt = 0;
        data.playUnlistedSound(soundClips[1]);
        currentStage=Mathf.Clamp(currentStage+1,0,waterStages);
        platScript.movementSpeed = drainSpeed;
        platScript.switchPoint(1);
    }
}
