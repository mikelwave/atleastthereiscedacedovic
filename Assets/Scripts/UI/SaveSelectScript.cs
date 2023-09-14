using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SaveSelectScript : MonoBehaviour {
	[Header("Calculate data")]
	public int sausageMaxAmount = 84;
	public int levelsAmount = 34;
	public int extrasAmount = 23;
	float levelPercent,sausagePercent,extraPercent = 0;
	[Space]
	TextAsset emptyFile;
	//TextAsset completefile;
	int selection = 0; //0 - file 1, 1 - file 2, 2 - file 3, 3 - erase mode
	bool eraseMode = false;
	AxisSimulator axis;
	bool pressedDown = false;
	bool canSelect = true;
	Coroutine cor;
	Transform highlight,brickParticles;
	SpriteRenderer highlightSprite,headerSprite;
	Image eraseModeRenderer;
	AudioSource asc;
	public AudioClip[] sounds;
	int colorFrames = 59;
	int colorMessageFrames = 0;
	int colorint,messageColorInt = 0;
	int deleteModeOffset = 0;
	Animator anim;
	dataShare DataS;
	public Color32[] highlightColors = new Color32[4];
	public int flashSpeed = 10;
	public Sprite[] header,eraseModeSprites,highlightSprites = new Sprite[2];
	public Vector2[] highlightSize = new Vector2[2]
	{
		new Vector2(7,9),
		new Vector2(7,3),
	};
	string[] messages = new string[1]
	{
		"Are you sure you want to delete this file?"
	};
	bool[] allClears = new bool[3];
	public AudioMixer Audio;
	public Sprite[] SaveIcons = new Sprite[4];
	public Sprite[] livesIcon;
	Image highlightMessageImage;
	TextMeshProUGUI text;
	bool choice = true;
	bool choiceConfirmed = false;
	bool displayingMessage = false;
	Transform[] optionButtons = new Transform[2];
	List<int> saveArr1,saveArr2,saveArr3;
	List<string> levelArr1,levelArr2,levelArr3;
	List<List<int>> saveArrs;
	List<List<string>> levelArrs;
	List<string> textAsLines;
	string celStr1,celStr2,celStr3;
	List<string> celStrs;
	TextMeshProUGUI[] livesCounters = new TextMeshProUGUI[3];
	TextMeshProUGUI[] sausagesCounters = new TextMeshProUGUI[3];
	TextMeshProUGUI[] potsCounters = new TextMeshProUGUI[3];
	TextMeshProUGUI[] floppiesCounters = new TextMeshProUGUI[3];
	TextMeshProUGUI[] CompletionRates = new TextMeshProUGUI[3];
	TextMeshProUGUI[] WorldDisplays = new TextMeshProUGUI[3];
	TextMeshProUGUI QuickStartText,swapFileText;
	bool loadingData = false;
	bool inAnimation = false;
	HubCamera cam;
	Image mask;
	Coroutine cor1;
	string[] saveStrings = new string[3]{"Save1","Save2","Save3"};
	int[] saveModes = new int[3]{0,0,0};
	public Sprite[] fileBlocks = new Sprite[2];
	public Color[] skyColors = new Color[2];
	public Color[] headerColors = new Color[2];

	public SpriteRenderer sky;
	
	IEnumerator fadeSky(int target)
	{
		float lerpFactor = 0;
		while(lerpFactor<0.5f)
		{
			lerpFactor+=Time.deltaTime*2f;
			sky.color = Color.Lerp(sky.color,skyColors[target],lerpFactor);
			headerSprite.color = Color.Lerp(headerSprite.color,headerColors[target],lerpFactor);
			yield return 0;
		}
		sky.color = skyColors[target];
		headerSprite.color = headerColors[target];
	}
	Coroutine skyFadeCor;
	void skyFadeFunc(int target)
	{
		if(skyFadeCor!=null)
			StopCoroutine(skyFadeCor);
		skyFadeCor = StartCoroutine(fadeSky(target));
	}

	void analizeUsed()
	{
		string s = PlayerPrefs.GetString("LastUsedSaves");
		//print("Last used saves: "+s);
		for(int i = 0;i<s.Length;i++)
		{
			switch(s[i])
			{
				default: break;
				case '1': //playuh mode
				if(i==0)
				saveStrings[i] = "Save4";
				else if(i==1) saveStrings[i] = "Save5";
				else saveStrings[i] = "Save6";
				saveModes[i] = 1;
				break;
				case '2': //speedrun mode
				if(i==0)
				saveStrings[i] = "Save7";
				else if(i==1) saveStrings[i] = "Save8";
				else saveStrings[i] = "Save9";
				saveModes[i] = 2;
				break;
			}
		}
	}
	void switchSave(int saveID,int repeats)
	{
		saveModes[saveID]+=1;
		if(saveModes[saveID]>2)saveModes[saveID] = 0; //increment mode
		//check if key exists
		int saveLabel = saveID+1;
		saveLabel+=3*saveModes[saveID];
		if(!PlayerPrefs.HasKey("Save"+saveLabel.ToString()))
		{
			//print("Repeat: "+repeats);
			if(repeats<2) switchSave(saveID,repeats+=1);
			return;
		}
		else
		{
			//print("File "+saveLabel+'\n'+PlayerPrefs.GetString("Save"+saveLabel.ToString()));
		}
		string oldString = saveStrings[saveID];
		//read new strings
		switch(saveModes[saveID])
		{
			default:
			if(saveID==0)
			saveStrings[saveID] = "Save1";
			else if(saveID==1) saveStrings[saveID] = "Save2";
			else saveStrings[saveID] = "Save3";
			break;
			case 1: //playuh mode
			if(saveID==0)
			saveStrings[saveID] = "Save4";
			else if(saveID==1) saveStrings[saveID] = "Save5";
			else saveStrings[saveID] = "Save6";
			break;
			case 2: //speedrun mode
			if(saveID==0)
			saveStrings[saveID] = "Save7";
			else if(saveID==1) saveStrings[saveID] = "Save8";
			else saveStrings[saveID] = "Save9";
			break;
		}
		if(saveStrings[saveID]!=oldString)
		{
			asc.PlayOneShot(sounds[1]);
			if(cor1!=null)StopCoroutine(cor1);
			cor1 = StartCoroutine(animateSave(saveID));
		}
	}
	void updateLastUsed()
	{
		string s = "";
		for(int i = 0;i<saveModes.Length;i++)
		{
			s+=saveModes[i].ToString();
		}
		PlayerPrefs.SetString("LastUsedSaves",s);
		//print("Last used: "+s);
	}
	IEnumerator animateSave(int saveID)
	{
		canSelect = false;
		Transform tr = transform.GetChild(saveID);
		float progress = 0;
		Transform mTr = mask.transform;
		mTr.SetParent(tr);
		mTr.SetSiblingIndex(mask.transform.parent.childCount-3);
		mTr.localPosition = Vector3.zero;
		Color transparent = new Color(1,1,1,0);
		yield return 0;
		while(progress<1)
		{
			progress+=Time.deltaTime*10;
			mask.color = Color.Lerp(transparent,Color.black,progress);
			tr.localPosition = new Vector3(tr.localPosition.x,Mathf.Lerp(tr.localPosition.y,0,progress),0);
			yield return 0;
		}
		translateString(PlayerPrefs.GetString(saveStrings[saveID]),saveID); //update save
		//print(saveStrings[saveID]);
		//print(saveID);
		//saveStrings[saveID] = "Save"+(saveID+1);
		UpdateSaveFiles(saveID+1);
		progress = 0;
		while(progress<1)
		{
			progress+=Time.deltaTime*10;
			mask.color = Color.Lerp(Color.black,transparent,progress);
			tr.localPosition = new Vector3(tr.localPosition.x,Mathf.Lerp(0,-10,progress),0);
			yield return 0;
		}
		canSelect = true;
	}
	// Use this for initialization
	void Start ()
	{
		axis = GetComponent<AxisSimulator>();
		DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		highlight = transform.GetChild(4);
		brickParticles = highlight.GetChild(0);
		asc = GetComponent<AudioSource>();
		highlightSprite = highlight.GetComponent<SpriteRenderer>();
		eraseModeRenderer = transform.GetChild(3).GetComponent<Image>();
		headerSprite = transform.GetChild(5).GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();
		cam = GameObject.Find("Main Camera").GetComponent<HubCamera>();
		QuickStartText = transform.GetChild(9).GetComponent<TextMeshProUGUI>();
		swapFileText = transform.GetChild(11).GetComponent<TextMeshProUGUI>();
		bool found = false;
		for(int i = 4;i<=9;i++)
		{
			if(PlayerPrefs.HasKey("Save"+i))
			{
				found = true;
				break;
			}
		}
		if(!found)
		{
			swapFileText.SetText("");
		}
		else
		{
			setFileSwapText();
		}
		mask = transform.GetChild(10).GetComponent<Image>();
		if(!PlayerPrefs.HasKey("LastUsedSaves"))
		{
			PlayerPrefs.SetString("LastUsedSaves","000");
		}
		analizeUsed();
		setQuickStartText();
		LoadSettings();
		for(int i = 0; i<livesCounters.Length;i++)
		{
			livesCounters[i]=transform.GetChild(i).GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
			sausagesCounters[i]=transform.GetChild(i).GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
			potsCounters[i]=transform.GetChild(i).GetChild(1).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
			floppiesCounters[i]=transform.GetChild(i).GetChild(1).GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>();
			CompletionRates[i]=transform.GetChild(i).GetChild(1).GetChild(4).GetComponent<TextMeshProUGUI>();
			WorldDisplays[i]=transform.GetChild(i).GetChild(1).GetChild(5).GetComponent<TextMeshProUGUI>();
		}
		text = transform.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>();
		highlightMessageImage = transform.GetChild(6).GetChild(1).GetComponent<Image>();
		levelPercent = 50/(float)levelsAmount; //50% of total
		sausagePercent = 25/(float)sausageMaxAmount; //25% of total
		extraPercent = 25/(float)extrasAmount; //25% of total
		emptyFile = (TextAsset)Resources.Load("Text/dummysavefile",typeof (TextAsset));
		//completefile = (TextAsset)Resources.Load("dummycompletesavefile",typeof (TextAsset));

		//if(!PlayerPrefs.HasKey(saveStrings[2]))
		//{
		//	PlayerPrefs.SetString(saveStrings[2],completefile.ToString());
		//}
		saveArr1 = new List<int>();
		levelArr1 = new List<string>();
		celStr1 = "";
		saveArr2 = new List<int>();
		levelArr2 = new List<string>();
		celStr2 = "";
		saveArr3 = new List<int>();
		levelArr3 = new List<string>();
		celStr3 = "";
		saveArrs = new List<List<int>>();
		levelArrs = new List<List<string>>();
		celStrs = new List<string>();
		levelArrs.Add(levelArr1);
		levelArrs.Add(levelArr2);
		levelArrs.Add(levelArr3);
		saveArrs.Add(saveArr1);
		saveArrs.Add(saveArr2);
		saveArrs.Add(saveArr3);
		celStrs.Add(celStr1);
		celStrs.Add(celStr2);
		celStrs.Add(celStr3);
		for(int i = 2; i<transform.GetChild(6).childCount;i++)optionButtons[i-2]=transform.GetChild(6).GetChild(i);
		UpdateSaveFiles(0);
	}
	void UpdateSaveFiles(int ID)
	{
		//1 = file1 updates, 2 = file2 updates, 3 = file3 updates
		switch(ID)
		{
			//all files update
			default:
			if(PlayerPrefs.HasKey(saveStrings[0]))
			{
				translateString(PlayerPrefs.GetString(saveStrings[0]),0);
				transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
			}
			else
			{
				transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
				transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
			}
			if(PlayerPrefs.HasKey(saveStrings[1]))
			{
				translateString(PlayerPrefs.GetString(saveStrings[1]),1);
				transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
			}
			else
			{
				transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
				transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
			}
			if(PlayerPrefs.HasKey(saveStrings[2]))
			{
				translateString(PlayerPrefs.GetString(saveStrings[2]),2);
				transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(2).GetChild(2).gameObject.SetActive(false);
			}
			else
			{
				transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
				transform.GetChild(2).GetChild(2).gameObject.SetActive(true);
			}
			break;
			case 1:if(PlayerPrefs.HasKey(saveStrings[0]))
			{
				translateString(PlayerPrefs.GetString(saveStrings[0]),0);
				transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
			}
			else
			{
				transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
				transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
			}
			break;
			case 2:if(PlayerPrefs.HasKey(saveStrings[1]))
			{
				//print(saveStrings[1]+" found");
				translateString(PlayerPrefs.GetString(saveStrings[1]),1);
				transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
			}
			else
			{
				//print(saveStrings[1]+" not found");
				transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
				transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
			}
			break;
			case 3:if(PlayerPrefs.HasKey(saveStrings[2]))
			{
				translateString(PlayerPrefs.GetString(saveStrings[2]),2);
				transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
				transform.GetChild(2).GetChild(2).gameObject.SetActive(false);
			}
			else
			{
				transform.GetChild(2).GetChild(1).gameObject.SetActive(false);
				transform.GetChild(2).GetChild(2).gameObject.SetActive(true);
			}
			break;
		}

	}
	string swapChar(string org,int index,string c)
	{
		org = org.Remove(index,1);
		org = org.Insert(index,c);
		return org;
	}
	void failSafe()
	{
		int ID = DataS.saveFileID;
		if(DataS.mode==0&&ID>2&&ID<6)DataS.mode=1;
		else if(DataS.mode==1&&(ID<3||ID>6))DataS.mode=0;
		//check if total completed levels matches with amount of absent "N"s in the data array
		//check if total collected sausages match with "1"s in the sausage segemnt of all levels
		int dataLevels = dataShare.totalCompletedLevels;
		int dataSausages = DataS.sausages;
		int assumedTotalCompleted = DataS.levelProgress.Length;
		int foundSausages = 0;

		dataLevels = Mathf.Clamp(dataLevels,0,DataS.levelProgress.Length);
		dataSausages = Mathf.Clamp(dataSausages,0,sausageMaxAmount);
		for(int i = 0;i<DataS.levelProgress.Length;i++)
		{
			string curLine = DataS.levelProgress[i];
			//check if starts with N
			if(curLine.StartsWith("N;"))assumedTotalCompleted--;

			//if length of string bigger than 2, set pointer to 2 and check until semicolon
			if(curLine.Length>2)
			for(int y = 2;y<curLine.Length;y++)
			{
				if(curLine[y]==';')break;
				else if(curLine[y]=='1')foundSausages++;
			}
		}
		foundSausages = Mathf.Clamp(foundSausages,0,sausageMaxAmount);
		print("Beaten levels In file: "+dataLevels+" Assumed: "+assumedTotalCompleted);
		print("Sausages value in file: "+dataSausages+" Found in progress: "+foundSausages);

		if(dataLevels<assumedTotalCompleted)
		dataLevels = assumedTotalCompleted;

		if(dataSausages<foundSausages)
		dataSausages = foundSausages;

		if(dataLevels!=assumedTotalCompleted||dataSausages!=foundSausages)
		{
			Debug.Log("Warning: Anomaly in save file levels, attempting fix.");
			//loop through all elements in level progress until assumed is the same as total

			int offset = (dataLevels-assumedTotalCompleted);
			int missingSausages = (dataSausages-foundSausages);

			print(offset+" expected levels not cleared in file");
			print(missingSausages+" expected sausages not appeared in file");

			for(int i = 0;i<dataLevels;i++)
			{
				string curLine = DataS.levelProgress[i];
				//levels fix
				if(offset>0&&curLine.StartsWith("N;"))
				{
					curLine = swapChar(curLine,0,"F");
					offset--;
				}
				//sausages fix
				if(missingSausages>0&&curLine.Length>=5)
				for(int y = 2;y<5;y++)
				{
					if(curLine[y]=='0')
					{
						curLine = swapChar(curLine,y,"1");
						missingSausages--;
					}
				}
				DataS.levelProgress[i]=curLine;
				if(offset<=0&&missingSausages<=0)
				{
					print("Save file fix attempted");
					break;
				}
			}
		}
		print("Check complete");
		DataS.sausages = dataSausages;
		dataShare.totalCompletedLevels = dataLevels;
	}
	void writeToDataShare(int ID)
	{
		try
		{
			loadingData = true;
			dataShare.allClear = allClears[ID];
			DataS.currentWorld = saveArrs[ID][1];
			dataShare.totalCompletedLevels = saveArrs[ID][2];
			DataS.score = saveArrs[ID][3];
			DataS.lives = saveArrs[ID][4];
			DataS.coins = saveArrs[ID][5];
			DataS.sausages = saveArrs[ID][6];
			DataS.floppies = saveArrs[ID][7];
			DataS.playerState = saveArrs[ID][8];
			DataS.storedItem = saveArrs[ID][9];
			DataS.lastLoadedLevel = saveArrs[ID][10];
			DataS.lastLoadedWorld = saveArrs[ID][11];
			DataS.worldProgression = saveArrs[ID][12];
			DataS.AndreMissionProgress = saveArrs[ID][13];
			DataS.DjoleMissionProgress = saveArrs[ID][14];
			DataS.MiroslavMissionProgress = saveArrs[ID][15];
			if(saveArrs[ID].Count>16)
			{
				DataS.mode = saveArrs[ID][16];
				DataS.infiniteLives = saveArrs[ID][17];
				DataS.difficulty = saveArrs[ID][18];
			}
			for(int i = 0; i<levelArrs[ID].Count;i++)
			{
				if(i<DataS.levelProgress.Length)
				DataS.levelProgress[i] = levelArrs[ID][i];
			}
			//load cell data
			//print("celStrs: "+celStrs[ID]);
			DataS.cellData = new bool[62];
			if(celStrs[ID]!="")
			{
				for(int i = 0; i<celStrs[ID].Length;i++)
				{
					if(celStrs[ID][i]=='1')
					DataS.cellData[i]=true;
				}
			}
			Debug.Log("Data loaded");
			failSafe();
			loadingData = false;
		}
		catch(System.IndexOutOfRangeException ex)
		{
			Debug.LogError(ex+ " file corrupt");
			switch(ID)
			{
				default:PlayerPrefs.DeleteKey(saveStrings[0]); break;
				case 1: PlayerPrefs.DeleteKey(saveStrings[1]); break;
				case 2: PlayerPrefs.DeleteKey(saveStrings[2]); break;
			}
			saveArrs[ID] = new List<int>();
			DataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		}
	}
	void translateString(string s,int ID)
	{
		//Debug.Log("Save file:"+'\n'+s);
		int fileProgression=0,saveProgress=0,levelProgress=0;
		textAsLines = new List<string>();
		textAsLines.AddRange(s.Split("\n"[0]));
		saveArrs[ID].Clear();
		levelArrs[ID].Clear();
		string curLine = "";
		for(int allLines = 0;allLines<textAsLines.Count;allLines++)
		{
			string currentTextLine = textAsLines[allLines];
			//Debug.Log(currentTextLine);
			if(currentTextLine.StartsWith(";1")&&fileProgression==0)
			fileProgression = 1;
			else if(currentTextLine.StartsWith(";2")&&fileProgression==1)
			{
			fileProgression = 2;
			continue;
			}
			else if(currentTextLine.StartsWith(";3")&&fileProgression==2)
			{
			//print("Found 3");
			fileProgression = 3;
			continue;
			}
			//read current line
			//print(currentTextLine);
			for(int i = 0; i<currentTextLine.Length;i++)
			{
				//Debug.Log(currentTextLine[i]+", "+System.Convert.ToInt32(currentTextLine[i]));
				//characters that end line reading: Return, =, /
				if(System.Convert.ToInt32(currentTextLine[i])==13
				||System.Convert.ToInt32(currentTextLine[i])==61
				||System.Convert.ToInt32(currentTextLine[i])==47)
				{
					switch(fileProgression)
					{
						default: //Debug.Log(fileProgression+": "+curLine);
						break;
						case 1:
						if(curLine!="")
						{
							int val = 0;
							int.TryParse(curLine, out val);

							saveArrs[ID].Add(val);
							saveProgress++;
							curLine = "";
						}
						break;
						case 2:
						levelProgress++;
						if(curLine!="")
						{
							//Debug.Log(curLine);
							levelArrs[ID].Add(curLine);
							curLine = "";
						}
						break;
						case 3:
							celStrs[ID] = curLine;
							curLine = "";
						break;
					}
					break;
				}
				else
				{
					curLine+=currentTextLine[i];
				}
			}
		}
		int mode = 0;
		if(saveArrs[ID].Count>16)
		{
			//print("Mode: "+saveArrs[ID][16]);
			mode = Mathf.Clamp(saveArrs[ID][16],0,2);
		}
		Image l = livesCounters[ID].transform.parent.GetComponent<Image>();
		if(mode==1)
		l.sprite = livesIcon[1];
		else l.sprite = livesIcon[0];
		livesCounters[ID].text = (saveArrs[ID][4].ToString("00")+(saveArrs[ID][17]==1 ? "*":""));
		potsCounters[ID].text = saveArrs[ID][5].ToString("000");
		sausagesCounters[ID].text = saveArrs[ID][6].ToString("00");
		floppiesCounters[ID].text = saveArrs[ID][7].ToString("00");
		//print("Total completed: "+saveArrs[ID][2]);
		float Percentage = (levelPercent*saveArrs[ID][2])+(sausagePercent*saveArrs[ID][6])
		+((saveArrs[ID][13]+saveArrs[ID][14]+saveArrs[ID][15])*extraPercent);

		if(mode==0)
		{
			if(saveArrs[ID][6]<sausageMaxAmount)
			{
				Percentage = Mathf.Clamp(Percentage,0,100);
			}
		}
		else if(mode==1)
		{
			if(saveArrs[ID][6]<sausageMaxAmount)
			{
				Percentage = Mathf.Clamp(Percentage,0,101);
			}
		}
		//Debug.Log("Percentage: "+Percentage);
		//print(saveArrs[ID][11].ToString());
		if(saveArrs[ID][11].ToString()!="0")
		WorldDisplays[ID].text = "World "+saveArrs[ID][11].ToString();
		else
		{
			//print("Total Comp: "+saveArrs[ID][2]);
			if(mode==1)
			{
				if(saveArrs[ID][2]!=0)
				{
					WorldDisplays[ID].text = "Poor 4";
				}
				else WorldDisplays[ID].text = "World 1";
			}
			else
			{
				WorldDisplays[ID].text = "At Home";
			}
		}
		//print(WorldDisplays[ID].text);
		ParticleSystem p = transform.GetChild(ID).GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
		if(mode==1) //check for golden god
		{
			string st = celStrs[ID];
			bool complete = true;
			if(st.Length>=29)
			for(int i = 0;i<29;i++)
			{
				if(st[i]!='1')
				{
					complete = false;
					break;
				}
			}
			Percentage+=complete ? 1 : 0;
		}
		if(Percentage>=100)
		{
			transform.GetChild(ID).GetChild(0).GetComponent<Image>().sprite = fileBlocks[1];
			if(Percentage>=102||mode!=1&&Percentage>=101)
			{
				p.Play();
				allClears[ID] = true;
			}
			else
			{
				allClears[ID] = false;
				if(p.isPlaying)
				{
					p.Stop(true,ParticleSystemStopBehavior.StopEmitting);
				}
			}
		}
		else
		{
			transform.GetChild(ID).GetChild(0).GetComponent<Image>().sprite = fileBlocks[0];
			allClears[ID] = false;
			if(p.isPlaying)
			p.Stop(true,ParticleSystemStopBehavior.StopEmitting);
		}
		if(Percentage!=0)
		CompletionRates[ID].text = Percentage.ToString("#.")+"%";
		else CompletionRates[ID].text = "0%";
		Image icon;
		if(transform.GetChild(ID).GetChild(3).name!="Mask")
		icon = transform.GetChild(ID).GetChild(3).GetComponent<Image>();
		else icon = transform.GetChild(ID).GetChild(4).GetComponent<Image>();
		switch(mode)
		{
			default:
			icon.sprite = SaveIcons[1];
			break;
			case 1:
			icon.sprite = SaveIcons[2];
			break;
			case 2:
			icon.sprite = SaveIcons[3];
			break;
		}
	}
	void changeOption(float dir)
	{
		if(dir>0) {selection++;if(selection>2)selection=0;}
		else if (dir<0) {selection--;if(selection<0)selection=2;}
		asc.PlayOneShot(sounds[0]);
		highlight.localPosition = transform.GetChild(selection).localPosition;
	}
	void changeGraphics(int i)
	{
		skyFadeFunc(i);
		highlightSprite.sprite = highlightSprites[i];
		headerSprite.sprite = header[i];
		eraseModeRenderer.sprite = eraseModeSprites[i];
		eraseModeRenderer.SetNativeSize();
	}
	void messageOption()
	{
		choice = !choice;
		int i = 0;
		asc.PlayOneShot(sounds[0]);
		if(!choice) i = 1;
			Vector3 buttonPos = optionButtons[i].localPosition;
		highlightMessageImage.transform.localPosition = new Vector3(buttonPos.x+10,buttonPos.y-10,buttonPos.z);
	}
	void setQuickStartText()
	{
		string runWord = SuperInput.GetKeyName("Run");
		if(runWord.Contains("Joystick"))
		runWord = changeWord(runWord);

		string jumpWord = SuperInput.GetKeyName("Jump");
		if(jumpWord.Contains("Joystick"))
		jumpWord = changeWord(jumpWord);

		QuickStartText.text = "Quick Start: "+runWord+" + "+jumpWord;
	}
	void setFileSwapText()
	{
		string swapWord = SuperInput.GetKeyName("Select");
		if(swapWord.Contains("Joystick"))
		swapWord = changeWord(swapWord);

		swapFileText.text = "Swap Game Mode: "+swapWord;
	}
	string changeWord(string wordInsert)
	{
		for(int i = 0; i<wordInsert.Length;i++)
		{
			if(wordInsert[i]=='B')
			{
				wordInsert = wordInsert.Substring(i);
				//print(wordInsert);
				break;
			}
		}
		string oldWord = wordInsert;
		wordInsert = InputReader.joystickInterpreter(wordInsert);
		if(wordInsert==oldWord)
		{
			Debug.Log("Couldn't find icon...");
			for(int i = 0; i<wordInsert.Length-1;i++)
			{
				if(char.IsDigit(wordInsert[i+1]))
				{
					wordInsert = wordInsert.Substring(i+1);
					break;
				}
			}
			wordInsert = "Btn"+wordInsert;
			//print(wordInsert);
		}
		return wordInsert;
	}
	void displayMessage(int disp)
	{
		choiceConfirmed = false;
		canSelect = true;
		choice = true;
		Vector3 buttonPos = optionButtons[0].localPosition;
		highlightMessageImage.transform.localPosition = new Vector3(buttonPos.x+10,buttonPos.y-10,buttonPos.z);
		if(disp<messages.Length)
		{
			displayingMessage = true;
			text.text = messages[disp];
			anim.SetBool("Message",true);
		}
	}
	void LoadSettings()
	{
		GameSettings settings = SettingsSaveSystem.LoadSettings();
		if(settings!=null)
		{
		int musicVolume = settings.musicVolume;
		bool musicEnabled = settings.musicEnabled;
		int sfxVolume = settings.sfxVolume;
		bool sfxEnabled = settings.sfxEnabled;

		if(sfxVolume!=100)
			Audio.SetFloat("SFXVolume",(-40f/100)*(100-sfxVolume));
		else Audio.SetFloat("SFXVolume",0f);

		if(musicEnabled&&musicVolume!=0)
			{
				if(musicVolume!=100)
				Audio.SetFloat("MusicVolume",(-40f/100)*(100-musicVolume));
				else Audio.SetFloat("MusicVolume",0f);
			}
			else Audio.SetFloat("MusicVolume",-80f);
			if(sfxEnabled&&sfxVolume!=0)
			{
				if(sfxVolume!=100)
				Audio.SetFloat("SFXVolume",(-40f/100)*(100-sfxVolume));
				else Audio.SetFloat("SFXVolume",0f);
			}
			else Audio.SetFloat("SFXVolume",-80f);
		}	
	}
	IEnumerator eraseSave(int ID)
	{
		inAnimation = true;
		canSelect = false;
		Image over = transform.GetChild(ID).GetChild(2).GetComponent<Image>();
		GameObject overText = over.transform.GetChild(0).gameObject;
		overText.SetActive(false);
		over.fillAmount = 0;
		over.gameObject.SetActive(true);
		ParticleSystem p = transform.GetChild(ID).GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
		if(p.isPlaying)p.Stop(true,ParticleSystemStopBehavior.StopEmitting);
		allClears[ID] = false;
		for(int i = 0; i<6;i++)
		{
			over.fillAmount = 0.175f*(i+1);
			asc.PlayOneShot(sounds[2+i]);
			cam.shakeCamera(0.1f,0.15f);
			yield return new WaitForSeconds(0.3f);
		}
		overText.SetActive(true);
		Image icon;
		if(transform.GetChild(ID).GetChild(3).name!="Mask")
		icon = transform.GetChild(ID).GetChild(3).GetComponent<Image>();
		else icon = transform.GetChild(ID).GetChild(4).GetComponent<Image>();
		icon.GetComponent<Image>().sprite = SaveIcons[0];
		asc.PlayOneShot(sounds[9]);
		transform.GetChild(ID).GetChild(0).GetComponent<Image>().sprite = fileBlocks[0];
		
		switch(ID)
		{
			default:PlayerPrefs.DeleteKey(saveStrings[0]); UpdateSaveFiles(1);break;
			case 1: PlayerPrefs.DeleteKey(saveStrings[1]); UpdateSaveFiles(2);break;
			case 2: PlayerPrefs.DeleteKey(saveStrings[2]); UpdateSaveFiles(3);break;
		}
		
		Image l = livesCounters[ID].transform.parent.GetComponent<Image>();
		l.sprite = livesIcon[0];
		saveArrs[ID] = new List<int>();
		levelArrs[ID] =new List<string>();
		Debug.Log("File "+(ID+1)+" erased");
		//switch to relevant save if found any
		bool altFound = false;
		for(int i = 0;i<3;i++)
		{
			int saveID = 1+ID+(3*i);
			//print("Testing for: "+saveID.ToString());
			if(PlayerPrefs.HasKey("Save"+saveID.ToString()))
			{
				altFound = true;
				//print("Found alt save: "+saveID.ToString());
				break;
			}
			//else print("Failed");
		}
		if(altFound)
		{
			yield return new WaitForSeconds(0.2f);
			switchSave(ID,0);
		}
		else
		{
			canSelect = true;
		}
		updateLastUsed();
		inAnimation = false;
	}
	IEnumerator createSave(int ID)
	{
		inAnimation = true;
		canSelect = false;
		//Create animation
		asc.PlayOneShot(sounds[8]);
		cam.shakeCamera(0.5f,0.2f);
		brickParticles.gameObject.SetActive(true);
		switch(ID)
		{
			default:PlayerPrefs.SetString(saveStrings[0],emptyFile.ToString());UpdateSaveFiles(1);break;
			case 1: PlayerPrefs.SetString(saveStrings[1],emptyFile.ToString());UpdateSaveFiles(2);break;
			case 2: PlayerPrefs.SetString(saveStrings[2],emptyFile.ToString());UpdateSaveFiles(3);break;
		}
		Image icon;
		if(transform.GetChild(ID).GetChild(3).name!="Mask")
		icon = transform.GetChild(ID).GetChild(3).GetComponent<Image>();
		else icon = transform.GetChild(ID).GetChild(4).GetComponent<Image>();
		icon.GetComponent<Image>().sprite = SaveIcons[1];
		yield return new WaitForSeconds(1.5f);
		Debug.Log("File "+ID+" created");
		inAnimation = false;
		//canSelect = true;
	}
	IEnumerator selectOption()
	{
		canSelect = false;
		bool quickStart = (SuperInput.GetKey("Run")&&SuperInput.GetKey("Jump")) ? true : false;
		if(selection==3)
		{
			if(!eraseMode)
			{
				asc.PlayOneShot(sounds[1]);
				canSelect = true;
				eraseMode = true;
				//print("erase mode");
				changeGraphics(1);
				deleteModeOffset = 2;
			}
			else
			{
				asc.PlayOneShot(sounds[1]);
				canSelect = true;
				//print("exited erase mode");
				deleteModeOffset = 0;
				changeGraphics(0);
				eraseMode = false;
			}
		}
		//save select
		else
		{
			//print("selected save #"+(selection+1));
			flashSpeed = 5;
			DataS.resetDS();
			if(!eraseMode)
			{
				//blank file detect
				if(saveArrs[selection].Count==0)
				{
					//print("Creating save");
					saveModes[selection]=0;
					saveStrings[selection]="Save"+(1+selection);
					//print(saveStrings[selection]);
					StartCoroutine(createSave(selection));
					quickStart = false;
					//canSelect = true;
				}
				if(levelArrs[selection][0].Contains("N"))
				{
					//print("Empty save");
					quickStart = false;
				}
				updateLastUsed();
				asc.PlayOneShot(sounds[1]);
				if(quickStart)asc.PlayOneShot(sounds[10]);
				yield return new WaitForSeconds(0.5f);
				DataS.saveFileID = selection+(saveModes[selection]*3);
				print("Load file #"+(selection+1));
				writeToDataShare(selection);
				yield return new WaitUntil(()=> !loadingData);
				//Debug.Log("Data loaded");
				cam.fadeScreen(true);
				yield return new WaitUntil(()=> cam.fadeAnim>=1f);
				if(!quickStart)
				DataS.loadSceneWithLoadScreen(2);
				else
				{
					DataS.updateBook(false);
					if(DataS.lastLoadedWorld!=0)
					DataS.loadWorldWithLoadScreen(DataS.lastLoadedWorld);
					else DataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex+2);
				}
			}
			else
			{
				bool fileExists = false;
				switch(selection)
				{
					default:
					if(PlayerPrefs.HasKey(saveStrings[0]))fileExists = true;
					break;
					case 1:
					if(PlayerPrefs.HasKey(saveStrings[1]))fileExists = true;
					break;
					case 2:
					if(PlayerPrefs.HasKey(saveStrings[2]))fileExists = true;
					break;
				}
				if(fileExists)
				{
					//print("Delete file #"+(selection+1)+"?");
					displayMessage(0);
					yield return new WaitUntil(()=> choiceConfirmed);
					if(choice)
					{
						//print("Delete file #"+(selection+1)+" here.");
						StartCoroutine(eraseSave(selection));
					}
				}
				flashSpeed = 30;
				canSelect = true;
			}
		}
	}
	IEnumerator closeMessageBox()
	{
		asc.PlayOneShot(sounds[1]);
		canSelect = false;
		colorMessageFrames=20;
		yield return new WaitUntil(()=> colorMessageFrames==0);
		choiceConfirmed = true;
		displayingMessage = false;
		anim.SetBool("Message",false);
		if(!choice)
		canSelect = true;
	}
	// Update is called once per frame
	void Update ()
	{
		if(colorFrames>0)
		{
			colorFrames--;
			if(colorFrames==0)
			{
				colorint = 0;
				colorFrames = 59;
			}
			else if(colorFrames%flashSpeed==0)
			{
				colorint++;
				if(colorint+deleteModeOffset>1+deleteModeOffset)
					colorint = 0;
			}
			if(highlightSprite.color !=highlightColors[colorint+deleteModeOffset])
				highlightSprite.color = highlightColors[colorint+deleteModeOffset];
		}
		if(colorMessageFrames>0)
			{
				colorMessageFrames--;
				if(colorMessageFrames==0)
					messageColorInt = 0;
				else if(colorMessageFrames%5==0)
				{
					messageColorInt++;
					if(messageColorInt>1)
					messageColorInt = 0;
				}
				if(displayingMessage)
				{
					highlightMessageImage.color = highlightColors[messageColorInt+4];
				}
			}
		if(!displayingMessage&&!inAnimation)
		{
			if(Mathf.Abs(axis.horAxis)==1&&!pressedDown&&selection!=3&&canSelect)
			{
				pressedDown = true;
				changeOption(axis.horAxis);
			}
			else if(axis.verAxis==-1&&!pressedDown&&selection!=3&&canSelect)
			{
				pressedDown = true;
				selection = 3;
				highlight.localPosition = transform.GetChild(selection).localPosition;
				highlightSprite.size = highlightSize[1];
				asc.PlayOneShot(sounds[0]);
			}
			else if(axis.verAxis==1&&!pressedDown&&selection==3&&canSelect)
			{
				pressedDown = true;
				selection = 0;
				highlight.localPosition = transform.GetChild(selection).localPosition;
				highlightSprite.size = highlightSize[0];
				asc.PlayOneShot(sounds[0]);
			}
			else if(axis.horAxis==0&&axis.verAxis==0&&pressedDown)
			{
				pressedDown = false;
			}
			if(SuperInput.GetKeyDown("Jump")&&canSelect||axis.reader.controllerType%5==0&&Input.GetKeyDown(KeyCode.Return)&&canSelect)
			{
					
				if(cor!=null)
					StopCoroutine(cor);
				cor = StartCoroutine(selectOption());
			}
			if(SuperInput.GetKeyDown("Select")&&canSelect)
			{
				if(eraseMode)
				{
					//print("exited erase mode");
					deleteModeOffset = 0;
					asc.PlayOneShot(sounds[1]);
					changeGraphics(0);
					eraseMode = false;
				}
				else
				{
					if(selection<=2)
					{
						switchSave(selection,0);
					}
				}
			}
		}
		else
		{
			if(Mathf.Abs(axis.horAxis)==1&&!pressedDown&&canSelect&&!inAnimation)
			{
				pressedDown = true;
				messageOption();
			}
			else if(axis.horAxis==0&&pressedDown)
			{
				pressedDown = false;
			}
			if(SuperInput.GetKeyDown("Jump")&&canSelect||axis.reader.controllerType%5==0&&Input.GetKeyDown(KeyCode.Return)&&canSelect)
			{
				StartCoroutine(closeMessageBox());
			}

		}
	}
}
