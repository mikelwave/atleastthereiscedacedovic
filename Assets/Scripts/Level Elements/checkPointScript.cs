using UnityEngine;
[ExecuteInEditMode]
public class checkPointScript : MonoBehaviour {
	public int childInt = 0;
	public int parTime = 0;
	public int startState = 0;
	public bool reached = false,silent = false,invisible = false,enableGrow = true,spawnPlayerAtCheckpoint = true;
	public bool subAreaCheckpoint = false;
	public float Yoffset = 0;
	public Vector3 customCameraSpawn = Vector3.zero;
	// Use this for initialization
	void Start () {
		if(!Application.isPlaying)
		{
			if(transform.name!="Checkpoint")
			{
				transform.name = "Checkpoint";
			}
			if(GameObject.Find("Checkpoints")==null)
			{
				Transform mainParent = GameObject.Find("LevelGrid").transform;
				var checkpointParent = new GameObject();
				checkpointParent.name = "Checkpoints";
				checkpointParent.transform.SetParent(mainParent);
				checkpointParent.transform.localPosition = Vector3.zero;
				//Debug.Log("No checkpoint parent found, created one as a child of "+mainParent.name);
			}
			transform.SetParent(GameObject.Find("Checkpoints").transform);
			if(transform.name=="Checkpoint")
			{
				for(int i = 0; i<transform.parent.childCount;i++)
				{
					if(transform.parent.GetChild(i).transform==transform)
					{
						//Debug.Log(transform + "is the same as "+transform.parent.GetChild(i));
						transform.name = transform.name + " " + (i+1);
						childInt = (i+1);
						break;
					}
				}
			}
		}
		if(Application.isPlaying)
		{
			if(invisible)GetComponent<SpriteRenderer>().enabled = false;
			for(int i = 0; i<transform.parent.childCount;i++)
				{
					if(transform.parent.GetChild(i).transform==transform)
					{
						childInt = (i+1);
						break;
					}
				}
		}
	}
	public void reach(Transform other)
	{
		if(reached) return;
		for(int i = 0; i< transform.parent.childCount;i++)
			{
				Animator a = transform.parent.GetChild(i).GetComponent<Animator>();
				if(a.gameObject.activeInHierarchy)
				a.SetBool("flipped", false);
				transform.parent.GetChild(i).GetComponent<checkPointScript>().reached = false;
			}
			reached = true;
			GetComponent<Animator>().SetBool("flipped",true);
			if(other!=null&&enableGrow&&!invisible&&other.parent.GetComponent<playerSprite>().state==0)
			{
				other.parent.GetComponent<PlayerScript>().growMethod(null,true);
			}
			Debug.Log("Reached checkpoint #"+childInt);
			dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
			GameData Data = GameObject.Find("_GM").GetComponent<GameData>();
			if(!silent&&other!=null) Data.playSoundStatic(26);
			DataS.savedCamPos = GameObject.Find("Main Camera").transform.position;
			if(subAreaCheckpoint)
			DataS.startInSub = true;
			DataS.hasred = Data.hasRed;
			DataS.hasblue = Data.hasBlue;
			DataS.hasyellow = Data.hasYellow;
			DataS.checkPointCoins = Data.coins;
			DataS.checkPointState = startState;
			Data.cheated = DataS.checkpointCheat;
			if(parTime!=0)
			{
				GameData data = GameObject.Find("_GM").GetComponent<GameData>();
				if(data.timer>parTime)
				{
					DataS.parTime = data.timer;
					//print("Remaining time bigger than Checkpoint "+childInt+ " par time, setting parTime to: "+DataS.parTime);
				}
				else
				{
					DataS.parTime = parTime;
					//print("Remaining time less than Checkpoint "+childInt+ " par time, setting parTime to: "+DataS.parTime);
				}
			}
			DataS.checkpointFloppies = Data.floppies;
			DataS.checkPointTimeCounter = Data.timeClock;
			DataS.checkpointLevelProgress = Data.currentLevelProgress;
			DataS.CheckpointSausages = Data.levelSausages;
			DataS.CheckpointScore = Data.score;
			DataS.checkpointValue = childInt;
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "PlayerCollider" && !other.transform.parent.GetComponent<PlayerScript>().dead)
		{
			reach(other.transform);
		}
	}
}
