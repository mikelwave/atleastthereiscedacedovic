using UnityEngine;

[ExecuteInEditMode]
public class trashFlyScript : MonoBehaviour {
	public Vector2 firstPoint = new Vector2(0,2);
	public Vector2 secondPoint = new Vector2(0,-2);
	public float maxSpeed = 1f;
	Vector2 targetPoint;
	Vector3 target;
	public bool flipOnPointChange = false;
	public GameObject replacement;
	GameObject spawnedReplacement;
	private Vector3 velocity = Vector3.zero;
	public float damping = 0.3f;
	EnemyCorpseSpawner eneC;
	EnemyOffScreenDisabler eneOff;
	Vector3 childOffset;
	public bool setChildOffset = true;
	string position1,positionTarget;
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nFirst point: "+
		firstPoint+"\nSecond point: "+secondPoint+"\nDamping: "+damping+"\nSpeed: "+maxSpeed);
	}
	// Use this for initialization
	void Start () {
		if(Application.isPlaying)
		{
			if(dataShare.debug)
			printer();
			eneC = GetComponent<EnemyCorpseSpawner>();
			eneOff = GetComponent<EnemyOffScreenDisabler>();
			firstPoint = new Vector2(transform.position.x+firstPoint.x,transform.position.y+firstPoint.y);
			secondPoint = new Vector2(transform.position.x+secondPoint.x,transform.position.y+secondPoint.y);
			//Vector3 cPos = transform.GetChild(0).localPosition;
			//transform.GetChild(0).localPosition = new Vector3(cPos.x,0,cPos.z);
			Debug.DrawLine(new Vector3(firstPoint.x,firstPoint.y,transform.position.z),new Vector3(secondPoint.x,secondPoint.y,transform.position.z),Color.red,5f);
			if(replacement!=null)
			{
				spawnedReplacement = Instantiate(replacement,transform.position,Quaternion.identity);
				spawnedReplacement.transform.GetChild(0).GetComponent<SpriteRenderer>().color = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
				spawnedReplacement.transform.localScale = transform.localScale;
				spawnedReplacement.transform.SetParent(transform.parent);
				spawnedReplacement.SetActive(false);
			}
			targetPoint = secondPoint;
			if(setChildOffset)
			{
				childOffset = transform.GetChild(0).localPosition;
				transform.position+=childOffset;
			}
			setTarget();
		}
	}
	void setTarget()
	{
		target = new Vector3(targetPoint.x,targetPoint.y,transform.position.z);
		positionTarget = targetPoint.x.ToString("F1")+" "+targetPoint.y.ToString("F1");
	}
	// Update is called once per frame
	void Update () {
		if(!Application.isPlaying)
		{
			Debug.DrawLine(new Vector3(transform.position.x+firstPoint.x,transform.position.y+firstPoint.y,transform.position.z),new Vector3(transform.position.x+secondPoint.x,transform.position.y+secondPoint.y,transform.position.z),Color.green);
		}
	}
	void spawnReplacement()
	{
		Destroy(gameObject);
		spawnedReplacement.transform.position = transform.position;
		spawnedReplacement.transform.localScale = transform.localScale;
		spawnedReplacement.SetActive(true);
	}
	void FixedUpdate()
	{
		if(Application.isPlaying)
		{
			if(eneOff!=null&&eneOff.visible||eneOff==null)
			{
				transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, damping,maxSpeed);
				position1 = transform.position.x.ToString("F1")+" "+transform.position.y.ToString("F1");
			}

			if(position1==positionTarget)
			{
				if(flipOnPointChange)
					transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
				if(targetPoint==secondPoint)
				targetPoint = firstPoint;
				else targetPoint = secondPoint;

				setTarget();
			}

			if(eneC!=null&&eneC.stompFlag)
			{
				eneC.stompFlag = false;
				if(replacement!=null)
					spawnReplacement();
			}
		}
	}
}
