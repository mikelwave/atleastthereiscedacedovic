using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class WarpHitboxResize : MonoBehaviour {
	BoxCollider2D col2D;
	playerSprite pSprites;
	AxisSimulator axis;
	Collider2D col;
	PlayerScript pScript;
	Gravity grav;
	int value = -1;
	public bool midWarp = false;
	public float animationGoalSpeed = 1f;
	GameObject cam;
	MGCameraController camScript;
	GameData data;
	public Sprite[] regularDoorSprite = new Sprite[3];
	public Sprite[] keyDoorSprite = new Sprite[2];
	bool canOpen = true;
	public bool inWarpCollider = false;
	// Use this for initialization
	void Start () {
		pSprites = transform.parent.transform.parent.GetComponent<playerSprite>();
		axis = transform.parent.transform.parent.GetComponent<AxisSimulator>();
		col2D = GetComponent<BoxCollider2D>();
		col = transform.parent.GetComponent<Collider2D>();
		pScript = transform.parent.transform.parent.GetComponent<PlayerScript>();
		grav = transform.parent.transform.parent.GetComponent<Gravity>();
		cam = GameObject.Find("Main Camera");
		camScript = cam.GetComponent<MGCameraController>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
	}
	public void setSize()
	{
		if(pSprites==null)
			Start();
		if(pSprites.state==0)
		{
			//vertical inputs take priority
			if(Mathf.Abs(axis.verAxis)==1f && !axis.acceptFakeInputs && value!=0|| axis.acceptFakeInputs && Mathf.Abs(axis.artificialY)>0.1f && value!=0)
			{
				value = 0;
				col2D.offset = new Vector2(0,0.5f);
				col2D.size = new Vector2(0.55f,1f);
			}

			else if(axis.verAxis==0f && Mathf.Abs(axis.horAxis)==1f && !axis.acceptFakeInputs && value!=1|| axis.acceptFakeInputs && Mathf.Abs(axis.artificialX)>0.1f && value!=1)
			{
				value = 1;
				col2D.offset = new Vector2(0,1);
				col2D.size = new Vector2(0.7f,0.1f);
			}
		}
		if(pSprites.state>=1)
		{
			//vertical inputs take priority
			if(Mathf.Abs(axis.verAxis)==1f && !axis.acceptFakeInputs && value!=2|| axis.acceptFakeInputs && Mathf.Abs(axis.artificialY)>0.1f && value!=2)
			{
				value = 2;
				col2D.offset = new Vector2(0,0.8f);
				col2D.size = new Vector2(0.55f,1.5f);
			}

			else if(axis.verAxis==0f && Mathf.Abs(axis.horAxis)==1f && !axis.acceptFakeInputs && value!=3|| axis.acceptFakeInputs && Mathf.Abs(axis.artificialX)>0.1f && value!=3)
			{
				value = 3;
				col2D.offset = new Vector2(0,1);
				col2D.size = new Vector2(0.7f,0.1f);
			}
		}
	}
	IEnumerator shakeDoor(Animation anim,Vector3 pos)
	{
		//print("Shake");
		data.playSoundOverWrite(116,pos);
		anim.Play();
		canOpen = false;
		yield return new WaitForSeconds(0.25f);
		canOpen = true;
	}
	void checkKeyDoor(DoorScript dScript,SpriteRenderer render, SpriteRenderer render2,bool isExit, Vector3 pos,Animation a)
	{
		if((dScript.keyID==0&&data.hasRed)||
		(dScript.keyID==1&&data.hasBlue)||
		(dScript.keyID==2&&data.hasYellow))
			StartCoroutine(enterDoor(pos,dScript,isExit,render,render2));
			
		else
			StartCoroutine(shakeDoor(a,pos));
	}
	void OnTriggerStay2D(Collider2D other)
	{
		if(!inWarpCollider)
		if(other.tag=="DoorWarp" || other.tag=="Warp" && other.name.Contains("Up"))
			inWarpCollider = true;
		

		if(other.tag=="DoorWarp"&&Time.timeScale!=0&&pScript.grounded&&canOpen&&camScript.fadeAnim<=0)
		{
			DoorScript dScript = other.transform.parent.GetComponent<DoorScript>();
			SpriteRenderer render = other.GetComponent<SpriteRenderer>(),
			render2 = other.transform.parent.GetChild(1).GetComponent<SpriteRenderer>(),
			render3 = other.transform.parent.GetChild(0).GetComponent<SpriteRenderer>();
			if(other.name.Contains("Entrance")&&axis.verAxis==1&&!midWarp
			||other.name.Contains("Exit")&&axis.verAxis==1&&!midWarp
			||other.name.Contains("Entrance")&&dScript.touchToWarp&&!midWarp
			||other.name.Contains("Exit")&&dScript.touchToWarp&&!midWarp)
			{
				bool isExit = false;
				if(other.name.Contains("Exit"))
					isExit = true;

				//Debug.Log("Entered "+ other.transform.parent.name+" "+other.name);
				camScript.cameraPointFreeze();
				//print(isExit);
				if(!isExit)
				{
					if(dScript.eventLocked)
					{
						//print("A");
						StartCoroutine(shakeDoor(other.GetComponent<Animation>(),other.transform.position));
					}
					else if(!other.transform.parent.name.Contains("KeyDoor"))
					{
						//print("B");
						StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render2));
					}
					else
					{
						//print("C");
						if(canOpen)
						checkKeyDoor(dScript,render,render2,isExit,other.transform.position,other.GetComponent<Animation>());
						/*
						if(dScript.keyID==0&&data.hasRed&&canOpen)
						StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render2));
						else if(dScript.keyID==1&&data.hasBlue&&canOpen)
						StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render2));
						else if(dScript.keyID==2&&data.hasYellow&&canOpen)
						StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render2));
						//locked door
						else
						{
							if(canOpen)
							StartCoroutine(shakeDoor(other.GetComponent<Animation>()));
						}*/
					}
				}
				else
				{	
					//print("ex");
					if(!other.transform.parent.name.Contains("KeyDoor"))
					{
						if(dScript.eventLocked||!dScript.twoSided)
						{
							if(!dScript.touchToWarp)
							StartCoroutine(shakeDoor(other.GetComponent<Animation>(),other.transform.position));
						}
						else
						{
							//print("1");
							StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render3));
						}
					}
					else
					{
						if(canOpen)
						checkKeyDoor(dScript,render,render3,isExit,other.transform.position,other.GetComponent<Animation>());
						/*
						if(dScript.keyID==0&&data.hasRed&&canOpen)
						StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render3));
						else if(dScript.keyID==1&&data.hasBlue&&canOpen)
						StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render3));
						else if(dScript.keyID==2&&data.hasYellow&&canOpen)
						StartCoroutine(enterDoor(other.transform.position,dScript,isExit,render,render3));
						//locked door
						else
						{
							if(canOpen)
							StartCoroutine(shakeDoor(other.GetComponent<Animation>()));
						}
						*/
					}
				}
			}
		}
		if(other.tag=="Warp"&&Time.timeScale!=0)
		{
				if(other.name.Contains("Entrance")&&axis.horAxis==1&&other.name.Contains("Right")&&!axis.acceptFakeInputs&&!midWarp&pScript.grounded
				||other.name.Contains("Entrance")&&!midWarp && axis.acceptFakeInputs && axis.artificialX == 1f&pScript.grounded
				||
				  other.name.Contains("Exit")&&axis.horAxis==-1&&other.name.Contains("Right")&&!axis.acceptFakeInputs&&!midWarp&pScript.grounded
				||other.name.Contains("Exit")&&!midWarp && axis.acceptFakeInputs && axis.artificialX == -1f&pScript.grounded)
				{
					bool isExit = false;
					if(other.name.Contains("Exit"))
						isExit = true;
						
					//Debug.Log("Entered "+ other.transform.parent.name+" "+other.name);
					StartCoroutine(enterWarp(3,other.transform.position,other.transform.parent.GetComponent<WarpScript>(),isExit));
				}
				if(other.name.Contains("Entrance")&&axis.horAxis==-1&&other.name.Contains("Left")&&!axis.acceptFakeInputs&&!midWarp&pScript.grounded
				||other.name.Contains("Entrance")&&!midWarp && axis.acceptFakeInputs && axis.artificialX == -1f&pScript.grounded
				||
				  other.name.Contains("Exit")&&axis.horAxis==1&&other.name.Contains("Left")&&!axis.acceptFakeInputs&&!midWarp&pScript.grounded
				||other.name.Contains("Exit")&&!midWarp && axis.acceptFakeInputs && axis.artificialX == 1f&pScript.grounded
				)
				{
					bool isExit = false;
					if(other.name.Contains("Exit"))
						isExit = true;
					//Debug.Log("Entered "+ other.transform.parent.name+" "+other.name);
					StartCoroutine(enterWarp(2,other.transform.position,other.transform.parent.GetComponent<WarpScript>(),isExit));
				}
				if(other.name.Contains("Entrance")&&axis.verAxis==1&&other.name.Contains("Up")&&!axis.acceptFakeInputs&&!midWarp
				||other.name.Contains("Entrance")&&!midWarp && axis.acceptFakeInputs && axis.artificialY == 1f
				||
				  other.name.Contains("Exit")&&axis.verAxis==-1&&other.name.Contains("Up")&&!axis.acceptFakeInputs&&!midWarp
				||other.name.Contains("Exit")&&!midWarp && axis.acceptFakeInputs && axis.artificialY == -1f)
				{
					bool isExit = false;
					if(other.name.Contains("Exit"))
					{
						isExit = true;
					}
					//Debug.Log("Entered "+ other.transform.parent.name+" "+other.name);
					StartCoroutine(enterWarp(1,other.transform.position,other.transform.parent.GetComponent<WarpScript>(),isExit));
				}
				if(other.name.Contains("Entrance")&&axis.verAxis==-1&&other.name.Contains("Down")&&!axis.acceptFakeInputs&&!midWarp
				||other.name.Contains("Entrance")&&!midWarp && axis.acceptFakeInputs && axis.artificialY == -1f
				||
				  other.name.Contains("Exit")&&axis.verAxis==1&&other.name.Contains("Down")&&!axis.acceptFakeInputs&&!midWarp
				||other.name.Contains("Exit")&&!midWarp && axis.acceptFakeInputs && axis.artificialY == 1f
				)
				{
					bool isExit = false;
					if(other.name.Contains("Exit"))
						isExit = true;
					//Debug.Log("Entered "+ other.transform.parent.name+" "+other.name);
					StartCoroutine(enterWarp(0,other.transform.position,other.transform.parent.GetComponent<WarpScript>(),isExit));
				}
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(inWarpCollider)
			if(other.tag=="DoorWarp" || other.tag=="Warp" && other.name.Contains("Up"))
				inWarpCollider = false;
	}
	IEnumerator enterDoor(Vector3 doorPos,DoorScript usedWarp,bool isExit,SpriteRenderer door1Render,SpriteRenderer door2Render)
	{
		//print("Entering door");
		//yield return new WaitUntil(()=>camScript.fadeAnim<=0);
		usedWarp.StartEventTriggered();
		Time.timeScale=0;
		midWarp = true;
		pScript.pauseMenu.enabled = false;
		data.timerGoesDown = false;
		Transform player = transform.parent.transform.parent;
		Animator anim = player.transform.GetChild(0).GetComponent<Animator>();
		Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
		rb.velocity = Vector2.zero;
		player.eulerAngles = Vector3.zero;
		pScript.anim.SetFloat("HorSpeed",Mathf.Abs(rb.velocity.x));
		col.enabled = false;
		Vector3 animationGoal = player.position;
		float zPos = player.position.z;
		Transform heldObj = null;
		if(pScript.heldObject!=null&& pScript.heldObject.GetComponent<shellScript>()!=null)
		{
			heldObj = pScript.heldObject;
			heldObj.GetComponent<shellScript>().stopTwitching();
		}
		anim.SetBool("dive",false);
		anim.SetBool("dive",false);
		if(pScript.canInterruptDive)
		{
			pScript.interruptDive = true;
			if(pScript.diveCor!=null)
			{
				pScript.StopCoroutine(pScript.diveCor);
				pScript.diveCor = null;
			}
		}
		rb.velocity = Vector2.zero;
		axis.acceptXInputs = false;
		axis.acceptYInputs = false;
		axis.axisPosX = 0;
		axis.axisPosY = 0;
		grav.enabled = false;
		pScript.disableDust();
		pScript.enabled = false;
		pScript.render.enabled = true;
		data.sMeterWorks = false;
		data.litSMeterArrows = 0;
		data.UpdateSMeterArrow();
		if(door1Render.enabled)
		data.playSound(39,transform.position);
		bool isKey = false;
		//normal door
		if(door1Render.sprite==regularDoorSprite[0])
		for(int i = 0; i<=10; i++)
		{
			yield return 0;
			if(i==5)
			{
				door1Render.sprite = regularDoorSprite[1];
			}
			else if(i==10)
			{
				door1Render.sprite = regularDoorSprite[2];
			}
		}
		//key door
		else if(door1Render.sprite==keyDoorSprite[0])
		{
		isKey = true;
		for(int i = 0; i<=10; i++)
		{
			yield return 0;
			if(i==5)
			{
				door1Render.sprite = keyDoorSprite[1];
			}
			else if(i==10)
			{
				door1Render.sprite = regularDoorSprite[2];
			}
		}
		}
		camScript.lockCamera=true;
		camScript.fadeScreen(true);
		yield return new WaitUntil(()=>camScript.fadeAnim>=1f);
		camScript.hardResetVelocity(true);
		//Debug.Log("Going to the other door");
		if(!isExit||isExit&&usedWarp.alwaysUseDoorEvent)
		usedWarp.DoorEventTriggered();
		else
		usedWarp.ExitEventTriggered();
		bool isSub = false;
		if(pScript.canInterruptDive)
			pScript.interruptDive = true;
		if(usedWarp.leadsToSubArea&&!isExit)
		{
			isSub = true;
		}
		data.switchArea(isSub);
		Vector3 camPos;
		if(isExit)
		{
			 camPos = usedWarp.entranceCameraPosition;
		}
		else camPos = usedWarp.exitCameraPosition;
		if(camPos!=Vector3.zero)
		camPos = new Vector3(camPos.x,camPos.y,cam.transform.position.z);
		else
		{
			Vector2 guessPos;
			if(!isExit)
			guessPos = usedWarp.transform.GetChild(1).transform.position;
			else guessPos = usedWarp.transform.GetChild(0).transform.position;
			camPos = new Vector3(guessPos.x,guessPos.y+camScript.verticalOffset,-10);
		}
		if(usedWarp.camStayInBounds)
		camScript.setPosition(camPos);
		else cam.transform.position = camPos;

		data.switchParallax(isSub);
		if(!isExit)
		player.position = usedWarp.transform.GetChild(1).position+new Vector3(0,0,zPos);
		else player.position = usedWarp.transform.GetChild(0).position+new Vector3(0,0,zPos);
		if(door1Render.sprite==regularDoorSprite[2]&&!isKey)
			door1Render.sprite=regularDoorSprite[0];
		else if(door1Render.sprite==regularDoorSprite[2]&&isKey)
			door1Render.sprite=keyDoorSprite[0];

		camScript.enabled = true;
		data.sMeterWorks = true;
		camScript.hardResetVelocity(false);
		Time.timeScale=1f;
		yield return 0;
		yield return 0;
		Time.timeScale=0;
		camScript.fadeScreen(false);
		if(door2Render.sprite==regularDoorSprite[0])
		{
		door2Render.sprite=regularDoorSprite[2];
		yield return new WaitUntil(()=>camScript.fadeAnim<=0f);
		if(door2Render.enabled)
		data.playSound(40,transform.position);
		for(int i = 0; i<=10; i++)
		{
			yield return 0;
			if(i==5) door2Render.sprite = regularDoorSprite[1];
			else if(i==10) door2Render.sprite = regularDoorSprite[0];
			}
		}

		else if(door2Render.sprite==keyDoorSprite[0])
		{
		door2Render.sprite=regularDoorSprite[2];
		yield return new WaitUntil(()=>camScript.fadeAnim<=0f);
		if(door2Render.enabled)
		data.playSound(40,transform.position);
		for(int i = 0; i<=10; i++)
		{
			yield return 0;
			if(i==5) door2Render.sprite = keyDoorSprite[1];
			else if(i==10) door2Render.sprite = keyDoorSprite[0];
			}
		}
		anim.SetBool("crouch",false);
		if(!usedWarp.keepAudioEffects)
		{
			int eff = 0;
			if(isExit)
			{
				if(data.muffleInMainArea)eff+=1;
				if(data.echoInMainArea)eff+=2;

				data.inSub = false;
			}
			else if(!isExit && usedWarp.leadsToSubArea)
			{
				if(data.muffleInSubArea)eff+=1;
				if(data.echoInSubArea)eff+=2;

				data.inSub = true;
			}
			if(usedWarp.leadsToSubArea&&!isExit)data.inSub = true;
			else data.inSub = false;
			data.audioEffectsToggle(eff);
		}
		Time.timeScale=1;
		camScript.lockCamera = usedWarp.keepCamLocked;
		pScript.platHeight = cam.transform.position.y;
		if(usedWarp.freePlayerOnExit)
		{
			axis.acceptYInputs = true;
			
			axis.acceptXInputs = true;
		}
		yield return 0;
		if(heldObj!=null&&pScript.heldObject!=null) heldObj.GetComponent<shellScript>().startTwitching();
		col.enabled = true;
		grav.enabled = true;
		pScript.enabled = true;
		pScript.running = false;
		if(pScript.axis.axisAdder==0.025f)
		pScript.axis.axisAdder = 0.07f;
		pScript.axis.axisPosX = 0;
		data.timerGoesDown = true;
		midWarp = false;
		pScript.pauseMenu.enabled = true;
	}
	IEnumerator enterWarp(int direction,Vector3 arrowPos,WarpScript usedWarp,bool isExit)
	{
		pScript.transform.GetChild(0).eulerAngles = Vector3.zero;
		midWarp = true;
		pScript.pauseMenu.enabled = false;
		data.timerGoesDown = false;
		Transform player = transform.parent.transform.parent;
		Animator anim = player.transform.GetChild(0).GetComponent<Animator>();
		Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
		anim.SetBool("dive",false);
		if(pScript.canInterruptDive)
		{
			pScript.interruptDive = true;
			/*if(pScript.diveCor!=null)
			{
				pScript.StopCoroutine(pScript.diveCor);
				pScript.diveCor = null;
			}*/
		}
		rb.velocity = Vector2.zero;
		axis.acceptXInputs = false;
		axis.acceptYInputs = false;
		axis.axisPosX = 0;
		axis.axisPosY = 0;
		pScript.disableDust();
		grav.enabled = false;
		pScript.enabled = false;
		pScript.render.enabled = true;
		anim.speed = 1;
		col.enabled = false;
		col.transform.parent.GetComponent<SortingGroup>().sortingLayerName = "Default";
		col.transform.parent.GetComponent<SortingGroup>().sortingOrder = -2;
		Vector3 animationGoal = player.position;
		float Zpos = player.position.z;
		Transform heldObj = null;
		if(pScript.heldObject!=null&& pScript.heldObject.GetComponent<shellScript>()!=null)
		{
			heldObj = pScript.heldObject;
			heldObj.GetComponent<shellScript>().stopTwitching();
		}
		data.sMeterWorks = false;
		data.litSMeterArrows = 0;
		data.UpdateSMeterArrow();
		// 0 = down, 1 = up, 2 = left, 3 = right
		if(!isExit)
		{
			switch(direction)
			{
				default:animationGoal+=new Vector3(-player.position.x+arrowPos.x,-2,0);anim.SetFloat("HorSpeed",0); anim.SetFloat("VerSpeed",1); break;
				case 1: animationGoal+=new Vector3(-player.position.x+arrowPos.x,2,0); anim.SetFloat("HorSpeed",0); anim.SetFloat("VerSpeed",1); break;
				case 2: animationGoal+=new Vector3(-1.5f,0.25f,0); break;
				case 3: animationGoal+=new Vector3(1.5f,0.25f,0); break;
			}
		}
		else
		{
			switch(direction)
			{
				default:animationGoal+=new Vector3(-player.position.x+arrowPos.x,2,0);anim.SetFloat("HorSpeed",0); anim.SetFloat("VerSpeed",1); break;
				case 1: animationGoal+=new Vector3(-player.position.x+arrowPos.x,-2,0); anim.SetFloat("HorSpeed",0); anim.SetFloat("VerSpeed",1); break;
				case 2: animationGoal+=new Vector3(1.5f,0.25f,0); break;
				case 3: animationGoal+=new Vector3(-1.5f,0.25f,0); break;
			}
		}
		data.playSound(3,transform.position);
		animationGoal+=new Vector3(0,0,Zpos);
		while(player.position!=animationGoal)
		{

			if(direction<=1) //only up and down
				player.position = Vector3.MoveTowards(player.position,new Vector3(animationGoal.x,player.position.y,player.position.z),animationGoalSpeed*2*Time.deltaTime);
			player.position = Vector3.MoveTowards(player.position,animationGoal,animationGoalSpeed*Time.deltaTime);
			yield return 0;
		}
		yield return new WaitUntil(()=> player.position == animationGoal);

		camScript.fadeScreen(true);
		camScript.lockCamera = true;
		yield return new WaitUntil(()=>camScript.fadeAnim>=1f);
		camScript.hardResetVelocity(true);
		//Debug.Log("Screen transition here");
		bool isSub = false;
		if(usedWarp.leadsToSubArea&&usedWarp.twoSided&&!isExit
		||usedWarp.leadsToSubArea&&!usedWarp.twoSided)
		{
			isSub = true;
		}
		data.switchArea(isSub);
		Vector3 camPos;
		if(isExit)
		{
			 camPos = usedWarp.entranceCameraPosition;
		}
		else camPos = usedWarp.exitCameraPosition;

		if(camPos!=Vector3.zero)
		camPos = new Vector3(camPos.x,camPos.y,cam.transform.position.z);
		else
		{
			Vector2 guessPos;
			if(!isExit)
			guessPos = usedWarp.transform.GetChild(1).transform.position;
			else guessPos = usedWarp.transform.GetChild(0).transform.position;
			camPos = new Vector3(guessPos.x,guessPos.y+camScript.verticalOffset,-10);
		}
		
		if(usedWarp.camStayInBounds)
		camScript.setPosition(camPos);
		else cam.transform.position = camPos;
		data.switchParallax(isSub);
		pScript.grounded = false;
		//go to the exit

		if(!isExit)
		direction = usedWarp.directionExit;
		else direction = usedWarp.directionEntrance;
		Vector3 offset = Vector3.zero;
		if(isExit)
		{
			switch(direction)
			{
				default:offset=new Vector3(0,-2,0); break;
				case 2: offset=new Vector3(0,0.5f,0); break;
				case 1: offset=new Vector3(-1,-1f,0); break;
				case 3: offset=new Vector3(1,-1f,0); break;
			}
		}
		if(!isExit)
		{
			switch(direction)
			{
				default:offset=new Vector3(0,0.5f,0); break;
				case 2: offset=new Vector3(0,-2,0); break;
				case 1: offset=new Vector3(1,-1f,0); break;
				case 3: offset=new Vector3(-1,-1f,0); break;
			}
		}
		//anim.SetFloat("VerSpeed",0);
		if(!isExit)
		{
			player.position = usedWarp.transform.GetChild(1).position+offset+new Vector3(0,0,Zpos);
		}
		else
		{
			player.position = usedWarp.transform.GetChild(0).position+offset+new Vector3(0,0,Zpos);
		}
		if(!isExit)
		{
			switch(direction)
			{
				default: player.localScale = new Vector3(1,transform.localScale.y,transform.localScale.z); break;
				case 1: player.localScale = new Vector3(-1,transform.localScale.y,transform.localScale.z); break;
			}
		}
		else
		{
			switch(direction)
			{
				default: player.localScale = new Vector3(1,transform.localScale.y,transform.localScale.z); break;
				case 3: player.localScale = new Vector3(-1,transform.localScale.y,transform.localScale.z); break;
			}
		}
		animationGoal = player.position+new Vector3(0,0,Zpos);
		float playerExtend = 0;
		if(pSprites.state >= 1)
		{
			playerExtend = 0.5f;
		}
		if(isExit)
		{
			//Debug.Log(direction+" "+isExit);
			switch(direction)
			{
				default:animationGoal+=new Vector3(0,1.5f,0);anim.SetFloat("HorSpeed",0); anim.SetBool("Grounded",true);break;
				case 2: animationGoal+=new Vector3(0,-1f-playerExtend,0);anim.SetFloat("HorSpeed",0);anim.SetBool("Grounded",false);break;
				case 1: animationGoal+=new Vector3(0.8f,0f,0);anim.SetFloat("HorSpeed",2); anim.SetBool("Grounded",true);break;
				case 3: animationGoal+=new Vector3(-0.8f,0f,0);anim.SetFloat("HorSpeed",2); anim.SetBool("Grounded",true);break;
			}
		}
		if(!isExit)
		{
			//Debug.Log(direction+" "+isExit);
			switch(direction)
			{
				default:animationGoal+=new Vector3(0,-1f-playerExtend,0);anim.SetFloat("HorSpeed",0); anim.SetBool("Grounded",false);break;
				case 2: animationGoal+=new Vector3(0,1.5f,0);anim.SetFloat("HorSpeed",0);anim.SetBool("Grounded",true);break;
				case 1: animationGoal+=new Vector3(-0.8f,0f,0);anim.SetFloat("HorSpeed",2); anim.SetBool("Grounded",true);break;
				case 3: animationGoal+=new Vector3(0.8f,0f,0);anim.SetFloat("HorSpeed",2); anim.SetBool("Grounded",true);break;
			}
		}
		anim.SetBool("crouch",false);

		int eff = 0;
		if(isExit)
		{
			if(data.muffleInMainArea)eff+=1;
			if(data.echoInMainArea)eff+=2;

			data.inSub = false;
		}
		else if(!isExit && usedWarp.leadsToSubArea)
		{
			if(data.muffleInSubArea)eff+=1;
			if(data.echoInSubArea)eff+=2;

			data.inSub = true;
		}
		if(usedWarp.leadsToSubArea&&!isExit)data.inSub = true;
		else data.inSub = false;
		data.audioEffectsToggle(eff);
		data.playSound(3,transform.position);
		camScript.enabled = true;
		camScript.fadeScreen(false);
		while(player.position!=animationGoal)
		{
			if(direction==2&&!isExit||direction==0&&isExit) //only up
			player.position = Vector3.MoveTowards(player.position,animationGoal,animationGoalSpeed*Time.deltaTime);
			else player.position = Vector3.MoveTowards(player.position,animationGoal,(animationGoalSpeed*0.75f)*Time.deltaTime);
			yield return 0;
		}
		yield return new WaitUntil(()=> player.position == animationGoal);
		yield return new WaitUntil(()=>camScript.fadeAnim<=0f);
		camScript.hardResetVelocity(false);
		data.sMeterWorks = true;
		camScript.lockCamera = false;
		col.transform.parent.GetComponent<SortingGroup>().sortingLayerName = "Player";
		col.transform.parent.GetComponent<SortingGroup>().sortingOrder = 0;
		axis.acceptYInputs = true;
		
		axis.acceptXInputs = true;
		yield return 0;
		if(heldObj!=null&&pScript.heldObject!=null) heldObj.GetComponent<shellScript>().startTwitching();
		
		if(pScript.diveCor!=null)
		{
			pScript.StopCoroutine(pScript.diveCor);
			pScript.diveCor = null;
		}
		col.enabled = true;
		grav.enabled = true;
		pScript.enabled = true;
		pScript.running = false;
		if(pScript.axis.axisAdder==0.025f)
		pScript.axis.axisAdder = 0.07f;
		pScript.axis.axisPosX = 0;
		data.timerGoesDown = true;
		midWarp = false;
		pScript.pauseMenu.enabled = true;
	}
}
