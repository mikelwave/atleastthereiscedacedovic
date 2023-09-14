using System.Collections.Generic;
using UnityEngine;

public class HTF_StageHazard : MonoBehaviour {
	Animator anim;
	Transform player;
	public float animalHeightOverPlayer = 30f;
	GameData data;
	int pooledAnimals = 2;
	List<GameObject> animals;
	public GameObject animalProj;
	ParticleSystem sys;
	PlayerScript sc;
	bool turnedoff = false;
	GameObject areaParallax;
	SpriteRenderer[] renders = new SpriteRenderer[3];
	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player").transform;
		areaParallax = GameObject.Find("MainAreaParallax");
		sc = player.parent.GetComponent<PlayerScript>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		animals = new List<GameObject>();
		sys = transform.GetChild(1).GetComponent<ParticleSystem>();
		anim = GetComponent<Animator>();
		renders[0] = GetComponent<SpriteRenderer>();
		renders[1] = transform.GetChild(0).GetComponent<SpriteRenderer>();
		renders[2] = transform.parent.GetComponent<SpriteRenderer>();
		for(int i = 0; i<pooledAnimals;i++)
		{
			GameObject obj = Instantiate(animalProj,transform.position,Quaternion.identity);
			obj.SetActive(false);
			obj.GetComponent<HTF_Enemy>().sys = sys;
			animals.Add(obj);
		}
	}
	void Update()
	{
		if(!sc.controllable && !turnedoff && !sc.reachedGoal && !sc.dead)
		{
			turnedoff = true;
			anim.SetBool("shooting",true);
		}
		if(!areaParallax.activeInHierarchy&&renders[0].enabled)
		{
			renders[0].enabled = false;
			renders[1].enabled = false;
			renders[2].enabled = false;
		}
		else if(areaParallax.activeInHierarchy&&!renders[0].enabled)
		{
			renders[0].enabled = true;
			renders[1].enabled = true;
			renders[2].enabled = true;
		}
	}
	public void spawnAnimal()
	{
		if(areaParallax.activeInHierarchy)
		for(int i = 0; i<animals.Count;i++)
		{
			if(!animals[i].activeInHierarchy)
			{
				Vector3 target = new Vector3(player.position.x,player.position.y+animalHeightOverPlayer,player.position.z);
				animals[i].transform.position = target;
				animals[i].SetActive(true);
				break;
			}
		}
	}
	public void playSound()
	{
		if(areaParallax.activeInHierarchy&&!sc.reachedGoal&& !sc.dead)
		data.playSoundStatic(33);
	}
}
