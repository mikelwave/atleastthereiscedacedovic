using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;

public class EnemyCorpseSpawner : MonoBehaviour {
	#region enemy script
	public bool debugPrintPosition = false;
	public Sprite deadSprite,deadSprite2;
	public bool destroyEnemy = true;
	public GameObject main;
	public GameObject flipped;
	GameData data;
	public Vector2 flipVelo;
	Vector2 flipVeloUsed;
	public float angVelocity;
	bool spawnedCorpse = false;
	public bool createCorpse = true;
	public bool createCorpseFlipped = false;
	public bool turnIntoDisc = false;
	[HideInInspector]
	public bool hitFlag = false;
	public bool stompFlag = false;
	public Vector2 flipOffset = new Vector2(1f,0.35f);
	public Vector3 corpseSpawnOffset = Vector3.zero, corpse2SpawnOffset;
	public Vector3 pointDisplayOffset = Vector3.zero;
	public float stompOffset = 0;
	public long pointValue = 200;
	public AudioClip deathSound,stompSound;
	[HideInInspector]
	public bool playDeathSound = true;
	public AudioClip[] additionalDeathSounds;
	public bool canDie = true;
	public bool useDeadSpriteForFlip = false;
	public bool buildsCombo = true;
	public bool stompBuildsCombo = true;
	public bool flippedStartUpright = false;
	public bool invertable = true;
	public bool disableOnCorpseSpawn = false;
	public bool bustBlock = false;
	public GameObject[] objectsToUnParentOnDeath;
	[Header("Unparented objects")]
	[Space]
	public bool setVelocity = false;
	public Vector2 releaseVelocity = Vector2.zero;
	[Space]
	public LayerMask whatIsGround;
	int inverter = 1;
	public float collisionDetectOffset = 0.05f,crusherMaxHeight = 0;
	public bool canGetCrushed = false;
	public Transform readPositionFrom;

