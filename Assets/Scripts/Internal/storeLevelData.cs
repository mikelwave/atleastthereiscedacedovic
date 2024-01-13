using UnityEngine;

[ExecuteInEditMode]
public class storeLevelData : MonoBehaviour {
	[Header ("Level audio effects")]
	public AudioClip Intro;
	public AudioClip Music;
	public int BPM = 240;
	public bool muffleInMainArea = false;
	public bool echoInMainArea = false;
	[Space]
	public bool muffleInSubArea = false;
	public bool echoInSubArea = false;
	[Space]
	public bool startInSubArea = false;
	[Space]
	public bool saveMusicPoint = false;
	[Space]
	[Header ("Time")]
	public int time = 350;
	[Space]
	[Header ("Gravity")]
	public float gravityDivider = 1f;
	[Space]
	[Header ("Sky sprites")]
	public Sprite mainSkySprite;
	public Sprite subSkySprite;
	[Header ("Script only stuff")]
	Transform cam;
	SpriteRenderer mainSky;
	SpriteRenderer subSky;
	GameObject play;
	GameData data;

	#if UNITY_EDITOR
	// Use this for initialization
	void OnEnable ()
	{
	if(!Application.isPlaying)
		{
		cam = GameObject.Find("Main Camera").transform;
		mainSky = cam.GetChild(1).GetComponent<SpriteRenderer>();
		subSky = cam.GetChild(2).GetComponent<SpriteRenderer>();
		play = GameObject.Find("Player_main");
		data = GameObject.Find("_GM").GetComponent<GameData>();
		data.inHub = false;
		}
	}
	#endif
	void Awake()
	{
		cam = GameObject.Find("Main Camera").transform;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		data.areaSkies = new GameObject[2];
		data.areaSkies[0] = cam.GetChild(1).gameObject;
		data.areaSkies[1] = cam.GetChild(2).gameObject;
		play = GameObject.Find("Player_main");
		play.GetComponent<PlayerMusicBounce>().BPM = BPM;
		data.timer = time;
		data.MainLoop = Music;
		data.Intro = Intro;
		data.gravityDivider = gravityDivider;
		data.echoInMainArea = echoInMainArea;
		data.muffleInMainArea = muffleInMainArea;
		data.echoInSubArea = echoInSubArea;
		data.muffleInSubArea = muffleInSubArea;
		data.saveMusicSamples = saveMusicPoint;
		if(!startInSubArea)
		this.enabled = false;
	}
	void Start()
	{
		if(Application.isPlaying&&startInSubArea)
		{
			//print("StoreLevelData StartInSub True");
			data.startInSub = true;
			data.switchArea(true);
			this.enabled = false;
		}
	}
	void OnDisable()
	{
		if(!Application.isPlaying)
		{
			this.enabled = true;
		}
	}
	// Update is called once per frame
	#if UNITY_EDITOR
	void Update ()
	{
	if(!Application.isPlaying)
	{
		if(mainSky.sprite!=mainSkySprite)
		{
			mainSky.sprite = mainSkySprite;
			Vector2 size = mainSky.size;
			if(size.x < 1)
				size.x = 24;

			mainSky.size = size;
		}
		if(subSky.sprite!=subSkySprite)
		{
			Vector2 size = mainSky.size;
			subSky.sprite = subSkySprite;
			if(size.x < 1)
				size.x = 24;

			subSky.size = size;
		}

		play.GetComponent<PlayerMusicBounce>().BPM = BPM;
		data.inHub = false;
		data.timer = time;
		data.transform.GetChild(0).GetComponent<AudioSource>().clip = Music;
		data.echoInMainArea = echoInMainArea;
		data.muffleInMainArea = muffleInMainArea;
		data.echoInSubArea = echoInSubArea;
		data.muffleInSubArea = muffleInSubArea;
	}
	}
	#endif
}
