using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_projectile : MonoBehaviour {
	public float movementSpeed = 1f;
	public Transform parent;
	[HideInInspector]
	public GameObject[] usedgibs;
	public bool groundCollision = true,playerCollision = true;
	[HideInInspector]
	public Transform target;
	GameData data;
	// Update is called once per frame
	void FixedUpdate () {
		if(Time.timeScale!=0)
		transform.position += transform.right * movementSpeed;
	}
	void Start()
	{
		if(parent!=null)
		data = GameObject.Find("_GM").GetComponent<GameData>();
	}
	void OnEnable()
	{
		if(playerCollision)
		transform.GetChild(0).rotation = Quaternion.identity;

		else
		{
			Transform child = transform.GetChild(0);
			if(transform.eulerAngles.z>=90&&transform.eulerAngles.z<=270)
			child.localScale = new Vector3(1,-1,1);
			else child.localScale = Vector3.one;

		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
			if(parent!=null)
			{
				//transform.SetParent(parent);
				this.gameObject.SetActive(false);
			}
			else Destroy(gameObject);
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Player" && playerCollision
		||other.tag == "Spring" && groundCollision
		||other.tag == "Ground" && groundCollision
		||other.tag == "Harm" && groundCollision
		||other.tag == "blockHoldable" && groundCollision
		||other.tag == "BigBlock" && groundCollision
		||other.tag =="Fireball" && groundCollision
		||other.name == "ScreenNuke" && groundCollision
		||other.tag =="Fireball" && groundCollision
		||other.tag =="Inv" && groundCollision
		||other.tag =="Disc" && groundCollision
		||other.name =="BlockParent(Clone)" && groundCollision
		)
		{
			if(gameObject.activeInHierarchy)
			{
				//print(other.gameObject.tag + " hit.");
				if(usedgibs.Length!=0)
				data.StartCoroutine(playgibs());
				if(parent!=null)
				{
					//transform.SetParent(parent);
					this.gameObject.SetActive(false);
				}
				else Destroy(gameObject);
			}
		}
	}
	IEnumerator playgibs()
	{
		//print("playing");
		for(int i = 0;i<usedgibs.Length;i++)
		{
			if(usedgibs[i]!=null&&!usedgibs[i].activeInHierarchy)
			{
				ParticleSystem particle = usedgibs[i].GetComponent<ParticleSystem>();
				usedgibs[i].transform.position = transform.position;
				usedgibs[i].SetActive(true);
				particle.Play();
				yield return new WaitForSeconds(1.5f);
				particle.Stop(true,ParticleSystemStopBehavior.StopEmitting);
				if(parent!=null)
				{
					usedgibs[i].SetActive(false);
				}
				else if(parent == null && usedgibs[i]!=null) Destroy(usedgibs[i]);
				break;
			}
		}
	}
}
