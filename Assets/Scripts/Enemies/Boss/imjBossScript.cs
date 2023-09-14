using System.Collections;
using UnityEngine;

public class imjBossScript : MonoBehaviour {
	int phase = 0;
	public int currentAttack = 0;
	Animator anim;
	bool attackInterrupt = false;
	Coroutine cor;
	PlayerMusicBounce playerBeat;
	public Sprite[] sprites = new Sprite[2];
	SpriteRenderer render;
	public GameObject[] plates = new GameObject[3];
	Transform objSpawnPoint;
	GameObject razor;
	AudioSource aS;
	public AudioClip[] sclips;
	Animation animWarn;
	public Animator stageScroll;
	ParticleSystem explodeLoop;
	float[] phaseTimer = new float[]{2,1.5f,1f};
	Transform master;
	public float speed = 0.1f;

	float[] boomBoxStops = new float[3];
	MGCameraController cam;
	SpriteRenderer whiteOverlay;
	dataShare DataS;
	GameData data;
	PlayerScript pScript;
	public Transform cutscene;
	bool audioPause = false;
	Transform boomBoxParent,checkpointHold;

	void loadCheckData()
	{
		int val = DataS.checkpointValue;
		if(val==0)return;

		phase = Mathf.Clamp(val,1,2);
		//destroy boom blocks
		Transform[] b = new Transform[phase];
		for(int i = 0;i<b.Length;i++)
		{
			b[i] = boomBoxParent.GetChild(i);
		}

		for(int i = 0;i<b.Length;i++)
		{
			Destroy(b[i].gameObject);
		}
		master.position = new Vector3(master.position.x,boomBoxStops[phase-1],0);

		if(val==1)
		{
			pScript.transform.localPosition = new Vector3(-2,-4,0);
			switchDir();
		}
		cam.transform.localPosition = new Vector3(0,0.5f,-10);
	}
	// Use this for initialization
	void Start ()
	{
		anim = transform.GetChild(0).GetComponent<Animator>();
		aS = GetComponent<AudioSource>();
		playerBeat = GameObject.Find("Player_main").GetComponent<PlayerMusicBounce>();
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		objSpawnPoint = transform.GetChild(0).GetChild(0);
		razor = transform.parent.GetChild(1).gameObject;
		animWarn = transform.GetChild(1).GetComponent<Animation>();
		explodeLoop = transform.GetChild(0).GetChild(5).GetComponent<ParticleSystem>();
		master = transform.parent.parent;
		whiteOverlay = transform.parent.GetChild(2).GetComponent<SpriteRenderer>();
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
		boomBoxParent = GameObject.Find("LevelGrid").transform.GetChild(10);
		checkpointHold = GameObject.Find("Checkpoints").transform;
		for(int i = 0; i<3;i++)
		{
			boomBoxStops[i] = (boomBoxParent.GetChild(i).position.y)-1.5f;
		}
		System.Array.Sort(boomBoxStops);
		for(int i = 0; i< plates.Length;i++)
		{
			if(i == 0)
			{
				plates[i] = transform.parent.GetChild(0).gameObject;
				plates[i].transform.position = transform.position;
				plates[i].transform.eulerAngles = transform.eulerAngles;
			}
			else
			{
				plates[i] = Instantiate(plates[0],transform.position,Quaternion.identity);
				plates[i].transform.parent = transform.parent;
			}
			plates[i].SetActive(false);
		}
		if(DataS.playCutscene)
		cutscene.gameObject.SetActive(true);

		//Load checkpoint data
		loadCheckData();
		//else cam.fadeScreen(false);
		cor = StartCoroutine(attackLoop());
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
	// Update is called once per frame
	void LateUpdate ()
	{
		if(playerBeat.frame == 1 && render.sprite == sprites[0])
		{
			render.sprite = sprites[1];	
		}
	}
	public IEnumerator explodeSoundLoop()
	{
		float delay = 1/explodeLoop.emission.rateOverTime.constant;
		explodeLoop.Play();
		while(Application.isPlaying)
		{
			aS.PlayOneShot(sclips[7+Random.Range(0,3)]);
			yield return new WaitForSeconds(delay);
		}
	}
	void Update()
	{
		if(phase<3)
		master.position = Vector3.MoveTowards(master.position,new Vector3(master.position.x,boomBoxStops[phase],0),speed*Time.timeScale);

		if(master.position.y==boomBoxStops[phase]&&stageScroll.speed!=0)
		{
			stageScroll.speed=0;
		}
		else if(master.position.y!=boomBoxStops[phase]&&stageScroll.speed==0)
		{
			stageScroll.speed=speed*20;
		}
		
		if(Time.timeScale==0&&aS.isPlaying&&!audioPause)
		{
			audioPause = true;
			aS.Pause();
		}
		else if(Time.timeScale!=0&&!aS.isPlaying&&audioPause)
		{
			audioPause = false;
			aS.UnPause();
		}
	}
	public void switchDir()
	{
		transform.localScale = new Vector3(-transform.localScale.x,1,1);
		transform.position = new Vector3(-transform.position.x,transform.position.y,transform.position.z);
	}
	public void endScene()
	{
		StartCoroutine(fade());
	}
	IEnumerator fade()
	{
		cutscenePlayer(true);
		byte alpha = 0;
		while(alpha<255)
		{
			alpha++;
			whiteOverlay.color = new Color32(255,255,255,alpha);
			yield return 0;
		}
		yield return new WaitForSeconds(1f);
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
		if(newClear)DataS.loadWorldWithLoadScreen(4);
		else DataS.loadWorldWithLoadScreen(DataS.currentWorld);
	}
	//This is called at the end of the hurt animation.
	public void newPhase()
	{
		//print("new phase");
		attackInterrupt = false;
		currentAttack = 0;
		checkpointHold.GetChild(phase).gameObject.SetActive(true);
		phase++;

		//DataS.checkpointValue = Mathf.Clamp(phase,0,2);

		//TODO: shit related to phase changes
		if(cor!=null)
		StopCoroutine(cor);
		cor = StartCoroutine(attackLoop());
	}
	public void playSound(int i)
	{
		aS.PlayOneShot(sclips[Mathf.Clamp(i,0,sclips.Length)]);
	}
	public void playLoopingSound(int i)
	{
		if(aS.isPlaying)
		aS.Stop();
		aS.clip = sclips[Mathf.Clamp(i,0,sclips.Length)];
		aS.loop = true;
		aS.Play();
	}
	public void killLoopSound()
	{
		if(aS.isPlaying)
		aS.Stop();

		aS.loop = false;
	}
	public void summonWarning()
	{
		animWarn.Play();
	}
	public void spawnPlate()
	{
		for(int i = 0; i< plates.Length;i++)
		{
			if(!plates[i].activeInHierarchy)
			{
				plates[i].transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,1);
				plates[i].transform.position = objSpawnPoint.position;
				plates[i].SetActive(true);
				break;
			}
		}
	}
	public void spawnRazor()
	{
		razor.SetActive(false);
		razor.GetComponent<Spinner>().speed *= -transform.localScale.x;
		razor.GetComponent<simpleMovement>().speed *= -transform.localScale.x;
		razor.transform.position = objSpawnPoint.position;
		razor.SetActive(true);
		StartCoroutine(razorDespawn());
	}
	IEnumerator razorDespawn()
	{
		yield return new WaitForSeconds(5f);
		razor.SetActive(false);
		razor.GetComponent<Spinner>().speed *= -transform.localScale.x;
		razor.GetComponent<simpleMovement>().speed *= -transform.localScale.x;
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		//print(other.name);
		//Only nuke damages the boss.
		if(other.name=="ScreenNuke")
		{
			//Debug.Log("Hurt");
			//Getting hurt (first 2 phases);
			if(phase<2)
			{
				attackInterrupt=true;
				anim.SetTrigger("Hurt");
			}
			//Die animation.
			else
			{
				attackInterrupt=true;
				anim.SetTrigger("Die");
			}
		}
	}
	IEnumerator attackLoop()
	{
		yield return new WaitUntil(()=>Time.timeScale!=0);
		while(!attackInterrupt)
		{
			//idle
			yield return new WaitForSeconds(phaseTimer[phase]*2);

			//launch next attacks, clamp and return to 1 at 5
			//print(currentAttack);
			currentAttack++;
			if(currentAttack>4)
				currentAttack = 1;
			anim.SetInteger("Attack",currentAttack);
			yield return 0;
			anim.SetInteger("Attack",0);
			yield return new WaitUntil(()=>anim.GetCurrentAnimatorClipInfo(0)[0].clip.name=="imj_idle");
			yield return new WaitForSeconds(phaseTimer[phase]);
		}
		currentAttack = 0;
		//
		attackInterrupt = false;
		if(cor!=null)
			StopCoroutine(cor);
		cor = null;
	}
}
