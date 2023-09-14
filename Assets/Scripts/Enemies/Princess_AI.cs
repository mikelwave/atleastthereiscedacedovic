using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Princess_AI : MonoBehaviour
{
    Transform player,spriteObj;
    PlayerScript pScript;
    playerSprite pSprites;
    GameData data;
    SpriteRenderer render;
    public float speed = 5f,spinSpeed = 5f;
    float savedSpinSpeed;
    public bool seesPlayer = false,spinning = false;
    bool active,resetting = false;
    LayerMask playerMask;
    [Range (-100,0)]
    public float LeftDistance = -40;
    [Range (0,100)]
    public float RightDistance = 40;
    Animator anim;
    List <GameObject> projectiles;
    public int storedHeads = 4;
    public GameObject Head;
    Vector3 startPos;
    public int BPM = 120,prevBPM;
	public AudioClip newMusic,prevMusic;
    public GameObject cam,Quad;
    Renderer QuadRenderer;
    public Color QuadTint;
    Coroutine tint;
    [ExecuteInEditMode]
    IEnumerator startSpinning()
    {
        yield return new WaitForSeconds(1f);
        Vector3 targetPos = spriteObj.localPosition+spriteObj.up*4;
        float t = 0.0f;
        spinning = true;
        while(Mathf.Abs(Mathf.Abs(targetPos.y)-Mathf.Abs(spriteObj.localPosition.y))>0.1f)
        {
            if(Time.timeScale!=0)
            {
                spriteObj.localPosition = new Vector3(spriteObj.localPosition.x, Mathf.Lerp(spriteObj.localPosition.y, targetPos.y, t), spriteObj.localPosition.z);
                t += 0.1f * Time.deltaTime;
            }
            yield return 0;
        }
        StartCoroutine(speedIncrease());
    }
    IEnumerator attackLoop()
    {
        yield return new WaitForSeconds(1f);
        while(seesPlayer)
        {
            yield return new WaitForSeconds(4f);
            if(seesPlayer)
            {
                anim.SetTrigger("throw");
            }
        }
    }
    IEnumerator speedIncrease()
    {
        while(speed<13)
        {
            if(Time.timeScale!=0)
            {
                speed+=8f * Time.deltaTime;
            }
            yield return 0;
        }
        speed = 13;
    }
    IEnumerator startChase()
    {
        speed = 0;
        Vector3 targetPos = transform.localPosition+Vector3.up*2;
        float t = 0.0f;
        while(Mathf.Abs(Mathf.Abs(targetPos.y)-Mathf.Abs(transform.localPosition.y))>0.01f)
        {
            //print(Mathf.Abs(Mathf.Abs(targetPos.y)-Mathf.Abs(transform.localPosition.y)));
            //transform.parent.localPosition = Vector2.MoveTowards(transform.localPosition,targetPos,speed*Time.deltaTime);
            if(Time.timeScale!=0)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, targetPos.y, t), transform.localPosition.z);
                t += 0.5f * Time.deltaTime;
            }
            yield return 0;
        }
        //targetPos = transform.localPosition-Vector3.up*2;
        //t = 0.0f;
        //if(player.localPosition.y<transform.localPosition.y-0.5f)
        if(tint!=null)StopCoroutine(tint);
        tint = StartCoroutine(tintScreen(true));

        while(transform.localPosition!=Vector3.zero)
        {
            if(Time.timeScale!=0)
            {
                //transform.parent.localPosition = Vector2.MoveTowards(transform.localPosition,targetPos,speed*Time.deltaTime);
                //transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, targetPos.y, t), transform.localPosition.z);
                //t += 0.2f * Time.deltaTime;
                transform.localPosition = Vector3.MoveTowards(transform.localPosition,Vector3.zero,2f * Time.deltaTime);
            }
            yield return 0;
        }
        //transform.localPosition = Vector3.zero;
        StartCoroutine(attackLoop());
    }
    IEnumerator flyAway()
    {
        while(speed>0||spinSpeed<0)
        {
            if(Time.timeScale!=0)
            {
                if(spinSpeed<0)
                spinSpeed+=1;
                transform.localEulerAngles+=new Vector3(0,0,spinSpeed*Time.deltaTime);
                transform.parent.position = Vector2.MoveTowards(transform.parent.position,player.position+player.up,speed*Time.deltaTime);
                spriteObj.eulerAngles = Vector3.zero;
                if(speed>0)
                speed-=10f * Time.deltaTime;
                else if(speed<0)speed = 0;

                if(spinSpeed>0)spinSpeed = 0;
            }
            yield return 0;
        }
        //print(speed+" "+spinSpeed);
        spinSpeed = 0;
        Vector3 point = new Vector3(spriteObj.position.x-(spriteObj.localScale.x*10),spriteObj.position.y+50,0);
        if(tint!=null)StopCoroutine(tint);

        if(!checkForOtherEnemiesOfType())
        data.StartCoroutine(tintScreen(false));
        while(spriteObj.gameObject.activeInHierarchy)
        {
            if(Time.timeScale!=0)
            {
                speed+=20f * Time.deltaTime;
                transform.parent.position = Vector2.MoveTowards(transform.parent.position,point,speed*Time.deltaTime);
            }
            yield return 0;
        }
    }
    public void playPop()
    {
        data.playSound(86,spriteObj.position);
    }
    public void throwHead()
    {
        data.playSound(87,spriteObj.position);
        Vector3 playerPos = new Vector3(player.position.x,player.position.y+0.5f,player.position.z),
        spawnPos = spriteObj.position+(spriteObj.up*0.3f);
		Vector3 dist = playerPos - spawnPos;
		float rotationZ = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
		Quaternion rot = Quaternion.Euler(0.0f, 0.0f, rotationZ);

		for(int i = 0; i<projectiles.Count;i++)
		{
			if(!projectiles[i].activeInHierarchy)
			{
				projectiles[i].transform.position = spawnPos;
				projectiles[i].transform.rotation = rot;
				projectiles[i].transform.parent = null;
                projectiles[i].transform.localScale = Vector3.one;
				projectiles[i].SetActive(true);
				break;
			}
		}
    }
    // Start is called before the first frame update
    public IEnumerator tintScreen(bool fadeIn)
    {
        //print("Active");
        float targetTint = 0.3f,TintAdditive = targetTint/60f;
        QuadTint.a = QuadRenderer.sharedMaterial.GetColor("_Color").a;
        if(fadeIn&&QuadRenderer.sharedMaterial.GetColor("_Color").a<targetTint)
        {
            //print("Fade In");
            int frames = 0;
            while(QuadRenderer.sharedMaterial.GetColor("_Color").a<targetTint-0.005f&&frames<=60)
            {
                if(Time.timeScale!=0)
                {
                    QuadTint.a = Mathf.Clamp(QuadTint.a+TintAdditive,0,targetTint);
                    //print("Frame: "+frames+" Tint: "+QuadTint.a+" Tint Additive: "+TintAdditive);
                    QuadRenderer.sharedMaterial.SetColor("_Color",QuadTint);
                    frames++;
                }
                yield return 0;
            }
            QuadTint.a = targetTint;
            QuadRenderer.sharedMaterial.SetColor("_Color",QuadTint);
            frames = 0;
        }
        else if(!fadeIn&&QuadRenderer.sharedMaterial.GetColor("_Color").a>0.005f)
        {
            //print("Fade Out");
            int frames = 0;
            while(QuadRenderer.sharedMaterial.GetColor("_Color").a>0.005f&&frames<=60)
            {
                if(Time.timeScale!=0)
                {
                    QuadTint.a = Mathf.Clamp(QuadTint.a-TintAdditive,0,targetTint);
                    //print("Frame: "+frames+" Tint: "+QuadTint.a+" Tint Additive: "+TintAdditive);
                    QuadRenderer.sharedMaterial.SetColor("_Color",QuadTint);
                    frames++;
                }
                yield return 0;
            }
            QuadTint.a = 0;
            QuadRenderer.sharedMaterial.SetColor("_Color",QuadTint);
            frames = 0;
        }
    }
    void Start()
    {
        if(Application.isPlaying)
        {
            if(cam!=null&&Quad!=null)
            {
                bool found1 = false,found2 = false;
                foreach (GameObject dup in GameObject.FindGameObjectsWithTag ("MainCamera"))
                {
                    if (dup.name=="PrincessCamera"&&!dup.Equals(cam))
                    {
                        found1 = true;
                        Destroy(cam);
                        cam = dup;
                        //print("found existing camera on the level, deleting personal camera");
                        continue;
                    }

                    if(dup.name=="Quad2"&&!dup.Equals(Quad))
                    {
                        found2 = true;
                        Destroy(Quad);
                        Quad = dup;
                        //print("found existing quad on the level, deleting personal quad");
                        continue;
                    }
                }
                if(!found1)
                {
                    cam.transform.SetParent(GameObject.Find("QuadCamera").transform,false);
                }
                if(!found2)
                {
                    Quad.transform.SetParent(GameObject.Find("Quad").transform.parent,false);
                    Quad.transform.position = GameObject.Find("Quad").transform.position-(Vector3.forward*0.005f);
                    Quad.transform.localScale = new Vector3(16,9,0);
                }
                cam.transform.localPosition = Vector3.zero;
                cam.SetActive(true);
                Quad.SetActive(true);
                QuadRenderer = Quad.GetComponent<Renderer>();
                QuadTint = QuadRenderer.sharedMaterial.GetColor("_Color");
                QuadRenderer.sharedMaterial.SetColor("_Color",new Color(QuadTint.r,QuadTint.g,QuadTint.b,0));
            }
            data = GameObject.Find("_GM").GetComponent<GameData>();
            player = GameObject.Find("Player_main").transform;
            pScript = player.GetComponent<PlayerScript>();
            pSprites = player.GetComponent<playerSprite>();
            spriteObj = transform.GetChild(0);
            storeLevelData levelData = GameObject.Find("LevelGrid").GetComponent<storeLevelData>();
            prevMusic = levelData.Music;
			prevBPM = levelData.BPM;
            playerMask |= (1 << LayerMask.NameToLayer("Player"));
            playerMask |= (1 << LayerMask.NameToLayer("Ground"));
            anim = GetComponent<Animator>();
            render = spriteObj.GetComponent<SpriteRenderer>();
            projectiles = new List<GameObject>();
            for(int i = 0; i<storedHeads; i++)
            {
                GameObject obj = Instantiate(Head,Vector3.zero,Quaternion.identity);
                obj.transform.parent = transform.GetChild(0);
                obj.SetActive(false);
                enemy_projectile proj = obj.GetComponent<enemy_projectile>();
                proj.parent = transform.GetChild(0);
                proj.target = player;
                //obj.GetComponent<enemy_projectile>().usedgibs = null;
                projectiles.Add(obj);
            }
            RightDistance +=transform.parent.position.x;
            LeftDistance +=transform.parent.position.x;
            savedSpinSpeed = spinSpeed;
            if(!seesPlayer)
            {
                reset(false);
            }
            startPos = transform.parent.position;
        }
    }
    void reset(bool resetPos)
    {
        spinSpeed = savedSpinSpeed;
        render.sortingLayerName = "Default";
        render.sortingOrder = -1;
        transform.localEulerAngles = new Vector3(0,0,270);
        transform.localPosition = new Vector3(0,-0.35f,0);
        speed = 5;
        anim.speed = 0;
        spinning = false;
        StopAllCoroutines();
        if(resetPos)
        {
            transform.parent.position = startPos;
            spriteObj.localPosition = Vector3.zero;
            spriteObj.localEulerAngles = Vector3.zero;
            spriteObj.localScale = Vector3.one;
            resetting = false;
        }
    }
    IEnumerator gameOverTurnOff()
    {
        yield return new WaitUntil(()=>Time.timeScale==0);
        QuadTint.a = 0;
        QuadRenderer.sharedMaterial.SetColor("_Color",QuadTint);
    }
    void Update()
    {
        if(active&&Time.timeScale!=0&&Application.isPlaying)
        {
            if(pScript.dead&&!resetting)
            {
                seesPlayer = false;
                resetting = true;
                StartCoroutine(flyAway());
                if(data.lives==0)
                {
                    StartCoroutine(gameOverTurnOff());
                }

            }
            if(Time.frameCount%5==0&&!seesPlayer&&!resetting)
            findPlayer();

            if(seesPlayer)
            {
                if(spriteObj.localScale.x== 1&&player.position.x>=spriteObj.position.x
                ||spriteObj.localScale.x ==-1&&player.position.x<spriteObj.position.x)
                spriteObj.localScale=new Vector3(-spriteObj.localScale.x,spriteObj.localScale.y,spriteObj.localScale.z);

                if(spriteObj.position.x>=RightDistance&&!resetting||spriteObj.position.x<=LeftDistance&&!resetting)
                {
                    if(spriteObj.position.y-1f>player.position.y)
                    {
                        //print("flies away");
                        //print(spriteObj.position.x+" "+RightDistance+" "+LeftDistance);
                        seesPlayer = false;
                        resetting = true;
                        data.StartCoroutine(flyAway());
                    }
                }
            }
        }
        else if(!Application.isPlaying)
		{
			Debug.DrawLine(new Vector3(transform.position.x+LeftDistance,transform.position.y,transform.position.z),new Vector3(transform.position.x+RightDistance,transform.position.y,transform.position.z),Color.red);
		}

        /*if(!spinning&&Mathf.Abs(Mathf.Abs(player.position.x)-Mathf.Abs(transform.position.x))<5f)
        {
            spinning = true;
            
        }*/
    }
    void FixedUpdate()
    {
        if(seesPlayer)
        {
            if(spinning)
            transform.localEulerAngles+=new Vector3(0,0,spinSpeed*Time.deltaTime);

            transform.parent.position = Vector2.MoveTowards(transform.parent.position,player.position+player.up,speed*Time.deltaTime);
            spriteObj.eulerAngles = Vector3.zero;
        }
    }
    void findPlayer()
    {
		Vector3 playerpos;
		if(pSprites.state==0||pScript.crouching)
		{
			playerpos = player.position;
		}
		else
		{
			playerpos = player.position+(player.up*0.75f);
		}
        Vector3 point = new Vector3(transform.position.x,transform.position.y+transform.up.y*1.5f,transform.position.z);
        RaycastHit2D ray;
        if(Mathf.Round(player.eulerAngles.z)==0)
        ray = Physics2D.Raycast(point,playerpos-point+(Vector3.up*0.5f),10f,playerMask);
        else ray = Physics2D.Raycast(point,playerpos-point-(Vector3.up*0.5f),10f,playerMask);
        if(ray.collider!=null)
        {
            Debug.DrawLine(point,ray.point,Color.blue,1f);
            //print(ray.collider.name);
            if(ray.collider.name=="PlayerCollider"&&!seesPlayer)
            {
                //Debug.DrawLine(point,ray.point,Color.blue,1f/60*5);
                seesPlayer = true;
                StartCoroutine(startChase());
                StartCoroutine(startSpinning());
                anim.speed = 1;
                render.sortingLayerName = "PlayerCollider";
                render.sortingOrder = 1;
                transform.localEulerAngles = Vector3.zero;
                transform.localPosition = Vector3.zero;
                transform.parent.position+=new Vector3(0,0.2f,0);
                if(data.getMusicClip()!=newMusic)
                {
                    data.changeMusicInSubArea(newMusic,BPM);
                }
            }
        }
    }
    public void DeathEvent()
    {
        if(!checkForOtherEnemiesOfType())
        {
            if(!pScript.dead&&!pScript.reachedGoal)
            data.changeMusicInSubArea(prevMusic,prevBPM);
            data.StartCoroutine(tintScreen(false));
        }
    }
    bool checkForOtherEnemiesOfType()
    {
        bool found = false;
        foreach (GameObject dup in GameObject.FindGameObjectsWithTag ("Boss"))
        {
			if (dup.transform!=transform.parent)
            {
                if(dup.transform.name==transform.parent.name&&dup.transform.GetChild(0).GetComponent<Princess_AI>().seesPlayer)
                {
                    //print("More princesses found, not switching music");
                    //print(dup.transform.GetChild(0).GetChild(0).gameObject+" active: "+dup.transform.GetChild(0).GetChild(0).gameObject.activeInHierarchy);
                    found = true;
                    break;
                }
            }
		}
        return found;
    }
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&!active)
		{
            active = true;
            spriteObj.gameObject.SetActive(true);
        }
    }
    void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&active)
		{
            //Debug.Log("Player exit, sees: "+seesPlayer);
            if(!seesPlayer)
            {
                active = false;
                spriteObj.gameObject.SetActive(false);
                if(resetting)
                {
                    if(!pScript.dead)
                    {
                        if(!checkForOtherEnemiesOfType()&&!pScript.dead&&!pScript.reachedGoal)
                        data.changeMusicInSubArea(prevMusic,prevBPM);
                        reset(true);
                    }
                }
            }
            else
            {
                float dist = Mathf.Abs(Vector2.Distance(player.position,transform.position));
                //Debug.Log(dist);
                if(dist>25)
                {
                    if(!resetting)
                    {
                        seesPlayer = false;
                        resetting = true;
                        if(!checkForOtherEnemiesOfType()&&!pScript.dead&&!pScript.reachedGoal)
                        data.changeMusicInSubArea(prevMusic,prevBPM);
                        reset(true);
                    }
                    data.StartCoroutine(tintScreen(false));
                }
            }
        }
    }
}
