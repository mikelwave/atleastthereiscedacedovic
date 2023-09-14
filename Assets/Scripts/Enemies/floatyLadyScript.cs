using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
public class floatyLadyScript : MonoBehaviour {
	[Header("Direction & speed")]
	public Vector2 velocity = new Vector2(0.5f,0.5f);
	[Space]
	[Header("Collision data")]
	public LayerMask whatIsGround;
	checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	EnemyCorpseSpawner eneCorpse;
	public bool ignoreSemiSolid = false;
	Rigidbody2D rb;
	Vector3 startPosition;
	EnemyOffScreenDisabler eneDis;
	AudioSource asc;
	float absVeloX = 3,absVeloY = 3;
	GameData data;
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nVelocity: "+velocity);
	}
	// Use this for initialization
	void Start ()
	{
		if(Application.isPlaying)
		{
			if(dataShare.debug)
			printer();
			data = GameObject.Find("_GM").GetComponent<GameData>();
			asc = GetComponent<AudioSource>();
			eneDis = GetComponent<EnemyOffScreenDisabler>();
			startPosition = transform.position;
			rb = GetComponent<Rigidbody2D>();
			if(transform.childCount>1)
			checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
			semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
			eneCorpse = GetComponent<EnemyCorpseSpawner>();
			Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(),gameObject.GetComponent<Collider2D>());
			GetComponent<Crusher>().assignValues(whatIsGround,GetComponent<EnemyCorpseSpawner>(),data);
		}
	}
	void OnDisable()
	{
		if(ignoreSemiSolid)
		{
			ignoreSemiSolid = false;
			Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
			whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
			//Debug.Log(gameObject.name+" Not ignoring collision");
		}
	}
	IEnumerator dieInLava()
	{
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
	void Update()
	{
		#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			Vector2 linePoint = new Vector2(Mathf.Clamp(velocity.x,-30,30),Mathf.Clamp(velocity.y,-30,30));
			Debug.DrawLine(transform.position,new Vector3(transform.position.x+linePoint.x,transform.position.y+linePoint.y,transform.position.z),Color.red);
		}
		#endif
		if(checker!=null)
		{
			if(rb.velocity.y>0&&!ignoreSemiSolid
			||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
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
		if(Application.isPlaying)
		{
			rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x*100,-absVeloX,absVeloX+0.1f),Mathf.Clamp(rb.velocity.y*100,-absVeloY,absVeloY+0.1f));
		}
	}
	void OnCollisionEnter2D(Collision2D other)
	{
		if(other.gameObject.tag=="Ground"||other.gameObject.tag=="semiSolidGround")
		{
			//print("sound");
			if(asc.enabled&&Time.timeSinceLevelLoad>0.3f)
			asc.Play();
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&eneDis.enableFlag)
		{
			rb.velocity = velocity;
		}
		if(other.name=="InstantDeath")
		{
			rb.velocity = new Vector2(0,-0.4f);
			data.spawnCheeseSplatterPoint(transform.position);
			StartCoroutine(dieInLava());
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = other.GetComponent<Renderer>().sortingOrder-1;
		}
		if(other.name=="deathZone")
		{
				rb.velocity = new Vector2(0,0);
				if(eneDis.visible)
				eneDis.toggle(false);
				else eneDis.enableFlag = false;
				transform.position = startPosition;
		}
	}
		void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			rb.velocity = new Vector2(0,0);
		}
	}
}
