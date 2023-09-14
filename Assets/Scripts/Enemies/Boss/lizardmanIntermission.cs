using System.Collections;
using UnityEngine;

public class lizardmanIntermission : MonoBehaviour
{
    ParticleSystem pSystem;
    GameObject[] cokes = new GameObject[2];
    GameObject[] activeCokes = new GameObject[2];
    lizardmanBossMaster bossMaster;
    GameObject[] bosses = new GameObject[3];
    public Animator anim;
    int currentPhase = 0;
    // Start is called before the first frame update
    void Start()
    {
        if(anim==null)
        {
            anim = GetComponent<Animator>();
            bossMaster = transform.parent.GetComponent<lizardmanBossMaster>();
            pSystem = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
            cokes[0] = transform.GetChild(0).GetChild(1).gameObject;
            cokes[1] = transform.GetChild(0).GetChild(1).gameObject;
            bosses[0] = transform.parent.GetChild(3).gameObject;
            bosses[1] = transform.parent.GetChild(4).gameObject;
            bosses[2] = transform.parent.GetChild(5).gameObject;
        }
    }
    IEnumerator waitSpriteEnable()
    {
        yield return 0;
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void enable(int phase)
    {
        currentPhase = phase;
        if(anim==null)Start();
        if(phase==1) transform.position = new Vector3(0,3,0);
        if(phase==2)
        { 
            transform.position = bosses[1].transform.GetChild(0).position;
        }
        else if (phase==3)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.position = new Vector3(bosses[2].transform.GetChild(0).position.x,3,0);
            if(transform.position.x<0)
            {
                transform.localScale=new Vector3(-1,1,1);
                transform.GetChild(1).GetComponent<SpriteRenderer>().flipX=true;
            }
        }
        gameObject.SetActive(true);
        anim.SetInteger("phase",phase);
        if(phase==2)anim.SetTrigger("teleport");
        else if(phase==3)StartCoroutine(waitSpriteEnable());
    }
    public void spawnCoke()
    {
        bossMaster.playSoundStatic(24);
        if(pSystem.isPlaying)pSystem.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
        pSystem.Play();
        MovementAI ai;
        if(activeCokes[0]==null)
        {
            activeCokes[0] = Instantiate(cokes[0],transform.GetChild(0).position,Quaternion.identity);
            activeCokes[0].transform.name = "Cola_item";
            activeCokes[0].SetActive(true);
            ai = activeCokes[0].GetComponent<MovementAI>();
            ai.direction = -1;
            ai.jump(false);
        }
        if(activeCokes[1]==null)
        {
            activeCokes[1] = Instantiate(cokes[1],transform.GetChild(0).position,Quaternion.identity);
            activeCokes[1].transform.name = "Cola_item";
            activeCokes[1].SetActive(true);
            ai = activeCokes[1].GetComponent<MovementAI>();
            ai.jump(false);
        }
    }
    public void playSteam()
    {
        bossMaster.playSteam(transform.position);
    }
    public void playSteamFromChild()
    {
        bossMaster.playSteam(transform.GetChild(0).position);
    }
    public void setPhase(int phase)
    {
        //if(anim==null)Start();
        currentPhase = phase;
        //anim.SetInteger("phase",phase);
    }
    public void setIntro(int i)
    {
        if(anim==null)Start();
        if(i==0&&bossMaster.pScript.pSprites.state>=2)
        {
            anim.SetTrigger("throw");
            transform.parent.GetChild(7).gameObject.SetActive(true);
        }
        else anim.SetTrigger("intro");
    }
    public void disable()
    {
        if(currentPhase==0)
        {
            bossMaster.pScript.goToCutsceneMode(false);
            transform.parent.position = Vector3.zero;
            transform.GetChild(0).localPosition = Vector3.zero;
            bosses[0].SetActive(true);
        }
        if(currentPhase==1)
        {
            bosses[1].SetActive(true);
        }
        else if(currentPhase==2)
        {
            bosses[2].SetActive(true);
        }
        gameObject.SetActive(false);
    }
    public void teleToSpot()
    {
        //print("Phase: "+currentPhase);
        transform.position = new Vector3(0,3,0);
    }
    public void playSound(int ID)
	{
		bossMaster.playSound(ID,transform.position);
	}
    public void spawnNPC()
    {
        bossMaster.activateNPC();
        gameObject.SetActive(false);
    }
    public void makePlayerLook()
    {
        bossMaster.playerLook();
    }
    public void makePlayerGoal()
    {
        bossMaster.playerGoal();
    }
    public void cutsceneStart()
    {
        bossMaster.npcCutsceneEvent();
    }
}
