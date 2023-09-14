using UnityEngine;

public class floppyKiller : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		if(GameObject.Find("_GM").GetComponent<GameData>().currentLevelProgress.Contains("C"))
		{
			transform.name = "FloppyGhost";
			transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1,1,1,0.5019608f);
			//Destroy(gameObject);
		}
	}
}
