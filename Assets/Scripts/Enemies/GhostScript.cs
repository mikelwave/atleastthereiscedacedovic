using UnityEngine;

public class GhostScript : MonoBehaviour {
	public Sprite[] sprites = new Sprite[3];
	Transform player;
	EnemyOffScreenDisabler eneOff;
	SpriteRenderer render;
	bool shy = false;
	public float speed = 1;
	float curSpeed = 0;
	public float speedAdditive = 0.05f;
	float wave = 0;
	public float omegaY = 2f;
	public float sineAmplitude = 2;
	float index;
	int animInt = 15;
	int spriteValue = 0;
	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("Player_main").transform;
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		eneOff = GetComponent<EnemyOffScreenDisabler>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(eneOff.visible&&Time.timeScale!=0)
		{
			Shy();
			if(!shy)
			Animation();

			if(shy&&render.color.a>0.7f)
			{
				if(render.sprite!=sprites[2])
				render.sprite = sprites[2];
				Color col = render.color;
				float i = Mathf.Clamp(col.a-0.05f,0.7f,1);
				render.color = new Color(col.r,col.g,col.b,i);
			}
			else if(!shy&&render.color.a<1)
			{
				Color col = render.color;
				float i = Mathf.Clamp(col.a+0.05f,0.7f,1);
				render.color = new Color(col.r,col.g,col.b,i);

				if(i==1&&render.sprite!=sprites[0])
				{
					render.sprite = sprites[0];
				}
			}

			if(!shy&&curSpeed<speed)
			curSpeed=Mathf.Clamp(curSpeed+=speedAdditive,0,speed);
			else if(shy&&curSpeed>0)
			curSpeed=Mathf.Clamp(curSpeed-=speedAdditive*3,0,speed);
		}
		if(!eneOff.visible&&curSpeed!=0)
		{
			curSpeed = 0;
			index = 0;
			wave = 0;
		}
	}
	void Animation()
	{
		animInt--;
		if(animInt==0)
		{
			spriteValue++;
			if(spriteValue>1)spriteValue = 0;
			render.sprite = sprites[spriteValue];
			animInt = 15;
		}
	}
	void FixedUpdate()
	{
		if(eneOff.visible&&Time.timeScale!=0)
		{
			if(!shy)
			lookAtPlayer();

			followPlayer();
		}
	}
	void Shy()
	{
		if(transform.localScale.x==player.localScale.x)
		{
			if(transform.position.x>player.position.x&&transform.localScale.x==1
			 ||transform.position.x<=player.position.x&&transform.localScale.x==-1)
			shy = true;
			else shy = false;
		}
		else shy = false;
	}
	void followPlayer()
	{

		transform.position = Vector3.MoveTowards(transform.position,new Vector3(player.position.x,player.position.y+player.up.y+wave,transform.position.z),curSpeed*Time.deltaTime);

		//sine wave movement
		if(!shy)
		{
			sineWave();
		}

	}
	void sineWave()
	{
		index+=Time.deltaTime;
		if(index>3)
		index = index-3;
		wave = sineAmplitude*Mathf.Sin(omegaY*index);
	}
	void lookAtPlayer()
	{
		if(player.position.x>transform.position.x&&transform.localScale.x!=-1)
		transform.localScale = new Vector3(-1,transform.localScale.y,transform.localScale.z);
		if(player.position.x<transform.position.x&&transform.localScale.x!=1)
		transform.localScale = new Vector3(1,transform.localScale.y,transform.localScale.z);
	}
}
