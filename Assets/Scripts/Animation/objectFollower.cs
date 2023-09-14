using UnityEngine;

public class objectFollower : MonoBehaviour {
	public Transform obj;
	public bool follow = false;

	void Update () {
		if(obj!=null&follow)
		{
			transform.position = obj.position;
		}
	}
}
