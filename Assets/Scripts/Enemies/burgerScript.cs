using System.Collections;
using UnityEngine;

public class burgerScript : MonoBehaviour
{
    Gravity grav;
    Transform player;
    EnemyOffScreenDisabler enemyOff;
    Vector3 startPoint;
    Rigidbody2D rb;
    bool ignoreSemiSolid = false;
    public LayerMask whatIsGround;
    CompositeCollider2D semiSolid;
    bool attack = false;
    public float speed = 10f;
    int thwompWait = 0;
    MGCameraController cam;
    public AudioClip impactSound;
    GameData data;
    public bool big = false;
    float playerdistance = 1.5f;
    float enemydistance = 0.5f;
    bool inLava = false;
    IEnumerator dieInLava()
	{
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
    // Start is called before the first frame update
    void Start()
    {
        grav = GetComponent<Gravity>();
        player = GameObject.Find("Player_main").transform;
        startPoint = transform.position;
        enemyOff = GetComponent<EnemyOffScreenDisabler>();
        rb = GetComponent<Rigidbody2D>();
        semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
        cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        gameObject.layer = 28;
        data = GameObject.Find("_GM").GetComponent<GameData>();
        if(big)
        {
            playerdistance = 2f;
            enemydistance = 1f;
        }
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        ignoreSemiSolid = false;
        Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
        whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
        //Debug.Log(gameObject.name+" Ignoring collision "+"Checker inside: "+checker.insideSemiSolid+" RbY: "+rb.velocity.y);
    }
    void FixedUpdate()
    {
        if(attack&&thwompWait==0&&!grav.enabled&&!inLava)
        {
            transform.position = Vector3.MoveTowards(transform.position,startPoint,speed*Time.deltaTime);
            if(transform.position==startPoint)
            {
                attack = false;
                thwompWait = 15;
                gameObject.layer = 28;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }
    public void offscreenReset()
    {
        if(!inLava)
        {
            if(transform.position!=startPoint&&Time.timeSinceLevelLoad>1)
            {
                transform.position = startPoint;
            }
            if(attack)
            {
                rb.velocity = Vector2.zero;
                attack = false;
                thwompWait = 0;
                grav.enabled = false;
                gameObject.layer = 28;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                ignoreSemiSolid = false;
                Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
                whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(enemyOff.visible&&Time.timeScale!=0&&!inLava)
        {
            if(thwompWait>0)thwompWait--;
            if(thwompWait==0&&player.position.y-0.5f<=transform.position.y&&Mathf.Abs(Mathf.Abs(transform.position.x)-Mathf.Abs(player.position.x))<=playerdistance&&!attack)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionX|RigidbodyConstraints2D.FreezeRotation;
                attack = true;
                grav.enabled = true;
                gameObject.layer = 13;
                rb.constraints = RigidbodyConstraints2D.FreezePositionX|RigidbodyConstraints2D.FreezeRotation;
                ignoreSemiSolid = false;
                Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
                whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
            }
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if(attack&&!inLava)
        {
            if(other.gameObject.tag=="Ground"
            ||other.gameObject.tag=="Harm"
            ||other.gameObject.tag=="semiSolid"&&!ignoreSemiSolid)
            {
                if(grav.enabled)
                {
                    //print("thwomp");
                    data.playUnlistedSoundPoint(impactSound,transform.position);
                    cam.easeShake = true;
                    cam.shakeCameraVertically(0.4f,0.8f);
                    rb.velocity = Vector2.zero;
                    if(!big
                    ||other.gameObject.tag=="Harm"
                    ||other.gameObject.tag!="semiSolid"&&big&&!data.explodeTile(new Vector3(transform.position.x+0.5f,transform.position.y-1f,transform.position.z),true)
                    ||other.gameObject.tag!="semiSolid"&&big&&!data.explodeTile(new Vector3(transform.position.x+0.5f-1,transform.position.y-1f,transform.position.z),true)
                    ||other.gameObject.tag=="semiSolid"&&big)
                    {
                        if(!big&&thwompWait==0) data.explodeTile(transform.position+new Vector3(0,-1,0),true);
                        thwompWait = 60;
                        grav.enabled = false;
                        gameObject.layer = 28;
                        rb.constraints = RigidbodyConstraints2D.FreezeAll;
                        ignoreSemiSolid = true;
                        Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
                        whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
                    }
                }
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //print(other.tag+" "+other.name);
        if(Mathf.Abs(Mathf.Abs(transform.position.x)-Mathf.Abs(other.transform.position.x))<=enemydistance&&attack)
        {
            //print(other.tag+" "+other.name);
            if(other.tag=="Enemy"
            ||other.tag=="EnemyCenterPivot"
            ||other.tag=="EnemyCustomPivot")
            {
                EnemyCorpseSpawner eneCorpse = other.GetComponent<EnemyCorpseSpawner>();
                if(eneCorpse!=null&&eneCorpse.canGetCrushed)
                {
                    eneCorpse.createCorpseFlipped = true;
                    eneCorpse.createCorpse = false;
                    eneCorpse.spawnCorpse();
                }
            }
        }
        if(other.name=="InstantDeath"&&!inLava)
		{
			if(GetComponent<Gravity>()!=null)
			{
            inLava = true;
			rb.velocity = Vector2.zero;
			Gravity grav = GetComponent<Gravity>();
			grav.pushForces = new Vector2(grav.pushForces.x,grav.pushForces.y/2);
			grav.maxVelocities = new Vector2(grav.maxVelocities.x,0.4f);
			}
			data.spawnCheeseSplatterPoint(transform.position);
			StartCoroutine(dieInLava());
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = other.GetComponent<Renderer>().sortingOrder-1;
		}
        if(other.name=="deathZone")
		{
            enemyOff.toggle(false);
        }

    }
}
