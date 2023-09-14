using UnityEngine;
[ExecuteInEditMode]
public class spawnWithOffset : MonoBehaviour {
	#if UNITY_EDITOR
	public Vector3 placementOffset = Vector3.zero;
	bool spawned = false;
	void Start()
	{
		if(!Application.isPlaying&&!spawned)
		{
			spawned = true;
			transform.position+=placementOffset;
			GetComponent<spawnWithOffset>().enabled = false;
		}
	}
	#endif
}
