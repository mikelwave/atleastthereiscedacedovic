using System.Collections;
using UnityEngine;

public class FireBarScript : MonoBehaviour {
	public float speed = 1;
	Transform main;
	SimpleAnim2 anim2;
	Transform[] fires;
	bool active = false;
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nSpeed: "+speed);
	}
	// Use this for initialization
	void Start ()
	{
		main = transform.GetChild(0);
		anim2 = GetComponent<SimpleAnim2>();
		anim2.render = new SpriteRenderer[main.childCount];
		fires = new Transform[main.childCount];
		for(int i = 0;i<main.childCount;i++)
		{
			Transform t = main.GetChild(i);
			t.tag = "FlameHarm";
			fires[i] = t;
			anim2.render[i] = fires[i].GetComponent<SpriteRenderer>();
		}
		if(!active)
		toggleFires(active);
		//StartCoroutine(getFireSpeed());
		if(dataShare.debug)
		printer();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Time.timeScale!=0&&active)
		{
			main.Rotate(Vector3.forward*Time.deltaTime*speed);
			foreach(Transform tr in fires)
			{
				tr.rotation = Quaternion.Euler(Vector3.zero);
			}
		}
	}
	public void DestroyFires()
	{
		StartCoroutine(DestroyFiresCor());
	}
	IEnumerator DestroyFiresCor()
	{
		for(int i = fires.Length-1;i>=0;i--)
		{
			fires[i].gameObject.SetActive(false);
			yield return new WaitForSeconds(0.02f);
		}
	}
	void toggleFires(bool toggle)
	{
		foreach (Transform tr in fires)
		{
			tr.gameObject.SetActive(toggle);
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = true;
			toggleFires(active);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = false;
			toggleFires(active);
		}
	}
}
