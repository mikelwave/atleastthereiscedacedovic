using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shadowManAI : MonoBehaviour
{
    public LayerMask whatIsGround;
	Transform player;
	EnemyOffScreenDisabler eneOff;
	checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	public bool ignoreSemiSolid = false;
	public Vector3 startPosition;
	Animator anim;
	public float outOfBoundsDistance = 12;
	public float horizontalSpeed = 3f;
	bool grounded = false;
	bool touchingGround = false;
	GameData data;
	bool visible = false;
    public Rigidbody2D rb;
    bool dying = false;
	int dirFrames = 60;
    public int shootWait = 81;
	public int firstAwakeShootWait = 61;
	int startShootWait = 0;
	int moveDir = 1;
	public bool inverted = false;
	bool leftEdge = false;
	bool rightEdge = false;
	bool inbounds = false;
	int voidInt = 0;
	int agroFrames = 0;
	public int fireSoundID = 80;
	public GameObject LKnife;
	List <GameObject> knives;
	public float jump = 0;
    // Start is called before the first frame update
    void Start()
    {
        if(player==null)
		{
			player = GameObject.Find("Player_main").transform;
			eneOff = GetComponent<EnemyOffScreenDisabler>();
			startPosition = transform.position;
			data = GameObject.Find("_GM").GetComponent<GameData>();
			rb = GetComponent<Rigidbody2D>();
			if(transform.childCount>1)
			checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
			semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
			anim = GetComponent<Animator>();
			startShootWait = shootWait;
			shootWait = firstAwakeShootWait;
			//print(transform.parent.GetChild(1).name);
			moveDir = (int)transform.localScale.x;
			//Debug.Log("Start");
			knives = new List<GameObject>();
			for(int i = 0;i<2;i++)
			{
				GameObject obj = Instantiate(LKnife,transform.position,Quaternion.identity);
				knifeScript ksc = obj.GetComponent<knifeScript>();
				ksc.main = transform;
				ksc.enemyAI = this;
				obj.SetActive(false);
				knives.Add(obj);
			}
			GetComponent<Crusher>().assignValues(whatIsGround,GetComponent<EnemyCorpseSpawner>(),data);
		}
    }
    // Update is called once per frame
    void Update()
    {
        if(eneOff.visible&&Time.timeScale!=0)
		{
			if(agroFrames>0)agroFrames--;
			if(inbounds)
			{
                if(shootWait>0&&!dying)
                {
                    shootWait--;
                    if(shootWait==0)
                    {
						if(jump==0||jump!=0&&player.position.y<transform.position.y+0.5f)
                        anim.SetTrigger("shoot");
						else StartCoroutine(jumpFire());
                        shootWait = startShootWait;
                    }
                }
				if(dirFrames<=0)
				changemoveDir(0);
				else dirFrames--;

				movement();
				switch(voidInt)
				{
				default: if(grounded)edgeDetect(); voidInt=2; break;

				case 1: if(grounded)wallDetect(); voidInt--; break;

				case 2: if(player.parent==null)facePlayer(); voidInt--; break;
				}
			}
			checkForPlayer();
			groundDetect();
            anim.SetFloat("HorSpeed",Mathf.Abs(rb.velocity.x));
        }
        if(checker!=null)
		{
			if(rb.velocity.y>0&&!ignoreSemiSolid||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
			{
				ignoreSemiSolid = true;
				Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
				whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
				////Debug.Log(gameObject.name+" Ignoring collision");
			}
			else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
			{
				ignoreSemiSolid = false;
				Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
				whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
				////Debug.Log(gameObject.name+" Not ignoring collision");
			}
		}
    }
    IEnumerator dieInLava()
	{
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
    public void Fire()
    {	
		for(int i = 0; i<knives.Count; i++)
		{
			if(!knives[i].activeInHierarchy)
			{
				data.playSound(fireSoundID,transform.position);
				knives[i].transform.position = transform.GetChild(0).GetChild(0).position;
				knives[i].SetActive(true);
				break;
			}
		}
    }
	IEnumerator jumpFire()
	{
		rb.velocity = new Vector2(rb.velocity.x,jump);
		yield return new WaitForSeconds(0.25f);
		anim.SetTrigger("shoot");
	}
	public void deathEvent()
	{
		for(int i = knives.Count-1;i>=0;i--)
		{
			if(knives[i].activeInHierarchy)
			{
				knives[i].GetComponent<knifeScript>().destroyOnDisable = true;
			}
			else Destroy(knives[i]);
		}
	}
	void checkForPlayer()
	{
		float dist = Mathf.Abs(transform.position.x-player.position.x);
		if(dist>outOfBoundsDistance&&inbounds&&agroFrames==0)
		{
			inbounds = false;
			//print("player too far");
			rb.velocity=new Vector2(0,rb.velocity.y);
		}
		else if(dist<=outOfBoundsDistance&&!inbounds)
		{
			inbounds = true;
			if(agroFrames==0)agroFrames = 30;
			//print("player in bounds");
		}
	}
    void groundDetect()
	{
		Vector3 StartPoint1 = new Vector3(transform.position.x-0.21f,transform.position.y+transform.up.y*0.2f,transform.position.z);
		Vector3 StartPoint2 = new Vector3(transform.position.x+0.21f,transform.position.y+transform.up.y*0.2f,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint1,-transform.up,0.63f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(StartPoint2,-transform.up,0.63f,whatIsGround);
		//Debug.DrawRay(StartPoint1, ray1.point,Color.blue);
		//Debug.DrawRay(StartPoint2, ray2.point,Color.red);
		//if(ray1.collider!=null)print(ray1.collider.transform.name);
		//if(ray2.collider!=null)print(ray2.collider.transform.name);

		if(ray1.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0
		|| ray2.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0)
		{
			grounded = true;
		}
		else if(ray1.collider == null && grounded && !touchingGround
		     || ray1.collider == null && rb.velocity.y > 0.02f
		     || ray2.collider == null && grounded && !touchingGround
		     || ray2.collider == null && rb.velocity.y > 0.02f)
			 {
				grounded = false;
			 }
		if(fireSoundID!=80)
		anim.SetBool("Grounded",grounded);
	}
	void movement()
	{
		rb.velocity=new Vector2(moveDir*horizontalSpeed*-1,rb.velocity.y);
	}
	void changemoveDir(int dir)
	{
		if(dir == 0) moveDir = -moveDir;
		else moveDir = dir;
		dirFrames = 60;

		//anim.SetFloat("Dir",moveDir);
	}
	void wallDetect()
	{
		RaycastHit2D ray1,ray2;
		Vector3 point = transform.position+(transform.up*0.1f);
		if(!inverted)
		{
			ray1 = Physics2D.Raycast(point,-transform.right,0.6f,whatIsGround);
			ray2 = Physics2D.Raycast(point, transform.right,0.6f,whatIsGround);
		}
		else
		{
			ray1 = Physics2D.Raycast(point, transform.right,0.6f,whatIsGround);
			ray2 = Physics2D.Raycast(point,-transform.right,0.6f,whatIsGround);
		}
		if(!inverted)
		{
		Debug.DrawLine(point,new Vector3(point.x-0.6f,point.y,point.z),Color.red);
		Debug.DrawLine(point,new Vector3(point.x+0.6f,point.y,point.z),Color.blue);
		}
		else
		{
		Debug.DrawLine(point,new Vector3(point.x+0.6f,point.y,point.z),Color.red);
		Debug.DrawLine(point,new Vector3(point.x-0.6f,point.y,point.z),Color.blue);
		}

		if(ray1.collider!=null&&moveDir != -1f)
		{
			//if(ray1.collider.gameObject == gameObject)
			//	Debug.Log("Collided with itself");
			//Debug.Log(transform.name+ " detected" + ray1.collider.name);

			//turn around logic
			changemoveDir(-1);
		}
		if(ray2.collider!=null&&moveDir != 1f)
		{
			//if(ray2.collider.gameObject == gameObject)
			//	Debug.LogError("Collided with itself");
			//Debug.Log(transform.name+ " detected" + ray2.collider.name);

			//turn around logic
			changemoveDir(1);
			
		}
	}
    void edgeDetect()
	{
		Vector3 point,point2;
		point = new Vector3(transform.position.x-0.5f,transform.position.y+transform.up.y*0.2f,transform.position.z);
		point2 = new Vector3(transform.position.x+0.5f,transform.position.y+transform.up.y*0.2f,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(point,-transform.up,0.6f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(point2, -transform.up,0.6f,whatIsGround);
		if(!inverted)
		{
			Debug.DrawLine(point,new Vector3(point.x,point.y-0.5f,point.z),Color.green);
			Debug.DrawLine(point2,new Vector3(point2.x,point2.y-0.5f,point2.z),Color.yellow);
		}
		else
		{
			Debug.DrawLine(point,new Vector3(point.x,point.y+0.5f,point.z),Color.green);
			Debug.DrawLine(point2,new Vector3(point2.x,point2.y+0.5f,point2.z),Color.yellow);
		}

		if(ray1.collider==null&&!leftEdge)
		{
			leftEdge = true;
			if(!rightEdge)
			{
			//Debug.Log(transform.name+ " detected edge on the left");

			//turn around logic
			changemoveDir(-1);
			}
		}
		if(ray2.collider==null&&!rightEdge)
		{
			rightEdge = true;
			if(!leftEdge)
			{
			//Debug.Log(transform.name+ " detected edge on the right");

			//turn around logic
			changemoveDir(1);
			}
			
		}
		if(ray1.collider!=null&&leftEdge)
		{
			leftEdge = false;
		}
		if(ray2.collider!=null&&rightEdge)
		{
			rightEdge = false;
		}
	}
	void facePlayer()
	{
		if(player.position.x<=transform.position.x&&transform.localScale.x!=1)
		transform.localScale = Vector3.one;
		else if(player.position.x>transform.position.x&&transform.localScale.x!=-1)
        transform.localScale = new Vector3(-1,1,1);
	}
	void OnCollisionStay2D(Collision2D other)
	{
		if(other.gameObject.tag == "Spring"||other.gameObject.tag == "NPC"||other.gameObject.tag == "Ground"||other.gameObject.tag == "Harm"||other.gameObject.tag == "semiSolid"&&!ignoreSemiSolid&&checker.insideSemiSolid)
		{
			touchingGround = true;
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		if(other.gameObject.tag == "Spring"||other.gameObject.tag == "NPC"||other.gameObject.tag == "Ground"||other.gameObject.tag == "Harm"||other.gameObject.tag == "semiSolid")
		{
			touchingGround = false;
		}
	}
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&!visible)
		{
			visible = true;
			//Debug.Log("trigger");
			if(player==null)
			Start();
		}
		if(other.name=="InstantDeath")
		{
			if(GetComponent<Gravity>()!=null)
			{
			rb.velocity = Vector2.zero;
			Gravity grav = GetComponent<Gravity>();
			grav.pushForces = new Vector2(grav.pushForces.x,grav.pushForces.y/2);
			grav.maxVelocities = new Vector2(grav.maxVelocities.x,0.4f);
			}
			data.spawnCheeseSplatterPoint(transform.position);
			StartCoroutine(dieInLava());
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = other.GetComponent<Renderer>().sortingOrder-1;
			horizontalSpeed = 0;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&visible)
		{
			visible = false;
		}
	}
}
