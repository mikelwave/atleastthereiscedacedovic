using UnityEngine;

public class disabler : MonoBehaviour {
	public GameObject toDisable;
	public bool destroy = false;
	void Start()
	{
		if(toDisable == null)
		toDisable = gameObject;
	}
	public void disable()
	{
		if(!destroy)
		toDisable.SetActive(false);
		else Destroy(toDisable);
	}
}
