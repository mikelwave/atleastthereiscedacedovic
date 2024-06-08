using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NuclearThrone : MonoBehaviour
{
    public PlayerScript pScript;
    public Transform player;
    MGCameraController cam;
    public GameData data;
    public GameObject NPC;
    TextBox textBox;
    dataShare DataS;
    public GameObject PauseMenu,HUDCanvas;
    MenuScript pauseScript;
    public Transform cutsceneQuad,NTCorpse;
    public AudioClip music;
    public AudioClip[] mainMusic = new AudioClip[2];
    public AudioClip[] sounds;
    AudioSource vaSource,ambience;
    PlayerMusicBounce pBounce;
    [HideInInspector]
    public bool cryTrigger = false;
    Animator anim;
    public GameObject[] holeSprites;
    public TileBase invTile;
    bool moveCamera = false;
    float camDamping = 0.2f;
	private Vector3 velocity = Vector3.zero;
	float maxSpeed = 6f;
    Transform point;
    Tilemap map;
    public GameObject MegaSatanIntro;
    public BoxCollider2D[] steamColliders;
    SpriteRenderer fadeRender;
    public GameObject megaSatan;
    bool fade = false;
    int mode = 0;
    public GameObject throneDetail;
    public void enableCryTrigger()
    {
        if(!cryTrigger)cryTrigger = true;
    }
    IEnumerator camFade(bool playIntro)
    {
        yield return new WaitUntil(()=>Time.timeScale!=0);
        cam.fadeScreen(false);
        if(!playIntro)
        {
            yield return new WaitUntil(()=>Time.timeScale!=0);
            StartCoroutine(data.musicIntro(null,music));
            pBounce.enabled = false;
            pBounce.BPM = 256;
            yield return 0;
            pBounce.enabled = true;
            transform.GetChild(0).GetChild(0).GetComponent<SimpleAnim2>().enabled = true;
            transform.GetChild(0).GetComponent<MovementAI>().speed = 4;
            NPC.GetComponent<NPCScript>().startLine = 8;
        }
        else
        {
            pScript.pauseMenu.enabled = false;
            player.position = new Vector3(32.5f,player.position.y,player.position.z);
            pScript.axis.acceptFakeInputs = true;
            pScript.axis.artificialX = 1;
            yield return new WaitUntil(()=>player.position.x>=36.5f);
            pScript.axis.artificialX = 0;
            pScript.axis.acceptFakeInputs = false;
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(cutscene1());
        }
    }
    IEnumerator cutscene1()
    {
        BoxCollider2D col = NPC.GetComponent<BoxCollider2D>();
        data.timeFrozen=true;
        col.offset-=new Vector2(0,15);
        yield return new WaitUntil(()=>textBox.eventInt==1);
        playVA(0);
        col.offset+=new Vector2(0,15);
        yield return new WaitUntil(()=>textBox.eventInt==2);
        playVA(1);
        yield return new WaitUntil(()=>textBox.eventInt==3);
        playVA(2);
        yield return new WaitUntil(()=>textBox.eventInt==4);
        //NT points
        transform.GetChild(0).GetChild(0).GetComponent<Animator>().enabled = true;
        playVA(3);
        yield return new WaitUntil(()=>textBox.eventInt==5);
        playVA(4);
        yield return new WaitUntil(()=>textBox.eventInt==6);
        playVA(5);
        yield return new WaitUntil(()=>textBox.eventInt==7);
        yield return new WaitUntil(()=>anim.GetInteger("Value")==1 || !anim.gameObject.activeInHierarchy);
        StartCoroutine(data.musicIntro(null,music));
        pBounce.enabled = false;
        pBounce.BPM = 256;
        yield return 0;
        pBounce.enabled = true;
        playVA(7);
        yield return new WaitUntil(()=>!textBox.gameObject.activeInHierarchy);
        pScript.goToCutsceneMode(false);
        transform.GetChild(0).GetChild(0).GetComponent<SimpleAnim2>().enabled = true;
        transform.GetChild(0).GetComponent<MovementAI>().speed = 4;
        NPC.GetComponent<NPCScript>().startLine = 8;
        data.timeFrozen=false;
        //
    }
    IEnumerator skipIntro()
    {
        data.timeFrozen=false;
        for(int i = 0;i<steamColliders.Length;i++)
        {
            var main = steamColliders[i].GetComponent<ParticleSystem>().main;
            main.prewarm = true;
            steamColliders[i].enabled = true;
        }
        yield return 0;
        pBounce.BPM = 224;
        if(DataS.checkpointValue<=1)
        data.StartCoroutine(data.musicIntro(mainMusic[0],mainMusic[1]));
        else data.StartCoroutine(data.musicIntro(null,mainMusic[1]));
        if(DataS.checkpointValue<2)
        {
            cam.transform.position = new Vector3(57,85,-10);
            cam.lockCamera = true;
        }
        yield return new WaitUntil(()=>data.sources[0].clip == mainMusic[1]);
        pBounce.enabled = false;
        pBounce.BPM = 256;
        yield return 0;
        pBounce.enabled = true;
    }
    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.Find("Player_main").transform;
        cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        data = GameObject.Find("_GM").GetComponent<GameData>();
        pBounce = player.GetComponent<PlayerMusicBounce>();
        mode = DataS.mode;
        if(mode==1) //playuh mode
        {
            MegaSatanIntro.GetComponent<AnimatorSoundPlayer>().clips[0]=sounds[23];
            DataS.playCutscene = false;
        }
        else Destroy(throneDetail);
        if(DataS.checkpointValue>=1)
        {
            data.StartCoroutine(skipIntro());
            Destroy(MegaSatanIntro);
            Destroy(gameObject);
        }
        else
        {
            
            megaSatan.SetActive(false);
            vaSource = GetComponent<AudioSource>();
            pScript = player.GetComponent<PlayerScript>();
            pauseScript = PauseMenu.GetComponent<MenuScript>();
            textBox = GameObject.Find("Textbox_Canvas").transform.GetChild(0).GetComponent<TextBox>();
            NTCorpse = transform.GetChild(0).GetChild(2);
            HUDCanvas = GameObject.Find("HUD_Canvas");
            anim = textBox.GetComponent<Animator>();
            bool playIntro = DataS.playCutscene;
            ambience = transform.GetChild(transform.childCount-2).GetComponent<AudioSource>();
            if(playIntro) pScript.goToCutsceneMode(true);
            if(Application.isPlaying)
            cutsceneQuad.gameObject.SetActive(true);
            map = GameObject.Find("MainMap").GetComponent<Tilemap>();
            fadeRender = transform.GetChild(transform.childCount-3).GetComponent<SpriteRenderer>();
            if(mode!=1)
            StartCoroutine(camFade(playIntro));
            else
            {
                pauseScript.pauseLock = true;
                StartCoroutine(fastCutscene());
            }
        }
        //debug
        //for(int i = 4;i>=0;i--)
        //{
        //    Destroy(transform.GetChild(i).gameObject);
        //}
        //player.position-=new Vector3(0,4,0);
        //debug
        //DataS.playCutscene = true;
    }
    void playVA(int ID)
    {
        stopVA();
        vaSource.clip = sounds[ID];
        vaSource.Play();
    }
    public void stopVA()
    {
        if(vaSource.isPlaying)vaSource.Stop();
    }
    public void stompCutsceneTrigger()
    {
        pauseScript.pauseLock = true;
        pScript.goToCutsceneMode(true);
        StartCoroutine(cutscene2());
    }
    IEnumerator cutscene2()
    {
        data.timeFrozen=true;
        Gravity grav = player.GetComponent<Gravity>();
        grav.maxVelocities = new Vector2(grav.maxVelocities.x,3);
        grav.pushForces = new Vector2(grav.pushForces.x,-0.05f);
        data.stopAllMusic();
        StartCoroutine(data.musicIntro(null,null));
        yield return new WaitUntil(()=>pScript.grounded);
        pBounce.enabled = false;
        pBounce.BPM = 128;
        yield return 0;
        pBounce.enabled = true;
        if(player.localScale.x==-1&&NTCorpse.position.x>=player.position.x)
        player.localScale = new Vector3(1,1,1);
        else player.localScale = new Vector3(-1,1,1);
        Transform tr = NPC.transform;
        if(NTCorpse.position.x>=tr.position.x)
        tr.localScale = new Vector3(-1,1,1);
        else tr.localScale = new Vector3(1,1,1);
        grav.maxVelocities = grav.savedMaxVelocities;
        grav.pushForces = grav.savedPushForces;
        yield return new WaitUntil(()=> cryTrigger);
        cam.constantShake = true;
        cam.shakeCamera(0.1f,1f);
        playVA(8);
        yield return new WaitUntil(()=> !vaSource.isPlaying);
        NPC.GetComponent<NPCScript>().turnsToPlayer = true;
        cam.constantShake = false;
        BoxCollider2D col = NPC.GetComponent<BoxCollider2D>();
        if(player.localScale.x==-1&&tr.position.x>=player.position.x)
        player.localScale = new Vector3(1,1,1);
        else player.localScale = new Vector3(-1,1,1);
        col.offset-=new Vector2(0,15);
        yield return new WaitUntil(()=>textBox.eventInt==1);
        playVA(6);
        col.offset+=new Vector2(0,15);
        yield return new WaitUntil(()=>anim.GetInteger("Value")==1 || !anim.gameObject.activeInHierarchy);
        stopVA();
        pScript.goToCutsceneMode(true);
        StartCoroutine(spawnHole());
        data.timeFrozen=false;
    }
    IEnumerator spawnHole()
    {
        Vector3Int[] tilePositions = new Vector3Int[8];
        GameObject[] explosions = new GameObject[4];
        explosions[0] = transform.GetChild(0).gameObject;
        explosions[1] = transform.GetChild(1).gameObject;
        explosions[2] = transform.GetChild(2).gameObject;
        explosions[3] = transform.GetChild(3).gameObject;
        if(player.position.x>44)
        {
            tilePositions[0] = new Vector3Int(38,115,0);
            tilePositions[1] = new Vector3Int(39,115,0);
            tilePositions[2] = new Vector3Int(40,115,0);
            tilePositions[3] = new Vector3Int(41,115,0);
            tilePositions[4] = new Vector3Int(38,114,0);
            tilePositions[5] = new Vector3Int(39,114,0);
            tilePositions[6] = new Vector3Int(40,114,0);
            tilePositions[7] = new Vector3Int(41,114,0);
        }
        else
        {
            tilePositions[0] = new Vector3Int(46,115,0);
            tilePositions[1] = new Vector3Int(47,115,0);
            tilePositions[2] = new Vector3Int(48,115,0);
            tilePositions[3] = new Vector3Int(49,115,0);
            tilePositions[4] = new Vector3Int(46,114,0);
            tilePositions[5] = new Vector3Int(47,114,0);
            tilePositions[6] = new Vector3Int(48,114,0);
            tilePositions[7] = new Vector3Int(49,114,0);
        }
        explosions[0].transform.position = tilePositions[0]+new Vector3(1.5f,1,0);
        explosions[1].transform.position = tilePositions[0]+new Vector3(2.5f,1,0);
        explosions[2].transform.position = tilePositions[0]+new Vector3(1.5f,0,0);
        explosions[3].transform.position = tilePositions[0]+new Vector3(2.5f,0,0);

        holeSprites[0].transform.position = tilePositions[0]+new Vector3(0.5f,0,0);
        holeSprites[1].transform.position = tilePositions[3]+new Vector3(0.5f,0,0);

        yield return 0;
        cam.easeShake = true;
        cam.shakeCamera(0.2f,0.25f);
        explosions[0].SetActive(true);
        explosions[1].SetActive(true);
        data.playUnlistedSound(sounds[9]);
        yield return new WaitForSeconds(0.05f);
        explosions[2].SetActive(true);
        explosions[3].SetActive(true);
        data.playUnlistedSound(sounds[10]);
        yield return new WaitForSeconds(0.1f);
        holeSprites[0].SetActive(true);
        holeSprites[1].SetActive(true);
        for(int i = 0; i<tilePositions.Length;i++)
        {
            if(i==0||i==4||i==3||i==7)
            {
                map.SetTile(tilePositions[i],invTile);
            }
            else map.SetTile(tilePositions[i],null);
        }
        yield return new WaitForSeconds(0.1f);
        pScript.goToCutsceneMode(false);
        pauseScript.pauseLock = false;
        cam.lockCamera = true;
        cam.obeyBounds = false;
    }
    public void skullCultCutsceneTrigger()
    {
        StartCoroutine(skullCultCutscene());
        pauseScript.pauseLock = true;
    }
    IEnumerator playAmbience()
    {
        ambience.Play();
        while(ambience.volume<0.7f)
        {
            ambience.volume+=0.05f;
            if(ambience.volume>1)ambience.volume = 0.7f;
            yield return 0;
        }

    }
    public void overlayFadeTrigger()
    {
        StartCoroutine(overlayFade());
    }
    IEnumerator overlayFade()
    {
        while(fadeRender.color.a<1)
        {
            fadeRender.color = new Color(1,1,1,Mathf.Clamp(fadeRender.color.a+0.2f,0,1));
            yield return 0;
        }
        fade = true;
    }
    IEnumerator fastCutscene()
    {
        player.position = new Vector3(51,101);
        cam.transform.position = new Vector3(57,104,-10);
        pScript.goToCutsceneMode(true);
        yield return new WaitUntil(()=>Time.timeScale!=0);
        yield return 0;
        HUDCanvas.SetActive(false);
        cam.fadeScreen(false);
        data.timeFrozen=true;
        data.echoInMainArea = true;
        data.audioEffectsToggle(2);
        GameObject cameraTarget = new GameObject("camTarget");
        cameraTarget.transform.position = new Vector3(57,85,-10);
        point = cameraTarget.transform;
        StartCoroutine(playAmbience());
        yield return 0;
        yield return new WaitUntil(()=>Time.timeScale!=0);
        cam.obeyBounds = true;
        moveCamera = true;
        yield return new WaitForSeconds(4f);
        cam.lockCamera = true;
		moveCamera = false;
        Destroy(cameraTarget);
        map.SetTile(new Vector3Int(50,100,0),null);
        map.SetTile(new Vector3Int(51,100,0),null);
        playVA(19);
        yield return 0;
        transform.GetChild(2).localScale = Vector3.one;
        yield return new WaitForSeconds(0.2f);
        transform.GetChild(3).localScale = Vector3.one;
        yield return new WaitUntil(()=>pScript.grounded);
        for(int i = 0;i<steamColliders.Length;i++)
        steamColliders[i].enabled = true;
        playVA(20);

        yield return new WaitForSeconds(0.5f);
        cam.easeShake = false;
        cam.constantShake = true;
        cam.shakeCamera(0.1f,10f);
        MegaSatanIntro.SetActive(true);
        data.StartCoroutine(data.musicIntro(mainMusic[0],mainMusic[1]));
        if(ambience.isPlaying) ambience.Stop();
        yield return new WaitForSeconds(6f);
        cam.constantShake = false;
        cam.shakeCamera(0.1f,1f);
        cam.easeShake = true;
        yield return new WaitUntil(()=>data.sources[0].clip == mainMusic[1]);
        pBounce.enabled = false;
        pBounce.BPM = 256;
        yield return 0;
        pBounce.enabled = true;
        yield return new WaitUntil(()=>fade);
        HUDCanvas.SetActive(true);
        Destroy(MegaSatanIntro);
        megaSatan.SetActive(true);
        while(fadeRender.color.a>0)
        {
            fadeRender.color = new Color(1,1,1,Mathf.Clamp(fadeRender.color.a-0.1f,0,1));
            yield return 0;
        }
        pScript.goToCutsceneMode(false);
        data.timeFrozen=false;
        Destroy(gameObject);
    }
    IEnumerator skullCultCutscene()
    {
        data.timeFrozen=true;
        data.echoInMainArea = true;
        data.audioEffectsToggle(2);
        textBox.shake_Sound = sounds[22];
        HUDCanvas.SetActive(false);
        pScript.goToCutsceneMode(true);
        BoxCollider2D NPCCol = transform.GetChild(2).GetComponent<BoxCollider2D>();
        textBox.usedAnimators = new Animator[4];
        textBox.usedAnimators[1] = transform.GetChild(2).GetComponent<Animator>();
        textBox.usedAnimators[2] = transform.GetChild(3).GetComponent<Animator>();
        textBox.usedAnimators[3] = transform.GetChild(4).GetComponent<Animator>();
        transform.GetChild(2).gameObject.SetActive(true);
        transform.GetChild(3).gameObject.SetActive(true);
        transform.GetChild(4).gameObject.SetActive(true);
        GameObject cameraTarget = new GameObject("camTarget");
        cameraTarget.transform.position = new Vector3(57,85,-10);
        point = cameraTarget.transform;
        StartCoroutine(playAmbience());
        yield return 0;
        yield return new WaitUntil(()=>Time.timeScale!=0);
        pScript.goToCutsceneMode(true);
        cam.obeyBounds = true;
        moveCamera = true;
        yield return new WaitForSeconds(3f);
        cam.lockCamera = true;
		moveCamera = false;
        Destroy(cameraTarget);
        NPCCol.offset+=new Vector2(20,0);
        yield return new WaitUntil(()=>textBox.eventInt==1);
        playVA(11);
        textBox.resumeInCutscene = true;
        player.localScale = new Vector3(1,1,1);
        NPCCol.offset-=new Vector2(20,0);
        textBox.usedAnimators[0] = player.GetChild(0).GetComponent<Animator>();
        yield return new WaitUntil(()=>textBox.eventInt==2);
        transform.GetChild(2).localScale = new Vector3(-1,1,1);
        playVA(12);
        yield return new WaitUntil(()=>textBox.eventInt==3);
        playVA(13);
        yield return new WaitUntil(()=>textBox.eventInt==4);
        map.SetTile(new Vector3Int(50,100,0),null);
        map.SetTile(new Vector3Int(51,100,0),null);
        playVA(19);
        yield return 0;
        transform.GetChild(2).localScale = Vector3.one;
        yield return new WaitForSeconds(0.2f);
        transform.GetChild(3).localScale = Vector3.one;
        yield return new WaitUntil(()=>pScript.grounded);
        playVA(20);
        yield return new WaitUntil(()=>textBox.eventInt==5);
        playVA(14);
        yield return new WaitUntil(()=>textBox.eventInt==6);
        playVA(15);
        yield return new WaitUntil(()=>textBox.eventInt==7);
        playVA(16);
        yield return new WaitUntil(()=>textBox.eventInt==8);
        stopVA();
        yield return new WaitUntil(()=>textBox.eventInt==9);
        int excl = 0;
        Random.State oldState = Random.state;
        Random.InitState(42);
        while(textBox.eventInt<10)
        {
            if(Time.timeScale!=0)
            {
                int rand = Random.Range(0,20);
                if(rand==0)
                {
                    rand = Random.Range(2,5);
                    while(excl==rand)
                    {
                        rand = Random.Range(2,5);
                        yield return 0;
                    }
                    transform.GetChild(rand).localScale = new Vector3(-transform.GetChild(rand).localScale.x,1,1);
                    excl = rand;
                }
            }
            yield return 0;
        }
        
        yield return new WaitUntil(()=>textBox.eventInt==10);
        transform.GetChild(2).localScale = Vector3.one;
        transform.GetChild(3).localScale = new Vector3(-1,1,1);
        transform.GetChild(4).localScale = Vector3.one;
        Random.state = oldState;
        playVA(17);
        yield return new WaitUntil(()=>textBox.eventInt==11);
        playVA(21);
        cam.constantShake = true;
        for(int i = 0;i<steamColliders.Length;i++)
        steamColliders[i].enabled = true;
        cam.shakeCamera(0.05f,10f);
        yield return new WaitForSeconds(0.05f);
        cam.shakeCamera(0.09f,10f);
        yield return new WaitForSeconds(0.05f);
        cam.shakeCamera(0.1f,10f);
        yield return new WaitForSeconds(0.05f);
        cam.shakeCamera(0.15f,10f);
        yield return new WaitForSeconds(0.05f);
        cam.shakeCamera(0.2f,5f);
        yield return new WaitForSeconds(4f);
        cam.easeShake = true;
        cam.constantShake = false;
        yield return new WaitUntil(()=>textBox.eventInt==12);
        playVA(18);
        yield return new WaitUntil(()=>anim.GetInteger("Value")==1);
        GameObject laser = transform.GetChild(transform.childCount-2).gameObject;
        cam.shakeCamera(0,0);
        laser.SetActive(true);
        cam.easeShake = false;
        cam.constantShake = true;
        //yield return new WaitUntil(()=>laser==null);
        cam.shakeCamera(0.2f,10f);
        yield return new WaitForSeconds(2.5f);
        cam.constantShake = false;
        cam.easeShake = true;
        yield return new WaitForSeconds(0.5f);
        cam.easeShake = false;
        cam.constantShake = true;
        cam.shakeCamera(0.1f,10f);
        MegaSatanIntro.SetActive(true);
        data.StartCoroutine(data.musicIntro(mainMusic[0],mainMusic[1]));
        if(ambience.isPlaying) ambience.Stop();
        yield return new WaitForSeconds(6f);
        cam.constantShake = false;
        cam.shakeCamera(0.1f,1f);
        cam.easeShake = true;
        yield return new WaitUntil(()=>data.sources[0].clip == mainMusic[1]);
        pBounce.enabled = false;
        pBounce.BPM = 256;
        yield return 0;
        pBounce.enabled = true;
        yield return new WaitUntil(()=>fade);
        HUDCanvas.SetActive(true);
        Destroy(MegaSatanIntro);
        megaSatan.SetActive(true);
        while(fadeRender.color.a>0)
        {
            fadeRender.color = new Color(1,1,1,Mathf.Clamp(fadeRender.color.a-0.1f,0,1));
            yield return 0;
        }
        pScript.goToCutsceneMode(false);
        data.timeFrozen=false;
        Destroy(gameObject);
    }
    void Update()
    {
        if(moveCamera&&point!=null)
        {
		    cam.transform.position = Vector3.SmoothDamp(cam.transform.position,point.position, ref velocity, camDamping,maxSpeed,Time.unscaledDeltaTime);
            cam.lockCamera = true;
        }
    }
}
