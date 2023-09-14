using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]
public class coinRingScript : MonoBehaviour
{
    [Range (-20,20)]
    public float speed = 2;
    [Range (2,30)]
	public int coinAmount = 2;
	int lastCoinAmount = 0;
    [Range (0.3f,10)]
	public float coinOffsetDistance = 0.75f;
	float lastCoinOffsetDistance = 0;
    public Color color = new Color(1,1,1,1);
	List<GameObject> coins;
	public GameObject coinSamp;
    bool active = false;
    CircleCollider2D col;
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CircleCollider2D>();
        col.radius = coinOffsetDistance+0.5f;
        transform.GetChild(0).GetComponent<Spinner>().speed = speed;
        if(coinSamp==null)
        coinSamp = transform.GetChild(0).GetChild(0).gameObject;
		coins = new List<GameObject>();
        for(int i = 0; i<transform.GetChild(0).childCount;i++)
        {
            coins.Add(transform.GetChild(0).GetChild(i).gameObject);
        }
    }
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = true;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = false;
		}
	}
    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying)
		{
		#if UNITY_EDITOR
		if(lastCoinAmount!=coinAmount&&coinAmount!=0)
		{
			if(PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null)
			{
				PrefabUtility.UnpackPrefabInstance(gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
			}
			//float rotationDivide = 360/(coinAmount);
			lastCoinAmount = coinAmount;
			for(int i = 0; i<coins.Count;i++)
			{
				DestroyImmediate(coins[i]);
			}
			coins = new List<GameObject>();
			if(coinSamp!=null)
            {
                //coinSamp.transform.eulerAngles = new Vector3(0,0,0);
                //coinSamp.transform.localPosition = new Vector3(0,0,0);
                //coinSamp.transform.localPosition = coinSamp.transform.right*coinOffsetDistance;
                //coinSamp.transform.localEulerAngles+=new Vector3(0,0,90);
                //coins.Add(coinSamp);
                for(int i = 0; i<coinAmount;i++)
                {
                    float angle = i * Mathf.PI * 2 / coinAmount;
                    float x = Mathf.Cos(angle) * coinOffsetDistance;
                    float y = Mathf.Sin(angle) * coinOffsetDistance;
                    Vector3 pos = transform.position + new Vector3(x, y, 0);
                    float angleDegrees = -angle*Mathf.Rad2Deg;
                    Quaternion rot = Quaternion.Euler(0, 0, angleDegrees);

                    //GameObject obj = PrefabUtility.InstantiatePrefab(coinSamp) as GameObject;
                    GameObject obj = Instantiate(coinSamp,pos,rot);
                    //obj.transform.position = transform.position;
                    obj.name = "Coin";
                    obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
                    obj.GetComponent<coinScript>().setParentToItems = false;
                    obj.transform.SetParent(transform.GetChild(0));
                    obj.transform.localScale = Vector3.one;
                    //obj.transform.eulerAngles = new Vector3(0,0,(rotationDivide*(i+1)));
                    //obj.transform.eulerAngles = Vector3.zero;
                    //obj.transform.localPosition = obj.transform.right*coinOffsetDistance;
                    //obj.transform.localPosition = Vector3.zero;
                    //obj.transform.localEulerAngles=new Vector3(0,0,0);
                    coins.Add(obj);
                }
                lastCoinOffsetDistance = coinOffsetDistance+1;
            }
            else Debug.LogError("No coin prefab assigned.");
		}
		if(coinOffsetDistance!=lastCoinOffsetDistance)
		{
			lastCoinOffsetDistance = coinOffsetDistance;
            if(col==null)col = GetComponent<CircleCollider2D>();
                col.radius = coinOffsetDistance+0.5f;
			for(int i = 0; i<coins.Count;i++)
			{
				coins[i].transform.position = (coins[i].transform.up*coinOffsetDistance)+transform.position;
			}
		}
		#endif
		}
        else
        {
            if(transform.GetChild(0).childCount==0)
            {
                Destroy(gameObject);
            }
            if(Time.timeScale!=0&&active)
            {
                for(int i = 0;i<coins.Count;i++)
                {
                    if(coins[i]!=null)
                    coins[i].transform.rotation = Quaternion.Euler(Vector3.zero);
                }
            }
        }
    }
}
