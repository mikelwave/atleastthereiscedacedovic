using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class hitBlockStore : MonoBehaviour {
	#region main
	public bool checkForEverythingOnLevelLoad = false;
	public int amount = 10;
	public float zItemOffset = 0;
	public GameObject block;
	public Color32 color = new Color32(255,255,255,255);
	Color32 colorNormalHarm = new Color32(229,229,229,255);
	public Color32 colorFlashHarm = new Color32(229,229,229,255);
	public List<GameObject> blocks;
	public Tilemap map,invmap,backgroundmap;
	public Sprite changeToSprites;
	public List <Vector3Int> editTilePositions,editInvTilePositions,redSwitchLines,redSwitchBlocks,redLocks,blueLocks,yellowLocks,bricks,bgBricks;
	int maxBrickXCheck = 0;
	public int maxBrickHorCheck = 0;
	GameObject deathZone;
	public bool drawDeathPit = false;
	public TileBase checkTile;
	public bool checkForTiles = false;
	public Sprite customBrick;
	public Sprite normalBrick;
	public TileBase[] customswitchBlocks;
	public Sprite[] customBrickParticles;
	GameData data;
	public bool checkOnStart = true,onlyBlocks = false;
	Tilemap harm;
	System.Collections.IEnumerator harmFlash()
	{
		bool flashing = false;
		int lastFrame = 0;
		PlayerMusicBounce beat = GameObject.Find("Player_main").GetComponent<PlayerMusicBounce>();
		while(true)
		{
			if(beat.frame!=lastFrame)
			{
				lastFrame = beat.frame;
				flashing = !flashing;
			    harm.color = flashing?colorNormalHarm:colorFlashHarm;
			}
			yield return 0;
		}
	}
	// Use this for initialization
	void Awake ()
	{
		if(Application.isPlaying)
		{
			GameObject obj = GameObject.Find("Harm");
			if(obj!=null)
			harm = obj.GetComponent<Tilemap>();
			//print(colorFlashHarm);
			if(harm!=null)
			{
				colorNormalHarm = harm.color;
				StartCoroutine(harmFlash());
			}
			if(checkOnStart)
			{
			data = GameObject.Find("_GM").GetComponent<GameData>();
			//see if burgers exist
			if(amount<=10&&(GameObject.Find("Burger")!=null||GameObject.Find("Burger_Large")!=null||GameObject.Find("Burger(Clone)")!=null||GameObject.Find("Burger_Large(Clone)")!=null))
			amount = 50;
			if(customswitchBlocks!=null&&customswitchBlocks.Length!=0)
			{
				data.switchTiles = customswitchBlocks;
				data.S_Switch(false,0,true);
			}
			if(backgroundmap==null)backgroundmap=GameObject.Find("BackgroundDark").GetComponent<Tilemap>();
			createBlocks();
			if(map == null)
			{
				map = transform.GetChild(0).GetComponent<Tilemap>();
			}
			findSkeletiles();
			findStaticBooms();
			}
			else if(onlyBlocks)
			{
				createBlocks();
			}
		}
	}
	public void createBlocks()
	{
		for(int i = 0; i < amount; i++)
			{
				GameObject obj = Instantiate(block,transform.position,Quaternion.identity);
				HitBlockScript hScript = obj.GetComponent<HitBlockScript>();
				hScript.hStore = this;
				hScript.zOffset = zItemOffset;
				SpriteRenderer r = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
				r.color = color;
				var pt = obj.GetComponent<ParticleSystem>().main;
				pt.startColor = r.color;
				obj.transform.parent = transform.parent;
				if(customBrick!=null)
				{
					hScript.blocksprites[0] = customBrick;
				}
				if(customBrick==null)customBrick = normalBrick;
				if(customBrickParticles!=null&&customBrickParticles.Length!=0)
				{
					ParticleSystem.TextureSheetAnimationModule p = obj.GetComponent<ParticleSystem>().textureSheetAnimation;
					//print(p.spriteCount);
					int e = p.spriteCount-1;
					//remove current brick sprites
					while(p.spriteCount>0)
					{
						p.RemoveSprite(0);
						e--;
					}
					//add replacements
					for(int k = 0; k<customBrickParticles.Length;k++)
					{
						p.AddSprite(customBrickParticles[k]);
					}
				}
				blocks.Add(obj);
			}
	}
	void Start()
	{
		if(!Application.isPlaying)
		{
			UpdateEditTiles();
		}
		if(Application.isPlaying)
		{
			if(checkForEverythingOnLevelLoad)
			{
				UpdateGameTiles();
			}
			refreshEditTiles();
		}
	}
	public void UpdateGameTiles()
	{
		//print("Updating tiles...");
		UpdateEditTiles();
		findRedSwitchBlocks();
		findRedLocks();
		findBlueLocks();
		findYellowLocks();
		findBricks();
		if(checkForTiles)
		{
			checkForTiles = false;

			BoundsInt bounds = map.cellBounds;
			int Zpos = bounds.z;
			//Debug.Log(map.cellBounds);
			for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
			{
				for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
				{
					if(map.GetTile(new Vector3Int(x,y,Zpos))!=null&&map.GetTile(new Vector3Int(x,y,Zpos))==checkTile)
					{
						Debug.Log(new Vector3Int(x,y,Zpos)+", "+checkTile.name);
					}
				}
			}
		}
	}
	#if UNITY_EDITOR
	//if(EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isPlaying)
	void Update()
	{
		//if(!Application.isPlaying)
		//{
		//	UpdateGameTiles();
		//}
		if(!EditorApplication.isPlaying)
		if(levelInfoUpdate.GetHitBlock()==null||levelInfoUpdate.GetHitBlock()!=this)
		{
			levelInfoUpdate.assignHitBlock(this);
		}
		if(drawDeathPit)
		{
			drawDeathPit = false;
			map.CompressBounds();
			if(deathZone!=null)
				DestroyImmediate(deathZone);
			deathZone = new GameObject("deathZone");
			deathZone.transform.parent = transform.GetChild(0);
			BoxCollider2D bCol = deathZone.AddComponent<BoxCollider2D>();
			bCol.isTrigger = true;
			bCol.size = new Vector2(map.cellBounds.size.x,1);
			bCol.offset = new Vector2(map.cellBounds.size.x/2f+31f,0.5f);
		}
	}
	#endif
	void findRedSwitchBlocks()
	{
		redSwitchLines = new List<Vector3Int>();
		redSwitchBlocks = new List<Vector3Int>();
		BoundsInt bounds = map.cellBounds;
		int Zpos = bounds.z;
		//Debug.Log(map.cellBounds);
		for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
		{
			for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
			{

				if(map.GetTile(new Vector3Int(x,y,Zpos))!=null)
				{
					if(map.GetTile(new Vector3Int(x,y,Zpos)).name=="redSLine")
					{
						redSwitchLines.Add(new Vector3Int(x,y,Zpos));
					}
					else if(map.GetTile(new Vector3Int(x,y,Zpos)).name=="redSBlock")
					{
						redSwitchBlocks.Add(new Vector3Int(x,y,Zpos));
					}

				}
			}
		}
	}
	void findBricks()
	{
		if(backgroundmap==null)backgroundmap=GameObject.Find("BackgroundDark").GetComponent<Tilemap>();
		bricks = new List<Vector3Int>();
		bgBricks = new List<Vector3Int>();
		BoundsInt bounds = map.cellBounds,bgbounds = backgroundmap.cellBounds;
		if(maxBrickHorCheck!=0) maxBrickXCheck = maxBrickHorCheck;
		else maxBrickXCheck = bounds.size.x+map.cellBounds.position.x;
		int Zpos = bounds.z,ZposBG = bgbounds.z;
		//Debug.Log(map.cellBounds);
		for(int x = map.cellBounds.position.x; x<maxBrickXCheck;x++)
		{
			for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
			{

				if(map.GetTile(new Vector3Int(x,y,Zpos))!=null)
				{
					if(map.GetTile(new Vector3Int(x,y,Zpos)).name=="Brick")
					{
						//Debug.Log(new Vector3Int(x,y,Zpos)+", "+map.GetTile(new Vector3Int(x,y,Zpos)));
						bricks.Add(new Vector3Int(x,y,Zpos));
					}
				}
			}
		}
		if(backgroundmap!=null)
		{
			for(int x = backgroundmap.cellBounds.position.x; x<bgbounds.size.x+backgroundmap.cellBounds.position.x;x++)
			{
				for(int y = backgroundmap.cellBounds.position.y; y<bgbounds.size.y+backgroundmap.cellBounds.position.y;y++)
				{

					if(backgroundmap.GetTile(new Vector3Int(x,y,Zpos))!=null)
					{
						if(backgroundmap.GetTile(new Vector3Int(x,y,Zpos)).name=="Brick")
						{
							//Debug.Log(new Vector3Int(x,y,Zpos)+", "+map.GetTile(new Vector3Int(x,y,Zpos)));
							bgBricks.Add(new Vector3Int(x,y,Zpos));
						}
					}
				}
			}
		}
	}
	void findStaticBooms()
	{
		BoundsInt bounds = map.cellBounds;
		int Zpos = bounds.z;
		int boomStaticAmount=0;
		for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
		{
			for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
			{
				TileBase t = map.GetTile(new Vector3Int(x,y,Zpos));
				if(t!=null&&t.name=="BoomStatic")
				{
					boomStaticAmount++;
					if(boomStaticAmount>=20)break;
				}
			}
		}
		if(boomStaticAmount>0)
		{
			data.boomStaticObjects = new List<boomStaticScript>();
			if(amount>20) amount = 20;

			for(int i = 0; i<amount;i++)
			{
				GameObject obj = Instantiate(data.boomStaticObject,transform.position,Quaternion.identity);
				obj.transform.name = "Q_BlockParent";
				boomStaticScript o = obj.GetComponent<boomStaticScript>();
				o.assign(data);
				data.boomStaticObjects.Add(o);
			}
		}
	}
	void findSkeletiles()
	{
		BoundsInt bounds = new BoundsInt();
		Tilemap ss = new Tilemap();
		bool found = false;
		int amount = 0;
		bool lipFound = false;
		for(int i = 0; i<transform.childCount;i++)
		{
			if(transform.GetChild(i).name=="SemiSolidMap")
			{
				bounds = transform.GetChild(i).GetComponent<Tilemap>().cellBounds;
				ss = transform.GetChild(i).GetComponent<Tilemap>();
				found = true;
				break;
			}
		}
		int Zpos = 0;
		if(found)
		{
			Zpos = bounds.z;
			for(int y = ss.cellBounds.position.y; y<bounds.size.y+ss.cellBounds.position.y;y++)
			{
				for(int x = ss.cellBounds.position.x; x<bounds.size.x+ss.cellBounds.position.x;x++)
				{
					TileBase t = ss.GetTile(new Vector3Int(x,y,Zpos));
					if(t!=null)
					{
						//print(t.name);
						if(t.name=="blocks 1_9")
						{
							amount++;
						}
					}
				}
			}
			bounds = map.cellBounds;
			for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
			{
				if(lipFound)break;
				for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
				{
					TileBase t2 = map.GetTile(new Vector3Int(x,y,Zpos));
					//if(t2!=null)
					//print(map.name+" "+new Vector3Int(x,y,Zpos)+" "+t2.name);
					if(!lipFound&&t2!=null&&t2.name=="test blocks_19")
					{
						lipFound = true;
						break;
					}
				}
			}
			if(amount>0)
			{
				//print("Generating skeleton tiles...");

				data.skeletileObjects = new List<skeletileScript>();
				if(amount>40) amount = 40;

				for(int i = 0; i<amount;i++)
				{
					GameObject obj = Instantiate(data.skeletileObject,transform.position,Quaternion.identity);
					data.skeletileObjects.Add(obj.GetComponent<skeletileScript>());
				}
			}
			if(lipFound)
			{
				//print("Generating lip particles...");
				data.lipParticles = new List<GameObject>();
				for(int i = 0; i<10;i++)
				{
					GameObject obj = Instantiate(data.lipParticle,transform.position,Quaternion.identity);
					data.lipParticles.Add(obj);
				}
			}
		}
	}
	void findRedLocks()
	{
		redLocks = new List<Vector3Int>();
		BoundsInt bounds = map.cellBounds;
		int Zpos = bounds.z;
		//Debug.Log(map.cellBounds);
		for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
		{
			for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
			{

				if(map.GetTile(new Vector3Int(x,y,Zpos))!=null)
				{
					if(map.GetTile(new Vector3Int(x,y,Zpos)).name=="lock_red")
					{
						//Debug.Log(new Vector3Int(x,y,Zpos)+", "+map.GetTile(new Vector3Int(x,y,Zpos)));
						redLocks.Add(new Vector3Int(x,y,Zpos));
					}
				}
			}
		}
	}
	void findBlueLocks()
	{
		blueLocks = new List<Vector3Int>();
		BoundsInt bounds = map.cellBounds;
		int Zpos = bounds.z;
		//Debug.Log(map.cellBounds);
		for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
		{
			for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
			{

				if(map.GetTile(new Vector3Int(x,y,Zpos))!=null)
				{
					if(map.GetTile(new Vector3Int(x,y,Zpos)).name=="lock_blue")
					{
						//Debug.Log(new Vector3Int(x,y,Zpos)+", "+map.GetTile(new Vector3Int(x,y,Zpos)));
						blueLocks.Add(new Vector3Int(x,y,Zpos));
					}
				}
			}
		}
	}
	void findYellowLocks()
	{
		yellowLocks = new List<Vector3Int>();
		BoundsInt bounds = map.cellBounds;
		int Zpos = bounds.z;
		//Debug.Log(map.cellBounds);
		for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
		{
			for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
			{

				if(map.GetTile(new Vector3Int(x,y,Zpos))!=null)
				{
					if(map.GetTile(new Vector3Int(x,y,Zpos)).name=="lock_yellow")
					{
						//Debug.Log(new Vector3Int(x,y,Zpos)+", "+map.GetTile(new Vector3Int(x,y,Zpos)));
						yellowLocks.Add(new Vector3Int(x,y,Zpos));
					}
				}
			}
		}
	}
	void UpdateEditTiles()
	{
			editTilePositions = new List<Vector3Int>();
			editInvTilePositions = new List<Vector3Int>();

			BoundsInt bounds = map.cellBounds;
			int Zpos = bounds.z;
			//Debug.Log(map.cellBounds);
			for(int y = map.cellBounds.position.y; y<bounds.size.y+map.cellBounds.position.y;y++)
			{
				for(int x = map.cellBounds.position.x; x<bounds.size.x+map.cellBounds.position.x;x++)
				{
					if(map.GetTile(new Vector3Int(x,y,Zpos))!=null
					&&map.GetTile(new Vector3Int(x,y,Zpos)).GetType().Name=="EditorTile")
					{
						//Debug.Log(new Vector3Int(x,y,Zpos)+", "+map.GetTile(new Vector3Int(x,y,Zpos)));
						editTilePositions.Add(new Vector3Int(x,y,Zpos));
					}
				}
			}
			Zpos = Mathf.RoundToInt(invmap.transform.position.z);
			bounds = invmap.cellBounds;
			for(int y = bounds.position.y; y<bounds.size.y+bounds.position.y;y++)
			{
				for(int x = bounds.position.x; x<bounds.size.x+bounds.position.x;x++)
				{
					if(invmap.GetTile(new Vector3Int(x,y,Zpos))!=null&&invmap.GetTile(new Vector3Int(x,y,Zpos)).GetType().Name=="EditorTile")
					{
						editInvTilePositions.Add(new Vector3Int(x,y,Zpos));
					}
				}
			}
			
	}
	void refreshEditTiles()
	{
		//print("refreshing tiles");
		for(int i = 0; i<editTilePositions.Count;i++)
		{
			TileBase t = map.GetTile(editTilePositions[i]);
			if((customBrick==null || customBrick == normalBrick)||t!=null && !t.name.ToLower().Contains("brick"))
			map.RefreshTile(editTilePositions[i]);
			else
			{
				EditorTile e = map.GetTile(editTilePositions[i]) as EditorTile;
				e.m_SpriteCustomGame = customBrick;
				e.sprite = customBrick;
				map.RefreshTile(editTilePositions[i]);
			}
		}
		for(int i = 0; i<editInvTilePositions.Count;i++)
		{
			invmap.RefreshTile(editInvTilePositions[i]);
		}
		for(int i = 0; i<bricks.Count;i++)
		{
			Tile t = map.GetTile(bricks[i]) as Tile;
			if(customBrick!=null)
			t.sprite = customBrick;
			else t.sprite = normalBrick;

			map.RefreshTile(bricks[i]);
		}
		if(backgroundmap!=null)
		{
			for(int i = 0; i<bgBricks.Count;i++)
			{
				Tile t = backgroundmap.GetTile(bgBricks[i]) as Tile;
				if(customBrick!=null)
				t.sprite = customBrick;
				else t.sprite = normalBrick;

				backgroundmap.RefreshTile(bgBricks[i]);
			}
		}
	}
	#endregion
    [Serializable]
    public class Event : UnityEvent { }
	[SerializeField]
    private Event eventBlockEvent = new Event();

	public Event onEvent { get { return eventBlockEvent; } set { eventBlockEvent = value; } }
	public void EventTriggered()
     {
         eventBlockEvent.Invoke();
     }
}
