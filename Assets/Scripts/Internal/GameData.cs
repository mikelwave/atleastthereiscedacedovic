using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;

public class GameData : MonoBehaviour {
	
	public string currentLevelProgress = "";
	public int mode = 0;
	public int coins;
	public int lives = 5;
	public int floppies;
	public long score = 0;
	public bool infiniteLives = false;
	public Tilemap map,invmap,ssmap;
	public GameObject boxItem;
	public Collider2D ball;
	public GameObject[] powerups;
	public TileBase[] replacementBlocks;
	public GameObject skeletileObject,lipParticle,boomStaticObject,cheeseSplatter;
	[HideInInspector]
	public List<skeletileScript> skeletileObjects;
	[HideInInspector]
	public List<GameObject> cheeseSplatters;
	[HideInInspector]
	public List<boomStaticScript> boomStaticObjects;
	[HideInInspector]
	public List<GameObject> lipParticles;
	[HideInInspector]
	public List<Vector3Int> bankPositions;
	playerSprite pSprites;
	AxisSimulator playerAxis;
	hitBlockStore blockStore;
	List<GameObject> fireballs,knives,LKnives;
	public GameObject fireball,knifeproj,lknifeproj;
	public int fireballCount = 3;
	public AudioClip[] sounds;
	public AudioClip[] Combosounds = new AudioClip[7];
	TextMeshProUGUI coinCount,liveCount,scoreCount,timeCount,floppiesCount,sausagesCount;
	Image storedItemSprite;
	Image SMeter;
	Animator Itembox;
	public AudioSource[] sources = new AudioSource[2];
	[HideInInspector]
	public AudioSource sfxSource;
	float musicVolume;
	float xAbs;
	//for Smeter
	Image[] SMeterArrows = new Image[7];
	public int litSMeterArrows = 0;
	int lastlitSMeterArrows = 0;
	public bool sMeterWorks = true;
	public Sprite[] sMeterSprites;
	int SMeterFrames = 0;

	int timeFrames = 60;
	public int timer = 500;
	public int timeClock = 0;
	public int startTime = 0;
	public bool musicSpedUp = false;
	public bool musicSpeedsUpWhenLowTime = true;
	public bool startInSub = false;
	bool paired = false;
	public Sprite[] storedItemSprites;
	public bool timerGoesDown = true;
	public bool timeFrozen = false;
	public bool timeToScore = false;
	public AudioMixerSnapshot[] snapshots;
	int usedSnapshot = 0;
	[Header ("for sausages")]
	public int levelSausages = 0;
	public Sprite[] sausageMeterSprites,keySprites;
	GameObject SausagesHold,KeyHold;
	Transform UISausages,UIKeys;
	public AudioSource eneSounds;
	Animator goalAnim;
	public bool reachedGoal = false;
	[HideInInspector] public bool scanForGoalEvent = false;
	public float gravityDivider = 1;
	public AudioClip MainLoop,Intro;

