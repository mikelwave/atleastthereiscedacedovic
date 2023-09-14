using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class worldMapScript : MonoBehaviour {
	public AudioClip music;
	public int worldID = 0;
	public int levelPads = 0;
	public int activePads = 0;
	//determines how many levels were in the game up to this point.
	public int levelOffset = 0;
	public bool updatePadsB = false;
	public int highlightedPad = 0;
	public Sprite[] padSprites;
	Transform player;
	Transform targetPad;
	bool playerMoves = false;
	bool enteredLevel = false;
	public float speed = 1;
	dataShare datShare;
	TextMeshProUGUI worldName;
	TextMeshProUGUI levelName;
	TextMeshProUGUI levelTime;
	TextMeshProUGUI recordTimeLabel;
	TextMeshProUGUI coinCount;
	TextMeshProUGUI liveCount;
	TextMeshProUGUI scoreCount;
	TextMeshProUGUI floppyCount;
	Image floppyCollect;
	AxisSimulator axis;
	HubCamera camHubScript;
	public Transform NPCPoint;
	GameData data;
	bool canEnterLevel = false;
	int currentSceneID;

	public Sprite[] sausageMeterSprites,floppyCollectSprites;
	public GameObject fakePad;
	Transform UISausages;
	GameObject textbox;
	MenuScript pauseMenu;
	bool isIdle = false,queueNotif = false;
	public GameObject notif;
	Animator playerAnim;
	//playuh
	public Sprite playuhLives;
	public Sprite[] darkSprites;
	bool blackSigns = false;
	IEnumerator camDamp()
	{
		float startDamp = camHubScript.damping;
		camHubScript.damping = 0;
		yield return 0;
		yield return 0;
		camHubScript.damping=startDamp;
	}
	void OnEnable()
	{
		if(data==null)
		data = GameObject.Find("_GM").GetComponent<GameData>();
		data.inHub = true;
	}
	// Use this for initialization
	void Start () 
	{
		currentSceneID = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		pauseMenu = GameObject.Find("PauseCanvas").GetComponent<MenuScript>();
		pauseMenu.enabled = false;
		levelPads = transform.childCount;
		player = GameObject.Find("Player").transform;
		textbox = GameObject.Find("Textbox_Canvas").transform.GetChild(0).gameObject;
		datShare = GameObject.Find("DataShare").GetComponent<dataShare>();
		if(data==null)
		data = GameObject.Find("_GM").GetComponent<GameData>();
		data.audioEffectsToggle(0);
		data.sources[0].clip = music;
		bool canSave = false;
		if(datShare.worldProgression==worldID)
		{
			canSave = true;
			datShare.playCutscene=true;
			datShare.worldProgression++;
			GameObject.Find("Pixels").transform.GetChild(2).gameObject.SetActive(true);
		}
		worldName = GameObject.Find("WorldName").GetComponent<TextMeshProUGUI>();
		levelName = GameObject.Find("LevelName").GetComponent<TextMeshProUGUI>();
		levelTime = GameObject.Find("TimeLevel").GetComponent<TextMeshProUGUI>();
		recordTimeLabel = levelTime.transform.parent.GetComponent<TextMeshProUGUI>();
		axis = GameObject.Find("Player_main").GetComponent<AxisSimulator>();
		UISausages = GameObject.Find("Sausages").transform;
		camHubScript = GameObject.Find("Main Camera").GetComponent<HubCamera>();
		coinCount = GameObject.Find("teapotsCounter").GetComponent<TextMeshProUGUI>();
		liveCount = GameObject.Find("livesCounter").GetComponent<TextMeshProUGUI>();
		floppyCount = GameObject.Find("floppiesCounter").GetComponent<TextMeshProUGUI>();
		scoreCount = GameObject.Find("Score").GetComponent<TextMeshProUGUI>();
		floppyCollect = GameObject.Find("FloppyCollect").GetComponent<Image>();

		if(datShare.mode==1)
		{
			liveCount.transform.parent.GetComponent<Image>().sprite = playuhLives;
		}
		axis.transform.GetComponent<playerSprite>().state = datShare.playerState;
		if(datShare.playerState==4) axis.transform.GetChild(0).gameObject.SetActive(true);

		if(datShare.coins>999)datShare.coins = 999;
		liveCount.SetText("X"+datShare.lives.ToString("00")+(datShare.infiniteLives==1?"*" : ""));
		coinCount.SetText("X"+datShare.coins.ToString("000"));
		//coinCount.SetText("X"+datShare.coins);
		//if(coinCount!=null)
		//Debug.Log("X"+datShare.coins+" Actual: "+coinCount.text);
		//else Debug.LogError("X"+datShare.coins+" Actual: "+"coinCount Missing!");
		if(coinCount == null) Debug.LogError("X"+datShare.coins+" Actual: "+"coinCount Missing!");
		floppyCount.SetText("X"+datShare.floppies.ToString("00"));
		scoreCount.SetText(datShare.score.ToString("00000000"));
		datShare.resetValues();
		Time.timeScale = 1;
		camHubScript.fadeScreen(false);
		//camHubScript.transform.position = new Vector3(player.position.x,camHubScript.transform.position.y,camHubScript.transform.position.z);
		StartCoroutine(camDamp());
		playerAnim = player.GetChild(0).GetComponent<Animator>();
		//world 7 secret
		if(worldID==6)
		{
			if(datShare.lastLoadedLevel==41&&dataShare.totalCompletedLevels<=41)//if left secret level and not completed, set to test level.
			{
				datShare.lastLoadedLevel = 40;
			}
			else if(dataShare.totalCompletedLevels>=42)//all completed
			{
				Transform t = transform.GetChild(5).GetChild(2);
				t.SetParent(transform);
				t.gameObject.SetActive(true);
			}
		}
		activePads = Mathf.Clamp(dataShare.totalCompletedLevels-levelOffset+1,0,transform.childCount);

		if(!datShare.leavingLevel)
		{
			UpdatePads(activePads);
		}
		if(datShare.clearedUnbeatenLevel||canSave)
		{
			queueNotif = updateBook(); //queue a notification if lorebook has changed
		}
		//set player's position
		if(datShare.leavingLevel)
			enteredLevel = true;
		//Debug.Log("Last loaded level: "+datShare.lastLoadedLevel+", current level offset: "+levelOffset);
		if(datShare.lastLoadedLevel>levelOffset)
		{
			//Debug.Log("Valid level");
			highlightedPad = datShare.lastLoadedLevel-levelOffset+1;
		}
		else highlightedPad = 1;

		if(highlightedPad>transform.childCount)
		{
			//print("Highlighted pad "+highlightedPad+" bigger than level pad child count of "+transform.childCount);
			highlightedPad = 1;
		}
		
		targetPad = transform.GetChild(highlightedPad-1);
		player.position = targetPad.position;
		camHubScript.transform.position = new Vector3(player.position.x,camHubScript.transform.position.y,camHubScript.transform.position.z);


		bool exitSave = false;
		if(datShare.leavingLevel)
		{
			exitSave = true;
			StartCoroutine(levelExit());
		}
		else
		{
			StartCoroutine(playMusic());
			pauseMenu.enabled = true;
			if(queueNotif)
			{
				queueNotif = false;
				StartCoroutine(waitforTime());
			}
		}

		removeWorldNames();
		//setWorldName();
		if(highlightedPad>0)
		canEnterLevel = true;
		if(canSave||exitSave&&(datShare.worldProgression==7||datShare.mode==1))
		StartCoroutine(waitForSave());

	}
	bool updateBook()
	{
		return datShare.updateBook(true);
	}
	IEnumerator waitForSave()
	{
		yield return 0;
		yield return new WaitUntil(()=>Time.timeScale!=0);
		datShare.StartCoroutine(datShare.saveData(true));
	}
	IEnumerator waitforTime()
	{
		yield return 0;
		yield return new WaitUntil(()=>Time.timeScale!=0);
		spawnNotif();
	}
	void spawnNotif()
	{
		Transform t = Instantiate(notif,transform.position,Quaternion.identity).transform;
		t.SetParent(camHubScript.transform);
		if(worldID!=1)
		t.localPosition = new Vector3(-9.56f,0.78f,1);
		else t.localPosition = new Vector3(-9.56f,-0.22f,1);
		t.gameObject.SetActive(true);
	}
	IEnumerator playMusic(){
		yield return new WaitForSeconds(0.05f);
		yield return new WaitUntil(()=>Time.timeScale!=0);
		data.stopMusic(false,true);
	}
	IEnumerator levelExit()
	{
		//Debug.Log("Active pads: "+activePads);

		player.GetChild(0).GetComponent<Animator>().SetTrigger("exitLevel");
		if(datShare.clearedUnbeatenLevel)
		{
			if(((datShare.lastLoadedLevel-levelOffset+1)<transform.childCount)&&activePads<=transform.childCount)
			{
				datShare.clearedUnbeatenLevel = false;
				datShare.lastLoadedLevel++;
				//spawning fake pad
				UpdatePads(activePads-1);
				yield return new WaitForSeconds(0.5f);
				fakePad.transform.position = transform.GetChild(activePads).position;
				fakePad.SetActive(true);
				UpdatePads(activePads+1);
			}
			else
			{
				datShare.clearedUnbeatenLevel = false;
				UpdatePads(activePads);
				yield return new WaitForSeconds(0.5f);
			}
		}
		else
		{
			UpdatePads(activePads);
			yield return new WaitForSeconds(0.5f);
		}
		datShare.leavingLevel = false;
		yield return new WaitForSeconds(1.5f);
		//Debug.Log("exitedLevel");
		enteredLevel = false;
		data.stopMusic(false,true);
		pauseMenu.enabled = true;
		if(queueNotif)
		{
			queueNotif = false;
			spawnNotif();
		}
	}
	void removeWorldNames()
	{
		levelName.text = "";
		recordTimeLabel.text = "";
		levelTime.text = "";
		UISausages.GetChild(0).GetComponent<Image>().sprite = sausageMeterSprites[0];
		UISausages.GetChild(0).gameObject.SetActive(false);
		for(int i = 1; i<UISausages.childCount;i++)
		{
			Destroy(UISausages.GetChild(i).gameObject);
		}
		floppyCollect.gameObject.SetActive(false);
	}
	void setWorldName()
	{
		int semicolons = 0;
		int timeCharStartInt = 0;
		int SausagesStartInt = 0;
		string s = datShare.levelProgress[highlightedPad-1+levelOffset];
		worldName.text = datShare.worldNames[worldID]+" WORLD";
		if(highlightedPad==0)
		{
			levelName.text = "";
		}
		else levelName.text = (worldID+1)+"-"+highlightedPad+"  "+"\""+datShare.levelNames[highlightedPad-1+levelOffset]+"\"";
		//check if time exists
		if(highlightedPad!=0)
		{
			recordTimeLabel.text = "RECORD TIME";
			for(int i = 0; i<s.Length;i++)
			{
				if(s[i]==';')
					semicolons++;
				if(semicolons==1 && SausagesStartInt==0)
				{
					SausagesStartInt = i;
				}
				if(semicolons==2)
				{
					//if no floppy data, normal behaviour
					if(s[i+1]!='C')
					{
						timeCharStartInt = i;
						break;
					}
					//if there is floppy data, skip the next 2 characters (C;).
					else if(s[i+1]=='C')
					{
						timeCharStartInt = i+2;
						break;
					}
				}
			}
			if(semicolons<2)
			{
				levelTime.text="- Not yet cleared -";
			}
			else if(timeCharStartInt>0)
			{
				levelTime.text+="- ";
				string timeDisp = "";
				for(int h = timeCharStartInt+1; h<s.Length;h++)
				{
					timeDisp+=s[h];
				}
				int timeAsSecs = int.Parse(timeDisp);
				float minutes = Mathf.Floor(timeAsSecs/60),
				seconds = (timeAsSecs%60);
				levelTime.text+=minutes.ToString("00")+":"+seconds.ToString("00");

				levelTime.text+=" -";
			}
		}
		else
		{
			levelTime.text="- Not yet cleared -";
		}
		if(highlightedPad!=0)
		{
			//read how many zeros after first semi colon
			GameObject UISausage = UISausages.GetChild(0).gameObject;
			int childC = 0;
			for(int o = SausagesStartInt+1; o<s.Length; o++)
			{
				if(s[o]==';')
					break;
				if(childC==0)
				{
					UISausages.GetChild(0).gameObject.SetActive(true);
				}
				if(childC>0)
				{
					GameObject obj = Instantiate(UISausage,Vector3.zero,Quaternion.identity);
					obj.transform.SetParent(UISausages);
					obj.transform.localPosition = new Vector3(53.25f*childC,0,0);
					obj.transform.localScale = Vector3.one;
					obj.name = "Sausage "+childC;
					obj.SetActive(true);
				}
				Image sSprite = UISausages.GetChild(childC).GetComponent<Image>();
				switch(s[o])
				{
					default: sSprite.sprite = sausageMeterSprites[0]; break;
					case '1': sSprite.sprite = sausageMeterSprites[1]; break;
				}
				childC++;
			}
			if(childC!=0)
			{
				if(s.Contains("C;"))
				floppyCollect.sprite = floppyCollectSprites[1];
				else floppyCollect.sprite = floppyCollectSprites[0];

				floppyCollect.transform.position = new Vector3(UISausages.GetChild(0).position.x-1,floppyCollect.transform.position.y,0);

				floppyCollect.gameObject.SetActive(true);
			}
		}
	}
	IEnumerator levelEntrance()
	{
		data.fadeMusic(false);
		pauseMenu.enabled = false;
		yield return new WaitForSeconds(1.25f);
		camHubScript.fadeScreen(true);
		datShare.lastLoadedLevel = levelOffset+highlightedPad-1;
		yield return new WaitForSeconds(0.5f);
		datShare.loadSceneWithLoadScreen(highlightedPad+currentSceneID);
		
	}
	// Update is called once per frame
	void Update ()
	{
		if(Time.timeScale!=0)
		{
			if(!playerMoves&&axis.horAxis==0&&!enteredLevel&&SuperInput.GetKeyDown("Jump")&&canEnterLevel&&Time.timeScale!=0)
			{
				enteredLevel = true;
				player.GetChild(0).GetComponent<Animator>().SetTrigger("enterLevel");
				StartCoroutine(levelEntrance());
			}
			if(updatePadsB)
			{
				updatePadsB = false;
				UpdatePads(activePads);
			}
			if(playerMoves)
			{
				if(isIdle)
				{
					isIdle = false;
					playerAnim.SetBool("Idle",false);
				}
				player.position = Vector3.MoveTowards(player.position,targetPad.position,speed);
			}
			if(targetPad!=null&&player.position==targetPad.position&&highlightedPad>=1)
			{
				playerMoves = false;
				canEnterLevel = true;
				if(axis.horAxis==0)
				{
					player.position = new Vector3(player.position.x,-5.5f,player.position.z);
					setWorldName();
				}
			}
			else if(targetPad!=null&&player.position==targetPad.position&&highlightedPad<1)
			{
				playerMoves = false;
				canEnterLevel = false;
				playerAnim.SetBool("Idle",true);
				isIdle = true;
			}
			if(targetPad!=null&&player.position==targetPad.position&&highlightedPad==0)
			{
				playerMoves = false;
				canEnterLevel = false;
			}
			if(axis.horAxis==1&&highlightedPad<activePads&&!playerMoves&&!enteredLevel&&!textbox.activeInHierarchy)
			{
				removeWorldNames();
				player.localScale = Vector3.one;
				playerMoves = true;
				canEnterLevel = true;
				player.position = new Vector3(player.position.x,-5.75f,player.position.z);
				highlightedPad++;
				targetPad = transform.GetChild(highlightedPad-1);
			}
			else if(axis.horAxis==-1&&highlightedPad>1&&!playerMoves&&!enteredLevel&&!textbox.activeInHierarchy)
			{
				removeWorldNames();
				player.localScale = new Vector3(-1,1,1);
				playerMoves = true;
				player.position = new Vector3(player.position.x,-5.75f,player.position.z);
				highlightedPad--;
				targetPad = transform.GetChild(highlightedPad-1);
			}
			else if(axis.horAxis==-1&&highlightedPad==1&&!playerMoves&&!enteredLevel&&!textbox.activeInHierarchy)
			{
				removeWorldNames();
				player.localScale = new Vector3(-1,1,1);
				playerMoves = true;
				player.position = new Vector3(player.position.x,-5.75f,player.position.z);
				highlightedPad--;
					targetPad = NPCPoint;
			}
		}
	}
	string cutStr(string i)
	{
		i = i.Substring(11);
		return i;
	}
	void replaceSprite(SpriteRenderer rend1)
	{
		string sprName = rend1.sprite.name;
		sprName = cutStr(sprName);
		int id = int.Parse(sprName);
		rend1.sprite = darkSprites[id];
	}
	void UpdatePads(int progress)
	{
		//print("Progress: "+progress);
		activePads = progress;
		bool applySigns = !blackSigns ? true:false;
		blackSigns = true;
		for(int i = 0; i < transform.childCount; i++)
		{
			string s = datShare.levelProgress[i+levelOffset];
			//int sausagesCount = 0;
			bool goldPad = true; //assume all sausages are collected
			for(int c = 2; c<5;c++)
			{
				if(c<s.Length)
				{
					//if(s[c]=='1')
					//sausagesCount++;
					if(s[c]=='0')
					{
						//print(s);
						goldPad = false;
						break;
					}
					else if(s[c]==';')//skip
					break;
				}
			}
			SpriteRenderer rend = transform.GetChild(i).GetChild(0).GetComponent<SpriteRenderer>();
			Animation anim = transform.GetChild(i).GetComponent<Animation>();
			if(i>=activePads)
			{
				//print(s);
				//if(sausagesCount<=0)
				//if(sausagesCount<=0)
				//{
					if(anim.isPlaying)
					anim.Stop();
					rend.sprite = padSprites[0];
					rend.enabled = false;
				//}
				/*if(sausagesCount>0)
				{
					anim.Play();
					rend.sprite = padSprites[4];
					rend.enabled = true;
				}*/
			}
			else if(i<=activePads-1)
			{
				anim.Play();
				//if level skipped, silver
				if(s[0]=='S')
				{
					rend.sprite = padSprites[4]; //silver pad
				}
				else
				{
					//if(sausagesCount<=2)
					if(i==activePads-1&&s[0]=='N')
					{
						rend.sprite = padSprites[2]; //red pad
					}
					else if(!goldPad)
					{
						rend.sprite = padSprites[1]; //blue pad
					}
					else
					{
						rend.sprite = padSprites[3]; //gold pad
					}
				}
				rend.enabled = true;
				//sign things
				if(s[0]=='D'&&applySigns) //black sign
				{
					Transform t = rend.transform.parent.GetChild(1);
					SpriteRenderer rend1 = t.GetComponent<SpriteRenderer>();
					replaceSprite(rend1);
					
					for(int g = 0;g<t.childCount;g++)
					{
						rend1 = t.GetChild(g).GetComponent<SpriteRenderer>();
						replaceSprite(rend1);
					}
				}
			}
			/*else if(i==activePads-1)
			{
				anim.Play();
				//F = finished, N = Not finished, S = Skipped
				switch(s[0])
				{
					default:
					rend.sprite = padSprites[2];
					break;
					case 'F':
					rend.sprite = padSprites[1];
					break;
					case 'S':
					rend.sprite = padSprites[4];
					break;
				}
				rend.enabled = true;
			}*/
		}
	}
}
