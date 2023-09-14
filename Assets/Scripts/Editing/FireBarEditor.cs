using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class FireBarEditor : MonoBehaviour {
	#if UNITY_EDITOR
	[Range (1,10)]
	public int fireballAmount = 4;
	[Range(0,360)]
	public float startRotation = 0;
	public float speed = 70;
	FireBarScript fr;
	Transform tr;
	// Use this for initialization
	void Start ()
	{
		//print("Editor loaded");
		if(!Application.isPlaying)
		{
			fr = GetComponent<FireBarScript>();
			tr = transform.GetChild(0);
			fr.speed = speed;
			//print("App not playing: "+transform.name+" Editor speed: "+speed+" Actual: "+fr.speed);
			tr.eulerAngles = new Vector3(0,0,startRotation);
		}
		else
		{
			fr = GetComponent<FireBarScript>();
			tr = transform.GetChild(0);
			fr.speed = speed;
			//print(transform.name+" Editor speed: "+speed+" Actual: "+fr.speed);
			tr.eulerAngles = new Vector3(0,0,startRotation);
			Destroy(this);
		}
	}
	#if UNITY_EDITOR
	// Update is called once per frame
	void Update ()
	{
		if(!Application.isPlaying)
		{
		fr.speed = speed;
		tr.eulerAngles = new Vector3(0,0,startRotation);
		if(tr.childCount!=fireballAmount)
		{
			//if our desired amount of fireballs is smaller than what we have, destroy some
			if(fireballAmount-1<tr.childCount)
			{
				if(PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null)
				{
					PrefabUtility.UnpackPrefabInstance(gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
				}
				//delete objects first, except first one
				for(int i = tr.childCount-1;i>fireballAmount-1;i--)
					DestroyImmediate(tr.GetChild(i).gameObject);
			}

			if(fireballAmount>tr.childCount)
			{
				//calculate how many fireballs to add
				int add = fireballAmount-1-tr.childCount;
				int lastObjectPos = tr.childCount-1;
				for(int i = 0;i<=add;i++)
				{
					GameObject obj = Instantiate(tr.GetChild(0).gameObject,tr.GetChild(lastObjectPos).position+tr.up,Quaternion.identity);
					obj.transform.SetParent(tr);
					obj.name = "flame";
					lastObjectPos++;
				}
			}
		}
		for(int i = 0; i<tr.childCount;i++)
		{
			tr.GetChild(i).rotation = Quaternion.Euler(Vector3.zero);
		}
		}
	}
	#endif
	#endif
}
