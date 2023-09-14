using UnityEngine;

[ExecuteInEditMode]
public class keyScript : MonoBehaviour {

	public Color32[] colors = new Color32[3];
	public int ID = 0;	
	void Start ()
	{
		if(Application.isPlaying)
		{
			GameData data = GameObject.Find("_GM").GetComponent<GameData>();
			switch(ID)
			{
				default: if(data.hasRed)Destroy(transform.parent.gameObject);break;
				case 1: if(data.hasBlue)Destroy(transform.parent.gameObject);break;
				case 2: if(data.hasYellow)Destroy(transform.parent.gameObject);break;
			}
		}
		if(GameObject.Find("KeysHold")==null)
		{
			//Debug.Log("Creating a keysHold object as an existing one wasn't found.");
			GameObject keyHold = new GameObject();
			keyHold.name = "KeysHold";
			keyHold.transform.SetParent(GameObject.Find("LevelGrid").transform);
			keyHold.transform.localPosition = Vector3.zero;
		}
		transform.parent.SetParent(GameObject.Find("KeysHold").transform);
	}
	// Update is called once per frame
	void Update ()
	{
		if(!Application.isPlaying)
		{
			ID = Mathf.Abs(ID);
			if(ID<colors.Length)
			GetComponent<SpriteRenderer>().color = colors[ID];
			else{GetComponent<SpriteRenderer>().color = new Color32(0,0,0,255);ID = 0;}
		}
	}
}