	[Header ("Death sources")]
	public bool killOffscreen = false;
	public bool blockHitKills = true;
	public bool discKills = true;
	public bool thrownBlockKills = true;
	public bool screenNukeKills = true;
	public bool FireballKills = true;
	public bool invincibilityKills = true;
	public bool axeKills = true;
	public bool lKnifeKills = true;
	Rigidbody2D rbody;
	EnemyOffScreenDisabler eneOff;
	public void givePoints()
	{
		if(stompBuildsCombo)
		{
			if(pointValue==200||data.stompStreak>7)
			{
				data.stompStreak++;
				data.streakScore(data.stompStreak,readPositionFrom.position+pointDisplayOffset,false);
			}
			else
			{
				data.playComboSound(0);
				data.stompStreak++;
				data.addScore(pointValue);
				data.ScorePopUp(readPositionFrom.position+pointDisplayOffset,"+"+pointValue,new Color32(255,255,255,255));
			}
		}
	}
    public void spawnCorpse()
	{
		DeathEventTriggered();
		for(int i = 0; i<objectsToUnParentOnDeath.Length;i++)
		{
			if(objectsToUnParentOnDeath[i]!=null)
			{
				objectsToUnParentOnDeath[i].transform.parent = transform.parent;
				if(setVelocity)
				{
					objectsToUnParentOnDeath[i].GetComponent<Rigidbody2D>().velocity = releaseVelocity;
				}
			}
		}
		if(createCorpse)
		{
		stompFlag = true;
		SpriteRenderer render = gameObject.AddComponent<SpriteRenderer>();
		if(inverter==-1)
		render.flipX = true;
		SpriteRenderer orgRender = transform.GetChild(0).GetComponent<SpriteRenderer>();
		render.sortingOrder = orgRender.sortingOrder;
		render.sortingLayerID = orgRender.sortingLayerID;
		render.color = orgRender.color;
		GetComponent<BoxCollider2D>().offset = transform.GetChild(0).GetComponent<BoxCollider2D>().offset;
		GetComponent<BoxCollider2D>().size = transform.GetChild(0).GetComponent<BoxCollider2D>().size;
		Destroy(transform.GetChild(0).gameObject);
		GetComponent<BoxCollider2D>().isTrigger = false;
		rbody = GetComponent<Rigidbody2D>();
		rbody.velocity = new Vector2(0,rbody.velocity.y);
		if(transform.name!="Enemy_Corpse")
		{
			render.sprite = deadSprite;
			gameObject.layer = 16;
			gameObject.name = "Enemy_Corpse";
			buildsCombo = false;
			transform.position+=corpseSpawnOffset;
			if(stompBuildsCombo)
			{
			float streak = data.stompStreak;
			data.playeneSound(stompSound,1f+(streak/20));
			}
			else data.playeneSound(stompSound,1f);
		}
		if(destroyEnemy)
			{
				transform.parent = GameObject.Find("Enemies").transform;
				Destroy(GetComponent<MovementAI>());
			}
		}
		if(turnIntoDisc)
		{
			shellScript shell = GetComponent<shellScript>();
			eneOff.canEnableAI = false;
			GetComponent<MovementAI>().enabled = false;
			shell.enabled = true;
			shell.StartCoroutine(shell.grabWaitCor());
			rbody = GetComponent<Rigidbody2D>();
			if(rbody.bodyType!=RigidbodyType2D.Static)
			rbody.velocity = new Vector2(0,rbody.velocity.y);
			transform.GetChild(0).gameObject.tag = "Disc";
			transform.GetChild(0).gameObject.layer = 17;
			if(stompBuildsCombo)
			{
			float streak = data.stompStreak;
			data.playeneSound(stompSound,1f+(streak/20));
			}
			else data.playeneSound(stompSound,1f);
		}
		if(createCorpseFlipped)
		{
			Vector3 pos;
			if(flippedStartUpright)
			pos = readPositionFrom.position+corpseSpawnOffset;
			else if(transform.name!="Enemy_Corpse")
			pos = new Vector3(readPositionFrom.position.x+corpseSpawnOffset.x,readPositionFrom.position.y+(flipOffset.x*inverter)+corpseSpawnOffset.y,readPositionFrom.position.z);
			else pos = new Vector3(readPositionFrom.position.x+corpseSpawnOffset.x,readPositionFrom.position.y+(flipOffset.y*inverter)+corpseSpawnOffset.y,readPositionFrom.position.z);

			flippedCorpseSpawn(pos,deadSprite,false);
			if(deadSprite2!=null)
			flippedCorpseSpawn(pos,deadSprite2,true);

			if(destroyEnemy)
			Destroy(main);
			else
			{
				StartCoroutine(corpseSpawnCoolDown());
			}
		}
		if(!createCorpse&&!createCorpseFlipped&&!turnIntoDisc)
		{
			stompFlag = true;
			if(stompBuildsCombo)
			{
			float streak = data.stompStreak;
			data.playeneSound(stompSound,1f+(streak/20));
			}
			else data.playeneSound(stompSound,1f);
		}
	}
	void flippedCorpseSpawn(Vector3 pos,Sprite usedDeadSprite,bool flipXVelo)
	{
			if(invertable)
			{
				if(transform.eulerAngles.z>90)
				{
					flipVelo = new Vector2(flipVelo.x,-Mathf.Abs(flipVelo.y));
					inverter = -1;
				}
				else
				{
					flipVelo = new Vector2(flipVelo.x,Mathf.Abs(flipVelo.y));
					inverter = 1;
				}
			}
			GameObject obj;
			if(invertable)
			obj = Instantiate(flipped,pos,Quaternion.Euler(0,0,readPositionFrom.eulerAngles.z));
			else obj = Instantiate(flipped,pos,Quaternion.Euler(0,0,0));

			if(debugPrintPosition)
			print(transform.name+": "+"Flipped pos = "+(pos+corpseSpawnOffset)+", Original pos = "+transform.position+" Offset: "+corpseSpawnOffset);
			SpriteRenderer orgRender;
			SpriteRenderer newRender = obj.GetComponent<SpriteRenderer>();
			if(transform.childCount != 0 && transform.GetChild(0).GetComponent<SpriteRenderer>()!=null)
				orgRender = transform.GetChild(0).GetComponent<SpriteRenderer>();
			else orgRender = GetComponent<SpriteRenderer>();
			if(transform.name != "Enemy_Corpse" &&!useDeadSpriteForFlip)
			{
				newRender.sprite = orgRender.sprite;
				newRender.color = orgRender.color;
			}
			else
			{
				newRender.sprite = usedDeadSprite;
				newRender.color = orgRender.color;
			}
			obj.transform.localScale = transform.localScale;
			if(flippedStartUpright)
			newRender.flipY = false;
			if(transform.name!="Enemy_Corpse"&&stompBuildsCombo)
			{
				float streak = data.stompStreak;
				if(eneOff==null||eneOff.visible)
				data.playeneSound(stompSound,1f+(streak/20));
			}
			else if(transform.name!="Enemy_Corpse"&&(eneOff==null||eneOff.visible))
				data.playeneSound(stompSound,1f);
			
			Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
			if(transform.localScale.x == -1&&!flipXVelo||transform.localScale.x == 1&&flipXVelo)
			{
				flipVeloUsed = new Vector2(-flipVelo.x,flipVelo.y);
				angVelocity = -angVelocity;
			}
			else
			{
				flipVeloUsed = new Vector2(flipVelo.x,flipVelo.y);
			}
			obj.GetComponent<deadEnemyScript>().invertable = invertable;
			obj.SetActive(true);
			rb.velocity = flipVeloUsed;
			rb.angularVelocity = angVelocity;
			//print(flipVeloUsed+" "+angVelocity);
			//print(rb.velocity+" "+rb.angularVelocity);
	}
	AudioClip pickDeathSound()
	{
		if(playDeathSound)
		{
			if(additionalDeathSounds==null)
			{
				return deathSound;
			}
			else
			{
				int val = UnityEngine.Random.Range(0,additionalDeathSounds.Length+1);
				if(val==0)return deathSound;
				else return additionalDeathSounds[val-1];
			}
		}
		return null;
	}
	void Start()
	{
		data = GameObject.Find("_GM").GetComponent<GameData>();
		if(canGetCrushed&&GetComponent<MovementAI>()!=null)
		whatIsGround = GetComponent<MovementAI>().whatIsGround;
		eneOff = GetComponent<EnemyOffScreenDisabler>();
		if(readPositionFrom==null)readPositionFrom = transform;
		if(stompSound==null)
		{
			stompSound = pickDeathSound();
		}
		if(transform.eulerAngles.z!=0&&invertable)
		{
		flipVelo = new Vector2(flipVelo.x,-flipVelo.y);
		inverter = -1;
		}
		if(main==null)main = gameObject;
	}
	public void springCorpse()
	{
		if(!spawnedCorpse && !turnIntoDisc && canDie)
		{
			spawnedCorpse = true;
			flipVeloUsed = new Vector2(-flipVelo.x,flipVelo.y);
			Vector3 pos;
			if(flippedStartUpright)
			pos = readPositionFrom.position+corpseSpawnOffset;
			else
			pos = new Vector3(readPositionFrom.position.x+corpseSpawnOffset.x,readPositionFrom.position.y+(flipOffset.y*inverter)+corpseSpawnOffset.y,readPositionFrom.position.z);
			GameObject obj;
			if(invertable)
			obj = Instantiate(flipped,pos+corpseSpawnOffset,Quaternion.Euler(0,0,transform.eulerAngles.z));
			else obj = Instantiate(flipped,pos+corpseSpawnOffset,Quaternion.Euler(0,0,0));
			obj.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
			obj.transform.localScale = transform.localScale;
			Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
			obj.SetActive(true);
			rb.velocity = new Vector2(flipVeloUsed.x,flipVeloUsed.y);
			rb.angularVelocity = angVelocity;
			if(destroyEnemy)
			Destroy(main);
			else
			{
				StartCoroutine(corpseSpawnCoolDown());
			}
		}
	}
	void crusher()
	{
		RaycastHit2D rayLeft = Physics2D.Raycast(readPositionFrom.position+new Vector3(0,collisionDetectOffset*5,0),-transform.right,0.3f,whatIsGround);
		RaycastHit2D rayRight = Physics2D.Raycast(readPositionFrom.position+new Vector3(0,collisionDetectOffset*5,0),transform.right,0.3f,whatIsGround);
		RaycastHit2D rayDown = Physics2D.Raycast(readPositionFrom.position+new Vector3(0,collisionDetectOffset*5,0),-transform.up,0.3f,whatIsGround);
		RaycastHit2D rayUp = Physics2D.Raycast(readPositionFrom.position+new Vector3(0,collisionDetectOffset*5,0),transform.up,0.5f+crusherMaxHeight,whatIsGround);

		if(rayLeft.collider!=null&rayRight.collider!=null&&rayLeft.collider.transform!=rayRight.collider.transform
		||rayUp.collider!=null&rayDown.collider!=null&&rayUp.collider.transform!=rayDown.collider.transform)
		{
			data.playSoundOverWrite(20,transform.position);
			//print(transform.name+" crushed");
			if(transform.name=="Enemy_Corpse")
			{
				createCorpseFlipped = true;
				createCorpse = false;
				spawnCorpse();
			}
			else
			{
				Destroy(main);
			}
		}
	}
	void Update()
	{
		if(transform.name=="Enemy_Corpse")
			{
				if(canGetCrushed) crusher();
				if(rbody!=null) rbody.velocity=new Vector2(0,rbody.velocity.y);
			}
	}
	void OnTriggerExit2D(Collider2D other)
	{

		if(other.name == "ObjectActivator")
		{
			//print("offscreen");
			if(killOffscreen)
			Destroy(main);
		}
	}
	void spawnFlippedCorpseImpact(Vector3 pos, Collider2D other, Sprite usedDeathSprite,bool flipXVelo,Vector3 offset)
	{
		float XMult = 1;
		if(flipXVelo)XMult = -1;
		GameObject obj;
		Transform tr;
		if(readPositionFrom==null)
		tr = transform;
		else tr = readPositionFrom;
		obj = Instantiate(flipped,pos+offset,Quaternion.Euler(0,0,tr.eulerAngles.z));
		SpriteRenderer orgRender;
		SpriteRenderer newRender = obj.GetComponent<SpriteRenderer>();
		if(debugPrintPosition)
			print(transform.name+": "+"Flipped pos = "+(pos)+", Original pos = "+transform.position+" Offset: "+offset);
		if(flippedStartUpright)
		newRender.flipY = false;
		if(transform.childCount != 0&&transform.GetChild(0).GetComponent<SpriteRenderer>()!=null)
			orgRender = transform.GetChild(0).GetComponent<SpriteRenderer>();
		else orgRender = GetComponent<SpriteRenderer>();

		if(transform.name!="Enemy_Corpse")
		{
			if(!useDeadSpriteForFlip)
			{
				newRender.sprite = orgRender.sprite;
			}
			else
			{
				newRender.sprite = usedDeathSprite;
			}
				newRender.color = orgRender.color;
			}
			else if(transform.name=="Enemy_Corpse")
			{
				newRender.sprite = orgRender.sprite;
				newRender.color = orgRender.color;
			}
		if(orgRender!=null)
		{
			obj.transform.localScale = orgRender.transform.localScale;
		}

		if(other.transform.position.x > readPositionFrom.position.x)
		{
			flipVeloUsed = new Vector2(-flipVelo.x,flipVelo.y);
		}
		Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
		obj.GetComponent<deadEnemyScript>().invertable = invertable;
		obj.SetActive(true);
		if(other.tag =="Fireball"||other.tag=="Inv"||other.tag=="lKnife")
		flipVeloUsed = new Vector2(flipVeloUsed.x,flipVeloUsed.y/1.5f);
		if(other.transform.position.x > readPositionFrom.position.x)
			rb.angularVelocity = angVelocity*XMult;
		else rb.angularVelocity = -angVelocity*XMult;

		rb.velocity = new Vector2(flipVeloUsed.x*XMult,flipVeloUsed.y);
		//print(rb.velocity+" "+XMult);
	}
	void spawnImpactKick(Transform other)
	{
		//float inv = transform.position.x>=other.position.x ? 1 :-1;
		float inv = (transform.eulerAngles.z>=180||transform.localScale.y==-1)?-1:1;
		data.spawnImpact(transform.position+new Vector3(0,0.5f*inv,0));
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		//if(dataShare.debug)
		//print(other.name);
		
		if(!spawnedCorpse&&((eneOff==null||eneOff.visible)&&(transform.childCount==0||transform.GetChild(0).gameObject.activeInHierarchy)))
		if(other.name.Contains("BlockParent")
			&& blockHitKills
			&& !turnIntoDisc
		||other.tag =="Disc"
			&& discKills
			&& other.transform.parent.GetComponent<shellScript>()!=null&&other.transform.parent.GetComponent<shellScript>().moving
		||other.tag =="blockHoldable"
			&& thrownBlockKills
			&& canDie
			&& Mathf.Abs(other.transform.parent.GetComponent<Rigidbody2D>().velocity.x)>=2f
		||other.name =="ScreenNuke"
			&& screenNukeKills
		||other.tag =="Fireball"
			&& FireballKills
			&& canDie
		||other.tag =="lKnife"
			&& lKnifeKills
		||other.tag =="Inv"&&other.name=="HalvaOverlay"
			&& invincibilityKills
			&& canDie
		||other.tag =="Inv"&&other.name=="axeAura"
			&& axeKills
			&& canDie)
		{
			if(other.name=="axeAura"&&turnIntoDisc)
			{
				shellScript shell = GetComponent<shellScript>();
				//print(shell.stompAllowWait);
				if(shell.stompAllowWait<=0)
				{
					float sub = Mathf.Clamp((other.transform.position.x-transform.position.x)*100,-1,1);
					if(sub==0)sub=1;
					//print("Sub: "+sub);
					if(shell.enabled&&!shell.moving)
					{
						data.playSound(119,transform.position);
						shell.discLaunch(other.transform.position.x);
						shell.spawnImpactKick();
					}
					else if(shell.moving)
					{
						data.playSound(119,transform.position);
						shell.changeDirection(-(int)sub,true);
					}
					else
					{
						shell.resetCor();
						shell.spawnImpactKick();
					}
					GetComponent<MovementAI>().jump(shell.inverted);
					spawnCorpse();
					shell.stompAllowWait = 30;
				}
				return;
			}
			if(invertable)
			{
				if(transform.eulerAngles.z>90)
				{
					flipVelo = new Vector2(flipVelo.x,-Mathf.Abs(flipVelo.y));
					inverter = -1;
				}
				else
				{
					flipVelo = new Vector2(flipVelo.x,Mathf.Abs(flipVelo.y));
					inverter = 1;
				}
			}
			//Debug.Log("Corpse inverted: "+inverter+" Flip velo: "+flipVelo);
			//Debug.Log(other.name);
			spawnedCorpse = true;
			flipVeloUsed = flipVelo;
			if(transform.name!="Enemy_Corpse")
			{
				DeathEventTriggered();
				if(transform.name.Contains("fugo_Enemy")&&other.tag=="Fireball"&&other.name=="sprite")
				{
					GetComponent<fugo_AI>().compareFireball(other.transform.parent.gameObject);
				}
				for(int i = 0; i<objectsToUnParentOnDeath.Length;i++)
				{
					if(objectsToUnParentOnDeath[i]!=null)
					objectsToUnParentOnDeath[i].transform.parent = transform.parent;
				}
				float streak;
				switch(other.tag)
				{
					default: data.playeneSound(pickDeathSound(),1f); break;
					case "Inv":
					streak = data.halvaStreak;
					data.playeneSound(pickDeathSound(),1f+(streak/20));
					break;
					case "Disc":
					streak = other.transform.parent.GetComponent<shellScript>().discKillStreak;
					data.playeneSound(pickDeathSound(),1f+(streak/20));
					break;
					case "blockHoldable":
						streak = other.transform.parent.GetComponent<holdableBlockScript>().streak;
						data.playeneSound(pickDeathSound(),1f+(streak/20));
					break;
				}
			}
			Vector3 pos;
			if(other.name == "HalvaOverlay")
			{
				if(buildsCombo)
				{
					if(pointValue==200||data.halvaStreak>7)
					{
						data.halvaStreak++;
						data.streakScore(data.halvaStreak,readPositionFrom.position+pointDisplayOffset,true);
					}
					else
					{
						data.halvaStreak++;
						data.addScore(pointValue);
						data.ScorePopUp(readPositionFrom.position+pointDisplayOffset,"+"+pointValue,new Color32(255,255,255,255));
					}
				}
				else
				{
					data.playComboSound(0);
					data.addScore(pointValue);
					data.ScorePopUp(readPositionFrom.position+pointDisplayOffset,"+"+pointValue,new Color32(255,255,255,255));
				}
			}
			if(other.tag == "Disc")
			{
				shellScript shellSc = other.transform.parent.GetComponent<shellScript>();
				int str = shellSc.discKillStreak;
				//print(str);
				if(pointValue==200||str>7)
				{
					shellSc.discKillStreak++;
					str = shellSc.discKillStreak;
					spawnImpactKick(other.transform);
					data.streakScore(str,readPositionFrom.position+pointDisplayOffset,true);
				}
				else
				{
					shellSc.discKillStreak++;
					data.addScore(pointValue);
					data.ScorePopUp(readPositionFrom.position+pointDisplayOffset,"+"+pointValue,new Color32(255,255,255,255));
				}
				//data.streakScore(str,readPositionFrom.position+pointDisplayOffset,true);
			}
			if(other.tag == "blockHoldable")
			{
				holdableBlockScript holdBlock = other.transform.parent.GetComponent<holdableBlockScript>();
				if(transform.name!="Enemy_Corpse")
				{
					int str = holdBlock.streak;
					if(pointValue==200||str>7)
					{
						holdBlock.streak++;
						str = holdBlock.streak;
						spawnImpactKick(other.transform);
						data.streakScore(str,readPositionFrom.position+pointDisplayOffset,true);
					}
					else
					{
						holdBlock.streak++;
						data.addScore(pointValue);
						data.ScorePopUp(readPositionFrom.position+pointDisplayOffset,"+"+pointValue,new Color32(255,255,255,255));
					}
				}
				else
				{
					data.playComboSound(0);
				}
				if(bustBlock)
				{
					holdBlock.HoldBlockBust();
				}
			}

			if(other.name.Contains("BlockParent")||other.tag =="Fireball"||other.name=="axeAura"||other.name=="ScreenNuke"||other.tag=="lKnife")
			{
				if(other.name=="axeAura"||other.name.Contains("knife_projectile")||other.transform.parent!=null&&other.transform.parent.name.Contains("knife_projectile"))
				{
					data.playSoundOverWrite(24,transform.position);
				}
				if(transform.name!="Enemy_Corpse")
				{
					data.addScore(pointValue);
					data.ScorePopUp(readPositionFrom.position+pointDisplayOffset,"+"+pointValue,new Color32(255,255,255,255));
				}
				data.playSoundOverWrite(20,transform.position);
			}
			if(flippedStartUpright)
			pos = readPositionFrom.position+corpseSpawnOffset;

			else if(transform.name!="Enemy_Corpse")
				pos = new Vector3(readPositionFrom.position.x+corpseSpawnOffset.x,readPositionFrom.position.y+(flipOffset.x*inverter)+corpseSpawnOffset.y,readPositionFrom.position.z);
			else pos = new Vector3(readPositionFrom.position.x+corpseSpawnOffset.x,readPositionFrom.position.y+(flipOffset.y*inverter)+corpseSpawnOffset.y,readPositionFrom.position.z);
			
			if(other.name == "HalvaOverlay")
			{
				data.spawnImpact(pos);
			}
			spawnFlippedCorpseImpact(pos,other,deadSprite,false,Vector3.zero);
			if(deadSprite2!=null)
			spawnFlippedCorpseImpact(pos,other,deadSprite2,true,corpse2SpawnOffset);
			
			if(destroyEnemy)
			Destroy(main);
			else
			{
				StartCoroutine(corpseSpawnCoolDown());
			}
		}
		if(other.name.Contains("BlockParent") && !spawnedCorpse && turnIntoDisc)
		{
			spawnedCorpse = true;
			/*flipVeloUsed = flipVelo;
			if(other.transform.position.x > readPositionFrom.position.x)
			flipVeloUsed = new Vector2(-flipVelo.x,flipVelo.y);
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			rb.velocity = flipVeloUsed;*/
			shellScript shell = GetComponent<shellScript>();
			eneOff.canEnableAI = false;
			GetComponent<MovementAI>().enabled = false;
			if(!shell.enabled)
			{
				shell.enabled = true;
				shell.StartCoroutine(shell.grabWaitCor());
			}
			else shell.resetCor();
			transform.GetChild(0).gameObject.tag = "Disc";
			transform.GetChild(0).gameObject.layer = 17;
			StartCoroutine(corpseSpawnCoolDown());
		}
	}
	IEnumerator corpseSpawnCoolDown()
	{
		hitFlag = true;
		yield return 0;
		spawnedCorpse = false;
		if(disableOnCorpseSpawn)
		main.SetActive(false);
	}
	#endregion
	//my event
     [Serializable]
     public class DeathEvent : UnityEvent { }
 
     [SerializeField]
     private DeathEvent deathEvent = new DeathEvent();
     public DeathEvent onDeathEvent { get { return deathEvent; } set { deathEvent = value; } }
 
     public void DeathEventTriggered()
     {
         onDeathEvent.Invoke();
     }
}
