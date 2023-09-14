using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SpaceshipHordeScript : MonoBehaviour {
	public int amountOfShips = 1;
	public float speed = 1f;
	public float movementSpeed = 1f;
	public Vector2 heightRestrictions = new Vector2(2f,-2f);
	Transform player;
	bool visible = false;

	#if UNITY_EDITOR
	GameObject shipObject;
	#endif
	// Use this for initialization
	void Start ()
	{
		#if UNITY_EDITOR
		if(!Application.isPlaying)
		shipObject = transform.GetChild(0).gameObject;
		#endif

		player = GameObject.Find("Player_main").transform;
		if(Application.isPlaying)
		{
			BoxCollider2D box = GetComponent<BoxCollider2D>();
			box.offset = new Vector2(2,0);
			box.size = new Vector2(amountOfShips,5);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		#if UNITY_EDITOR
		if(!Application.isPlaying)
		{
			if(amountOfShips!=transform.childCount)
			{
				if(PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null)
				{
					PrefabUtility.UnpackPrefabInstance(gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
				}
				for(int i = transform.childCount-1; i>0;i--)
				{
					if(i!=0)
					DestroyImmediate(transform.GetChild(i).gameObject);
				}
				//spawn new ships
				for(int k = 0; k<amountOfShips-1;k++)
				{
					GameObject obj;
					obj = Instantiate(shipObject,transform.GetChild(k).position+Vector3.right,Quaternion.identity);
					obj.name = transform.GetChild(0).name;
					obj.transform.SetParent(transform);
				}
			}
		}
		#endif
		if(Application.isPlaying)
		{
			if(visible&&Time.timeScale!=0)
				transform.position+=new Vector3(movementSpeed*-transform.localScale.x,0,0);
			if(transform.childCount==0)
				Destroy(gameObject);
		}
	}
	void LateUpdate ()
	{
		if(Application.isPlaying&&Time.timeScale!=0&&visible)
		{
			if(transform.childCount!=0)
			{
				float targetPos;
				for(int i = 0; i<transform.childCount;i++)
				{
					if(i==0)
					targetPos = Mathf.Clamp(player.position.y,transform.position.y+heightRestrictions.y,transform.position.y+heightRestrictions.x);
					else targetPos = Mathf.Clamp(transform.GetChild(i-1).position.y,transform.position.y+heightRestrictions.y,transform.position.y+heightRestrictions.x);

					//transform.GetChild(i).position = Vector3.MoveTowards(transform.GetChild(i).position,new Vector3(transform.GetChild(i).position.x,targetPos,transform.GetChild(i).position.z),speed);
					transform.GetChild(i).position += (new Vector3(transform.GetChild(i).position.x,targetPos,transform.GetChild(i).position.z) - transform.GetChild(i).position) * speed;
				}
			}

		}
    }
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&!visible)
		{
			visible = true;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&visible)
		{
			visible = false;
		}
	}
}
