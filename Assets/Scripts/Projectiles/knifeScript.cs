using System.Collections;
using UnityEngine;

public class knifeScript : MonoBehaviour {
	public Transform main;
	Rigidbody2D rb;
	public float speed = 18f,vertForceLKnife = 5f;
	Coroutine cor;
	AudioSource asc;
	public bool lknife = false,homing = false;
	PlayerScript pScript;
	public shadowManAI enemyAI;
	Gravity grav;
	public bool isPlayer = true;
	public bool destroyOnDisable = false;
	public bool flipDirection = false;
	[HideInInspector]
	public Vector3 targetPoint;
	public float waitForSpeedReduce = 0.35f;
	public bool workOffscreen = false;
	IEnumerator boomerang()
	{
		bool startRight = true;
		if(rb.velocity.x<0)
		startRight = false;
		yield return new WaitForSeconds(waitForSpeedReduce);
		if(startRight)
		{
			while(rb.velocity.x>-5)
			{
				if(Time.timeScale!=0)
				rb.velocity = Vector2.MoveTowards(rb.velocity,new Vector2(-5,0),0.3f);
				yield return 0;
			}
		}
		else
		{
			while(rb.velocity.x<5)
			{
				if(Time.timeScale!=0)
				rb.velocity = Vector2.MoveTowards(rb.velocity,new Vector2(5,0),0.3f);
				yield return 0;
			}
		}
		Vector3 transformRounded,playerRounded;
		if(main!=null)
		{
		playerRounded = new Vector3(Mathf.Round(main.position.x),Mathf.Round(main.position.y),transform.position.z);
		transformRounded = new Vector3(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y),transform.position.z);
		}
		else
		{
			transformRounded = new Vector3(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y),transform.position.z);
			playerRounded = transformRounded;
		}
		while(transformRounded!=playerRounded&&main!=null)
		{
			if(Time.timeScale!=0)
			{
				transform.position = Vector3.MoveTowards(transform.position,playerRounded,0.35f);
				if(Mathf.Round(main.eulerAngles.z)==0)
				{
					playerRounded = new Vector3(Mathf.Round(main.position.x),Mathf.Round(main.position.y+1f),transform.position.z);
				}
				else playerRounded = new Vector3(Mathf.Round(main.position.x),Mathf.Round(main.position.y-1f),transform.position.z);
				transformRounded = new Vector3(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y),transform.position.z);
			}
			yield return 0;
		}
		if(main==null)
		{
			EnemyCorpseSpawner eCS = GetComponent<EnemyCorpseSpawner>();
			eCS.destroyEnemy = true;
			eCS.disableOnCorpseSpawn = false;
			eCS.spawnCorpse();
		}
		gameObject.SetActive(false);
	}
	IEnumerator homeIn()
	{
		//Vector3 targetPoint = transform.position-(transform.right*8);
		targetPoint = new Vector3(Mathf.Round(targetPoint.x),Mathf.Round(targetPoint.y),Mathf.Round(targetPoint.z));
		//Debug.DrawLine(transform.position,targetPoint,Color.magenta,5f);
		float progress = 0;
		while(progress<0.3f)
		{
			if(Time.timeScale!=0)
			{
				progress=Mathf.Clamp(progress+=Time.deltaTime/5,0,1);
				transform.position = Vector3.Lerp(transform.position,targetPoint,progress);
			}
			yield return 0;
		}
		//yield return new WaitForSeconds(1f);//go at player
		Vector3 playerTarget = main.position;

		Vector3 moveDir = (main.position-transform.position).normalized;
		//Debug.DrawLine(transform.position,moveDir*5,Color.green,5f);
		float maxSpeed = 0;
		while(main!=null)
		{
			if(Time.timeScale!=0)
			{
				maxSpeed=Mathf.Clamp(maxSpeed+=Time.deltaTime*0.8f,0,0.5f);
				transform.position += moveDir * maxSpeed;
			}
			yield return 0;
		}
		if(main==null)
		{
			EnemyCorpseSpawner eCS = GetComponent<EnemyCorpseSpawner>();
			eCS.destroyEnemy = true;
			eCS.disableOnCorpseSpawn = false;
			eCS.spawnCorpse();
		}
		gameObject.SetActive(false);
	}
	// Use this for initialization
	void Start ()
	{
		asc = GetComponent<AudioSource>();
		if(asc==null) asc = transform.GetChild(0).GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody2D>();
		if(isPlayer)
		{
			main = GameObject.Find("Player_main").transform;
			pScript = main.GetComponent<PlayerScript>();
		}
		if(homing)
		{
			main = GameObject.Find("Player_main").transform;
		}
		grav = GetComponent<Gravity>();
	}
	void OnDisable()
	{
		if(cor!=null)
		StopCoroutine(cor);
		rb.velocity = Vector2.zero;
		transform.GetChild(0).localPosition = Vector3.zero;
	}
	/*void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log(other.name);
	}*/
	void Update()
	{
		if(Time.timeScale==0&&asc.isPlaying)
		{
			asc.Pause();
		}
		else if(Time.timeScale!=0&&!asc.isPlaying)
		{
			asc.Play();
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag=="enemyAxeAura")
		{
			EnemyCorpseSpawner eCS = GetComponent<EnemyCorpseSpawner>();
			if(eCS!=null)
			eCS.spawnCorpse();
		}

	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&!workOffscreen)
		{
			if(!destroyOnDisable)
			{
				gameObject.SetActive(false);
				if(lknife)
				{
					grav.pushForces = new Vector2(Mathf.Abs(grav.pushForces.x),-Mathf.Abs(grav.pushForces.y));
					grav.savedPushForces = new Vector2(Mathf.Abs(grav.savedPushForces.x),-Mathf.Abs(grav.savedPushForces.y));
				}
			}
			else Destroy(gameObject);
		}
	}
	void OnEnable()
	{
		if(isPlayer&&main==null||!isPlayer&&rb==null)
		Start();

		if(!lknife)
		{
			if(!homing)
			{
					if(main.localScale.x==1&&isPlayer||main.localScale.x==-1&&!isPlayer)
					rb.velocity=new Vector2(speed,0);
					else rb.velocity=new Vector2(-speed,0);

					if(flipDirection)
					{
						rb.velocity=new Vector2(-rb.velocity.x,rb.velocity.y);
					}

				if(cor!=null)
				StopCoroutine(cor);
				cor = StartCoroutine(boomerang());
			}
			else
			{
				if(cor!=null)
				StopCoroutine(cor);
				cor = StartCoroutine(homeIn());
			}
		}
		else
		{
			if(isPlayer)
			{
				float vForce = vertForceLKnife+(pScript.rb.velocity.y/3);
				if(pScript.inverted)
				{
					vForce = -vForce;
					grav.pushForces = new Vector2(Mathf.Abs(grav.pushForces.x),Mathf.Abs(grav.pushForces.y));
					grav.savedPushForces = new Vector2(Mathf.Abs(grav.savedPushForces.x),Mathf.Abs(grav.savedPushForces.y));
				}
				if(pScript.crouching)vForce = vForce/2;
				rb.velocity = new Vector2(speed*main.localScale.x,vForce);
			}
			else
			{
				float vForce;
				if(enemyAI!=null)
				{
					vForce = vertForceLKnife+(enemyAI.rb.velocity.y/3);
					if(enemyAI.inverted)
					{
						vForce = -vForce;
						grav.pushForces = new Vector2(Mathf.Abs(grav.pushForces.x),Mathf.Abs(grav.pushForces.y));
						grav.savedPushForces = new Vector2(Mathf.Abs(grav.savedPushForces.x),Mathf.Abs(grav.savedPushForces.y));
					}
				}
				else vForce = vertForceLKnife;
				rb.velocity = new Vector2(speed*-main.localScale.x,vForce);
				if(flipDirection)
				{
					rb.velocity=new Vector2(-rb.velocity.x,rb.velocity.y);
				}
			}
		}
	}
}
