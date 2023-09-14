using UnityEngine;
[ExecuteInEditMode]
public class sayPosition : MonoBehaviour {

	// Use this for initialization
	void OnEnable ()
	{
		if(!Application.isPlaying)
		print(gameObject.name+" "+transform.position);
	}
}
