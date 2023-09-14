using UnityEngine;

[ExecuteInEditMode]
public class BossFightScript : MonoBehaviour {
	public int eventInt = 0;
	public bool newEvent = false;
	public Vector2 stompPointOffset = new Vector2(0f,0f);
	Rigidbody2D playerRigid;
	PlayerScript pScript;
	GameObject player;
	MGCameraController cam;
	public BoxCollider2D cambounds;
	public Vector2[] camBoundsSections;
	public Vector2[] camBoundsOffset;
	public Vector3[] cameraStartPoints;
	public Vector3[] playerStartPoints;
	public Sprite[] skySections;
	SpriteRenderer subSky;
	dataShare dataS;
	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player_main");
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		pScript = player.GetComponent<PlayerScript>();
		playerRigid = player.GetComponent<Rigidbody2D>();
		subSky = cam.transform.GetChild(1).GetComponent<SpriteRenderer>();
		dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		if(dataS.checkpointValue==0)
		setPositions(0);
		else
		{
			setPositions(2);
			eventInt=2;
		}
	}
	void Update()
	{
		if(!Application.isPlaying)
		{
			Debug.DrawLine(
			new Vector3(transform.position.x-stompPointOffset.x,transform.position.y+stompPointOffset.y,transform.position.z),
			new Vector3(transform.position.x+stompPointOffset.x,transform.position.y+stompPointOffset.y,transform.position.z),Color.red);
		}
	}
	public void setPositions(int ID)
	{
		dataS.savedCamPos = cam.transform.position;
		if(skySections.Length>ID)
			subSky.sprite = skySections[ID];
		cambounds.size = camBoundsSections[ID];
		cambounds.offset = camBoundsOffset[ID];
		cam.transform.position = cameraStartPoints[ID];
		if(dataS.checkpointValue==0)
		{
			player.transform.position = playerStartPoints[ID];
			playerRigid.velocity = Vector2.zero;
		}
		pScript.knockedBack = false;
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "PlayerCollider")
		{
			if(playerRigid.velocity.y<-0.1f&&player.transform.position.y>transform.position.y+stompPointOffset.y&&!newEvent)
			{
				//Debug.Log("Stomped");
				eventInt++;
				newEvent = true;
				pScript.stompBoss(gameObject,true);
				if(eventInt>=3)pScript.goToCutsceneMode(true);
			}
			else
			{
				if(Time.timeScale!=0 && pScript.invFrames==0)
				{
					pScript.Damage(false,false);
				}
				if(pScript.knockbackCor !=null)
				StopCoroutine(pScript.knockbackCor);
				if(player.transform.position.x<transform.position.x)
				pScript.knockbackCor = StartCoroutine(pScript.knockBack(-1,1,0.5f,true));
				else
				pScript.knockbackCor = StartCoroutine(pScript.knockBack(1,1,0.5f,true));
			}
		}
	}
	public void cutscene(bool activate)
	{
		pScript.controllable = !activate;
		pScript.knockedBack = activate;
		if(activate)
		playerRigid.velocity = new Vector2(-2,0.1f);
		pScript.inCutscene = activate;
		pScript.axis.acceptXInputs = !activate;
		pScript.axis.acceptYInputs = !activate;
	}
}
