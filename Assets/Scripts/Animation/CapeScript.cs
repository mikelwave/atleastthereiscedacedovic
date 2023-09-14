using UnityEngine;
using System;

public class CapeScript : MonoBehaviour {
	Animator capeAnim;
	Animator playerAnim;
	string PlayerAnimName;
	SpriteRenderer render,playerRender;
	Sprite[] capeSprites,eternalCapeSprites;
	PlayerMusicBounce musicBounce;
	int walkAnimInt = -1;
	int diveInt = 0;
	int diveLandInt = -1;
	int jumpInt = -1;
	bool alternator = false;
	bool alternatorDive = false;
	bool alternatorDiveLand = false;
	bool alternatorJump = false;
	PlayerScript pScript;
	playerSprite pSprites;
	Rigidbody2D rb;
	AudioSource asc;
	// Use this for initialization
	void Start ()
	{
		render = GetComponent<SpriteRenderer>();
		pScript = transform.parent.parent.GetComponent<PlayerScript>();
		pSprites = transform.parent.parent.GetComponent<playerSprite>();
		capeSprites = Resources.LoadAll<Sprite>("burek_cape");
		eternalCapeSprites = Resources.LoadAll<Sprite>("eternal_burek_cape");
		if(transform.parent.name!="Player_main")
		musicBounce = transform.parent.parent.GetComponent<PlayerMusicBounce>();
		else musicBounce = transform.parent.GetComponent<PlayerMusicBounce>();

		playerAnim = transform.parent.GetComponent<Animator>();
		playerRender = transform.parent.GetComponent<SpriteRenderer>();
		rb = transform.parent.parent.GetComponent<Rigidbody2D>();
		asc = GetComponent<AudioSource>();
	}
	void Update()
	{
		render.enabled = playerRender.enabled;
	}
	public void stopCapeSound()
	{
		if(asc!=null&&asc.isPlaying)asc.Stop();
		this.enabled = false;
	}
	void OnDisable()
	{
		if(asc.isPlaying)asc.Stop();
	}
	void LateUpdate()
	{
		if(Time.timeScale==0&&asc.isPlaying) asc.Stop();
		if(pScript!=null&&pScript.inverted)
		{
			transform.localPosition=new Vector3(-transform.localPosition.x,transform.localPosition.y,transform.localPosition.z);
		}
		if(render.sprite==capeSprites[0])
		{
			walkAnimInt = -1;
			alternator = false;
		}
		if(musicBounce.frame == 1 && render.sprite == capeSprites[0]&&!playerAnim.GetBool("Talk"))
		{
			render.sprite = capeSprites[1];
		}
		if(render.sprite == capeSprites[3]||render.sprite == capeSprites[4]) walkRun();
		if(render.sprite == capeSprites[9]||render.sprite == capeSprites[10]) dive();
		if(render.sprite == capeSprites[12]||render.sprite == capeSprites[13]) diveLand();
		if(pScript!=null&&(render.sprite == capeSprites[23]||render.sprite == capeSprites[24])) jump();

		if(pSprites!=null&&render.sprite!=null&&pSprites.eternal)
		{
			string spriteName = render.sprite.name;
			var newSpriteInt = Array.FindIndex(capeSprites, item => item.name == spriteName);
			if(newSpriteInt != -1)
			render.sprite = eternalCapeSprites[newSpriteInt];
		}
	}
	void walkRun()
	{
		//Debug.Log((walkAnimInt+3));
		if(asc.isPlaying)asc.Stop();
		if(render.sprite == capeSprites[3])
		{
			if(!alternator)
			{
				alternator = true;
				if(walkAnimInt>-1)
				walkAnimInt++;
				if(walkAnimInt>4)
					walkAnimInt = 0;
			}
		}
		if(render.sprite == capeSprites[4])
		{
			if(alternator)
			{
				alternator = false;
				walkAnimInt++;
				if(walkAnimInt>4)
					walkAnimInt = 0;
			}
		}
		render.sprite = capeSprites[3+walkAnimInt];
	}
	void dive()
	{
		if(!asc.isPlaying&&Time.timeScale!=0)asc.Play();
		if(alternatorDiveLand){diveLandInt=-1; alternatorDiveLand = false;}
		if(render.sprite == capeSprites[9])
		{
			if(!alternatorDive)
			{
				alternatorDive = true;
				diveInt++;
				if(diveInt>2)
					diveInt = 0;
			}
		}
		if(render.sprite == capeSprites[10])
		{
			if(alternatorDive)
			{
				alternatorDive = false;
				diveInt++;
				if(diveInt>2)
					diveInt = 0;
			}
		}
		render.sprite = capeSprites[9+diveInt];
	}
	void diveLand()
	{
		if(asc.isPlaying)asc.Stop();
		if(render.sprite == capeSprites[12])
		{
			if(!alternatorDiveLand)
			{
				alternatorDiveLand = true;
				if(diveLandInt<2)
				diveLandInt++;
			}
		}
		if(render.sprite == capeSprites[13])
		{
			if(alternatorDiveLand)
			{
				alternatorDiveLand = false;
				if(diveLandInt<2)
				diveLandInt++;
			}
		}
		render.sprite = capeSprites[12+diveLandInt];
	}
	void jump()
	{
		if(asc.isPlaying)asc.Stop();
		if(pScript.enabled&&pScript!=null&&!pScript.inverted&&rb.velocity.y>=0&&jumpInt!=-1
		||pScript.enabled&&pScript!=null&&pScript.inverted&&rb.velocity.y<=0&&jumpInt!=-1
		||pScript==null)
		{
			//print(rb.velocity);
			if(rb.velocity.y!=0)
			{
				//print("1");
				jumpInt = -1;
			}
			else
			{
				//print("2");
				jumpInt = 1;
			}
			alternatorJump = false;
		}
		if(pScript!=null&&!pScript.inverted&&rb.velocity.y<=0
		||pScript!=null&&pScript.inverted&&rb.velocity.y>=0
		||pScript == null
		||!pScript.enabled)
		{
			if(render.sprite == capeSprites[23])
			{
				if(!alternatorJump)
				{
					alternatorJump = true;
					jumpInt++;
					if(jumpInt>2)
						jumpInt = 1;
				}
			}
			if(render.sprite == capeSprites[24])
			{
				if(alternatorJump)
				{
					alternatorJump = false;
					jumpInt++;
					if(jumpInt>2)
						jumpInt = 1;
				}
			}
			render.sprite = capeSprites[25+jumpInt];
		}
	}
}
