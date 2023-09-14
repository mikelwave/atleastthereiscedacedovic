using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(skeletonPlantScript))]
public class SkeletonHeadShooter : MonoBehaviour {
	int directionChanger = 1;
	skeletonPlantScript plantScript;
	Transform player;
	bool facingRight = true;
	bool changeVertically = false;
	public GameObject boneproj;
	public GameObject bonegibs;
	GameObject[] usedgibs;
	List <GameObject> projectiles;
	public int storedBones = 4;
	public int storedParticles = 1;
	Transform spawner;
	bool attacking = false;
	public bool turnToPlayer = true;
	Coroutine coroutine;
	Vector3 dist;
	public Sprite[] animations;

	// Use this for initialization
	void Start () {
		int rotation = Mathf.RoundToInt(transform.parent.localEulerAngles.z);
		plantScript = GetComponent<skeletonPlantScript>();
		player = GameObject.Find("Player_main").transform;
		spawner = transform.GetChild(0);
		usedgibs = new GameObject[storedParticles];
		Color mainCol = GetComponent<SpriteRenderer>().color;
		for(int i = 0; i<usedgibs.Length;i++)
		{
			usedgibs[i] = Instantiate(bonegibs,Vector3.zero,Quaternion.identity);
			var main = usedgibs[i].GetComponent<ParticleSystem>().main;
			main.startColor = mainCol;
			usedgibs[i].SetActive(false);
			usedgibs[i].transform.SetParent(null);
		}
		projectiles = new List<GameObject>();
		for(int i = 0; i<storedBones; i++)
		{
			GameObject obj = Instantiate(boneproj,Vector3.zero,Quaternion.identity);
			//obj.transform.SetParent(transform.GetChild(0));
			obj.transform.SetParent(null);
			obj.SetActive(false);
			obj.GetComponent<enemy_projectile>().parent = spawner;
			obj.GetComponent<enemy_projectile>().usedgibs = usedgibs;
			obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = mainCol;
			projectiles.Add(obj);
		}

		if(plantScript.distance.x != 0)
			changeVertically = true;

		switch(rotation)
		{
			default: directionChanger = 1; break;
			case 180: directionChanger = -1; facingRight = false;break;
			case 270: directionChanger = -1; break;
		}
	}
	void Update()
	{
		if(turnToPlayer)
		{
			if(!changeVertically)
			{
				if(player.position.x<=transform.position.x && !facingRight)
				{
					facingRight = true;
					transform.localScale = new Vector3 (1*directionChanger,1,1);
				}
				else if(player.position.x>transform.position.x && facingRight)
				{
					facingRight = false;
					transform.localScale = new Vector3 (-1*directionChanger,1,1);
				}
			}
			else if(changeVertically)
			{
				if(player.position.y>transform.position.y && !facingRight)
				{
					facingRight = true;
					transform.localScale = new Vector3 (-1*directionChanger,1,1);
				}
				else if(player.position.y<=transform.position.y && facingRight)
				{
					facingRight = false;
					transform.localScale = new Vector3 (1*directionChanger,1,1);
				}	
			}
		}
		if(plantScript.canAttack&&!attacking)
		{
			attacking = true;
			if(coroutine!=null)
				StopCoroutine(coroutine);
			coroutine = StartCoroutine(attackLoop());
		}
	}
	void OnDisable()
	{
		attacking = false;
		GetComponent<SpriteRenderer>().sprite = animations[0];
		if(coroutine!=null)
			StopCoroutine(coroutine);
	}
	void OnDestroy()
	{
		bool allDisabled = true;
		if(projectiles!=null&&projectiles.Count!=0)
		for(int i = 0; i<projectiles.Count;i++)
		{
			if(projectiles[i].gameObject!=null&&!projectiles[i].activeInHierarchy)
			{
			allDisabled = false;
			Destroy(projectiles[i].gameObject);
			}
		}
		if(allDisabled&&usedgibs!=null&&usedgibs.Length!=0)
		for(int i = 0; i<usedgibs.Length;i++)
		{
			if(usedgibs[i].gameObject!=null&&!usedgibs[i].activeInHierarchy)
			{
			Destroy(usedgibs[i].gameObject);
			}
		}
	}
	public void attack(bool targetPlayer)
	{
		if(targetPlayer)
		{
			Vector3 playerPos = new Vector3(player.position.x,player.position.y+0.5f,player.position.z);
			dist = playerPos - spawner.position;
			float rotationZ = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
			spawner.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);

			for(int i = 0; i<projectiles.Count;i++)
			{
				if(!projectiles[i].activeInHierarchy)
				{
				projectiles[i].transform.position = spawner.position;
				projectiles[i].transform.rotation = spawner.rotation;
				projectiles[i].transform.SetParent(null);
				projectiles[i].SetActive(true);
				break;
				}
			}
		}
		else
		{
			for(int c = 0; c<3;c++)
			{
				spawner.eulerAngles = transform.parent.eulerAngles+new Vector3(0,0,90);
				if(c==1)
				spawner.eulerAngles+= new Vector3(0,0,30);
				else if(c==2)
				spawner.eulerAngles+= new Vector3(0,0,-30);
				for(int i = 0; i<projectiles.Count;i++)
				{
					if(!projectiles[i].activeInHierarchy)
					{
					projectiles[i].transform.position = spawner.position;
					projectiles[i].transform.rotation = spawner.rotation;
					projectiles[i].transform.SetParent(null);
					projectiles[i].SetActive(true);
					break;
					}
				}
			}
		}
	}
	IEnumerator attackLoop()
	{
		GetComponent<SpriteRenderer>().sprite = animations[0];
		yield return new WaitForSeconds(0.75f);
		while(plantScript.canAttack&&gameObject.activeInHierarchy)
		{
			if(plantScript.canAttack)
			{
				GetComponent<SpriteRenderer>().sprite = animations[1];
				attack(turnToPlayer);
			}
			yield return new WaitForSeconds(0.30f);
			GetComponent<SpriteRenderer>().sprite = animations[0];
			yield return new WaitForSeconds(0.45f);
		}
		attacking = false;
	}
}
