using UnityEngine;

public class deadEnemyScript : MonoBehaviour {
	public bool invertable = true;
	void Start()
	{
		if(invertable&&Mathf.Round(transform.eulerAngles.z)!=0)
		{
			Gravity grav =GetComponent<Gravity>();
			grav.pushForces = new Vector2(Mathf.Abs(grav.pushForces.x),-grav.pushForces.y);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			Destroy(gameObject);
		}
	}
}
