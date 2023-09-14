using System.Collections;
using UnityEngine;

public class spawnCubeScript : MonoBehaviour
{
    public LayerMask whatIsGround;
    Vector3 travelPoint = Vector3.zero;
    public float speed = 1;
    public Vector2 randomSpotRange = new Vector2(-3,3);
    Vector3 raySpot;
    bool travelling = false;
    Transform chtr;
    GameData data;
    public GameObject linkedToSpawnObject,objectToSpawnAtPoint;
    public bool debugActivate = false,active = false;
    IEnumerator spawnAnim()
    {
        data.playSound(98,chtr.position);
        Vector3 targetScale = Vector3.one*1.5f,startScale = Vector3.zero;
        float progress = 0;
        while(progress<1)
        {
            progress = Mathf.Clamp(progress+=Time.deltaTime*20,0,1);
            chtr.localScale = Vector3.Lerp(startScale,targetScale,progress);
            yield return 0;
            yield return new WaitUntil(()=>Time.timeScale!=0);
        }
        startScale = chtr.localScale;
        targetScale = Vector3.one;
        progress = 0;
        while(progress<1)
        {
            progress = Mathf.Clamp(progress+=Time.deltaTime*20,0,1);
            chtr.localScale = Vector3.Lerp(startScale,targetScale,progress);
            yield return 0;
            yield return new WaitUntil(()=>Time.timeScale!=0);
        }
    }
    IEnumerator endAnim()
    {
        data.playSound(99,chtr.position);
        linkedToSpawnObject.SetActive(true);
        Transform spTr = null;
        if(objectToSpawnAtPoint!=null)
        {
            objectToSpawnAtPoint.transform.position = new Vector3(chtr.position.x,Mathf.Floor(chtr.position.y),0);
            spTr = objectToSpawnAtPoint.transform;
        }
        Vector3 targetScale = Vector3.zero,startScale = chtr.localScale;
        Vector3 targetScaleObj = Vector3.one*1.5f,startScaleObj = Vector3.zero;
        float progress = 0;
        if(spTr==null)spTr = linkedToSpawnObject.transform;
        spTr.localScale = startScaleObj;
        while(progress<1)
        {
            progress = Mathf.Clamp(progress+=Time.deltaTime*20,0,1);
            chtr.localScale = Vector3.Lerp(startScale,targetScale,progress);
            spTr.localScale = Vector3.Lerp(startScaleObj,targetScaleObj,progress);
            yield return 0;
            yield return new WaitUntil(()=>Time.timeScale!=0);
        }
        progress = 0;
        targetScaleObj = Vector3.one;
        startScaleObj = spTr.localScale;
        while(progress<1)
        {
            progress = Mathf.Clamp(progress+=Time.deltaTime*20,0,1);
            spTr.localScale = Vector3.Lerp(startScaleObj,targetScaleObj,progress);
            yield return 0;
            yield return new WaitUntil(()=>Time.timeScale!=0);
        }
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        if(linkedToSpawnObject!=null)
        {
            linkedToSpawnObject.SetActive(false);
        }
        else Destroy(gameObject);
        chtr = transform.GetChild(0);
        chtr.localScale = Vector3.zero;
        data = GameObject.Find("_GM").GetComponent<GameData>();
        chtr.gameObject.SetActive(false);
    }
    public void activate()
    {
        active = true;
        chtr.gameObject.SetActive(true);
        raySpot = transform.position+new Vector3(Random.Range(randomSpotRange.x,randomSpotRange.y+0.01f),0,0);
        RaycastHit2D ray = Physics2D.CircleCast(raySpot,0.2f,Vector2.down,50,whatIsGround);
        if(ray.collider==null)
        {
            travelPoint = raySpot+(Vector3.down*50);
        }
        else
        {
            travelPoint = new Vector3(ray.centroid.x,ray.centroid.y,raySpot.z);
        }
        travelling = true;
        StartCoroutine(spawnAnim());
        Debug.DrawLine(chtr.position,travelPoint,Color.red,2f);
    }
    // Update is called once per frame
    void Update()
    {
        if(travelling)
        {
            transform.position = Vector3.MoveTowards(transform.position,travelPoint,speed*Time.deltaTime);
            if(transform.position==travelPoint)
            {
                travelling = false;
                //print("Spawning object");
                StartCoroutine(endAnim());
            }
        }
        if(debugActivate)
        {
            debugActivate = false;
            activate();
        }
    }
}
