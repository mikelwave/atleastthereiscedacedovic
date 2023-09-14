using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ChickenScript : MonoBehaviour
{
    [Header ("Editor values")]
    [Space]
    [Range(1,50)]
    public int chickensAmount = 5;
    private int m_chickensAmount = 1;
    [Range(3,50)]
    public int length = 5;
    private int m_length = 5;
    [Range(-25,25)]
    public int chickensSpeed = 1;
    [Space]
    [Header("Pre-Swoop")]
    [Space]
    public float cameraYSnapPosition = 9f;
    public float xFollowSpeed = 1;
    public float playerOffset = 3;
    [Space]
    List <GameObject> chickens;
    Transform chickensHolder;
    bool active = false;
    public Vector2 deathChickenHitbox = new Vector2(0.75f,0.75f);
    public float sinMultiplier = 5;
    public float sinMax = 0.5f;
    public float sinDelay = 2;
    GameData data;


    //chicken
    Transform cam;
    Transform player;
    int sequence = 0;
    float neg = 1;

    // Start is called before the first frame update
    void Start()
    {
        if(Application.isPlaying)
        getGameValues();

        #if UNITY_EDITOR
        else getEditorValues();
        #endif
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(Application.isPlaying&&active)
        {
            if(Time.timeScale!=0)
            switch(sequence)
            {
                default: break;
                case 1: followPlayerX(); break;
                case 3: chickenMovement(); break;
            }
            if(chickensHolder.childCount==0)
            {
                Destroy(gameObject);
            }
        }
        #if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            Debug.DrawLine(transform.position,new Vector3(transform.position.x,transform.position.y-length,transform.position.z),Color.green);
            if(m_chickensAmount!=chickensAmount)
            {
                m_chickensAmount = chickensAmount;
                changeChickenCount(chickensAmount);
            }
            if(m_length!=length)
            {
                m_length = length;
                repositionchickens();
            }
        }
        #endif
    }
    void getGameValues()
    {
        cam = GameObject.Find("Main Camera").transform;
        player = GameObject.Find("Player_main").transform;
        data = GameObject.Find("_GM").GetComponent<GameData>();
        chickensHolder = transform.GetChild(0);
        chickens = new List<GameObject>();
        SimpleAnim2 anim2 = GetComponent<SimpleAnim2>();
        anim2.render = new SpriteRenderer[chickensHolder.childCount];
        for(int i = 0; i<chickensHolder.childCount;i++)
        {
            chickens.Add(chickensHolder.GetChild(i).gameObject);
            anim2.render[i] = chickensHolder.GetChild(i).GetChild(0).GetComponent<SpriteRenderer>();
            chickensHolder.GetChild(i).GetChild(0).GetComponent<BoxCollider2D>().size = deathChickenHitbox;

            if(i!=chickensHolder.childCount-1)chickens[i].SetActive(false);
        }
        chickensHolder.gameObject.SetActive(active);
        cameraYSnapPosition = cameraYSnapPosition+(length-5);

    }
    //Ingame
    void progressSequence()
    {
        sequence++;
        //1. attach to camera, follow player on x
        //2. go up offscreen
        //3. swoop down, kill offscreen
        switch(sequence)
        {
            default: break;
            case 1: transform.position = new Vector3(transform.position.x,cam.position.y+cameraYSnapPosition,transform.position.z); break;
            case 3: chickenSwoopPrepare(); break;
        }
    }
    void followPlayerX()
    {
        //print("Following x");
        transform.position += (new Vector3(player.position.x+(playerOffset*player.localScale.x),transform.position.y,transform.position.z) - transform.position) * xFollowSpeed;
        transform.position = new Vector3(transform.position.x,cam.position.y+cameraYSnapPosition,transform.transform.position.z);
    }
    IEnumerator behaviourSequence()
    {
        data.playSoundStatic(66);
        yield return new WaitForSeconds(2f);
        //go up
        progressSequence();
        float target = cam.position.y+13f+(length-5);
        float additive = 0.0001f;
        neg = player.transform.localScale.x;
        while(transform.position.y<target)
        {
            transform.position += new Vector3(additive*2*neg,additive,0);
            if(additive<0.5f)
            {
                additive = Mathf.Clamp(additive+additive,0,0.5f);
            }
            yield return 0;
        }
        data.playSoundStatic(67);
        progressSequence();
        //go down
    }
    void chickenSwoopPrepare()
    {
        transform.position = new Vector3(player.position.x+(playerOffset*neg),transform.position.y,transform.position.z);
        for(int i = 0; i<chickens.Count;i++)
        {
            if(chickens[i]!=null)
            {
                chickens[i].SetActive(true);
                chickens[i].GetComponent<EnemyCorpseSpawner>().killOffscreen = true;
            }
        }
    }
    void chickenMovement()
    {
        for(int i = 0; i<chickens.Count;i++)
        {
            if(chickens[i]!=null)
            {
                Transform chicken = chickens[i].transform;
                chicken.position -= new Vector3(0,chickensSpeed*Time.deltaTime,0);
                /*if(chicken.position.y<=Yrange.y&&chickensSpeed>0)
                {
                    chicken.position+=new Vector3(0,(float)length,0);
                }
                if(chicken.position.y>=Yrange.x&&chickensSpeed<0)
                {
                    chicken.position-=new Vector3(0,(float)length,0);
                } */
                float lineWidth = Mathf.Clamp(sinMax*Mathf.Sin(((float)i/sinDelay+Time.timeSinceLevelLoad)*sinMultiplier),-sinMax,sinMax);
                chicken.position = new Vector3(lineWidth+transform.position.x,chicken.position.y,chicken.position.z);
            }
        }
    }

    //Editor
    #if UNITY_EDITOR
    void getEditorValues()
    {
        chickensHolder = transform.GetChild(0);
        chickens = new List<GameObject>();
        for(int i = 0; i<chickensHolder.childCount;i++)
        {
            chickens.Add(chickensHolder.GetChild(i).gameObject);
        }
    }

    void changeChickenCount(int amount)
    {
        if(PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null)
		{
			PrefabUtility.UnpackPrefabInstance(gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
		}
        for(int i = chickensHolder.childCount-1;i>0;i--)
        {
            DestroyImmediate(chickensHolder.GetChild(i).gameObject);
        }
        chickens = new List<GameObject>();
        chickens.Add(chickensHolder.GetChild(0).gameObject);

        float yPos = float.Parse(length.ToString())/float.Parse(chickensAmount.ToString());
        chickensHolder.GetChild(0).position = new Vector3(transform.position.x,transform.position.y-yPos/2,transform.position.z);

        for(int i = 1;i<chickensAmount;i++)
        {
            GameObject obj=Instantiate(chickens[0].gameObject,new Vector3(transform.position.x,transform.position.y-(yPos*(i)+(yPos/2)),transform.position.z),Quaternion.identity);
            obj.transform.parent = chickensHolder;
            obj.name = chickens[0].name;
            chickens.Add(obj);
        }
    }

    void repositionchickens()
    {
        float yPos = float.Parse(length.ToString())/float.Parse(chickensAmount.ToString());
        chickensHolder.GetChild(0).position = new Vector3(transform.position.x,transform.position.y-yPos/2,transform.position.z);
        for(int i = 1; i<chickens.Count;i++)
        {
            chickens[i].transform.position = new Vector3(transform.position.x,transform.position.y-(yPos*(i)+(yPos/2)),transform.position.z);
        }
    }
    #endif

    //Triggers
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = true;
            chickensHolder.gameObject.SetActive(active);
            if(sequence==0)
            {
                progressSequence();
                StartCoroutine(behaviourSequence());
            }
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = false;
            chickensHolder.gameObject.SetActive(active);
		}
	}
}
