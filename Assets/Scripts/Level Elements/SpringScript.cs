using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringScript : MonoBehaviour {
	public float springStrength = 10f;
	public float playerJumpStrength = 25f;
	[HideInInspector]
	public playerSprite pSprite;
	public Vector2 launchDir = Vector2.up;
	public float[] springTypeOffsets = new float[]{0.5f,1f,1f};
	public int type = 0;
	int pointingValue = 3; // value to determine which direction it is pointing 0 - left, 1 - down, 2 - right, 3 - up
	public Sprite[] type0Sprites;
	public Sprite[] type1Sprites;
	public Sprite[] type2Sprites;
	SimpleAnim2 anim;
	AudioClip boing;
	public AudioClip shatter;
	GameData data;
	Transform cam;
	//Vector3[] bounds = new Vector3[2];
	Bounds bounds;
	IEnumerator breakSpring()
	{
		yield return new WaitUntil(()=>!anim.isPlaying);
		GetComponent<AudioSource>().PlayOneShot(shatter,1f);
		GetComponent<BoxCollider2D>().enabled=false;
		GetComponent<SpriteRenderer>().enabled = false;
		transform.GetChild(0).GetComponent<ParticleSystem>().Play();
		yield return new WaitForSeconds(2f);
		Destroy(gameObject);

	}
	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<SimpleAnim2>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		boing = data.sounds[31];
		anim.sprites = new List<Sprite>();
		cam = GameObject.Find("Main Camera").transform;
		pSprite = GameObject.Find("Player_main").GetComponent<playerSprite>();
		bounds.min = transform.position+new Vector3(-15,-11,0);
		bounds.max = transform.position+new Vector3(15,11,0);
		if(type==0)
		{
			for(int i = 0; i<type0Sprites.Length;i++)
			{
				anim.sprites.Add(type0Sprites[i]);
			}
		}
		else if(type==1)
		{
			for(int i = 0; i<type1Sprites.Length;i++)
			{
				anim.sprites.Add(type1Sprites[i]);
			}
		}
		else
		{
			for(int i = 0; i<type2Sprites.Length;i++)
			{
				anim.sprites.Add(type2Sprites[i]);
			}
		}
		anim.Start();
		float dir = transform.eulerAngles.z;
		if(dir > 0 && dir <=90)
			pointingValue = 0;
		else if(dir > 90 && dir <=180)
			pointingValue = 1;
		else if(dir > 180 && dir <=270)
			pointingValue = 2;
		else pointingValue = 3;
	}
	void bounce(Rigidbody2D rb,float strength)
	{
		GetComponent<AudioSource>().PlayOneShot(boing,1f);
		launchDir = new Vector2(transform.up.x,transform.up.y);
		rb.velocity = new Vector2(launchDir.x*springStrength,launchDir.y*strength);
		anim.StartPlaying();
		//Debug.Log(rb.velocity);
	}
	void OnTriggerStay2D(Collider2D other)
	{
		if(bounds.Contains(new Vector2(cam.position.x,cam.position.y)))
		{
			if(type==2&&other.GetComponent<Rigidbody2D>()!=null&&other.name!="ObjectActivator")
			{
				bool canBounce = false;
				switch(pointingValue)
				{
					//default is 3, up;
					default:
					if(other.transform.position.y >= transform.position.y+springTypeOffsets[type]
					&& other.gameObject.GetComponent<Rigidbody2D>().velocity.y<=0.01f)
					canBounce = true;
					break;
					//left
					case 0:
					if(other.transform.position.x <= transform.position.x-springTypeOffsets[type])
					canBounce = true;
					break;
					//right
					case 2:
					if(other.transform.position.x >= transform.position.x+springTypeOffsets[type])
					canBounce = true;
					break;
					//down
					case 1:
					if(other.transform.position.y <= transform.position.y-springTypeOffsets[type]
					&& other.gameObject.GetComponent<Rigidbody2D>().velocity.y>=-0.01f)
					canBounce = true;
					break;
				}
				if(canBounce)
				{
					Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
					rb.velocity = new Vector2(rb.velocity.x,0);
					//Debug.Log(other.gameObject.name+" tag: "+other.gameObject.tag);
					if(other.gameObject.name=="Player_main")
					{
						PlayerScript pScript = other.gameObject.GetComponent<PlayerScript>();
						pScript.resetCanHit();
						if(!pScript.inverted)
						{
							if(!pScript.reTrampoline)
							{
								pScript.reTrampoline = true;
								pScript.maxHeight+=11f;
							}
							pScript.currentJumpPoint = 0;
							pScript.startJumpPoint = transform.position.y;
							if(SuperInput.GetKey("Jump"))
							pScript.platHeight = pScript.transform.position.y-0.3f+10f;
							else pScript.platHeight = pScript.transform.position.y-0.3f;
						}
						else
						{
							if(!pScript.reTrampoline)
							{
								pScript.reTrampoline = true;
								pScript.maxHeight+=11f;
							}
							pScript.currentJumpPoint = 0;
							pScript.startJumpPoint = transform.position.y;
							if(SuperInput.GetKey("Jump"))
							pScript.platHeight = pScript.transform.position.y-0.3f-10f;
							else pScript.platHeight = pScript.transform.position.y-0.3f;
						}
						pScript.grav.maxVelocities = new Vector2(pScript.grav.maxVelocities.x,pScript.grav.savedMaxVelocities.y);
						if(pScript.anim.GetBool("dive"))
						{
							pScript.anim.SetBool("dive",false);
						}
					}
					if(type==0)
					{
						if(other.gameObject.name=="Player_main" && SuperInput.GetKey("Jump"))
							bounce(other.gameObject.GetComponent<Rigidbody2D>(),playerJumpStrength);	
						else
						{
							bounce(other.gameObject.GetComponent<Rigidbody2D>(),springStrength);
						}

						if(other.gameObject.name == "Enemy_Corpse")
						{
							other.gameObject.GetComponent<EnemyCorpseSpawner>().springCorpse();
						}
					}
					else if (type == 1&&other.gameObject.name=="Player_main")
					{
						if(other.gameObject.GetComponent<playerSprite>().state<=0)
						{
							if(SuperInput.GetKey("Jump"))
							bounce(other.gameObject.GetComponent<Rigidbody2D>(),playerJumpStrength);	
							else
							{
								bounce(other.gameObject.GetComponent<Rigidbody2D>(),springStrength);
							}
						}
					}
					else
					{
						if(other.gameObject.name=="Player_main" && SuperInput.GetKey("Jump"))
							bounce(other.gameObject.GetComponent<Rigidbody2D>(),playerJumpStrength);	
						else
						{
							bounce(other.gameObject.GetComponent<Rigidbody2D>(),springStrength);
						}

						if(other.gameObject.name=="Player_main")
						{
							StartCoroutine(breakSpring());
						}
					}
				}
			}
		}
	}
	void OnCollisionEnter2D(Collision2D other)
	{
		if(type==1&&other.gameObject.name=="Player_main"&&pSprite.state!=0
		&&other.transform.position.y>transform.position.y+springTypeOffsets[1])
		{
			GetComponent<SpriteRenderer>().sprite = type1Sprites[1];
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		if(type==1&&other.gameObject.name=="Player_main"&&pSprite.state!=0)
		{
			GetComponent<SpriteRenderer>().sprite = type1Sprites[0];
		}
	}
	void OnCollisionStay2D(Collision2D other)
	{
		if(bounds.Contains(new Vector2(cam.position.x,cam.position.y)))
		{
			//print(other.gameObject.name);
			bool canBounce = false;
			switch(pointingValue)
			{
				//default is 3, up;
				default:
				if(other.transform.position.y >= transform.position.y+springTypeOffsets[type]
				&& other.gameObject.GetComponent<Rigidbody2D>().velocity.y<=0.01f)
				canBounce = true;
				break;
				//left
				case 0:
				if(other.transform.position.x <= transform.position.x-springTypeOffsets[type])
				canBounce = true;
				break;
				//right
				case 2:
				if(other.transform.position.x >= transform.position.x+springTypeOffsets[type])
				canBounce = true;
				break;
				//down
				case 1:
				if(other.transform.position.y <= transform.position.y-springTypeOffsets[type]
				&& other.gameObject.GetComponent<Rigidbody2D>().velocity.y>=-0.01f)
				canBounce = true;
				break;
			}
			if(canBounce)
			{
				//Debug.Log(other.gameObject.name+" tag: "+other.gameObject.tag);
				if(other.gameObject.name=="Player_main")
				{
					PlayerScript pScript = other.gameObject.GetComponent<PlayerScript>();
					pScript.resetCanHit();
					if(!pScript.inverted)
						{
							if(!pScript.reTrampoline)
							{
								pScript.reTrampoline = true;
								pScript.maxHeight+=11f;
							}
							pScript.currentJumpPoint = 0;
							pScript.startJumpPoint = transform.position.y;
							if(SuperInput.GetKey("Jump"))
							pScript.platHeight = pScript.transform.position.y-0.3f+10f;
							else pScript.platHeight = pScript.transform.position.y-0.3f;
						}
						else
						{
							if(!pScript.reTrampoline)
							{
								pScript.reTrampoline = true;
								pScript.maxHeight+=11f;
							}
							pScript.currentJumpPoint = 0;
							pScript.startJumpPoint = transform.position.y;
							if(SuperInput.GetKey("Jump"))
							pScript.platHeight = pScript.transform.position.y-0.3f-10f;
							else pScript.platHeight = pScript.transform.position.y-0.3f;
						}
					pScript.grav.maxVelocities = new Vector2(pScript.grav.maxVelocities.x,pScript.grav.savedMaxVelocities.y);
					
					if(pScript.anim.GetBool("dive"))
					{
						pScript.anim.SetBool("dive",false);
					}
				}
				holdableBlockScript hb = other.transform.GetComponent<holdableBlockScript>();
				if(type==0)
				{
					if(other.gameObject.name=="Player_main" && SuperInput.GetKey("Jump"))
						bounce(other.gameObject.GetComponent<Rigidbody2D>(),playerJumpStrength);	
					else
					{
						if(other.transform.childCount==0||other.transform.childCount>0&&other.transform.GetChild(0).gameObject.activeInHierarchy)
						{
							if(hb==null||hb!=null&&hb.visible)
							{
								bounce(other.gameObject.GetComponent<Rigidbody2D>(),springStrength);
							}
						}
					}

					if(other.gameObject.name == "Enemy_Corpse")
					{
						other.gameObject.GetComponent<EnemyCorpseSpawner>().springCorpse();
					}
				}
				else if (type == 1&&other.gameObject.name=="Player_main")
				{
					if(other.gameObject.GetComponent<playerSprite>().state<=0)
					{
						if(SuperInput.GetKey("Jump"))
						bounce(other.gameObject.GetComponent<Rigidbody2D>(),playerJumpStrength);	
						else
						{
							if(other.transform.childCount==0||other.transform.childCount>0&&other.transform.GetChild(0).gameObject.activeInHierarchy)
							{
								if(hb==null||hb!=null&&hb.visible)
								bounce(other.gameObject.GetComponent<Rigidbody2D>(),springStrength);
							}
						}
					}
				}
				else
				{
					if(other.gameObject.name=="Player_main" && SuperInput.GetKey("Jump"))
						bounce(other.gameObject.GetComponent<Rigidbody2D>(),playerJumpStrength);	
					else
					{
						//print(other.gameObject.name);
						if(other.transform.childCount==0||other.transform.childCount>0&&other.transform.GetChild(0).gameObject.activeInHierarchy)
						{
							if(hb==null||hb!=null&&hb.visible)
							bounce(other.gameObject.GetComponent<Rigidbody2D>(),springStrength);
						}
					}

					if(other.gameObject.name=="Player_main")
					{
						StartCoroutine(breakSpring());
					}
				}
			}
		}
	}
}
