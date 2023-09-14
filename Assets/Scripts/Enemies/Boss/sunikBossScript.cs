using System.Collections;
using UnityEngine;

public class sunikBossScript : MonoBehaviour
{
    bulletHellSpawner spawner;
    public bool canStomp = false;
    lizardmanBossMaster bossMaster;
    Animator anim;
    int phase = 0;
    Vector3[] points = new Vector3[2];
    Vector3 targetPoint,startPoint;
    public bool moving = false;
    public float duration = 5.0f;
    float startTime;
    float ActiveTime = 0f;
    bool invincible = false;
    bool debugMode = false;
    float yOffset = 2;
    float baseY = 0;
    // Start is called before the first frame update
    void Start()
    {
        bossMaster = transform.parent.GetComponent<lizardmanBossMaster>();
		spawner = transform.GetChild(1).GetComponent<bulletHellSpawner>();
		anim = GetComponent<Animator>();
        if(gameObject.activeInHierarchy&&Time.timeSinceLevelLoad<2f)
        {
            print("sunik debug mode");
            debugMode = true;
        }
        if(Time.timeSinceLevelLoad>6f)
        {
            yOffset = 1;
        }
        baseY = transform.position.y;
        if(moveUp!=null)StopCoroutine(moveUp);
        moveUp = StartCoroutine(IMoveUp(false));
    }
    Coroutine moveUp;
    IEnumerator IMoveUp(bool up)
    {
        float t = 0;
        if(up)
        {
            while(t<=1&&yOffset!=3)
            {
                t+=Time.fixedDeltaTime*0.5f;
                yOffset = Mathf.Lerp(yOffset,3,t);
                yield return 0;
            }
            yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).IsName("sunik_atkLoop"));
            anim.speed = Mathf.Lerp(anim.speed,3,t);
            yield return new WaitUntil(()=>!invincible);
        }
        yield return new WaitForSeconds(0.5f);
        t = 0;
        while(t<=1&&yOffset!=0)
        {
            t+=Time.fixedDeltaTime*0.5f;
            yOffset = Mathf.Lerp(yOffset,0,t);
            anim.speed = Mathf.Lerp(anim.speed,1,t);
            yield return 0;
        }
    }
    IEnumerator phaseChange()
	{
        switch(phase)
        {
            default:

                bossMaster.eraseSkeletiles();
                
            break;
            case 0:
                
                yield return 0;
                points[0] = new Vector3(-11,0,0);
                points[1] = new Vector3(11,0,0);
                startTime = Time.time;
                ActiveTime = Time.time;
                duration = 5f;
                targetPoint = points[0];
                startPoint = transform.position;
                moving = true;
                bossMaster.spawnSkeletile(new Vector3Int(-3,-2,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(2,-2,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-8,0,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(7,0,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-9,0,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(8,0,0),true);
                invincible = false;

            break;
            case 1:

                bossMaster.eraseSkeletiles();
                yield return new WaitForSeconds(1f);
                points[0] = new Vector3(-6,0,0);
                points[1] = new Vector3(6,0,0);
                startTime = Time.time;
                ActiveTime = Time.time;
                duration = 4f;
                targetPoint = points[0];
                startPoint = transform.position;
                moving = true;
                yield return new WaitForSeconds(3f);
                bossMaster.bridgeTilesFall(new Vector3Int(-10,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(9,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-9,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(8,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-6,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(5,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-5,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(4,-5,0));
                yield return new WaitUntil(()=>bossMaster.canSpawnTiles);

                bossMaster.spawnSkeletile(new Vector3Int(-3,1,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(2,1,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-7,1,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(6,1,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-8,1,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(7,1,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-11,-2,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(10,-2,0),true);
                invincible = false;
            
            break;
            case 2:

                bossMaster.eraseSkeletiles();
                bossMaster.restoreBridge();
                yield return new WaitForSeconds(1f);
                points[0] = new Vector3(-3,0,0);
                points[1] = new Vector3(3,0,0);
                startTime = Time.time;
                ActiveTime = Time.time;
                duration = 2f;
                targetPoint = points[0];
                startPoint = transform.position;
                moving = true;
                yield return new WaitForSeconds(3f);
                bossMaster.bridgeTilesFall(new Vector3Int(-12,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(11,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-11,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(10,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-10,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(9,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-6,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(5,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-5,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(4,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-2,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(1,-5,0));
                yield return new WaitForSeconds(0.5f);
                bossMaster.bridgeTilesFall(new Vector3Int(-1,-5,0));
                bossMaster.bridgeTilesFall(new Vector3Int(0,-5,0));
                yield return new WaitUntil(()=>bossMaster.canSpawnTiles);

                bossMaster.spawnSkeletile(new Vector3Int(-11,-1,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(10,-1,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-9,2,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(8,2,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-8,2,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(7,2,0),true);
                invincible = false;
                /*yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-4,2,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(3,2,0),true);
                yield return new WaitForSeconds(0.05f);
                bossMaster.spawnSkeletile(new Vector3Int(-3,2,0),true);
                bossMaster.spawnSkeletile(new Vector3Int(2,2,0),true);*/

            break;
        }
    }
    void Update()
    {
        if(Time.timeScale!=0)
        {
            if(moving)
            {
                ActiveTime+=Time.deltaTime;
                float t = (ActiveTime - startTime) / duration;
                transform.position = new Vector3(Mathf.SmoothStep(startPoint.x, targetPoint.x, t), baseY+yOffset, transform.position.z);
                //print(transform.position.x+" "+targetPoint.x);
                if(transform.position.x==targetPoint.x)
                {
                    //print("a");
                    startTime = Time.time;
                    ActiveTime = Time.time;
                    if(targetPoint==points[0])
                    {
                        targetPoint = points[1];
                        startPoint = points[0];
                    }
                    else if(targetPoint==points[1])
                    {
                        targetPoint = points[0];
                        startPoint = points[1];
                    }
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x, baseY+yOffset, transform.position.z);
            }
            if(debugMode&&Input.GetKeyDown(KeyCode.P))
            {
                damage();
            }
        }
    }
    void damage()
    {
        //Debug.Log("Stomped");
        if(!invincible)
        {
            spawner.killAllActive();
            moving = false;
            phase++;
            StartCoroutine(phaseChange());
            if(phase==3)
            {
                moving = false;
                //print("defeated");
                anim.speed = 1f;
                bossMaster.restoreBridgeFinal();
                StartCoroutine(finalBridge());
                bossMaster.playSound(21,transform.GetChild(0).position);
                anim.SetTrigger("die");
                bossMaster.data.stopAllMusic();
                bossMaster.overlays[2].gameObject.SetActive(false);
            }
            else
            {
                bossMaster.playSound(20,transform.GetChild(0).position);
                anim.SetTrigger("damage");
                invincible = true;
                if(moveUp!=null)StopCoroutine(moveUp);
                moveUp = StartCoroutine(IMoveUp(true));
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "PlayerCollider"&&!bossMaster.pScript.dead&&bossMaster.pScript.rb.velocity.y<=0f)
		{
			//Debug.Log(bossMaster.player.transform.position.y+ " boss: "+(transform.GetChild(0).position.y-0.4f));
			if(canStomp&&bossMaster.player.transform.position.y>transform.GetChild(0).position.y-0.1f)
			{
				damage();
				bossMaster.pScript.stompBoss(gameObject,true);

			}
			else
			{
				if(Time.timeScale!=0 && bossMaster.pScript.invFrames==0 &&!bossMaster.pScript.dead)
				{
					bossMaster.pScript.Damage(false,false);
				}
			}
		}
	}
    IEnumerator finalBridge()
    {
		bossMaster.bridgeTilesFall(new Vector3Int(-9,-5,0));
		bossMaster.bridgeTilesFall(new Vector3Int(8,-5,0));
		yield return new WaitForSeconds(0.2f);
		bossMaster.bridgeTilesFall(new Vector3Int(-8,-5,0));
		bossMaster.bridgeTilesFall(new Vector3Int(7,-5,0));
        yield return new WaitForSeconds(0.2f);
		bossMaster.bridgeTilesFall(new Vector3Int(-7,-5,0));
		bossMaster.bridgeTilesFall(new Vector3Int(6,-5,0));
		yield return new WaitForSeconds(0.2f);
    }
    public void shoot(int offsetting)
    {
        bossMaster.playSound(19,transform.GetChild(0).position);
		if(offsetting==0)
		spawner.transform.eulerAngles = Vector3.zero;
		else spawner.transform.eulerAngles = new Vector3(0,0,22.5f);
		spawner.fire();
    }
    public void spawn()
    {
        StartCoroutine(phaseChange());
    }
    public void turnOff()
	{
        bossMaster.spawnLizardman(3);
		gameObject.SetActive(false);
	}
	public void playSteam()
	{
		bossMaster.playSteam(transform.GetChild(0).position+new Vector3(-0.2f,0,0));
	}
    public void spawnSound()
    {
        bossMaster.playSound(22,transform.GetChild(0).position);
        bossMaster.overlayMusicFade(2,true);
    }
    public void deathSound()
    {
        bossMaster.playSound(23,transform.GetChild(0).position);
    }
}
