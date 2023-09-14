using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusBoardScript : MonoBehaviour
{
    dataShare DataS;
    int[] worldLevelOffsets = new int[]{0,4,10,16,22,28,34,40};
    int[] activeCells = new int[]{4,6,6,6,6,6,6,1};
    int currentWorld = 0;
    int maxWorld = 0;
    int lastActiveCells = -1;
    Transform t;
    TextMeshProUGUI worldName;
    GameObject[] cells = new GameObject[6];
    TextMeshProUGUI[] levelNames = new TextMeshProUGUI[6];
    TextMeshProUGUI[] levelTimes = new TextMeshProUGUI[6];
    Transform[] sausageHolders = new Transform[6];
    Image[] diskRenderers = new Image[6];
    public Sprite[] sprites; //0 = empty sausage, 1 = full sausage, 2 = empty disk, 3 = full disk
    public bool finishedUpdating = false;
    string pressedString = "";
    string[] inputStrings = {"Left", "Alt Left", "Right", "Alt Right"};
    Image[] arrows = new Image[2];
    public Color[] arrowColors = new Color[]{Color.white,Color.yellow};
    [HideInInspector]
    public PlayerScript pScript;
    public bool ready = false;
    Transform master;
    GameData data;
    public AudioClip[] clips;
    public Sprite[] altSigns = new Sprite[3];
    //Handle inputs in a second script

    // Start is called before the first frame update
    public void Initialize()
    {
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        if(dataShare.totalCompletedLevels==0)
        {
            Destroy(gameObject);
            return;
        }
        pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
        master = transform.GetChild(1).GetChild(0);
        worldName = master.GetChild(1).GetComponent<TextMeshProUGUI>();
        Transform levelCellsHold = master.GetChild(2);
        arrows[0] = worldName.transform.GetChild(0).GetComponent<Image>();
        arrows[1] = worldName.transform.GetChild(1).GetComponent<Image>();
        data = GameObject.Find("_GM").GetComponent<GameData>();

        if(DataS.mode!=1)
        {
            if(dataShare.totalCompletedLevels>=42&&DataS.sausages>=84)
            GetComponent<SpriteRenderer>().sprite = altSigns[0]; //gold ceda sign
        }
        else
        {
            if(dataShare.totalCompletedLevels>=42&&DataS.sausages>=84)
            {
                bool allBought = true;
                for(int i = 0;i<29;i++)
                {
                    if(!DataS.cellData[i])
                    {
                        allBought = false;
                        break;
                    }
                }
                GetComponent<SpriteRenderer>().sprite = altSigns[allBought ? 2:1]; //gold playuh sign
            }
            else GetComponent<SpriteRenderer>().sprite = altSigns[1]; //normal playuh sign
        }

        //pScript.goToCutsceneMode(true);

        //assign all
        for(int i = 0;i<levelCellsHold.childCount;i++)
        {
            Transform cur = levelCellsHold.GetChild(i);
            cells[i] = cur.gameObject;
            levelNames[i]=cur.GetChild(0).GetComponent<TextMeshProUGUI>();
            levelTimes[i]=cur.GetChild(3).GetComponent<TextMeshProUGUI>();
            sausageHolders[i] = cur.GetChild(2);
            diskRenderers[i] = cur.GetChild(1).GetComponent<Image>();
        }
        if(dataShare.totalCompletedLevels==42)//all unlocked
        {
            maxWorld = 8;
        }
        else maxWorld = DataS.worldProgression;

        if(maxWorld<=1)
        {
            arrows[0].enabled = false;
            arrows[1].enabled = false;
        }
        //UpdateLevels();
    }
    void playSound(int ID)
    {
        data.playStaticSoundOverwrite(clips[ID]);
    }
    void playAppear(bool appear)
    {
        if(apCor!=null)StopCoroutine(apCor);
        apCor = StartCoroutine(appearAnim(appear));
    }
    Coroutine apCor;
    IEnumerator appearAnim(bool appear)
    {
        playSound(appear ? 0:2);
        if(appear)pScript.goToCutsceneMode(appear);
        else ready = false;

        float progress = 0;
        float targetPoint = appear ? 0 : -450,
        startPoint = appear ? -450 : 0;
        master.localPosition = new Vector3(0,startPoint,0);
        if(appear)master.gameObject.SetActive(true);
        while(progress<1)
        {
            progress+=Time.deltaTime*3;
            master.localPosition = Vector3.Slerp(master.localPosition,new Vector3(0,targetPoint,0),progress);
            yield return 0;
        }
        //yield return new WaitForSeconds(0.5f);

        if(!appear)pScript.goToCutsceneMode(appear);
        else ready = true;
        if(!appear)master.gameObject.SetActive(false);
    }
    public void toggleEnable(bool activate)
    {
        this.enabled = activate;
        if(activate)
        UpdateLevels();
        playAppear(activate);
    }
    public void UpdateLevels()
    {
        finishedUpdating = false;
        //1 set how many active levels (0 = 4 active, rest = 6, special = 1)
        int curActiveCells = activeCells[currentWorld];

        if(curActiveCells!=lastActiveCells) //update which cells are active if last cell length is different than current cell length
        {
            for(int i = 0;i<cells.Length;i++)
            {
                if(i<curActiveCells)
                    cells[i].SetActive(true);

                else cells[i].SetActive(false);
            }
        }
        //set world title
        worldName.text = (DataS.worldNames[Mathf.Clamp(currentWorld,0,6)]+" world").ToUpper();
        int startLevelOffset = currentWorld == 7 ? 6 : 0;
        bool firstUnCleared = true; //show first uncleared level
        //update all active cells
        for(int i = 0;i<curActiveCells;i++)
        {
            levelNames[i].color = Color.white;
            int pointer = i+worldLevelOffsets[currentWorld]+1;
            //get current level cell
            string lvlCur = DataS.levelProgress[pointer];

            //check if level unbeaten
            Transform ct = cells[i].transform;
            if(lvlCur.StartsWith("N;")&&!firstUnCleared)
            {
                for(int x = 1;x<ct.childCount;x++)
                {
                    ct.GetChild(x).gameObject.SetActive(false);
                }
                levelNames[i].text = "<align=\"center\">???";
                continue; //stop loop segment here
            }
            //else show all parts normally
            else
            {
                if(lvlCur.StartsWith("N;"))
                    firstUnCleared = false;
                    
                for(int x = 1;x<ct.childCount;x++)
                {
                    ct.GetChild(x).gameObject.SetActive(true);
                }
            }

            //set current level title
            levelNames[i].text = Mathf.Clamp(currentWorld+1,1,7)+"-"+(i+1+startLevelOffset)+" \""+DataS.levelNames[pointer]+"\"";
            if(lvlCur.StartsWith("D;"))levelNames[i].color = new Color(0.8f,0f,0f,1);

            //hide sausage/floppy data if doesn't exist
            if(lvlCur.Length<=2||lvlCur.Contains(";;"))
            {
                ct.GetChild(1).gameObject.SetActive(false);
                ct.GetChild(2).gameObject.SetActive(false);
            }
            else //else display and update
            {
                ct.GetChild(1).gameObject.SetActive(true);
                ct.GetChild(2).gameObject.SetActive(true);

                if(lvlCur.Contains("C")) //show/hide disk
                {
                    diskRenderers[i].sprite = sprites[3];
                }
                else diskRenderers[i].sprite = sprites[2];

                //show sausages
                for(int x = 2;x<5;x++)
                {
                    if(lvlCur[x]==';')break;
                    else
                    {
                        sausageHolders[i].GetChild(x-2).GetComponent<Image>().sprite = sprites[lvlCur[x]=='1' ? 1 : 0];
                    }
                }
            }

            //time display
            if(lvlCur.Contains("S")) //if skipped level, set time as skipped and continue
            {
                levelTimes[i].text = ("Skip").ToUpper();
                continue;
            }
            //get time string: read lvl cur backwards until semicolon
            string timeDisp = "";
            for(int z = lvlCur.Length-1;z>=0;z--)
            {
                if(lvlCur[z]==';') //found time start index
                {
                    timeDisp = lvlCur.Substring(z+1);
                    break;
                }
            }
            //print("Time disp: "+timeDisp);
            if(timeDisp!=""&&!lvlCur.Contains("N;"))
            {
                int timeAsSecs = int.Parse(timeDisp);
                float minutes = Mathf.Floor(timeAsSecs/60),
                seconds = (timeAsSecs%60);
                levelTimes[i].text=minutes.ToString("00")+":"+seconds.ToString("00"); //translate and display time
            }
            else levelTimes[i].text=("No time").ToUpper();
        }
        finishedUpdating = true;
    }
    void Update()
    {
        if(pressedString==""&&maxWorld>1)
        keyPressDetect();
    }
    Coroutine waitCor;
    IEnumerator relWaitCor()
    {
        Image usedArr = null;
        if(pressedString.Contains("Left"))
        {
            usedArr = arrows[1];
        }
        else usedArr = arrows[0];
        playSound(1);

        usedArr.color = arrowColors[1];
        yield return new WaitUntil(()=>SuperInput.GetKeyUp(pressedString));
        pressedString = "";
        usedArr.color = arrowColors[0];
    }
    void playCor()
    {
        if(waitCor!=null)StopCoroutine(waitCor);
        waitCor = StartCoroutine(relWaitCor());
    }
    void keyPressDetect()
    {
        foreach(string s in inputStrings)
        {
            if(SuperInput.GetKeyDown(s))
            {
                pressedString = s;
                if(pressedString.Contains("Left"))
                {
                    currentWorld = (int)Mathf.Repeat(currentWorld-=1,maxWorld);
                    UpdateLevels();
                    playCor();
                }
                else if(pressedString.Contains("Right"))
                {
                    currentWorld = (int)Mathf.Repeat(currentWorld+=1,maxWorld);
                    UpdateLevels();
                    playCor();
                }
                break;
            }
        }
    }
}
