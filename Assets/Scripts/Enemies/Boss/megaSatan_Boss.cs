using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class megaSatan_Boss : MonoBehaviour
{
    public Animator headAnim,handsAnim;
    megaSatanHandCollisionDetect[] handsAI = new megaSatanHandCollisionDetect[2];
    megaSatan_face mFace;
    Gravity[] handGrav = new Gravity[2];
    [HideInInspector]
    public Transform enemyPocket,skullPocket;
    public Transform skullParticleHolder;
    public GameObject[] enemies;
    Transform head;
    GameObject steamSpawn;
    Transform[] spikes;
    GameObject[] skullblocks;
    public float spikeDelay = 0.2f;
    MGCameraController cam;
    public AudioClip[] sounds;
    public GameData data;
    public dataShare dataS;
    bool inPhase = false;
    public PlayerScript pScript;
    int curAttack = 0;
    public int HP = 6;
    public int phase = 1;
    GameObject[] bloodOrbs = new GameObject[2];
    spawnCubeScript[] spawnCubes = new spawnCubeScript[2];
    [Header ("Sequence objects")]
    public SpriteRenderer[] skies = new SpriteRenderer[2];
    public Sprite[] skySprites = new Sprite[2];
    public GameObject[] phase2ToDestroy;
    public GameObject[] phase3ToDestroy;
    public GameObject[] finalPhaseElements;
    public eventTrigger phase3Beam;
    public Tilemap skullmap;
    public bool canStomp = false;
    public float stompPointOffset = 0;
    Coroutine cor3;
    public Collider2D flameSkullCol;
    bool pDead = false;
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        data = GameObject.Find("_GM").GetComponent<GameData>();
        dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        headAnim = GetComponent<Animator>();
        pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
        head = transform.GetChild(0);
        handsAnim = head.GetChild(1).GetComponent<Animator>();
        handsAI[0] = head.GetChild(1).GetChild(0).GetComponent<megaSatanHandCollisionDetect>();
        handsAI[1] = head.GetChild(1).GetChild(1).GetComponent<megaSatanHandCollisionDetect>();
        handGrav[0] = head.GetChild(1).GetChild(0).GetComponent<Gravity>();
        handGrav[1] = head.GetChild(1).GetChild(1).GetComponent<Gravity>();
        mFace = head.GetChild(0).GetChild(0).GetComponent<megaSatan_face>();
        enemyPocket = new GameObject("enemyPocket").transform;
        skullPocket = transform.GetChild(2);
        enemyPocket.transform.position = Vector3.zero;
        steamSpawn = transform.GetChild(3).gameObject;
        Transform tr = transform.GetChild(1);
        spikes = new Transform[tr.childCount];
        for(int i = 0; i<spikes.Length;i++)
        {
            spikes[i] = tr.GetChild(i);
        }
        tr = transform.GetChild(4);
        skullblocks = new GameObject[tr.childCount];
        for(int i = 0; i<skullblocks.Length;i++)
        {
            skullblocks[i] = tr.GetChild(i).gameObject;
        }
        inPhase = true;
        int chk = dataS.checkpointValue;
        if(chk==1)
        {
            data.echoInMainArea = true;
            data.audioEffectsToggle(2);
            if(data.storedItemID==0)
            data.storeItem(1,true);
        }
        else if(chk==2) //phase 2
        {
            headAnim.SetTrigger("Phase2Check");
            handsAnim.SetTrigger("Phase2Check");
            //cam.transform.position = new Vector3(57,65,-10);
            //pScript.transform.position = new Vector3(52.5f,60.2f,0);
            //transform.position = new Vector3(56,65.5f,0);
        }
        else if(chk==3) //phase 3
        {
            headAnim.SetTrigger("Phase3Check");
            phase3ToDestroy[0].gameObject.SetActive(true);
        }
        else if(chk>=4)
        {
            Destroy(flameSkullCol.gameObject);
            skies[1].sprite = skySprites[2];
            data.echoInMainArea = true;
            data.audioEffectsToggle(2);
            Destroy(enemyPocket.gameObject);
            foreach(GameObject obj in phase2ToDestroy)
            {
                Destroy(obj);
            }
            Destroy(enemyPocket.gameObject);
            foreach(GameObject obj in phase3ToDestroy)
            {
                Destroy(obj);
            }
            Destroy(enemyPocket.gameObject);
            if(chk==5)
            {
                Destroy(finalPhaseElements[0].gameObject);
                StartCoroutine(waitScroll());
                cam.lockCamera = true;
                cam.lockLeft = true;
                cam.lockRight = true;
                cam.lockUp = true;
                cam.lockDown = true;
                cam.lockscroll = true;
                cam.transform.position = new Vector3(451.5f,136.06f,-10);
            }
            else
            {
                StartCoroutine(waitScroll());
                cam.transform.position = new Vector3(90f,136.06f,-10);
                cam.lockCamera = false;
                cam.lockscroll = false;              
            }
        }
        Transform t = head.GetChild(3);
        for(int i = 0; i<bloodOrbs.Length;i++)
		{
			bloodOrbs[i] = t.GetChild(i).gameObject;
		}
        //assign spawn cubes
        t = head.GetChild(4);
        for(int i = 0; i<spawnCubes.Length;i++)
		{
			spawnCubes[i] = t.GetChild(i).GetComponent<spawnCubeScript>();
		}
        if(chk!=2&&chk!=3)
        StartCoroutine(phase1Attack());
        else
        {
            if(chk==2)
            {
                resetHands(2);
                setPhase(2);
                StartCoroutine(phase2Attack(true));
            }
            else if(chk==3)
            {
                Destroy(transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject);
                handEvent(36);
                setPhase(3);
                cor3 = StartCoroutine(phase3Attack(true));
                headAnim.ResetTrigger("PhaseTrigger");
            }
            StartCoroutine(gotoPhaseArena(true));
        }

        //debug
        //resetHands(1);
        //resetHands(2);
        //setPhase(2);
       // StartCoroutine(phase2Attack());
    }
    IEnumerator waitScroll()
    {
        yield return 0;
        enableFinalPhase();
        Destroy(gameObject);
    }
    void setPhase(int toSet)
    {
        phase = toSet;
        headAnim.SetInteger("Phase",phase);
        handsAnim.SetInteger("Phase",phase);
    }
    void Update()
    {
        if(pScript.dead&&!pDead)
        {
            pDead = true;
            headAnim.SetBool("Win",true);
            if(handsAnim!=null)
            handsAnim.SetBool("Win",true);
        }
    }
    public void decreaseHP()
    {
        HP--;
        if(phase!=2)
        {
            switch(HP)
            {
                default: handsAnim.speed = 1; break;
                case 4:
                case 3:
                handsAnim.speed = 1.25f; break;
                case 2:
                case 1:
                handsAnim.speed = 1.3f; break;
            }
            headAnim.speed = handsAnim.speed;
        }
        //print("Head anim speed: "+headAnim.speed);
    }
    public void handEvent(int e)
    {
        switch(e)
        {
            default: break;
            //move the left hand
            case 1:
            handsAI[0].pickPoint();
            break;
            //move the right hand
            case 2:
            handsAI[1].pickPoint();
            break;
            //stop the hands
            case 3:
            handsAI[0].moving = false;
            handsAI[1].moving = false;
            break;
            //enable gravity for left hand
            case 4:
            handGrav[0].enabled = true;
            break;
            //enable gravity for right hand
            case 5:
            handGrav[1].enabled = true;
            break;
            //disable gravity
            case 6:
            handGrav[0].enabled = false;
            handGrav[1].enabled = false;
            break;
            //left hand return
            case 7:
            handsAI[0].returnToStart();
            break;
            //right hand return
            case 8:
            handsAI[1].returnToStart();
            break;
            //reset
            case 9:
            handsAnim.SetInteger("Attack",0);
            break;
            //hurt
            case 10:
            //print("Hurt");
            mFace.looking = false;
            handsAnim.SetTrigger("Hurt");
            headAnim.SetTrigger("Hurt");
            data.playUnlistedSound(sounds[Random.Range(19,21)]);
            break;
            //hurt end
            case 11:
            mFace.looking = true;
            break;
            //kill hand
            case 12:
            mFace.looking = false;
            handsAnim.SetTrigger("Death");
            if(HP!=0)
            {
                if(phase<=1)
                headAnim.SetTrigger("Heavy");
                if(phase==2)headAnim.SetTrigger("Hurt");
                data.playUnlistedSound(sounds[Random.Range(19,21)]);
            }
            else
            {
                headAnim.speed = 1;
                headAnim.SetTrigger("Die");
            }
            break;
            //shake cam laser
            case 13:
            data.playUnlistedSound(sounds[4]);
            cam.constantShake = true;
            cam.easeShake = false;
            cam.shakeCamera(0.15f,10f);
            break;
            //shake cam end
            case 14:
            cam.constantShake = false;
            cam.shakeCamera(0.15f,0.5f);
            cam.easeShake = true;
            break;
            //laser eyes
            case 15:
            if(HP<=2)
            {
            headAnim.speed = handsAnim.speed;
            headAnim.SetTrigger("LaserEyes");
            }
            break;
            //reset idle speed
            case 16:
            if(headAnim!=null)
            headAnim.speed = 1;
            break;
            //phase 2 enter slam
            case 17:
            data.playUnlistedSound(sounds[0]);
            data.playUnlistedSound(sounds[1]);
            cam.easeShake = true;
            cam.shakeCameraVertically(2f,2f);
            //StartCoroutine(skullSpawnRow(6,0.05f,0,4,0));
            //StartCoroutine(skullSpawnRow(6,0.05f,22,-4,0.3f));
            StartCoroutine(skullSpawnRow(5,0.1f,13,2,0f));
            StartCoroutine(skullSpawnRow(5,0.1f,10,-2,0f));

            StartCoroutine(skullSpawnRow(6,0.05f,0,4,0.3f));
            StartCoroutine(skullSpawnRow(6,0.05f,23,-4,0.3f));
            
            if(pScript.grounded)
            pScript.rb.velocity = new Vector2(pScript.rb.velocity.x,10);
            break;
            //laser eyes warn sound
            case 18:
            StartCoroutine(warnSoundLaser());
            break;
            //start moving hands
            case 19:
            handsAI[0].movementScript.enabled = true;
            handsAI[1].movementScript.enabled = true;
            break;
            //greed spawn
            case 20:
            case 21:
            handsAI[e-20].spawnGreed();
            break;
            //stop moving hands
            case 22:
            case 23:
            handsAI[e-22].movementScript.enabled = false;
            break;
            //phase 1 raise hand after slam
            case 24:
            case 25:
            handsAI[e-24].raiseHand();
            break;
            //goto next phase arena
            case 26:
            StartCoroutine(gotoPhaseArena(false));
            break;
            case 27:
            case 28:
            splatterBlood(e-27);
            break;
            //left hand move to bleed start point
            case 29:
            handsAI[0].bleedPoint(45,4);
            break;
            //right hand move to bleed start point
            case 30:
            handsAI[1].bleedPoint(68,4);
            break;
            //left hand move to bleed end point
            case 31:
            handsAI[0].bleedPoint(68f,25);
            handsAI[0].startBleedSequence(true);
            break;
            //right hand move to bleed end point
            case 32:
            handsAI[1].bleedPoint(45,25);
            handsAI[1].startBleedSequence(false);
            break;
            //set hands start points
            case 33:
            handsAI[0].setStartPoint();
            handsAI[1].setStartPoint();
            break;
            //spawn cube
            case 34:
            spawnCube();
            break;
            //spawn eye
            case 35:
            //print("Spawning eye anim");
            if(handsAI[0].HP>0||handsAI[1].HP>0)
            headAnim.SetTrigger("AttackLaser");
            break;
            //destroy hands
            case 36:
            Destroy(handsAI[0].transform.parent.gameObject);
            Transform tr = head;
            tr.position+=Vector3.right*8;
            tr.position+=Vector3.down;
            tr.localScale = new Vector3(-1,1,1);
            headAnim.SetTrigger("PhaseTrigger");
            break;
            case 37:
            handsAI[0].resetPos();
            break;
            //shake cam hand explode
            case 38:
            data.playUnlistedSound(sounds[8]);
            cam.constantShake = true;
            cam.easeShake = false;
            cam.shakeCamera(0.3f,10f);
            phase3ToDestroy[0].gameObject.SetActive(true);
            break;
            //drop sound
            case 39:
            data.playUnlistedSound(sounds[7]);
            break;
            //skull sound
            case 40:
            case 41:
            data.playUnlistedSound(sounds[e-30]);
            break;
            //pre spawn skull block
            case 42:
            cam.easeShake = true;
            cam.shakeCamera(0.4f,3f);
            data.playUnlistedSound(sounds[13]);
            break;
            //spawn skull block
            case 43:
            spawnSkullBlock();
            break;
            //shoot beam
            case 44:
            phase3Beam.EventTriggered();
            break;
            case 45:
            cam.easeShake = true;
            cam.shakeCamera(0.4f,10f);
            break;
            case 46:
            data.playUnlistedSound(sounds[17]);
            cam.shakeCamera(0.2f,0.7f);
            pScript.rb.velocity = new Vector2(0,0);
            pScript.knockedBack = false;
            StartCoroutine(goToFinalArena());
            break;
            case 47:
            flameSkullCol.transform.position = head.GetChild(0).position+new Vector3(0,-0.25f,0);
            flameSkullCol.enabled = true;
            break;
        }
    }
    IEnumerator destroySkullTiles()
    {
        BoundsInt b = skullmap.cellBounds;
        int count = 0;
        for(int y = 38;y<50;y++)
        {
            bool found = false;
            for(int x = 0;x<b.size.x;x++)
            {
                Vector3Int tempPos = new Vector3Int(x,y,0);
                if(skullmap.GetTile(tempPos)!=null)
                {
                    found = true;
                    Transform tr = skullParticleHolder.GetChild(count);
                    tr.position = new Vector3(tempPos.x+0.5f,tempPos.y+0.5f,0);
                    skullmap.SetTile(tempPos,null);
                    tr.gameObject.SetActive(true);
                    //print(tempPos+" has tile.");
                    count++;
                }
            }
            if(found)
            {
                data.playUnlistedSound(sounds[14+Random.Range(0,2)]);
                cam.shakeCamera(0.2f,0.5f);
            }
            if(y>40)
            yield return new WaitForSeconds(0.05f);
        }
    }
    public void destroySkullBlocks()
    {
        StartCoroutine(destroySkullTiles());
    }
    public void destroyAllSkullsAndSkullBlocks()
    {
        for(int i = 0;i<skullblocks.Length;i++)
        {
            if(skullblocks[i].activeInHierarchy)
            {
                skullblocks[i].GetComponent<MSSkull>().kill();
            }
        }
        for(int i = 0;i<skullPocket.childCount;i++)
        {
            if(skullPocket.GetChild(i).gameObject.activeInHierarchy)
            {
                skullPocket.GetChild(i).GetComponent<MSSkull>().kill();
            }
        }
        //destroy skull tiles all at once
        BoundsInt b = skullmap.cellBounds;
        int count = 0;
        for(int y = 37;y<50;y++)
        {
            for(int x = 0;x<b.size.x;x++)
            {
                Vector3Int tempPos = new Vector3Int(x,y,0);
                if(skullmap.GetTile(tempPos)!=null)
                {
                    skullmap.SetTile(tempPos,null);
                    if(y>37)
                    {
                        if(count<skullParticleHolder.childCount-1)
                        {
                        Transform tr = skullParticleHolder.GetChild(count);
                        tr.position = new Vector3(tempPos.x+0.5f,tempPos.y+0.5f,0);
                        tr.gameObject.SetActive(true);
                        }
                        //print(tempPos+" has tile.");
                        count++;
                    }
                }
            }
        }
        data.playUnlistedSound(sounds[14]);
        data.playUnlistedSound(sounds[15]);
        skullParticleHolder.GetChild(12).gameObject.SetActive(true);
        Destroy(phase3ToDestroy[1]);
    }
    public void spawnSkullBlock()
    {
        float xPos = Mathf.Round(pScript.transform.position.x-0.5f)+0.5f;
        for(int i = 0;i<skullblocks.Length;i++)
        {
            if(!skullblocks[i].activeInHierarchy)
            {
                skullblocks[i].transform.position = new Vector3(Mathf.Clamp(xPos,48.5f,59.5f),46.53f,0);
                skullblocks[i].SetActive(true);
                break;
            }
        }
    }
    public void splatterBlood(int hand)
	{
        Vector3 pos = handsAI[hand].transform.GetChild(0).position-(Vector3.up*0.5f);
		GameObject obj = Instantiate(bloodOrbs[0],pos+new Vector3(0,0.4f,0),Quaternion.identity);
		obj.transform.localScale = Vector3.one;
		obj.SetActive(true);
        data.playUnlistedSound(sounds[Random.Range(0,2)]);
        cam.easeShake = true;
		cam.shakeCameraVertically(0.5f,2f);
	}
    public void dripBlood(float xPoint,float yPoint)
    {
        Vector3 pos = new Vector3(Mathf.Floor(xPoint)+0.5f,yPoint,0);
        data.playUnlistedSoundPoint(sounds[12],pos);
        GameObject obj = Instantiate(bloodOrbs[1],pos,Quaternion.identity);
		obj.transform.localScale = Vector3.one;
		obj.SetActive(true);
    }
    void spawnCube()
    {
        for(int i = 0;i<spawnCubes.Length;i++)
        {
            if(!spawnCubes[i].active)
            {
                spawnCubes[i].activate();
                break;
            }
        }
    }
    public void playWarnSound(Vector3 pos,int times)
    {
        StartCoroutine(playWarning(pos,times));
    }
    public void playCrush()
    {
        data.playUnlistedSound(sounds[18]);
    }
    void resetHands(int newHP)
    {
        handsAI[0].HP = newHP;
        handsAI[1].HP = newHP;
        HP = newHP*2;
        handsAnim.speed = 1;
        headAnim.speed = handsAnim.speed;
        handsAI[0].gameObject.SetActive(true);
        handsAI[1].gameObject.SetActive(true);
    }
    public void playSound(int index)
    {
        data.playUnlistedSound(sounds[index]);
    }
    public void enableFinalPhase()
    {
        foreach(GameObject obj in finalPhaseElements)
        {
            if(obj!=null)
            obj.SetActive(true);
        }
    }
    IEnumerator gotoPhaseArena(bool startSwitch)
    {
        if((cam.fadeAnim<=0&&!pScript.dead)||startSwitch)
        {
            if(!startSwitch)
            {
                data.playUnlistedSound(sounds[9]);
                cam.flashWhite();
                yield return 0;
                yield return new WaitUntil(()=>!cam.flash);
            }
            else if(phase<=2)
            {
                yield return 0;
                handsAI[0].movementScript.enabled = true;
                handsAI[1].movementScript.enabled = true;
            }
            data.switchArea(true);
            data.switchParallax(true);
        if(phase==2)
        {
            cam.transform.position = new Vector3(57,65,-10); //second lowest
            pScript.transform.position = new Vector3(52.5f,60.2f,0);
            transform.position = new Vector3(56,65.5f,0);
            //print("go to second phase");
        }
        else
        {
            Destroy(enemyPocket.gameObject);
            foreach(GameObject obj in phase2ToDestroy)
            {
                Destroy(obj);
            }
            if(startSwitch)yield return 0;
            skies[1].sprite = skySprites[0];
            cam.transform.position = new Vector3(57,43,-10); //lowest
            pScript.transform.position = new Vector3(51.5f,38.2f,0);
            transform.position = new Vector3(56,44,0);
            //print("go to third phase");
            headAnim.speed = 1;
        }
        }
    }
    IEnumerator goToFinalArena()
    {
        if(cam.fadeAnim<=0&&!pScript.dead)
        {
        cam.fadeScreen(true);
        yield return 0;
        yield return new WaitUntil(()=>cam.fadeAnim>=1);
        foreach(GameObject obj in phase3ToDestroy)
        {
            Destroy(obj);
        }
        yield return new WaitForSeconds(0.5f);
        skies[1].sprite = skySprites[2];
        cam.transform.position = new Vector3(86.2f,136.06f,-10);
        pScript.transform.position = new Vector3(78.5f,147,0);
        transform.position = new Vector3(98.5f,136.5f,0);
        cam.fadeScreen(false);
        yield return new WaitUntil(()=>cam.fadeAnim<=0.05f);
        headAnim.SetInteger("Phase",4);
        headAnim.SetTrigger("PhaseTrigger");
        cam.AssignTarget(transform);
        cam.lockRight = false;
        cam.lockLeft = false;
        cam.lockCamera = false;
        cam.lockscroll = false;
        yield return new WaitForSeconds(2f);
        cam.AssignTarget(pScript.transform);
        cam.overWriteLockScroll =true;
        cam.lockscroll = false;
        cam.lockLeft = false;
        yield return new WaitForSeconds(1f);
        cam.overWriteLockScroll =false;
        pScript.goToCutsceneMode(false);
        enableFinalPhase();
        yield return 0;
        Destroy(gameObject);
        }
    }
    public IEnumerator spawnEnemy(Vector3 pos,int min,int max)
    {
        int random = Random.Range(min,max+1);
        steamSpawn.transform.position = pos;
        steamSpawn.SetActive(true);
        data.playUnlistedSoundPoint(sounds[6],pos);
        yield return new WaitForSeconds(0.1f);
        GameObject obj = Instantiate(enemies[random],pos,Quaternion.identity);
        obj.transform.SetParent(enemyPocket);
    }
    IEnumerator warnSoundLaser()
    {
        Vector3 pos = new Vector3(57,80,0);
        yield return new WaitForSeconds(0.1f);
        data.playUnlistedSoundPoint(sounds[5],pos);
        yield return new WaitForSeconds(0.2f);
        data.playUnlistedSoundPoint(sounds[5],pos);
    }
    IEnumerator playWarning(Vector3 pos,int times)
    {
        for(int i = 0; i<times;i++)
        {
            yield return new WaitForSeconds(0.3f);
            data.playUnlistedSoundPoint(sounds[5],pos);
        }
    }
    IEnumerator phase1Attack()
    {
        //intro
        yield return new WaitForSeconds(2f);
        while(inPhase)
        {
            //1 - left smash
            //2 - lasers
            //3 - right smash
            curAttack++;
            if(curAttack>3)curAttack=1;
            switch(curAttack)
            {
                default: break;
                case 1:
                if(!handsAI[0].gameObject.activeInHierarchy)
                {
                    if(handsAI[1].gameObject.activeInHierarchy)
                    {
                        //print("skip left hand, go to right");
                        curAttack=3;
                    }
                    else inPhase = false;
                }
                break;
                case 2:
                if(!handsAI[0].gameObject.activeInHierarchy||!handsAI[1].gameObject.activeInHierarchy)
                {
                    if(handsAI[1].gameObject.activeInHierarchy)
                    {
                        //print("skip laser, go to right");
                        curAttack=3;
                    }
                    else if(handsAI[0].gameObject.activeInHierarchy)
                    {
                        //print("skip laser, go to left");
                        curAttack=1;
                    }
                    else inPhase = false;
                }
                break;
                case 3:
                if(!handsAI[1].gameObject.activeInHierarchy)
                {
                    if(handsAI[0].gameObject.activeInHierarchy)
                    {
                        //print("skip right hand, go to left");
                        curAttack=1;
                    }
                    else inPhase = false;
                }
                break;
            }
            yield return 0;
            if(inPhase&&HP!=0)
            {
                //print("Current attack: "+curAttack);
                if(curAttack==2)
                {
                headAnim.speed = handsAnim.speed;
                headAnim.SetTrigger("AttackLaser");
                }
                else yield return new WaitForSeconds(0.5f);
                handsAnim.SetInteger("Attack",curAttack);
                handsAnim.SetTrigger("AttackTrigger");
                yield return new WaitUntil(()=>handsAnim.GetInteger("Attack")==0);
            }
            //else print("Exit");
        }
        setPhase(2);
        StartCoroutine(phase2Attack(false));
    }
    IEnumerator phase2Attack(bool skipWait)
    {
        if(!skipWait)
        {
            yield return new WaitForSeconds(2f);
            resetHands(2);
            headAnim.SetTrigger("PhaseTrigger");
            handsAnim.SetTrigger("PhaseTrigger");
            yield return new WaitForSeconds(8f);
        }
        else
        {
            yield return 0;
            yield return 0;
            handEvent(33);
        }
        //print("Entering phase 2");
        inPhase = true;
        int atk = Random.Range(1,3);
        while(inPhase)
        {
            //attack cycle
            atk++;
            if(atk>3)atk = 1;

            else if(atk==3)
            {
                //0 = no bleed, 1 = left bleed, 2 = right bleed, 3 = both bleed
                int bleedVal = 0;
                if(handsAI[0].HP==1) bleedVal+=1;
                if(handsAI[1].HP==1)bleedVal+=2;
                switch(bleedVal)
                {
                    default: //no bleed
                    atk=1;
                    break;
                    case 1: //left bleed
                    atk=3;
                    break;
                    case 2: //right bleed
                    atk=4;
                    break;
                    case 3: //both bleed
                    atk=5;
                    break;
                }
                //print("Bleed val: "+bleedVal);
            }
            //print(atk);
            if(atk<=2&&enemyPocket.childCount<5)
            {
                int atkF = atk;
                if(!handsAI[0].gameObject.activeInHierarchy)atkF=2;
                else if(!handsAI[1].gameObject.activeInHierarchy)atkF=1;
                handsAnim.SetInteger("Attack",atkF);
                handsAnim.SetTrigger("AttackTrigger");
                bool inIdle = false;
                while(!inIdle)
                {
                    if(handsAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="mega_satan_hands_idle_2")
                        inIdle = true;
                    else yield return new WaitForSeconds(0.5f);
                    yield return 0;
                }
            }
            else //bleed attack
            {
                bool inIdle = false;
                while(!inIdle)
                {
                    if(handsAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="mega_satan_hands_idle_2")
                        inIdle = true;
                    yield return 0;
                }
                handsAnim.SetInteger("Attack",atk);
                handsAnim.SetTrigger("AttackTrigger");
                headAnim.SetTrigger("LaserEyes"); //just live with it okay I've been doing this shit for 6 months
                inIdle = false;
                while(!inIdle)
                {
                    if(handsAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="mega_satan_hands_idle_2")
                        inIdle = true;
                    else yield return new WaitForSeconds(0.5f);
                    yield return 0;
                }
            }
            yield return 0;
            if(!handsAI[0].gameObject.activeInHierarchy&&
            !handsAI[1].gameObject.activeInHierarchy)
            {
                inPhase = false;
            }
            else
            yield return new WaitForSeconds(4f);
        }
        //print("Exiting phase 2");
        setPhase(3);
        cor3 = StartCoroutine(phase3Attack(false));
    }
    IEnumerator phase3Attack(bool skipWait)
    {
        int skullCounter = 0;
        if(!skipWait)
        {
            yield return new WaitForSeconds(0.1f);
            resetHands(1);
            handsAnim.SetTrigger("PhaseTrigger");
            yield return new WaitForSeconds(10f);
        }
        else
        {
            yield return new WaitForSeconds(2.5f);
        }
        inPhase = true;
        float xPos = pScript.transform.position.x;
        //print("Starting attacking in phase 3.");
        while(inPhase)
        {
            if(skullCounter<6)
            {
                skullCounter++;
                if(xPos<=60)
                headAnim.SetTrigger("LaserEyes"); //He spawns skull blocks with this shut your whore mouth.
                yield return new WaitForSeconds(1.5f);
                xPos = pScript.transform.position.x;
                if(xPos<=60)
                {
                    StartCoroutine(skullSpawn(1,0,true,true));
                    yield return new WaitForSeconds(0.75f);
                    StartCoroutine(skullSpawn(2,0.75f,true,false));
                    yield return new WaitForSeconds(1.5f);
                }
            }
            else
            {
                skullCounter = 0;
                headAnim.SetTrigger("AttackLaser");
                yield return new WaitForSeconds(5f);
            }
        }
    }
    IEnumerator spikeRaise(int amount,float startPoint,bool left)
    {
        for(int i = 0;i<amount;i++)
        {
            for(int x = 0; x<spikes.Length;x++)
            {
                if(!spikes[x].gameObject.activeInHierarchy)
                {
                    if(left)
                    spikes[x].transform.position = new Vector3(startPoint-i-1,spikes[x].transform.position.y,spikes[x].transform.position.z);
                    else spikes[x].transform.position = new Vector3(startPoint+i+1,spikes[x].transform.position.y,spikes[x].transform.position.z);
                    spikes[x].gameObject.SetActive(true);
                    data.playUnlistedSoundPoint(sounds[2],spikes[x].transform.position);
                    break;
                }
            }
            yield return new WaitForSeconds(spikeDelay);
        }
    }
    IEnumerator skullSpawn(int amountOfSkulls,float delay,bool narrowBounds,bool trackPlayer)
    {
        float lastPos = 0;
        for(int i = 0;i<amountOfSkulls;i++)
        {
            for(int x = 0;x<skullPocket.childCount;x++)
            {
                Transform tr = skullPocket.GetChild(x);
                if(!tr.gameObject.activeInHierarchy)
                {
                    float newPos = lastPos;
                    if(!trackPlayer)
                    {
                        while(newPos==lastPos)
                        {
                            if(!narrowBounds)
                            newPos = ((int)Random.Range(45,69))+0.5f;
                            else newPos = ((int)Random.Range(48,59))+0.5f;
                        }
                    }
                    else
                    {
                        newPos = Mathf.Round(Mathf.Clamp(pScript.transform.position.x-0.5f,48.5f,59.5f))+0.5f;
                    }
                    lastPos = newPos;
                    if(!narrowBounds)
                    tr.transform.position = new Vector3(newPos,86.5f,tr.transform.position.z);
                    else tr.transform.position = new Vector3(newPos,46.53f,tr.transform.position.z);
                    //print(tr.transform.position);
                    tr.gameObject.SetActive(true);
                    break;
                }
            }
            yield return new WaitForSeconds(delay);
        }
    }
    IEnumerator skullSpawnRow(int amountOfSkulls,float delay,float offset,int gap,float spawnDelay)
    {
        float lastPos = 0;
        yield return new WaitForSeconds(spawnDelay);
        for(int i = 0;i<amountOfSkulls;i++)
        {
            for(int x = 0;x<skullPocket.childCount;x++)
            {
                Transform tr = skullPocket.GetChild(x);
                if(!tr.gameObject.activeInHierarchy)
                {
                    tr.transform.position = new Vector3((45+lastPos+offset)+0.5f,86.5f,tr.transform.position.z);
                    lastPos+=gap;
                    //print(tr.transform.position);
                    tr.gameObject.SetActive(true);
                    break;
                }
            }
            yield return new WaitForSeconds(delay);
        }
    }
    public void spawnSpikes(int leftAmount,int rightAmount,float startPoint)
    {
        data.playUnlistedSound(sounds[Random.Range(0,2)]);
        cam.easeShake = true;
        cam.shakeCameraVertically(1.5f,2f);
        //print("Startpoint: "+startPoint);
        //print("Left: "+leftAmount);
        //print("Right: "+rightAmount);
        //if(HP<=2)StartCoroutine(skullSpawn(12,0.1f));
        if (HP<=4) StartCoroutine(skullSpawn(9,0.2f,false,false));
        else StartCoroutine(skullSpawn(6,0.4f,false,false));
        StartCoroutine(spikeRaise(leftAmount,startPoint,true));
        StartCoroutine(spikeRaise(rightAmount,startPoint,false));
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(phase==3)
        {
        if(other.tag=="Player")
        {
            Transform tr = pScript.transform;
			if(canStomp&&pScript.rb.velocity.y<-0.1f&&
            tr.position.y>head.position.y+stompPointOffset)
			{
                StopCoroutine(cor3);
				//Debug.Log("Stomped");
                inPhase = false;
                HP = 0;
                headAnim.SetTrigger("Hurt");
                cam.shakeCamera(0.2f,1f);
                data.playUnlistedSoundPoint(sounds[16],head.position);
				pScript.stompBoss(gameObject,true);
                //pScript.knockbackCor = StartCoroutine(pScript.knockBack(-1,0.1f,0.5f));
                pScript.goToCutsceneMode(true);
                pScript.knockedBack = true;
                pScript.rb.velocity = new Vector2(-8,0.1f);
                destroyAllSkullsAndSkullBlocks();
			}
			else
			{
				if(Time.timeScale!=0 && pScript.invFrames==0)
				{
					pScript.Damage(false,false);
				}
				if(pScript.knockbackCor !=null)
				StopCoroutine(pScript.knockbackCor);
				if(tr.position.x<head.position.x)
				pScript.knockbackCor = StartCoroutine(pScript.knockBack(-1,1,0.5f,true));
				else
				pScript.knockbackCor = StartCoroutine(pScript.knockBack(1,1,0.5f,true));
			}
        }
        }
    }
}