using UnityEngine;

public class checkForSemiSolid : MonoBehaviour {
	public bool insideSemiSolid = false;

	void OnTriggerStay2D(Collider2D other)
	{
		if(other.tag == "semiSolid"&&!insideSemiSolid)
		{
			//Debug.Log("Inside semiSolidCollider");
			insideSemiSolid = true;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.tag == "semiSolid")
		{
			//Debug.Log("Inside semiSolidCollider");
			insideSemiSolid = false;
		}
	}
}
