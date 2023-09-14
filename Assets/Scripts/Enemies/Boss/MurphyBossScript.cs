using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class MurphyBossScript : MonoBehaviour
{
    [Header("Debug")]
    public bool debug = false;
    public int startPhase = 0;
    [Space]
    public Vector2Arr[] travelPaths;
    public Vector2Arr[] bossPaths;
    [Space]
    public Vector2Int currentPath = Vector2Int.zero;
    [Space]
    public bool recordPos = false;
    public float speed = 0.25f,fightSpeed = 0.3f;
    public AudioClip[] warningSounds,chompSounds,bossSounds;
    AudioSource aSource;
    bool moving = false;
    Vector2 bosspos;
    Vector3Int[] basePoints = new Vector3Int[3];
    GameData data;
    int checkframe = 0;
    AnimatorSoundPlayer aPlayer;
    MurphyBossAnim mAnimSc;
    Transform player,laserHolder;
    PlayerScript pScript;
    public Tilemap map,backgroundmap,bossmap,refMap;
    Tilemap eatMap;
    Animator anim;
    public bool bossMode = false;
    Collider2D col;
    MGCameraController cam;
    Coroutine mainPatternLoop;
    int attackFrames = 0;
    Transform bossArea;
    Animation warningAnimation;
    Animator scanner;
    SpriteRenderer scannerRender;
    public GameObject Zonk,OrangeDiskette,yellowDiskette;
    int phase = 0;
    int preBossDMG = 0;
    
    //3 tables of values
    [Header("Making Arenas")]
    [Space]
    public Vector2Int[] bottomLeftPoints;
    Vector2Int arenaStartPoint = new Vector2Int(558,-21);
    public int arenaToDraw = 0;
    public bool drawBlocks = false;
    [HideInInspector]
    public int rowToScan = 0;
    bool finishedDrawing = true;
    LaserScript[] lasers;
    AnimatorSoundPlayer scannerSound;
    MurphyDetonator detonator;
    bool midDamage = false;
    public bool exploding = false;
    ParticleSystem explodeLoop;
    public Transform cutscene;
    Transform checkpointsHold;
    dataShare DataS;
    // Start is called before the first frame update
    void Start()
    {
            currentPath = Vector2Int.zero;
            anim = transform.GetChild(0).GetComponent<Animator>();
            aSource = transform.GetChild(0).GetComponent<AudioSource>();
            explodeLoop = transform.GetChild(0).GetChild(3).GetComponent<ParticleSystem>();
            data = GameObject.Find("_GM").GetComponent<GameData>();
            aPlayer = transform.GetChild(0).GetComponent<AnimatorSoundPlayer>();
            player = GameObject.Find("Player_main").transform;
            pScript = player.GetComponent<PlayerScript>();
            bossArea = GameObject.Find("bossArea").transform;
            laserHolder = transform.GetChild(2);
            eatMap = map;
            checkpointsHold = GameObject.Find("Checkpoints").transform;
            DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
            if(DataS.checkpointValue<=3&&Application.isPlaying)
            {
                checkpointsHold.GetChild(3).gameObject.SetActive(true);
            }
            if(Application.isPlaying)
            {
                laserHolder.SetParent(null);
                phase = startPhase;
            }
            warningAnimation = bossArea.GetChild(1).GetChild(0).GetComponent<Animation>();
            detonator = bossArea.GetChild(2).GetComponent<MurphyDetonator>();
            scanner = bossArea.GetChild(3).GetComponent<Animator>();
            scannerRender = scanner.transform.GetChild(0).GetComponent<SpriteRenderer>();
            scannerSound = scanner.transform.GetComponent<AnimatorSoundPlayer>();
            mAnimSc = aPlayer.transform.GetComponent<MurphyBossAnim>();
            cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
            col = GetComponent<Collider2D>();
            int val = DataS.checkpointValue;
            switch(val)
            {
                default: break;
                case 1: currentPath = new Vector2Int(1,0); break;
                case 2: currentPath = new Vector2Int(2,0); break;
                case 3: currentPath = new Vector2Int(3,0); break;
            }
        if(Application.isPlaying)
        cutscene.gameObject.SetActive(true);
    }
    IEnumerator stageCharge()
    {
        transform.position = travelPaths[currentPath.x].array[0];
        if(currentPath.x!=0)
        aSource.PlayOneShot(warningSounds[Random.Range(1,warningSounds.Length)]);
        else  aSource.PlayOneShot(warningSounds[0]);
        yield return new WaitForSeconds(2.5f);
        col.enabled = true;
        moving = true;
        aPlayer.canPlay = moving;
        mAnimSc.canShake = moving;
    }
    IEnumerator introShake()
    {
        float shakePwr = 0.005f,timeWait = 0.1f;
        while(timeWait<3f)
        {
            cam.shakeCamera(shakePwr,0.1f);
            yield return new WaitForSeconds(0.1f);
            shakePwr+=0.005f; timeWait+=0.1f;
        }
    }
    void setStartPhase()
    {
        if(!debug)
        switch(DataS.checkpointValue)
        {
            default:
            phase = 0;
            break;
            case 5:
            phase = 1;
            break;
            case 6:
            phase = 2;
            break;
        }
    }
    IEnumerator bossIntroSequence()
    {
        speed = fightSpeed;
        setStartPhase();
        if(preBossDMG>=2)
        {
            phase = Mathf.Clamp(preBossDMG-1,0,2);
            setCheck(phase);
        }
        bossMode = true;
        eatMap = bossmap;
        data.map = bossmap;
        PlayerScript pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
        StartCoroutine(introShake());
        yield return new WaitForSeconds(1f);
        transform.position = new Vector3(570,8.5f,0);
        int i = 4;
        switch(phase)
        {
            default:
            break;
            case 1:
            i = 1;
            break;
            case 2:
            i = 2;
            break;
        }
        aSource.PlayOneShot(warningSounds[i]);
        print("Start phase: "+phase);
        //draw empty arena;
        StartCoroutine(drawArena(0+(5*(phase)),0));
        //else StartCoroutine(drawArena(5));
        yield return new WaitUntil(()=>finishedDrawing);
        yield return new WaitForSeconds(2f);
        pScript.goToCutsceneMode(false);
        mainPatternLoop = StartCoroutine(mainBossPattern());
    }
    IEnumerator mainBossPattern()
    {
        /*
        0 - Tubes Attack
        1 - Zonk Drops
        2 - Laser Shock
        3 - Yellow Diskettes     
         */

        int phaseAdditive = 0;
        if(phase!=0) phaseAdditive = 5*phase;
        int attackVal;
        for(attackVal = 0; attackVal<5;attackVal++)
        {
            int randOffset = Random.Range(0,4);
            //print("Attack: "+attackVal);
        switch (attackVal)
        {
            //Reset if unknown
            default: attackVal = -1; //print("reset attack");
            break;

            //Tube Attack
            case 0:
            //draw new arena
            StartCoroutine(drawArena(1+phaseAdditive,randOffset));
            yield return new WaitUntil(()=>finishedDrawing);
            //7 seconds
            attackFrames = 420;
            int path = Random.Range(1,3);
            bool firstExit = true;
            while(attackFrames>=0)
            {
                //Give boss a path;
                if(!moving)
                {
                    //semi random, favour going out the same height as player, but have a chance to be random
                    
                    //1 - comes from left side, 2 - comes from right side
                    //randomY = if 0, true random, else base on player height;
                    path++; if(path>2)path=1;
                    //check for appropriate height
                        //player at the bottom
                        if(player.position.y<-14.5f)
                        {
                            if(path==2)
                            currentPath = new Vector2Int(5,0);
                            else currentPath = new Vector2Int(2,0);
                        }
                        //player at the top
                        else if(player.position.y>-10.5f)
                        {
                            if(path==2)
                            currentPath = new Vector2Int(3,0);
                            else currentPath = new Vector2Int(0,0);
                        }
                        //player in the middle
                        else
                        {
                            if(path==2)
                            currentPath = new Vector2Int(4,0);
                            else currentPath = new Vector2Int(1,0);
                        }
                    transform.position = bossPaths[currentPath.x].array[0];
                    //Debug.Log(currentPath+" "+transform.position);
                    warningAnimation.transform.parent.position = new Vector3(warningAnimation.transform.parent.position.x,transform.position.y,transform.position.z);
                    warningAnimation.Play("Murphy_WarningFlash"+(path==2?"2":"1"));
                    yield return new WaitForSeconds(1.1f);

                    //send murphy
                    col.enabled = true;
                    moving = true;
                    aPlayer.canPlay = moving;
                    mAnimSc.canShake = moving;
                    yield return new WaitUntil(()=>!moving);
                    yield return 0;
                    if(firstExit)firstExit=false;
                }
            }
            break;

            //Zonk Drop Attack
            case 1:
            StartCoroutine(drawArena(2+phaseAdditive,randOffset));
            yield return new WaitUntil(()=>finishedDrawing);
            //7 seconds
            attackFrames = 420;
            float zonkDelay = 0.5f-(0.15f*phase);
            while(attackFrames>=0)
            {
                int xPos = Random.Range(558,582);
                Vector3Int posInt = new Vector3Int(xPos,-10,0);
		        Vector3 spawnPos = new Vector3(posInt.x+0.5f,posInt.y+0.5f,posInt.z);
                transform.position = spawnPos+new Vector3(0,2,0);
                GameObject obj = Instantiate(Zonk,spawnPos,Quaternion.identity);
                obj.SetActive(true);
                //aSource.PlayOneShot(bossSounds[0]);
                yield return new WaitForSeconds(zonkDelay);
                yield return 0;
            }
            yield return new WaitForSeconds(0.5f);
            break;

            //Zap Attack
            case 2:
                StartCoroutine(drawArena(3+phaseAdditive,randOffset));
                yield return new WaitUntil(()=>finishedDrawing);
            
                //send murphy
                currentPath = new Vector2Int(6,0);
                transform.position = bossPaths[currentPath.x].array[0];
                col.enabled = true;
                moving = true;
                aPlayer.canPlay = moving;
                mAnimSc.canShake = moving;
                yield return new WaitUntil(()=>!moving);
                col.enabled = true;
                transform.GetChild(1).gameObject.SetActive(false);
                anim.SetInteger("Mode",2);
                yield return new WaitForSeconds(0.4f);
                aSource.PlayOneShot(bossSounds[0]);
                if(phase==0)
                yield return new WaitForSeconds(0.5f);
                //7 seconds
                attackFrames = 420;

                //Make red lasers spin around murphy in random speeds, after 2.5 seconds, flash red and yellow for half a second
                //then shoot, repeat until time runs out
                laserHolder.position = transform.position;
                int i;
                if(lasers==null)
                {
                    lasers = new LaserScript[laserHolder.childCount];
                   for(i = 0; i<laserHolder.childCount;i++)
                    {
                       lasers[i] = laserHolder.GetChild(i).GetComponent<LaserScript>();
                    }
                }
                for(i = 0; i<lasers.Length;i++)
                {
                    lasers[i].gameObject.SetActive(true);
                }
                yield return 0;
                while(attackFrames>=0)
                {
                    //print("Laser activate");
                    for(i = 0; i<5;i++)
                    {
                        Spinner spin = laserHolder.GetChild(i).GetComponent<Spinner>();
                        spin.transform.eulerAngles = Vector3.zero;
                        spin.speed = Random.Range(0.1f,0.4f);
                        if((int)Random.Range(0,2)==0)
                        {
                            spin.speed = -spin.speed;
                        }
                        if(phase!=0)
                        spin.toSub = spin.speed*0.013f;
                        else spin.toSub = spin.speed*0.008f;
                        lasers[i].shoot = true;
                    }
                    if(phase==0)
                    yield return new WaitForSeconds(1.5f);
                    else yield return new WaitForSeconds(0.75f);
                    //flash
                    for(i = 0; i<5;i++)
                    {
                        lasers[i].colorFlashing = true;
                    }
                    anim.SetTrigger("Shoot");
                    yield return new WaitForSeconds(0.1f);
                    aSource.PlayOneShot(bossSounds[2]);
                    yield return new WaitUntil(()=>mAnimSc.zap);
                    mAnimSc.zap = false;
                    //shoot normal lasers
                    for(i = 0; i<lasers.Length;i++)
                    {
                       if(i<5)
                       {
                            lasers[i+5].transform.eulerAngles = lasers[i].transform.eulerAngles;
                            lasers[i].shoot = false;
                            lasers[i].colorFlashing = false;
                            lasers[i+5].shoot = true;
                        }
                    }
                    aSource.PlayOneShot(bossSounds[1]);
                    yield return new WaitForSeconds(0.25f);
                    //stop shooting
                    for(i = 5; i<lasers.Length;i++)
                    {
                        lasers[i].shoot = false;
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                for(i = 0; i<lasers.Length;i++)
                {
                    lasers[i].gameObject.SetActive(false);
                }
                //make murphy exit the other pipe when attackframes run out
                transform.GetChild(1).gameObject.SetActive(true);
                anim.SetInteger("Mode",1);
                currentPath = new Vector2Int(7,0);
                col.enabled = true;
                moving = true;
                aPlayer.canPlay = moving;
                mAnimSc.canShake = moving;
                yield return new WaitUntil(()=>!moving);
            break;

            //Diskette Attack
            case 3:
            //First, draw arena, but check for positions of yellow diskettes and orange diskettes,
            //Instead of spawning the tiles for those, spawn the objects.
            StartCoroutine(drawArena(4+phaseAdditive,randOffset));
            yield return new WaitUntil(()=>finishedDrawing);
            //Add yellow diskettes to the detonator spawnpoint list.

            //Start detonation corutine when stage finishes drawing (wait a few seconds first?)
            detonator.activate();
            yield return 0;
            yield return new WaitUntil(()=>!detonator.activated);
            break;
        }
        yield return new WaitForSeconds(1f);
        //draw empty arena
        if(attackVal!=-1)
        {
            StartCoroutine(drawArena(0+phaseAdditive,0));
            yield return new WaitUntil(()=>finishedDrawing);
        }
        //print("pick next attack");
        }
    }
    public void IntroStart()
    {
        StartCoroutine(bossIntroSequence());
    }
    public void trigger()
    {
        StartCoroutine(stageCharge());
    }
    void setBasePoints(bool back)
    {
        Vector3Int backOffset = Vector3Int.zero;
        if(back)
        {
            backOffset = new Vector3Int(Mathf.RoundToInt(transform.right.x*transform.localScale.x),Mathf.RoundToInt(transform.right.y*transform.localScale.x),Mathf.RoundToInt(transform.right.z));
        }
        Vector3Int intPos = new Vector3Int(Mathf.FloorToInt(transform.position.x-0.5f),Mathf.FloorToInt(transform.position.y-0.5f),Mathf.FloorToInt(transform.position.z))
        ,Offset = new Vector3Int(Mathf.RoundToInt(transform.up.x),Mathf.RoundToInt(transform.up.y),Mathf.RoundToInt(transform.up.z));
        Vector3Int flipOffset = Vector3Int.zero;
        if(transform.localScale.x==-1&&!back)
        {
            flipOffset = Vector3Int.right;
        }
        if(transform.eulerAngles.z>200&&!back)
        {
            flipOffset = Vector3Int.up;
        }
        basePoints[0]=intPos-backOffset+flipOffset;
        basePoints[1]=intPos+Offset-backOffset+flipOffset;
        basePoints[2]=intPos-Offset-backOffset+flipOffset;
        data.removeTiles(basePoints,eatMap);
        data.removeTiles(basePoints,backgroundmap);
    }
    void bossPath()
    {
        if(bosspos==bossPaths[currentPath.x].array[currentPath.y])
        {
            if(anim.GetInteger("Mode")!=1)anim.SetInteger("Mode",1);
            //compare points if not last
            if(currentPath.y<bossPaths[currentPath.x].array.Length-1)
            {
                Vector2 newPoint = bossPaths[currentPath.x].array[currentPath.y+1];
                //going up or down?
                if(newPoint.y!=transform.position.y)
                {
                    //up
                    if(newPoint.y>transform.position.y)
                    {
                        transform.eulerAngles = new Vector3(0,0,90);
                        transform.localScale = Vector3.one;
                    }
                    else
                    {
                       transform.eulerAngles = new Vector3(0,0,270);
                       transform.localScale = Vector3.one;
                    }
                }
                else
                {
                    transform.eulerAngles = Vector3.zero;
                    //left or right
                    if(newPoint.x>=transform.position.x)
                    {
                        transform.localScale = Vector3.one;
                    }
                    else
                    {
                        transform.localScale = new Vector3(-1,1,1);
                    }
                }
            }
            checkframe = 0;
            setBasePoints(false);
            setBasePoints(true);
            currentPath += Vector2Int.up;
            currentPath = new Vector2Int(Mathf.Clamp(currentPath.x,0,bossPaths.Length),Mathf.Clamp(currentPath.y,0,bossPaths[currentPath.x].array.Length));

            //if reached end
            //print(currentPath.y+", "+bossPaths[currentPath.x].array.Length);
            if(currentPath.y==bossPaths[currentPath.x].array.Length)
            {
                moving = false;
                aPlayer.canPlay = moving;
                mAnimSc.canShake = moving;
                col.enabled = false;
                if(anim.GetInteger("Mode")!=0)anim.SetInteger("Mode",0);
                if(!bossMode)
                {
                    currentPath += Vector2Int.right;
                    currentPath = new Vector2Int(Mathf.Clamp(currentPath.x,0,bossPaths.Length),0);
                }
            }
        }
    }
    IEnumerator drawArena(int arenaID,int xOffset)
    {
        //print("Arena ID: "+arenaID+"Offset: "+xOffset);
        finishedDrawing = false;
        arenaToDraw = arenaID;
        int lastRow = -1;
        rowToScan = -1;
        scanner.SetInteger("Speed",1);
        detonator.floppySpawnPoints = new List<Vector2>();
        //if(arenaToDraw!=0&&arenaToDraw!=5&&arenaToDraw!=10)
        //{
        //   scanner.SetInteger("Speed",1);
        //}
        if(arenaToDraw!=0&&arenaToDraw!=5&&arenaToDraw!=10)
        {
            yield return new WaitUntil(()=>player.position.y<-12.5f);
            //print("Player on ground, can draw");
        }
        scanner.SetTrigger("Scan");
        yield return new WaitUntil(()=>lastRow!=rowToScan);
        //draw blocks down to up;
        //int toAdd = 16;
        for(int y = 0; y<17;y++)
        {
            bool canPlaySound = false;
            //draw a horizontal row of blocks
            Vector3Int playerPos = new Vector3Int(Mathf.RoundToInt(player.position.x),Mathf.RoundToInt(player.position.y+0.5f),(int)bossmap.transform.position.z);
            for(int x = 0; x<24;x++)
            {
                TileBase t;
                //if(arenaToDraw==0||arenaToDraw==5||arenaToDraw==10)
                //{
                    t = refMap.GetTile(new Vector3Int(bottomLeftPoints[arenaToDraw].x+x+(75*xOffset),bottomLeftPoints[arenaToDraw].y+y,(int)map.transform.position.z));

                    //get floppy data
                    //orange
                    if(t!=null&&t.name=="Mpx32_7")
                    Instantiate(OrangeDiskette,new Vector3(arenaStartPoint.x+x+0.5f,arenaStartPoint.y+y+0.5f,(int)bossmap.transform.position.z),Quaternion.identity);
                    //yellow
                    else if(t!=null&&t.name=="Mpx32_17")
                    {
                        Vector3Int point = new Vector3Int(arenaStartPoint.x+x,arenaStartPoint.y+y,(int)bossmap.transform.position.z);
                        //Instantiate(yellowDiskette,point,Quaternion.identity);
                        detonator.floppySpawnPoints.Add(new Vector2(point.x,point.y));
                    }
                    else
                    {
                        Vector3Int v = new Vector3Int(arenaStartPoint.x+x,arenaStartPoint.y+y,(int)bossmap.transform.position.z);
                        if(v==playerPos&&t!=null)
                        {
                            //print("PlayerPos: "+playerPos+" BlockTile: "+v);
                            //print("Moving player up");
                            player.position+=Vector3.up;
                        }
                        bossmap.SetTile(v,t);
                    }

                    if(t!=null&&!canPlaySound)canPlaySound = true;
            }
            if(canPlaySound)
            {
                scannerSound.playSoundVol(1,0.6f);
            }
            yield return new WaitUntil(()=>lastRow!=rowToScan||y==16||lastRow==16||lastRow>y||!scannerRender.enabled);
            lastRow = rowToScan;
        }
        yield return new WaitUntil(()=>!scannerRender.enabled);
        finishedDrawing = true;
        //print("Finished drawing");
    }
    public void increaseRow()
    {
        rowToScan++;
    }
    void travelPath()
    {
        if(bosspos==travelPaths[currentPath.x].array[currentPath.y])
        {
            if(anim.GetInteger("Mode")!=1)anim.SetInteger("Mode",1);
            //compare points if not last
            if(currentPath.y<travelPaths[currentPath.x].array.Length-1)
            {
                Vector2 newPoint = travelPaths[currentPath.x].array[currentPath.y+1];
                //going up or down?
                if(newPoint.y!=transform.position.y)
                {
                    //up
                    if(newPoint.y>transform.position.y)
                    {
                        transform.eulerAngles = new Vector3(0,0,90);
                        transform.localScale = Vector3.one;
                    }
                    else
                    {
                       transform.eulerAngles = new Vector3(0,0,270);
                       transform.localScale = Vector3.one;
                    }
                }
                else
                {
                    transform.eulerAngles = Vector3.zero;
                    //left or right
                    if(newPoint.x>=transform.position.x)
                    {
                        transform.localScale = Vector3.one;
                    }
                    else
                    {
                        transform.localScale = new Vector3(-1,1,1);
                    }
                }
            }
            checkframe = 0;
            setBasePoints(false);
            setBasePoints(true);
            currentPath += Vector2Int.up;
            currentPath = new Vector2Int(Mathf.Clamp(currentPath.x,0,travelPaths.Length),Mathf.Clamp(currentPath.y,0,travelPaths[currentPath.x].array.Length));

            //if reached end
            ////print(currentPath.y+", "+travelPaths[currentPath.x].array.Length);
            if(currentPath.y==travelPaths[currentPath.x].array.Length)
            {
                moving = false;
                aPlayer.canPlay = moving;
                mAnimSc.canShake = moving;
                col.enabled = false;
                if(anim.GetInteger("Mode")!=0)anim.SetInteger("Mode",0);
                if(!bossMode)
                {
                    currentPath += Vector2Int.right;
                    currentPath = new Vector2Int(Mathf.Clamp(currentPath.x,0,travelPaths.Length),0);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying)
        {
            if(recordPos)
            {
            recordPos = false;
            bossPaths[currentPath.x].array[currentPath.y] = transform.position;
            currentPath=new Vector2Int(currentPath.x,Mathf.Clamp(currentPath.y+1,0,bossPaths[currentPath.x].array.Length));
            }
        }
        if(Application.isPlaying)
        {
            if(Time.timeScale!=0)
		    {  
                if(attackFrames>-1)attackFrames--;   
                if(moving)
                {           
                    if(!bossMode)
                    {
                        if(!midDamage)
                        {
                            transform.position = Vector3.MoveTowards(transform.position,travelPaths[currentPath.x].array[currentPath.y],speed);
                            travelPath();
                        }
                    }
                    else
                    {
                    if(!midDamage)
                    {
                        transform.position = Vector3.MoveTowards(transform.position,bossPaths[currentPath.x].array[currentPath.y],speed);
                    }
                    bossPath();
                    }
                    checkframe++;
                    if(checkframe==1)
                    {
                        checkframe = 0;
                        setBasePoints(false);
                    }
                }
                anim.speed = speed*4;	
		    }
            bosspos = new Vector2(transform.position.x,transform.position.y);
            /*if(debug)
            {
                debug = false;
                trigger();
            }*/
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        ////print(other.name);
        if(other.name.Contains("FloppyExplosive")&&!midDamage)
        {
            if(phase<2) anim.SetTrigger("Damage");
            else
            {
                speed = 0.25f;
                anim.SetTrigger("Death");
            }
            other.GetComponent<holdableBlockScript>().Explode();
            StartCoroutine(damage());
        }
    }
    public IEnumerator explodeSoundLoop()
	{
        exploding = true;
		float delay = 1/explodeLoop.emission.rateOverTime.constant;
		explodeLoop.Play();
		while(exploding)
		{
            cam.shakeCamera(0.2f,delay);
			aSource.PlayOneShot(bossSounds[3+Random.Range(0,3)]);
			yield return new WaitForSeconds(delay);
		}
        explodeLoop.Stop(false,ParticleSystemStopBehavior.StopEmitting);
	}
    void setCheck(int i)
    {
        switch(i)
        {
        default:
        checkpointsHold.GetChild(3).gameObject.SetActive(true);
        break;
        case 1:
        checkpointsHold.GetChild(3).gameObject.SetActive(false);
        checkpointsHold.GetChild(4).gameObject.SetActive(true);
        break;
        case 2:
        checkpointsHold.GetChild(4).gameObject.SetActive(false);
        checkpointsHold.GetChild(5).gameObject.SetActive(true);
        break;
        }
    }
    IEnumerator damage()
    {
        midDamage = true;
        if(bossMode)
        {
            StopCoroutine(mainPatternLoop);
            phase++;
            int i = 0;
            switch(phase)
            {
                default: break;
                //phase 2
                case 2: i = 5;
                break;
                //phase 3
                case 3: i = 10;
                break;
            }
            //set phase up by one, phase 3 = death
            if(phase<3)
            {
                setCheck(phase);
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(drawArena(i,0));
                yield return new WaitUntil(()=>mAnimSc.dmgEnd);
                float savedSpeed = speed;
                speed = 0;
                midDamage = false;
                while(speed<savedSpeed)
                {
                    speed+=(0.0025f);
                    yield return 0;
                }
                speed = savedSpeed;
                //wait until not moving
                yield return new WaitUntil(()=>!moving);
                yield return new WaitUntil(()=>finishedDrawing);
                //send murphy
                currentPath = new Vector2Int(7+phase,0);
                transform.position = bossPaths[currentPath.x].array[0];
                col.enabled = true;
                moving = true;
                aPlayer.canPlay = moving;
                mAnimSc.canShake = moving;
                yield return new WaitUntil(()=>!moving);
                //start the new attack sequence
                mainPatternLoop = StartCoroutine(mainBossPattern());
            }
            else
            {
                UnityEngine.Rendering.SortingGroup sr = GetComponent<UnityEngine.Rendering.SortingGroup>();
                sr.sortingLayerName = "Player";
                sr.sortingOrder = 3;
                StartCoroutine(drawArena(15,0));
                data.stopAllMusic();
                cam.explicitStayInsideBounds = true;
                cam.target = transform;
                pScript.goToCutsceneMode(true);
                col.enabled = false;
                transform.GetChild(1).gameObject.SetActive(false);
                dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
                yield return new WaitUntil(()=>!mAnimSc.dmgEnd);
                yield return new WaitUntil(()=>mAnimSc.dmgEnd);
                yield return new WaitForSeconds(2f);
                cam.fadeScreen(true);
                yield return new WaitUntil(()=>cam.fadeAnim>=1);
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
                if(newClear)DataS.loadWorldWithLoadScreen(5);
                else DataS.loadWorldWithLoadScreen(DataS.currentWorld);
            }
        }
        else
        {
            yield return new WaitUntil(()=>!mAnimSc.dmgEnd);
            yield return new WaitUntil(()=>mAnimSc.dmgEnd);
            //print("hurt outside boss stage");
            preBossDMG++;
            midDamage = false;
            float savedSpeed = speed;
            speed = 0;
            while(speed<savedSpeed)
            {
                speed+=(0.005f);
                yield return 0;
            }
            speed = savedSpeed;
        }
    }
    //void FixedUpdate ()
	//{
		//if(Time.timeScale!=0&&moving)
		//{	
		//	transform.position = Vector3.MoveTowards(transform.position,travelPaths[currentPath.x].array[currentPath.y],speed);
		//}
	//}
}
