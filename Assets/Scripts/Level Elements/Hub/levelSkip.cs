using System.Collections;
using UnityEngine;

public class levelSkip : MonoBehaviour
{
    public int noStockLine = 98,cantSkipLine = 101,noLevelsLeftLine = 104,noLevelsWithSkip = 107;
    public int[] unskippableLevels = {4,10,16,22,28,34,40};
    public Sprite empty;
    NPCScript npc;
    TextBox TBScript;
    Animation anim;
    GameData data;
    dataShare dataS;
    IEnumerator loadTB()
    {
        yield return 0;
        determineStartLine();
        yield return new WaitUntil(()=>npc.TBScript!=null);
        TBScript = npc.TBScript;
        data = npc.data;
    }
    // Start is called before the first frame update
    void Start()
    {
        npc = GetComponent<NPCScript>();
        anim = transform.GetChild(0).GetComponent<Animation>();
        dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        StartCoroutine(loadTB());
    }
    void determineStartLine()
    {
        if(dataS.skipsRemaining()==0)
        {
            //print("No skips left");
            npc.npcSprites[0]=empty;
            npc.npcSprites[1]=empty;
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = empty;
            npc.startLine = noStockLine;
            return;
        }
        //else print("Skips left: "+dataS.skipsLeft);
        //check if level is unskippable
        for(int i = 0;i<unskippableLevels.Length;i++)
        {
            if(dataShare.totalCompletedLevels==unskippableLevels[i])
            {
                npc.startLine = cantSkipLine;
                //print("On unskippable level");
                return;
            }
        }
        //scan if any levels remain
        int max = 33;
        if(dataShare.totalCompletedLevels>=35&&dataS.worldProgression>=7)
        {
            //print("Full list scan, total completed: "+dataS.totalCompletedLevels+" world prog: "+dataS.worldProgression);
            max = dataS.levelProgress.Length;
        }
        bool foundSkip = false;
        for(int i = 0;i<max;i++)
        {
            if(dataS.levelProgress[i][0]=='N')
            {
                //print("Can buy skip");
                return;
            }
            else if(dataS.levelProgress[i][0]=='S')
            {
                foundSkip = true;
            }
        }
        //if here, didn't find unfinished levels, check if found skipped
        if(foundSkip) npc.startLine = noLevelsWithSkip;
        //else all levels are cleared
        else npc.startLine = noLevelsLeftLine;
    }
    public void shopBlockHit()
    {
        anim.Play("Block_BounceNoEventanim");
        //has enough to buy
        if(data.coins>=999)
        {
            //open message box here
            TBScript.option1StartLine = npc.option1StartLine;
			TBScript.option2StartLine = npc.option2StartLine;
            npc.infoBlockHit();
        }
        else data.playSound(96,transform.position); //not enough money
    }
    IEnumerator fadeOut()
    {
        MGCameraController cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        PlayerScript pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
        pScript.goToCutsceneMode(true);
        dataS.StartCoroutine(dataS.saveData(true));
        yield return 0;
        yield return new WaitUntil(()=>TBScript.eventInt>=1);
        if(dataS.lastLoadedWorld!=0)
        {
            TBScript.resumeInCutscene = true;
            pScript.goToCutsceneMode(true);
            cam.fadeScreen(true);
            yield return new WaitUntil(()=>cam.fadeAnim>=1&&!dataS.saving);
            data.stopAllMusic();
            data.audioEffectsToggle(0);
            //load world here
			dataS.loadWorldWithLoadScreen(dataS.lastLoadedWorld);
        }
        else
        {
            yield return new WaitUntil(()=>!dataS.saving);
            pScript.goToCutsceneMode(false);
        }
		/*else 
        {
            dataS.resetCheckData();
            dataS.loadSceneWithLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }*/
    }
    // Update is called once per frame
    void Update()
    {
			if(TBScript!=null&&TBScript.gameObject.activeInHierarchy&&TBScript.confirmOptionforOutside)
			{
				TBScript.confirmOptionforOutside = false;
				switch(TBScript.currentOption)
				{
					default: break;
                    //buy skip here
					case 0:
                        //print("Skip bought");
                        data.playSound(95,transform.position);
                        data.addCoin(-999,false);
                        data.saveCoin();
                        string s = dataS.levelProgress[dataShare.totalCompletedLevels];
                        dataS.levelProgress[dataShare.totalCompletedLevels] = "S"+s.Substring(1);
                        dataShare.totalCompletedLevels++;
                        dataS.saveData(true);
                        npc.npcSprites[0]=empty;
                        npc.npcSprites[1]=empty;
                        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = empty;
                        StartCoroutine(fadeOut());
                        data.playSoundStatic(101);
					break;
				}
			}
    }
}
