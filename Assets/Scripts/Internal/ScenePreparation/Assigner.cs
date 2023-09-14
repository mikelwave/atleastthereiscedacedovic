using UnityEngine;
public class Assigner : MonoBehaviour {
	public string parent = "Special";
	// Use this for initialization
	void Start ()
	{
		transform.SetParent(GameObject.Find(parent).transform);
		Destroy(this);
	}
}
