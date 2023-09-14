using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HitBlockScript : MonoBehaviour {
	/*
	O - Brick bounce
	1 - Brick break
	2 - Slash block bounce (coin)
	3 - Slash block bounce (powerup)
	4 - Brick bounce (powerup)
	5 - Brick bank
	6 - Slash block bank
	*/
	public int state = 0;
	public float zOffset = 0;
	public TileBase[] deadBlocks = new TileBase[2];
	public TileBase ghostBlock;
	public Sprite[] blocksprites;
	public GameObject coin;
	public GameObject powerup;
	public GameObject enemy;
	public AudioClip brickShatter;
	GameObject storedItem;
	SpriteRenderer render;
	Color32 color;
	GameObject obj;
	GameObject power;
	GameData data;
	BoxCollider2D col;
	public hitBlockStore hStore;
	AudioSource aSource;
	IEnumerator die(float time,Tilemap map, TileBase t, Vector3Int pos,bool enableCol)
	{
		if(enableCol)
		col.enabled = true;
		yield return new WaitForSeconds(0.15f);
		col.enabled = false;
		yield return new WaitForSeconds(time-0.15f);

		if(map.GetTile(pos) == null || map.GetTile(pos).name == ghostBlock.name)
		map.SetTile(pos,t);
		gameObject.SetActive(false);
	}
	IEnumerator brickDestroy(Tilemap map,Vector3Int pos)
	{
		yield return 0;
		map.SetTile(pos,null);
	}
	// Use this for initialization
	void Awake () {
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		col = GetComponent<BoxCollider2D>();
		color = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		aSource = GetComponent<AudioSource>();

	}
	public void storeItem(GameObject obj)
	{
		storedItem = obj;
	}
	public void activate(Tilemap map, TileBase t, Vector3Int pos, bool invert)
	{
		float offset = 1f;
		if(invert)
		{
			offset = -1;
			transform.eulerAngles = new Vector3(0,0,180);
			render.flipX=true;
			render.flipY=true;
		}
		else
		{
			transform.eulerAngles = new Vector3(0,0,0);
			render.flipX=false;
			render.flipY=false;
		}
		transform.position = new Vector3(pos.x+0.5f,pos.y+0.5f,pos.z);
		if(GetComponent<ParticleSystem>().isPlaying)
		GetComponent<ParticleSystem>().Stop();
		SpriteRenderer renderPower;
		killOnEnd objKill;
		switch(state)
		{
			//O - Brick bounce
			default:
			Tile e = map.GetTile(pos) as Tile;
			render.sprite = e.sprite;
			//Debug.Log(e.sprite);
			map.SetTile(pos,ghostBlock);
			StartCoroutine(die(0.3f,map,t,pos,true));
			break;
			//1 - Brick break
			case 1:
			map.SetTile(pos,ghostBlock);
			
				render.sprite = null;
				GetComponent<ParticleSystem>().Play();

			data.addScore(50);
			aSource.PlayOneShot(brickShatter);
			StartCoroutine(die(1.15f,map,null,pos,true));
			StartCoroutine(brickDestroy(map,pos));
			break;
			//2 - Slash block bounce (coin)
			case 2:
			map.SetTile(pos,ghostBlock);
			render.sprite = blocksprites[1];
			obj = Instantiate(coin,transform.position,Quaternion.identity);
			obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
			obj.transform.eulerAngles = transform.eulerAngles;
			obj.SetActive(true);
			aSource.PlayOneShot(data.sounds[0]);
			StartCoroutine(die(0.3f,map,deadBlocks[0],pos,true));
			break;
			//3 - Slash block bounce (powerup)
			case 3:
			map.SetTile(pos,ghostBlock);
			render.sprite = blocksprites[1];
			if(map.GetTile(pos+(Vector3Int.up)*(invert?-1:1))==null)
			{
				power = Instantiate(powerup,new Vector3(transform.position.x,transform.position.y+offset,transform.position.z),Quaternion.identity);
				power.transform.eulerAngles = transform.eulerAngles;
				renderPower = power.transform.GetChild(0).GetComponent<SpriteRenderer>();
				renderPower.color = color;
				renderPower.sprite = storedItem.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
				objKill = power.transform.GetChild(0).GetComponent<killOnEnd>();
				objKill.prefab = storedItem;
				objKill.color = color;
				objKill.zOffset = zOffset;
				power.SetActive(true);
				data.sfxSource.PlayOneShot(data.sounds[5]);
			}
			else data.sfxSource.PlayOneShot(data.sounds[118]);
			StartCoroutine(die(0.3f,map,deadBlocks[0],pos,true));
			break;
			//4 - Brick bounce (powerup)
			case 4:
			map.SetTile(pos,ghostBlock);
			render.sprite = blocksprites[0];
			if(map.GetTile(pos+(Vector3Int.up)*(invert?-1:1))==null)
			{
				power = Instantiate(powerup,new Vector3(transform.position.x,transform.position.y+offset,transform.position.z),Quaternion.identity);
				power.transform.eulerAngles = transform.eulerAngles;
				renderPower = power.transform.GetChild(0).GetComponent<SpriteRenderer>();
				renderPower.color = color;
				renderPower.sprite = storedItem.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
				objKill = power.transform.GetChild(0).GetComponent<killOnEnd>();
				objKill.prefab = storedItem;
				objKill.color = color;
				objKill.zOffset = zOffset;
				power.SetActive(true);
				data.sfxSource.PlayOneShot(data.sounds[5]);
			}
			else data.sfxSource.PlayOneShot(data.sounds[118]);
			StartCoroutine(die(0.3f,map,deadBlocks[0],pos,true));
			break;
			//5 - Brick bank
			case 5:
			if(!data.bankPositions.Contains(pos))
			{
			data.bankPositions.Add(pos);
			data.StartCoroutine(data.coinBankCountDown(5f,t,map,pos));
			}
			map.SetTile(pos,ghostBlock);
			render.sprite = blocksprites[0];
			obj = Instantiate(coin,transform.position,Quaternion.identity);
			obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
			obj.transform.eulerAngles = transform.eulerAngles;
			obj.SetActive(true);
			aSource.PlayOneShot(data.sounds[0]);
			StartCoroutine(die(0.3f,map,t,pos,true));
			break;
			//6 - SlashBlock bank
			case 6:
			if(!data.bankPositions.Contains(pos))
			{
			data.bankPositions.Add(pos);
			data.StartCoroutine(data.coinBankCountDown(5f,t,map,pos));
			}
			map.SetTile(pos,ghostBlock);
			render.sprite = blocksprites[1];
			obj = Instantiate(coin,transform.position,Quaternion.identity);
			obj.transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
			obj.transform.eulerAngles = transform.eulerAngles;
			obj.SetActive(true);
			aSource.PlayOneShot(data.sounds[0]);
			StartCoroutine(die(0.3f,map,t,pos,true));
			break;
			//7 - Brick break (enemy)
			case 7:
			map.SetTile(pos,null);

				render.sprite = null;
				GetComponent<ParticleSystem>().Play();

			data.addScore(50);

				power = Instantiate(enemy,new Vector3(transform.position.x,transform.position.y,transform.position.z),Quaternion.identity);
				if(invert)
				{
					Gravity g = power.GetComponent<Gravity>();
					g.savedPushForces = new Vector2(g.savedPushForces.x,Mathf.Abs(g.savedPushForces.y));
					g.pushForces = g.savedPushForces;
					power.GetComponent<bumbo_AI>().inverted = true;
					power.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
				}
				power.transform.eulerAngles = transform.eulerAngles;
				renderPower = power.transform.GetChild(0).GetComponent<SpriteRenderer>();
				renderPower.color = color;
				power.SetActive(true);

			aSource.PlayOneShot(brickShatter);
			StartCoroutine(die(1.15f,map,null,pos,true));
			break;
			//8 - Brick explode
			case 8:
			map.SetTile(pos,null);
			
				render.sprite = null;
				GetComponent<ParticleSystem>().Play();

			aSource.PlayOneShot(brickShatter);
			StartCoroutine(die(1.15f,map,null,pos,false));
			break;
			//9 - Brick explode no sound
			case 9:
			map.SetTile(pos,null);
			
				render.sprite = null;
				GetComponent<ParticleSystem>().Play();

			StartCoroutine(die(1.15f,map,null,pos,false));
			break;
			//10 - Event block ++
			case 10:
			map.SetTile(pos,ghostBlock);
			render.sprite = blocksprites[1];
			aSource.PlayOneShot(data.sounds[72]);
			hStore.EventTriggered();
			StartCoroutine(die(0.3f,map,deadBlocks[1],pos,true));
			break;
			//11 - Slash block bounce (nothing)
			case 11:
			map.SetTile(pos,ghostBlock);
			render.sprite = blocksprites[2];
			StartCoroutine(die(0.3f,map,deadBlocks[0],pos,true));
			break;
			//12 - Powerup spawn (no block)
			case 12:
			map.SetTile(pos,null);
			render.sprite = blocksprites[1];
			if(map.GetTile(pos+(Vector3Int.up)*(invert?-1:1))==null)
			{
				power = Instantiate(powerup,new Vector3(transform.position.x,transform.position.y+offset,transform.position.z),Quaternion.identity);
				power.transform.eulerAngles = transform.eulerAngles;
				renderPower = power.transform.GetChild(0).GetComponent<SpriteRenderer>();
				renderPower.color = color;
				renderPower.sprite = storedItem.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
				objKill = power.transform.GetChild(0).GetComponent<killOnEnd>();
				objKill.nameToSet = "Box_item(Clone)";
				objKill.prefab = storedItem;
				objKill.color = color;
				objKill.zOffset = zOffset;
				power.SetActive(true);
				data.sfxSource.PlayOneShot(data.sounds[5]);
			}
			else data.sfxSource.PlayOneShot(data.sounds[118]);
			StartCoroutine(die(0.3f,map,null,pos,true));
			break;

		}
	}
	public void slashTurnOff()
	{
		switch(state)
		{
			default: break;
			case 2:
			case 3:
			case 4:
			render.sprite = blocksprites[2];
			break;
			case 6:
			render.sprite = blocksprites[3];
			break;
			case 10:
			render.sprite = blocksprites[5];
			break;
			case 12:
			render.sprite = null;
			break;
		}
	}
}
