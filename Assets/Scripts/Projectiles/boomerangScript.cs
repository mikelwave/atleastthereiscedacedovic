using System.Collections;
using UnityEngine;

public class boomerangScript : MonoBehaviour {
	public Vector3 endPointOffset;
	Vector3 endPoint;
	public float speed = 12f;
	public float waitUntilReturn = 0.35f;
	Rigidbody2D rb;
	Coroutine cor;
	// Use this for initialization
	void OnEnable ()
	{
		endPoint = transform.position+new Vector3(endPointOffset.x*transform.up.x,endPointOffset.y*transform.up.y,0);
		if(rb==null) rb = GetComponent<Rigidbody2D>();

		if(cor!=null)
		StopCoroutine(cor);

			if(transform.localScale.x==1)
			rb.velocity=new Vector2(speed,0);
			else rb.velocity=new Vector2(-speed,0);
		cor = StartCoroutine(boomerang());
	}
	IEnumerator boomerang()
	{
		bool startRight = true;
		if(rb.velocity.x<0)
		startRight = false;
		yield return new WaitForSeconds(waitUntilReturn);
		if(startRight)
		{
			while(rb.velocity.x>-15)
			{
				if(Time.timeScale!=0)
				rb.velocity = Vector2.MoveTowards(rb.velocity,new Vector2(-15,-5),0.8f);
				yield return 0;
			}
		}
		else
		{
			while(rb.velocity.x<15)
			{
				if(Time.timeScale!=0)
				rb.velocity = Vector2.MoveTowards(rb.velocity,new Vector2(15,-5),0.8f);
				yield return 0;
			}
		}
		Vector3 transformRounded,endPointRounded;
		endPointRounded = new Vector3(Mathf.Round(endPoint.x),Mathf.Round(endPoint.y),transform.position.z);
		transformRounded = new Vector3(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y),transform.position.z);
		while(transformRounded!=endPointRounded)
		{
			if(Time.timeScale!=0)
			{
				transform.position = Vector3.MoveTowards(transform.position,endPointRounded,0.6f);
				endPointRounded = new Vector3(Mathf.Round(endPoint.x),Mathf.Round(endPoint.y),transform.position.z);

				transformRounded = new Vector3(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y),transform.position.z);
			}
			yield return 0;
		}
		gameObject.SetActive(false);
	}
}
