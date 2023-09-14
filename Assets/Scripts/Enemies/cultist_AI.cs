using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class cultist_AI : MonoBehaviour {
	Transform cultist;
	Transform cloud;
	Transform particles;
	Transform player;
	[Header ("ONLY EDIT THIS")]
	[Space]
	public Vector2 travelDistance = new Vector2(0, 50);
	public Vector2 verticalTravelDistance = new Vector2(0,0);
	[Space]
	[Header ("Other")]
	[Space]
	public float frontOfPlayer = 9f;
	public Vector2 randomCheck = new Vector2(-3f,4f);
	public float damping = 0.3f;
	public float dampingCatchUp = 0.3f;
	private Vector3 velocity = Vector3.zero;
	public Vector3 travelPoint;
	Transform cam;
	bool startOnce = false;
	public bool visible = false;
	bool spawn = true;
	bool catchUp = false;
	bool catchUpLeft = false;
	bool catchUpRight = false;
	bool leave = false;
	int framesToReachPoint = 0;
	public int waitFrames = 270;
	List <GameObject> balls;
	List <GameObject> spikeys;
	public int amount = 5;
	public GameObject ball;
	public GameObject spikeEnemy;
	public GameObject cloudDisintegrate;
	public Sprite[] cultistSprites = new Sprite[2];
	SpriteRenderer render;
	int animFrames = 0;
	bool canSpawn = true;
	GameData data;
	public int spawnFramesWait = 170;
	int spawnFrames = 0;
	bool returning = false;
	Vector3 startPoint;
	EnemyCorpseSpawner corpseSpawn;
	public bool spawnsSpikeBugs = true;
	Animator anim;
	public AudioClip spawnSound;
	Coroutine shootCor;
	void DestroyCultist()
	{
		if(Application.isPlaying)
		{
			if(cloudDisintegrate!=null)
			Instantiate(cloudDisintegrate,cloud.position,Quaternion.identity);
			Destroy(gameObject);
		}
	}
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nTravel dist: "+travelDistance+"\nVery travel dist: "+verticalTravelDistance);
	}
	// Use this for initialization
	void Start () {
		if(Application.isPlaying&&!startOnce)
		{
			if(dataShare.debug)
			printer();
			spawnFrames = spawnFramesWait;
			corpseSpawn = GetComponent<EnemyCorpseSpawner>();
			startOnce = true;
			player = GameObject.Find("Player_main").transform;
			data = GameObject.Find("_GM").GetComponent<GameData>();
			cultist = transform.GetChild(0);
			if(transform.childCount>1)
			{
				cloud = transform.GetChild(1);
				particles = transform.GetChild(2);
				particles.parent=null;
			}
			cam = GameObject.Find("Main Camera").transform;
			render = transform.GetChild(0).GetComponent<SpriteRenderer>();
			anim = GetComponent<Animator>();
			travelDistance = new Vector2(travelDistance.x+Mathf.RoundToInt(transform.position.x),travelDistance.y+Mathf.RoundToInt(transform.position.x));
			if(verticalTravelDistance!=Vector2.zero)
			verticalTravelDistance  = new Vector2(verticalTravelDistance.x+Mathf.RoundToInt(transform.position.y),verticalTravelDistance.y+Mathf.RoundToInt(transform.position.y));
			startPoint = transform.position;
			if(!visible)
			toggle(false);
			if(spawnsSpikeBugs)
			balls = new List<GameObject>();

			spikeys = new List<GameObject>();

			for(int i = 0; i<amount;i++)
			{
				if(spawnsSpikeBugs)
				{
					GameObject obj = Instantiate(ball,transform.position,Quaternion.identity);
					obj.SetActive(false);
					balls.Add(obj);
					obj.GetComponent<spikeyBallScript>().cultist = this;
					obj.transform.SetParent(transform.parent);
					obj = Instantiate(spikeEnemy,transform.position,Quaternion.identity);
					obj.SetActive(false);
					obj.transform.SetParent(transform.parent);
					spikeys.Add(obj);
				}
				else
				{
					GameObject obj = Instantiate(ball,transform.position,Quaternion.identity);
					obj.SetActive(false);
					spikeys.Add(obj);
				}
			}
		}
	}
	void Enable()
	{
		if(cam==null&&!startOnce)
		Start();
		if(Application.isPlaying&&visible)
		{
			if(spawnSound!=null)data.playUnlistedSound(spawnSound);
			if(verticalTravelDistance==Vector2.zero)
			 travelPoint = new Vector2(Mathf.Clamp(cam.position.x,travelDistance.x,travelDistance.y),startPoint.y);
		else travelPoint = new Vector2(Mathf.Clamp(cam.position.x,travelDistance.x,travelDistance.y),Mathf.Clamp(cam.position.y+6f,verticalTravelDistance.x,verticalTravelDistance.y));
			Debug.DrawLine(new Vector3(travelDistance.x,startPoint.y,transform.position.z),new Vector3(travelDistance.y,transform.position.y,transform.position.z),Color.blue,5f);
			pickNewLocation();
		}
	}
	void OnDisable()
	{
		spawn = true;
	}
	IEnumerator returnToStart()
	{
		returning = true;
		yield return new WaitUntil(()=> !visible);
		velocity = Vector3.zero;
		transform.position = startPoint;
		toggle(false);
		leave = false;
		spawn = true;
		if(verticalTravelDistance==Vector2.zero)
			 travelPoint = new Vector2(Mathf.Clamp(cam.position.x,travelDistance.x,travelDistance.y),startPoint.y);
		else travelPoint = new Vector2(Mathf.Clamp(cam.position.x,travelDistance.x,travelDistance.y),Mathf.Clamp(cam.position.y+6f,verticalTravelDistance.x,verticalTravelDistance.y));
		yield return new WaitForSeconds(0.1f);
		returning = false;

	}
	IEnumerator shootWait()
	{
		bool clear = false;
		while(!clear)
		{
			if(Time.timeScale!=0)
			{
				float value = transform.position.x;
				if(Mathf.Floor(transform.position.x)%2==1)
					value = transform.position.x+1;

				if(Mathf.Round(value)%2==0)
					clear = true;
			}
			yield return 0;
		}
		//print(Mathf.Round(Mathf.Floor(transform.position.x)/2));
		anim.SetTrigger("shoot");
		shootCor = null;
	}
	// Update is called once per frame
	void Update () {
		if(Application.isPlaying&&visible)
		{
			if(corpseSpawn.hitFlag)
			{
				corpseSpawn.hitFlag = false;
				DestroyCultist();
			}
			if(transform.position.x>=player.position.x&&transform.localScale.x!=1)
			{
				transform.localScale = new Vector3(1,1,0);
			}
			else if(cultist.position.x<player.position.x&&transform.localScale.x!=-1)
			{
				transform.localScale = new Vector3(-1,1,0);
			}

			if(framesToReachPoint!=0)
			{
				framesToReachPoint--;
				if(framesToReachPoint==0)
				pickNewLocation();
			}
			if(animFrames!=0&&spawnsSpikeBugs)
			{
				animFrames--;
				if(animFrames==0)
				render.sprite = cultistSprites[1];
			}
			if(spawnFrames!=0&&!leave&&Time.timeScale!=0)
			{
				spawnFrames--;
				if(spawnFrames==0)
				{
					if(canSpawn)
					{
						if(spawnsSpikeBugs)
						spawnBall();
						else
						{
							if(shootCor!=null)StopCoroutine(shootCor);
							shootCor = StartCoroutine(shootWait());
						}
					}
					spawnFrames = spawnFramesWait;
				}
			}
			if(Mathf.Round(transform.position.x)==Mathf.Round(travelPoint.x)&&!catchUpLeft&&!catchUpRight)
			{
				if(spawn)
				spawn = false;
				pickNewLocation();
			}

			if(catchUpLeft&&!leave)
			{
				framesToReachPoint = waitFrames;
				if(Mathf.Round(transform.position.x)!=Mathf.Round(travelPoint.x))
				if(verticalTravelDistance==Vector2.zero)
					 travelPoint = new Vector2(Mathf.Clamp(cam.position.x+10f,travelDistance.x,travelDistance.y),startPoint.y);
				else travelPoint = new Vector2(Mathf.Clamp(cam.position.x+10f,travelDistance.x,travelDistance.y),Mathf.Clamp(cam.position.y+6f,verticalTravelDistance.x,verticalTravelDistance.y));
				else {catchUpLeft = false;pickNewLocation();}
			}
			if(catchUpRight&&!leave)
			{
				framesToReachPoint = waitFrames;
				if(Mathf.Round(transform.position.x)!=Mathf.Round(travelPoint.x))
				if(verticalTravelDistance==Vector2.zero)
					 travelPoint = new Vector2(Mathf.Clamp(cam.position.x-10f,travelDistance.x,travelDistance.y),startPoint.y);
				else travelPoint = new Vector2(Mathf.Clamp(cam.position.x-10f,travelDistance.x,travelDistance.y),Mathf.Clamp(cam.position.y+6f,verticalTravelDistance.x,verticalTravelDistance.y));
				else {catchUpRight = false;pickNewLocation();}
			}

			if(verticalTravelDistance==Vector2.zero)
			{
				//fly away left
				if(Mathf.RoundToInt(transform.position.x)==Mathf.RoundToInt(travelDistance.x)&&!leave)
				{
					catchUp = false;
					leave = true;
					travelPoint = new Vector2(transform.position.x-5f,startPoint.y+35f);
					StartCoroutine(returnToStart());
				}
				//fly away right
				if(Mathf.RoundToInt(transform.position.x)==Mathf.RoundToInt(travelDistance.y)&&!leave)
				{
					catchUp = false;
					leave = true;
					travelPoint = new Vector2(transform.position.x+15f,startPoint.y+35f);
					StartCoroutine(returnToStart());
				}
			}
			if(verticalTravelDistance!=Vector2.zero)
			{
				//fly away up
				if(Mathf.RoundToInt(transform.position.y)==Mathf.RoundToInt(verticalTravelDistance.y)&&!leave)
				{
					catchUp = false;
					leave = true;
					travelPoint = new Vector2(transform.position.x-5f,transform.position.y+35f);
					StartCoroutine(returnToStart());
				}
				//fly away down
				if(Mathf.RoundToInt(transform.position.y)==Mathf.RoundToInt(verticalTravelDistance.x)&&!leave)
				{
					catchUp = false;
					leave = true;
					travelPoint = new Vector2(transform.position.x+15f,transform.position.y+35f);
					StartCoroutine(returnToStart());
				}
			}
		}
		if(!Application.isPlaying)
		{
			Debug.DrawLine(new Vector3(transform.position.x+travelDistance.x,transform.position.y,transform.position.z),new Vector3(transform.position.x+travelDistance.y,transform.position.y,transform.position.z),Color.red);
			Debug.DrawLine(new Vector3(transform.position.x,transform.position.y+verticalTravelDistance.x,transform.position.z),new Vector3(transform.position.x,transform.position.y+verticalTravelDistance.y,transform.position.z),Color.green);
		}
	}
	void FixedUpdate()
	{
		if(visible)
		{
			if(verticalTravelDistance!=Vector2.zero&&!leave)
			{
				transform.position = Vector3.MoveTowards(transform.position,new Vector3(transform.position.x,cam.position.y+6f,transform.position.z),damping*Time.deltaTime*3.5f);
				//else transform.position = Vector3.MoveTowards(transform.position,new Vector3(transform.position.x,cam.position.y+6f,transform.position.z),damping*Time.deltaTime*1.5f);
			}
			//Movement AI
			if(catchUp)
			transform.position = Vector3.SmoothDamp(transform.position, travelPoint, ref velocity, dampingCatchUp);
			else transform.position = Vector3.SmoothDamp(transform.position, travelPoint, ref velocity, damping);
			if(transform.position.x <= cam.position.x-11f&&!leave)
			{
				//Debug.Log("almost gone offscreen, trying to catch up");
				framesToReachPoint = waitFrames;
				catchUp = true;
				catchUpLeft = true;
			}
			else if(transform.position.x>=cam.position.x+11f&&!leave&&!spawn)
			{
				//Debug.Log("almost gone offscreen, trying to catch up");
				catchUp = true;
				catchUpRight = true;
			}
		}
	}
	void pickNewLocation()
	{
		if(!leave)
		{
		catchUp = false;
		framesToReachPoint = waitFrames;
		//try to position yourself in front of him if player is facing left
		//Debug.Log(player.position.x);
		float previousPoint = travelPoint.x;
		if(verticalTravelDistance==Vector2.zero)
			 travelPoint = new Vector2(Mathf.Clamp(player.position.x+(frontOfPlayer*player.localScale.x),travelDistance.x,travelDistance.y),startPoint.y);
		else travelPoint = new Vector2(Mathf.Clamp(player.position.x+(frontOfPlayer*player.localScale.x),travelDistance.x,travelDistance.y),Mathf.Clamp(cam.position.y+6f,verticalTravelDistance.x,verticalTravelDistance.y));
		if(Mathf.Round(previousPoint)==Mathf.Round(travelPoint.x))
		{
			//Debug.Log("Points are the same, adding a random");
			float rand = Random.Range(randomCheck.x,randomCheck.y)*2;
			if(verticalTravelDistance==Vector2.zero)
				 travelPoint = new Vector2(Mathf.Clamp(player.position.x+(frontOfPlayer*player.localScale.x)+rand,travelDistance.x,travelDistance.y),startPoint.y);
			else travelPoint = new Vector2(Mathf.Clamp(player.position.x+(frontOfPlayer*player.localScale.x)+rand,travelDistance.x,travelDistance.y),Mathf.Clamp(cam.position.y+6f,verticalTravelDistance.x,verticalTravelDistance.y));
		}
		Debug.DrawLine(new Vector3(travelDistance.x,transform.position.y,transform.position.z),new Vector3(travelDistance.y,transform.position.y,transform.position.z),Color.blue,3f);
		Debug.DrawLine(new Vector3(transform.position.x,transform.position.y,transform.position.z),new Vector3(travelPoint.x,transform.position.y,transform.position.z),Color.red,3f);
		}
	}
	public void spawnSpikey(Vector3 pos,int i,float dir)
	{
		data.StartCoroutine(spikeySpawn(pos,i,dir));
	}
	IEnumerator spikeySpawn(Vector3 pos,int i,float dir)
	{
		if(i<spikeys.Count)
			{
			//Debug.Log(i);
			particles.GetComponent<ParticleSystem>().Stop(true,ParticleSystemStopBehavior.StopEmitting);
			objectFollower objF = particles.GetComponent<objectFollower>();
			objF.follow =false;
			objF.obj =null;
			if(spikeys[i]==null&&this!=null)
			{
				GameObject obj = Instantiate(spikeEnemy,transform.position,Quaternion.identity);
				obj.SetActive(false);
				obj.transform.SetParent(transform.parent);
				spikeys[i]=obj;
			}
			spikeys[i].GetComponent<MovementAI>().constantCheckDir = true;
			spikeys[i].transform.position = pos;
			spikeys[i].transform.localScale = new Vector3(dir,1,1);
			spikeys[i].SetActive(true);
			yield return 0;
			balls[i].SetActive(false);
			}
	}
	void spawnBall()
	{
		for(int i = 0; i<spikeys.Count;i++)
		{
			if(spikeys[i]==null)
			{
				GameObject obj = Instantiate(spikeEnemy,transform.position,Quaternion.identity);
				obj.SetActive(false);
				obj.transform.SetParent(transform.parent);
				spikeys[i]=obj;
			}
			if(balls[i]==null)
			{
				GameObject obj = Instantiate(ball,transform.position,Quaternion.identity);
				obj.SetActive(false);
				obj.GetComponent<spikeyBallScript>().cultist = this;
				obj.transform.SetParent(transform.parent);
				balls[i]=obj;
			}
			if(!spikeys[i].activeInHierarchy||spikeys[i].GetComponent<EnemyOffScreenDisabler>().visible==false)
			{
				render.sprite = cultistSprites[0];
				animFrames = 20;
				data.playSound(19,transform.position);
				balls[i].transform.position = transform.position;
				balls[i].GetComponent<spikeyBallScript>().spawned = false;
				balls[i].GetComponent<spikeyBallScript>().ID = i;
				balls[i].GetComponent<spikeyBallScript>().direction = -transform.localScale.x;
				objectFollower objF = particles.GetComponent<objectFollower>();
				objF.obj = balls[i].transform;
				objF.follow = true;

				balls[i].SetActive(true);
				balls[i].GetComponent<Rigidbody2D>().velocity = new Vector2(0,9f);
				balls[i].GetComponent<Rigidbody2D>().angularVelocity = 800f;
				particles.GetComponent<ParticleSystem>().Play(true);
				break;
			}
		}
	}
	public void spawnLaser()
	{
		if(ball!=null)
		{
			for(int i = 0; i<spikeys.Count;i++)
			{
				if(!spikeys[i].gameObject.activeInHierarchy)
				{
					spikeys[i].transform.position = transform.position+new Vector3(0,-0.5f,0);
					spikeys[i].transform.eulerAngles = new Vector3(0,0,90);
					data.playSound(88,transform.position);
					spikeys[i].GetComponent<bulletScript>().Enable(true);
					break;
				}
			}
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&!returning)
		{
			visible = true;
			toggle(true);
		}
		if(other.tag == "Ground"||other.tag=="Harm")
		{
			canSpawn = false;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator" && transform.parent != null && transform.parent.name != "Player_main")
		{
			visible = false;
			toggle(false);
		}
		if(other.tag == "Ground"||other.tag=="Harm")
		{
			canSpawn = true;
		}
	}
	public void toggle(bool active)
	{
		if(!active)
		{
			Rigidbody2D rb = GetComponent<Rigidbody2D>();
			rb.velocity = new Vector2(0,rb.velocity.y);
		}
		if(active)
		{
			Enable();
		}
		if(cloud!=null)
		cloud.gameObject.SetActive(active);
		cultist.gameObject.SetActive(active);
		//Debug.Log(active+" "+cloud.gameObject.activeInHierarchy);
	}
}
