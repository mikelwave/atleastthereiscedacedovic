using UnityEngine;

public class hammerScript : MonoBehaviour {
	Rigidbody2D rb;
	public Vector2 direction = new Vector2(0,10);
	public Transform parentObj;
	public bool destroyOnEnd = false;
	// Use this for initialization
	void OnEnable ()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.velocity = Vector2.zero;
		rb.velocity = new Vector2(direction.x*transform.localScale.x,direction.y*transform.localScale.y);
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			if(!destroyOnEnd)
			{
				transform.SetParent(parentObj);
				gameObject.SetActive(false);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}

}
