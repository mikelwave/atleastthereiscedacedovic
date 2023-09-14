using UnityEngine;

public class BarbarianAnimScript : MonoBehaviour {
	BarbarianScript bScript;
	void Start()
	{
		bScript = transform.parent.GetComponent<BarbarianScript>();
	}
	public void Fire()
	{
		bScript.Fire();
	}
	public void spawnHead()
	{
		bScript.spawnHead();
	}
	public void playSound(int ID)
	{
		bScript.playSound(ID);
	}
}
