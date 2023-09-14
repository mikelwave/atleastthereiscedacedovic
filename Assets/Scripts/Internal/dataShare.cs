using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class dataShare : MonoBehaviour {
    public int saveFileID = 0;
    public int infiniteLives = 0; //0 - off, 1 = on
    public int difficulty = 0; //0 = Normal, 1 = Hard, 2 = Insane
    public int mode = 0; //0 = Standard, 1 = Playuh, 2 = Speedrun
    public int currentWorld = 1;
    public int startTotalCompletedLevels = 0;
    public static int totalCompletedLevels = 0;
	public long score;
    public long CheckpointScore;
    public int CheckpointSausages = 0,checkpointFloppies;
    public int savedTimeClock = 0;
    public bool checkpointCheat = false;
	public int lives = 5;
	public int coins = 0;
    public int checkPointCoins = 0;
    public int checkPointState = 0;
	public int sausages = 0;
    public int spentSausages = 0;
	public int floppies = 0;
	public int playerState = 0;
	public int storedItem = 0;
    public int checkPointTimeCounter = 0;
    public int lastLoadedLevel = 0;
    public int lastLoadedWorld = 0;
    public int levelToLoad = 0;
    public int parTime = 0;
    public int skipsLeft = -1;
    public bool startAllClear = false;
    public static bool allClear = false;
   // public int levelToUnload = 0;
    public int checkpointValue = 0;
    public bool startInSub = false;
    public int specialData = 0;
    //F = level finished, N = level not finished, 1 = sausage, 0 = no sausage.
    //Name > is beaten > sausages > time in seconds. (if no time, put in not yet cleared instead)
    public string checkpointLevelProgress;
    public string[] levelProgress = {"N;000"};
    public string[] levelNames = {"welcome to death"};
    public string[] worldNames = {"BORING","JAVA","FLASH","SHAREWARE","SPOOKY","FINAL","MEME CORE"};
    public int worldProgression = 0;
	private bool created = false;
    public bool clearedUnbeatenLevel = false;
    public bool leavingLevel = false;
    public bool playCutscene = true;
    public int AndreMissionProgress,DjoleMissionProgress,MiroslavMissionProgress = 0;
    public Vector3 savedCamPos = Vector3.zero;
    public bool hasred,hasblue,hasyellow = false;
    public GameObject subtitlePrefab;
    public static bool godMode = false,randSounds = false;
    [HideInInspector]
    public bool saving = false;
    public GameObject loreBook;
    GameObject activeBook;
    LorebookScript lbScript;
    public bool[] cellData = new bool[62];
    public GameObject saveIcon;
    public string savedTrackName = "";
    public int savedTrackPoint = 0;
    public float savedTrackBeatDelay = 0;
    public static bool debug = false;
    public bool debugMode = false;
    //proburek: make my ears bleed
    //proburek: how are you so small
    //go and rest our heroes
    //find more disks
    //just turn on infinite lives
    //where are you at
    //find the way
    //eternal prefix
    //todd: triumph over double digits
    static int charPoint = 0;
    static string[] codes = {"pbmmeb","pbhayss","pbpower##","pbgaroh","pbreset","pbteatime###","pbfmd##","pbeteatime",
    "pbefmd","pbjtoil##","pbejtoil","pbwaya","pbftw#","pbeftw","pbcheckit#","pbnteatime","pbnfmd","pbnjtoil","pbtodd########","pbntodd","pbdiff#","pbetodd"};
    static string typedCode = "",valueCode = "";
    public static int powerBufferInt = -1,codeIDBuffer = -1;
    public AudioClip cheatSound;
    List <GameObject> duplicates;
	// Use this for initialization
    public static void codeInput()
    {
        foreach(KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
        {
            if(Input.GetKeyDown(k))
            {
                string v = k.ToString();
                if(v.Contains("Alpha"))
                    v = v.Substring(v.Length-1);

                char c = v[0];
                c = System.Char.ToLower(c);

                //Debug.Log(typedCode);
                //validate code
                bool valid = false;
                for(int i = 0;i<codes.Length;i++)
                {
                    string s = codes[i];
                    string substr = "";
                    if(charPoint<s.Length)
                    {
                        substr = s.Substring(0,charPoint);
                    }
                    else substr = s;
                    if(charPoint>0&&typedCode!=substr)
                    {
                        if(typedCode=="")break;
                        continue;
                    }
                    //else print("Charpoint: "+charPoint+" Typedcode: "+typedCode+" SubString: "+substr);
                    //check if typed char is a digit, if it is - only type it if it's not the end of the code string
                    
                    char a = '!';
                    if(charPoint<s.Length)
                    a = s[charPoint];

                    //Valid char
                    if(a=='#'||a==c)
                    {
                        valid = true;
                        if(a=='#')
                        {
                            typedCode+=a;
                            valueCode+=c;
                            charPoint++;
                            if(charPoint==s.Length)
                            {
                                //Debug.Log("Code: "+s+" Value: "+valueCode);
                                int val = 0;
                                int.TryParse(valueCode,out val);
                                if(powerBufferInt==-1)
                                IDCode(s,val,i);
                            }
                            break;
                        }
                        else
                        {
                            typedCode+=c;
                            charPoint++;
                        }

                        evalCode(typedCode);
                        break;
                    }
                }
                //print(typedCode+" "+typedCode.Length+" cpoint: "+charPoint);
                if(!valid)
                {
                    if(dataShare.debug&&typedCode.Length>2)
                    {
                        Debug.Log("Invalid code: "+typedCode);
                    }
                    resetCode();
                }
            }
        }
    }
    static void resetCode()
    {
        charPoint = 0;
        typedCode = "";
        valueCode = "";
    }
    static void evalCode(string inCode)
    {
        if(SceneManager.GetActiveScene().buildIndex==2)
        {
            resetCode();
            return;
        }
        for(int i = 0;i<codes.Length;i++)
        {
            if(inCode==codes[i])
            {
                codeIDBuffer = i;
                //print(inCode+" "+i);
                switch(i)
                {
                    default:
                    break;
                    case 0:
                    randSounds = !randSounds;
                    Debug.Log("Random sounds: "+randSounds);
                    break;
                    case 1:
                    godMode = !godMode;
                    Debug.Log("God mode: "+godMode);
                    break;
                    case 3:
                    break;
                    case 4:
                    godMode = false;
                    randSounds = false;
                    debug = false;
                    //print("Reset");
                    break;
                    case 11:
                    debug = !debug;
                    Debug.Log("Debug: "+debug);
                    break;
                }
                resetCode();
                break;
            }
        }
    }
    public static void IDCode(string inCode,int id,int bufferID)
    {
        codeIDBuffer = bufferID;
        powerBufferInt = id;
        if(debug)
        print("ID Code: "+inCode+id);
        resetCode();
        //cheats++;
    }
    void evalAllClear()
    {
        if(totalCompletedLevels>=42&&sausages>=84)
        {
            if(mode==1)
            {
                bool complete = true;
                for(int i = 0;i<29;i++)
                {
                    if(cellData[i]!=false)
                    {
                        complete = false;
                        break;
                    }
                }
                allClear = complete;
            }
            else allClear = true;
            print("All clear: "+allClear);
        }
    }
    void Awake()
    {
        if (!created)
        {
            foreach (GameObject dup in GameObject.FindGameObjectsWithTag ("DataShare"))
            {
			if (dup.Equals(this.gameObject))
            {
                //print("found self");
				continue;
            }
            else
            {
                //print("found copy");
                Destroy(gameObject);
            }
			return;
		    }
		    DontDestroyOnLoad (transform.gameObject);
            if(transform.childCount==0)
            {
                Transform t = Instantiate(loreBook,transform.position,Quaternion.identity).transform;
                t.gameObject.SetActive(false);
                t.SetParent(transform);
                activeBook = t.gameObject;
            }
            else
            {
                activeBook = transform.GetChild(0).gameObject;
            }
            lbScript = activeBook.transform.GetChild(0).GetComponent<LorebookScript>();
            lbScript.Initialize();
            created = true;
            allClear = startAllClear;
            totalCompletedLevels = startTotalCompletedLevels;
            //Debug.Log("Awake: " + this.gameObject);
            debug = debugMode;
        }
    }
    public void showLoreBook()
    {
        activeBook.SetActive(true);
    }
    public bool bookActive()
    {
        return activeBook.activeInHierarchy;
    }
    public bool getBookDisableFlag()
    {
        return lbScript.disableFlag;
    }
    public void setBookCam(Camera c,float distance)
    {
        StartCoroutine(assignCamBook(c,distance));
    }
    public bool updateBook(bool spawnNotifs)
    {
        return lbScript.updateUnlocked(spawnNotifs);
    }
    public void forceBookNotifs()
    {
        lbScript.forceNotifs();
    }
    IEnumerator assignCamBook(Camera c,float distance)
    {
        yield return 0;
        Canvas C = activeBook.GetComponent<Canvas>();
        C.worldCamera = c;
        C.planeDistance = distance;
    }
    public void loadSceneWithLoadScreen(int sceneID)
    {
        if(sceneID<4)
        currentWorld = 0;
        levelToLoad = sceneID;
        //levelToUnload = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene (0);
    }
    public void loadSceneWithoutLoadScreen(int sceneID)
    {
        SceneManager.LoadScene (sceneID);
    }
    public void loadWorldWithLoadScreen(int worldID)
    {
        //print("Progression: "+worldProgression+" ID: "+worldID);
        bool noWorld = false;
        int sceneID = 2;
        currentWorld = worldID;
        switch(worldID)
        {
            default:sceneID = 4; break;
            case 2: sceneID = 9; break;
            case 3: sceneID = 16; break;
            case 4: sceneID = 23; break;
            case 5: sceneID = 30; break;
            case 6: sceneID = 37; break;
            case 7: sceneID = 44; break;
            //case 7: noWorld = true; Debug.LogError("No world "+worldID+" scene ID assigned"); break;
        }
        levelToLoad = sceneID;
        //levelToUnload = SceneManager.GetActiveScene().buildIndex;
        if(!noWorld)
        lastLoadedWorld = worldID;
        else lastLoadedWorld = 0;
		SceneManager.LoadScene (0);
    }
    public void resetCheckData()
    {
        checkpointLevelProgress = "";
		checkpointValue = 0;
        hasyellow = false;
        hasblue = false;
        hasred = false;
        checkPointState = 0;
		checkPointTimeCounter = 0;
        checkpointFloppies = 0;
        CheckpointSausages = 0;
        checkPointCoins = 0;
        CheckpointScore = 0;
        checkpointCheat = false;
        parTime = 0;
		startInSub = false;
		savedCamPos = Vector3.zero;
        savedTimeClock = 0;
        savedTrackName = "";
        savedTrackPoint = 0;
        savedTrackBeatDelay = 0;
        //if(specialData!=0)print("Reset special flag");
        specialData = 0;
    }
    public void resetValues()
    {
        resetCheckData();
        if(!allClear)
        evalAllClear();
		playCutscene=true;
    }
    public void resetDS()
    {
        clearedUnbeatenLevel = false;
        leavingLevel = false;
        allClear = false;
        resetValues();
    }
    public void resetStats()
    {
        lives = 5;
        coins = 0;
        score = 0;
    }
    void setSaveIcon()
    {
        gameSaveIcon o = Instantiate(saveIcon,transform.position,Quaternion.identity).GetComponent<gameSaveIcon>();
        o.assignData(this);
        o.startMeasure();
    }
    public void setTextPopup(string text)
    {
        gameSaveIcon o = Instantiate(saveIcon,transform.position,Quaternion.identity).GetComponent<gameSaveIcon>();
        o.assignData(this);
        o.setText(text);
        o.display();
    }
    public IEnumerator saveData(bool unFreeze)
	{
        saving = true;
        setSaveIcon();
		Time.timeScale = 0;
		string save = "";
		save = ";1"+'/'+'\n'
		+currentWorld+'/'+'\n'
		+totalCompletedLevels+'/'+'\n'
		+score+'/'+'\n'
		+lives+'/'+'\n'
		+Mathf.Clamp(coins,0,999)+'/'+'\n'
		+sausages+'/'+'\n'
		+floppies+'/'+'\n'
		+playerState+'/'+'\n'
		+storedItem+'/'+'\n'
		+lastLoadedLevel+'/'+'\n'
		+lastLoadedWorld+'/'+'\n'
		+worldProgression+'/'+'\n'
		+AndreMissionProgress+'/'+'\n'
		+DjoleMissionProgress+'/'+'\n'
		+MiroslavMissionProgress+'/'+'\n'
        +mode+'/'+'\n'
        +infiniteLives+'/'+'\n'
        +difficulty+'/'+'\n'
		+"="+'\n'+";2"+'/'+'\n';
		for(int i = 0; i<levelProgress.Length;i++)
		{
			save+=levelProgress[i]+'/'+'\n';
		}
        save+="="+'\n'+";3"+'/'+'\n';
        for(int i = 0;i<cellData.Length;i++)
        {
            if(cellData[i]==true)
            save+='1';
            else save+='0';
        }
		save+='\n'+"="+'/'+'\n'+"END OF SAVE FILE";
		//Debug.Log(save);
        PlayerPrefs.SetString("Save"+(saveFileID+1).ToString(),save);
		/*switch(saveFileID)
		{
			default:PlayerPrefs.SetString("Save1",save);break;
			case 1: PlayerPrefs.SetString("Save2",save);break;
			case 2: PlayerPrefs.SetString("Save3",save);break;
		}*/
		//Debug.Log("Data saved");
        yield return 0;
        if(unFreeze)
        Time.timeScale = 1;
        saving = false;
	}
    public int skipsRemaining()
    {
        int skipsFound = 0;
        //go through all levels in the array
        for(int i = 0;i<levelProgress.Length;i++)
        {
            if(levelProgress[i][0]=='S')
            skipsFound++;
        }
        skipsLeft = 3-skipsFound;
        if(skipsLeft<0)skipsLeft = 0;
        return skipsLeft;
    }
}
