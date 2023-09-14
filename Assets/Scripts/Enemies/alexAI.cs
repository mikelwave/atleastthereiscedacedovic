using System.Collections;
using UnityEngine;

public class alexAI : MonoBehaviour
{
    bool grounded = false;
    EnemyOffScreenDisabler eneOff;
    //EnemyCorpseSpawner eneCorpse;
    Transform player;
    float StartY;
    public float speed = 0.1f;
    Animator anim;
    public LayerMask whatisFireball;
    public LayerMask whatisEnemy;
    bool attacking = false;
    Coroutine attackCor;
    Transform spikeTr;
    Animator spikeAnim;
    bool spikeOut = false;
    PlayerScript pScript;
    Rigidbody2D rb;
    bool knockedBack = false;
    Gravity grav;
    Coroutine knockbackCor;
    float endPos;
    public AudioClip alexDeath,clingSound;
    bool dying = false;
    GameData data;
    IEnumerator knockback()
    {
        yield return new WaitUntil(()=>pScript.grounded);
        knockedBack = false;
        knockbackCor = null;
    }
    IEnumerator deathScream()
    {
        spikeOut = false;
        spikeAnim.SetBool("appear",spikeOut);
        yield return new WaitUntil(()=>transform.position.y<StartY);
        AudioSource asc = GetComponent<AudioSource>();
        if(asc.isPlaying)asc.Stop();
        asc.clip = alexDeath;
        asc.pitch = 1f;
        asc.spatialBlend = 0;
        asc.Play();
        yield return new WaitUntil(()=>!asc.isPlaying&&transform.position.y<StartY-5f);
        Destroy(transform.parent.gameObject);
        
    }
    IEnumerator attack()
    {
        attacking = true;
        anim.SetBool("spin",attacking);
        yield return new WaitForSeconds(0.1f);
        attacking = false;
        anim.SetBool("spin",attacking);
        attackCor = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        data = GameObject.Find("_GM").GetComponent<GameData>();
        eneOff = GetComponent<EnemyOffScreenDisabler>();
        //eneCorpse = GetComponent<EnemyCorpseSpawner>();
        anim = GetComponent<Animator>();
        player = GameObject.Find("Player_main").transform;
        StartY = transform.position.y;
        spikeTr = transform.parent.GetChild(1);
        spikeAnim = spikeTr.GetComponent<Animator>();
        pScript = player.GetComponent<PlayerScript>();
        rb = GetComponent<Rigidbody2D>();
        grav = GetComponent<Gravity>();
        endPos = transform.parent.GetChild(2).position.x;
        knockedBack = true;
        if(knockedBack&&knockbackCor==null)
        {
            knockbackCor = StartCoroutine(knockback());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(eneOff.visible&&Time.timeScale!=0)
        {
            if(transform.position.x>=endPos)
            {
                transform.position = new Vector3(endPos,transform.position.y,transform.position.z);
                rb.velocity = new Vector3(0,rb.velocity.y);
                if(!dying)
                {
                    dying = true;
                    StartCoroutine(deathScream());
                }

            }
            if(player.position.y>StartY+0.1f||!grounded)
            {
                if(!knockedBack&&!dying)
                {
                    //print("test");
                    float targetPos;
                    if(player.position.y>StartY+0.2f)
                    targetPos = Mathf.Clamp(player.position.y+0.2f,StartY,StartY+10);
                    else targetPos = Mathf.Clamp(player.position.y-0.1f,StartY-0.4f,StartY+10);
                    transform.position += (new Vector3(0,targetPos-transform.position.y,0)) * speed * Time.deltaTime;
                }
            }

            if(transform.position.y>StartY+0.05f)
            {
                //print(transform.position.y+" > "+(StartY+0.05f));
                if(!attacking)
                anim.SetBool("spin",true);
            }
            /*else
            {
                if(!attacking)
                anim.SetBool("spin",false);
            }*/
            if(grounded&&!attacking&&!dying)
            {
                scanforProjectiles();
                scanForEnemies();
                if(!pScript.dead)
                scanForPlayer();
            }
            if((attacking||!grounded||player.position.x>transform.position.x&&spikeOut)&&!pScript.dead&&pScript.knockbackCor==null)
            {
                if(player.position.x>transform.position.x-0.9f&&!(player.position.x>transform.position.x)&&!dying)
                {
                    if(!grounded)
                    {
                        if(!pScript.midDive)data.playSoundStatic(25);
                        if(!pScript.midSpin||player.position.x>transform.position.x)
                        {
                            pScript.Damage(false,false);
                            if(pScript.knockbackCor!=null)
                            {
                                pScript.StopCoroutine(pScript.knockbackCor);
                                pScript.knockbackCor = null;
                            }
                            pScript.knockbackCor = StartCoroutine(pScript.knockBack(-0.15f,1,0.5f,true));
                        }
                        else
                        {
                            grav.enabled = true;
                            pScript.knockbackCor = StartCoroutine(pScript.knockBack(-0.07f,1,0.5f,true));
                            rb.velocity = new Vector2(rb.velocity.x+10,rb.velocity.y+100);
                            knockedBack = true;
                            anim.SetBool("knockback",knockedBack);
                        }
                    }
                    else
                    {
                        if(!pScript.midDive)data.playSoundStatic(25);
                        if(!pScript.midSpin||player.position.x>transform.position.x)
                        {
                            pScript.Damage(false,false);
                            if(pScript.knockbackCor!=null)
                            {
                                pScript.StopCoroutine(pScript.knockbackCor);
                                pScript.knockbackCor = null;
                            }
                            pScript.knockbackCor = StartCoroutine(pScript.knockBack(-2,2,0.5f,true));
                        }
                        else
                        {
                            grav.enabled = true;
                            pScript.knockbackCor = StartCoroutine(pScript.knockBack(-0.1f,1,0.5f,true));
                            rb.velocity = new Vector2(rb.velocity.x+10,rb.velocity.y+100);
                            knockedBack = true;
                            anim.SetBool("knockback",knockedBack);
                        }
                    }
                }
                else if(player.position.x>transform.position.x+3f&&spikeOut)
                {
                     if(!pScript.midDive)data.playSoundStatic(25);
                    pScript.Damage(false,false);
                    if(pScript.knockbackCor!=null)
                    {
                        pScript.StopCoroutine(pScript.knockbackCor);
                        pScript.knockbackCor = null;
                    }
                    if(!grounded)
                    pScript.knockbackCor = StartCoroutine(pScript.knockBack(-0.2f,1,0.5f,true));
                    else pScript.knockbackCor = StartCoroutine(pScript.knockBack(-2,2,0.5f,true));

                }
            }
            if(!dying)
            spike();
        }
    }
    void scanForPlayer()
    {
        if(player.position.x>transform.position.x-1.2f&&!(player.position.x>transform.position.x))
        {
            attackCor = StartCoroutine(attack());
        }
    }
    void spike()
    {
        if(player.position.x>transform.position.x+1.5f&&!spikeOut)
        {
            spikeTr.position = new Vector3(transform.position.x+3.5f,spikeTr.position.y,spikeTr.position.z);
            spikeOut = true;
            spikeAnim.SetBool("appear",spikeOut);
        }
        else if(player.position.x<transform.position.x&&spikeOut)
        {
            spikeOut = false;
            spikeAnim.SetBool("appear",spikeOut);
        }
    }
    void scanForEnemies()
    {
        Vector3 point = new Vector3(transform.position.x+(Vector3.right.x*transform.localScale.x*0.7f),transform.position.y+0.1f,transform.position.z);
        Debug.DrawLine(point,point+(transform.up)*2f,Color.red);
        RaycastHit2D ray = Physics2D.Raycast(point,transform.up,2f,whatisEnemy);
        if(ray.collider!=null&&attackCor==null)
        {
            //print("found "+ray.collider.transform.name);
            if(ray.collider.transform.name.Contains("mixTape"))
            {
                shellScript s = ray.collider.GetComponent<shellScript>();
                s.transform.GetComponent<MovementAI>().jump(false);
                if(!s.moving)return;

                data.playUnlistedSoundPoint(clingSound,transform.position);
                s.changeDirection(-1,true);
                attackCor = StartCoroutine(attack());
            }
        }
    }
    void scanforProjectiles()
    {
        //print("scan");
        Vector3 point = new Vector3(transform.position.x+(Vector3.right.x*transform.localScale.x*0.7f),transform.position.y+0.1f,transform.position.z);
        Debug.DrawLine(point,point+(transform.up)*2f,Color.blue);
        RaycastHit2D ray = Physics2D.Raycast(point,transform.up,2f,whatisFireball);
        if(ray.collider!=null&&attackCor==null)
        {
            //print("found fireball");
            attackCor = StartCoroutine(attack());
        }
    }
    void OnCollisionEnter2D(Collision2D other)
	{
        if(other.gameObject.tag == "Ground")
        {
            anim.SetBool("knockback",false);
            grav.enabled = false;
            if(knockedBack&&knockbackCor==null)
            {
                knockbackCor = StartCoroutine(knockback());
            }
            grounded = true;
            if(!attacking)
            anim.SetBool("spin",false);
        }
    }
    void OnCollisionExit2D(Collision2D other)
	{
        if(other.gameObject.tag == "Ground")
        {
            grounded = false;
        }
    }
}
