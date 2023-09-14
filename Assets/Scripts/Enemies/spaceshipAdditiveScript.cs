using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteInEditMode]
public class spaceshipAdditiveScript : MonoBehaviour {
	public float speed = 2;
	public int spikeAmount = 2;
	int lastSpikeAmount = 0;
	public float spikeOffsetDistance = 0.75f;
	float lastSpikeOffsetDistance = 0;
	List<GameObject> spikes;
	GameObject spikeSamp;
	EnemyCorpseSpawner eneC;
	bool dead = false;
	public GameObject flipped;
	GameObject spikeFlippedHolder;
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nSpike Speed: "+speed);
	}
	// Use this for initialization
	void Start ()
	{
		if(dataShare.debug)
		printer();
		spikeSamp = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
		if(!Application.isPlaying)
		{
		spikes = new List<GameObject>();
		for(int i = 0; i<spikeSamp.transform.parent.childCount;i++)
		{
			spikes.Add(spikeSamp.transform.parent.GetChild(i).gameObject);
		}
		}
		else
		{
			Transform spinner = transform.GetChild(0).GetChild(0);
			spinner.GetComponent<Spinner>().speed = speed;
			eneC = GetComponent<EnemyCorpseSpawner>();
			spikeFlippedHolder = new GameObject();
			spikeFlippedHolder.name = "spikeFlipHold";
			spikeFlippedHolder.transform.parent = transform.GetChild(0);
			spikes = new List<GameObject>();
			for(int i = 0; i<spikeAmount;i++)
			{
				spinner.GetChild(i).tag="Harm";
				GameObject obj = Instantiate(flipped,transform.position,Quaternion.identity);
				obj.name = "spikeFlip "+i;
				obj.GetComponent<deadEnemyScript>().invertable = false;
				SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
				spr.sprite = spikeSamp.GetComponent<SpriteRenderer>().sprite;
				spr.flipY = false;
				obj.transform.parent = spikeFlippedHolder.transform;
				spikes.Add(spinner.GetChild(i).gameObject);
			}
		}
	}
	// Update is called once per frame
	void Update ()
	{
		if(!Application.isPlaying)
		{
		#if UNITY_EDITOR
		if(lastSpikeAmount!=spikeAmount&&spikeAmount!=0)
		{
			if(PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null)
			{
				PrefabUtility.UnpackPrefabInstance(gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
			}
			float rotationDivide = Mathf.Round(360/spikeAmount);
			lastSpikeAmount = spikeAmount;
			for(int i = 1; i<spikes.Count;i++)
			{
				DestroyImmediate(spikes[i]);
			}
			spikes = new List<GameObject>();
			if(spikeSamp==null) spikeSamp = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
			spikeSamp.transform.eulerAngles = new Vector3(0,0,0);
			spikeSamp.transform.localPosition = new Vector3(0,0,0);
			//spikeSamp.transform.localPosition = spikeSamp.transform.right*spikeOffsetDistance;
			//spikeSamp.transform.localEulerAngles+=new Vector3(0,0,90);
			spikes.Add(spikeSamp);
			for(int i = 1; i<spikeAmount;i++)
			{
				GameObject obj = Instantiate(spikeSamp,transform.position,Quaternion.identity);

				obj.name = "spike";
				obj.transform.parent = spikeSamp.transform.parent;
				obj.transform.localScale = Vector3.one;
				obj.transform.eulerAngles = new Vector3(0,0,(rotationDivide*i));
				//obj.transform.localPosition = obj.transform.right*spikeOffsetDistance;
				//obj.transform.localPosition = Vector3.zero;
				//obj.transform.localEulerAngles=new Vector3(0,0,0);
				spikes.Add(obj);
			}
			lastSpikeOffsetDistance = spikeOffsetDistance+1;
		}
		if(spikeOffsetDistance!=lastSpikeOffsetDistance)
		{
			lastSpikeOffsetDistance = spikeOffsetDistance;
			for(int i = 0; i<spikes.Count;i++)
			{
				spikes[i].transform.position = (spikes[i].transform.up*spikeOffsetDistance)+transform.position;
			}
		}
		#endif
		}
		else
		{
			if(eneC.hitFlag&&!dead)
			{
				dead = true;
				if(transform.GetChild(0).gameObject.activeInHierarchy)
				{
					Transform curSpikeFlipped;
					Rigidbody2D curSpikeRb;
					//Debug.Log(spikes.Count);
					//Debug.Log(spikeFlippedHolder.transform.childCount);
					for(int c = 0; c<spikeAmount;c++)
					{
						//Debug.Log(c);
						//Debug.Log(spikeFlippedHolder.transform.GetChild(c).name);
						curSpikeFlipped = spikeFlippedHolder.transform.GetChild(c);
						curSpikeRb = curSpikeFlipped.GetComponent<Rigidbody2D>();

						curSpikeFlipped.position = spikes[c].transform.position;
						curSpikeFlipped.eulerAngles = spikes[c].transform.eulerAngles;

						curSpikeFlipped.gameObject.SetActive(true);
						curSpikeRb.angularVelocity = Random.Range(-1000,1001);
						curSpikeRb.velocity = new Vector2(Random.Range(-2,3),Random.Range(6,13));
					}
					for(int i = spikeAmount-1; i>=0;i--)
					spikeFlippedHolder.transform.GetChild(i).parent = null;
				}
				Destroy(spikeSamp.transform.parent.gameObject);
				Destroy(gameObject);
			}
		}
	}
}
