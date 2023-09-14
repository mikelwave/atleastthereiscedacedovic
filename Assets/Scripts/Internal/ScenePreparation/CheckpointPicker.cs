using UnityEngine;

public class CheckpointPicker : MonoBehaviour
{
    SceneRestarter r;
    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<SceneRestarter>();
    }
    void setPoint(int val)
    {
        val = Mathf.Clamp(val,1,7);
        	//Debug.Log("To checkpoint #"+val);
			dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
			GameData Data = GameObject.Find("_GM").GetComponent<GameData>();
			DataS.savedCamPos = GameObject.Find("Main Camera").transform.position;
			DataS.hasred = Data.hasRed;
			DataS.hasblue = Data.hasBlue;
			DataS.hasyellow = Data.hasYellow;
			DataS.checkPointCoins = Data.coins;
			/*if(parTime!=0)
			{
				GameData data = GameObject.Find("_GM").GetComponent<GameData>();
				if(data.timer>parTime)
				{
					DataS.parTime = data.timer;
					print("Remaining time bigger than Checkpoint "+childInt+ " par time, setting parTime to: "+DataS.parTime);
				}
				else
				{
					DataS.parTime = parTime;
					print("Remaining time less than Checkpoint "+childInt+ " par time, setting parTime to: "+DataS.parTime);
				}
			}*/
			DataS.checkpointFloppies = Data.floppies;
			DataS.checkPointTimeCounter = Data.timeClock;
			DataS.checkpointLevelProgress = Data.currentLevelProgress;
			DataS.CheckpointSausages = Data.levelSausages;
			DataS.CheckpointScore = Data.score;
			DataS.checkpointValue = val;
        
        r.restartScene();
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.O)&&Input.GetKey(KeyCode.P))
        {   
            //if(Input.GetKey(KeyCode.Alpha0))
            //setPoint(0);
            foreach(char c in Input.inputString)
            {
                int o = 0;
                int.TryParse(c.ToString(),out o);
                if(o!=0)
                {
                    setPoint(o);
                }
            }

        }
    }
}
