using System.Collections;
using UnityEngine;

public class KlempodromeEvents : MonoBehaviour {
	public int newStartLine = 140;
	public int[] reEnterStartLine = new int[] {154,160,165,170};
	NPCScript NPC;
	TextBox TBScript;
	bool notActivated = true;
	Coroutine cor;
	public GameObject mapCanvas;
	bool showMapOnSelect = true;
	GameData Data;
	int timeFrames = 0;
	bool setOptionLine = false;
	Animator anim;
	dataShare DataS;
	// Use this for initialization
	void Start ()
	{
		if(NPC==null)
		{
			NPC = GetComponent<NPCScript>();
			TBScript = GameObject.Find("Textbox_Canvas").transform.GetChild(0).GetComponent<TextBox>();
			Data = GameObject.Find("_GM").GetComponent<GameData>();
			DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
			anim = GetComponent<Animator>();
			if(Data.mode==1)
			{
				anim.SetBool("Alt",true);
				if(NPC.inHub)
				{
					newStartLine = 700;
					reEnterStartLine = new int[]{705,709,714,719};
				}
			}
			else
			{
				if(dataShare.totalCompletedLevels>=35)NPC.startLine = NPC.inHub?779:774;
			}
		}
	}
	void OnEnable()
	{
		if(NPC==null)Start();
		if(DataS.mode==1)
		{
			anim.SetBool("Alt",true);
		}
	}
	// Update is called once per frame
	void Update ()
	{
		if(timeFrames>0)
		{
			timeFrames--;
			if(timeFrames==0)
			Time.timeScale = 1;
		}
		if(!NPC.active&&setOptionLine)
		setOptionLine = false;
		if(NPC.active)
		{
			if(!NPC.inHub)
			{
				if(TBScript.option2StartLine!=105&&!setOptionLine)
				{
					if(Data.mode!=1)
					{
						if(Data.floppies<2&&dataShare.totalCompletedLevels<35)
						{
							TBScript.option2StartLine = 125;
							//Debug.Log("Can't save.");
						}
						else
						{
							TBScript.option2StartLine = 120;
							//Debug.Log("Can save.");
						}
					}
					else TBScript.option3StartLine = 719;
					setOptionLine = true;
				}
			}
			else
			{
				if(!setOptionLine)
				{
					if(NPC.startLine!=81&&NPC.startLine!=93)
					{
						if(Data.mode!=1)
						{
							if(Data.floppies<2&&dataShare.totalCompletedLevels<35)
							{
								TBScript.option3StartLine = 125;
								//Debug.Log("Can't save.");
							}
							else
							{
								TBScript.option3StartLine = 120;
								//Debug.Log("Can save.");
							}
						}
						else TBScript.option3StartLine = 714;
					}
					setOptionLine = true;
				}
			}

			if(notActivated)
			{
				notActivated = false;
				if(cor!=null)
					StopCoroutine(cor);
				cor = DataS.StartCoroutine(events());
			}
		}
		if(NPC.talking&&TBScript.confirmOptionforOutside&&showMapOnSelect)
		{
			TBScript.confirmOptionforOutside=false;
			//Debug.Log(TBScript.currentOption + " selected");
			if(!NPC.inHub)
			{
				switch(TBScript.currentOption)
				{
					default: break;
					case 0: showMap(); break;
					case 1:
					//this is for saving;
					DataS.lastLoadedWorld = 0;
					if(cor!=null)
						StopCoroutine(cor);
					cor = StartCoroutine(saveData());
					break;
				}
			}
			else
			{
				switch(TBScript.currentOption)
				{
					default: break;
					case 0: showMap(); break;
					//go to shop
					case 1: if(cor!=null)
						StopCoroutine(cor);
					cor = StartCoroutine(enterHome()); break;
					case 2:
					//this is for saving;
					if(cor!=null)
						StopCoroutine(cor);
					cor = StartCoroutine(saveData());
					break;
				}
			}
		}
	}
	void showMap()
	{
		mapCanvas.SetActive(true);
	}
	IEnumerator saveData()
	{
		yield return new WaitUntil(()=>TBScript.eventInt==1);
		if(Data.mode!=1&&dataShare.totalCompletedLevels<35)
		Data.addFloppy(-2,false);
		Data.pauseMusic(true);
		anim.SetTrigger("Save");
		DataS.storedItem = Data.storedItemID;
		DataS.playerState = GameObject.Find("Player_main").GetComponent<playerSprite>().state;
		yield return new WaitForSeconds(0.8f);
		Time.timeScale = 0;
		DataS.StartCoroutine(DataS.saveData(false));
		timeFrames = 10;
		yield return new WaitForSeconds(0.7f);
		yield return new WaitUntil(()=>!DataS.saving);
		Data.pauseMusic(false);
	}
	IEnumerator events()
	{
		yield return new WaitUntil(()=>TBScript.eventInt>=1);
		showMapOnSelect = false;
		NPC.startLine = newStartLine;
		yield return new WaitUntil(()=>TBScript.eventInt>=2);
		MGCameraController cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		if(Data.mode!=1)
		Data.playSoundStatic(37);
		else Data.playSoundStatic(108);
		Data.fadeMusic(false);
		cam.fadeScreen(true);
		yield return new WaitUntil(()=>cam.fadeAnim>=1f);
		yield return new WaitUntil(()=> !Data.sfxSource.isPlaying);
		dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		dataShare.totalCompletedLevels++;
		DataS.loadWorldWithLoadScreen(1);
	}
	IEnumerator enterHome()
	{
		dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		HubCamera cam = GameObject.Find("Main Camera").GetComponent<HubCamera>();
		yield return new WaitUntil(()=>TBScript.eventInt==1);
			if(Data.mode!=1)
			Data.playSoundStatic(37);
			else Data.playSoundStatic(108);
			Data.fadeMusic(false);
			cam.fadeScreen(true);
			yield return new WaitUntil(()=> cam.fadeAnim>=1f);
			yield return new WaitUntil(()=> !Data.sfxSource.isPlaying);
			DataS.startInSub = true;
			DataS.checkpointValue = 1;
			DataS.loadSceneWithLoadScreen(3);
	}
}
