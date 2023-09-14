using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class movingPlatformScript : MonoBehaviour {
	[Header ("EDIT MODE")]
	[Space]
	public bool createPoint;
	public int platformLength = 3;
	public bool isLooping = false;
	public Sprite[] sprites = new Sprite[2];
	[Space]
	public float movementSpeed = 5f;
	public Transform CurrentTarget;
	Transform Platform;
	int numberOfPoints = 0;
	int CurrentPoint = 1;
	bool forward = true;
	public bool waitForPlayerToMove = false,waitForActivatorToMove = false;
	public bool moving = true;
	public bool stopMovingOnReachedPoint = false,disableOnReachedPoint = false,alwaysMoving = false;
	public bool usesSaw = false;
	public bool pauseOnNPCTalk = false;
	SpriteRenderer render;
	PlayerScript playerScript;
	//PlayerScript pScript;
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nSpeed: "+movementSpeed);
	}
	// Use this for initialization
	void Start ()
	{
		if(Application.isPlaying)
		{
			if(dataShare.debug)
			printer();
			if(pauseOnNPCTalk)
			{
				playerScript = FindObjectOfType<PlayerScript>();
			}
			if(waitForPlayerToMove||waitForActivatorToMove)
			moving = false;
			else if(alwaysMoving) moving = true;
			Platform = transform.GetChild(0);
			CurrentTarget = transform.GetChild(1);
			numberOfPoints = transform.childCount-1;
			if(!usesSaw)
			transform.GetChild(0).GetComponent<BoxCollider2D>().size = new Vector2(Mathf.Abs(platformLength),1);
		}
	}
	public void switchPoint(int pointID)
	{
		if(!moving)moving = true;
		CurrentPoint = pointID;
		CurrentTarget = transform.GetChild(CurrentPoint);
	}
	// Update is called once per frame
	void FixedUpdate ()
	{
		if(Application.isPlaying&&moving)
		{
			if(CurrentTarget!=null&&Platform!=null&&(!pauseOnNPCTalk||pauseOnNPCTalk&&!playerScript.inCutscene))
			Platform.position = Vector3.MoveTowards(Platform.position, CurrentTarget.position, movementSpeed*Time.timeScale);
			if(Platform!=null&&Platform.position==CurrentTarget.position)
			{
				if(!isLooping)
				{
					if(CurrentPoint>numberOfPoints-1 && forward)
						forward = false;
					else if(CurrentPoint<1 && !forward)
						forward = true;
					if(forward) CurrentPoint++;
					else CurrentPoint--;
				}
				else
				{
					CurrentPoint++;
					if(CurrentPoint>numberOfPoints)
					{
						CurrentPoint = 1;
					}
				}
				//Debug.Log("Target: "+CurrentTarget.name+", position: "+CurrentTarget.position+" Platform position: "+Platform.position);
				if(stopMovingOnReachedPoint)
				{
					moving = false;
					if(disableOnReachedPoint)gameObject.SetActive(false);
				}
				CurrentTarget = transform.GetChild(CurrentPoint);
			}
		}
	}
	void Update()
	{
		if(!Application.isPlaying)
		{
			if(!usesSaw)
			{
				if(platformLength==0)
				{
					platformLength = 1;
				}
				if(render==null)
					render = transform.GetChild(0).GetComponent<SpriteRenderer>();
				render.size = new Vector2(Mathf.Abs(platformLength),1);
				if(platformLength==1)
				render.sprite = sprites[0];
				else render.sprite = sprites[1];
			}
			if(createPoint)
			{
				createPoint = false;
				Transform pointPos = transform.GetChild(transform.childCount-1);
				GameObject obj = Instantiate(transform.GetChild(1).gameObject,new Vector3(pointPos.position.x,pointPos.position.y+1f,pointPos.position.z),Quaternion.identity);
				obj.transform.SetParent(transform);
				obj.transform.name = "Point "+(transform.childCount-1);
			}
		}
		else
		{
			if(Platform==null) Destroy(gameObject);
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		//print(other.name);
		if(other.name == "ObjectActivator"&&waitForActivatorToMove)
		{
			moving = true;
			Destroy(GetComponent<BoxCollider2D>());
		}
	}
	void OnCollisionEnter2D(Collision2D other)
	{
		Transform t = other.transform;
		bool inv = (t.eulerAngles.z>=180||t.localScale.y==-1)?true:false;
		//print("Collision: "+(t.position.y)+" Plat: "+(Platform.position.y+0.3f));
		if((!inv&&t.position.y>=Platform.position.y+0.3f)||(inv&&t.position.y<=Platform.position.y-0.3f))
		{

			//print("Player landed");
			if(t.parent==null
			||t.parent!=null&&t.parent.name!="Player_main"&&t.name!="floatyLady_enemy"&&!t.name.ToLower().Contains("burger"))
			{
				t.SetParent(Platform);
				if(!moving&&other.gameObject.name=="Player_main"&&waitForPlayerToMove)
				moving = true;
			}
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		Transform t = other.transform;
		if(other.gameObject==null)return;
		if ((other.gameObject.tag == "Enemy" && (t.parent!=null&&t.parent.name != "Player_main")) || other.gameObject.tag == "EnemyCenterPivot" || other.gameObject.tag == "EnemyCustomPivot")
		{
			t.SetParent(GameObject.Find("Enemies").transform);
		}
		else if (base.gameObject.activeInHierarchy && ((t.parent != null && t.parent.name != "Player_main") || t.parent == null))
		{
			t.SetParent(null);
		}
		if (other.gameObject.name == "Player_main")
		{
			if(playerScript==null) playerScript = other.gameObject.GetComponent<PlayerScript>();
			playerScript.convertPosSpeedToVelocity();
		}
	}
}