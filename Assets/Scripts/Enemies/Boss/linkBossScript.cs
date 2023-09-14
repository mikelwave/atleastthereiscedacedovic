using System.Collections;
using UnityEngine;

public class linkBossScript : MonoBehaviour
{
    lizardmanBossMaster bossMaster;
	public bool canStomp = false;
	Animator anim;
	Transform dmgObj,spawner;
	GameObject superBlood,superBlood2;
	Rigidbody2D rb;
	int phase = 0,sequenceInt = 0;
	Coroutine sequenceCor;
	bulletScript blood1,blood2;
	IEnumerator Sequence()
	{
		while(phase<3)
		{
			yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).IsName("linkh_idle"));
			anim.SetInteger("sequence",0);
			//print("idle");
			if(sequenceInt==0)
			{
				if(phase==0)
				yield return new WaitForSeconds(0.5f);
				else if(phase==1) yield return new WaitForSeconds(0.25f);
				else yield return new WaitForSeconds(0.1f);
			}
			//print("sequence");
			sequenceInt++;if(sequenceInt>2)sequenceInt = 1;
			anim.SetInteger("sequence",sequenceInt);
			yield return new WaitForSeconds(1f);
		}
	}
	IEnumerator phaseChange()
	{
		if(phase==0)
		{
			bossMaster.bridgeTilesFall(new Vector3Int(-12,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(11,-5,0));
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-11,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(10,-5,0));
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-10,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(9,-5,0));
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-9,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(8,-5,0));
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-8,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(7,-5,0));
		}
		if(phase==1)
		{
			anim.SetBool("2",true);
			anim.speed = 1.2f;
			blood1.speed = 12f;
			blood2.speed = 8f;
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-6,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(5,-5,0));
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-5,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(4,-5,0));
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-1,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(0,-5,0));
		}
		if(phase==2)
		{
			anim.speed = 1.3f;
			blood1.speed = 14f;
			blood2.speed = 10f;
			yield return new WaitForSeconds(0.5f);
			bossMaster.bridgeTilesFall(new Vector3Int(-2,-5,0));
			bossMaster.bridgeTilesFall(new Vector3Int(1,-5,0));
		}
	}
    // Start is called before the first frame update
    void Start()
    {
        bossMaster = transform.parent.GetComponent<lizardmanBossMaster>();
		anim = GetComponent<Animator>();
		dmgObj = transform.GetChild(1);
		spawner = transform.GetChild(0).GetChild(0);
		superBlood = transform.GetChild(2).gameObject;
		superBlood2 = transform.GetChild(3).gameObject;
		superBlood.transform.SetParent(null);
		superBlood2.transform.SetParent(null);
		blood1 = superBlood.GetComponent<bulletScript>();
		blood2 = superBlood2.GetComponent<bulletScript>();
		rb = GetComponent<Rigidbody2D>();
    }
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "PlayerCollider"&&!bossMaster.pScript.dead)
		{
			if(canStomp&&bossMaster.player.transform.position.y>transform.GetChild(0).position.y-0.25f)
			{
				//Debug.Log("Stomped");
				dmgObj.position = transform.GetChild(0).position;
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
				if(Time.timeScale!=0 && bossMaster.pScript.invFrames==0&&!bossMaster.pScript.dead)
				{
					bossMaster.pScript.Damage(false,false);
				}
			}
		}
	}
	public void shootBlood()
	{
		if(!superBlood.activeInHierarchy)
		{
			superBlood.transform.position = spawner.position;
			blood1.Enable(true);
		}
		else
		{
			superBlood2.transform.position = spawner.position;
			blood2.Enable(true);
		}
	}
	public void flipX()
	{
		transform.localScale = new Vector3(-transform.localScale.x,1,1);
	}
	public void resetX()
	{
		transform.localScale = new Vector3(-1,1,1);
	}
	public void freezeRB()
	{
		rb.velocity = Vector2.zero;
	}
	public void resetMainPos(int ID)
	{
		if(ID==0)
		transform.localPosition = new Vector3(0,4,0);
		else transform.localPosition = new Vector3(0,3,0);
		dmgObj.localPosition = new Vector3(0,-7.68f,0);
		flipX();
	}
	public void enableSequence()
	{
		bossMaster.playSoundStatic(1);
		bossMaster.overlayMusicFade(0,true);
		sequenceCor = StartCoroutine(Sequence());
		StartCoroutine(phaseChange());
	}
	public void turnOff()
	{
		bossMaster.overlayMusicFade(0,false);
		bossMaster.spawnLizardman(1);
		gameObject.SetActive(false);
	}
	public void playSteam()
	{
		bossMaster.playSteam(transform.position);
	}
	public void playSound(int ID)
	{
		bossMaster.playSound(ID,transform.GetChild(0).position);
	}

}
