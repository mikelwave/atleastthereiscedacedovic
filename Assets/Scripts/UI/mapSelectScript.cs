using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class mapSelectScript : MonoBehaviour {
	AxisSimulator axis;
	Transform mapIcon,worlds,textBox;
	dataShare dataS;
	GameData data;
	int currentWorld = 0;
	public int offset = 0;
	public Sprite[] sprites = new Sprite[2];
	Image player;
	int playerFrames = 15;
	int currentSprite = 0;
	bool pressedDown = false;
	public bool inHub = true;
	public bool entering = false;
	MGCameraController camLevel;
	HubCamera mapCamera;
	IEnumerator waitUntilTextboxClose()
	{
		yield return new WaitUntil(()=>!textBox.gameObject.activeInHierarchy);
		Time.timeScale = 0;
		mapIcon.gameObject.SetActive(true);
		mapIcon.position = worlds.GetChild(dataS.currentWorld).GetChild(0).position;
	}
	void setOffset(int newOffset)
	{
		offset = newOffset;
		player.sprite = sprites[offset];
		currentSprite = 2;
		player.SetNativeSize();
		playerFrames = 0;
	}
	// Use this for initialization
	void Awake ()
	{
		dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		textBox = GameObject.Find("Textbox_Canvas").transform.GetChild(0);
		axis = GameObject.Find("Player_main").GetComponent<AxisSimulator>();
		if(inHub)
			mapCamera = GameObject.Find("Main Camera").GetComponent<HubCamera>();
		else camLevel = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		worlds = transform.GetChild(1);
		mapIcon = transform.GetChild(2);
		player = mapIcon.GetComponent<Image>();
		mapIcon.gameObject.SetActive(false);
		currentWorld = dataS.currentWorld;
		if(data.mode ==1)
		{
			transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = sprites[4];
			setOffset(2);
		}
	}
	void OnEnable()
	{
		if(dataS.worldProgression==0)
		{
			dataS.worldProgression=1;
			dataShare.totalCompletedLevels++;
		}
		StartCoroutine(waitUntilTextboxClose());
		for(int i = 0;i<worlds.childCount;i++)
		{
			worlds.GetChild(i).gameObject.SetActive(true);
		}
		for(int i = dataS.worldProgression+1;i<worlds.childCount;i++)
		{
			worlds.GetChild(i).gameObject.SetActive(false);
		}
	}
	void OnDisable()
	{
		pressedDown = false;
		entering = false;
		mapIcon.gameObject.SetActive(false);
		//print("1:"+mapIcon.gameObject.activeInHierarchy);
		Time.timeScale = 1;
		//dataS.StartCoroutine(waitForTime());
	}
	// Update is called once per frame
	void Update ()
	{
		if(mapIcon.gameObject.activeInHierarchy)
		{
			if(Mathf.Abs(axis.horAxis)==1&&!pressedDown)
			{
				pressedDown = true;
				if(!entering) movePlayer(axis.horAxis);
			}
			else if(axis.horAxis==0&&pressedDown)
			{
				pressedDown = false;
			}
			if(SuperInput.GetKeyDown("Jump")&&!entering)
			{
				entering = true;
				StartCoroutine(enterWorld());
			}
			playerFrames--;
			if(playerFrames<=0)
			{
				currentSprite++;
				if(currentSprite>1+offset)
					currentSprite = 0+offset;
				player.sprite = sprites[currentSprite];
				playerFrames = 15;
			}
			if(Time.timeScale!=0&&mapIcon.gameObject.activeInHierarchy)
			{
				//print("2:"+mapIcon.gameObject.activeInHierarchy);
				Time.timeScale=0;
			}
		}
	}
	void movePlayer(float dir)
	{
		if(dir==1)
		{
			if(currentWorld<dataS.worldProgression)
			{
				data.playSoundStatic(36);
				currentWorld++;
			}
			else if(currentWorld==7&&dataS.worldProgression>6)
			{
				data.playSoundStatic(36);
				currentWorld = 0;
			}
		}
		else if(dir==-1)
		{
			if(currentWorld>0)
			{
				data.playSoundStatic(36);
				currentWorld--;
			}
			else if(currentWorld==0&&dataS.worldProgression>6)
			{
				data.playSoundStatic(36);
				currentWorld = dataS.worldProgression;
			}
		}
		mapIcon.position = worlds.GetChild(currentWorld).GetChild(0).position;
	}
	IEnumerator enterWorld()
	{
		if(currentWorld==dataS.currentWorld)
		{
			//Debug.Log("current world");
			gameObject.SetActive(false);
		}
		else
		{
			if(!inHub)
			{
				dataS.storedItem = data.storedItemID;
				dataS.playerState = GameObject.Find("Player_main").GetComponent<playerSprite>().state;
			}

			if(data.mode!=1)
			data.playSoundStatic(37);
			else data.playSoundStatic(108);
			data.fadeMusic(false);
			if(inHub)
			{
				mapCamera.fadeScreen(true);
				yield return new WaitUntil(()=> mapCamera.fadeAnim>=1f);
			}
			else
			{
				camLevel.fadeScreen(true);
				yield return new WaitUntil(()=> camLevel.fadeAnim>=1f);
			}
			yield return new WaitUntil(()=> !data.sfxSource.isPlaying);
				switch(currentWorld)
				{
					default: dataS.loadWorldWithLoadScreen(currentWorld); break;
					case 0: dataS.currentWorld = 0; dataS.loadSceneWithLoadScreen(3); break;
				}
		}
	}
}
