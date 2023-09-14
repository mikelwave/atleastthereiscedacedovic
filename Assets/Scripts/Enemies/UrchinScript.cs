using System.Collections;
using UnityEngine;

public class UrchinScript : MonoBehaviour {
	public float speed = 1f;
	public Sprite deathSprite;
	Transform dirPad,blockade;
	Vector3 targetPos;
	public Vector2 offset = new Vector2(.5f,.5f);
	public LayerMask whatIsGround;
	Coroutine raycaster;
	RaycastHit2D rayForward;
	RaycastHit2D rayWall;
	int Turnframes = 0;
	SimpleAnim2 anim2;
	SpriteRenderer render;
	public bool reversed = false;
	int dirInvert = 1;
	public GameObject flipped;
	public Vector2 flipVelo = new Vector2(1,10);
	Vector2 flipVeloUsed;
	public AudioClip deathSound;
	GameData data;
	GameObject obj ;
	bool dead = false;
	bool visible = false;
	public bool startAsMoving = false;
	// Use this for initialization
	void Start ()
	{
		if(reversed)
		dirInvert = -1;
		dirPad = transform.GetChild(0).GetChild(0);
		blockade = transform.GetChild(1);
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		anim2 = render.GetComponent<SimpleAnim2>();
		obj = Instantiate(flipped,transform.position,Quaternion.identity);
		obj.transform.parent = transform;
		if(transform.parent==null)
		blockade.parent = null;
		else blockade.parent = transform.parent;
		targetPos = transform.position;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		casting();

		if(!visible)
		togglevisible(false);
		if(startAsMoving&&raycaster==null)
		raycaster = StartCoroutine(rayCastWalls());
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			togglevisible(true);
			if(dirPad==null)
			Start();
			if(raycaster==null)
			raycaster = StartCoroutine(rayCastWalls());
		}
		if(other.name=="ScreenNuke"&&!dead
		||other.name =="BlockParent(Clone)"&&!dead
		||other.name=="HalvaOverlay"&&!dead
		||other.tag =="blockHoldable"&&!dead && Mathf.Abs(other.transform.parent.GetComponent<Rigidbody2D>().velocity.x)>=2f
		||other.tag == "lKnife"&&!dead)
		{
			dead = true;
			if(raycaster!=null)
			StopCoroutine(raycaster);
			Vector3 pos = new Vector3(transform.position.x,transform.position.y+0.5f,transform.position.z);
			Transform objTrans = obj.transform;
			objTrans.parent = null;
			objTrans.position = pos;
			objTrans.localScale = transform.parent.localScale;
			SpriteRenderer render2 = obj.GetComponent<SpriteRenderer>();
			render2.sprite = deathSprite;
			render2.color = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
			if(other.tag=="lKnife")
			{
				data.playSoundOverWrite(24,transform.position);
			}
			if(other.name!="HalvaOverlay")
			{
				data.addScore(200);
				if(other.name!="ScreenNuke")
				{
					data.ScorePopUp(transform.position,"+200",new Color32(255,255,255,255));
					data.GetComponent<AudioSource>().PlayOneShot(deathSound);
				}
			}
			else
			{
				data.playeneSound(deathSound,1f+(data.halvaStreak/20));
				data.halvaStreak++;
				data.streakScore(data.halvaStreak,transform.position,true);
			}
			Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
			obj.SetActive(true);
			int rando = Random.Range(0,2);
			if(rando==1)
				flipVeloUsed = new Vector2(-flipVelo.x,flipVelo.y);
			else flipVeloUsed = new Vector2(flipVelo.x,flipVelo.y);
			rb.velocity = flipVeloUsed;
			if(rando==1)
				rb.angularVelocity = 100;
			else rb.angularVelocity = -100;
			Destroy(blockade.gameObject);
			Destroy(gameObject);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			togglevisible(false);
		}
	}
	void togglevisible(bool toggle)
	{
		visible = toggle;
		render.enabled = toggle;
		anim2.enabled = toggle;
	}
	void Update()
	{
		if(Turnframes>0&&Time.timeScale!=0)
		{
			Turnframes--;
		}
	}
	// Update is called once per frame
	void FixedUpdate ()
	{
		if(Time.timeScale!=0)
		{	
			transform.position = Vector3.MoveTowards(transform.position,targetPos,speed);
		}
	}
	void moveForward()
	{
		//Debug.Log(Mathf.RoundToInt(transform.position.x+0.5f)+" "+Mathf.RoundToInt(transform.position.y+0.5f));
		targetPos = new Vector3(Mathf.RoundToInt(transform.position.x+0.5f)+offset.x,Mathf.RoundToInt(transform.position.y+0.5f)+offset.y,0);
		targetPos+=dirPad.up;
		blockade.position = targetPos;
		casting();
	}
	void casting()
	{
		rayForward = Physics2D.Raycast(blockade.position,dirPad.up,0.95f,whatIsGround);
		rayWall = Physics2D.Raycast(blockade.position,-dirPad.right*dirInvert,0.95f,whatIsGround);
		//Debug.DrawLine(blockade.position,blockade.position+dirPad.up,Color.red,0.1f);
		//Debug.DrawLine(blockade.position,blockade.position-dirPad.right,Color.yellow,0.1f);
	}
	IEnumerator rayCastWalls()
	{
		while(!dead)
		{
			casting();
			//Cond 1: no side wall detected
			if(rayWall.collider==null)
			{
				//Debug.Log("Cond1");
				//turning
				dirPad.eulerAngles+=new Vector3(0,0,90*dirInvert);
				Turnframes = 5;
				//after a turn, recast surroundings
				casting();
				//if path is clear, move forward
				if(rayForward.collider==null)
				{
					blockade.position = targetPos+dirPad.up;
					yield return new WaitUntil(()=>Turnframes==0);
					moveForward();

				}
			}
			//Cond 2: side wall detected and can move forward
			else if(rayForward.collider==null)
			{
				casting();
				if(rayForward.collider==null)
				{
					//Debug.Log("Cond2");
					moveForward();

				}
			}
			//Cond 3: side wall detected but can't move forward
			else if(rayForward.collider!=null)
			{
				//Debug.Log("Cond3");
				//turning
				dirPad.eulerAngles+=new Vector3(0,0,-90*dirInvert);
				Turnframes = 5;
				//after a turn, recast surroundings
				casting();
				//if path is clear, move forward
				if(rayForward.collider==null)
				{
					blockade.position = targetPos+dirPad.up;
					yield return new WaitUntil(()=>Turnframes==0);
					moveForward();

				}
			}
			yield return new WaitForSeconds(0.195f);
		}
	}
}
