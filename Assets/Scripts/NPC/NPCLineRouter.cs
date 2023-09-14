using System.Collections;
using UnityEngine;

public class NPCLineRouter : MonoBehaviour
{
    NPCScript n;
    dataShare DataS;
    public string who;
    public int startLineCompleted = 0,hasEnoughLine = 0,extraLine = 0;
    // Start is called before the first frame update
    void Start()
    {
        n = GetComponent<NPCScript>();
        n.nline = this;
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        if(DataS.mode!=1)
        {
            if(who!="babushka")
            {
                if(DataS.levelProgress[34].Contains("N"))gameObject.SetActive(false);
            }
        }
    }
    public void checkStatus()
    {
        if(DataS.mode!=1)
        {
            switch(who)
            {
                default:
                    if(DataS.AndreMissionProgress==1) {n.startLine = startLineCompleted; return;}
                break; //andre
                case "djole":
                    if(DataS.DjoleMissionProgress==1) {n.startLine = startLineCompleted; return;}
                break; //djole
                case "miroslav":
                    if(DataS.MiroslavMissionProgress==1) {n.startLine = startLineCompleted; return;}
                break; //miroslav
                case "babushka":
                int t = dataShare.totalCompletedLevels;
                if(t>=41){n.startLine = hasEnoughLine; return;} //post playuh
                else if(DataS.worldProgression==7&&t<41){n.startLine = extraLine; return;} //post mega satan pre playuh
                else if(t>=35){n.startLine = startLineCompleted; return;} //post mega satan
                else return; //dont change
                case "emily":
                if(dataShare.totalCompletedLevels==41) //finished main game
                {
                    n.startLine = hasEnoughLine;
                }
                else if(dataShare.totalCompletedLevels>=42) // finished all levels
                {
                    n.startLine = extraLine;
                }
                break;
            }
            if((DataS.sausages-DataS.spentSausages)>=15){n.option1StartLine = hasEnoughLine; return;};
        }
        else
        {
            if(who=="emily")
            {
                if(dataShare.totalCompletedLevels<41)
                    n.startLine = startLineCompleted; //732
                if(dataShare.totalCompletedLevels==41)
                    n.startLine = hasEnoughLine; //726
                else if(dataShare.totalCompletedLevels>=42) // finished all levels
                {
                    n.startLine = extraLine;
                }
            }
        }
    }
    public void givePiece()
    {
        //print("Getting apple piece from "+who);
        switch(who)
        {
            default:
                DataS.AndreMissionProgress=1;
            break; //andre
            case "djole":
                DataS.DjoleMissionProgress=1;
            break; //djole
            case "miroslav":
                DataS.MiroslavMissionProgress=1;
            break; //miroslav
            case "babushka":
            break;
        }
        if(who!="babushka")
        {
            DataS.playerState = GameObject.Find("Player_main").GetComponent<playerSprite>().state;
            DataS.storedItem = GameObject.Find("_GM").GetComponent<GameData>().storedItemID;
            StartCoroutine(DataS.saveData(true));
            if(DataS.AndreMissionProgress==1&&DataS.MiroslavMissionProgress==1&&DataS.DjoleMissionProgress==1)
            {
                //print("Unlocked world 7");
                n.TBScript.resumeInCutscene = true;
                StartCoroutine(specialWorldUnlock());
            }
        }
    }
    IEnumerator specialWorldUnlock()
    {
        GameObject tb = n.TBScript.gameObject;
        MGCameraController cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        GameData data = GameObject.Find("_GM").GetComponent<GameData>();
        yield return new WaitUntil(()=> !DataS.saving);
        yield return new WaitUntil(()=>!tb.activeInHierarchy);
        cam.fadeScreen(true);
		yield return new WaitUntil(()=>cam.fadeAnim>=1f);
        data.stopAllMusic();
        data.audioEffectsToggle(0);
        DataS.loadWorldWithLoadScreen(7);
    }
}
