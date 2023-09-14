using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class DeathHeadStringScript : MonoBehaviour
{
    [Header ("Editor values")]
    [Space]
    [Range(1,50)]
    public int headsAmount = 5;
    private int m_headsAmount = 1;
    [Range(3,50)]
    public int length = 5;
    private int m_length = 5;
    [Range(-25,25)]
    public int headsSpeed = 1;
    [Space]
    List <GameObject> heads;
    Transform headsHolder;
    bool active = false;
    Vector2 Yrange;
    public Vector2 deathHeadHitbox = new Vector2(0.75f,0.75f);
    public float sinMultiplier = 5;
    public float sinMax = 0.5f;
    public float sinDelay = 2;
    void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nHead Speed: "+headsSpeed+"\nDeath head hitbox: "+deathHeadHitbox+"\nSin Mult: "+sinMultiplier
        +"\nSin max: "+sinMax+"\nSin delay: "+sinDelay);
	}
    // Start is called before the first frame update
    void Start()
    {
        if(Application.isPlaying)
        {
            if(dataShare.debug)
            printer();
            getGameValues();
        }
        #if UNITY_EDITOR
        else getEditorValues();
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if(Application.isPlaying&&active)
        {
            headMovement();
            if(headsHolder.childCount==0)
            {
                Destroy(gameObject);
            }
        }
        #if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            Debug.DrawLine(transform.position,new Vector3(transform.position.x,transform.position.y-length,transform.position.z),Color.green);
            if(m_headsAmount!=headsAmount)
            {
                m_headsAmount = headsAmount;
                changeHeadCount(headsAmount);
            }
            if(m_length!=length)
            {
                m_length = length;
                repositionHeads();
            }
        }
        #endif
    }
    void getGameValues()
    {
        headsHolder = transform.GetChild(0);
        heads = new List<GameObject>();
        SimpleAnim2 anim2 = GetComponent<SimpleAnim2>();
        anim2.render = new SpriteRenderer[headsHolder.childCount];
        for(int i = 0; i<headsHolder.childCount;i++)
        {
            Transform t = headsHolder.GetChild(i);
            Rigidbody2D rb = t.gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
            heads.Add(t.gameObject);
            t.GetComponent<EnemyCorpseSpawner>().screenNukeKills = false;
            anim2.render[i] = t.GetChild(0).GetComponent<SpriteRenderer>();
            t.GetChild(0).GetComponent<BoxCollider2D>().size = deathHeadHitbox;
        }
        Yrange = new Vector2(transform.position.y,transform.position.y-(float)length);

    }
    #if UNITY_EDITOR
    void getEditorValues()
    {
        headsHolder = transform.GetChild(0);
        heads = new List<GameObject>();
        for(int i = 0; i<headsHolder.childCount;i++)
        {
            heads.Add(headsHolder.GetChild(i).gameObject);
        }
    }
    void changeHeadCount(int amount)
    {
        if(PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null)
		{
			PrefabUtility.UnpackPrefabInstance(gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
		}
        for(int i = headsHolder.childCount-1;i>0;i--)
        {
            DestroyImmediate(headsHolder.GetChild(i).gameObject);
        }
        heads = new List<GameObject>();
        heads.Add(headsHolder.GetChild(0).gameObject);

        float yPos = float.Parse(length.ToString())/float.Parse(headsAmount.ToString());
        headsHolder.GetChild(0).position = new Vector3(transform.position.x,transform.position.y-yPos/2,transform.position.z);

        for(int i = 1;i<headsAmount;i++)
        {
            GameObject obj=Instantiate(heads[0].gameObject,new Vector3(transform.position.x,transform.position.y-(yPos*(i)+(yPos/2)),transform.position.z),Quaternion.identity);
            obj.transform.parent = headsHolder;
            obj.name = heads[0].name;
            heads.Add(obj);
        }
    }
    void repositionHeads()
    {
        float yPos = float.Parse(length.ToString())/float.Parse(headsAmount.ToString());
        headsHolder.GetChild(0).position = new Vector3(transform.position.x,transform.position.y-yPos/2,transform.position.z);
        for(int i = 1; i<heads.Count;i++)
        {
            heads[i].transform.position = new Vector3(transform.position.x,transform.position.y-(yPos*(i)+(yPos/2)),transform.position.z);
        }
    }
    #endif
    void headMovement()
    {
        for(int i = 0; i<heads.Count;i++)
        {
            if(heads[i]!=null)
            {
                Transform head = heads[i].transform;
                head.position -= new Vector3(0,headsSpeed*Time.deltaTime,0);
                if(head.position.y<=Yrange.y&&headsSpeed>0)
                {
                    head.position+=new Vector3(0,(float)length,0);
                }
                if(head.position.y>=Yrange.x&&headsSpeed<0)
                {
                    head.position-=new Vector3(0,(float)length,0);
                }
                float lineWidth = Mathf.Clamp(sinMax*Mathf.Sin(((float)i/sinDelay+Time.timeSinceLevelLoad)*sinMultiplier),-sinMax,sinMax);
                head.position = new Vector3(lineWidth+transform.position.x,head.position.y,head.position.z);
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = true;
            headsHolder.gameObject.SetActive(active);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = false;
            headsHolder.gameObject.SetActive(active);
		}
	}
}
