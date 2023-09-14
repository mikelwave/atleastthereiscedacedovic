using System.Collections.Generic;
using UnityEngine;

public class Hardcore_AI : MonoBehaviour {
	EnemyOffScreenDisabler eneOff;
	float[] ranges = new float[2];
	Transform player;
	Transform mainSprite;
	public float speed = 1f;
	float lastHeight = 0;
	Animator anim;
	public GameObject projectile;
	List<GameObject> projectiles;
	float projAmount = 2;
	int throwCountdown = 40;
	public bool throws = false;
	bool lockSpeed = false;
	GameData data;
	PlayerScript pScript;
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nSpeed: "+speed);
	}
	// Use this for initialization
	void Start ()
	{
		if(dataShare.debug)
		printer();
		player = GameObject.Find("Player_main").transform;
		pScript = player.GetComponent<PlayerScript>();
		mainSprite = transform.GetChild(0);
		eneOff = GetComponent<EnemyOffScreenDisabler>();
		anim = GetComponent<Animator>();
		float cableHeight = transform.GetChild(1).GetComponent<SpriteRenderer>().size.y;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		projectiles = new List<GameObject>();
		for(int i = 0; i<projAmount;i++)
		{
			GameObject obj = Instantiate(projectile,transform.position,Quaternion.identity);
			obj.SetActive(false);
			obj.transform.SetParent(transform);
			obj.GetComponent<hammerScript>().parentObj = transform;
			projectiles.Add(obj);
		}
		//float offset = mainSprite.localPosition.y-transform.GetChild(1).localPosition.y;
		ranges[0] = (cableHeight/2)-1.5f;
		ranges[1] = -(cableHeight/2);

		Vector3 cablePos = transform.GetChild(1).position;
		Debug.DrawLine(new Vector3(cablePos.x,cablePos.y+ranges[0],cablePos.z),new Vector3(cablePos.x,cablePos.y+ranges[1],cablePos.z),Color.green,3f);

		lastHeight = Mathf.Abs(mainSprite.localPosition.y);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(eneOff.visible&&Time.timeScale!=0)
		{
			facePlayer();
			if(!throws&&!lockSpeed)
			{
				followPlayer();
				climbAnimation();
			}
			if(!throws&&!lockSpeed&&!pScript.inCutscene)
			throwCountdown--;
			if(throwCountdown<=0)
			{
				lockSpeed = true;
				throwCountdown = 150;
				anim.speed = 1;
				anim.SetTrigger("Throw");
			}
		}	
	}
	void OnDestroy()
	{
		if(Application.isPlaying)
		for(int i = projectiles.Count-1; i>=0;i--)
		{
			if(projectiles[i]!=null&&projectiles[i].activeInHierarchy)
			projectiles[i].GetComponent<hammerScript>().destroyOnEnd = true;
			else Destroy(projectiles[i]);
		}
	}
	public void playSound()
	{
		data.playSoundOverWrite(60,mainSprite.position);
		anim.speed = 1;
		lockSpeed = false;
	}
	void climbAnimation()
	{
		float f = Mathf.Abs(Mathf.Abs(mainSprite.localPosition.y)-lastHeight);
		if(!lockSpeed&&!throws)
		anim.speed = f*70;

		lastHeight = Mathf.Abs(mainSprite.localPosition.y);
	}
	public void throwHammer(int hammerOrder)
	{
		if(eneOff.visible)
		for(int i = 0; i<projectiles.Count;i++)
		{
			if(!projectiles[i].activeInHierarchy)
			{
				projectiles[i].transform.position = mainSprite.GetChild(0).position;
				projectiles[i].transform.localScale = new Vector3(-mainSprite.localScale.x,mainSprite.localScale.y,mainSprite.localScale.z);
				if(hammerOrder==0)
				projectiles[i].GetComponent<hammerScript>().direction = new Vector2(4f,8f);
				else projectiles[i].GetComponent<hammerScript>().direction = new Vector2(5f,15f);

				projectiles[i].transform.SetParent(null);
				projectiles[i].SetActive(true);
				break;
			}
		}
	}
	void followPlayer()
	{
		mainSprite.position += (new Vector3(mainSprite.position.x,player.position.y,mainSprite.position.z)
		- mainSprite.position) * speed;

		mainSprite.localPosition = new Vector3
		(mainSprite.localPosition.x,Mathf.Clamp(mainSprite.localPosition.y,ranges[1],ranges[0]),mainSprite.localPosition.z);
	}
	void facePlayer()
	{
		if(player.position.x<=transform.position.x&&mainSprite.localScale.x!=-1)
		{
			mainSprite.localScale = new Vector3(-1,1,1);
			mainSprite.localPosition = new Vector3(0.125f,mainSprite.localPosition.y,mainSprite.localPosition.z);
		}
		else if(player.position.x>transform.position.x&&mainSprite.localScale.x!=1)
		{
			mainSprite.localScale = Vector3.one;
			mainSprite.localPosition = new Vector3(-0.125f,mainSprite.localPosition.y,mainSprite.localPosition.z);
		}
	}
}
