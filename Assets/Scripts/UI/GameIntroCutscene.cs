using System.Collections;
using UnityEngine;

public class GameIntroCutscene : MonoBehaviour {
	public int startLine = 106;
	public TextAsset usedText;
	public AudioClip[] poor4voices = new AudioClip[3];
	public AudioClip stinger;
	Transform textboxTransform;
	TextBox TBScript;
	Transform player;
	PlayerScript pScript;
	AxisSimulator axis;
	Transform cam;
	Transform point;
	dataShare DataShare;
	public float camDamping = 0.2f;
	private Vector3 velocity = Vector3.zero;
	public float maxSpeed = 1f;
	bool moveCamera = false;
	GameData data;
	MGCameraController camControl;
	MenuScript pauseMenu;
	public Transform cutscene;
	Coroutine cor;
	bool canSkip = true;
	//NPCs
	Transform NPCHolder;
	// Use this for initialization
	void Start ()
	{
		NPCHolder = GameObject.Find("Storage_NPC").transform;
		DataShare = GameObject.Find("DataShare").GetComponent<dataShare>();
		//DataShare.lastLoadedWorld = 0;
		DataShare.currentWorld = 0;

		//spawn proper NPC
		int ID = 1; //Default: main mode
		if(DataShare.mode==1) //playuh mode
		{
			ID = 2;
		}
		else if(dataShare.totalCompletedLevels==0) //ceda intro
		{
			ID = 0;
		}
		//spawn only if not in shop
		NPCHolder.GetChild(ID).gameObject.SetActive(true);
		if(DataShare.startInSub)NPCHolder.gameObject.SetActive(false);

		//print("Used NPC: "+NPCHolder.GetChild(ID).name);

		pauseMenu = GameObject.Find("PauseCanvas").GetComponent<MenuScript>();
		pauseMenu.enabled = false;
		GameObject cedaShop = GameObject.Find("0_ceda");
		//Debug.Log(DataShare.levelProgress[0][0]);
		if(DataShare.levelProgress[0][0]=='N')
		{
			player = GameObject.Find("Player_main").transform;
			cam = GameObject.Find("Main Camera").transform;
			camControl = cam.GetComponent<MGCameraController>();
			data = GameObject.Find("_GM").GetComponent<GameData>();
			GameObject.Find("HUD_Canvas").GetComponent<Canvas>().enabled = false;
			pScript = player.GetComponent<PlayerScript>();
			if(pScript!=null)
				axis = pScript.transform.GetComponent<AxisSimulator>();
			else axis = GameObject.Find("Player_main").GetComponent<AxisSimulator>();
			player.localScale = Vector3.one;
			player.position = new Vector3(56.46f,player.position.y,player.position.z);
			textboxTransform = GameObject.Find("Textbox_Canvas").transform.GetChild(0);
			if(cedaShop!=null)cedaShop.SetActive(false);
			data.addFloppy(1,false);
			data.saveFloppies();
			TBScript = textboxTransform.GetComponent<TextBox>();
			TBScript.TextFile = usedText;
			if(player.childCount>0)
				TBScript.usedAnimators[0] = player.GetChild(0).GetComponent<Animator>();
			else TBScript.usedAnimators[0] = player.GetComponent<Animator>();
			TBScript.usedAnimators[1] = transform.GetChild(0).GetComponent<Animator>();
			TBScript.usedAnimators[2] = transform.GetChild(1).GetComponent<Animator>();
			TBScript.usedAnimators[3] = transform.GetChild(2).GetComponent<Animator>();
			point = transform.GetChild(3).transform;
			TBScript.startLine = startLine;
			TBScript.letter_type_other = poor4voices[0];
			TBScript.poor4Voices[0] = poor4voices[1];
			TBScript.poor4Voices[1] = poor4voices[2];
			cam.position = new Vector3(58.54f,45f,-10);
			data.stopMusic(true,true);
			cor = StartCoroutine(intro());
			for(int i = 1;i<6;i++)
			{
				DataShare.cellData[i] = true;
			}
			DataShare.updateBook(true);
			DataShare.forceBookNotifs();
		}
		else if(DataShare.levelProgress[0][0]=='F')
		{
			pauseMenu.enabled = true;
			if(DataShare.startInSub)	
			{
				GameObject.Find("Player_main").transform.localScale = Vector3.one;
			}
			else
			{
				if(cedaShop!=null)cedaShop.SetActive(false);
			}

			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(moveCamera)
		cam.position = Vector3.SmoothDamp(cam.position,point.position, ref velocity, camDamping,maxSpeed,Time.unscaledDeltaTime);

		if(SuperInput.GetKeyDown("Start")&&TBScript.eventInt!=3&&Time.timeScale!=0&canSkip)
		{
			canSkip = false;
			StopCoroutine(cor);
			cor = StartCoroutine(introSkip());
		}
	}
	IEnumerator introSkip()
	{
		TBScript.eventInt=3;
		data.stopMusic(true,false);
		cam.GetComponent<MGCameraController>().setInstantFade(true);
		yield return 0;
		moveCamera = false;
		for(int i = 0; i<3;i++)
			{
				transform.GetChild(i).gameObject.SetActive(false);
			}
			GameObject.Find("HUD_Canvas").GetComponent<Canvas>().enabled = true;
			cam.GetComponent<Camera>().orthographicSize = 6.75f;
			cam.position = new Vector3(60f,10.7f,-10);
			cam.GetComponent<MGCameraController>().setInstantFade(false);
			DataShare.levelProgress[0] = DataShare.levelProgress[0].Remove(0,1);
			DataShare.levelProgress[0] = DataShare.levelProgress[0].Insert(0,"F");
			data.stopMusic(false,true);
			pauseMenu.enabled = true;
			pScript.controllable = true;
			pScript.inCutscene = false;
			pScript.anim.SetBool("Talk",false);
			player.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
			axis.acceptXInputs = true;
			axis.acceptYInputs = true;
			TBScript.eventInt=0;
			TBScript.gameObject.SetActive(false);
	}
	IEnumerator intro()
	{
		cutscene.gameObject.SetActive(true);
		data.stopMusic(true,false);
		yield return 0;
		cam.GetComponent<Camera>().orthographicSize = 3.375f;
		player.GetComponent<playerSprite>().state = 1;
		if(pScript!=null)
		{
			pScript.controllable = false;
			pScript.inCutscene = true;
			player.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
			axis.acceptXInputs = false;
			axis.acceptYInputs = false;
		}
		camControl.setInstantFade(true);
		yield return new WaitForSeconds(0.05f);
		camControl.fadeScreen(false);
		yield return new WaitForSeconds(2f);
		data.stopMusic(false,true);
		moveCamera = true;
		yield return new WaitUntil(()=>cam.position==point.position);
		moveCamera = false;
		TBScript.protagonistOnLeft = true;
		TBScript.gameObject.SetActive(true);
		yield return new WaitUntil(()=>TBScript.eventInt==1);
		cam.GetComponent<MGCameraController>().setInstantFade(true);
		data.stopMusic(true,false);
		data.playUnlistedSound(stinger);
		yield return new WaitUntil(()=>TBScript.eventInt==2);
		for(int i = 0; i<3;i++)
		{
			transform.GetChild(i).gameObject.SetActive(false);
		}
		canSkip=false;
		GameObject.Find("HUD_Canvas").GetComponent<Canvas>().enabled = true;
		cam.GetComponent<Camera>().orthographicSize = 6.75f;
		cam.position = new Vector3(60f,10.7f,-10);
		data.playUnlistedSound(stinger);
		cam.GetComponent<MGCameraController>().setInstantFade(false);
		yield return new WaitUntil(()=>TBScript.eventInt==3);
		DataShare.levelProgress[0] = DataShare.levelProgress[0].Remove(0,1);
		DataShare.levelProgress[0] = DataShare.levelProgress[0].Insert(0,"F");
		data.stopMusic(false,false);
		pauseMenu.enabled = true;
	}
}
