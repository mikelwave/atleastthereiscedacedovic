using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class playuhBossScript : MonoBehaviour
{
	public int intensity = 0; //0-4
	public int hits = 0;
	int phase = 1;
    public float horizontalSpeed = 4,groundDashSpeed = 4;
    public bool actionJump = false,actionDash = false,actionShoot = false,actionSpecial = false,lockChangeDirection = false,canTakeDamage = false,lookforWall = false;
	public int cheeseFrameWait = 10,csgoFrameWait = 20,lKnifeFrameWait = 20;
	public Vector2 axeSpinVelocity = new Vector2(-5,5);
	[Space]
	[Header("Sounds")]
	public AudioClip[] sounds;
	public AudioClip[] music;
	public int[] tempos = new int[2]{300,255};
	AudioSource asc,warnSoundAsc;
    [Space]
    bool midJump = false,midDive = false,grounded = true,releasedJump = false,canDive = false,diveNudge = false, frameCollision = false,stickingToGround = false,touchingGround = false,pressedOppositeDirInAir = false;
    float jumpForce = 1;
    public float releaseJumpVelocity = 6,startJumpPoint = 0,currentJumpPoint = 0;
    public float maxHeight = 4f;
	int hp = 6,damageFrames = 0;
    public Vector3 jumpHeights = new Vector3(2100,2200,2400);
    public LayerMask whatIsGround;
    public bool interruptDive = false;
	public bool canInterruptDive = false;
	float savedGravity;
	float savedMax;
    PolygonCollider2D pCol;
    Coroutine stickToGround,diveCor,powerUpCor,trailCor,specialCor,shootCor;
	Vector2 colliderHeight = new Vector2(0.7f,1.4f);
	Vector2[] pathPoints;
	Gravity grav;
	Transform player,auraTransform;
	PlayerScript pScript;
	PlayerMusicBounce pBounce;
	MGCameraController cam;
	Tilemap map;
	LineRenderer lRender;

	AxisSimulator axis;
    Rigidbody2D rb;
    Animator anim,targetUIAnim;
    public GameData data;
	GameObject axeAura,targetUI;
	colorFade swishEffect;
	public int powerUpState = 0; //0 = normal, 1 = fire, 2 = axe, 3 = cape, 4 = csgo knife, 5 = L knife
	public Color[] colors = new Color[2];
	public Material[] materials;
	public PhysicsMaterial2D[] physicsMaterials;
	Transform[] spikes;
	[Header ("Sprite arrays")]
	[Space]
	public Sprite[] bigSprites;
	public Sprite[] fireSprites,axeSprites,capeSprites,csgoSprites,lKnifeSprites;
	public Sprite[] items;
	Sprite lastSprite = null; //remember last sprite
	int spriteIndex = -1;
	SpriteRenderer render,itemRender;
	public int targetState = 0;
	public int auraWaitFrames = 0;
	int wallsToHit = 0;
	int fireWaitFrames = 0;
	public bool changeState = false;
	bool dontUnlockDamagePass = false;
	bool auraEnabled = false;
	bool canMove = true;
	bool trackerFollow = false;
	bool playerFollow = false;
	bool finalJump = false;

	//Projectiles
	[Header ("Projectiles")]
	[Space]
	public GameObject cheeseBall;
	public GameObject knife,homingKnife,lKnife;
	List<GameObject> balls;
	List<GameObject> csgoKnives;
	List<GameObject> homingKnives;
	List<GameObject> lKnives;
	Transform[] knifePoints = new Transform[3];
	Vector3[] dashPoints = new Vector3[4];
	GameObject trailSprite;
	SpriteRenderer[] trailSprites = new SpriteRenderer[10];
	//Wall scan variables
	Vector2 scanDir = Vector2.right;
	float scanLength = 2;
	int jumpAmount = 0,shootAmount = 0;
	//index
	int curPatternIndex=-1;
	int[] attackPattern = new int[5];
	int[] forbiddenPowerUps = new int[5];
	[HideInInspector]
	public bool midIntro = false;
	//Intro assets
	dataShare DataS;
	public Transform cutsceneQuad;
	public colorFade colorPulse;
	//outro assets
	GameObject[] particles;
	public GameObject axeBurek;
	bigBlockScript axeBurekBlock;
	[Header ("Phase 2")]
	public float camSpeed = 3;

	IEnumerator auraAnim()
    {
		yield return new WaitUntil(()=>!midIntro);
		auraTransform.gameObject.SetActive(true);
        //data.playSound(98,auraTransform.position);
        Vector3 targetScale = Vector3.one*1.5f,startScale = Vector3.zero;
        float progress = 0;
		//Color color = auraRender.color,targetColor = new Color(color.r,color.g,color.b,0);
		//color = new Color(color.r,color.g,color.b,1);
		//auraRender.color = color;
		auraTransform.localScale = Vector3.zero;
        while(progress<1)
        {
            progress = Mathf.Clamp(progress+=Time.deltaTime*20,0,1);
            auraTransform.localScale = Vector3.Lerp(startScale,targetScale,progress);
            yield return 0;
            yield return new WaitUntil(()=>Time.timeScale!=0);
        }
        startScale = auraTransform.localScale;
        targetScale = Vector3.one;
        progress = 0;
        while(progress<1)
        {
            progress = Mathf.Clamp(progress+=Time.deltaTime*20,0,1);
            auraTransform.localScale = Vector3.Lerp(startScale,targetScale,progress);
            yield return 0;
            yield return new WaitUntil(()=>Time.timeScale!=0);
        }
		//wait for a bit
		auraWaitFrames = 120;
		while(auraWaitFrames>0)
		{
			yield return 0;
			auraWaitFrames--;
			yield return new WaitUntil(()=>Time.timeScale!=0);
		}
		startScale = auraTransform.localScale;
        targetScale = Vector3.zero;
		progress = 0;
        while(progress<1)
        {
            progress = Mathf.Clamp(progress+=Time.deltaTime*20,0,1);
            auraTransform.localScale = Vector3.Lerp(startScale,targetScale,progress);
			//auraRender.color = Color.Lerp(color,targetColor,progress);
            yield return 0;
            yield return new WaitUntil(()=>Time.timeScale!=0);
        }
		auraTransform.gameObject.SetActive(false);
		auraEnabled = false;
		if(!dontUnlockDamagePass)
		canTakeDamage = true;
		else dontUnlockDamagePass = false;
    }
	IEnumerator diveTime (float lengthOfDive,bool inDir)
	{
		yield return new WaitForSeconds(0.2f-(0.1f*(intensity/5)));
		canTakeDamage = false;
		midDive = true;
		diveNudge = false;
		float diveLength = 0;
		if(!inDir)
			diveLength = lengthOfDive;
		else
		{
			diveLength = 0.4f;
		}
		pathPoints[1] = new Vector2(0,colliderHeight.x);
		pCol.SetPath(0,pathPoints);
		axis.acceptFakeInputs = false;
		savedGravity = grav.pushForces.y;
		savedMax = grav.maxVelocities.x;
		grav.maxVelocities = new Vector2(20f,grav.maxVelocities.y);
		grav.pushForces = new Vector2(grav.pushForces.x,0);
		anim.SetBool("gravity",false);
		interruptDive = false;
		while(diveLength > 0f && rb.velocity.x != 0)
		{
			diveLength -= Time.deltaTime;
			yield return 0;
		}
		diveLength = 0;
		releasedJump = true;
		grav.pushForces = new Vector2(grav.pushForces.x,-Mathf.Abs(savedGravity));
		anim.SetBool("gravity",true);
		canTakeDamage = true;
		while(!grounded && anim.GetBool("dive"))
		{
			yield return 0;
			if(Mathf.Abs(rb.velocity.x)<0.05f&&rb.bodyType!=RigidbodyType2D.Static)
			rb.velocity = new Vector2(0.05f*transform.localScale.x,rb.velocity.y);
		}
		if(powerUpState==1)horizontalSpeed = 2+((float)intensity/5);
		axis.axisAdder = 0.10f;
		yield return new WaitForSeconds(0.5f-((float)intensity/10));
		midDive = false;
		canInterruptDive = true;
		axis.acceptFakeInputs = true;
		yield return new WaitUntil(() => rb.velocity.x == 0 || interruptDive || !anim.GetBool("dive"));
		interruptDive = false;
		canInterruptDive = false;
		grav.maxVelocities = new Vector2(savedMax,grav.maxVelocities.y);
		anim.SetBool("dive",false);
		axis.setRange(1.9f);

		if(grounded)
			axis.axisAdder = 0.07f;
		else axis.axisAdder = 0.10f;
		pathPoints[1] = new Vector2(0,colliderHeight.y);
		pCol.SetPath(0,pathPoints);
		diveCor = null;
	}
    void generatePattern()
	{
		int[] prevPattern = new int[5];
		//string s2 = "Previous: ";
		for(int i = 0;i<prevPattern.Length;i++)
		{
			prevPattern[i]=attackPattern[i];
			//s2+=prevPattern[i]+" ";
		}
		//print(s2);
		for(int i = 0;i<attackPattern.Length;i++)
		{
			attackPattern[i] = i+1;
		}
		//randomize pattern
		System.Random rnd = new System.Random();
        var randomizedList = (from item in attackPattern
                              orderby rnd.Next()
                              select item).ToArray();
 
		attackPattern = randomizedList;
		string s ="Pattern: ";
		//swap if last attack of previous same as the first of new pattern
		if(prevPattern[4]==attackPattern[0])
		{
			//print("Swapping "+attackPattern[0]+" with "+attackPattern[4]+" because of last being: "+prevPattern[4]);
			int tmp = attackPattern[0];
			attackPattern[0]=attackPattern[4];
			attackPattern[4]=tmp;
		}
		for(int i = 0;i<attackPattern.Length;i++)
		{
        	s+=attackPattern[i].ToString()+" ";
		}
		//print(s);
	}
	void patternProgress(int repeatCount)
	{
		int repeats = repeatCount;
		curPatternIndex++;
		if(curPatternIndex>4)
		{
			curPatternIndex = 0;
			generatePattern();
		}
		targetState = attackPattern[curPatternIndex];
		if(forbiddenPowerUps.Contains(targetState))
		{
			repeats++;
			if(repeats>10)
			{
				changeState = false;
				//print("No more powerups");
				return;
			}
			patternProgress(repeats);
		}
		canTakeDamage = false;
		changeState = true;
		return;	
	}
	void addToForbidden(int toAdd)
	{
		for(int i = 0;i<forbiddenPowerUps.Length;i++)
		{
			if(forbiddenPowerUps[i]==0)
			{
				forbiddenPowerUps[i]=toAdd;
				break;
			}
		}
	}
	// Start is called before the first frame update
    void Start()
    {
		asc = GetComponent<AudioSource>();
        data = GameObject.Find("_GM").GetComponent<GameData>();
		DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		pCol = GetComponent<PolygonCollider2D>();
		player = GameObject.Find("Player_main").transform;
		pScript = player.GetComponent<PlayerScript>();
		pBounce = pScript.transform.GetComponent<PlayerMusicBounce>();
		anim = transform.GetChild(0).GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		grav = GetComponent<Gravity>();
		axis = GetComponent<AxisSimulator>();
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		if(DataS.mode==1)
		{
			materials[0]= materials[3];
			render.material = materials[0];
		}
		if(DataS.checkpointValue==1)//phase 2
		{
			phase = 2;
			transform.localScale = Vector3.one;
			GameObject.Find("Door 0").GetComponent<DoorScript>().DoorEventTriggered();
			return;
		}
		itemRender = transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
		auraTransform = transform.GetChild(0).GetChild(0).GetChild(0);
		axeAura = transform.GetChild(0).GetChild(3).gameObject;
		swishEffect = axeAura.transform.GetChild(0).GetComponent<colorFade>();
		lRender = transform.GetChild(0).GetChild(6).GetComponent<LineRenderer>();
		auraTransform.gameObject.SetActive(false);
		if(DataS.difficulty>0)intensity = 2;
		map = GameObject.Find("MainMap").GetComponent<Tilemap>();
        jumpForce = jumpHeights.x;
		pathPoints = pCol.GetPath(0);
		savedGravity = grav.pushForces.y;
		savedMax = grav.maxVelocities.x;
		trailSprite = transform.parent.GetChild(8).gameObject;
		targetUI = transform.parent.GetChild(9).gameObject;
		particles = new GameObject[4];
		axeBurekBlock=transform.parent.GetChild(12).GetComponent<bigBlockScript>();
		axeBurekBlock.storeItem(axeBurek);
		for(int i = 0;i<particles.Length;i++)
		{
			particles[i]=transform.parent.GetChild(11).GetChild(i).gameObject;
		}
		targetUIAnim = targetUI.GetComponent<Animator>();
		warnSoundAsc = targetUI.GetComponent<AudioSource>();
		Transform tr = transform.parent.GetChild(10);
        spikes = new Transform[tr.childCount];
        for(int i = 0; i<spikes.Length;i++)
        {
            spikes[i] = tr.GetChild(i);
        }
		trailSprites[0]=trailSprite.GetComponent<SpriteRenderer>();
		for(int i = 1;i<trailSprites.Length;i++)
		{
			trailSprites[i]=Instantiate(trailSprite,transform.position,Quaternion.identity).GetComponent<SpriteRenderer>();
			trailSprites[i].transform.SetParent(transform.parent);
		}
		for(int i = 0;i<knifePoints.Length;i++)
		{
			knifePoints[i] = transform.parent.GetChild(i+1);
		}
		for(int i = 0;i<dashPoints.Length;i++)
		{
			dashPoints[i] = transform.parent.GetChild(i+4).position;
		}
		//load projectiles
		balls = new List<GameObject>();
		for(int i = 0; i<10;i++)
		{
			GameObject obj = Instantiate(cheeseBall,transform.position,Quaternion.identity);
			obj.SetActive(false);
			obj.transform.SetParent(transform);
			obj.GetComponent<Jumper>().data = data;
			obj.GetComponent<Jumper>().setParent = transform;
			balls.Add(obj);
		}
		csgoKnives = new List<GameObject>();
		for(int i = 0;i<9;i++)
		{
			GameObject obj = Instantiate(knife,transform.position,Quaternion.identity);
			knifeScript ksc = obj.GetComponent<knifeScript>();
			ksc.main = transform;
			obj.SetActive(false);
			csgoKnives.Add(obj);
		}
		homingKnives = new List<GameObject>();
		for(int i = 0;i<9;i++)
		{
			GameObject obj = Instantiate(homingKnife,transform.position,Quaternion.identity);
			knifeScript ksc = obj.GetComponent<knifeScript>();
			ksc.main = transform;
			obj.SetActive(false);
			homingKnives.Add(obj);
		}
		lKnives = new List<GameObject>();
		for(int i = 0;i<12;i++)
		{
			GameObject obj = Instantiate(lKnife,transform.position,Quaternion.identity);
			knifeScript ksc = obj.GetComponent<knifeScript>();
			ksc.main = transform;
			obj.SetActive(false);
			lKnives.Add(obj);
		}
		generatePattern();
		if(Application.isPlaying)
        cutsceneQuad.gameObject.SetActive(true);
		StartCoroutine(introSequence());
	}
	public void playSound(int id,bool oneShot)
	{
		if(id<sounds.Length)
		{
			if(oneShot)
			asc.PlayOneShot(sounds[id]);
			else
			{
				if(asc.isPlaying)asc.Stop();
				asc.clip = sounds[id];
				asc.Play();
			}
		}
	}
	void stopAllSound()
	{
		if(asc.isPlaying)asc.Stop();
	}
	IEnumerator shoot()
	{
		anim.SetBool("shoot",true);
		yield return new WaitForSeconds(0.15f);
		anim.SetBool("shoot",false);
	}
    IEnumerator introSequence()
	{
		if(pScript.pSprites.state==0)
		{
			pScript.pSprites.state=3;
			pScript.SetStateProperties(3);
		}
		pScript.pauseMenu.enabled = false;
		yield return new WaitUntil(()=>Time.timeScale!=0);
		if(data.storedItemID==0)
		data.storeItem(3,true);
        cam.fadeScreen(false);
		midIntro = true;
		pScript.goToCutsceneMode(true);
		yield return new WaitForSeconds(1f);
		patternProgress(0);
		yield return 0;
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_item");
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		yield return new WaitForSeconds(0.5f);
		pScript.pauseMenu.enabled = true;
        pBounce.StopCoroutine(pBounce.bouncing);
		pBounce.BPM = tempos[0];
        pBounce.bouncing = pBounce.StartCoroutine(pBounce.bounce());
		pBounce.colorPulse = colorPulse;
		data.changeMusicWithIntro(music[0],music[1],tempos[0]);
	}
	void stateSprite()
	{
		if(lastSprite!=render.sprite) //find what sprite is currently displaying
		{
			for(int i = 0;i<bigSprites.Length;i++)
			{
				if(bigSprites[i]==render.sprite)
				{
					spriteIndex = i;
					break;
				}
				if(i==bigSprites.Length-1&&bigSprites[i]!=render.sprite)
				{
					spriteIndex = -1;
				}
			}
			lastSprite = render.sprite;
		}
		if(spriteIndex!=-1)
		{
			switch(powerUpState)
			{
				default: break;
				case 1: if(fireSprites[spriteIndex]!=null)render.sprite = fireSprites[spriteIndex]; break;
				case 2: if(axeSprites[spriteIndex]!=null)render.sprite = axeSprites[spriteIndex]; break;
				case 3: if(capeSprites[spriteIndex]!=null)render.sprite = capeSprites[spriteIndex]; break;
				case 4: if(csgoSprites[spriteIndex]!=null)render.sprite = csgoSprites[spriteIndex]; break;
				case 5: if(lKnifeSprites[spriteIndex]!=null)render.sprite = lKnifeSprites[spriteIndex]; break;
			}
		}

	}
	public void powerUpEvent() //called by animation
	{
		if(powerUpCor!=null)StopCoroutine(powerUpCor);
		powerUpCor = StartCoroutine(powerUpSequence(true));
	}
	public void powerDownEvent() //called by animation
	{
		if(powerUpCor!=null)StopCoroutine(powerUpCor);
		powerUpCor = StartCoroutine(powerUpSequence(false));
	}
	void powerUpChangeStart()
	{
		turnToPlayer();
		changeState = false;
		targetState = Mathf.Clamp(targetState,1,6);
		anim.SetTrigger("item");
		itemRender.sprite = items[targetState-1];
		StartCoroutine(takeDMGWait());
	}
	IEnumerator takeDMGWait()
	{
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_item");
		if(!midIntro)
		data.playUnlistedSound(sounds[12]);
		canTakeDamage = false;
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		canTakeDamage = true;
	}
	IEnumerator powerUpSequence(bool timeStop)
	{
		hp = 6;
		actionShoot = false;
		fireWaitFrames = 0;
		if(Time.timeScale!=0&&timeStop)
		Time.timeScale=0;
		int powerFrames = 45;
		if(midIntro)powerFrames = 30;
		bool lastStateShown = true;
		int lastStateIndex = powerUpState;
		int currentTargetState = 0;
		if(timeStop)
		{
			currentTargetState = targetState;
		}
		else playSound(5,true);
		//print("TargetState: "+currentTargetState);
		if(powerUpState!=currentTargetState)
		{
			if(timeStop&&!midIntro)
			data.playUnlistedSound(sounds[4]);
			while(powerFrames>0)
			{
				if(powerFrames%5==0)
				{
					if(lastStateShown)
						powerUpState = currentTargetState ;
					else powerUpState = lastStateIndex;

					lastStateShown = !lastStateShown;
				}
				powerFrames--;
				yield return 0;
			}
			powerUpState = currentTargetState;
		}
		if(midIntro) //finish intro here
		{
			StartCoroutine(introFinish());
		}
		if(currentTargetState!=0&&!auraEnabled)
		{
			auraEnabled = true;
			StartCoroutine(auraAnim());
		}
		//perform a special attack
		switch(currentTargetState)
		{
			default: break;
			case 2:
			case 3:
			case 5:
			actionSpecial = true;
			break;
		}
		if(timeStop)
		Time.timeScale=1;
		else
		{
			axis.acceptFakeInputs = true;
			anim.SetBool("jumpLock",false);
		}
		AIStart();
		powerUpCor = null;
	}
	IEnumerator introFinish()
	{
		yield return new WaitForSeconds(0.7f);
		pScript.goToCutsceneMode(false);
		midIntro = false;
		//print("Intro over");
	}
	void LateUpdate() //Method is run right before rendering the frame
	{
		/*if(actionShoot&&fireWaitFrames>=0)
		{
			if(powerUpState==4)
			{
				fireLine();
			}
		}*/
		if(powerUpState!=0)
		{
			stateSprite();
		}
	}
	void FixedUpdate()
	{
		if(phase==2&&!pScript.dead)
		{
			cam.transform.position+=Vector3.right*camSpeed*Time.fixedDeltaTime;
		}
	}
	// Update is called once per frame
    void Update()
    {
		//debug
		//findClosestDashPoint();
		if(trackerFollow)
		{
			targetUI.transform.position = player.position+(Vector3.up*0.5f);
		}
        if(Time.timeScale!=0)
		{
			if(phase>=2)
			{
				return;
			}
			if(changeState)
			{
				powerUpChangeStart();
			}
			if(canMove)
			{
				horizontalMovement();
				groundDetect();
			}
			if(playerFollow&&diveCor==null)
			{
				turnToPlayer();
				axis.artificialX = transform.localScale.x;
			}
			if(damageFrames>0)
			{
				damageFrames--;
				if(damageFrames<=0)
				{
					render.material = materials[0]; //todo change into negative sprite if playuh mode
				}
			}
			if(actionShoot&&fireWaitFrames>=0)
			{
				fireWaitFrames--;
				if(fireWaitFrames<=0)
				{
					shootProjEvent();
				}
			}
			if(actionSpecial)
			{
				actionSpecial = false;
				specialAttack();
			}
			if(lookforWall)
			{
				wallScan();
			}
            if(grounded)
            {
                canDive = false;
                if(Mathf.Abs(rb.velocity.x) < 0.1f && maxHeight != 4.0f)
                {
                    jumpForce = jumpHeights.x;
                    maxHeight = 5.0f;
                }
                else if(Mathf.Abs(rb.velocity.x) > 0.1f && Mathf.Abs(rb.velocity.x) < 5.8f && maxHeight != 4.25f)
                {
                    jumpForce = jumpHeights.y;
                    maxHeight = 5.25f;
                }
                else if(Mathf.Abs(rb.velocity.x) > 5.8f && maxHeight != 5.0f)
                {
                    jumpForce = jumpHeights.z;
                    maxHeight = 6.0f;
                }
            }
			else
			{
				if(!canDive && anim.GetBool("dive")==false&& (currentJumpPoint-3)>=startJumpPoint)
				canDive = true;
			}
			if(canDive)
				dive();
            if(!releasedJump&&!grounded&&rb.velocity.y>0)
			{
				currentJumpPoint = transform.position.y;
			}
            if(anim.GetBool("dive")==false||anim.GetBool("dive")==true&&canInterruptDive)
			{
				jump();
			}
            anim.SetFloat("VerSpeed",Mathf.Abs(rb.velocity.y));
        }
    }
	//Special actions
	IEnumerator superShootEvent()
	{
		Fire(3,5,Vector3.zero);
		yield return new WaitForSeconds(0.05f);
		Fire(3,13,Vector3.zero);
		yield return new WaitForSeconds(0.05f);
		Fire(3,15,Vector3.zero);
		yield return new WaitForSeconds(0.05f);
		Fire(3,11,Vector3.zero);
	}
	public void superShoot() //called by animation
	{
		StartCoroutine(superShootEvent());
	}
	void specialAttack()
	{
		switch (powerUpState)
		{
			default: break;
			case 1:
			anim.SetTrigger("superShoot"); //cheese scatter
			break;
			case 2:
			if(specialCor!=null)StopCoroutine(specialCor);
			specialCor = StartCoroutine(axeSequence());
			break;
			case 3:
			if(specialCor!=null)StopCoroutine(specialCor);
			specialCor = StartCoroutine(capeSequence());
			break;
			case 4:
			data.playSound(93,transform.position);
			if(shootCor!=null)
				StopCoroutine(shootCor);
			shootCor = StartCoroutine(shoot()); //super knife shoot
			for(int i = 0;i<knifePoints.Length;i++)
			{
				Fire(4,0,knifePoints[i].position);
			}
			break;
			case 5: //l knife get in position
			findClosestDashPoint();
			break;
		}
	}
	//Cheese AI
	IEnumerator cheeseSequence()
	{
		//shoot cheese at intro
		turnToPlayer();
		yield return new WaitUntil(()=>!midIntro);
		lockChangeDirection = true;
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		actionSpecial = true;
		yield return new WaitForSeconds(0.5f);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		//intro, go to the wall ceda is not facing, i.e. go backwards
		horizontalSpeed = 4;
		axis.artificialX = -transform.localScale.x;
		actionJump = true;
		scanDir = new Vector2(1*-transform.localScale.x,0);
		scanLength = 2.5f;
		lookforWall = true;
		yield return 0;
		//shoot until reaches wall
		yield return new WaitUntil(()=>!lookforWall);
		axis.artificialX = 0;
		actionJump = false;
		yield return new WaitUntil(()=>grounded);
		actionJump = true;
		horizontalSpeed = 2+((float)intensity/5);
		playerFollow = true;
		actionDash = true;
		jumpAmount = 10+(intensity);
		yield return 0;
		yield return new WaitUntil(()=>jumpAmount<=0);
		playerFollow = false;
		axis.artificialX = 0;
		horizontalSpeed = 4;
		yield return new WaitUntil(()=>Mathf.Abs(rb.velocity.x)<=0.1f);
		if(intensity<=1)intensity++;
		patternProgress(0);
	}
	IEnumerator csgoSequence()
	{
		//look at player
		turnToPlayer();
		yield return new WaitUntil(()=>!midIntro);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		actionSpecial = true;
		yield return new WaitForSeconds(0.5f);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		actionDash = false;
		//intro, go to the wall ceda is not facing, i.e. go backwards
		lockChangeDirection = false;
		horizontalSpeed = 4;
		scanLength = 2.5f;
		axis.artificialX = -transform.localScale.x;
		scanDir = new Vector2(-transform.localScale.x,0);
		lookforWall = true;
		yield return 0;
		yield return new WaitUntil(()=>!lookforWall||axis.artificialX==0);
		axis.artificialX = 0;
		lockChangeDirection = true;
		transform.localScale = new Vector3(-transform.localScale.x,1,1);
		yield return new WaitUntil(()=>grounded); //start shooting
		actionJump = true;
		actionShoot = true;
		shootAmount = 11+(intensity);
		csgoFrameWait = 70-(intensity*3);
		yield return 0;
		yield return new WaitUntil(()=>shootAmount<=0);
		lRender.enabled = false;
		actionShoot = false;
		yield return new WaitUntil(()=>checkForActiveProjectiles(csgoKnives));
		actionJump = false;
		lockChangeDirection = false;
		yield return new WaitUntil(()=>grounded);
		yield return new WaitUntil(()=>Mathf.Abs(rb.velocity.x)<=0.1f);
		yield return new WaitForSeconds(0.5f);
		if(intensity<=1)intensity++;
		patternProgress(0);
	}
	//cape attack
	IEnumerator capeSequence()
	{
		int times = 3+Mathf.RoundToInt((float)intensity/1.5f); 
		yield return new WaitUntil(()=>!midIntro);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		bool loop = true;
		while(loop)
		{
			grav.enabled = false;
			canMove = false;
			rb.velocity = Vector2.zero;
			anim.SetBool("capeAttack",true);
			anim.SetBool("jumpLock",true);
			yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_capeFlyUpMain");
			spawnSpikesLaunch(transform.position.x);
			canTakeDamage = false;
			grounded = false;
			anim.SetBool("Grounded",grounded);
			dontUnlockDamagePass = true;
			auraWaitFrames = 1;
			rb.velocity = new Vector2(0,120+(intensity*3));
			cam.easeShake = true;
			cam.shakeCamera(0.4f,1);
			playSound(6,true);
			pCol.enabled = false;
			if(trailCor!=null)StopCoroutine(trailCor);
			trailCor = StartCoroutine(spawnTrails(6,0.03f,materials[0],colors[1]));
			yield return new WaitUntil(()=>transform.position.y>=18);
			playSound(7,true);
			rb.velocity = Vector2.zero;
			transform.position = new Vector3(transform.position.x,18,0);
			trackerFollow = true;
			float speedT = 1+((float)intensity/10f);
			targetUIAnim.speed = speedT;
			warnSoundAsc.pitch = speedT;
			targetUI.transform.position = player.position;
			targetUI.SetActive(true);
			yield return 0;
			yield return new WaitUntil(()=>!targetUI.activeInHierarchy);
			transform.position = new Vector3(targetUI.transform.position.x,transform.position.y,transform.position.z);
			turnToPlayer();
			anim.SetBool("capeAttack",false);
			rb.velocity = new Vector2(0,-60-(intensity*3));
			yield return 0;
			if(trailCor!=null)StopCoroutine(trailCor);
			trailCor = StartCoroutine(spawnTrails(6,0.03f,materials[0],colors[1]));
			yield return new WaitUntil(()=>transform.position.y<=8);
			pCol.enabled = true;
			if(transform.position.y<=0) //fail check if were to fall through floor
			{
				transform.position = new Vector3(transform.position.x,0,0);
				grounded = true;
			}
			canMove = true;
			yield return new WaitUntil(()=>grounded);
			playSound(1,false);
			//hit ground here
			rb.velocity = Vector2.zero;
			grav.enabled = true;
			cam.easeShake = true;
			canTakeDamage = true;
			cam.shakeCameraVertically(1f,0.5f);
			spawnSpikes(transform.position.x);
			yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
			times--;
			if(times>0)
			{
				turnToPlayer();
			}
			else loop = false;
		}
		yield return new WaitForSeconds(0.1f);
		anim.SetBool("jumpLock",false);
		turnToPlayer();
		yield return new WaitUntil(()=>Mathf.Abs(rb.velocity.x)<=0.1f);
		if(intensity<=1)intensity++;
		patternProgress(0);
		specialCor = null;
	}
	public void disableTracking() //called by animation
	{
		trackerFollow = false;
	}
	//axe attack
	IEnumerator axeSequence()
	{
		yield return new WaitUntil(()=>!midIntro);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		swishEffect.setAlpha(0);
		grav.enabled = false;
		canMove = false;
		rb.velocity = Vector2.zero;
		anim.SetBool("axeSpin",true);
		wallsToHit = 10+intensity; //change with the intensity rising
		playSound(9,true);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_spin");
		dontUnlockDamagePass = true;
		auraWaitFrames = 1;
		pCol.sharedMaterial = physicsMaterials[1];
		axeAura.SetActive(true);
		canTakeDamage = false;
		anim.speed = 0;
		int divider = 1;
		float progress = 0;
		if(player.position.x>transform.position.x)divider = -1;
		Vector2 targetVelocity = new Vector2(axeSpinVelocity.x*divider,axeSpinVelocity.y)*(1+((intensity)/10));
		//start spinning here
		asc.clip = sounds[8];
		asc.loop = true;
		asc.Play();
		while(progress<1)
		{
			if(Time.timeScale!=0)
			{
				rb.velocity = new Vector2(targetVelocity.x*progress,targetVelocity.y*progress);
				progress+=5*Time.deltaTime;
				anim.speed = progress;
			}
			yield return 0;
		}
		anim.speed = 1;
		rb.velocity = targetVelocity;
		yield return new WaitUntil(()=> wallsToHit<=0);
		progress = 0;
		targetVelocity = Vector2.zero;
		//stop spinning here
		Vector2 startVelocity = rb.velocity;
		while(progress<1)
		{
			if(Time.timeScale!=0)
			{
				rb.velocity = new Vector2(Mathf.Lerp(startVelocity.x,targetVelocity.x,progress),Mathf.Lerp(startVelocity.y,targetVelocity.y,progress));
				progress+=2*Time.deltaTime;
				anim.speed = progress;
			}
			yield return 0;
		}
		pCol.sharedMaterial = physicsMaterials[0];
		rb.velocity = targetVelocity;
		canMove = true;
		grounded = false;
		grav.enabled = true;
		swishEffect.triggerFade(0);
		yield return new WaitUntil(()=> grounded);
		asc.loop = false;
		if(asc.isPlaying)asc.Stop();
		playSound(10,true);
		anim.SetBool("axeSpin",false);
		axeAura.SetActive(false);
		canTakeDamage = true;
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		turnToPlayer();
		yield return new WaitForSeconds(0.5f);
		yield return new WaitUntil(()=>Mathf.Abs(rb.velocity.x)<=0.1f);
		if(intensity<=1)intensity++;
		patternProgress(0);
		specialCor = null;
	}
	//L knife dash
	void findClosestDashPoint()
	{
		float bPos = transform.position.x-34.5f;
		int targetIndex = Mathf.Clamp(Mathf.CeilToInt(Mathf.Floor(bPos/3.5f)/2),0,3);
		Debug.DrawLine(transform.position,dashPoints[targetIndex],Color.green,2f);
		if(diveCor!=null)StopCoroutine(diveCor);
		diveCor = StartCoroutine(dashToPoint(targetIndex));

	}
	IEnumerator dashToPoint(int currentPoint)
	{
		yield return new WaitUntil(()=>!midIntro);
		float posTarg = dashPoints[currentPoint].x;
		if(posTarg<transform.position.x)
		{
			transform.localScale = new Vector3(-1,1,1);
		}
		else transform.localScale = Vector3.one;
		int repeats = 3;
		while(repeats>0)
		{
		int indexAdder = 1;
		if(transform.localScale.x<0)indexAdder = -1;
		bool active = true;
		float progress = 0;
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		anim.SetBool("groundDash",true);
		yield return 0;
		if(anim.GetBool("warn"))
		{
			yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_LKnifeDash");
			anim.SetBool("warn",false);
		}
		playSound(11,true);
		if(trailCor!=null)StopCoroutine(trailCor);
		trailCor = StartCoroutine(spawnTrails(7,0.02f,materials[1],colors[0]));
		float distance = Mathf.Abs(transform.position.x-dashPoints[currentPoint].x)/7;
		
		while(active)
		{
			if(Time.timeScale!=0)
			{
				transform.position = Vector3.Lerp(transform.position,dashPoints[currentPoint],progress);
				progress+=((groundDashSpeed+((float)intensity/10))*Time.deltaTime)/distance;
			}
			yield return 0;
			if(progress>=1) //check for re dash
			{
				progress = 0;
				currentPoint+=indexAdder;
				if(Mathf.Clamp(currentPoint,0,dashPoints.Length-1)!=currentPoint)//if outside bounds, point is reached, stop
				{
					transform.localScale = new Vector3(-transform.localScale.x,1,1);
					active = false;
				}
				else
				{
					distance = 1;
					anim.SetBool("groundDash",false);
					yield return 0;
					anim.SetBool("groundDash",true);
					playSound(11,true);
					if(trailCor!=null)StopCoroutine(trailCor);
					trailCor = StartCoroutine(spawnTrails(7,0.02f,materials[1],colors[0]));
				}
			}
		}
		anim.SetBool("groundDash",false);
		currentPoint = 2;
		if(transform.position.x<=45)
		{
			currentPoint = 1;
			transform.localScale = Vector3.one;
		}
		else transform.localScale = new Vector3(-1,1,1);
		
		//throw knives here before redashing
		shootAmount=5+Mathf.FloorToInt((intensity/5)*3);
		lKnifeFrameWait = 60-(intensity*4);
		actionShoot = true;
		bool skipped = false;
		while(shootAmount>0)
		{
			//print(Mathf.Abs(player.position.x-transform.position.x));
			if(Mathf.Abs(player.position.x-transform.position.x)<=3) //cancel shoot if player too close
			{

				shootAmount = 0;
				fireWaitFrames = 0;
				actionShoot = false;
				skipped = true;
			}
			else yield return new WaitForSeconds(0.1f);
			yield return 0;
		}
		//yield return new WaitUntil(()=>shootAmount<=0);
		if(!skipped)
		yield return new WaitForSeconds(0.3f);
		//else yield return new WaitForSeconds(0.1f);
		
		anim.SetBool("warn",true); //first redash has warning
		repeats--;
		yield return 0;
		}
		yield return new WaitUntil(()=>Mathf.Abs(rb.velocity.x)<=0.1f);
		if(intensity<=1)intensity++;
		patternProgress(0);
		diveCor = null;
	}
	IEnumerator spawnTrails(int amount,float delay,Material mat,Color col)
	{
		for(int i = 0;i<amount;i++)
		{
			for(int j = 0;j<trailSprites.Length;j++)
			{
				if(!trailSprites[j].gameObject.activeInHierarchy)
				{
					Transform tr = trailSprites[j].transform,renderTr = render.transform;
					tr.position = renderTr.position;
					tr.localScale = transform.localScale;
					tr.eulerAngles = renderTr.localEulerAngles;
					trailSprites[j].material = mat;
					trailSprites[j].color = col;
					trailSprites[j].sprite = render.sprite;
					trailSprites[j].gameObject.SetActive(true);
					break;
				}
			}
			yield return new WaitForSeconds(delay);
		}
		trailCor = null;
	}
	void spawnSpikes(float startPoint)
    {
        StartCoroutine(spikeRaise(4,startPoint,true,false,0));
        StartCoroutine(spikeRaise(4,startPoint,false,false,0));
    }
	void spawnSpikesLaunch(float startPoint)
    {
		StartCoroutine(spikeRaise(1,startPoint,true,true,0)); //middle
        StartCoroutine(spikeRaise(1,startPoint,true,false,0.05f)); //left
        StartCoroutine(spikeRaise(1,startPoint,false,false,0.05f)); //right
    }
	IEnumerator spikeRaise(int amount,float startPoint,bool left,bool startMiddle,float startDelay)
    {
		yield return new WaitForSeconds(startDelay);
		float currentPoint = startPoint;
        for(int i = 0;i<amount;i++)
        {
			if(startMiddle) startMiddle = false;
			else
			{
				if(left) currentPoint-=1;
				else currentPoint+=1;
			}
			if(currentPoint!=Mathf.Clamp(currentPoint,34.5f,55.5f)) yield break;

            for(int x = 0; x<spikes.Length;x++)
            {
                if(!spikes[x].gameObject.activeInHierarchy)
                {
					spikes[x].transform.position = new Vector3(currentPoint,spikes[x].transform.position.y,spikes[x].transform.position.z);
                    spikes[x].gameObject.SetActive(true);
                    data.playUnlistedSoundPoint(sounds[0],spikes[x].transform.position);
                    break;
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
	
	//AI
	void AIStart()
	{
		switch(powerUpState)
		{
			default:
			break;
			case 1:
			StartCoroutine(cheeseSequence());
			break;
			case 4:
			StartCoroutine(csgoSequence());
			break;
		}
	}
	void wallScan()
	{
		RaycastHit2D ray = Physics2D.Raycast(transform.position+Vector3.up,scanDir,scanLength*Mathf.Clamp((rb.velocity.x/7.6f),0.5f,1),whatIsGround);
		if(ray.collider!=null)
			lookforWall = false;
		else
		{
			Vector2 startPoint = (Vector2)transform.position+Vector2.up;
			Debug.DrawLine(startPoint,startPoint+(scanDir*scanLength*Mathf.Clamp((rb.velocity.x/7.6f),0.5f,1)),Color.yellow);
		}
	}

	//Normal actions
	void turnToPlayer()
	{
		if(transform.position.x>player.position.x)
		{
			transform.localScale = new Vector3(-1,1,1);
		}
		else transform.localScale = Vector3.one;
	}
	void turnFromPlayer()
	{
		if(transform.position.x>player.position.x)
		{
			transform.localScale = Vector3.one;
		}
		else transform.localScale = new Vector3(-1,1,1);
	}
	void shootProjEvent()
	{
		switch(powerUpState)
		{
			default: break;
			case 1: //cheese shoot
			Fire(0,0,Vector3.zero);
			if(actionShoot)
				fireWaitFrames = cheeseFrameWait; 
			break;
			case 4: //csgo shoot
			if(shootAmount%4!=0)
			Fire(1,0,Vector3.zero);
			else actionSpecial = true;
			lRender.enabled = false;
			if(actionShoot)
				fireWaitFrames = csgoFrameWait; 
			break;
			case 5:  //lknife shoot
			Fire(2,0,Vector3.zero);
			if(actionShoot)
				fireWaitFrames = lKnifeFrameWait; 
			break;
		}
		if(shootAmount>0)
		{
			shootAmount--;
			if(shootAmount<=0)actionShoot = false;
		}
	}
	bool checkForActiveProjectiles(List <GameObject> type)
	{
		for(int i = 0;i<type.Count;i++)
		{
			if(type[i].gameObject.activeInHierarchy)
			return false;
		}
		return true;
	}
	void killAllActiveProjectiles(List <GameObject> type)
	{
		data.StartCoroutine(killAllActiveProjectilesCor(type));
	}
	IEnumerator killAllActiveProjectilesCor(List <GameObject> type)
	{
		for(int i = 0;i<type.Count;i++)
		{
			if(type[i].gameObject.activeInHierarchy)
			{
				EnemyCorpseSpawner eCS = type[i].GetComponent<EnemyCorpseSpawner>();
				eCS.spawnCorpse();
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
	void Fire(int type,float setVelo,Vector3 targetPoint)
	{
		List<GameObject> projectiles;
		Vector2 addedVelocity = Vector2.zero;
		switch(type)
		{
			default:
			projectiles = balls;
			break; //cheese
			case 1:
			projectiles = csgoKnives;
			break;//csgo
			case 2:
			projectiles = lKnives;
			break;//l knives
			case 4:
			projectiles = homingKnives;
			break;//csgo homing
		}
		for(int i = 0; i< projectiles.Count; i++)
			if(!projectiles[i].activeInHierarchy)
			{
				if(type!=3)
				{
					if(shootCor!=null)
					StopCoroutine(shootCor);
					shootCor = StartCoroutine(shoot());
				}
				switch(type)
				{
					default: //cheese
					data.playSound(7,transform.position);
					if(!grounded)
					addedVelocity = new Vector2(0,10);
					break;
					case 3: //cheese cluster
					data.playSound(7,transform.position);
					addedVelocity = new Vector2(0,setVelo);
					break;
					case 1: //csgo
					data.playSound(93,transform.position);
					break;
					case 2: //l knives
					data.playSound(80,transform.position);
					break;
					case 4: //csgo homing
					projectiles[i].GetComponent<knifeScript>().targetPoint = targetPoint;
					break;
				}
				if(type!=1&&type!=4)
				{
					Gravity grav = projectiles[i].GetComponent<Gravity>();
					grav.pushForces = new Vector2(grav.pushForces.x,-Mathf.Abs(grav.pushForces.y));
				}
				Vector3 pos = transform.position;
				projectiles[i].transform.position = new Vector3(pos.x+(0.5f*transform.localScale.x),pos.y+0.75f,pos.z);

				projectiles[i].transform.parent = null;
				if(type==2)//l knife random speed
				{
					projectiles[i].GetComponent<knifeScript>().speed = Random.Range(3.5f,4.9f)+(float)intensity/4;
				}
				if(type!=1&&type!=2)
				{
					projectiles[i].transform.localScale = transform.localScale;
				}
				else
				{
					projectiles[i].transform.localScale = new Vector3(-transform.localScale.x,1,1);
				}
				projectiles[i].SetActive(true);
				if(addedVelocity!=Vector2.zero)
				{
					projectiles[i].GetComponent<Rigidbody2D>().velocity+=addedVelocity;
				}
				break;
			}
	}
	void horizontalMovement()
	{
		rb.velocity=new Vector2((axis.axisPosX*horizontalSpeed),rb.velocity.y);

		anim.SetFloat("HorSpeed",Mathf.Abs(rb.velocity.x));

		if(!lockChangeDirection)
		{
			if(axis.artificialX!=-1&&rb.velocity.x>0&&transform.localScale.x!=1)
			{
				transform.localScale = new Vector3(1,transform.localScale.y,transform.localScale.z);
			}
			else if(axis.artificialX!=1&&rb.velocity.x<0&&transform.localScale.x!=-1)
			{
				transform.localScale = new Vector3(-1,transform.localScale.y,transform.localScale.z);
			}
		}

		if(axis.acceptFakeInputs && rb.velocity.x*axis.artificialX<0 && anim.GetBool("dive")==false)
		{
			if(grounded)
			{
				axis.axisAdder = 0.06f;
			}
			else
			{
				if(axis.artificialX!=0||axis.artificialX!=0&&axis.acceptFakeInputs)
				{
					//print("switching dir");
					axis.axisAdder = 0.10f;
				}
				else
				{
					//print("stopping in air");
					if(!pressedOppositeDirInAir)
					axis.axisAdder = 0.04f;
					else axis.axisAdder = 0.12f;
				}
			}
		}
		else if(axis.artificialX == 0 && Mathf.Abs(rb.velocity.x)<=0.1f)
		{
			if(grounded)
			{
				jumpForce = jumpHeights.x;
				maxHeight = 5.0f;
			}
			if(anim.GetBool("dive")==false)
			{
				if(grounded)
				{
					if(axis.Run)
					{
						axis.axisAdder = 0.1f;
					}
					else axis.axisAdder = 0.07f;
				}
				else
				{
					if(axis.artificialX!=0||axis.artificialX!=0&&axis.acceptFakeInputs)
					{
						//print("switching dir");
						axis.axisAdder = 0.10f;
					}
					else
					{
						//print("stopping in air");
						if(!pressedOppositeDirInAir)
							axis.axisAdder = 0.04f;
						else axis.axisAdder = 0.12f;
					}
				}
			}
		}
	}
    bool checkRay(RaycastHit2D ray)
	{
		return ray.collider!=null ? true : false;
	}
    IEnumerator stickGround()
	{
		stickingToGround = true;
		yield return new WaitForSeconds(0.08f);
		stickingToGround = false;
	}
    void groundDetect()
	{
		Vector3 StartPoint1 = new Vector3(transform.position.x-0.215f,transform.position.y+(0.12f),transform.position.z);
		Vector3 StartPoint2 = new Vector3(transform.position.x+0.215f,transform.position.y+(0.12f),transform.position.z);
		Vector3 StartPoint3 = Vector3.zero,StartPoint4 = Vector3.zero,StartPoint5=Vector3.zero,StartPoint6=Vector3.zero;

		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint1,-transform.up,0.15f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(StartPoint2,-transform.up,0.15f,whatIsGround);
		RaycastHit2D ray3,ray4;
		bool ray4Found = false;
		if(diveCor==null)
		{
			StartPoint3 = new Vector3(transform.position.x,transform.position.y+(0.20f),transform.position.z);
			StartPoint4 = new Vector3(transform.position.x,transform.position.y+(1.20f),transform.position.z);
		}
		else
		{
			
			StartPoint3 = new Vector3(transform.position.x,transform.position.y+(0.6f),transform.position.z);
			StartPoint4 = new Vector3(transform.position.x,transform.position.y+(0.4f),transform.position.z);
			StartPoint5 = new Vector3(transform.position.x,transform.position.y+(1f),transform.position.z);
			StartPoint6 = new Vector3(transform.position.x,transform.position.y,transform.position.z);//backup check
		}
		if(transform.localScale.x == 1)
		{
			if(!midDive) ray3 = Physics2D.Raycast(StartPoint3,transform.right,0.25f,whatIsGround);
			else ray3 = Physics2D.Raycast(StartPoint3,transform.right,0.3f,whatIsGround);
			if(StartPoint4!=Vector3.zero)
			{
				if(!midDive) ray4 = Physics2D.Raycast(StartPoint4,transform.right,0.25f,whatIsGround);
				else ray4 = Physics2D.Raycast(StartPoint4,transform.right,0.3f,whatIsGround);
				ray4Found = checkRay(ray4);
				}
			}
			else
			{
				if(!midDive)
				ray3 = Physics2D.Raycast(StartPoint3,-transform.right,0.25f,whatIsGround);
				else ray3 = Physics2D.Raycast(StartPoint3,-transform.right,0.3f,whatIsGround);
				if(StartPoint4!=Vector3.zero)
				{
					if(!midDive)
					ray4 = Physics2D.Raycast(StartPoint4,-transform.right,0.25f,whatIsGround);
					else ray4 = Physics2D.Raycast(StartPoint4,-transform.right,0.3f,whatIsGround);
					ray4Found = checkRay(ray4);
				}
			}
		Debug.DrawRay(StartPoint1, -transform.up,Color.blue);
		Debug.DrawRay(StartPoint2, -transform.up,Color.red);
		Debug.DrawRay(StartPoint3, transform.right*transform.localScale.x,Color.green);
		Debug.DrawRay(StartPoint4, transform.right*transform.localScale.x,Color.magenta);
		if(midDive&&!diveNudge&&!anim.GetBool("gravity"))
		{
			//ray 1 = top
			//ray 3 = up
			//ray 4 = down
			//ray 2 = bottom
			ray1 = Physics2D.Raycast(StartPoint5,transform.right*transform.localScale.x,0.3f,whatIsGround);
			ray2 = Physics2D.Raycast(StartPoint6,transform.right*transform.localScale.x,0.3f,whatIsGround); //backup check
			Vector3 pos = transform.position;
			int nudgeVal = 0; //0 = None, 1 = Up, 2 = Down, 3 = Stop
			if(ray3.collider==null&&(ray4Found||ray2.collider!=null))
			{
				//print("Can nudge up");
				Debug.DrawRay(StartPoint5, transform.right*transform.localScale.x,Color.red,5f);
				nudgeVal = 1;
			}
			else if(!ray4Found&&(ray3.collider!=null||ray1.collider!=null))
			{
				//print("Can nudge down");
				Debug.DrawRay(StartPoint6, transform.right*transform.localScale.x,Color.blue,5f);
				nudgeVal = 2;
			}
			else if(ray4Found&&(ray3.collider!=null))
			{
				//print("Wall hit");
				nudgeVal = 3;
			}
			if(nudgeVal==1)//nudge up
			{
				//print("Nudge up");
				transform.position = new Vector3(pos.x,Mathf.Ceil(pos.y)+0.02f,pos.z);
				diveNudge = true;
				return;
			}
			else if(nudgeVal==2)//nudge down
			{
				//print("Nudge down");
				transform.position = new Vector3(pos.x,Mathf.Floor(pos.y)+0.3f,pos.z);
				diveNudge = true;
				return;
			}
			else if(nudgeVal==3)
			{
				//print("Dive stop");
				midDive = false;
				if(rb.bodyType!=RigidbodyType2D.Static)
				{
					rb.velocity = new Vector2(0,rb.velocity.y);
					transform.GetChild(0).localEulerAngles = Vector3.zero;
				}
				return;
			}
		}
		else if(midDive&&anim.GetBool("gravity"))
		{
			//print("Dive stop because gravity");
			midDive = false;
			if(rb.bodyType!=RigidbodyType2D.Static)
			{
				rb.velocity = new Vector2(0,rb.velocity.y);
				transform.GetChild(0).localEulerAngles = Vector3.zero;
			}
		}
		if(!anim.GetBool("final")&&Mathf.Abs(axis.artificialX)>0&&ray3.collider != null && !frameCollision)
		{
			frameCollision = true;
			axis.axisPosX = 0;
			axis.artificialX = 0;
			if(grounded)
			axis.axisAdder = 0.07f;
			else axis.axisAdder = 0.10f;
			if(!finalJump)
			rb.velocity = new Vector2(0,rb.velocity.y);
		}
		else if(ray3.collider == null)
		{
			if(frameCollision)
			frameCollision = false;
		}
		if(ray1.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0 && !midJump&&pCol.enabled
		|| ray2.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0 && !midJump&&pCol.enabled)
		{
			if(stickToGround!=null)
			StopCoroutine(stickToGround);
			stickToGround = StartCoroutine(stickGround());
			rb.velocity = new Vector2(rb.velocity.x,0);
			grounded = true;
			if(jumpAmount>0)
			{
				jumpAmount--;
				if(jumpAmount<=0)actionJump = false;
			}
			if(axis.axisAdder!=0.07f)
			{
				axis.axisAdder = 0.07f;
			}
			currentJumpPoint = 0;
		}
		else if(ray1.collider == null && grounded && !touchingGround &&!stickingToGround
		     || ray1.collider == null && rb.velocity.y < -3.3f && grounded &&!stickingToGround
		     || ray2.collider == null && grounded && !touchingGround &&!stickingToGround
		     || ray2.collider == null && rb.velocity.y < -3.3f && grounded &&!stickingToGround)
			{
				 if(rb.velocity.y<0)
				 rb.velocity = new Vector2(rb.velocity.x,0);
				 axis.currentDivider = axis.savedNormalDivider;
				 grounded = false;
			}
		anim.SetBool("Grounded",grounded);
	}
    IEnumerator jumpDur()
	{
		yield return new WaitForSeconds(0.05f);
		midJump = false;
	}
    void jump()
	{
		if(actionJump && !midJump && grounded && !midDive)
		{
			axis.currentDivider = axis.savedNormalDivider;
			if(axis.artificialX!=0&&!lockChangeDirection)
			transform.localScale = new Vector3(axis.artificialX,transform.localScale.y,transform.localScale.z);
			if(canInterruptDive)
				interruptDive = true;
			midJump = true;
			//Count travel distance
			startJumpPoint = transform.position.y;
			rb.velocity = new Vector2(rb.velocity.x,0);
			grounded = false;
			releasedJump = false;
			Vector2 dir = transform.up * jumpForce;
			playSound(2,true);
			rb.AddForce(new Vector2(0,dir.y),ForceMode2D.Force);
			StartCoroutine(jumpDur());
			//jump shoot attack
			if(powerUpState==1&&fireWaitFrames<=0)
			{
				shootProjEvent();
			}
		}
		if(!actionJump && rb.velocity.y > releaseJumpVelocity && !releasedJump&&!midDive
		||((currentJumpPoint)-(startJumpPoint))>maxHeight && !releasedJump && rb.velocity.y > releaseJumpVelocity&&!midDive)
		{
			releasedJump = true;
			rb.velocity = new Vector2(rb.velocity.x,rb.velocity.y/2.25f);
		}
	}
    void dive()
	{
		//print("Pressing dash: "+pressedDash);
		if(actionDash && !grounded && !anim.GetBool("dive") && player.localScale.x == -transform.localScale.x && player.position.y-1>=transform.position.y && Mathf.Abs(transform.position.x-player.position.x)<=4)
		{
			turnFromPlayer();
			axis.artificialX = transform.localScale.x;
			if(axis.artificialX!=0)
				transform.localScale = new Vector3(axis.artificialX,transform.localScale.y,transform.localScale.z);
			pathPoints[1] = new Vector2(0,colliderHeight.x);
			pCol.SetPath(0,pathPoints);
			canDive = false;
			anim.SetBool("dive",true);

			playSound(3,true);

			anim.speed = 1;
			axis.axisPosX = 3f*transform.localScale.x;
			axis.axisAdder = 0;
			horizontalSpeed = 4;
			float savedVelo;
			if(axis.artificialX==0)
			savedVelo = 0;
			else savedVelo = Mathf.Abs(rb.velocity.x/30);
			rb.velocity = new Vector2(axis.axisPosX*horizontalSpeed*transform.localScale.x,0);
			diveCor = StartCoroutine(diveTime(0.05f+savedVelo,false));
		}
	}
	void dmgFlash(int hpLoss)
	{
		render.material = materials[1];
		damageFrames = 5;
		hp-=hpLoss;
		playSound(14,true);
		if(hp<=0) //take damage
		{
			damageEvent(true);
		}
	}
	Vector3 offset = new Vector3(0,0.725f,0);
	void fireLine()
	{
		RaycastHit2D ray = Physics2D.Raycast(transform.position+offset,transform.right*transform.localScale.x,50f,whatIsGround);
        //draw line
        if(ray.collider!=null)
        {
            if(!lRender.enabled)
            {
                lRender.enabled = true;
            }
            Vector3[] linePositions = new Vector3[lRender.positionCount];
            linePositions[0] = transform.position+offset;
            linePositions[1] = new Vector3(ray.point.x,ray.point.y,transform.position.z); 
            lRender.SetPositions(linePositions);
        }
        else
        {
            if(!lRender.enabled)
            {
                lRender.enabled = true;
            }
            Vector3[] linePositions = new Vector3[lRender.positionCount];
            linePositions[0] = transform.position+offset;
            linePositions[1] = transform.position+(transform.right*transform.localScale.x*50);
            lRender.SetPositions(linePositions);
        }
	}
	IEnumerator dmgNewPowerUp()
	{
		yield return 0;
		killAllActiveProjectiles(csgoKnives);
		killAllActiveProjectiles(homingKnives);
		killAllActiveProjectiles(lKnives);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_idle");
		yield return new WaitUntil(()=>Mathf.Abs(rb.velocity.x)<=0.1f);
		intensity= Mathf.Clamp(intensity+=1,0,5);
		patternProgress(0);
	}
	IEnumerator deathCor()
    {
		data.stopAllMusic();
		pBounce.colorPulse = null;
		anim.SetBool("dead",true);
		StartCoroutine(deathSlide());
		if(transform.position.x<=45)
		transform.localScale = new Vector3(-1,1,1);
		else transform.localScale = Vector3.one;
        pScript.goToCutsceneMode(true);
        pScript.knockedBack = true;
        pScript.rb.velocity = new Vector2(-4,0.1f);
		if(player.position.x<=45)
			player.localScale = Vector3.one;
		else player.localScale = new Vector3(-1,1,1);
		yield return 0;
		killAllActiveProjectiles(csgoKnives);
		killAllActiveProjectiles(homingKnives);
		killAllActiveProjectiles(lKnives);
        yield return new WaitUntil(()=>pScript.grounded);
		yield return 0;
		//print("Player X: "+player.position.x);
		if(player.position.x<=45)
		{
			//print("Player right");
			player.localScale = Vector3.one;
		}
		else
		{
			//print("Player left");
			player.localScale = new Vector3(-1,1,1);
		}
		if(transform.position.x<=45)
		transform.localScale = new Vector3(-1,1,1);
		else transform.localScale = Vector3.one;
        pScript.knockedBack = false;
    }
	IEnumerator deathSlide()
	{
		yield return new WaitUntil(()=>grounded);
		float progress = 0;
		while(progress<1)
		{
			if(Time.deltaTime!=0)
			{
				transform.position = new Vector3(Mathf.Lerp(transform.position.x,45,progress),5,0);
				progress+=0.1f*Time.deltaTime;
			}
			yield return 0;
		}
		//print("Slide end");
	}
	public IEnumerator fakeFinalAttack()
	{
		anim.speed = 1;
		Vector3 camPos = cam.transform.position;
		//shakecam intensely
		playSound(20,true);
		playSound(21,true);
		playSound(25,true);
		cam.shakeCamera(0.1f,0.5f);
		yield return new WaitForSeconds(0.5f);
		cam.shakeCamera(0.15f,0.25f);
		yield return new WaitForSeconds(0.25f);
		cam.shakeCamera(0.2f,0.25f);
		yield return new WaitForSeconds(0.25f);
		cam.shakeCamera(0.25f,0.65f);
		yield return new WaitForSeconds(0.25f);
		cam.shakeCamera(0.3f,2f);
		yield return new WaitForSeconds(1.5f);
		//stop allsound
		stopAllSound();
		//fuuny run away here
		anim.SetBool("final",true);
		transform.localScale = Vector3.one;
		cam.easeShake = true;
		yield return new WaitForSeconds(0.5f);
		cam.transform.position = camPos;
		yield return new WaitForSeconds(0.5f);
		pScript.goToCutsceneMode(false);
		axeBurekBlock.gameObject.SetActive(true);
	}
	public void phase2Start()
	{
		data.changeMusicWithIntro(null,music[2],tempos[1]);
		if(pBounce.bouncing!=null)
		{
			pBounce.StopCoroutine(pBounce.bouncing);
			pBounce.BPM = tempos[1];
			pBounce.bouncing = pBounce.StartCoroutine(pBounce.bounce());
		}
		else
		{
			pBounce.BPM = tempos[1];
		}
		phase = 2;
		anim.SetBool("final2",true);
		anim.SetBool("dead",false);
		anim.SetBool("jumpLock",false);
		anim.SetBool("Grounded",false);
		transform.SetParent(cam.transform);
		transform.localPosition = new Vector3(11.5f,4.25f,10);
		cam.lockCamera = true;
		if(specialCor!=null)StopCoroutine(specialCor);
		specialCor = StartCoroutine(obamaPos());
		StartCoroutine(playerDeadSequence());
	}
	IEnumerator obamaPos()
	{
		float minX = 85,maxX = 285,obMax = 11.5f,obMin = 5.1f;
		float progress = 0;
		hp = 1;
		while(transform.localPosition.x>obMin)
		{
			//progress = Mathf.Clamp(maxX-(transform.localPosition.x-minX)/maxX,0,1);
			progress = ((cam.transform.position.x-minX)*100/(maxX-minX)/100);
			//print(progress);
			transform.localPosition = new Vector3(Mathf.Lerp(obMax,obMin,progress),transform.localPosition.y,transform.localPosition.z);
			yield return 0;
		}
		//wait for the end jump here
		//while(transform.position.x<=330.5f)
		grounded = true;
		midDive = false;
		midJump = false;
		yield return new WaitUntil(()=>transform.position.x>=304f); //start scanning for block hits
		data.scanForGoalEvent = true;
		//print("Scanning now");
		StartCoroutine(finalKill()); //prepare for winning
		print("Check for end");
		yield return new WaitUntil(()=>transform.position.x>=330f); //end position
		data.scanForGoalEvent = false;
		//print("jump here");
		finalJump = true;
		anim.SetBool("final2",false);
		anim.SetBool("final",true);
		anim.SetBool("Grounded",false);
		anim.SetBool("dive",false);
		anim.SetBool("axeSpin",false);
		anim.SetBool("shoot",false);
		anim.SetFloat("VerSpeed",5);

		playSound(2,true);
		rb.velocity = new Vector2(rb.velocity.x,15);
		//go offscreen
		float target = 15,start = transform.localPosition.x;
		progress = 0;
		while(transform.localPosition.x<target)
		{
			//progress = Mathf.Clamp(maxX-(transform.localPosition.x-minX)/maxX,0,1);
			progress += 1*Time.deltaTime;
			//print(progress);
			transform.localPosition = new Vector3(Mathf.Lerp(start,target,progress),transform.localPosition.y,transform.localPosition.z);
			yield return 0;
		}
	}
	IEnumerator finalKill()
	{
		yield return 0;
		yield return 0;
		print("Scanning now");
		//reached goal will turn true after hitting the chain block
		yield return new WaitUntil(()=>data.reachedGoal&&transform.position.x<=329.7f&&(hp!=9||transform.position.y<=13.8f));
		GameObject.Find("HUD_Canvas").SetActive(false);
		print("Win here");
		pBounce.StopCoroutine(pBounce.bouncing);
		data.stopAllMusic();
		//grav.enabled = false;
		if(specialCor!=null)StopCoroutine(specialCor);
		hp = 1;
		phase = 3;
		GameObject.Find("PlayerBlocker").SetActive(false);
		pScript.goToCutsceneMode(true);
		Time.timeScale = 0.3f;
		transform.SetParent(null);
		cam.AssignTarget(transform);
		cam.overWriteLockScroll = true;
		cam.lockCamera = false; // slow down time, move cam towards playuh
		cam.lockLeft = false;
		cam.lockRight = false;
		cam.lockscroll = false;
		while(((hp>=1&&hp!=9)&&transform.position.y>13.8f)&&transform.position.x<330f)
		{
			if(transform.position.x<330f)
			transform.position+=new Vector3(Time.deltaTime*2,0,0);
			else break;
			yield return 0;
		}
		grav.enabled = false;
		if(transform.position.y<13.9f)
		{
			hp = 0;
		}
		transform.position = new Vector3(transform.position.x,14,transform.position.z);
		yield return 0; //wait for a frame to resolve conflict between getting hit or hitting the switch.
		//fail
		if(hp>=1)
		{
			print("failed");
			Time.timeScale = 1f;
			cam.lockCamera = true; // slow down time, move cam towards playuh
			cam.lockRight = true;
			cam.lockscroll = true;
			data.scanForGoalEvent = false;
			//print("jump here");
			finalJump = true;
			anim.SetBool("final2",false);
			anim.SetBool("final",true);
			anim.SetBool("Grounded",false);
			anim.SetBool("dive",false);
			anim.SetBool("axeSpin",false);
			anim.SetBool("shoot",false);
			anim.SetFloat("VerSpeed",5);

			playSound(2,true);
			grav.enabled = true;
			rb.velocity = new Vector2(rb.velocity.x,15);
			//go offscreen
			float target = 343.5f,start = transform.position.x;
			float progress2 = 0;
			while(transform.position.x<target)
			{
				progress2 += 1*Time.deltaTime;
				//print(progress);
				transform.position = new Vector3(Mathf.Lerp(start,target,progress2),transform.position.y,transform.position.z);
				yield return 0;
			}
			yield break;
		}
		cam.workInStoppedTime = true;//pause for 60 frames
		Time.timeScale = 0;
		grounded = false;
		anim.SetBool("Grounded",grounded);
		UnityEngine.Rendering.SortingGroup s = GetComponent<UnityEngine.Rendering.SortingGroup>();
		anim.SetBool("dead",true);
		anim.updateMode = AnimatorUpdateMode.UnscaledTime;
		//int timeFrames = 60;
		transform.localScale = new Vector3(-1,1,1);
		grav.enabled = true;
		data.playUnlistedSound(sounds[26]);
		yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="BPlayuh_death_part2");
		/*while(timeFrames>0)
		{
			timeFrames--;
			yield return 0;
		}*/
		Time.timeScale = 1;
		anim.updateMode = AnimatorUpdateMode.Normal;
		cam.workInStoppedTime = false;
		playSound(27,true);
		//print("launch fucker here");
		anim.SetBool("final2",false);
		s.sortingOrder = -1;
		s.sortingLayerName = "Default";
		rb.velocity = new Vector2(rb.velocity.x,25);
		grounded = false;
		float progress = 0;
		//float startPos = transform.position.x;
		while(progress<0.25f)
		{
			Vector3 pos = transform.position;
			progress+=0.05f*Time.deltaTime;
			transform.position = new Vector3(Mathf.Lerp(pos.x,348.5f,progress),pos.y,pos.z);
			yield return 0;
		}
		//anim.speed = 1;
		yield return new WaitUntil(()=>grounded);
		cam.fadeScreen(true);
		yield return 0;
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
			DataS.clearedUnbeatenLevel = newClear;
			yield return new WaitUntil(()=>cam.fadeAnim>=1);
			if(newClear)
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene (53);
			}
			else DataS.loadWorldWithLoadScreen(DataS.currentWorld);

	}
	IEnumerator playerDeadSequence()
	{
		yield return 0;
		yield return 0;
		yield return new WaitUntil(()=>pScript.dead);
		//print("Player died");
		if(specialCor!=null&&!finalJump)StopCoroutine(specialCor);
		transform.SetParent(null);
		while(transform.position.x<330f)
		{
			transform.position+=new Vector3(Time.deltaTime*10*Time.timeScale,0,0);
			yield return 0;
		}
		//print("Dead sequence end");
		if(finalJump)yield break;
			cam.lockCamera = true; // slow down time, move cam towards playuh
			cam.lockRight = true;
			cam.lockscroll = true;
			data.scanForGoalEvent = false;
			//print("jump here");
			finalJump = true;
			anim.SetBool("final2",false);
			anim.SetBool("final",true);
			anim.SetBool("Grounded",false);
			anim.SetBool("dive",false);
			anim.SetBool("axeSpin",false);
			anim.SetBool("shoot",false);
			anim.SetFloat("VerSpeed",5);

			playSound(2,true);
			grav.enabled = true;
			rb.velocity = new Vector2(rb.velocity.x,15);
			//go offscreen
			float target = 343.5f,start = transform.position.x;
			float progress2 = 0;
			while(transform.position.x<target)
			{
				progress2 += 1*Time.deltaTime;
				//print(progress);
				transform.position = new Vector3(Mathf.Lerp(start,target,progress2),transform.position.y,transform.position.z);
				yield return 0;
			}
			yield break;
	}
	public void breakTiles(int ID)
	{
		cam.shakeCamera(0.2f,0.3f);
		Vector3Int pos = new Vector3Int(56,7,0);
		if(ID==0) //side wall
		{
			spawnSlashPart(pos);
			map.SetTile(pos,null);
			pos = new Vector3Int(56,8,0);
			spawnSlashPart(pos);
			map.SetTile(pos,null);
		}
		else //top
		{
			pos = new Vector3Int(50,15,0);
			spawnSlashPart(pos);
			map.SetTile(pos,null);
			pos = new Vector3Int(51,15,0);
			spawnSlashPart(pos);
			map.SetTile(pos,null);
		}
	}
	void spawnSlashPart(Vector3 pos)
	{
		pos+=new Vector3(0.5f,0.5f,0);
		for(int i = 0;i<particles.Length;i++)
		{
			if(!particles[i].activeInHierarchy)
			{
				particles[i].transform.position = pos;
				particles[i].SetActive(true);
				break;
			}
		}
	}
	void damageEvent(bool skipStomp)
	{
		hits++;
		if(!forbiddenPowerUps.Contains(powerUpState))
		addToForbidden(powerUpState);
		playerFollow = false;
		fireWaitFrames = 0;
		actionJump = false;
		actionShoot = false;
		actionDash = false;
		axis.artificialX = 0;
		axis.acceptFakeInputs = false;
		lockChangeDirection = false;
		canTakeDamage = false;
		canMove = true;
		midJump = false;
		midDive = false;
		asc.loop = false;
		jumpAmount = 0;
		StopAllCoroutines();
		anim.speed = 1;
		anim.SetBool("groundDash",false);
		anim.SetBool("capeAttack",false);
		anim.SetBool("axeSpin",false);
		anim.SetBool("shoot",false);
		anim.SetBool("warn",false);
		lRender.enabled = false;
		stickToGround=null;
		diveCor=null;
		powerUpCor=null;
		trailCor=null;
		specialCor=null;
		horizontalSpeed = 4;
		axis.axisAdder = 0.10f;
		midDive = false;
		interruptDive = false;
		canInterruptDive = false;
		grav.enabled = true;
		grav.maxVelocities = new Vector2(savedMax,grav.maxVelocities.y);
		anim.SetBool("dive",false);
		axis.setRange(1.9f);
		colorPulse.startTransparency+=0.01f;
		if(!grounded)
		{
			anim.SetBool("jumpLock",true);
			rb.velocity = new Vector2(rb.velocity.x/4,-5);
		}
		else
		{
			rb.velocity = new Vector2(rb.velocity.x/6,0);
		}
		if(!skipStomp||!grounded)
		anim.SetTrigger("stomped");
		else
		anim.SetBool("hurt",true);
		//print("Hits taken: "+hits);
		if(hits<=4) //regular hit
		{
			playSound(13,false);
			StartCoroutine(dmgNewPowerUp());
		}
		else //final hit
		{
			playSound(15,true);
			StartCoroutine(deathCor());
		}
	}
	void OnCollisionEnter2D(Collision2D other)
	{
		if(other.gameObject.tag=="Ground")
		{
			if(phase<3)
			{
				if(wallsToHit>0)
				{
					wallsToHit--;
					//increase bounce velocity here
					rb.velocity*=(1.025f);
				}
			}
			else
			{
				grounded = true;
				anim.SetBool("Grounded",grounded);
			}
		}
	}
	void OnCollisionStay2D(Collision2D other)
	{
		string TTag = other.gameObject.tag;
		if(TTag == "Ground"
		||TTag == "semiSolid")
		{
			touchingGround = true;
		}
    }
    void OnCollisionExit2D(Collision2D other)
	{
		string TTag = other.gameObject.tag;
		if(TTag == "Ground"
		||TTag == "semiSolid")
		{
			touchingGround = false;
		}
    }
	void OnTriggerEnter2D(Collider2D other)
	{
		if(phase<2)
		{
			if(other.name=="PlayerCollider")
			{
				if(!auraEnabled&&canTakeDamage&&pScript.rb.velocity.y<-0.1f)
				{
					damageEvent(false);
					pScript.stompBoss(gameObject,true);
				}
				else //deal contact damage to player
				{
					if(!pScript.midSpin)
					pScript.Damage(false,false);
				}
			}
			else
			{
				if(other.transform.parent!=null&&!auraEnabled&&canTakeDamage&&damageFrames<=0)
				switch(other.transform.parent.name)
				{
					default:
					break;
					case "cheese_Ball(Clone)":
					damageFrames = 5;
					dmgFlash(1);
					return;
					case "knife_projectile(Clone)":
					damageFrames = 5;
					dmgFlash(1);
					return;
					case "lknife_projectile(Clone)":
					damageFrames = 5;
					dmgFlash(6);
					return;
				}
				if(other.name=="axeAura")
				{
					if(!auraEnabled&&canTakeDamage&&damageFrames<=0)
					{
						damageFrames = 5;
						dmgFlash(2);
					}
					if(!pScript.knockedBack)
					{
						if(pScript.knockbackCor!=null)
						{
							pScript.StopCoroutine(pScript.knockbackCor);
							pScript.knockbackCor = null;
						}
						if(!pScript.midDive)data.playSoundStatic(25);
						if(!pScript.grounded)
						{
							pScript.knockbackCor = pScript.StartCoroutine(pScript.knockBack(-0.07f,1,0.5f,false));
						}
						else
						{
							pScript.knockbackCor = pScript.StartCoroutine(pScript.knockBack(-0.1f,1,0.5f,false));
						}
					}
				}
			}
		}
		else
		{
			if(other.name.Contains("BlockParent")&&hp>=1&&data.reachedGoal)
			{
				hp=0;
			}
			if(other.name.Contains("Switch")&&phase>=2)
			{
				//print("pressed switch, cant win");
				if(hp!=0&&hp!=9)
				{
					hp = 9;
					pScript.goToCutsceneMode(false);
					StartCoroutine(switchSequence(other));
				}
			}
			//else print(other.name);
		}
	}
	IEnumerator switchSequence(Collider2D other)
	{
		yield return 0;
		//if(!pScript.dead)
		//print("Switch activated");
		other.transform.parent.GetComponent<switchScript>().Activate(false);
	}
}
