using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class BarbarianScript : MonoBehaviour {
	PlayerScript pScript;
	GameObject player;
	public int eventInt = 0;
	public bool newEvent = false;
	public float speed = 0.05f;
	Animator anim;
	bool moving = false;
	MGCameraController cam;
	public float camCenterOffset = -10;
	GameObject autoScrollObj;
	Transform camBounds;
	int timeStopFrames = 0;
	public Sprite[] layer1Backgrounds,layer2Backgrounds,layer3Backgrounds = new Sprite[3];
	GameObject mainParallax;
	Coroutine attacking;
	[Space]
	[Header("Sword spawns")]
	public GameObject projectile,projectile2;
	public AudioClip shootSound;
	Transform spawner,spawnerSub;
	float spawnDelay = 0.1f;
	float phaseOffset = 0;
	float swordOffset = 2;
	int attackInt = 2;
	bool midAttack = false;
	Coroutine winWait;
	GameObject text;
	GameData data;
	dataShare dataS;
	bool stopped = false;
	public Sprite[] texts = new Sprite[4];
	public Transform cutscene;
	Transform goblin,head;
	public AudioClip[] sounds;
	AudioSource aSource;
	GameObject blocker;
	Transform slashPoint;
	void spawnText(int ID)
	{
		text.transform.SetParent(cam.transform);
		text.transform.position = cam.transform.position+Vector3.forward;
		text.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = texts[Mathf.Clamp(ID,0,4)];
		text.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = texts[Mathf.Clamp(ID,0,4)];
		text.SetActive(true);
		if(ID==0) text.GetComponent<Animator>().SetBool("lose",true);
	}
	IEnumerator win()
	{
		yield return 0;
		yield return new WaitUntil(()=> Time.timeScale!=0);
		spawnText(dataS.checkpointValue+1);
		yield return new WaitUntil(()=> pScript.dead);
		yield return new WaitUntil(()=> !midAttack);
		spawnText(0);
		moving = false;
		if(attacking!=null)
		StopCoroutine(attacking);
		anim.SetBool("Moving",moving);
		anim.SetBool("Win",true);
	}
	IEnumerator swordLoop(int ID)
	{
		float divider = -1;
		if(ID==1) divider = 1;

		for(int i = 0; i<3;i++)
		{
			//Debug.Log(i+": "+(swordOffset*(i)*divider));
			if(i==0)SpawnProj(0);
			else SpawnProj(swordOffset*(i)*divider);
			yield return new WaitForSeconds(spawnDelay);
		}
	}
	void SpawnProj(float Yoffset)
	{
		aSource.PlayOneShot(shootSound,1f);
		Vector3 spawnerLocal = new Vector3(0,Yoffset+0.05f,0);
		spawnerSub.localPosition = spawnerLocal;
		GameObject obj;
		if(eventInt!=2)
		obj = Instantiate(projectile,new Vector3(spawnerSub.position.x,spawnerSub.position.y,0),Quaternion.identity);
		else obj = Instantiate(projectile2,new Vector3(spawnerSub.position.x,spawnerSub.position.y,0),Quaternion.identity);
		if(transform.localScale.x==-1)
		{
			obj.transform.eulerAngles = new Vector3(spawner.eulerAngles.x,spawner.eulerAngles.y,spawner.eulerAngles.z+180f);
		}
		obj.transform.SetParent(transform.parent);
		obj.gameObject.SetActive(true);
	}
	public void Fire()
	{
		if(attackInt==2)
		StartCoroutine(swordLoop(1));
		else StartCoroutine(swordLoop(0));
	}
	IEnumerator attackLoop(int timeDelay)
	{
		while(!newEvent && Application.isPlaying)
		{
			yield return new WaitForSeconds(timeDelay);
			//high attack
			if(player.transform.position.y>slashPoint.position.y-2)
			{
				attackInt = 0;
			}
			//low attack
			else if(player.transform.position.y+phaseOffset<7)
			{
				attackInt = 1;
			}
			//normal attack
			else attackInt = 2;
			//Debug.Log("Attack: "+attackInt);
			midAttack = true;
			cam.lockCamera = true;
			moving = false;
			anim.SetBool("Moving",moving);
			anim.SetInteger("AttackInt",attackInt);
			anim.SetTrigger("Attack");
			yield return new WaitForSeconds(1f);
			midAttack = false;
			cam.lockCamera = false;
			if(!stopped)
			moving = true;
			anim.SetBool("Moving",moving);
		}	
	}
	// Use this for initialization
	void Start ()
	{
		if(Application.isPlaying)
		{
		player = GameObject.Find("Player_main");
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		pScript = player.GetComponent<PlayerScript>();
		spawner = transform.GetChild(0).GetChild(1).GetChild(0);
		spawnerSub = spawner.GetChild(0);
		anim = transform.GetChild(0).GetComponent<Animator>();
		aSource = GetComponent<AudioSource>();
		projectile2 = transform.GetChild(1).gameObject;
		autoScrollObj = GameObject.Find("AutoScroll");
		moving = true;
		mainParallax = GameObject.Find("MainAreaParallax");
		anim.SetBool("Moving",moving);
		camBounds = GameObject.Find("MainBounds").transform;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		text = transform.GetChild(2).gameObject;
		cutscene.gameObject.SetActive(true);
		goblin = transform.GetChild(0).GetChild(3);
		blocker = cam.transform.GetChild(0).GetChild(0).gameObject;
		slashPoint = transform.GetChild(0).GetChild(4);
		if(dataS.checkpointValue==0)
		{
			updateBackgrounds(1);
			attacking = StartCoroutine(attackLoop(5));
		}
		else if(dataS.checkpointValue!=0)
		{
			cam.startWithFade = true;
			eventInt = dataS.checkpointValue;
			if(eventInt==1)
			phaseOffset = 21;
			else phaseOffset = 42;
			cam.transform.position = new Vector3(65,11-21*eventInt,-10);
			mainParallax.transform.position = new Vector3(mainParallax.transform.position.x,mainParallax.transform.position.y-21*eventInt,mainParallax.transform.position.z);
			updateBackgrounds(eventInt+1);
			transform.position = new Vector3(72,transform.position.y-21*eventInt,transform.position.z);
			camBounds.transform.position -= new Vector3(0,22*(eventInt),0);
			switch(eventInt)
			{
				default: attacking = StartCoroutine(attackLoop(4)); break;
				case 2: attacking = StartCoroutine(attackLoop(3)); break;
			}
		}
		winWait = StartCoroutine(win());
		}
	}
	public void spawnHead()
	{
		head.GetComponent<barBarHead>().sc = this;
		head.parent = null;
		Rigidbody2D rb2 = head.GetComponent<Rigidbody2D>();
		cam.AssignTarget(head);
		head.gameObject.SetActive(true);
		rb2.velocity = new Vector2(100,20);
		rb2.angularVelocity = -1000;
		Time.timeScale = 1;
		StartCoroutine(spawnGoblin());
	}
	IEnumerator spawnGoblin()
	{
		yield return new WaitForSeconds(4f);
		cam.lockCamera = true;
		goblin.SetParent(null);
		goblin.position = new Vector3(head.position.x+20,-36,0);
		goblin.gameObject.SetActive(true);
		yield return new WaitForSeconds(4f);
		cam.fadeScreen(true);
	}
	void updateBackgrounds(int phase)
	{
		int id = Mathf.Clamp(phase-1,0,layer1Backgrounds.Length-1);
		mainParallax.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = layer1Backgrounds[id];
		mainParallax.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = layer2Backgrounds[id];
		mainParallax.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = layer3Backgrounds[id];
		for(int i = 0; i<mainParallax.transform.childCount;i++)
		{
			mainParallax.transform.GetChild(i).GetComponent<SpriteRenderer>().size = new Vector2(182,13.5f);
		}
	}
	public void playSound(int ID)
	{
		if(ID<5)
		aSource.PlayOneShot(sounds[Mathf.Clamp(ID,0,sounds.Length-1)]);
		else data.playUnlistedSound(sounds[ID]);
	}
	// Update is called once per frame
	void Update()
	{
		if(Application.isPlaying)
		{
			if(moving&&Time.timeScale!=0)
			{
				transform.position = Vector3.MoveTowards(transform.position,new Vector3(cam.transform.position.x+camCenterOffset,transform.position.y,transform.position.z),speed);
			}
			//transform.position+=new Vector3(speed,0,0);
			if(timeStopFrames>0)
			{
				timeStopFrames--;
				if(timeStopFrames==0)
				{
					Time.timeScale = 1;
				}
			}
		}
	}
	IEnumerator Damage()
	{
		blocker.SetActive(false);
		if(attacking!=null)
		StopCoroutine(attacking);
		autoScrollObj.SetActive(false);
		stopped = false;
		moving = false;
		camBounds.gameObject.SetActive(false);
		anim.SetBool("Moving",moving);
		cutscenePlayer(true);
		if(eventInt<=2)
		{
			cam.AssignTarget(transform.GetChild(0).GetChild(0));
			cam.damping = 0.5f;
			Time.timeScale = 0;
			cam.lockRight = false;
			cam.hardLockRight = false;
			cam.lockCamera = false;
			cam.overWriteLockScroll = true;
			cam.workInStoppedTime = true;
			anim.SetBool("Hurt",true);
			aSource.PlayOneShot(sounds[0]);
			timeStopFrames = 60;
			if(eventInt==1)
			phaseOffset = 21;
			else phaseOffset = 42;
			yield return new WaitUntil(()=> Time.timeScale != 0);
			aSource.PlayOneShot(sounds[6]);
			cam.workInStoppedTime = false;
			cam.damping = 0.2f;
			yield return new WaitForSeconds(2f);
			cam.fadeScreen(true);
			yield return new WaitUntil(()=> cam.fadeAnim>=1f);
			//prepare next area
			cam.enabled = false;
			cam.transform.position = new Vector3(65,11-21*(eventInt),-10);
			mainParallax.transform.position = new Vector3(mainParallax.transform.position.x,mainParallax.transform.position.y-21,mainParallax.transform.position.z);
			updateBackgrounds(eventInt+1);
			transform.position = new Vector3(72,transform.position.y-21,transform.position.z);
			player.transform.position = new Vector3(55,6-(21*(eventInt)),player.transform.position.z);
			camBounds.transform.position -= new Vector3(0,21,0);
			camBounds.gameObject.SetActive(true);
			anim.SetBool("Hurt",false);
			newEvent = false;
			yield return new WaitForSeconds(0.5f);
			cam.overWriteLockScroll = false;
			cam.lockRight = true;
			cam.hardLockRight = true;
			autoScrollObj.SetActive(true);
			blocker.SetActive(true);
			moving = true;
			anim.SetBool("Moving",moving);
			cam.enabled = true;
			cam.AssignTarget(player.transform);
			cutscenePlayer(false);
			pScript.controllable = true;
			pScript.axis.acceptXInputs = true;
			pScript.axis.acceptYInputs = true;
			cam.fadeScreen(false);
			spawnText(eventInt+1);
			yield return new WaitUntil(()=> cam.fadeAnim<=0f);
			//Debug.Log("faded out");
			switch(eventInt)
			{
				default: attacking = StartCoroutine(attackLoop(4)); break;
				case 2: attacking = StartCoroutine(attackLoop(3)); break;
			}
		}
		//death
		else
		{
			eventInt = 3;
			pScript.goToCutsceneMode(true);
			anim.SetBool("Dead",true);
			head = transform.GetChild(0).GetChild(2);
			cam.AssignTarget(head);
			anim.updateMode = AnimatorUpdateMode.UnscaledTime;
			cam.damping = 0.5f;
			Time.timeScale = 0;
			cam.lockRight = false;
			cam.lockCamera = false;
			cam.overWriteLockScroll = true;
			cam.workInStoppedTime = true;
			aSource.PlayOneShot(sounds[3]);
			data.stopAllMusic();
			yield return new WaitUntil(()=>Time.timeScale!=0);
			aSource.PlayOneShot(sounds[6]);
			anim.updateMode = AnimatorUpdateMode.Normal;
			cam.damping = 0f;
			StopCoroutine(winWait);
			bool newClear = false;
				if(data.currentLevelProgress!="")
				{
					char c = data.currentLevelProgress[0];
					if(c=='N')
						newClear = true;
					if(c!='D')
					{
						data.currentLevelProgress = data.currentLevelProgress.Remove(0, 1);
						if(data.cheated||dataS.difficulty!=2)
						data.currentLevelProgress = data.currentLevelProgress.Insert(0,"F");
						else data.currentLevelProgress = data.currentLevelProgress.Insert(0,"D");
					}
					
					dataS.coins+=data.coins;
					data.saveLevelProgress(newClear,false);
					yield return new WaitUntil(()=> data.finishedSaving);
					dataS.resetValues();
				}
			yield return new WaitUntil(()=>cam.fadeAnim>=1);
			if(newClear)
			dataS.loadWorldWithLoadScreen(3);
			else dataS.loadWorldWithLoadScreen(dataS.currentWorld);
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		//Debug.Log(other.name);
		if(other.name == "BossStopper")
		{
			moving = false;
			stopped = true;
			anim.SetBool("Moving",moving);
			autoScrollObj.SetActive(false);
		}
		if(other.name == "PlayerCollider")
		{
			//print(player.transform.position.y+" "+slashPoint.position.y);
			if(pScript.midDive
			&&pScript.transform.GetComponent<playerSprite>().state==3
			&&player.transform.position.y>slashPoint.position.y
			&&!newEvent)
			{
				//Debug.Log("Stomped");
				cutscenePlayer(false);
				eventInt++;
				newEvent = true;
				StartCoroutine(Damage());
				if(eventInt<3)
				pScript.stompBoss(gameObject,true);
			}
			else
			{
				//Debug.Log("Knockback");
				if(Time.timeScale!=0 && pScript.invFrames==0)
				{
					pScript.Damage(false,false);
				}
				if(pScript.knockbackCor !=null)
				pScript.StopCoroutine(pScript.knockbackCor);
				if(player.transform.position.x<transform.position.x)
				pScript.knockbackCor = pScript.StartCoroutine(pScript.knockBack(-1,0.3f,0.3f,false));
				else
				pScript.knockbackCor = pScript.StartCoroutine(pScript.knockBack(1,0.3f,0.3f,false));
			}
		}
	}
	void cutscenePlayer(bool activate)
	{
		if(activate)
		{
			pScript.rb.velocity = new Vector2(0,pScript.rb.velocity.y);
		}
		pScript.controllable = !activate;
		pScript.knockedBack = activate;
		pScript.inCutscene = activate;
		pScript.axis.acceptXInputs = !activate;
		pScript.axis.acceptYInputs = !activate;
	}
}
