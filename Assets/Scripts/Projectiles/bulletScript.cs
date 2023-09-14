using System.Collections;
using UnityEngine;

public class bulletScript : MonoBehaviour {
	public float speed = 1f;
	public float destroyTimer = 4f;
	float usedSpeed;
	public bool lookAtPlayer = false,spriteAlwaysZeroRotation = false;
	Rigidbody2D rb;
	public GameObject bugEnemy;
	bool move = false;
	public Sprite impact;
	Sprite orgSprite;
	SimpleAnim2 spa;

	checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	public bool ignoreSemiSolid = false;
	public LayerMask whatIsGround;
	SpriteRenderer render;
	Collider2D col;
	GameObject obj;
	EnemyOffScreenDisabler enec;
	public bool workOffscreen = true,turnOffOffscreen = false,playAnimOnContact = false,playerImpact = false;
	public bool destroyOffscreen = false;
	public bool breakBrickOnImpact = false;
	bool visible = false;
	bool setdirection = false;
	bool canTurnInVisible = true;
	public AudioClip[] clips = new AudioClip[2];
	Coroutine dest;
	public Transform parent;
	ParticleSystem pSystem;
	bool midImpact = false;
	public bool resetTagOnCollision = false;
	string TrTag;
	GameData data;
	public AudioClip awakeSound;
	public int direction = 1;
	public bool disableAnimOnStart = true;
	public bool alwaysIgnoreSemiSolid = false;
	// Use this for initialization
	void Start () {
		if(transform.childCount!=0) TrTag = transform.GetChild(0).tag;
		rb = GetComponent<Rigidbody2D>();
		spa = transform.GetChild(0).GetComponent<SimpleAnim2>();
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		col = GetComponent<Collider2D>();
		orgSprite = render.sprite;
		if(GetComponent<EnemyOffScreenDisabler>()!=null)
		enec = GetComponent<EnemyOffScreenDisabler>();
		if(transform.childCount>1)
		pSystem = transform.GetChild(1).GetComponent<ParticleSystem>();
		data = GameObject.Find("_GM").GetComponent<GameData>();

		if(transform.childCount>1&&pSystem==null)
		checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
		semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
		if(alwaysIgnoreSemiSolid)
		{
			ignoreSemiSolid = true;
			Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
			whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
		}
		if(workOffscreen)
			Enable(false);
	}
	
