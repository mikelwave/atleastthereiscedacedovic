using UnityEngine;

public class barBarHead : MonoBehaviour {
	public BarbarianScript sc;
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name.Contains("goblin"))
		{
			sc.playSound(7);
			GetComponent<Rigidbody2D>().angularVelocity = 1000f;
			GetComponent<Rigidbody2D>().velocity = new Vector2(-50,10);
		}
	}
}
