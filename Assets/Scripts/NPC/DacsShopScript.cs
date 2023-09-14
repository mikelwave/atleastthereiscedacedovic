using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class DacsShopScript : MonoBehaviour
{
    string[] songList = {
        "A Medley of Copyright Infridgement (Old)",
        "97",
        "A Kazoo Appears",
        "Here's Proof!",
        "I Wish I Could Count!",
        "Insane in the Sausage",
        "KING OF THE BRITAINS",
        "LOL funny 2014 meme disease",
        "Mash",
        "Onslaught of Clubshit",
        "Rattle Those Bunions",
        "Ravioli",
        "Mystery Factory",
        "Space Bicycle",
        "Why You Gotta Be So Rude"
    };
    string[] videoList = 
    {
        "The Wait",
        "Alternate Intro",
        "Barbarian",
        "Greed",
        "British World",
        "Whater",
        "IMJ",
        "Creepypasta World",
        "Zalgo",
        "Ending",
        "Special World",
        "Alternate Ending",
        "Weird Ending"
    };
    public int[] songPrices = new int[15];
    public int[] videoPrices = new int[13];
    NPCScript npc;
    TextBox TBScript;
    string[] storeList;
    TextMeshProUGUI text;
    Transform pointer,storeCanvas;
    int maxSelection = 0;
    int selection = 0,boxSel = 0; //if selection is bigger than 10 and going down, redraw with offset selection-10
    // Start is called before the first frame update
    bool canSelect = false;
    int offset = 0,storeType = 0;
    dataShare DataS;
    Transform mainBox;
    MenuScript menu;
    VideoPlayer vidplayer;
    cutsceneEvent cEvent;
    public VideoClip[] clips;
    public AudioClip[] songs;
    public AudioClip[] sounds;
    GameData Data;
    MGCameraController cam;
    bool toSave = false;
    //dataS cells:
    /*
    0 - Whether talked for the first time
    1-12 - videos
    13-28 - songs
    */

    void Start()
    {
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        if(DataS.mode!=1)
        {
            Destroy(gameObject);
            return;
        }
        npc = transform.GetChild(0).GetComponent<NPCScript>();

        Data = GameObject.Find("_GM").GetComponent<GameData>();
        TBScript = GameObject.Find("Textbox_Canvas").transform.GetChild(0).GetComponent<TextBox>();
        storeCanvas = transform.GetChild(1);
        text = storeCanvas.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        pointer = text.transform.parent.GetChild(2);
        mainBox = transform.GetChild(1).GetChild(0);
        if(!DataS.cellData[0])
        {
            DataS.StartCoroutine(setStart());
        }
        menu = GameObject.Find("PauseCanvas").GetComponent<MenuScript>();
        vidplayer = GameObject.Find("Pixels").transform.GetChild(2).GetComponent<VideoPlayer>();
        cEvent = vidplayer.transform.GetComponent<cutsceneEvent>();
        cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        calculateSpent();
    }
    void calculateSpent()
    {
        int spent = 0;
        for(int i = 0;i<songPrices.Length;i++)
        {
            if(DataS.cellData[i+14])
            {
                spent+=songPrices[i];
            }
        }
        DataS.spentSausages = spent;
        //print("Spent: "+spent);
        Data.updateSausageDisplay(0);
    }
    void playSound(int ID)
    {
        Data.playUnlistedSound(sounds[ID]);
    }
    IEnumerator setStart()
    {
        yield return 0;
        npc.startLine = 739;
    }
    void openStore(int mode) //0 = videos, 1 = songs
    {
        transform.GetChild(1).GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>().text = "Exit: "+getBackKeyName();
        selection = 0;
        boxSel = 0;
        offset = 0;
        //assign store items
        storeType = mode;
        drawTextList();
        text.ForceMeshUpdate();
        storeCanvas.gameObject.SetActive(true);
        openCloseMethod(true);
    }
    void openCloseMethod(bool open)
    {
        if(openCloseCor!=null)StopCoroutine(openCloseCor);
        openCloseCor = StartCoroutine(openClose(open));
    }
    void drawTextList()
    {
        if(storeType==1) //songs
        {
            maxSelection = songList.Length;
            storeList = new string[songList.Length];
            for(int i = 0;i<storeList.Length;i++)
            {
                if(!DataS.cellData[i+videoList.Length+1])
                storeList[i]=songList[i]+" ("+songPrices[i]+")";
                else storeList[i]=songList[i];
            }
        }
        else //videos
        {
            maxSelection = videoList.Length;
            storeList = new string[videoList.Length];
            for(int i = 0;i<storeList.Length;i++)
            {
                if(!DataS.cellData[i+1])
                storeList[i]=videoList[i]+" ("+videoPrices[i]+")";
                else storeList[i]=videoList[i];
            }
        }
        drawStoreText(0);
    }
    Coroutine openCloseCor;
    IEnumerator openClose(bool open)
    {
        canSelect = false;
        float progress = 0;
        float targetSize = 0;
        if(open)
        {
            mainBox.localScale = new Vector3(32,0,32);
            targetSize = 32;
        }
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*2;
            mainBox.localScale = new Vector3(32,Mathf.Lerp(mainBox.localScale.y,targetSize,progress),32);
            yield return 0;
        }
        mainBox.localScale = new Vector3(32,targetSize,32);
        if(!open)
        {
            menu.pauseLock = false;
            if(toSave)
            {
                DataS.lastLoadedWorld = 0;
                DataS.StartCoroutine(DataS.saveData(true));
            }
        }
        else
            canSelect = true;
        toSave = false;
        openCloseCor = null;
    }
    void drawStoreText(int toAdd)
    {
        if(toAdd!=0)
        playSound(0);
        selection= (int)Mathf.Repeat(selection+toAdd,maxSelection);
        int sel2 = 0;
        if(selection==0)
        {
            offset = 0;
            sel2 = 0;
        }
        else if(selection==maxSelection-1)
        {
            offset = (selection-10)+1;
            sel2 = 9;
        }
        else
        {
            boxSel+=toAdd;
            sel2 = (int)Mathf.Clamp(boxSel,1,8); //shift on second last on either side
            if(sel2>boxSel||sel2<boxSel) //push down/up
            {
                offset = (int)Mathf.Clamp(offset+toAdd,0,maxSelection-10);
            }
        }
        boxSel = sel2;
        string s = "";
        //write to text box 10 lines
        for(int i = 0;i<10;i++)
        {
            if(i==boxSel)
            {
                s+="<color=#fcba03>"+storeList[i+offset]+'\n'+"</color>";
            }
            else
            s+=storeList[i+offset]+'\n';
        }
        text.text = s;
        pointer.localPosition=new Vector3(-7.75f,2.61f-(0.565f*(boxSel)),0);
    }
    Coroutine movementCor;
    IEnumerator IMovementCor(int dir,string pressedKeyName)
    {
        int frameWait = 16;
        drawStoreText(dir);
        while(SuperInput.GetKey(pressedKeyName))
        {
            frameWait--;

            if(frameWait<=0)
            {
                drawStoreText(dir);
                frameWait = 4;
            }
            yield return 0;
        }
        movementCor = null;
    }
    string getBackKeyName()
    {
        string s = SuperInput.GetKeyName("Start");
        if(s.Contains("Joy"))
        return changeWord(s);
        else return s;
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
    Coroutine choiceConfirmWaitCor;
    IEnumerator choiceConfirmWait()
    {
        yield return new WaitUntil(()=>menu.choiceConfirmed);
        menu.partEnableDisable(false);
        if(!menu.choice)
        canSelect = true;
        else
        {
            playItem(selection);
        }
    }
    void buyItem()
    {
        bool b = false,boughtItem = false;
        if(storeType==1)//songs
        {
            b = DataS.cellData[selection+videoList.Length+1];
            if(!b) //item not yet bought
            {
                if(Data.updateSausageDisplay(-songPrices[selection]))
                {
                    boughtItem = true;
                    DataS.cellData[selection+videoList.Length+1] = true;
                    drawTextList();
                    toSave = true;
                    playSound(1);
                }
                else
                {
                    canSelect = true; //not enough sausages
                    playSound(2);
                }
            }
        }
        else
        {
            b = DataS.cellData[selection+1];
            if(!b)
            {
                if(Data.floppies>=videoPrices[selection])
                {
                    Data.addFloppy(-videoPrices[selection],false);
                    boughtItem = true;
                    DataS.cellData[selection+1] = true;
                    drawTextList();
                    toSave = true;
                    playSound(1);
                }
                else
                {
                    canSelect = true; //not enough floppies
                    playSound(2);
                }
            }
        }

        if(boughtItem)//bought
        {
            canSelect = false;
            menu.pauseLock = true;
            menu.partEnableDisable(true);
            menu.displayMessage(5+storeType);

            if(choiceConfirmWaitCor!=null)StopCoroutine(choiceConfirmWaitCor);
            choiceConfirmWaitCor = StartCoroutine(choiceConfirmWait());
        }
        else if(b)//item already bought
        {
            playItem(selection);
        }
    }
    IEnumerator cutscenePlay()
    {
        canSelect = false;
        playSound(3);
        GameObject o = cEvent.gameObject;
        Data.fadeMusic(false);
        cam.fadeScreen(true);
        yield return new WaitUntil(()=>cam.fadeAnim>=1);
        DataS.playCutscene = true;
        cEvent.Start();
        Data.pauseMusic(true);
        o.SetActive(true);
        yield return new WaitUntil(()=>!o.activeInHierarchy);
        cam.fadeScreen(false);
        Data.pauseMusic(false);
        Data.fadeMusic(true);
        yield return new WaitUntil(()=>cam.fadeAnim<=0);
        canSelect = true;
    }
    void playItem(int s)
    {
        if(storeType==1) //song
        {
            Data.setCustomMusic(songs[selection]);
            canSelect = true;
        }
        else //cutscene
        {
            cEvent.subtitleFrames = new List<long>();
            cEvent.subtitlesFile = null;
            vidplayer.clip = clips[selection];
            cEvent.ignoreEmpty = true;
            StartCoroutine(cutscenePlay());
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(npc.talking==!DataS.cellData[0])
        {
            DataS.cellData[0] = true;
            npc.startLine = 745;
        }
        if(npc.talking&&TBScript.confirmOptionforOutside)
        {
            TBScript.confirmOptionforOutside=false;
            //print("Selection: "+TBScript.currentOption);
            switch(TBScript.currentOption)
            {
                default:
                openStore(TBScript.currentOption);
                break;
                case 2:
                break;
            }
        }
        if(canSelect&&Time.timeScale!=0)
        {
            if(movementCor==null&&SuperInput.GetKeyDown("Start"))
            {
                canSelect = false;
                TBScript.skip();
                openCloseMethod(false);
            }
            if(SuperInput.GetKeyDown("Jump"))
            {
                buyItem();
            }
            if(SuperInput.GetKeyDown("Up"))
            {
                if(movementCor!=null)StopCoroutine(movementCor);
                movementCor = StartCoroutine(IMovementCor(-1,"Up"));
            }
            else if(SuperInput.GetKeyDown("Alt Up"))
            {
                if(movementCor!=null)StopCoroutine(movementCor);
                movementCor = StartCoroutine(IMovementCor(-1,"Alt Up"));
            }
            else if(SuperInput.GetKeyDown("Down"))
            {
                if(movementCor!=null)StopCoroutine(movementCor);
                movementCor = StartCoroutine(IMovementCor(1,"Down"));
            }
            else if(SuperInput.GetKeyDown("Alt Down"))
            {
                if(movementCor!=null)StopCoroutine(movementCor);
                movementCor = StartCoroutine(IMovementCor(1,"Alt Down"));
            }
            

        }
        if(!TBScript.gameObject.activeInHierarchy)
        {
            this.enabled = false;
        }
    }
}
