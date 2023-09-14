using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class holdableBlockScript : MonoBehaviour {
	Rigidbody2D rb;
	Collider2D physBox;
	public GameObject defaultParent;
	public GameObject ExplosionObj;
	bool isParented = false;
	public bool explosive = true;
	public bool canExplode = false;
	public bool canShatter = false;
	checkForSemiSolid checker;
	public LayerMask whatIsGround;
	CompositeCollider2D semiSolid;
	GameData data;
	MGCameraController camControl;
	bool inverted = false;
	public int streak = 0;
	public Sprite impact;
	SpriteRenderer render;
	Animation anim;
	public Sprite[] normalSprites = new Sprite[2];
	public int soundInt = 58;
	public float blockHardness = 8f;
	public bool isHazard = false;
	public bool dropLogic = false;
	bool dropped = false;
	public bool visible = false;
	string curParentName = "";
	AudioSource asc;
	public bool triggerCountdown = false;
	Coroutine cor;
	Color oldColor;
	bool lockVis = false;
	bool groundTouch = false,lockGroundTouch = false;
	public bool exploded = false;
	Gravity grav;
	string[] grounds = new string[6]
	{
		"Ground","blockHoldable","semiSolid","bigBlock","Harm","NPC"
	};
	public bool bossActivated = false;
	int spawnFrames = 10;
	int ungroundedFrames = 0;
	public int ungroundedFramesBeforeExplode = 10;
	public int saveDropFrames = 0;
	bool canDetectGround = true;
	bool ignoreBall = false;
	bool inLava = false;
	public bool yeetThrow = false;
	public GameObject steamParticles;
	IEnumerator dieInLava()
	{
		yield return new WaitForSeconds(4f);
		unparentAll();
		if(gameObject.activeInHierarchy) data.StartCoroutine(DestroyQueue());
	}
	bool ignoreSemiSolid = false;
	float lastXVelo = 0;
	public bool deathzoneProof = false;
	platformStick pl;
	void instantiateExplosion(Vector3 point)
	{
		if(data.explodeTile(point,true))
		{
			GameObject obj;
			if(data.explodeTile(point,true))
			{
				obj = Instantiate(ExplosionObj,point,Quaternion.identity);
				obj.name="ScreenNuke";
				obj.SetActive(true);
			}
		}
	}
	public void Explode()
	{
		if(ExplosionObj==null)
		{
			lockVis = true;
			canExplode = false;
			exploded = true;
			anim.Play();
			data.playSoundOverWrite(53,transform.position);
			GetComponent<Gravity>().enabled = false;
			render.sprite = impact;
			rb.velocity = Vector2.zero;
			physBox.enabled = false;
			camControl.shakeCamera(0.2f,0.5f);
			camControl.nukeEvent = true;
		}
		else
		{
			Vector3Int posInt = new Vector3Int(Mathf.RoundToInt((transform.position.x-0.5f)),Mathf.RoundToInt((transform.position.y-0.5f)),Mathf.RoundToInt(transform.position.z));
			Vector3 spawnPos = new Vector3(posInt.x+0.5f,posInt.y+0.5f,posInt.z);
			//print("Middle: "+posInt);
			canExplode = false;
			exploded = true;
			GetComponent<Gravity>().enabled = false;
			if(rb.bodyType!=RigidbodyType2D.Static)
			rb.velocity = Vector2.zero;
			physBox.enabled = false;
			instantiateExplosion(spawnPos+new Vector3(-1,1,0));
			instantiateExplosion(spawnPos+new Vector3(0,1,0));
			instantiateExplosion(spawnPos+new Vector3(1,1,0));
			instantiateExplosion(spawnPos+new Vector3(-1,0,0));
			instantiateExplosion(spawnPos);
			instantiateExplosion(spawnPos+new Vector3(1,0,0));
			instantiateExplosion(spawnPos+new Vector3(-1,-1,0));
			instantiateExplosion(spawnPos+new Vector3(0,-1,0));
			instantiateExplosion(spawnPos+new Vector3(1,-1,0));
			data.playSoundOverWrite(68,transform.position);
			camControl.shakeCamera(0.1f,0.5f);
			unparentAll();
			if(gameObject.activeInHierarchy) data.StartCoroutine(DestroyQueue());
		}
	}
	public void unparentAll()
	{
		int allowedCount = explosive?2:3;
		//for(int i = 0;i<transform.childCount;i++)print("Main: "+transform.name+" "+i+" "+transform.GetChild(i).name);
		//print(transform.name+" i = "+(transform.childCount-1)+" i>="+allowedCount);
		for(int i = transform.childCount-1;i>=allowedCount;i--)
		{
			//print("Unparented: "+transform.GetChild(i).name);
			pl.unparent(transform.GetChild(i));
		}
	}
	// Use this for initialization
	void Start ()
	{
		data = GameObject.Find("_GM").GetComponent<GameData>();
		rb = GetComponent<Rigidbody2D>();
		physBox = GetComponent<Collider2D>();
		if(defaultParent==null)defaultParent = GameObject.Find("Special");
		checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
		semiSolid = data.semiSolid;
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		asc = GetComponent<AudioSource>();
		oldColor = render.color;
		grav = GetComponent<Gravity>();
		pl = GetComponent<platformStick>();
		if(explosive)
		{
			camControl = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
			anim = transform.GetChild(0).GetComponent<Animation>();
		}
		if(Mathf.Round(transform.eulerAngles.z)==180)
		{
			inverted = true;
			grav.pushForces = new Vector2(grav.pushForces.x,Mathf.Abs(grav.pushForces.y));
		}
		toggleVisible(visible);
		killBlock();
	}
	IEnumerator Countdown()
	{
		int phaseframes = 240,beepframes = 0;
		while(phaseframes>0)
		{
			if(phaseframes%60==0)
			{
				beepframes = 2;
				if(asc.isPlaying)asc.Stop();
				asc.Play();
				render.color = Color.red;
				while(beepframes>0)
				{
					beepframes--;
					yield return 0;
				}
				render.color = oldColor;
			}
			else
			{
				yield return 0;
			}
			phaseframes--;
			if(Time.timeScale==0)
			yield return new WaitUntil(()=>Time.timeScale!=0);
		}
		phaseframes = 120;
		while(phaseframes>0)
		{
			if(phaseframes%30==0)
			{
				beepframes = 2;
				if(asc.isPlaying)asc.Stop();
				asc.Play();
				render.color = Color.red;
				while(beepframes>0)
				{
					beepframes--;
					yield return 0;
				}
				render.color = oldColor;
			}
			else
			{
				yield return 0;
			}
			phaseframes--;
			if(Time.timeScale==0)
			yield return new WaitUntil(()=>Time.timeScale!=0);
		}
		phaseframes = 60;
		while(phaseframes>0)
		{
			if(phaseframes%10==0)
			{
				beepframes = 2;
				if(asc.isPlaying)asc.Stop();
				asc.Play();
				render.color = Color.red;
				while(beepframes>0)
				{
					beepframes--;
					yield return 0;
				}
				render.color = oldColor;
			}
			else
			{
				yield return 0;
			}
			phaseframes--;
			if(Time.timeScale==0)
			yield return new WaitUntil(()=>Time.timeScale!=0);
		}
		phaseframes = 30;
		while(phaseframes>0)
		{
			if(phaseframes%5==0)
			{
				beepframes = 2;
				if(asc.isPlaying)asc.Stop();
				asc.Play();
				render.color = Color.red;
				while(beepframes>0)
				{
					beepframes--;
					yield return 0;
				}
				render.color = oldColor;
			}
			else
			{
				yield return 0;
			}
			phaseframes--;
			if(Time.timeScale==0)
			yield return new WaitUntil(()=>Time.timeScale!=0);
		}
		//print("1");
		Explode();
	}
	void crusher()
	{
		RaycastHit2D rayLeft = Physics2D.Raycast(transform.position,-transform.right,0.45f,whatIsGround);
		RaycastHit2D rayRight = Physics2D.Raycast(transform.position,transform.right,0.45f,whatIsGround);
		RaycastHit2D rayDown = Physics2D.Raycast(transform.position,-transform.up,0.45f,whatIsGround);
		RaycastHit2D rayUp = Physics2D.Raycast(transform.position,transform.up,0.45f,whatIsGround);
		if(rayLeft.collider!=null&rayRight.collider!=null&&rayLeft.collider.transform!=rayRight.collider.transform
		||rayUp.collider!=null&rayDown.collider!=null&&rayUp.collider.transform!=rayDown.collider.transform)
		{
			if(explosive&&!exploded)
			{
				Explode();
			}
			else
			{
				HoldBlockBust();
			}
		}
	}
	public void HoldBlockBust()
	{
		if(transform.childCount>2)
		{
			GameObject obj = transform.GetChild(2).gameObject;
			var p = obj.GetComponent<ParticleSystem>().main;
			p.startColor = render.color;
			unparentAll();
			obj.transform.SetParent(null);
			data.playSoundOverWrite(soundInt,transform.position);
			if(obj.GetComponent<SortingGroup>()==null)
			{
				SortingGroup gr = obj.AddComponent<SortingGroup>();
				gr.sortingLayerName = "Default";
				gr.sortingOrder = -1;
			}
			obj.SetActive(true);
			if(gameObject.activeInHierarchy) data.StartCoroutine(DestroyQueue());
		}
	}
	void LateUpdate()
	{
		if(visible&&!groundTouch&&!isParented&&spawnFrames==0&&Time.timeScale!=0)
		{
			if(Mathf.Round(Mathf.Abs(rb.velocity.x))>=1f)
			lastXVelo = Mathf.Abs(rb.velocity.x);
			if(!groundTouch&&!isParented)
			{
				if(saveDropFrames<=0)
				{
					if(rb.velocity.y<0&&!inverted||rb.velocity.y>0&&inverted)
					ungroundedFrames++;
				}
				else saveDropFrames--;
			}
		}
	}
	void Update()
	{
		if(Time.timeScale!=0&&spawnFrames>0)
		{
			spawnFrames--;
		}
		if(transform.parent!=null&&isParented&&transform.parent.name==curParentName)
		{
			Vector3 p = transform.lossyScale;
			if(transform.lossyScale.x==-1)
			{
				p = transform.localScale;
				transform.localScale = new Vector3(-p.x,p.y,p.z);
				p = transform.localScale;

			}
			if(curParentName=="blockParent")
			render.transform.localScale = p;
		}
		else
		{
			if(dropLogic&&!dropped)
				{
					dropped = true;
					if(explosive)
						canExplode = true;
					else canShatter = true;
					transform.tag = "Untagged";
					transform.GetChild(0).tag = "Untagged";
					if(transform.localScale.y==1) blockParent(false,Vector3.zero,new Vector2(0,5),false);
					else blockParent(false,Vector3.zero,new Vector2(0,-5),false);
				}
			if(visible)
			{
				if(physBox.enabled)
				{
					crusher();
					if(!isParented&&canDetectGround&&groundTouch&&(explosive||!canShatter))
					{
						rb.velocity = new Vector2(0,rb.velocity.y);
					}
				}
				if(Mathf.Round(transform.eulerAngles.z)!=0&&!inverted) inverted=true;
				else if(Mathf.Round(transform.eulerAngles.z)==0&&inverted) inverted=false;
					if(!inverted)
					{
						if(rb.velocity.y>0&&!ignoreSemiSolid
						||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
						{
							ignoreSemiSolid = true;
							Physics2D.IgnoreCollision(semiSolid, physBox,true);
							whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
							//Debug.Log(gameObject.name+" Ignoring collision");
						}
						else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid
						||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
						{
							ignoreSemiSolid = false;
							Physics2D.IgnoreCollision(semiSolid, physBox,false);
							whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
							//Debug.Log(gameObject.name+" Not ignoring collision");
						}
					}
					else
					{
						if(rb.velocity.y<0&&!ignoreSemiSolid
						||rb.velocity.y<0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
						{
							ignoreSemiSolid = true;
							Physics2D.IgnoreCollision(semiSolid, physBox,true);
							whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
							//Debug.Log(gameObject.name+" Ignoring collision");
						}
						else if(rb.velocity.y>=0&&ignoreSemiSolid&&checker.insideSemiSolid
						||rb.velocity.y>=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
						{
							ignoreSemiSolid = false;
							Physics2D.IgnoreCollision(semiSolid, physBox,false);
							whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
							//Debug.Log(gameObject.name+" Not ignoring collision");
						}
					}
			}
		}
	}
	void OnDisable()
	{
		if(ignoreSemiSolid)
		{
			ignoreSemiSolid = false;
			if(semiSolid!=null&&physBox!=null)
			Physics2D.IgnoreCollision(semiSolid, physBox,false);
			whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
			//Debug.Log(gameObject.name+" Not ignoring collision");
		}
	}
	void OnCollisionEnter2D(Collision2D other)
	{
		//print(other.gameObject.name);
		if(data.reachedGoal&&other.gameObject.name=="Player_main")
		{
			Collider2D col = other.transform.GetChild(0).GetComponent<Collider2D>();
			if(physBox!=null&&col!=null)
			Physics2D.IgnoreCollision(physBox,col,true);
		}
	}
	void OnCollisionStay2D(Collision2D other)
	{
		if(!inLava)
		{
		for(int j = 0; j<other.contacts.Length;j++)
		{
			if((!groundTouch||(groundTouch&&!explosive))&&(!isParented&&canDetectGround))
			for(int i = 0; i<grounds.Length;i++)
			{
				RaycastHit2D ray1;
				RaycastHit2D ray2;
				if(explosive||!canShatter||yeetThrow)
					{
						ray1 = Physics2D.Raycast(transform.position+new Vector3(0.48f,0,0),-transform.up,0.55f,whatIsGround);
					    ray2 = Physics2D.Raycast(transform.position+new Vector3(-0.48f,0,0),-transform.up,0.55f,whatIsGround);
					}
					else
					{
						ray1 = Physics2D.Raycast(transform.position,-transform.right,0.55f,whatIsGround);
					    ray2 = Physics2D.Raycast(transform.position, transform.right,0.55f,whatIsGround);
						if(ray1.collider!=null)
						{
							Debug.DrawLine(transform.position,ray1.point,Color.red,2f);
						}
						if(ray2.collider!=null)
						{
							Debug.DrawLine(transform.position,ray2.point,Color.green,2f);
						}
					}
				RaycastHit2D ray3 = Physics2D.Raycast(transform.position,-transform.up,0.55f,whatIsGround);
				if(other.gameObject.tag==grounds[i])
				{
					if((yeetThrow&&canShatter&&lastXVelo>1f)||(explosive&&lastXVelo>1f)||ray1.collider!=null||ray2.collider!=null||(!groundTouch&&ray3.collider!=null))
					{
						if(!groundTouch)
						{
							groundTouch = true;
							if(!explosive&&!dropped)lockGroundTouch = true;
							lastXVelo = 0;
							//print(ungroundedFrames);
							if(!dropped&&!explosive&&ungroundedFrames<=ungroundedFramesBeforeExplode+5)break;
							//else print("Touched ground without break");
						}
						if(canExplode&&!exploded||explosive&&ungroundedFrames>=ungroundedFramesBeforeExplode&&!exploded)
						{
							//print(ungroundedFrames);
							Explode();
						}
						else if(canShatter||(ungroundedFrames>=ungroundedFramesBeforeExplode||dropped))
						{
							//print(ungroundedFrames);
							HoldBlockBust();
						}
						ungroundedFrames = 0;
						rb.velocity = new Vector2(rb.velocity.x,0);
						grav.maxVelocities = new Vector2(grav.savedMaxVelocities.x,grav.maxVelocities.y);
						break;
					}
				}
				if(explosive||!explosive&&!lockGroundTouch)
				groundTouch = false;
				else groundTouch = true;
			}
		}
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		if(!isParented)
		for(int i = 0; i<grounds.Length;i++)
		{
			if(other.gameObject.tag==grounds[i]&&!lockGroundTouch)
			{
				groundTouch = false;
				break;
			}
		}
	}
	void toggleVisible(bool toggle)
	{
		//print("A");
		render.enabled = toggle;
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(!isParented&&!lockVis)
		{
			if(other.name=="ObjectActivator"&&visible)
			{
				visible = false;
				toggleVisible(visible);
			}
		}
	}
	public void detonate()
	{
		if(explosive&&!exploded)
		{
			Explode();
		}
		else
		{
			HoldBlockBust();
		}
	}
	IEnumerator groundTouchCooldown()
	{
		yield return 0;
		groundTouch = false;
		lockGroundTouch = false;
		canDetectGround = false;
		yield return new WaitForSeconds(0.05f);
		canDetectGround = true;
	}
	IEnumerator goalKill()
	{
		yield return 0;
		if(data.reachedGoal)
		{
			yield return new WaitUntil(()=>Time.timeScale!=0);
			GameObject obj = Instantiate(steamParticles,transform.position,Quaternion.identity);
			var p = obj.GetComponent<ParticleSystem>().main;
			p.startColor = render.color;
			yield return 0;
			unparentAll();
			if(gameObject.activeInHierarchy) if(gameObject.activeInHierarchy) data.StartCoroutine(DestroyQueue());
		}
	}
	public void killBlock()
	{
		StartCoroutine(goalKill());
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ScreenNuke")
		{
			killBlock();
		}
		if(!inLava)
		{
		if(!isParented)
		{
			if(other.name=="ObjectActivator"&&!visible)
			{
				visible = true;
				toggleVisible(visible);
			}
		if(isHazard&&other.tag=="Player")
		{
			if(explosive&&!exploded)
			{
				Explode();
			}
			else HoldBlockBust();
			other.transform.parent.GetComponent<PlayerScript>().Damage(true,false);
		}
		if(other.name=="BlockParent(Clone)"
		||other.name=="BlockParent"
		||other.tag =="blockHoldable"
		||other.name.Contains("Burger")
		||other.tag =="Disc" && other.transform.parent.GetComponent<shellScript>().moving
		||other.tag=="knockHarm" && bossActivated)//imj boss
		{
			shellScript sc = null;
			if(other.transform.parent!=null)
			sc = other.transform.parent.GetComponent<shellScript>();
			if(sc!=null)
			{
				if(sc.transform.position.x>transform.position.x)
				sc.changeDirection(1,true);
				else sc.changeDirection(-1,true);
			}
			if(other.name.Contains("Burger"))
			{
				detonate();
			}
			if(other.tag=="blockHoldable")
			{
				//print(Mathf.Abs(other.transform.parent.GetComponent<Rigidbody2D>().velocity.x));
				if(Mathf.Abs(other.transform.parent.GetComponent<Rigidbody2D>().velocity.x)>=2f)
				{
					holdableBlockScript otherBlock = other.transform.parent.GetComponent<holdableBlockScript>();
					detonate();
					otherBlock.detonate();
				}
			}
			else if(explosive&&canExplode&&!exploded||explosive&&!exploded&&sc!=null&&visible)
			{
				Explode();
			}
			else if(other.name.Contains("BlockParent"))
			{
				if(canDetectGround)
				{
					groundTouch = false;
					lockGroundTouch = false;
					canDetectGround = false;
					if(!inverted)
					{
						rb.velocity = new Vector2(rb.velocity.x,15);
					}
					else rb.velocity = new Vector2(rb.velocity.x,-15);
					StartCoroutine(groundTouchCooldown());
				}

			}
			else
			{
				HoldBlockBust();
			}
		}
		if(other.name=="InstantDeath")
		{
			inLava= true;
				inverted = false;
				transform.GetChild(0).tag = "Ground";
				if(grav!=null)
				{
					rb.velocity = Vector2.zero;
					grav.pushForces = new Vector2(0,-Mathf.Abs(grav.pushForces.y/2));
					grav.maxVelocities = new Vector2(0,0.4f);
				}
				if(cor!=null)StopCoroutine(cor);
				render.color = oldColor;
				data.spawnCheeseSplatterPoint(transform.position);
				StartCoroutine(dieInLava());
				transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
				transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = other.GetComponent<Renderer>().sortingOrder-1;
		}
		if(other.name.Contains("deathZone")&&!deathzoneProof)
		{
			unparentAll();
			if(gameObject.activeInHierarchy) data.StartCoroutine(DestroyQueue());
		}
		}
		}
	}
	IEnumerator DestroyQueue()
	{
		unparentAll();
		gameObject.SetActive(false);
		yield return 0;
		yield return new WaitForSeconds(0.1f);
		unparentAll();
		Destroy(gameObject);
	}
	public void togglePl(bool enable)
	{
		pl.enabled = enable;
	}
	public void blockParent(bool parent,Vector3 parentedPos,Vector2 velo,bool playerPickUp)
	{
		if(parent)
		{
			pl.enabled = false;
			unparentAll();
		}

		if(!playerPickUp)dropLogic = true;
		isParented = parent;
		SpriteRenderer render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		physBox.enabled = !isParented;
		groundTouch = false;
		ungroundedFrames = 0;
		lastXVelo = 0;
		if(isParented)
		{
			if(playerPickUp)
			yeetThrow = false;
			if(!explosive)
			render.sprite = normalSprites[1];
			streak = 0;
			render.sortingLayerName = "Player";
			render.sortingOrder = 2;
			transform.localPosition = parentedPos;
			
			grav.enabled = false;
			rb.bodyType = RigidbodyType2D.Static;
			rb.simulated = false;
			lockGroundTouch = false;
			if(playerPickUp&&triggerCountdown)
			{
				if(cor==null)
				cor = StartCoroutine(Countdown());
			}
			if(playerPickUp&&transform.parent.GetComponent<PlayerScript>().inverted||!playerPickUp&&transform.parent.eulerAngles.z>179)
			{
				transform.eulerAngles = new Vector3(0,0,180);
				inverted = true;
			}
			else
			{
				transform.eulerAngles = Vector3.zero;
				inverted = false;
			}
			curParentName = transform.parent.name;
			//print(curParentName);
		}
		else
		{
			if(!ignoreBall&&data.ball!=null)
			{
				ignoreBall = true;
				Physics2D.IgnoreCollision(physBox,data.ball,true);
			}
			if(cor!=null&&explosive&&canExplode)
			{
				StopCoroutine(cor);
				render.color = oldColor;
			}
			if(!explosive)
			render.sprite = normalSprites[0];
			transform.parent = defaultParent.transform;
			transform.localScale = Vector3.one;
			render.transform.localScale = Vector3.one;
			if(playerPickUp)
			{
				grav.maxVelocities = new Vector2(16,grav.maxVelocities.y);
			}
			rb.bodyType = RigidbodyType2D.Dynamic;
			if(!explosive&&playerPickUp)
			{
				togglePl(true);
				rb.drag = 0;
				rb.velocity += new Vector2(velo.x*0.65f,0);
			}
			else rb.velocity += new Vector2(velo.x,velo.y);
			grav.enabled = true;
			grav.pushForces = grav.savedPushForces;
			render.sortingLayerName = "Default";
			render.sortingOrder = -1;
			rb.simulated = true;
		}
	}
}