	public int storedItemID = 0;
	[Header("Kill streaks")]
	public int stompStreak = 0;
	public int halvaStreak = 0;
	[Header ("Audio effects")]
	public bool muffleInMainArea = false;
	public bool echoInMainArea = false;
	public bool muffleInSubArea = false;
	public bool echoInSubArea = false;
	[Header("Bounds")]
	public List<CameraBounds> camBounds;
	public GameObject[] parallaxBackgrounds = new GameObject[2];
	public GameObject[] areaSkies;
	List<GameObject> scoreIndicators;
	[HideInInspector]
	public CompositeCollider2D semiSolid;
	public bool inHub = false;
	public MGCameraController cam;
	public dataShare DataS;
	public bool finishedSaving = false;
	public static bool finishedLoading = false;
	int tickInt = 0;
	float musicMixerVolume;
	public AudioMixer mus;
	//bool musicOff = false;
	Color32[] timeColors = new Color32[2]{new Color32(255,255,255,255),new Color32(255,0,0,255)};
	int timeColorFrames = 0,currentTimeColor = 0;
	[Header("Eternal values")]
	public Sprite[] eternalItem = new Sprite[2];
	GameObject healthBar;
	public int currentHealth = 0;
	public Sprite timeUp;
	public TileBase[] switchTiles = new TileBase[2];
	public int redSwitchFrames = 0;
	bool redKey,blueKey,yellowKey = false;
	public bool hasRed,hasBlue,hasYellow = false;
	public bool switchMusic,halvaMusic = false;
	Coroutine introMusic,sPitch;
	string[] explodableBlocks = new string[]{"Brick","Brick_Pot","Brick_Bank","Brick_Bumbo","Brick_Cola","Brick_Pepper","Brick_Halva","Brick_1up","Brick_Garlic","Brick_Burek","Brick_Axe","Brick_Knife","Brick_Cola","Brick_C_Pepper","Brick_C_Halva","Brick_C_Burek","Brick_C_Axe","Brick_C_Knife","test blocks_7","test blocks_8","test blocks_9"};
	public GameObject pointSound,staticSound;
	AudioSource[] pointSounds,staticSounds;
	public List <Vector2> points;
	public GameObject smallExplosion;
	[HideInInspector]
	public bool forbidSkeletileRespawn = false;
	public bool inSub = false;
	bool homeLevel = false;
	PlayerScript pScript;
	Transform playerTransform;
	Coroutine musicFade;
	public GameObject impactGraphic;
	impactScript[] impacts;
	public bool saveMusicSamples;
	[HideInInspector]
	public int checkpointCount = 0;
	public bool cheated = false;
	int fillSoundFrames = 0;
	public IEnumerator musicIntro(AudioClip Intro, AudioClip actualLoop)
	{
		if(sources[0]!=null)
		{
			if(Intro!=null)
			{
				int clipLength = Intro.samples;
				//Debug.Log("Clip length: "+clipLength);
				sources[0].loop = false;
				sources[0].clip = Intro;
				yield return 0;
				yield return new WaitUntil(()=>Time.timeScale!=0);
				sources[0].Play();
				yield return new WaitUntil(()=>sources[0].timeSamples>=clipLength||!sources[0].isPlaying&&Time.timeScale!=0&&Application.isFocused);
				//Debug.Log(sources[0].isPlaying);
				//Debug.Log("Playing loop");
				if(sources[0].isPlaying)
				sources[0].Stop();
			}
			if(!pSprites.GetComponent<PlayerScript>().dead)
			{
				sources[0].timeSamples = 0;
				sources[0].loop = true;
				sources[0].clip = actualLoop;
				sources[0].Play();
			}
		}
	}
	IEnumerator skeletileCountdown(Vector3Int pos)
	{
		yield return new WaitForSeconds(5f);
		for(int i = 0;i<skeletileObjects.Count;i++)
		{
			if(!skeletileObjects[i].gameObject.activeInHierarchy)
			{
				if(!forbidSkeletileRespawn)
				{
					skeletileObjects[i].Spawn(pos,1);
				}
				break;
			}
		}
	}
	void createImpacts()
	{
		impacts = new impactScript[5];
		for(int i = 0;i<impacts.Length;i++)
		{
			impacts[i] = Instantiate(impactGraphic,transform.position,Quaternion.identity).GetComponent<impactScript>();
			impacts[i].initialize();
		}
	}
	public void spawnImpact(Vector3 pos)
	{
		//find unused impact
		if(impacts.Length!=0)
		{
			for(int i = 0;i<impacts.Length;i++)
			{
				if(!impacts[i].gameObject.activeInHierarchy)
				{
					impacts[i].setPos(pos);
					impacts[i].spawn();
					return;
				}
			}
			impacts[0].setPos(pos);
			impacts[0].spawn();
		}
	}
	public void spawnImpactAtPlayer()
	{

		spawnImpact(playerTransform.position+(Vector3.up*0.5f*playerTransform.localScale.y));
	}
	public void addSkeletileCor(Vector3Int pos)
	{
		StartCoroutine(skeletileCountdown(pos));
	}
	public void spawnSkeletileAtPoint(Vector3Int pos,bool appear)
	{
		for(int i = 0;i<skeletileObjects.Count;i++)
		{
			if(!skeletileObjects[i].gameObject.activeInHierarchy)
			{
				if(!appear)
				{
					if(ssmap.GetTile(pos)!=null)
					{
						ssmap.SetTile(pos,replacementBlocks[5]);
						skeletileObjects[i].Spawn(pos,2);
					}
				}
				else skeletileObjects[i].Spawn(pos,1);
				break;
			}
		}
	}
	public void spawnCheeseSplatterPoint(Vector3 pos)
	{
		for(int i = 0;i<cheeseSplatters.Count;i++)
		{
			if(!cheeseSplatters[i].gameObject.activeInHierarchy)
			{
				cheeseSplatters[i].transform.position = pos-new Vector3(0,0.5f,0);
				playSound(111+Random.Range(0,3),pos);
				cheeseSplatters[i].SetActive(true);
				break;
			}
		}
	}
	public void spawnCheeseSplatterPointNoSound(Vector3 pos)
	{
		for(int i = 0;i<cheeseSplatters.Count;i++)
		{
			if(!cheeseSplatters[i].gameObject.activeInHierarchy)
			{
				cheeseSplatters[i].transform.position = pos-new Vector3(0,0.5f,0);
				cheeseSplatters[i].SetActive(true);
				break;
			}
		}
	}
	public bool fire (int type,Vector3 pos,Vector3 scale,bool flipGravity)
	{
		switch(type)
		{
		//fireball is a default projectile type
		default:
			for(int i = 0; i<fireballs.Count;i++)
			{
				if(!fireballs[i].activeInHierarchy)
				{
					pSprites.gameObject.GetComponent<AudioSource>().PlayOneShot(sounds[7]);
					Gravity grav = fireballs[i].GetComponent<Gravity>();
					if(!flipGravity)
					{

						fireballs[i].transform.eulerAngles = Vector3.zero;
						grav.pushForces = new Vector2(grav.pushForces.x,-Mathf.Abs(grav.pushForces.y));
						fireballs[i].transform.position = new Vector3(pos.x+(0.5f*scale.x),pos.y+0.75f,pos.z);
					}
					else
					{
						fireballs[i].transform.eulerAngles = new Vector3(0,0,180);
						grav.pushForces = new Vector2(grav.pushForces.x,Mathf.Abs(grav.pushForces.y));
						fireballs[i].transform.position = new Vector3(pos.x+(0.5f*scale.x),pos.y-0.75f,pos.z);
					}
					fireballs[i].transform.localScale = scale;
					fireballs[i].SetActive(true);
					return true;
					//break;
				}
			}
			return false;
		//break;
		//1 is the knife projectile
		case 1:
			for(int i = 0; i<knives.Count;i++)
			{
				if(!knives[i].activeInHierarchy)
				{
					pSprites.gameObject.GetComponent<AudioSource>().PlayOneShot(sounds[52]);
					if(!flipGravity)
					{
						knives[i].transform.eulerAngles = Vector3.zero;
						knives[i].transform.position = new Vector3(pos.x+(0.5f*scale.x),pos.y+0.5f,pos.z);
					}
					else
					{
						knives[i].transform.eulerAngles = new Vector3(0,0,180);
						knives[i].transform.position = new Vector3(pos.x+(0.5f*scale.x),pos.y-0.5f,pos.z);
					}
					knives[i].transform.localScale = scale;
					knives[i].SetActive(true);
					return true;
					//break;
				}
			}
			return false;
		//break;
		//2 is the L knife projectile
		case 2:
			for(int i = 0; i<LKnives.Count;i++)
			{
				if(!LKnives[i].activeInHierarchy)
				{
					pSprites.gameObject.GetComponent<AudioSource>().PlayOneShot(sounds[80]);
					if(!flipGravity)
					{
						LKnives[i].transform.eulerAngles = Vector3.zero;
						LKnives[i].transform.position = new Vector3(pos.x+(0.5f*scale.x),pos.y+0.5f,pos.z);
					}
					else
					{
						LKnives[i].transform.eulerAngles = new Vector3(0,0,180);
						LKnives[i].transform.position = new Vector3(pos.x+(0.5f*scale.x),pos.y-0.5f,pos.z);
					}
					LKnives[i].transform.localScale = scale;
					LKnives[i].SetActive(true);
					return true;
					//break;
				}
			}
			return false;
		//break;
		}
	}
	public void updateKeys(int ID,bool playunlockSound)
	{
		if(UIKeys.childCount==0)return;
		unlockBlock(ID);
		if(playunlockSound)
		playSoundStatic(50);
		switch(ID)
		{
			//def = 0 = red key
			default:
			if(!hasRed&&redKey)
			{
				UIKeys.GetChild(0).GetComponent<Image>().sprite = keySprites[1];
				hasRed = true;
			}
			break;
			//1 = blue key
			case 1:
			if(!hasBlue&&blueKey)
			{
				if(redKey)
				{
					if(UIKeys.childCount<2)return;
					UIKeys.GetChild(1).GetComponent<Image>().sprite = keySprites[1];
				}
				else UIKeys.GetChild(0).GetComponent<Image>().sprite = keySprites[1];
				hasBlue = true;
			}
			break;
			//2 = yellow key
			case 2:
			if(!hasYellow&&yellowKey)
			{
				if(redKey&&blueKey)
				{
					if(UIKeys.childCount<3)return;
					UIKeys.GetChild(2).GetComponent<Image>().sprite = keySprites[1];
				}
				else if(!redKey&&blueKey||redKey&&!blueKey)
				{
					if(UIKeys.childCount<2)return;
					UIKeys.GetChild(1).GetComponent<Image>().sprite = keySprites[1];
				}
				else UIKeys.GetChild(0).GetComponent<Image>().sprite = keySprites[1];
				hasYellow = true;
			}
			break;
		}
	}
	void updateEnemies()
	{
		Transform e = GameObject.Find("Enemies").transform;
		if(e!=null)
		for(int i = 0;i<e.childCount;i++)
		{
			if(e.GetChild(i).name.Contains("skeleton"))
			{
				e.GetChild(i).GetChild(0).GetComponent<skeletonPlantScript>().hardMode();
			}
			else
			{
				switch(e.GetChild(i).name)
				{
					default: break;
					case "Cannon":
					e.GetChild(i).GetComponent<CannonScript>().hardMode();
					break;
					case "Gopnik_main":
					e.GetChild(i).GetChild(1).GetComponent<CannonScript>().hardMode();
					break;
				}
			}
		}
	}
	void Awake()
	{
		if(!inHub)
			semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
		Time.timeScale = 1;
	}
	void OnEnable()
	{
		finishedLoading = false;
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		sfxSource = GetComponent<AudioSource>();
		eneSounds = transform.GetChild(3).GetComponent<AudioSource>();
		GameObject soundList = new GameObject();
		soundList.name = "soundList";
		//create point sounds
		soundList.transform.SetParent(transform);
		int sourceLength = 20;
		pointSounds = new AudioSource[sourceLength];
		staticSounds = new AudioSource[5];
		for(int i = 0; i<sourceLength;i++)
		{
			GameObject obj = Instantiate(pointSound,transform.position,Quaternion.identity);
			pointSounds[i] = obj.GetComponent<AudioSource>();
			obj.transform.SetParent(soundList.transform);
		}
		for(int i = 0; i<5;i++)
		{
			GameObject obj = Instantiate(staticSound.gameObject,transform.position,Quaternion.identity);
			staticSounds[i] = obj.GetComponent<AudioSource>();
			obj.transform.SetParent(soundList.transform);
		}
		if(GameObject.Find("DataShare")!=null)
		{
			//if reached a checkpoint and sausages have been collected, update them on level restart.
			DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
			mode = DataS.mode;
			if(DataS.checkpointLevelProgress!="")
				currentLevelProgress = DataS.checkpointLevelProgress;
			else currentLevelProgress = DataS.levelProgress[DataS.lastLoadedLevel];
		}
		if(!inHub)
		{
			pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
			playerTransform = pScript.transform;
			pSprites = playerTransform.GetComponent<playerSprite>();
			pSprites.GetComponent<PlayerMusicBounce>().enabled = false;

			playerAxis = pSprites.transform.GetComponent<AxisSimulator>();
			SMeter = GameObject.Find("S_Meter").GetComponent<Image>();
			goalAnim = GameObject.Find("GoalText").GetComponent<Animator>();
			goalAnim.gameObject.SetActive(false);
			timeCount = GameObject.Find("timeCounter").GetComponent<TextMeshProUGUI>();
			healthBar = GameObject.Find("HUD_Canvas").transform.GetChild(10).gameObject;
			healthBar.SetActive(false);
			infiniteLives = DataS.infiniteLives == 1 ? true : false;
			createImpacts();
			if(mode==1)
			{
				updateEnemies();
			}

			if(parallaxBackgrounds[0]==null)
			{
				parallaxBackgrounds[0] = GameObject.Find("MainAreaParallax");
				parallaxBackgrounds[1] = GameObject.Find("SubAreaParallax");
				GameObject camBoundsObj = GameObject.Find("CameraBounds");
				for(int i = 0; i< camBoundsObj.transform.childCount;i++)
				{
					camBounds.Add(camBoundsObj.transform.GetChild(i).GetComponent<CameraBounds>());
				}
			}
			scoreIndicators = new List<GameObject>();
			cheeseSplatters = new List<GameObject>();
			Color cS = Color.white;
			GameObject o = GameObject.Find("InstantDeath");
			if(o!=null)
			{
				cS = o.GetComponent<Tilemap>().color;
			}
			for(int i = 0;i<5;i++)
			{
				GameObject obj = Instantiate(cheeseSplatter,transform.position,Quaternion.identity);
				if(cS!=Color.white)
				{
					var main = obj.GetComponent<ParticleSystem>().main;
					main.startColor = cS;
				}
				obj.SetActive(false);
				cheeseSplatters.Add(obj);
			}
			GameObject scoreObject = transform.GetChild(2).transform.GetChild(0).gameObject;
			scoreIndicators.Add(scoreObject);
			for(int i = 0; i<20;i++)
			{
				GameObject obj = Instantiate(scoreObject,scoreObject.transform.position,Quaternion.identity);
				obj.transform.SetParent(transform.GetChild(2));
				obj.SetActive(false);
				scoreIndicators.Add(obj);
			}

			for(int i = 0; i<SMeter.transform.childCount;i++)
			{
				SMeterArrows[i]=SMeter.transform.GetChild(i).GetComponent<Image>();
			}
			fireballs = new List<GameObject>();
			knives = new List<GameObject>();
			LKnives = new List<GameObject>();
			Color playerCol = playerTransform.GetChild(0).GetComponent<SpriteRenderer>().color;
			for(int i = 0; i<fireballCount;i++)
			{
				GameObject obj = Instantiate(fireball,Vector3.zero,Quaternion.identity);
				obj.transform.SetParent(transform);
				obj.SetActive(false);
				obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = playerCol;
				fireballs.Add(obj);

				obj = Instantiate(knifeproj,Vector3.zero,Quaternion.identity);
				obj.transform.SetParent(transform);
				obj.SetActive(false);
				obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = playerCol;
				knives.Add(obj);

				obj = Instantiate(lknifeproj,Vector3.zero,Quaternion.identity);
				obj.transform.SetParent(transform);
				obj.SetActive(false);
				obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = playerCol;
				LKnives.Add(obj);
			}
			if(map == null)
				map = GameObject.Find("MainMap").GetComponent<Tilemap>();
			if(ssmap==null)
				ssmap = GameObject.Find("SemiSolidMap").GetComponent<Tilemap>();
			if(invmap==null)
				invmap = GameObject.Find("InvBlockMap").GetComponent<Tilemap>();

			if(blockStore==null)
			blockStore = GameObject.Find("LevelGrid").GetComponent<hitBlockStore>();

			BoundsInt bounds = map.cellBounds;
			int Zpos = bounds.z;
			//Debug.Log(map.cellBounds);

			SausagesHold = GameObject.Find("SausagesHold");
			KeyHold = GameObject.Find("KeysHold");
			UIKeys = GameObject.Find("Keys").transform;
			if(KeyHold!=null&&KeyHold.transform.childCount!=0)
			{
				//Check which keys exist
				for(int i = 0; i< KeyHold.transform.childCount;i++)
				{
					int id = KeyHold.transform.GetChild(i).GetChild(0).GetComponent<keyScript>().ID;
					switch(id)
					{
						default: Debug.LogError("Invalid key ID"); break;
						case 0: redKey = true; break;
						case 1: blueKey = true; break;
						case 2: yellowKey = true; break;
					}
				}
				Color32[] keyColors = KeyHold.transform.GetChild(0).GetChild(0).GetComponent<keyScript>().colors;
				//Show keys on the UI
				int keyAmount = 0;
				if(redKey)keyAmount++;
				if(blueKey)keyAmount++;
				if(yellowKey)keyAmount++;
				//Destroy keys that dont exist
				for(int i = UIKeys.childCount-1;i>=keyAmount;i--)
				{
					Destroy(UIKeys.GetChild(i).gameObject);
				}
				//Color keys
				for(int c = 0; c<UIKeys.childCount;c++)
				{
					if(c==0)
					{
						if(redKey) UIKeys.transform.GetChild(c).GetComponent<Image>().color = keyColors[0];
						else if(!redKey&&blueKey) UIKeys.transform.GetChild(c).GetComponent<Image>().color = keyColors[1];
						else UIKeys.transform.GetChild(c).GetComponent<Image>().color = keyColors[2];
					}
					else if(c==1)
					{
						if(blueKey&&!yellowKey||blueKey&&yellowKey&&redKey) UIKeys.transform.GetChild(c).GetComponent<Image>().color = keyColors[1];
						else UIKeys.transform.GetChild(c).GetComponent<Image>().color = keyColors[2];
					}
					else UIKeys.transform.GetChild(c).GetComponent<Image>().color = keyColors[2];
				}
				if(!inHub)
				{
					if(DataS.hasred)updateKeys(0,false);if(DataS.hasblue)updateKeys(1,false);if(DataS.hasyellow)updateKeys(2,false);
				}
			}
			else
			{
				for(int i = UIKeys.childCount-1;i>=0;i--)
				{
					Destroy(UIKeys.GetChild(i).gameObject);
				}
			}

			UISausages = GameObject.Find("Sausages").transform;
			if(UISausages.childCount<2||!UISausages.GetChild(1).name.Contains("Image"))
			{
				if(UISausages.childCount!=0)
				{
					UISausages.GetChild(0).GetComponent<Image>().sprite = sausageMeterSprites[0];
					UISausages.GetChild(0).gameObject.SetActive(true);
					for(int i = 1; i<UISausages.childCount;i++)
					{
						Destroy(UISausages.GetChild(i).gameObject);
					}
				}
				if(SausagesHold.transform.childCount!=0)
				{
					//Debug.Log(SausagesHold.transform.childCount+" sausages");
					GameObject UISausage = UISausages.GetChild(0).gameObject;

					for(int s = 0; s<SausagesHold.transform.childCount;s++)
					{
						if(s!=0)
						{
							GameObject obj = Instantiate(UISausage,Vector3.zero,Quaternion.identity);
							obj.transform.SetParent(UISausages);
							obj.transform.localPosition = new Vector3(53.25f*s,0,0);
							obj.transform.localScale = Vector3.one;
							obj.name = "Sausage "+s;
						}
						if(DataS == null)
							UISausages.GetChild(s).GetComponent<Image>().sprite = sausageMeterSprites[0];
						else
						{
							if(s+2>=currentLevelProgress.Length)
							{
								currentLevelProgress = currentLevelProgress.Insert(currentLevelProgress.Length,"0");
								Debug.LogWarning("Warning: The default level values for sausages in Level "+DataS.lastLoadedLevel+" does not match with sausages placed in this level.\nCopy the CurrentLevelProgress string from the "+gameObject.name+" object, stop the scene and paste it inside the "+GameObject.FindGameObjectWithTag ("DataShare").name+" DataShare script in the \"Level Progress\" section at index "+DataS.lastLoadedLevel+" or change the \"LastLoadedLevel\" value to the id this level is supposed to have \"LevelProgress\" array");
							}
							//read the level progress of the sausages for the last loaded level which in a level is the same level.
							switch(currentLevelProgress[s+2])
							{
								default: UISausages.GetChild(s).GetComponent<Image>().sprite = sausageMeterSprites[0]; break;
								case '1':
									UISausages.GetChild(s).GetComponent<Image>().sprite = sausageMeterSprites[1];
									SausagesHold.transform.GetChild(s).GetComponent<SausageAdderScript>().turnIntoGhost();
									break;
							}
							//Debug.Log(currentLevelProgress[s+2]+" "+UISausages.GetChild(s).GetComponent<Image>().sprite.name);
						}
					}
				}
				else UISausages.GetChild(0).gameObject.SetActive(false);
			}
			//inside hub world
			else
			{
				sausagesCount = UISausages.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
				Destroy(UISausages.GetChild(0).gameObject);
				if(DataS.mode!=1)
				{
					DataS.spentSausages = 0;
					//calculate how much are spent
					if(DataS.AndreMissionProgress>=1)DataS.spentSausages+=15;
					if(DataS.DjoleMissionProgress>=1)DataS.spentSausages+=15;
					if(DataS.MiroslavMissionProgress>=1)DataS.spentSausages+=15;
				}
				updateSausageDisplay(0);
				homeLevel = true;
			}
		}
		if(mode==1&&(homeLevel||inHub))
		{
			GameObject g = GameObject.Find("Bus_BG");
			if(inHub)
			{
				g.SetActive(false); //turn off bus sprite
			}
			else
			{
				g.transform.position-=new Vector3(6,0,0);
				g.GetComponent<SpriteRenderer>().color = new Color(0.6681648f,0.7131764f,0.7264151f,1);
			}
		}
		coinCount = GameObject.Find("teapotsCounter").GetComponent<TextMeshProUGUI>();
		liveCount = GameObject.Find("livesCounter").GetComponent<TextMeshProUGUI>();
		scoreCount = GameObject.Find("Score").GetComponent<TextMeshProUGUI>();
		floppiesCount = GameObject.Find("floppiesCounter").GetComponent<TextMeshProUGUI>();
		Itembox = GameObject.Find("Item box").GetComponent<Animator>();
		storedItemSprite = GameObject.Find("Itembox_content").GetComponent<Image>();
		sources[1] = transform.GetChild(1).GetComponent<AudioSource>();
		if(mus!=null)
		{
			mus.GetFloat("MusicVolume",out musicMixerVolume);
		}
		sources[0] = transform.GetChild(0).GetComponent<AudioSource>();
		sources[1] = transform.GetChild(1).GetComponent<AudioSource>();
		musicVolume = sources[0].volume;
	}
	void Start()
	{
		if(DataS!=null)
		{
			floppies = DataS.floppies;
			lives = DataS.lives;
			if(DataS.checkpointFloppies==0||inHub) floppies = DataS.floppies;
			else floppies = DataS.checkpointFloppies;
			if(DataS.CheckpointScore==0) score = DataS.score;
			else score = DataS.CheckpointScore;

			cheated = DataS.checkpointCheat;
			if(!inHub)
			{
				if(homeLevel)
				{
					score = DataS.score;
					coins = DataS.coins;
				}
				else
				{
					//if(DataS.checkpointLevelProgress!="")
					coins = DataS.checkPointCoins;
					levelSausages = DataS.CheckpointSausages;
				}
				startInSub = DataS.startInSub;
				if(DataS.savedTimeClock!=0)timeClock = DataS.savedTimeClock; //transition betweens scenes that are the same level
				liveCount.SetText("X"+lives.ToString("00")+(DataS.infiniteLives==1?"*" : ""));
				coinCount.SetText("X"+coins.ToString("000"));
				floppiesCount.SetText("X"+floppies.ToString("00"));
				scoreCount.SetText(score.ToString("00000000"));

				if(DataS.checkPointTimeCounter!=0)
				timeClock = DataS.checkPointTimeCounter;
				if(DataS.parTime!=0)timer = DataS.parTime;
			}
			if(DataS.storedItem!=0 && DataS.storedItem<storedItemSprites.Length)
			{
				storedItemSprite.sprite = storedItemSprites[DataS.storedItem];
				storedItemSprite.SetNativeSize();
				storedItemID = DataS.storedItem;
			}
		}
		if(!inHub)
		{
			if(homeLevel)
			{
				score = DataS.score;
				coins = DataS.coins;
			}

			addCoin(0,false);
			addLives(0);
			addScore(0);
			addFloppy(0,false);
			addTime(0);
		}
		startTime = timer;
		inSub = startInSub;
		if(!startInSub)
		{
			if(muffleInMainArea && echoInMainArea)
				instantAudioEffectsToggle(3);
			else if(!muffleInMainArea && echoInMainArea)
				instantAudioEffectsToggle(2);
			else if(muffleInMainArea && !echoInMainArea)
				instantAudioEffectsToggle(1);
			else if(!muffleInMainArea && !echoInMainArea)
				instantAudioEffectsToggle(0);
			if(!inHub)
			{
				switchArea(false);
				switchParallax(false);
			}
		}
		else if(startInSub)
		{
			if(muffleInSubArea && echoInSubArea)
				instantAudioEffectsToggle(3);
			else if(!muffleInSubArea && echoInSubArea)
				instantAudioEffectsToggle(2);
			else if(muffleInSubArea && !echoInSubArea)
				instantAudioEffectsToggle(1);
			else if(!muffleInSubArea && !echoInSubArea)
				instantAudioEffectsToggle(0);
			if(!inHub)
			{
				switchArea(true);
				switchParallax(true);	
			}
		}
		if(DataS!=null)
		{
			Transform checkpointMain = null;
			if(!inHub)
			{
				checkpointMain = GameObject.Find("Checkpoints").transform;
				checkpointCount = checkpointMain.childCount;
			}
			if(dataShare.debug)
			print("Checkpoint count: "+checkpointCount);
			//if checkpoint value exists put player & camera at that checkpoint.
			if(DataS.checkpointValue!=0&&!inHub)
			{
				Transform checkpoint = checkpointMain.GetChild(DataS.checkpointValue-1);
				Animator a = checkpoint.GetComponent<Animator>();
				if(a.gameObject.activeInHierarchy)
				{
					a.SetBool("startFlipped",true);
					a.SetBool("flipped",true);
				}
				checkPointScript cP = checkpoint.GetComponent<checkPointScript>();
				cP.reached = true;

				if(cP.spawnPlayerAtCheckpoint)
				{
					GameObject.Find("Player_main").transform.position = checkpoint.position;
					if(cP.customCameraSpawn==Vector3.zero)
					cam.setPosition(new Vector3(checkpoint.position.x,checkpoint.position.y+cam.verticalOffset+cP.Yoffset,checkpoint.position.z));
					else cam.transform.position = cP.customCameraSpawn;
				}
			}
		}
		/*if(!inHub)
		{
			if(DataS!=null)
			Debug.Log("Checkpoint value: "+DataS.checkpointValue);
			Debug.Log("Start in Sub: "+startInSub);
		}*/
		sources[0].enabled = true;
		if(!inHub)
		{
			//load samples if exists
			if(DataS.savedTrackPoint!=0)
			{
				if(DataS.savedTrackName!=Intro.name)
				Intro=null;

				sources[0].timeSamples = DataS.savedTrackPoint;
			}

			if(Intro==null)
			{
				sources[0].clip = MainLoop;
				sources[0].Play();

				//print("Playing from sample: "+sources[0].timeSamples+" Saved: "+DataS.savedTrackName+" Name: "+DataS.savedTrackName);
			}
			else
			{
				introMusic = StartCoroutine(musicIntro(Intro,MainLoop));
			}
			//Debug.Log("Now playing: "+sources[0].clip);
		}


		if(!inHub)
		{
			PlayerMusicBounce p = pSprites.GetComponent<PlayerMusicBounce>(); 
			if(DataS.savedTrackPoint!=0)
			p.setStartDelay(DataS.savedTrackBeatDelay);

			p.enabled = true;
		}
		if(dataShare.randSounds)
		sounds = RandomizeSounds(sounds);

		finishedLoading = true;
		print("GameData finished loading");
	}
	public AudioClip[] RandomizeSounds(AudioClip[] randSounds)
	{
		for(int i = randSounds.Length-1;i>=0;i--)
		{
			int r = Random.Range(0,i);
			AudioClip tmp = randSounds[i];
			randSounds[i] = randSounds[r];
			randSounds[r] = tmp;
		}
		return randSounds;
	}
	public AudioClip getMusicClip()
	{
		return sources[0].clip;
	}
	void Update()
	{
		/*if(Input.GetKeyDown(KeyCode.M)&&mus!=null)
		{
			if(!musicOff)
			{
				musicOff = true;
				mus.SetFloat("MusicVolume",-80f);
			}
			else
			{
				musicOff = false;
				mus.SetFloat("MusicVolume",musicMixerVolume);
			}
		}*/
		if(!inHub)
		{
			if(Time.timeScale!=0)
			{
				if(redSwitchFrames>0)
				{
					if(!pScript.dead)
					{
						redSwitchFrames--;
						if(redSwitchFrames==204&&!reachedGoal)
						{
							//Debug.Log("switch about to end");
							playSoundStaticGetID(48);
						}
						if(redSwitchFrames==0)
						{
							S_Switch(false,0,true);
						}
					}
				}
			}
		if(Time.timeScale!=0)
		{
			if(fillSoundFrames>0)
			fillSoundFrames--;
			if(timeColorFrames>0)
			{
				if(timeColorFrames%5==0)
				{
					currentTimeColor++;
					if(currentTimeColor>=timeColors.Length)
					currentTimeColor = 0;
					timeCount.color = timeColors[currentTimeColor];
				}
				timeColorFrames--;
				if(timeColorFrames==0)
				timeCount.color = timeColors[0];
			}
			if(!timeFrozen)
			{
				if(timeFrames>0)
				timeFrames--;
				if(timeFrames==0)
				{
					timeFrames=60;
					timeClock++;
					if(timer>0 && timerGoesDown)
					addTime(-1);
				}
			}
		}
		if(timer>0&&timeToScore)
		{
			playTickSound();

			if(timer>=200)
			{
				addTime(-100);
				addScore(5000);
			}
			else if(timer>=20)
			{
				addTime(-10);
				addScore(500);
			}
			else
			{
				addTime(-1);
				addScore(50);
			}
		}
		if(sMeterWorks)
		{
			if(Mathf.Abs(playerAxis.axisPosX)>1.2f)
			xAbs = Mathf.Abs(playerAxis.axisPosX)-1;
			else xAbs = 0;
			litSMeterArrows = Mathf.FloorToInt(xAbs/0.15f);

			if(litSMeterArrows!=lastlitSMeterArrows)
			UpdateSMeterArrow();
		}
		if(SMeterFrames>0)
		{
			SMeterFrames--;

			if(SMeterFrames==0&&litSMeterArrows==8)
			{
				for(int i = 0; i<SMeterArrows.Length;i++)
				{
					if(paired)
					{
						if(i%2==0)
						{
						SMeterArrows[i].sprite = sMeterSprites[2];
						SMeter.sprite = sMeterSprites[6];
						}
						else
						{
						SMeterArrows[i].sprite = sMeterSprites[3];
						}
					}
					else
					{
						if(i%2==0)
						{
						SMeterArrows[i].sprite = sMeterSprites[3];
						}
						else
						{
						SMeterArrows[i].sprite = sMeterSprites[2];
						SMeter.sprite = sMeterSprites[5];
						}
					}
				}
				if(paired)paired = false;
				else paired = true;
				SMeterFrames = 8;
			}
		}
		//spawn box item
		if(SuperInput.GetKeyDown("Select")&&storedItemID!=0 && !reachedGoal&&Time.timeScale!=0&&!pScript.inCutscene)
		{
			string spawnedItemName;
			switch(storedItemID)
			{
				default: spawnedItemName = "Cola"; break;
				case 2:  spawnedItemName = "Pepper"; break;
				case 3:  spawnedItemName = "Axe"; break;
				case 4:  spawnedItemName = "Burek"; break;
				case 5:  spawnedItemName = "Knife"; break;
				case 6:  spawnedItemName = "LKnife"; break;
				case 7:  spawnedItemName = "GarlicCola"; break;
			}
			//print(storedItemID+" "+spawnedItemName);
			Itembox.SetTrigger("empty");
			playStaticSoundOverwrite(sounds[82]);
			if(DataS!=null)
			{
				DataS.storedItem = 0;
			}
			GameObject obj = Instantiate(boxItem,cam.transform.position+new Vector3(0,6,10),Quaternion.identity);
			obj.transform.GetChild(0).name = spawnedItemName;
			obj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = storedItemSprite.sprite;
			storedItemSprite.sprite = storedItemSprites[0];
			storedItemID = 0;
		}
		//speed up music
		if(musicSpeedsUpWhenLowTime && musicSpedUp && sources[0].pitch < 1.3f&&!reachedGoal&&!timeFrozen&&Time.timeScale!=0)
		{
			sources[0].pitch+=0.01f;
			if(sources[0].pitch>1.3f)
			sources[0].pitch = 1.3f;
			//sources[1].pitch = sources[0].pitch;
		}
		//slow down music
		if(musicSpeedsUpWhenLowTime && !musicSpedUp && sources[0].pitch > 1.0f &&!timeFrozen&&Time.timeScale!=0||timeFrozen && sources[0].pitch > 1.0f&&Time.timeScale!=0)
		{
			sources[0].pitch-=0.01f;
			if(sources[0].pitch<1.0f)
			sources[0].pitch = 1.0f;
			//sources[1].pitch = sources[0].pitch;
		}
		}
	}
	public void UpdateSMeterArrow()
	{
		lastlitSMeterArrows = litSMeterArrows;
		//raise arrows normally
		if(litSMeterArrows>0 && litSMeterArrows<=7)
		{
			SMeter.sprite = sMeterSprites[4];
			for(int i = 0; i<litSMeterArrows;i++)
				{
					SMeterArrows[i].sprite = sMeterSprites[1];
					if(i<6)
					{
						for(int z = i+1;z<SMeterArrows.Length;z++)
						SMeterArrows[z].sprite = sMeterSprites[0];
					}
				}
		}
		//if all arrows make an animation
		else if(litSMeterArrows==8 &&sMeterWorks)
		{
			SMeterFrames = 1;
		}
		//else all arows are blank
		else
		{
			SMeter.sprite = sMeterSprites[4];
			for(int i = 0; i<SMeterArrows.Length;i++)
				{
					SMeterArrows[i].sprite = sMeterSprites[0];
					SMeterFrames = 0;
				}
		}
	}
	public void givePowerup(int id)
	{
		if(givePowerupCor!=null)StopCoroutine(givePowerupCor);
		givePowerupCor = StartCoroutine(IGivePowerup(id));
	}
	Coroutine givePowerupCor;
	IEnumerator IGivePowerup(int id)
	{
		yield return new WaitUntil(()=>Time.timeScale!=0);
		id = Mathf.Clamp(id,0,powerups.Length-1);
		if(playerTransform==null)playerTransform = GameObject.Find("Player_main").transform;
		GameObject obj = Instantiate(powerups[id],playerTransform.position,Quaternion.identity);
		Transform tr = obj.transform.GetChild(0);
		tr.GetComponent<SpriteRenderer>().enabled = false;
		yield return 0;
		if(tr!=null)
		tr.gameObject.SetActive(true);

	}
	public IEnumerator coinBankCountDown(float time,TileBase t,Tilemap map,Vector3Int pos)
	{
		//Debug.Log(t.name +" at: "+pos+" activated successfully");
		yield return new WaitForSeconds(time);
		//if(map.GetTile(pos)!=null) Debug.Log(t.name +" at: "+pos+" ran out");
		//else Debug.Log(t.name +" at: "+pos+" has been destroyed.");
		if(map.GetTile(pos)!=null)
		{
			Color col = Color.white;
			if(map.GetTile(pos)!=null)col = map.GetColor(pos);
			yield return new WaitUntil(() => map.GetTile(pos)==null||t.name == map.GetTile(pos).name);
			switch(t.name)
			{
				default: map.SetTile(pos,replacementBlocks[0]); map.SetColor(pos,col); break;
				case "SlashBlock_Bank": map.SetTile(pos,replacementBlocks[1]); map.SetColor(pos,col); break;
			}
			if(bankPositions.Contains(pos))
			{
				bankPositions.Remove(pos);
			}
		}
	}
	public bool revealBlock(Vector2 point, Vector3 position, string name,bool invert)
	{
		//Debug.Log(Time.timeSinceLevelLoad+" "+point+" block reveal");
		bool doSomething = false;
		Vector3Int pos;
		if(!invert)
		{
			pos = new Vector3Int(Mathf.RoundToInt((point.x-0.5f)-(map.transform.position.x)),Mathf.RoundToInt((point.y)-(map.transform.position.y)),Mathf.RoundToInt(map.transform.position.z));
		}
		else
		{
			pos = new Vector3Int(Mathf.RoundToInt((point.x-0.5f)-(map.transform.position.x)),Mathf.RoundToInt((point.y-1f)-(map.transform.position.y)),Mathf.RoundToInt(map.transform.position.z));
		}
		Debug.DrawLine(position,pos,Color.green,3f);
		//Debug.Log("Testing 2: "+pos);
		if(invmap.GetTile(pos)!= null)
		{
			if(name=="Player_main")
			{
				//Debug.Log("Found hit by player");
				pScript.rb.velocity = new Vector2(pScript.rb.velocity.x,0);
				if(map.GetTile(pos)==null&&map.GetTile(pos-(Vector3Int.up*(invert? -1:1)))!=null)
				{
					//print("Hit tile on ground, cancel.");
					return false;
				}
			}
			//print(pos+" "+invmap.GetTile(pos).name);
			int Bstate = 0;
			int itemVal = -1;
			Transform playerPos = pSprites.transform;
			float Offset = 0.75f;
			if(pSprites.state >= 1)
				Offset = 1.25f;
			if(!invert)
			playerPos.position = new Vector3(playerPos.position.x,pos.y-Offset,playerPos.position.z);
			else playerPos.position = new Vector3(playerPos.position.x,pos.y+Offset+1f,playerPos.position.z);
			string tileName = invmap.GetTile(pos).name;
			invmap.SetTile(pos,null);
			if(tileName=="Inv_Bank")
			map.SetTile(pos,replacementBlocks[2]);
			switch(tileName)
				{
					default: doSomething = true; addCoin(1,true); Bstate = 2; break;
					case "Inv_C_Pepper":doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 1;Bstate = 3; break;
					case "Inv_C_Axe":   doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 4;Bstate = 3; break;
					case "Inv_C_Burek": doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 5;Bstate = 3; break;
					case "Inv_C_Knife": doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 7;Bstate = 3; break;
					case "Inv_C_LKnife":doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 8;Bstate = 3; break;

					case "Inv_Nothing": doSomething = true; Bstate = 11; break;
					case "Inv_Bank":    doSomething = true; addCoin(1,true); Bstate = 6; break;
					case "Inv_Cola":    doSomething = true; itemVal = 0; Bstate = 3; break;
					case "Inv_Pepper":  doSomething = true; itemVal = 1; Bstate = 3; break;
					case "Inv_Halva":   doSomething = true; itemVal = 2; Bstate = 3; break;
					case "Inv_1up":     doSomething = true; if(mode==1)itemVal = 11; else itemVal = 3; Bstate = 3; break;
					case "Inv_Axe":     doSomething = true; itemVal = 4; Bstate = 3; break;
					case "Inv_Burek":   doSomething = true; itemVal = 5; Bstate = 3; break;
					case "Inv_Knife":   doSomething = true; itemVal = 7; Bstate = 3; break;
					case "Inv_LKnife":  doSomething = true; itemVal = 8; Bstate = 3; break;

					case "Inv_Garlic":  doSomething = true; itemVal = 9; Bstate = 3; break;
				}

			
			if(doSomething)
			{
				sfxSource.PlayOneShot(sounds[1]);
				for(int i = 0; i < blockStore.amount; i++)
				{
					if(!blockStore.blocks[i].activeInHierarchy)
					{
						HitBlockScript h = blockStore.blocks[i].GetComponent<HitBlockScript>();
						if(itemVal >= 0 && itemVal < powerups.Length)
						h.storeItem(powerups[itemVal]);
						
						h.state = Bstate;
						blockStore.blocks[i].SetActive(true);
						h.activate(map,map.GetTile(pos),pos,invert);
						break;
					}
				}
			}
			return true;
		}
		else return false;
	}
	public void shopPowerup(int itemVal,Vector3Int pos)
	{
		sfxSource.PlayOneShot(sounds[1]);
		for(int i = 0; i < blockStore.amount; i++)
			{
				if(!blockStore.blocks[i].activeInHierarchy)
				{
					HitBlockScript h = blockStore.blocks[i].GetComponent<HitBlockScript>();
					if(itemVal >= 0 && itemVal < powerups.Length)
					h.storeItem(powerups[itemVal]);
						
					h.state = 12;
					blockStore.blocks[i].SetActive(true);
					h.activate(map,map.GetTile(pos),pos,false);
					break;
				}
			}
	}
	public string GetBlockName(Vector2 point)
	{
		Vector3Int pos = new Vector3Int(Mathf.RoundToInt(point.x-0.5f),Mathf.RoundToInt(point.y-0.6f),Mathf.RoundToInt(map.transform.position.z));

		if(map.GetTile(pos)!= null)
		{
			//print(pos+" "+map.GetTile(pos).name);
			return map.GetTile(pos).name;
		}
		else
		{
			//print(pos+" "+"null");
			return "null";
		}
	}
	public int testForBlock(Vector2 point,PlayerScript pScript,Tilemap usedmap)
	{
		Vector3Int pos = new Vector3Int(Mathf.RoundToInt(point.x-0.5f),Mathf.RoundToInt(point.y-0.6f),Mathf.RoundToInt(map.transform.position.z));
		//if(usedmap.GetTile(pos)==null) Debug.Log(usedmap.name+": "+pos + "null");
		if(usedmap.GetTile(pos)!= null)
		{
			string s = (usedmap.GetTile(pos).name);
			//Debug.Log(usedmap.name+": "+pos+" "+s);
			if(s=="Ham"||s=="test blocks_21")
			{
				//player effect
				//print("sliding physics");
				return 2;
			}
			else if(s=="blood_ground"||s=="test blocks_19")
			{
				//player effect
				//print("blood detect");
				return 3;
			}
			if(s=="cheeseBlock"&&!pScript.dead && !pScript.inCutscene)
			{
				//print("cheese hurt");
				pScript.Damage(true,false);
				return 0;
			}
			if(s=="blocks 1_9")
			{
				//print("skeletile");
				usedmap.SetTile(pos,replacementBlocks[5]);
				for(int i = 0;i<skeletileObjects.Count;i++)
				{
					if(!skeletileObjects[i].gameObject.activeInHierarchy)
					{
						skeletileObjects[i].Spawn(pos,0);
						break;
					}
				}
				return 0;
			}
			if(s=="test blocks_13")
			{
				usedmap.SetTile(pos,replacementBlocks[7]);
				return 0;
			}
			else return 0;
			//Debug.Log(map.GetTile(pos).name);
		}
		else return 1;
	}
	public string getBlockName(Vector2 point,Tilemap usedmap)
	{
		Vector3Int pos = new Vector3Int(Mathf.RoundToInt(point.x-0.5f),Mathf.RoundToInt(point.y-0.6f),Mathf.RoundToInt(map.transform.position.z));
		if(usedmap.GetTile(pos)==null) return "";
		else
		{
			string s = (usedmap.GetTile(pos).name);
			//Debug.Log(usedmap.name+": "+pos+" "+s);
			return s;
		}
	}
	public void spawnLipParticle(Vector3 point)
	{
		for(int i = 0;i<lipParticles.Count;i++)
		{
			if(!lipParticles[i].activeInHierarchy)
			{
				lipParticles[i].transform.position = point;
				lipParticles[i].SetActive(true);
				break;
			}
		}
	}
	public void removeTiles(Vector3Int[] points,Tilemap usedmap)
	{
		for(int i = 0; i< points.Length;i++)
		{
			if(usedmap.GetTile(points[i])!=null&&usedmap.GetTile(points[i]).name!="Inv_Pot")
			usedmap.SetTile(points[i],null);
		}
	}
	public bool explodeTile(Vector3 pos,bool withSound)
	{
		Vector3Int posInt = new Vector3Int(Mathf.RoundToInt((pos.x)-(map.transform.position.x)-0.5f),Mathf.RoundToInt((pos.y)-(map.transform.position.y)-0.5f),Mathf.RoundToInt(map.transform.position.z));
		//checks when there's an empty tile or an acceptable tile
		//print(posInt);
				bool free = false;
				//if tile is null, return free
				if(map.GetTile(posInt)!=null)
				for(int j = 0; j<explodableBlocks.Length;j++)
				{
					//print(map.GetTile(posInt).name);
					//print("not null");
					//if now, check if it's explodable from the list
					if(map.GetTile(posInt).name==explodableBlocks[j])
					{
						free = true;
						//print("found");
						//explode the tile
						for(int i = 0; i < blockStore.amount; i++)
						{
							if(!blockStore.blocks[i].activeInHierarchy)
							{
								HitBlockScript hsc = blockStore.blocks[i].GetComponent<HitBlockScript>();
								if(withSound)
								hsc.state = 8;
								else hsc.state = 9;
								blockStore.blocks[i].SetActive(true);
								hsc.activate(map,map.GetTile(posInt),posInt,false);
								break;
							}
						}
						//no free particles, break block
						map.SetTile(posInt,null);
						break;
					}
				}
				else free = true;
				//print(free);
				return free;
	}
	public void addVectorPoints(Vector2[] vList)
	{
		for(int i = 0; i< vList.Length;i++)
		{
			if(points==null)
			points = new List<Vector2>();

			points.Add(vList[i]);
		}
	}
	public void createExplosions()
	{
		points = points.Distinct().ToList();
		cam.shakeCamera(0.2f,0.5f);
		bool playBrickSound = false;
		for(int i = 0; i<points.Count;i++)
		{
			if(explodeTile(new Vector3(points[i].x,points[i].y,transform.position.z),false))
			{
				if(!playBrickSound)playBrickSound = true;
				GameObject obj;
				obj = Instantiate(smallExplosion,points[i],Quaternion.identity);
				obj.name="ScreenNuke";
				obj.SetActive(true);
			}
		}
		if(playBrickSound)
		{
			playSoundStatic(71);
			playSoundStatic(71);
		}
		points.Clear();
	}
	public void bigBlockHit(string name,bigBlockScript bigBlockSc)
	{
		bool doSomething = false;
		int itemVal = 0;
		switch(name.ToLower())
		{
			default:
			sfxSource.PlayOneShot(sounds[1]);
			break;
			case "big_block_burek":
			itemVal = 6;
			doSomething = true;
			break;
			case "big_block_axe":
			itemVal = 10;
			doSomething = true;
			break;
		}
		if(doSomething)
		{
			bigBlockSc.gameObject.name="used_big_block";
			cam.shakeCamera(0.05f,0.5f);
			sfxSource.PlayOneShot(sounds[1]);
			bigBlockSc.storeItem(powerups[itemVal]);
			bigBlockSc.activate();
		}
	}
	public bool blockHit(Vector2 point, Vector3 position, string name,bool invert)
	{
		bool doSomething = false;
		Vector3Int pos = new Vector3Int(Mathf.RoundToInt((point.x-0.5f)-(map.transform.position.x)),Mathf.RoundToInt((point.y)-(map.transform.position.y)),Mathf.RoundToInt(map.transform.position.z));
		Debug.DrawLine(position,pos,Color.magenta,3f);
		int Bstate = 0;
		int itemVal = -1;
		//Debug.Log("Testing "+pos);
		if(map.GetTile(pos)!= null)
		{
			//pScript.rb.velocity = new Vector2(pScript.rb.velocity.x,0);
			//Debug.Log(Time.timeSinceLevelLoad+" "+point+" block hit");
			//Debug.Log(map.GetTile(pos).name);
				switch(map.GetTile(pos).name)
				{
					default:
					sfxSource.PlayOneShot(sounds[1]);
					break;
					case "Brick":
					case "test blocks_7":
					case "test blocks_8":
					case "test blocks_9":
					doSomething = true;
					if(pSprites.state == 0 && name == "Player_main")
					Bstate = 0;
					else Bstate = 1;
					break;
					case "EventBlock":         doSomething = true; Bstate = 10; break;
					case "SlashBlock":         doSomething = true; addCoin(1,true); Bstate = 2; break;
					case "Brick_Pot":          doSomething = true; addCoin(1,true); Bstate = 2; break;
					case "Brick_Bank":         doSomething = true; addCoin(1,true); Bstate = 5; break;
					case "SlashBlock_Bank":    doSomething = true; addCoin(1,true); Bstate = 6; break;
					//Block state = 3 (hit slashblock)
					case "SlashBlock_Cola":
					case "7-6SlashBlockCola":
					doSomething = true; itemVal = 0; Bstate = 3; break;
					case "SlashBlock_Pepper":  doSomething = true; itemVal = 1; Bstate = 3; break;
					case "SlashBlock_Halva":   doSomething = true; itemVal = 2; Bstate = 3; break;
					case "SlashBlock_1up":     doSomething = true; if(mode==1)itemVal = 11; else itemVal = 3; Bstate = 3; break;
					case "SlashBlock_Axe":     doSomething = true; itemVal = 4; Bstate = 3; break;
					case "SlashBlock_Burek":   doSomething = true; itemVal = 5; Bstate = 3; break;
					case "SlashBlock_Knife":   doSomething = true; itemVal = 7; Bstate = 3; break;
					case "SlashBlock_LKnife":  doSomething = true; itemVal = 8; Bstate = 3; break;
					case "SlashBlock_Garlic":  doSomething = true; itemVal = 9; Bstate = 3; break;
					case "SlashBlock_Nothing":
					case "7-6SlashBlock":
					doSomething = true; Bstate = 11; break;
					//Block state = 3 (hit slashblock)
					case "SlashBlock_C_Pepper":doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 1;Bstate = 3; break;
					case "SlashBlock_C_Halva": doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 2;Bstate = 3; break;
					case "SlashBlock_C_Axe":   doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 4;Bstate = 3; break;
					case "SlashBlock_C_Burek": doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 5;Bstate = 3; break;
					case "SlashBlock_C_Knife": doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 7;Bstate = 3; break;
					case "SlashBlock_C_LKnife":doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 8;Bstate = 3; break;
					//Block state = 4 (hit brick)
					case "Brick_Nothing":      doSomething = true; Bstate = 11; break;
					case "Brick_Cola":         doSomething = true; itemVal = 0; Bstate = 4; break;
					case "Brick_Pepper":       doSomething = true; itemVal = 1; Bstate = 4; break;
					case "Brick_Halva":        doSomething = true; itemVal = 2; Bstate = 4; break;
					case "Brick_1up":          doSomething = true; if(mode==1)itemVal = 11; else itemVal = 3; Bstate = 4; break;
					case "Brick_Axe":          doSomething = true; itemVal = 4; Bstate = 4; break;
					case "Brick_Burek":        doSomething = true; itemVal = 5; Bstate = 4; break;
					case "Brick_Knife":        doSomething = true; itemVal = 7; Bstate = 4; break;
					case "Brick_LKnife":       doSomething = true; itemVal = 8; Bstate = 4; break;
					case "Brick_Garlic":       doSomething = true; itemVal = 9; Bstate = 4; break;
					
					case "Brick_Bumbo":		   doSomething = true;
					if(pSprites.state == 0 && name == "Player_main") Bstate = 0;
					else Bstate = 7;
					break;
					//Block state = 4 (hit brick)
					case "Brick_C_Pepper":     doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 1;Bstate = 4; break;
					case "Brick_C_Halva":      doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 2;Bstate = 4; break;
					case "Brick_C_Axe":        doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 4;Bstate = 4; break;
					case "Brick_C_Burek":      doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 5;Bstate = 4; break;
					case "Brick_C_Knife":      doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 7;Bstate = 4; break;
					case "Brick_C_LKnife":     doSomething = true; if(pSprites.state == 0) itemVal = 0; else itemVal = 8;Bstate = 4; break;
					case "BoomStatic": spawnBoomStatic(point); if(scanForGoalEvent)reachedGoal = true; break;
				}
			if(doSomething)
			{
				sfxSource.PlayOneShot(sounds[1]);
				for(int i = 0; i < blockStore.amount; i++)
				{
					if(!blockStore.blocks[i].activeInHierarchy)
					{
						HitBlockScript h = blockStore.blocks[i].GetComponent<HitBlockScript>();
						if(itemVal >= 0 && itemVal < powerups.Length)
						h.storeItem(powerups[itemVal]);
						
						h.state = Bstate;
						blockStore.blocks[i].SetActive(true);
						h.activate(map,map.GetTile(pos),pos,invert);
						break;
					}
				}
			}
			return true;
		}
		else
		{
			if(!invert)
				return revealBlock(point,position,name,invert);
			else
			{
				//print("Try reveal");
				return revealBlock(point+(Vector2.up),position,name,invert);
			}
		}
	}
	public void spawnBoomStatic(Vector3 pos)
	{
		Vector3Int posInt = new Vector3Int(Mathf.RoundToInt((pos.x)-(map.transform.position.x)-0.5f),Mathf.RoundToInt((pos.y)-(map.transform.position.y)-0.5f),Mathf.RoundToInt(map.transform.position.z));
		//check in 4 directions for spread
		playSoundOverWrite(53,pos);
		map.SetTile(posInt,null);
		cam.shakeCamera(0.2f,0.2f);
		checkStaticBoom(posInt+Vector3Int.up);
		checkStaticBoom(posInt-Vector3Int.up);
		checkStaticBoom(posInt+Vector3Int.right);
		checkStaticBoom(posInt-Vector3Int.right);
	}
	void checkStaticBoom(Vector3Int pos)
	{
		TileBase t = map.GetTile(pos);
		if(t!=null&&t.name=="BoomStatic")
		{
			for(int i = 0;i<boomStaticObjects.Count;i++)
			{
				if(!boomStaticObjects[i].gameObject.activeInHierarchy)
				{
					boomStaticObjects[i].transform.position = pos+(new Vector3(0.5f,0.5f));
					boomStaticObjects[i].gameObject.SetActive(true);
					map.SetTile(pos,null);
					break;
				}
			}
		}
	}
	public void healthBarEnable(int itemID,bool withAnimation)
	{
		currentHealth = 3;
		healthBar.GetComponent<Image>().sprite=eternalItem[Mathf.Clamp(itemID,0,eternalItem.Length-1)];
		for(int i =0; i<healthBar.transform.childCount;i++)
		{
			healthBar.transform.GetChild(i).gameObject.SetActive(true);
		}
		healthBar.SetActive(true);
		if(withAnimation)
		healthBar.GetComponent<Animator>().SetTrigger("activate");
	}
	public void healthBarDisable()
	{
		healthBar.SetActive(false);
	}
	public void health(int value)
	{
		if(value>0)
		playSoundStatic(44);
		else playSoundStatic(43);
		if(currentHealth+value<=0)
		killPlayer();

		currentHealth = Mathf.Clamp(currentHealth+value,0,3);
		for(int i =0; i<healthBar.transform.childCount;i++)
		{
			healthBar.transform.GetChild(i).gameObject.SetActive(false);
		}
		if(currentHealth>0)
		for(int i=0; i<currentHealth;i++)
		{
			healthBar.transform.GetChild(i).gameObject.SetActive(true);
		}

	}
	public void nullCoin()
	{
		coins = 0;
		coinCount.SetText("X"+coins.ToString("000"));
		if(inHub)DataS.coins = coins;
	}
	public void nullFloppy()
	{
		floppies = 0;
		floppiesCount.SetText("X"+floppies.ToString("00"));
		DataS.floppies = floppies;
	}
	public void nullLives()
	{
		lives = 0;
		liveCount.SetText("X"+lives.ToString("00")+(DataS.infiniteLives==1?"*" : ""));
		saveLives();
	}
	public void saveScore()
	{
		DataS.score = score;
	}
	public void nullScore()
	{
		score=0;
		scoreCount.SetText(score.ToString("00000000"));
		saveScore();
	}
	public void addCoin(int amount,bool withScore)
	{
		if(coins<999||amount<=0)
		{
			//int coinsDoubleDig = coins%100;
			if(!homeLevel&&!inHub)
			coins = Mathf.Clamp(coins+amount,0,500);
			else coins = Mathf.Clamp(coins+amount,0,999);

			if(inHub)DataS.coins = coins;
			/*if(coinsDoubleDig+amount >= 100&&coins<999)
			{
				addLives(1);
			}*/
			if(amount>0&&withScore)
			addScore(10*amount);
			coinCount.SetText("X"+coins.ToString("000"));
		}
	}
	public void setPoint(int val)
    {
        val = Mathf.Clamp(val,0,checkpointCount);
		if(val==0)DataS.resetCheckData();
		else
		{
			DataS.checkpointCheat = true;
			GameObject o = GameObject.Find("Checkpoints");
			if(o!=null)
			{
				o.transform.GetChild(val-1).GetComponent<checkPointScript>().reach(null);
			}
			else return;
		}
        DataS.loadSceneWithoutLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
	public void saveCoin()
	{
		DataS.coins = coins;
	}
	public void addFloppy(int amount,bool giveScore)
	{
		floppies = Mathf.Clamp(floppies+amount,0,99);
		floppiesCount.SetText("X"+floppies.ToString("00"));
		if(amount<0||(amount>0&&!giveScore))
		DataS.floppies = floppies;
		if(amount>0)
		{
			if(giveScore)
			{
				addScore(1000);
				bool write = true;
				//write floppy data after the second ;, but only if the next character isn't already a C.
				for (int i = 2;i<currentLevelProgress.Length;i++)
				{
					if(currentLevelProgress[i]==';'&&write)
					{
						write = false;
						if(currentLevelProgress[i+1]!='C')
						{
							//print("write C");
							if(currentLevelProgress[i+2]==';')
							currentLevelProgress= currentLevelProgress.Insert(i+1,"C");
							else currentLevelProgress= currentLevelProgress.Insert(i+1,"C;");
						}
					}
				}
				//No ; detected when reading, assuming clean file.
				if(write)
				currentLevelProgress = currentLevelProgress.Insert(currentLevelProgress.Length,";C");
			}
		}
	}
	public void saveFloppies()
	{
		DataS.floppies = floppies;
	}
	public void addLives(int amount)
	{
		if(amount>0)
			playLifeSound();
		lives = Mathf.Clamp(lives+amount,0,99);
		liveCount.SetText("X"+lives.ToString("00")+(DataS.infiniteLives==1?"*" : ""));
	}
	public void saveLives()
	{
		DataS.lives = lives;
	}
	public bool updateSausageDisplay(int amount)
	{
		bool b = false;
		if((DataS.sausages-DataS.spentSausages)>=Mathf.Abs(amount))
		{
			DataS.spentSausages-=amount;
			b = true;
		}
		sausagesCount.text = "X"+(Mathf.Clamp(DataS.sausages-DataS.spentSausages,0,10000)).ToString("00");
		return b;
	}
	public void addLivesSilent(int amount)
	{
		lives = Mathf.Clamp(lives+amount,0,99);
		liveCount.SetText("X"+lives.ToString("00")+(DataS.infiniteLives==1?"*" : ""));
	}
	void playLifeSound()
	{
		if(sfxSource.isPlaying)
			sfxSource.Stop();
		sfxSource.clip = sounds[17];
		sfxSource.Play();
	}
	public void playTickSound()
	{
		if(tickInt==0)
		{
			tickInt=4;
			if(sfxSource.isPlaying)
				sfxSource.Stop();
			sfxSource.clip = sounds[27];
			sfxSource.Play();
		}
		else
		{
			tickInt--;
		}
	}
	public void addScore(long amount)
	{
		if(score<99999999)
		score+=amount;
		if(score>99999999)
		score=99999999;
		scoreCount.SetText(score.ToString("00000000"));
	}
	public void streakScore(int usedStreak,Vector3 pos,bool Sound)
	{
		if(infiniteLives)usedStreak = Mathf.Clamp(usedStreak,0,7);
		switch(usedStreak)
		{
			default:
				addLives(1);

				if(mode!=1)
					ScorePopUp(pos,"1up",new Color32(133,251,124,255));
				else ScorePopUp(pos,"ALT1up",new Color32(133,251,124,255));
			break;
			case 1: addScore(200);if(Sound){playComboSound(0);} ScorePopUp(pos,"+200",new Color32(255,255,255,255));break;
			case 2: addScore(400);if(Sound){playComboSound(1);} ScorePopUp(pos,"+400",new Color32(255,255,255,255));break;
			case 3: addScore(800);if(Sound){playComboSound(2);} ScorePopUp(pos,"+800",new Color32(255,255,255,255));break;
			case 4: addScore(1000);if(Sound){playComboSound(3);} ScorePopUp(pos,"+1000",new Color32(255,255,255,255));break;
			case 5: addScore(2000);if(Sound){playComboSound(4);} ScorePopUp(pos,"+2000",new Color32(255,255,255,255));break;
			case 6: addScore(4000);if(Sound){playComboSound(5);} ScorePopUp(pos,"+4000",new Color32(255,255,255,255));break;
			case 7: addScore(8000);if(Sound){playComboSound(6);} ScorePopUp(pos,"+8000",new Color32(255,255,255,255));break;
		}
	}
	public void killPlayer()
	{
		pSprites.state=0;
		pSprites.eternal = false;
		pSprites.GetComponent<PlayerScript>().eternal = false;
		pSprites.gameObject.GetComponent<PlayerScript>().Die();
	}
	IEnumerator timeUpMethod()
	{
		stopAllMusic();
		Time.timeScale = 0;
		litSMeterArrows = 0;
		sMeterWorks = false;
		GameObject obj = new GameObject("timeUp");
		SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
		rend.sprite = timeUp;
		rend.sortingLayerName = "UI";
		rend.sortingOrder = 13;
		obj.transform.parent = cam.transform;
		obj.transform.localPosition = new Vector3(0,3,1);
		int i = 180;
		while(i>0)
		{
			yield return 0;
			i--;
		}
		pScript.StartCoroutine(pScript.timeUpDeath());
		//yield return new WaitUntil(()=>cam.fadeAnim!=0);
		//obj.SetActive(false);

	}
	public void addTime(int amount)
	{
		if(timer+amount==0 && !reachedGoal&&amount<0)
		{
			if(timerGoesDown)
			{
				timerGoesDown = false;
				StartCoroutine(timeUpMethod());
			}
		}
		if(!reachedGoal&&amount>0)
		{
			timeColorFrames = 60;
			if(sfxSource.isPlaying)
				sfxSource.Stop();
			sfxSource.clip = sounds[38];
			sfxSource.Play();
		}
		if(timer+amount<999)
		timer+=amount;
		else timer = 999;
		if(timer<=60 && !musicSpedUp&&timer!=0)
			musicSpedUp = true;
		if(timer<=5&&timer!=0&&!reachedGoal)
		playSoundStatic(92);
		if(timer>60 && musicSpedUp)
			musicSpedUp = false;
		timeCount.SetText(timer.ToString("000"));
	}
	public void collectSausage(GameObject sausage)
	{
		int id = 0;
		levelSausages++;
		for(int i = 0; i<UISausages.childCount; i++)
		{
			if(SausagesHold.transform.GetChild(i).gameObject == sausage)
			{
				id = i;
				break;
			}
		}
		UISausages.GetChild(id).GetComponent<Image>().sprite = sausageMeterSprites[1];
		if(currentLevelProgress!="")
		{
			currentLevelProgress = currentLevelProgress.Remove(id+2,1);
			currentLevelProgress = currentLevelProgress.Insert(id+2,"1");
		}
	}
	public void storeItem(int itemID,bool silent)
	{
		if(itemID<storedItemSprites.Length)
		{
			storedItemSprite.sprite = storedItemSprites[itemID];
			storedItemSprite.SetNativeSize();
			storedItemID = itemID;
			if(!silent)
			{
				Itembox.SetTrigger("fill");
				if(fillSoundFrames<=0)
				{
				playSoundStatic(81);
				fillSoundFrames = 2;
				}
			}
		}

	}
	public void S_Switch(bool press,int SwitchColor,bool withMusic)
	{
		if(blockStore==null)blockStore = GameObject.Find("LevelGrid").GetComponent<hitBlockStore>();
		if(SwitchColor==0)
		{
			switchMusic = press;
			if(press)		
			cam.shakeCamera(0.05f,0.5f);
			for(int i = 0; i<blockStore.redSwitchLines.Count;i++)
			{
				if(press)
				map.SetTile(blockStore.redSwitchLines[i],switchTiles[1]);
				else map.SetTile(blockStore.redSwitchLines[i],switchTiles[0]);
			}
			for(int i = 0; i<blockStore.redSwitchBlocks.Count;i++)
			{
				if(press)
				map.SetTile(blockStore.redSwitchBlocks[i],switchTiles[0]);
				else map.SetTile(blockStore.redSwitchBlocks[i],switchTiles[1]);
			}
		}
		if(press)
		{
			redSwitchFrames = 960;
			if(sPitch!=null)StopCoroutine(sPitch);
			sPitch = StartCoroutine(switchPitch());
			if(!pScript.dead&&withMusic)
			{
				if(!halvaMusic)
				changeMusic(false,45,false,true,0.35f);
				else changeMusic(false,47,false,true,0.35f);
			}
		}
		else 
		{
			if(!halvaMusic)
				{
					//print(reachedGoal);
					if(!reachedGoal)
					changeMusic(true,0,true,false,0);
					else stopMusic(true,false);
				}
				else
				{
					if(mode!=1)
					changeMusic(false,9,true,true,0.35f);
					changeMusic(false,120,true,true,0.7f);
				}
		}
	}
	public void unlockBlock(int ID)
	{
		// 0 - red, 1 - blue, 2 = yellow.
		switch(ID)
		{
			default:
			for(int i = 0; i<blockStore.redLocks.Count;i++)
				map.SetTile(blockStore.redLocks[i],null);
			break;
			case 1:
			for(int i = 0; i<blockStore.blueLocks.Count;i++)
				map.SetTile(blockStore.blueLocks[i],null);
			break;
			case 2:
			for(int i = 0; i<blockStore.yellowLocks.Count;i++)
				map.SetTile(blockStore.yellowLocks[i],null);
			break;
		}
	}
	public void changeMusic(bool isDefault,int soundID,bool isLooping,bool normalPitch,float altVolume)
	{
		if(sources[0]!=null&&sources[1]!=null)
		{
		//Debug.Log("changed music");
		if(isDefault)
		{
			sources[0].volume = musicVolume;
			sources[1].Stop();
			sources[1].loop = false;
			sources[1].enabled = false;
			if(normalPitch)
			{
				if(sPitch!=null)StopCoroutine(sPitch);
				sources[0].pitch = 1f;
				sources[1].pitch = 1f;
			}
			if(sources[0].enabled)
			sources[0].Play();
			restoreAudioEffects();
		}
		else
		{
			suspendAudioEffects();
			if(normalPitch)
			{
				if(sPitch!=null&&sources[1].pitch!=1f)StopCoroutine(sPitch);
				sources[0].pitch = 1f;
				sources[1].pitch = 1f;
			}
			sources[0].volume = 0f;
			sources[1].volume = altVolume;
			sources[1].loop = isLooping;
			sources[1].clip = sounds[soundID];
			sources[1].enabled = true;
			sources[1].Play();
		}
		}
	}
	public void setCustomMusic(AudioClip newMusic)
	{
		if(sPitch!=null)StopCoroutine(sPitch);
		if(introMusic!=null)StopCoroutine(introMusic);
		sources[0].volume = musicVolume;
		sources[0].Stop();
		sources[0].clip = newMusic;
		sources[0].loop = true;
		sources[0].Play();
	}
	IEnumerator switchPitch()
	{
		yield return new WaitUntil(()=>redSwitchFrames<=360);
		while(sources[1].pitch<=1.5f)
		{
			if(Time.timeScale!=0)
			sources[1].pitch+=0.001f;
			yield return 0;
		}
		yield return new WaitUntil(()=>redSwitchFrames<=0);
		sources[1].pitch=1f;
	}
	public void changeMusicInSubArea(AudioClip newMusic,int BPM)
	{
		if(introMusic!=null)
		StopCoroutine(introMusic);
		sources[0].volume = musicVolume;
		sources[0].Stop();
		sources[0].loop = true;
		sources[0].clip = newMusic;
		if(!sources[1].isPlaying) sources[0].Play();
		PlayerMusicBounce pMusic = pSprites.GetComponent<PlayerMusicBounce>();
		pMusic.enabled = false;
		pMusic.BPM = BPM;
		pMusic.enabled = true;
	}
	public void changeMusicWithIntro(AudioClip newintro, AudioClip newloop,int BPM)
	{
		if(introMusic!=null)
		StopCoroutine(introMusic);
		PlayerMusicBounce pMusic = pSprites.GetComponent<PlayerMusicBounce>();
		pMusic.enabled = false;
		pMusic.BPM = BPM;
		pMusic.enabled = true;
		//print("Changing music with intro: "+newintro.name+" "+newintro);
		//check if other music is playing, if not play this instead.
		if(!sources[1].isPlaying)
		{
			introMusic = StartCoroutine(musicIntro(newintro,newloop));
		}

	}
	public void loadNextLevelScene(int ID)
	{
		DataS.playerState = pSprites.state;
		DataS.storedItem = storedItemID;
		DataS.savedTimeClock = timeClock;
		DataS.loadSceneWithoutLoadScreen(ID);
	}
	IEnumerator fadeMusicCor(bool fadeIn)
	{
		//print("Fading music");
		float TargetVolume = 0,curVolume = sources[0].volume,progress = 0;
		if(fadeIn)TargetVolume = musicVolume;

		while(progress<=1)
		{
			progress+=Time.unscaledDeltaTime*2f;
			sources[0].volume = Mathf.Lerp(curVolume,TargetVolume,progress);
			//print("Fade progress: "+progress+" Volume: "+sources[0].volume);
			yield return 0;
		}
		//print("Fading music over");
	}
	public void fadeMusic(bool fadeIn)
	{
		if(musicFade!=null)StopCoroutine(musicFade);
		musicFade = StartCoroutine(fadeMusicCor(fadeIn));
	}
	public void pauseMusic(bool isPaused)
	{
		if(isPaused)
		{
			//print("music paused");
			if(!sources[1].enabled)
			{
				sources[0].Pause();
			}
			if(sources[1].enabled)
			sources[1].Pause();
		}
		else
		{
			if(!sources[1].enabled)
			sources[0].Play(0);
			if(sources[1].enabled)
			sources[1].Play(0);
		}
	}
	public void stopMusic(bool stop,bool reset)
	{
		if(!sources[0].enabled)
			sources[0].enabled = true;
		//Debug.Log("Stop music called");
		if(stop && !reset)
		{
		sources[0].volume = 0f;
		}
		if(stop && reset)
		{
			sources[0].volume = 0f;
			sources[0].Stop();
		}
		if(!stop && reset)
		{
			sources[0].Stop();
			sources[0].volume = musicVolume;
			sources[0].Play();
		}
		else if(!stop&&!reset)
			sources[0].volume = musicVolume;
	}
	public void stopAllMusic()
	{
		if(introMusic!=null)
		StopCoroutine(introMusic);
		//Debug.Log("Stop all music called");
		if(saveMusicSamples)
		{
			//print("Saving samples");
			DataS.savedTrackBeatDelay = pSprites.GetComponent<PlayerMusicBounce>().getDelay();
			DataS.savedTrackName = sources[0].clip.name;
			DataS.savedTrackPoint = sources[0].timeSamples;
		}
		sources[0].Stop();
		sources[1].Stop();
		sources[0].clip = null;
		sources[1].clip = null;
	}
	public void playSound(int ID, Vector3 point)
	{
		for(int i = 0; i<pointSounds.Length;i++)
		{
			if(!pointSounds[i].isPlaying)
			{
				//print(i);
				pointSounds[i].transform.position = point;
				pointSounds[i].clip = sounds[ID];
				pointSounds[i].Play();
				break;
			}
		}
	}
	public void playSoundOverWrite(int ID, Vector3 point)
	{
		for(int i = 0; i<pointSounds.Length;i++)
		{
			if(!pointSounds[i].isPlaying||pointSounds[i].clip==sounds[ID])
			{
				//print(i);
				if(pointSounds[i].isPlaying)pointSounds[i].Stop();
				pointSounds[i].transform.position = point;
				pointSounds[i].clip = sounds[ID];
				pointSounds[i].Play();
				break;
			}
		}
	}
	public void playStaticSoundOverwrite(AudioClip clip)
	{
		if(sfxSource.isPlaying)
			sfxSource.Stop();
		sfxSource.clip = clip;
		sfxSource.Play();
	}
	public bool checkIfIDPlaying(int ID)
	{
		for(int i = 0; i<pointSounds.Length;i++)
		{
			if(pointSounds[i].isPlaying)
			{
				if(pointSounds[i].clip == sounds[ID])
				{
					return true;
				}
			}
		}
		return false;
	}
	public int playSoundStaticGetID(int ID)
	{
		for(int i = 0; i<staticSounds.Length;i++)
		{
			if(!staticSounds[i].isPlaying)
			{
				staticSounds[i].clip = sounds[ID];
				staticSounds[i].Play();
				return i;
			}
		}
		return 0;
	}
	public void stopSoundStatic(int ID)
	{
		if(staticSounds[ID].isPlaying)
		{
			staticSounds[ID].Stop();
		}
	}
	public void stopSoundStaticType(int ID)
	{
		for(int i = 0; i<staticSounds.Length;i++)
		{
			if(staticSounds[i].isPlaying)
			{
				if(staticSounds[i].clip == sounds[ID])
				{
					staticSounds[i].Stop();
				}
			}
		}
	}
	public void playSoundStatic(int ID)
	{
		sfxSource.PlayOneShot(sounds[ID]);
	}
	public void playSoundPitched(int ID,float pitch)
	{
		if(eneSounds.isPlaying)
		eneSounds.Stop();

		eneSounds.clip = sounds[ID];
		eneSounds.pitch = pitch;
		eneSounds.Play();
	}
	public void playeneSound(AudioClip clip,float pitch)
	{
		pitch = Mathf.Clamp(pitch,0,1.4f);
		//Debug.Log(pitch);
		if(eneSounds.isPlaying)
		eneSounds.Stop();

		eneSounds.clip = clip;
		eneSounds.pitch = pitch;
		eneSounds.Play();
	}
	public void playUnlistedSound(AudioClip clip)
	{
		sfxSource.PlayOneShot(clip);
	}
	public void playUnlistedSoundPoint(AudioClip clip, Vector3 point)
	{
		for(int i = 0; i<pointSounds.Length;i++)
		{
			if(!pointSounds[i].isPlaying)
			{
				pointSounds[i].transform.position = point;
				pointSounds[i].clip = clip;
				pointSounds[i].Play();
				break;
			}
		}
	}
	public void playComboSound(int ID)
	{
		sfxSource.PlayOneShot(Combosounds[ID]);
	}
	public IEnumerator goalAnimate(int value,float timeWait)
	{
		stopSoundStaticType(48);
		timeFrozen = true;
		yield return new WaitForSeconds(timeWait);
		goalAnim.gameObject.SetActive(true);
		goalAnim.SetInteger("value",value);
	}
	public void audioEffectsToggle(int value)
	{
		/* 0 - no effects
		   1 - music muffle
		   2 - sound echo
		   3 - muffle & echo */
		if(value == 1 && usedSnapshot == 2 || value == 2 && usedSnapshot == 1)
		{
			value = 3;
		}
		snapshots[value].TransitionTo(0.25f);
	}
	public void instantAudioEffectsToggle(int value)
	{
		/* 0 - no effects
		   1 - music muffle
		   2 - sound echo
		   3 - muffle & echo */
		if(value == 1 && usedSnapshot == 2 || value == 2 && usedSnapshot == 1)
		{
			value = 3;
		}
		snapshots[value].TransitionTo(0f);
	}
	public void restoreAudioEffects()
	{
		int eff = 0;
		if(inSub)
		{
			if(muffleInSubArea)eff+=1;
			if(echoInSubArea)eff+=2;
		}
		else
		{
			if(muffleInMainArea)eff+=1;
			if(echoInMainArea)eff+=2;
		}
		instantAudioEffectsToggle(eff);
	}
	public void suspendAudioEffects()
	{
		int eff = 2; //assume echo is on
		if(inSub)
		{
			//if(muffleInSubArea)eff-=1; //remove muffle
			if(!echoInSubArea)eff-=2; //if no echo, remove echo effect, otherwise keep
		}
		else
		{
			//if(muffleInMainArea)eff-=1;
			if(!echoInMainArea)eff-=2;
		}
		instantAudioEffectsToggle(eff);
	}
	public void switchArea(bool isSub)
	{
		if(parallaxBackgrounds[0] ==null ||camBounds.Count==0)
		{
			GameObject camBoundsObj = GameObject.Find("CameraBounds");
			for(int i = 0; i< camBoundsObj.transform.childCount;i++)
			{
				camBounds.Add(camBoundsObj.transform.GetChild(i).GetComponent<CameraBounds>());
			}
		}
		if(isSub)
		{
			//print("Switched bounds to Sub.");
			camBounds[0].used = false;
			camBounds[0].enabled = false;
			camBounds[1].used = true;
			camBounds[1].enabled = true;
			areaSkies[0].SetActive(false);
			areaSkies[1].SetActive(true);

			cam.usedBounds = camBounds[1].GetComponent<Collider2D>();
		}
		else
		{
			//print("Switched bounds to Main.");
			camBounds[1].used = false;
			camBounds[1].enabled = false;
			camBounds[0].used = true;
			camBounds[0].enabled = true;
			areaSkies[1].SetActive(false);
			areaSkies[0].SetActive(true);

			cam.usedBounds = camBounds[0].GetComponent<Collider2D>();
		}
	}
	public void switchParallax(bool isSub)
	{
		if(parallaxBackgrounds[0] ==null ||camBounds.Count==0)
		{
			parallaxBackgrounds[0] = GameObject.Find("MainAreaParallax");
			parallaxBackgrounds[1] = GameObject.Find("SubAreaParallax");
		}
		if(isSub)
		{
		if(parallaxBackgrounds[0]!=null)
		parallaxBackgrounds[0].SetActive(false);
		if(parallaxBackgrounds[1]!=null)
		parallaxBackgrounds[1].SetActive(true);
		}
		else
		{
		if(parallaxBackgrounds[1]!=null)
		parallaxBackgrounds[1].SetActive(false);
		if(parallaxBackgrounds[0]!=null)
		parallaxBackgrounds[0].SetActive(true);
		}
	}
	public void ScorePopUp(Vector3 pos,string text,Color32 color)
	{
		if(text!="+0")
		for(int i = 0; i<scoreIndicators.Count;i++)
		{
			if(!scoreIndicators[i].activeInHierarchy)
			{
				TextMeshPro tm = scoreIndicators[i].transform.GetChild(0).GetComponent<TextMeshPro>();
				tm.color = color;
				if(text=="ALT1up")
				{
					scoreIndicators[i].transform.GetChild(1).gameObject.SetActive(true);
					tm.text = "";
				}
				else
				{
					tm.text = text;
				}
				scoreIndicators[i].transform.position = new Vector3(pos.x, pos.y+0.5f,pos.z);
				scoreIndicators[i].SetActive(true);
				return;
			}
		}
	}
	public void saveLevelProgress(bool newLevelClear,bool showClearedAnim)
	{
		
		DataS.leavingLevel = true;
		if(newLevelClear)
		{
			if(showClearedAnim)
			{
				DataS.clearedUnbeatenLevel = true;
			}
			else DataS.leavingLevel = false;
			dataShare.totalCompletedLevels++;
		}
		if(cheated)timeClock = 4884;
		//print("DataSave TimeClock: "+timeClock);
		//read time if exists, or if there's 1 semi colon, write to the end;
		int semicolonCount = 0,timerStartpoint = 0,oldTime = 0;
		string oldTimeString = "";
		bool containsFloppyData = false;
		for (int i = 0;i<currentLevelProgress.Length;i++)
		{
			if(semicolonCount>=1&&!containsFloppyData)
			{
				if(i<currentLevelProgress.Length-1
				&&currentLevelProgress[i+1]=='C'
				&&!containsFloppyData)
				{
					containsFloppyData = true;
					//print("Floppy data detected");
				}
			}
			if(semicolonCount==2&&!containsFloppyData)
			{
				//print("test2");
				oldTimeString+=currentLevelProgress[i];
			}
			else if(semicolonCount==3&&containsFloppyData)
			{
				//print("test3");
				oldTimeString+=currentLevelProgress[i];
			}

			if(currentLevelProgress[i]==';')
				semicolonCount++;
			
			if(semicolonCount==2&&timerStartpoint==0&&!containsFloppyData
			||semicolonCount==3&&timerStartpoint==0&&containsFloppyData)
			{
				timerStartpoint = i;
				Debug.Log("Contains C: "+containsFloppyData+" ,Timer start point: "+timerStartpoint);
			}
		}
		if(oldTimeString!="")
		{
			int.TryParse(oldTimeString,out oldTime);
			Debug.Log("Old time = "+oldTimeString+" and as int: "+oldTime);
		}
		//Debug.Log(";s: "+semicolonCount+" ,has floppy data: "+containsFloppyData);
		if(semicolonCount<=1&&!containsFloppyData
		||semicolonCount<=2&&containsFloppyData)
		{
			print("time recorded");
			currentLevelProgress = currentLevelProgress+';'+timeClock;
		}
		else if(timeClock<oldTime && ( (semicolonCount==2&&!containsFloppyData) || (semicolonCount==3&&containsFloppyData) ))
		{
			print("new time record: "+timeClock);
			//Debug.Log(timerStartpoint);
			currentLevelProgress = currentLevelProgress.Remove(timerStartpoint);
			currentLevelProgress = currentLevelProgress+';'+timeClock;
		}
		DataS.levelProgress[DataS.lastLoadedLevel] = currentLevelProgress;
		DataS.storedItem = storedItemID;
		//DataS.coins = coins;
		DataS.lives = lives;
		DataS.score = score;
		DataS.floppies = floppies;
		//DataS.sausages += levelSausages;
		DataS.playerState = GameObject.Find("Player_main").GetComponent<playerSprite>().state;
		DataS.resetValues();
		//if(DataS.totalCompletedLevels>35)StartCoroutine(waitForSaveData());
		finishedSaving = true;
	}
	/*IEnumerator waitForSaveData()
	{
		DataS.StartCoroutine(DataS.saveData(true));
		yield return 0;
		yield return new WaitUntil(()=>!DataS.saving);
		finishedSaving = true;
	}*/
}