	// Update is called once per frame
	void Update () {
		if(move && Time.timeScale!=0 && enec !=null && enec.visible || move && Time.timeScale!=0)
		{
			Vector3 dir = -transform.right * usedSpeed;
			rb.velocity = new Vector2(dir.x,dir.y);
			if(transform.position.z>=-1)
			{
				transform.position -= new Vector3(0,0,0.025f);
				if(transform.position.z<=-1)
				{
					transform.position = new Vector3(transform.position.x,transform.position.y,-1f);
					render.sortingLayerName = "Player";
					render.sortingOrder = 6;
				}
			}
		}
		else if(!move || Time.timeScale==0 ||enec !=null && !enec.visible)
		{
			if(rb.velocity!=Vector2.zero)
				rb.velocity = Vector2.zero;
		}

		if(checker!=null)
		{
		if(rb.velocity.y>0&&!ignoreSemiSolid||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
			{
				ignoreSemiSolid = true;
				Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
				whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
				//Debug.Log(gameObject.name+" Ignoring collision");
			}
		else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy&&ignoreSemiSolid)
			{
				ignoreSemiSolid = false;
				Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
				whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
				//Debug.Log(gameObject.name+" Not ignoring collision");
			}
		}
	}
	public void disableParticles()
	{
		if(pSystem!=null)pSystem.Stop(false,ParticleSystemStopBehavior.StopEmitting);
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		//print(other.name);
		if(other.name == "ObjectActivator"&&!visible||other.name == "ObjectActivator"&& !canTurnInVisible)
		{
			Enable(false);
			visible = true;
		}
		if(other.name=="InstantDeath"&&gameObject.activeInHierarchy)
		{
			StartCoroutine(spawnBug(0f));
		}
		if(other.name == "PlayerCollider"&&playerImpact&&!midImpact)
		{
			midImpact = true;
			if(bugEnemy!=null)
			canTurnInVisible = false;
			if(enec!=null)
			enec.enabled = false;
			move = false;
			if(spa!=null&&!playAnimOnContact) spa.enabled = false;
			if(col!=null)
			col.enabled = false;
			if(playAnimOnContact)
			{
				int rando = Random.Range(0,clips.Length);
				data.playUnlistedSound(clips[rando]);
				spa.StartPlaying();
			}
			disableParticles();
		}
	}
	public void suicide()
	{
		if(!midImpact)
		{
			midImpact = true;
			if(resetTagOnCollision)render.transform.tag = "Untagged";
			if(bugEnemy!=null)
			canTurnInVisible = false;
			if(enec!=null)
			enec.enabled = false;
			move = false;
			if(spa!=null&&!playAnimOnContact) spa.enabled = false;
			if(col!=null)
			col.enabled = false;
			if(playAnimOnContact)
			{
				int rando = Random.Range(0,clips.Length);
				data.playUnlistedSound(clips[rando]);
				spa.StartPlaying();
			}
			disableParticles();
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			//print("offscreen");
			if(!workOffscreen)
				move = false;
			if(visible && canTurnInVisible)
			visible = false;
			if(turnOffOffscreen)gameObject.SetActive(false);
		}
	}
	void OnDisable()
	{
		if(playAnimOnContact)
		{
			spa.isPlaying=false;
			render.sprite = orgSprite;
		}
		if(spriteAlwaysZeroRotation)
		{
			transform.localScale = Vector3.one;
			transform.eulerAngles = Vector3.zero;
			transform.localPosition = Vector3.zero;
			render.transform.localEulerAngles = Vector3.zero;
		}
		midImpact = false;
		if(resetTagOnCollision)render.transform.tag = TrTag;
		if(parent!=null&&gameObject.activeInHierarchy)
		transform.SetParent(parent);
	}
	IEnumerator waitforDestroy(float timeTilDestroy)
	{
		yield return new WaitForSeconds(timeTilDestroy);
		yield return new WaitUntil(()=>!visible);
		Destroy(gameObject);
	}
	public void Enable(bool withObject)
	{
		//Debug.Log(gameObject.name +" activated.");
		if(semiSolid==null)
		{
			Start();
		}
		if(parent!=null)
		transform.SetParent(null);
		if(col!=null)
			col.enabled = true;
		if(spa!=null)
		{
			spa.isPlaying=!disableAnimOnStart;
			spa.enabled = true;
		}
		move = true;
		if(playAnimOnContact)render.sprite = orgSprite;
		if(destroyOffscreen)
		{
			if(dest!=null)
				StopCoroutine(dest);
			dest = StartCoroutine(waitforDestroy(destroyTimer));
		}
		if(awakeSound!=null)
		{
			data.playUnlistedSound(awakeSound);
		}
		if(lookAtPlayer)
		{
			Vector3 difference = GameObject.Find("Player_main").transform.position - transform.position;
 			float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
 			transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ+180f);
		}
		if(lookAtPlayer||!setdirection)
		{
			setdirection = true;
			if(transform.eulerAngles.z > 90 && transform.eulerAngles.z <= 270)
			{
				//print("-1");
				transform.localScale = new Vector3(-1,1,1);
				transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z+180f);
			}
			else
			{
				//print("1");
				transform.localScale = new Vector3(direction,1,1);
			}
		}
		if(spriteAlwaysZeroRotation)
		{
			render.transform.rotation = Quaternion.Euler(Vector3.zero);
		}
		usedSpeed = speed*transform.localScale.x;
		if(checker!=null)
		{
			checker.transform.position = new Vector3(transform.position.x,transform.position.y-0.08f,transform.position.z);
			checker.transform.localEulerAngles =  new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z*(-transform.localScale.x));
		}
		if(withObject)gameObject.SetActive(true);
		if(pSystem!=null)
		{
			pSystem.Play(false);
		}
	}
	void OnCollisionEnter2D(Collision2D other)
	{
		//print(other.gameObject.name);
		if(other.gameObject.tag == "Ground"&&!midImpact||other.gameObject.tag == "semiSolid"&&!ignoreSemiSolid&&!midImpact)
		{
			//print("a");
			midImpact = true;
			if(resetTagOnCollision)render.transform.tag = "Untagged";
			if(bugEnemy!=null)
			canTurnInVisible = false;
			if(enec!=null)
			enec.enabled = false;
			move = false;
			if(spa!=null&&!playAnimOnContact) spa.enabled = false;
			if(col!=null)
				col.enabled = false;
			if(breakBrickOnImpact)
			{
				//float value = transform.position.x;
				data.explodeTile(transform.position-Vector3.up,true);
				/*bool shot = false;
				if(Mathf.Floor(value)%2==1)
				{
					value = value+1;
					shot = true;
					print("odd");
					//data.explodeTile(transform.position-Vector3.up-(Vector3.right),true);
				}
				if(Mathf.Round(value)==Mathf.Floor(value)||Mathf.Round(value)==Mathf.Ceil(value))
				{
					data.explodeTile(transform.position-Vector3.up-(Vector3.right*0.5f),true);
					data.explodeTile(transform.position-Vector3.up+(Vector3.right*0.5f),true);
				}
				else
				{
					if(!shot)
					data.explodeTile(transform.position-Vector3.up,true);
				}*/
			}
			Vector3 StartPoint = new Vector3(transform.position.x,transform.position.y,transform.position.z);
			RaycastHit2D ray = Physics2D.Raycast(StartPoint,-Vector3.up,0.3f,whatIsGround);
			Debug.DrawRay(StartPoint,ray.point,Color.blue);
			if(ray.collider!=null&&bugEnemy!=null)
			{
				//print("b");
				transform.eulerAngles = Vector3.zero;
				if(checker!=null)
					checker.transform.eulerAngles = Vector3.zero;
				render.sprite = impact;
				transform.GetChild(0).tag = "Enemy";
				transform.position = new Vector3(transform.position.x,Mathf.RoundToInt(transform.position.y-0.5f),transform.position.z);
				StartCoroutine(spawnBug(1f));
			}
			else
			{
				//print("c");
				StartCoroutine(spawnBug(0f));
			}
			if(playAnimOnContact)
			{
				int rando = Random.Range(0,clips.Length);
				data.playUnlistedSound(clips[rando]);
				spa.StartPlaying();
			}
			disableParticles();
		}
	}
	IEnumerator spawnBug(float time)
	{
		if(visible&&bugEnemy!=null&&!workOffscreen||workOffscreen&&bugEnemy!=null)
		{
			if(visible)
			{
				int rando = Random.Range(0,2);
				data.playUnlistedSound(clips[rando]);
				yield return new WaitForSeconds(time);
				if(time==0)
				obj = Instantiate(bugEnemy,new Vector3(transform.position.x,Mathf.RoundToInt(transform.position.y-0.5f),transform.position.z),Quaternion.identity);
				else obj = Instantiate(bugEnemy,new Vector3(transform.position.x,transform.position.y,transform.position.z),Quaternion.identity);
				//Debug.Log(Mathf.RoundToInt(-transform.localScale.x));
				obj.GetComponent<MovementAI>().changeDirection(Mathf.RoundToInt(-transform.localScale.x));
				yield return new WaitUntil(()=>obj!=null);
				if(transform.parent!=null && obj!=null)
				{
					obj.transform.parent = transform.parent;
				}
			}
			Destroy(gameObject);
		}
	}
}
