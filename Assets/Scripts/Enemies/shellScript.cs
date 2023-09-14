using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shellScript : MonoBehaviour {
	bool loaded = false;
	public Sprite[] discSprites;
	public float secondsUntilRespawn = 10f;
	List<Sprite> regularSprites;
	float frameSpeed;
	public Coroutine waitUntilReturn;
	Rigidbody2D rb;
	public float direction;
	public float speed = 5f;
	public bool faceDirection = true;
	public LayerMask whatIsGround;
	public LayerMask whatIsHittable;
	public float rayCastLength = 0.37f;
	public bool moving = true;
	public bool sendContactInfo = false;
	bool canCollide = true;
	public bool kickable = true;
	public int discValue = 0;
	public int discKillStreak = 0;
	EnemyOffScreenDisabler eneOffScreen;
	public GameObject defaultParent;
	GameData data;
	public bool hurtsPlayer = true;
	CompositeCollider2D semiSolid;
	bool ignoreSemiSolid = false;
	public bool grabbable = false;
	int framesUntilRevert = 0;
	public int stompAllowWait = 0;
	public bool pauseAnimation = false;
	public bool inverted=false;
	public bool inLava = false;
	bool parented = false;
	float collisionDetectOffset = 0.05f,crusherMaxHeight = 0,crusherDetectionOffset = 0;
	LayerMask whatIsSolidGround;
	EnemyCorpseSpawner eneCorpse;
	checkForSemiSolid checker;
	MovementAI ai;
	Gravity grav;
	Transform player;
	IEnumerator respawn()
	{
		//Debug.Log("Triggered respawn timer.");
		framesUntilRevert = Mathf.RoundToInt(secondsUntilRespawn*60);
		yield return new WaitUntil(()=> framesUntilRevert<=150);
		startTwitching();
	}
	public IEnumerator grabWaitCor()
	{
		grabbable = false;
		yield return new WaitForSeconds(0.1f);
		grabbable = true;
	}
	public void shortStartTwitch()
	{
		pauseAnimation = false;
		framesUntilRevert = 150;
		GetComponent<Animation>().Play();
	}
	public void startTwitching()
	{
		pauseAnimation = false;
		if(framesUntilRevert<=150)
		{
			framesUntilRevert = 150;
			GetComponent<Animation>().Play();
		}
	}
	public void stopTwitching()
	{
		//Debug.Log(gameObject.name+"'s twitching has been interrupted.");
		Animation anim = GetComponent<Animation>();
		pauseAnimation = true;
		if(!anim.isPlaying)
			anim.Stop();
		if(framesUntilRevert<=150)
		{
			framesUntilRevert = 150;
		}
	}
	public IEnumerator returnToEnemy()
	{
		EnemyOffScreenDisabler enec = GetComponent<EnemyOffScreenDisabler>();
		enec.canUnload = false;
		grabbable = false;
		//Debug.Log("Timer ended.");
		discKillStreak = 0;
		SimpleAnim2 anim = transform.GetChild(0).GetComponent<SimpleAnim2>();
		eneOffScreen.canEnableAI = true;
		eneOffScreen.despawnWait = 0;
		if(player==null)player = GameObject.Find("Player_main").transform;
		float dist = (transform.position.x-player.position.x)<0?-1:1;
		changeDirection(Mathf.RoundToInt(dist),false);
		transform.localScale = new Vector3(dist,1,transform.localScale.z);
		transform.GetChild(0).gameObject.tag = "Enemy";
		transform.GetChild(0).gameObject.layer = 12;
		anim.StopPlaying();
		anim.waitBetweenFrames = frameSpeed;
		anim.playOnAwake = true;
		anim.sprites.Clear();
		for(int i = 0; i<regularSprites.Count;i++)
		anim.sprites.Add(regularSprites[i]);
		yield return 0;
		if(eneOffScreen.visible)
		anim.StartPlaying();

		discParent(false,Vector3.zero);
		yield return new WaitUntil(()=> rb.simulated);
		
		if(eneOffScreen.visible)
		{
			ai.enabled = true;
		}
	    this.enabled = false;
		enec.canUnload = true;
	}
	public void changeDirection(int dir,bool playImpact)
	{
		direction = dir;
			if(faceDirection)
				transform.localScale = new Vector3(direction,transform.localScale.y,transform.localScale.z);
		if(moving&&playImpact&&transform.GetChild(0).gameObject.activeInHierarchy)
		spawnImpactKick();
	}
	void crusher()
	{
		RaycastHit2D rayLeft = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.right,0.3f,whatIsGround);
		RaycastHit2D rayRight = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),transform.right,0.3f,whatIsGround);
		RaycastHit2D rayDown = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.up,0.3f,whatIsGround);
		RaycastHit2D rayUp = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5+crusherMaxHeight,0),transform.up,0.5f,whatIsSolidGround);

		if(rayLeft.collider!=null&rayRight.collider!=null&&rayLeft.collider.transform!=rayRight.collider.transform
		||rayUp.collider!=null&rayDown.collider!=null&&rayUp.collider.transform!=rayDown.collider.transform)
		{
			if(sendContactInfo)
			{
				print(transform.name+" crushed");
				if(rayUp.collider!=null)
				print(gameObject.name+" "+rayUp.transform.name);
				if(rayDown.collider!=null)
				print(gameObject.name+" "+rayDown.transform.name);
				if(rayLeft.collider!=null)
				print(gameObject.name+" "+rayLeft.transform.name);
				if(rayRight.collider!=null)
				print(gameObject.name+" "+rayRight.transform.name);
			}
			if(eneCorpse!=null)
			{
				data.playSoundOverWrite(20,transform.position);
				eneCorpse.createCorpseFlipped = true;
				eneCorpse.createCorpse = false;
				eneCorpse.spawnCorpse();
			}
			else
			{
				data.playSoundOverWrite(20,transform.position);
				Destroy(gameObject);
			}
		}
	}
	IEnumerator colDetectCooldown()
	{
		canCollide = false;
		yield return 0;
		canCollide = true;
	}
	// Use this for initialization
	void Start () {

		regularSprites = new List<Sprite>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		if(defaultParent==null)
		defaultParent = GameObject.Find("Enemies");
		rb = GetComponent<Rigidbody2D>();
		eneOffScreen = GetComponent<EnemyOffScreenDisabler>();
		grav = GetComponent<Gravity>();
		ai = GetComponent<MovementAI>();
		if(this.enabled)
		this.enabled = false;
		loaded = true;
		SimpleAnim2 anim = transform.GetChild(0).GetComponent<SimpleAnim2>();
		for(int i = 0; i<anim.sprites.Count;i++)
			regularSprites.Add(anim.sprites[i]);
		frameSpeed = anim.waitBetweenFrames;
		semiSolid = data.semiSolid;
		if(transform.childCount>1)
		checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
		collisionDetectOffset = ai.collisionDetectOffset;crusherMaxHeight = ai.crusherMaxHeight;crusherDetectionOffset = ai.crusherDetectionOffset;
		whatIsSolidGround = ai.whatIsGround ^ (1 << LayerMask.NameToLayer("semiSolidGround"));
		eneCorpse = GetComponent<EnemyCorpseSpawner>();
	}
	
	// Update is called once per frame
	void Update () {
		if(parented&&transform.parent!=null)
		{
			Vector3 p = transform.lossyScale;
			if(transform.lossyScale.x==-1)
			{
				p = transform.localScale;
				transform.localScale = new Vector3(-p.x,p.y,p.z);
				p = transform.localScale;

			}
		}
		if(Mathf.Round(transform.eulerAngles.z)!=0&&!inverted)
		{
			inverted=true;
			ai.flipBoxes(inverted);
		}
		else if(Mathf.Round(transform.eulerAngles.z)==0&&inverted)
		{
			inverted=false;
			ai.flipBoxes(inverted);
		}
		if(!moving && framesUntilRevert>0 && !pauseAnimation &&Time.timeScale!=0)
		{
			framesUntilRevert--;
			if(framesUntilRevert==0 && this.enabled)
				StartCoroutine(returnToEnemy());
		}
		if(moving && eneOffScreen.visible)
		{
			rb.velocity=new Vector2(speed*direction+grav.pushForces.x,rb.velocity.y);
		}
		else if(!moving&&rb.bodyType==RigidbodyType2D.Dynamic)
		{
			rb.velocity=new Vector2(0,rb.velocity.y);
		}
		if(transform.childCount>1)
		{
			if(!inverted)
			{
				if(rb.velocity.y>0&&!ignoreSemiSolid
				||!ignoreSemiSolid&&!checker.insideSemiSolid)
				{
					ignoreSemiSolid = true;
					Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
					whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
					//Debug.Log(gameObject.name+" Ignoring collision");
				}
				else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid
				||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
				{
					ignoreSemiSolid = false;
					Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
					whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
					//Debug.Log(gameObject.name+" Not ignoring collision");
				}
			}
			else
			{
				if(rb.velocity.y<0&&!ignoreSemiSolid
				||rb.velocity.y<0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
				{
					ignoreSemiSolid = true;
					Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
					whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
					//Debug.Log("Shell Ignoring collision");
				}
				else if(rb.velocity.y>=0&&ignoreSemiSolid&&checker.insideSemiSolid
				||rb.velocity.y>=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
				{
					ignoreSemiSolid = false;
					Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
					whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
					//Debug.Log(gameObject.name+" Not ignoring collision");
				}
			}
		}
		if(Time.timeScale!=0)
		{
			crusher();
			if(stompAllowWait>0)
			{
				stompAllowWait--;
			}
		}

	}
	void OnEnable()
	{
		if(loaded)
		{
		SimpleAnim2 anim = transform.GetChild(0).GetComponent<SimpleAnim2>();
		anim.StopPlaying();
		anim.waitBetweenFrames = 0.05f;
		anim.playOnAwake = false;
		anim.sprites.Clear();
		for(int i = 0; i<discSprites.Length;i++)
			{
				anim.sprites.Add(discSprites[i]);
			}
		transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = discSprites[0];

		if(waitUntilReturn!=null)
			StopCoroutine(waitUntilReturn);
		waitUntilReturn = StartCoroutine(respawn());
		}
	}
	void wallDetect(Vector2 contact)
	{
		Vector3 point = new Vector3(transform.position.x,contact.y,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(point,-transform.right,rayCastLength,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(point, transform.right,rayCastLength,whatIsGround);

		Debug.DrawLine(point,new Vector3(point.x-rayCastLength,point.y,point.z),Color.red);
		Debug.DrawLine(point,new Vector3(point.x+rayCastLength,point.y,point.z),Color.blue);
		bool d=false;

		if(Mathf.Round(contact.y) > Mathf.Floor(transform.position.y)&&!inverted
		||Mathf.Round(contact.y) < Mathf.Ceil(transform.position.y)&&inverted)
			{
				RaycastHit2D UsedRay;
				if(direction == -1f)
				{
					//Debug.Log("used ray: ray1");
					if(!inverted)
					UsedRay = ray1;
					else UsedRay = ray2;
				}
				else
				{
					//Debug.Log("used ray: ray2");
					if(!inverted)
					UsedRay = ray2;
					else UsedRay = ray1;
				}
				Debug.DrawLine(transform.position,contact,Color.yellow,0.3f);
				if(UsedRay.collider != null)
				{
					StartCoroutine(colDetectCooldown());
					//Debug.Log("registered");
					Debug.DrawLine(transform.position,UsedRay.point,Color.green,0.3f);
					Vector2 blockPoint = Vector2.zero;
					if(direction == 1f)
					{
						blockPoint = new Vector2(UsedRay.point.x+0.5f,UsedRay.point.y-0.5f);
					}
					else if(direction == -1f) blockPoint = new Vector2(UsedRay.point.x-0.5f,UsedRay.point.y-0.5f);
					
					if(inverted)d = data.blockHit(blockPoint,transform.position,gameObject.name,false);
					else d = data.blockHit(blockPoint,new Vector3(transform.position.x,Mathf.Floor(transform.position.y-0.5f),transform.position.z),gameObject.name,false);
				}
			}
		if(!inverted)
		{
			if(ray1.collider!=null&&direction!=1f)
			{
				//Debug.Log(transform.name+ " detected barrier on the left");
				//turn around logic
				changeDirection(1,true);
				if(!d)
				data.playSound(1,transform.position);
			}
			if(ray2.collider!=null&&direction!=-1f)
			{
				//Debug.Log(transform.name+ " detected barrier on the right");
				//turn around logic
				changeDirection(-1,true);
				if(!d)
				data.playSound(1,transform.position);
			
			}
		}
		else
		{
			if(ray2.collider!=null&&direction!=1f)
			{
				//Debug.Log(transform.name+ " detected barrier on the left");
				//turn around logic
				changeDirection(1,true);
				if(!d)
					data.playSound(1,transform.position);
			}
			if(ray1.collider!=null&&direction!=-1f)
			{
				//Debug.Log(transform.name+ " detected barrier on the right");
				//turn around logic
				changeDirection(-1,true);
				if(!d)
					data.playSound(1,transform.position);
			
			}
		}
	}
	public void spawnImpactKick()
	{
		//float inv = transform.position.x>=other.position.x ? 1 :-1;
		data.spawnImpact(transform.position+new Vector3(0.4f*-direction,0.2f*(inverted?-1:1),0));
	}
	public void discLaunch(float pos)
	{
		if(inLava)
		{
			moving = false;
			return;
		}
		SimpleAnim2 anim = transform.GetChild(0).GetComponent<SimpleAnim2>();
		if(!moving && transform.GetChild(0).gameObject.tag == "Disc")
		{
		//Debug.Log("Launched");
		eneOffScreen.despawnWait = 3;
		discParent(false,Vector3.zero);
		if(waitUntilReturn!=null)
			StopCoroutine(waitUntilReturn);

		GetComponent<Animation>().Stop();
		transform.GetChild(0).localEulerAngles = Vector3.zero;
		anim.waitBetweenFrames = 0.05f;
		anim.StartPlaying();
		if(pos < transform.position.x)
		transform.localScale = new Vector3(1,transform.localScale.y,transform.localScale.z);
		else transform.localScale = new Vector3(-1,transform.localScale.y,transform.localScale.z);

		direction = transform.localScale.x;
		moving = true;
		}
		else if(moving)
		{
			StartCoroutine(grabWaitCor());
			moving = false;
			discKillStreak=0;
			rb.velocity = new Vector2(0,rb.velocity.y);
			anim.StopPlaying();
			if(waitUntilReturn!=null)
				StopCoroutine(waitUntilReturn);
			waitUntilReturn = StartCoroutine(respawn());
		}
	}
	public void discParent(bool isParented,Vector3 parentedPos)
	{
		SpriteRenderer render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		parented = isParented;
		if(isParented)
		{
			render.sortingLayerName = "Player";
			render.sortingOrder = 2;
			transform.localPosition = parentedPos;
			GetComponent<Gravity>().enabled = false;
			rb.bodyType = RigidbodyType2D.Static;
			rb.simulated = false;
		}
		else
		{
			transform.SetParent(defaultParent.transform);
			rb.bodyType = RigidbodyType2D.Dynamic;
			GetComponent<Gravity>().enabled = true;
			render.sortingLayerName = "Default";
			render.sortingOrder = -1;
			rb.simulated = true;
		}
	}
	public void resetCor()
	{
		if(waitUntilReturn!=null)
			StopCoroutine(waitUntilReturn);
		waitUntilReturn = StartCoroutine(respawn());
		GetComponent<Animation>().Stop();
		transform.GetChild(0).localEulerAngles = Vector3.zero;
	}
	void OnCollisionStay2D(Collision2D other)
	{
		if(this.enabled)
		{
			string s = other.gameObject.tag;
		if((s == "Ground"||s == "Harm"||s=="Spring")&&canCollide&&moving)
		{
			for(int i = 0; i<other.contacts.Length;i++)
			{
				if(Mathf.Round(other.contacts[i].point.y) > Mathf.Floor(transform.position.y)&&!inverted
				||Mathf.Round(other.contacts[i].point.y) < Mathf.Ceil(transform.position.y)&&inverted)
				{
					if(sendContactInfo)
						{
							Debug.Log(Mathf.Round(other.contacts[i].point.y)+" "+Mathf.Floor(transform.position.y));
							Debug.DrawLine(transform.position,other.contacts[i].point,Color.red,0.3f);
							Debug.Log(gameObject.name+" ground collision");
						}
					wallDetect(other.contacts[i].point);
					if(s=="Spring")
					data.playSound(1,transform.position);
				}
			}
		}
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="InstantDeath"&&!inLava)
		{
			stopTwitching();
			inLava = true;
			moving = false;
			hurtsPlayer = false;
			kickable = false;
			rb.velocity = new Vector2(0,rb.velocity.y);
			data.spawnCheeseSplatterPoint(transform.position);
		}
	}
	void OnTriggerStay2D(Collider2D other)
	{
		if(moving)
		{
		coinScript c = other.gameObject.GetComponent<coinScript>();
		if(c!=null&&!c.collected)
		{
			Animator an = other.transform.GetChild(0).GetComponent<Animator>();
			if(an!=null&&an.enabled)
				an.SetTrigger("collected");
			c.killCoin();
			switch(other.name)
			{
				default: break;
				case "Coin":;
				data.addCoin(1,true);
				data.playSound(0,transform.position);
				break;

				case "Floppy":
				data.addFloppy(1,true);
				data.playSound(34,transform.position);
				break;

				case "FloppyGhost":
				data.addScore(2000L);
				data.addCoin(30,true);
				data.playSound(79, other.transform.position);
				data.ScorePopUp(transform.position, "+2000", Color.white);
				data.playSound(34,transform.position);
				break;

				case "Sausage":
				data.addCoin(10,true);
				data.addScore(7900);
				data.ScorePopUp(transform.position,"+8000",Color.white);
				data.collectSausage(other.gameObject);
				data.playSound(6,transform.position);
				break;

				case "Ghost Sausage":
				data.addCoin(10,true);
				data.addScore(900);
				data.ScorePopUp(transform.position,"+1000",Color.white);
				data.playSound(6,transform.position);
				break;
			}
		}
		if(other.name== "Time_Apple")
			{
				data.addScore(1000);
				data.ScorePopUp(transform.position,"+1000",new Color32(255,255,155,255));
				data.addTime(100);
				Destroy(other.transform.gameObject);
			}
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "PlayerCollider"&&!kickable)
		{
			kickable = true;
		}
		if(other.name == "PlayerCollider"&&!hurtsPlayer)
		{
			hurtsPlayer = true;
		}
	}
}
