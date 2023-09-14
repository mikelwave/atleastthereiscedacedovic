using UnityEngine;
using System.Collections;

public class GoalBall : MonoBehaviour {
	GameData data;
	Rigidbody2D rb;
	public float ballSpeed = 1f;
	bool netBreak = false;
	int direction = 1;
	public bool kicked = false;
	int tilesToGo = 0;
	int currentTile = 0;
	int litArrows = 0;
	float distance;
	float distanceLeft = 0;
	float speedSubtract = 1;
	public bool goal = false;
	public bool wallImpact = false;
	int timeStopFrames = 0;
	Transform player;
	PlayerScript pScript;
	bool emer = false;
	// Use this for initialization
	void Start () {
		data = GameObject.Find("_GM").GetComponent<GameData>();
		rb = GetComponent<Rigidbody2D>();
		player = GameObject.Find("Player_main").transform;
		pScript = player.GetComponent<PlayerScript>();
		data.ball = GetComponent<Collider2D>();
	}
	IEnumerator emerSave()
	{
		dataShare DataS = data.DataS;
		Debug.LogError("Player deleted, resetting.");
		DataS.loadSceneWithoutLoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		yield return 0;
	}
	void Update()
	{
		if(Mathf.Abs(currentTile) < Mathf.Abs(tilesToGo))
		{
			currentTile = Mathf.RoundToInt(transform.localPosition.x);

			if(Mathf.Abs(currentTile) < Mathf.Abs(tilesToGo))
			{
				rb.velocity = new Vector2(ballSpeed*litArrows*direction*speedSubtract,rb.velocity.y);
			}
			else
			{
				rb.velocity = new Vector2(0,rb.velocity.y);
				//print(litArrows);
				if(litArrows<=1)
				{
					data.addScore(100);
					data.ScorePopUp(transform.position,"+100",new Color32(255,255,255,255));
				}
				else
				{
					long sc = 400*litArrows;
					data.addScore(sc);
					data.ScorePopUp(transform.position,"+"+sc.ToString(),new Color32(255,255,255,255));
				}
			}
		}
		//count speed subtract if dir 1
		if(distance!=0 && distanceLeft > 0 && direction == 1)
		{
			distanceLeft = distance-Mathf.Abs(transform.localPosition.x);
			if(distanceLeft < 0)
			{
				distanceLeft = 0;
			}
			
			if(distanceLeft>0)
			speedSubtract = distanceLeft/distance;
		}
		//count speed subtract if dir -1
		if(distance!=0 && distanceLeft < 0 && direction == -1)
		{
			distanceLeft = distance+Mathf.Abs(transform.localPosition.x);
			if(distanceLeft > 0)
				distanceLeft = 0;
			
			if(distanceLeft<0)
			speedSubtract = distanceLeft/distance;
		}
		if(goal && distanceLeft == 0 && speedSubtract!=0)
		{
			rb.velocity = new Vector2(ballSpeed*speedSubtract*direction,rb.velocity.y);
			speedSubtract+=0.025f*direction;
			if(speedSubtract<0)
			{
				speedSubtract = 0;
			}
		}
		transform.Rotate(Vector3.forward*-(rb.velocity.x)*Time.timeScale);
		if(timeStopFrames > 0)
		{
			timeStopFrames--;
			if(timeStopFrames==0)
				Time.timeScale = 1;
		}
		if(player==null)
		{
			if(!emer)
				StartCoroutine(emerSave());
			emer = true;
			return;
		}
		if(!player.gameObject.activeInHierarchy)
		{
			player.gameObject.SetActive(true);
		}
		if(player.transform.position.x>transform.position.x-0.4f&&player.transform.position.y>=transform.position.y
		&&player.transform.position.x<transform.position.x+10f)
		{
			kicked = true;
			player.GetComponent<PlayerScript>().reachedGoal = true;
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "PlayerCollider" && !kicked&&!pScript.dead)
		{
			pScript.pauseMenu.enabled = false;
			pScript.axis.axisPosX = pScript.axis.axisPosX/4f;
			Rigidbody2D r = other.transform.parent.GetComponent<Rigidbody2D>();
			//print("Velocity: "+r.velocity.y);
			if(r.velocity.y > 0.2f&&SuperInput.GetKey("Jump"))
			{
				netBreak = true;
			}
			kicked = true;
			if(other.transform.position.x < transform.position.x)
			direction = 1;
			else direction = -1;

			if(data.litSMeterArrows!=0 && data.litSMeterArrows <= 7)
			{
				tilesToGo = data.litSMeterArrows*2*direction;
				litArrows = data.litSMeterArrows;
				distance = tilesToGo;
				distanceLeft = tilesToGo;
				//set goal anim to hit;
				data.playSound(10,transform.position);
				StartCoroutine(data.goalAnimate(1,1.5f));
			}
			else if(data.litSMeterArrows == 8)
			{
				tilesToGo = 40*direction;
				litArrows = 12;
				distance = 40;
				distanceLeft = 40;
				if(netBreak)
				{
					timeStopFrames = 60;
					ballSpeed = 20;
					GetComponent<Gravity>().pushForces = Vector2.zero;
					rb.velocity = new Vector2(rb.velocity.x,3f);
					//set goal anim to net break;
					Debug.Log("NET BREAKER!!");
					StartCoroutine(data.goalAnimate(3,2f));
					data.playSound(12,transform.position);
				}
				else
				{
					//set goal anim to goal
					StartCoroutine(data.goalAnimate(2,0.8f));
					data.playSound(11,transform.position);
				}
			}
			else
			{
				tilesToGo = 1;
				litArrows = 1;
				distance = tilesToGo;
				distanceLeft = tilesToGo;
				//set goal anim to hit;
				data.playSound(10,transform.position);
				StartCoroutine(data.goalAnimate(1,1.5f));
			}
			//Debug.Log(data.litSMeterArrows+", "+tilesToGo);
		}
		if(other.name == "GoalEdge")
		{
			goal = true;
		}
		if(other.name == "Net"&&!netBreak)
		{
			rb.velocity = new Vector2(rb.velocity.x,5f);
			direction = -direction;
			litArrows = 0;
			distance = 0;
			distanceLeft = 0;
			speedSubtract = 1;
			data.playSound(13,transform.position);
			other.transform.GetChild(0).GetComponent<Animator>().SetInteger("value",1);
			data.addScore(5000);
			data.ScorePopUp(transform.position,"+5000",new Color32(255,255,255,255));
		}
		if(other.name == "Net"&&netBreak)
		{
			data.playSound(14,transform.position);
			other.transform.GetChild(0).GetComponent<Animator>().SetInteger("value",2);
			data.addLivesSilent(1);
			//data.ScorePopUp(transform.position,"+1up",new Color32(133,251,124,255));
			if(data.mode!=1)
				data.ScorePopUp(transform.position,"1up",new Color32(133,251,124,255));
			else data.ScorePopUp(transform.position,"ALT1up",new Color32(133,251,124,255));
		}
	}
}
