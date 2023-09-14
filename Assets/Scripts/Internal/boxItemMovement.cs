using UnityEngine;

public class boxItemMovement : MonoBehaviour {

	public float moveSpeed = -2f;
	// Update is called once per frame
	void Update () {
		if(Time.timeScale!=0 &&moveSpeed!=0)
		transform.Translate(transform.up*moveSpeed*Time.deltaTime);
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			Destroy(gameObject);
		}
	}
}
