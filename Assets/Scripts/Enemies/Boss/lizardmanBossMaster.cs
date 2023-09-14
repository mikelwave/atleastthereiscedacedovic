using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lizardmanBossMaster : MonoBehaviour
{
    public PlayerScript pScript;
    public Transform player;
    bridgeTile[] bridgeTiles = new bridgeTile[9];
    GameObject steam;
    MGCameraController cam;
    public GameData data;
    List<Vector3Int> skeletilePositions;
    [HideInInspector]
    public bool canSpawnTiles = true;
    public AudioClip[] music = new AudioClip[2];
    public AudioClip[] sounds;
    public lizardmanIntermission lizInter;
    public GameObject NPC;
    TextBox textBox;
    public Sprite[] angrySprites;
    dataShare DataS;
    public Transform cutsceneQuad;
    public AudioSource[] overlays = new AudioSource[3];
    AudioSource vaSource;
    bool paused = false;
    public GameObject PauseMenu;
    [Space]
    public int startPhase = 0;
    Transform checkpointHold;
    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.Find("Player_main").transform;
        vaSource = GetComponent<AudioSource>();
        if(NPC==null)NPC = GameObject.Find("NPCLizardMan");
        cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        pScript = player.GetComponent<PlayerScript>();
        bridgeTiles[0] = transform.GetChild(0).GetChild(0).GetComponent<bridgeTile>();
        steam = transform.GetChild(1).gameObject;
        data = GameObject.Find("_GM").GetComponent<GameData>();
        skeletilePositions = new List<Vector3Int>();
        textBox = GameObject.Find("Textbox_Canvas").transform.GetChild(0).GetComponent<TextBox>();
        lizInter = transform.GetChild(6).GetComponent<lizardmanIntermission>();
        overlays[0] = transform.GetChild(9).GetComponent<AudioSource>();
        overlays[1] = transform.GetChild(10).GetComponent<AudioSource>();
        overlays[2] = transform.GetChild(11).GetComponent<AudioSource>();
        if(startPhase==0) startPhase = DataS.checkpointValue;

        lizInter.setPhase(startPhase);
        checkpointHold = GameObject.Find("Checkpoints").transform;
        NPC.GetComponent<Animator>().speed = 0.5f;
        for(int i = 1; i<bridgeTiles.Length; i++)
        {
            GameObject obj = Instantiate(bridgeTiles[0].gameObject,bridgeTiles[0].transform.position,Quaternion.identity);
            obj.transform.SetParent(transform.GetChild(0));
            bridgeTiles[i] = obj.GetComponent<bridgeTile>();
        }
        bool playIntro = DataS.playCutscene;
        if(playIntro) pScript.goToCutsceneMode(true);
        if(Application.isPlaying)
        cutsceneQuad.gameObject.SetActive(true);
        if(lizInter.gameObject.activeInHierarchy)
        {
            StartCoroutine(camFade(playIntro));
        }
        
    }
    public void bridgeTilesFall(Vector3Int pos)
    {
        for(int i = 0; i<bridgeTiles.Length;i++)
        {
            if(!bridgeTiles[i].gameObject.activeInHierarchy)
            {
                bridgeTiles[i].spawn(pos,false);
                break;
            }
        }
    }
    public void playSteam(Vector3 pos)
    {
        steam.transform.position = pos;
        steam.SetActive(true);
        playSoundStatic(0);
    }
    void Update()
    {
        if(pScript.dead&&overlays[0].gameObject.activeInHierarchy)
        {
            overlays[0].gameObject.SetActive(false);
            overlays[1].gameObject.SetActive(false);
            overlays[2].gameObject.SetActive(false);
        }
        if(PauseMenu.activeInHierarchy&&!paused)
        {
            paused = true;
            overlays[0].Pause();
            overlays[1].Pause();
            overlays[2].Pause();
        }
        else if(!PauseMenu.activeInHierarchy&&paused)
        {
            paused = false;
            overlays[0].UnPause();
            overlays[1].UnPause();
            overlays[2].UnPause();
        }
    }
    IEnumerator camFade(bool playIntro)
    {
        yield return new WaitUntil(()=>Time.timeScale!=0);
        if(!playIntro)
        {
            yield return new WaitUntil(()=>Time.timeScale!=0);
            StartCoroutine(data.musicIntro(music[0],music[1]));
            transform.GetChild(9).gameObject.SetActive(true);
            transform.GetChild(10).gameObject.SetActive(true);
            transform.GetChild(11).gameObject.SetActive(true);
            lizInter.setIntro(1);
        }
        else
        {
            lizInter.anim.SetBool("playComeOut",true);
            pScript.pauseMenu.enabled = false;
        }
        data.storeItem(1,true);
        cam.fadeScreen(false);
    }
    bridgeTile bridgeTileFind()
    {
        for(int i = 0; i<bridgeTiles.Length;i++)
        {
            if(!bridgeTiles[i].gameObject.activeInHierarchy)
            {
                return bridgeTiles[i];
            }
        }
        return null;
    }
    IEnumerator bridgeTilesRestore(int startInt)
    {
        for(int i = startInt; i<12;i++)
        {
            bridgeTileFind().spawn(new Vector3Int(-12+i,-5,0),true);
            bridgeTileFind().spawn(new Vector3Int(11-i,-5,0),true);
            if(startInt==0)
            yield return new WaitForSeconds(0.05f);
            else yield return new WaitForSeconds(0.01f);
        }
        //END HERE
        if(startInt>0)
        {
            yield return 0;
            yield return new WaitUntil(()=>pScript.grounded);
            pScript.pauseMenu.enabled = false;
            pScript.unCrouch();
            pScript.goToCutsceneMode(true);
            yield return new WaitForSeconds(3.7f);
            //print("Player: "+pScript.transform.position.x+" Enemy: "+lizInter.transform.GetChild(0).position.x);
            if(pScript.transform.position.x>=lizInter.transform.GetChild(0).position.x)
            {
                pScript.transform.localScale = new Vector3(-1,1,1);
            }
            else pScript.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(7f);
            cam.fadeScreen(true);
            yield return new WaitUntil(()=>cam.fadeAnim>=1f);
            data.echoInMainArea=false;
            if(data.mode!=1)
            {
                playSoundStatic(35);
                transform.GetChild(8).gameObject.SetActive(true);
                yield return new WaitForSeconds(1f);
                cam.fadeScreen(false);
                yield return new WaitForSeconds(8f);
            }
            bool newClear = false;
            if(data.currentLevelProgress!="")
			{
				char c = data.currentLevelProgress[0];
				if(c=='N')
					newClear = true;
                    
				if(c!='D')
                {
                    data.currentLevelProgress = data.currentLevelProgress.Remove(0, 1);
                    if(data.cheated||DataS.difficulty!=2)
                    data.currentLevelProgress = data.currentLevelProgress.Insert(0,"F");
                    else data.currentLevelProgress = data.currentLevelProgress.Insert(0,"D");
                }
                
                DataS.coins+=data.coins;
                data.saveLevelProgress(newClear,false);
                yield return new WaitUntil(()=> data.finishedSaving);
                DataS.resetValues();
            }
            if(newClear)DataS.loadWorldWithLoadScreen(6);
            else DataS.loadWorldWithLoadScreen(DataS.currentWorld);
            
        }
    }
    IEnumerator skeletilesClear()
    {
        yield return new WaitForSeconds(1f);
        for(int i = 0;i<skeletilePositions.Count;i++)
        {
            data.spawnSkeletileAtPoint(skeletilePositions[i],false);
            yield return new WaitForSeconds(0.05f);
        }
        skeletilePositions.Clear();
    }
    IEnumerator disableRespawn()
    {
        canSpawnTiles = false;
        data.forbidSkeletileRespawn = true;
        yield return new WaitForSeconds(7f);
        data.forbidSkeletileRespawn = false;
        canSpawnTiles = true;
        //print("can Spawn Tiles");
    }
    public void eraseSkeletiles()
    {
        StartCoroutine(skeletilesClear());
        StartCoroutine(disableRespawn());
    }
    public void restoreBridge()
    {
        StartCoroutine(bridgeTilesRestore(0));
    }
    public void restoreBridgeFinal()
    {
        StartCoroutine(bridgeTilesRestore(10));
    }
    public void shakeCam(float pwr,float amount)
    {
        cam.shakeCamera(pwr,amount);
    }
    public void spawnSkeletile(Vector3Int pos,bool appear)
    {
        skeletilePositions.Add(pos);
        data.spawnSkeletileAtPoint(pos,appear);
    }
    public void playSound(int ID,Vector3 pos)
    {
        if(ID==28)
        {
            data.spawnCheeseSplatterPointNoSound(transform.position-new Vector3(0,0.2f,0));
        }
        data.playUnlistedSoundPoint(sounds[Mathf.Clamp(ID,0,sounds.Length)],pos);
    }
    public void playSoundStatic(int ID)
    {
        data.playUnlistedSound(sounds[Mathf.Clamp(ID,0,sounds.Length)]);
    }
    public void spawnLizardman(int phase)
    {
        lizInter.enable(phase);
        if(phase>0&&phase<3)
        {
            print("Checkpoint "+(phase-1)+" reached");
            if(phase==2)
            {
                checkpointHold.GetChild(0).gameObject.SetActive(false);
            }
            checkpointHold.GetChild(phase-1).gameObject.SetActive(true);
        }
    }
    public void activateNPC()
    {
        NPC.SetActive(true);
    }
    public void playerLook()
    {
        StartCoroutine(pScript.idleLook());
    }
    public void playerGoal()
    {
        pScript.anim.SetTrigger("goal");
    }
    void playVA(int ID)
    {
        if(vaSource.isPlaying)vaSource.Stop();
        vaSource.clip = sounds[ID];
        vaSource.Play();
    }
    IEnumerator cutscene()
    {
        if(DataS.mode!=1)
        {
            yield return new WaitUntil(()=>textBox.eventInt==1);
            playVA(29);
            yield return new WaitUntil(()=>textBox.eventInt==2);
            playVA(30);
            yield return new WaitUntil(()=>textBox.eventInt==3);
            playVA(31);
            yield return new WaitUntil(()=>textBox.eventInt==4);
            NPC.GetComponent<NPCScript>().npcSprites=angrySprites;
            playVA(32);
            yield return new WaitUntil(()=>textBox.eventInt==5);
            playVA(33);
            yield return new WaitUntil(()=>!textBox.gameObject.activeInHierarchy);
        }
        else
        {
            NPC.GetComponent<BoxCollider2D>().enabled = false;
            yield return new WaitForSeconds(7f);
        }
        pScript.pauseMenu.enabled = false;
        pScript.goToCutsceneMode(true);


        spawnLizardman(startPhase);


        NPC.SetActive(false);
        lizInter.setIntro(startPhase);
        yield return new WaitForSeconds(0.5f);
        PlayerMusicBounce pBounce = pScript.transform.GetComponent<PlayerMusicBounce>();
        pBounce.StopCoroutine(pBounce.bouncing);
        pBounce.bouncing = pBounce.StartCoroutine(pBounce.bounce());
        StartCoroutine(data.musicIntro(music[0],music[1]));
        transform.GetChild(9).gameObject.SetActive(true);
        transform.GetChild(10).gameObject.SetActive(true);
        transform.GetChild(11).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        pScript.goToCutsceneMode(false);
        pScript.pauseMenu.enabled = true;
    }
    public void npcCutsceneEvent()
    {
        StartCoroutine(cutscene());
    }
    IEnumerator musicFade(int ID,bool fadeIn)
    {
        float volume = 0, additive = 0.005f,max = 0.7f;
        if(fadeIn)
        {
            while(volume<max)
            {
                if(Time.timeScale!=0)
                {
                    volume+=additive;
                    if(volume>max)volume = max;

                    overlays[ID].volume = volume;
                }
                yield return 0;
            }
        }
        else
        {
            volume = overlays[ID].volume;
            while(volume>0)
            {
                if(Time.timeScale!=0)
                {
                    volume-=additive;
                    if(volume<0)volume = 0;
                    
                    overlays[ID].volume = volume;
                }
                yield return 0;
            }
        }
    }
    public void overlayMusicFade(int ID,bool fadeIn)
    {
        StartCoroutine(musicFade(ID,fadeIn));
    }
}
