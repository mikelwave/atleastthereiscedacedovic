using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KingTrashScript : MonoBehaviour {
	BossFightScript bossF;
	Animator anim;
	Tilemap map;
	MGCameraController cam;
	public TileBase trashTile;
	List<Vector3Int> trashBags;
	ParticleSystem trashGibs;
	public Vector3[] trashKingPositions;
	bool canSpit = false;
	public GameObject trashJectile;
	GameObject enemyParent;
	Coroutine cor;
	public GameObject finalDeathzone;
	public bool dead = false;
	public AudioClip[] sounds;
	public Transform cutscene;
	public Transform cameraTarget;
	GameData data;
	dataShare dataS;

	// Use this for initialization
	void Start () {
		dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		bossF = GetComponent<BossFightScript>();
		if(dataS.checkpointValue==1)
		{
			transform.position = trashKingPositions[2];
			canSpit = false;
		}
		anim = transform.GetChild(0).GetComponent<Animator>();
		map = GameObject.Find("MainMap").GetComponent<Tilemap>();
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		//data.inSub = true;
		//data.switchArea(true);
		//data.switchParallax(true);
		enemyParent = GameObject.Find("BossEnemies");
		trashBags = new List<Vector3Int>();
		trashGibs = transform.GetChild(0).transform.GetChild(1).GetComponent<ParticleSystem>();
		BoundsInt bounds = map.cellBounds;
		int Zpos = bounds.z;
		for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
		{
			for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
			{
				if(map.GetTile(new Vector3Int(x,y,Zpos))!=null&&map.GetTile(new Vector3Int(x,y,Zpos))==trashTile)
				{
					trashBags.Add(new Vector3Int(x,y,Zpos));
				}
			}
		}
		cutscene.gameObject.SetActive(true);
		if(dataS.checkpointValue!=1)
		{
			transform.position = trashKingPositions[0];
			canSpit = true;
			cor = StartCoroutine(spitTrash());
		}
		cam.lockscroll = true;
	}
	IEnumerator spitTrash()
	{
		yield return new WaitForSeconds(4f);
		while(canSpit)
		{
			if(enemyParent.transform.childCount<10 &&canSpit &&!dead)
			{
				anim.SetTrigger("spit");
				int rando = Random.Range(0,2);
				GetComponent<AudioSource>().PlayOneShot(sounds[rando]);
				yield return new WaitForSeconds(0.5f);
				GameObject obj = Instantiate(trashJectile,new Vector3(transform.position.x-4.78f,transform.position.y+3.16f,transform.position.z),Quaternion.identity);
				obj.transform.parent = enemyParent.transform;
				obj.SetActive(true);
			}
			yield return new WaitForSeconds(4f);
		}
	}
	IEnumerator stompCutscene()
	{
			canSpit = false;
			GameObject enemyMainParent = GameObject.Find("Enemies");
			for(int i = 0; i<enemyParent.transform.childCount; i++)
			{
				enemyParent.transform.GetChild(i).SetParent(enemyMainParent.transform);
			}
			if(cor!=null)
				StopCoroutine(cor);
			anim.ResetTrigger("spit");
			if(!dead)
			{
				GetComponent<AudioSource>().PlayOneShot(sounds[2]);
				trashGibs.Play();
				cam.lockCamera = true;
				cam.shakeCamera(0.1f,0.2f);
				cam.nukeEvent = true;
				anim.SetBool("stomp",true);
				GetComponent<AudioSource>().PlayOneShot(sounds[4]);
				trashDestroy(bossF.eventInt);
				yield return new WaitForSeconds(1f);
				GetComponent<AudioSource>().PlayOneShot(sounds[6]);
				cam.shakeCamera(0.2f,0.7f);
				yield return new WaitForSeconds(0.4f);
				anim.SetBool("stomp",false);
				cam.fadeScreen(true);
				yield return new WaitUntil(()=>cam.fadeAnim>=1);
				yield return new WaitForSeconds(0.3f);
				bossF.setPositions(bossF.eventInt);
				yield return new WaitForSeconds(0.1f);
				anim.SetTrigger("flee");
				cam.fadeScreen(false);
				GetComponent<Gravity>().enabled = false;
				yield return new WaitForSeconds(0.7f);
				transform.position = trashKingPositions[bossF.eventInt];
				GetComponent<Gravity>().enabled = true;
				yield return new WaitUntil(()=>cam.fadeAnim<=0);
				//Debug.Log("New Scene");
				bossF.cutscene(false);
				cam.lockCamera = false;
				canSpit = true;
				if(cor!=null)
					StopCoroutine(spitTrash());
				if(bossF.eventInt < 2)
				cor = StartCoroutine(spitTrash());
			}
			else
			{
				GetComponent<AudioSource>().PlayOneShot(sounds[3]);
				Destroy(finalDeathzone);
				cam.target = cameraTarget;
				cam.workInStoppedTime = true;
				trashGibs.Play();
				cam.shakeCamera(0.1f,0.2f);
				cam.nukeEvent = true;
				anim.SetBool("Dead",true);
				anim.SetBool("stomp",true);
				trashDestroy(bossF.eventInt);
				GetComponent<AudioSource>().PlayOneShot(sounds[4]);
				canSpit = false;
				if(cor!=null)
					StopCoroutine(cor);
				yield return new WaitForSeconds(2f);
				GetComponent<AudioSource>().PlayOneShot(sounds[5]);
				cam.shakeCamera(0.2f,3f);
				yield return new WaitForSeconds(1f);
				cam.fadeScreen(true);
				yield return new WaitForSeconds(3f);
				data.stopAllMusic();
				bool newClear = false;
				if(data.currentLevelProgress!="")
				{
					char c = data.currentLevelProgress[0];
					if(c=='N')
						newClear = true;
					if(c!='D')
					{
						data.currentLevelProgress = data.currentLevelProgress.Remove(0, 1);
						if(data.cheated||dataS.difficulty!=2)
						data.currentLevelProgress = data.currentLevelProgress.Insert(0,"F");
						else data.currentLevelProgress = data.currentLevelProgress.Insert(0,"D");
					}
					
					dataS.coins+=data.coins;
					data.saveLevelProgress(newClear,false);
					yield return new WaitUntil(()=> data.finishedSaving);
					dataS.resetValues();
				}
				if(newClear)dataS.loadWorldWithLoadScreen(2);
				else dataS.loadWorldWithLoadScreen(dataS.currentWorld);
			}

	}
	// Update is called once per frame
	void Update () {
		if(bossF.newEvent)
		{
			if(bossF.eventInt == 3)
				dead = true;

			bossF.newEvent = false;
			bossF.cutscene(true);
			StartCoroutine(stompCutscene());
		}
	}
	public void trashDestroy(int step)
	{
		int minY = 0;
		var sh = trashGibs.shape;
		if(step==1)
		{
			minY = 3;
			sh.position = Vector3.zero;
			sh.scale = new Vector3(1,22,2.25f);
		}
		if(step==2)
		{
			minY = -13;
			sh.position = new Vector3(3.5f,0,0);
			sh.scale = new Vector3(1,15,2.25f);
		}
		if(step==3)
		{
			minY = -63;
			sh.position = new Vector3(4,0,0);
			sh.scale = new Vector3(1,14,2.25f);
		}
			for(int i = trashBags.Count-1; i>=0;i--)
			{
				if(trashBags[i].y<minY)
					break;
				else map.SetTile(trashBags[i],null);
			}
	}
}
