using System.Collections;
using UnityEngine;

public class audinoBossScript : MonoBehaviour
{
    public bool canStomp = false;
	bool stompLock = false;
    lizardmanBossMaster bossMaster;
    Animator anim;
    int phase = 0,sequenceInt = 0;
	bool jumping = false;
	bool knife = true;
    Coroutine sequenceCor;
	Collider2D main;
	bulletHellSpawner spawner;
	GameObject[] bloodOrbs = new GameObject[3];
	movingPlatformScript[] saws = new movingPlatformScript[3];
	AudioSource src;
    IEnumerator Sequence()
	{
        //1 = spit
        //pull knife
        //2 = leap
        //pull knife
		while(phase<3)
		{
			yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).IsName("audino_idle"));
			stompLock = false;
			//yield return new WaitForSeconds(0.5f);
			if(knife)
			{
				if(phase==0)
				yield return new WaitForSeconds(0.3f);
				else yield return new WaitForSeconds(0.15f);
				anim.SetBool("knife",true);
				knife = false;
			}
			else
			{
				knife = true;
				sequenceInt++;if(sequenceInt>2)sequenceInt = 1;
				anim.SetInteger("sequence",sequenceInt);
			}
			yield return new WaitForSeconds(0.3f);
			anim.SetInteger("sequence",0);
		}
	}
    IEnumerator phaseChange()
	{
		if(phase==1)
		{
			saws[0].gameObject.SetActive(true);
			saws[1].gameObject.SetActive(true);
            yield return 0;
        }
		else if(phase == 2)
		{
			saws[2].gameObject.SetActive(true);
		}
    }
	void FixedUpdate()
	{
		if(!main.enabled&&transform.position.y<=5.3f)
		{
			main.enabled = true;
		}
	}
    // Start is called before the first frame update
    void Start()
    {
        bossMaster = transform.parent.GetComponent<lizardmanBossMaster>();
		spawner = transform.GetChild(1).GetComponent<bulletHellSpawner>();
		anim = GetComponent<Animator>();
		main = GetComponent<Collider2D>();
		src = transform.GetChild(0).GetComponent<AudioSource>();
		for(int i = 0; i<bloodOrbs.Length;i++)
		{
			bloodOrbs[i] = transform.GetChild(0).GetChild(i).gameObject;
		}
		for(int i = 0; i<saws.Length;i++)
		{
			saws[i] = transform.parent.GetChild(2).GetChild(i).GetComponent<movingPlatformScript>();
		}
    }
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "PlayerCollider"&&!bossMaster.pScript.dead)
		{
			//Debug.Log(bossMaster.player.transform.position.y+ " boss: "+(transform.GetChild(0).position.y-0.4f));
			if(!stompLock&&canStomp&&bossMaster.player.transform.position.y>transform.GetChild(0).position.y-0.4f)
			{
				//Debug.Log("Stomped");
				stompLock = true;
				anim.SetBool("knife",true);
				sequenceInt = 1;
				anim.SetInteger("sequence",sequenceInt);
				knife = true;
				//dmgObj.localScale = transform.localScale;
				phase++;
				StartCoroutine(phaseChange());
				if(phase==3)
				{
					StopCoroutine(sequenceCor);
					anim.SetInteger("sequence",0);
					//print("defeated");
					anim.speed = 1f;
					bossMaster.restoreBridge();
					bossMaster.playSound(6,transform.GetChild(0).position);
					anim.SetTrigger("die");
					bossMaster.lizInter.transform.position = transform.GetChild(0).position;
					for(int i = 0; i<saws.Length;i++)
					{
						saws[i].stopMovingOnReachedPoint = true;
						saws[i].disableOnReachedPoint = true;
					}
				}
				else
				{
					bossMaster.playSound(5,transform.GetChild(0).position);
					anim.SetTrigger("damage");
				}
				bossMaster.pScript.stompBoss(gameObject,true);

			}
			else
			{
				//Debug.Log("Knockback");
				if(Time.timeScale!=0 && bossMaster.pScript.invFrames==0 &&!bossMaster.pScript.dead)
				{
					bossMaster.pScript.Damage(false,false);
				}
			}
		}
	}
    public void shoot()
    {
		if(transform.localScale.x==1)
		spawner.transform.eulerAngles = new Vector3(0,0,-22.5f);
		else spawner.transform.eulerAngles = new Vector3(0,0,202.5f);
		spawner.fire();
    }
	public void hitGround()
	{
		if(jumping)
		{
			jumping = false;
			anim.SetTrigger("groundtrigger");
			lookAtPlayer();
		}
	}
    public void leap()
    {
		jumping = true;
		main.enabled = false;
		lookAtPlayer();
		transform.position = new Vector3(Mathf.Clamp(Mathf.Floor(bossMaster.player.transform.position.x)+0.5f,-11.5f,11.5f),20,0);
    }
    public void resetKnife()
    {
        anim.SetBool("knife",false);
    }
	public void enableSequence()
	{
		bossMaster.overlayMusicFade(1,true);
		jumping = true;
		lookAtPlayer();
		sequenceCor = StartCoroutine(Sequence());
	}
	void lookAtPlayer()
	{
		if(transform.localScale.x==1&&bossMaster.player.transform.position.x>transform.position.x)
		{
			transform.localScale=new Vector3(-1,1,1);
		}
		else if(transform.localScale.x==-1&&bossMaster.player.transform.position.x<=transform.position.x)
		{
			transform.localScale=new Vector3(1,1,1);
		}
	}
	public void splatterBlood()
	{
		for(int i = 0; i<bloodOrbs.Length;i++)
		{
			if(i==0&&transform.position.x<10.5f||i==1&&transform.position.x>-10.5f||i==2)
			{
				GameObject obj = Instantiate(bloodOrbs[i],bloodOrbs[i].transform.position,Quaternion.identity);
				//obj.transform.localScale = Vector3.one;
				obj.SetActive(true);
			}
		}
		bossMaster.shakeCam(0.1f,0.1f);
	}
	public void turnOff()
	{
		bossMaster.overlayMusicFade(1,false);
		bossMaster.spawnLizardman(2);
		gameObject.SetActive(false);
	}
	public void playSteam()
	{
		bossMaster.playSteam(transform.position);
	}
	public void playSoundCut(int ID)
	{
		if(src.isPlaying)src.Stop();
		if(ID==0) src.clip = bossMaster.sounds[13];
		else  src.clip = bossMaster.sounds[11];

		src.Play();
	}
	public void playSound(int ID)
	{
		bossMaster.playSound(ID,transform.GetChild(0).position);
	}
	public void playSpitSound()
	{
		bossMaster.playSound(Random.Range(15,18),transform.GetChild(0).position);
	}
	public void playSoundStatic(int ID)
	{
		bossMaster.playSoundStatic(ID);
	}
}
