using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

//[ExecuteInEditMode]
public class cutsceneEvent : MonoBehaviour {
	[Header ("Debug")]
	public bool printSubtitles = false;
	[Space]
	public VideoClip alternateCutscene;
	public TextAsset altSubtitles;
	VideoPlayer vidplayer;
	bool playing = false;
	GameData data;
	MeshRenderer render;
	dataShare dataS;
	public TextAsset subtitlesFile;
	public List<long> subtitleFrames;
	public List<long> altSubtitleFrames;
	//int presses = 0;
	int currentLine = 0;
	List<string> textAsLines;
	public bool playMusicOnEnd = true,forceAlt = false,ignoreEmpty = false;
	TextMeshPro text;
	Coroutine textWait;
	IEnumerator textStop()
	{
		int frames = 600;
		while(frames>0)
		{
			frames--;
			yield return 0;
		}
		textWait = null;
		text.text = "";
	}
	public void Start()
	{
		//if(Application.isPlaying)
		if(vidplayer==null)
		{
			vidplayer = GetComponent<VideoPlayer>();
			GameObject dataObj = GameObject.Find("_GM");
			if(dataObj!=null)
			data = dataObj.GetComponent<GameData>();
			render = GetComponent<MeshRenderer>();
			dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
			if(!ignoreEmpty&&(forceAlt||dataS.mode==1))
			{
				if(alternateCutscene==null)
				{
					dataS.playCutscene = false;
				}
				vidplayer.clip = alternateCutscene;
				subtitlesFile = altSubtitles;
				subtitleFrames = altSubtitleFrames;
			}
			reloadSubtitles();
			if(transform.childCount>=2)
			{
				text = transform.GetChild(1).GetComponent<TextMeshPro>();
			}
			if(text==null)
			{
				text = Instantiate(dataS.subtitlePrefab,transform.position,Quaternion.identity).GetComponent<TextMeshPro>();
				Transform tr = text.transform;
				tr.SetParent(transform);
				tr.localPosition = new Vector3(0,-0.44f,-1);
				tr.localScale = new Vector3(0.01302188f,0.02315f,0);
				tr.name = "subtitleText";
			}
			text.text = "";
		}
	}
	public void reloadSubtitles()
	{
		if(subtitleFrames==null)
			subtitleFrames = new List<long>();
		if(subtitlesFile!=null)
		{
			string textAsString = subtitlesFile.text;
			textAsLines = new List<string>();
			textAsLines.AddRange(textAsString.Split("\n"[0]));
		}
		currentLine = 0;
	}
	// Use this for initialization
	void OnEnable()
	{
		if(vidplayer == null)
			Start();
		if(render == null)
			render = GetComponent<MeshRenderer>();
		//Debug.Log("Play cutscene: "+dataS.playCutscene);
		if(dataS.playCutscene)
		{
			dataS.playCutscene=false;
			if(vidplayer.clip!=null)
			{
				if(data!=null)
				{
					data.suspendAudioEffects();
					if(playMusicOnEnd)
						data.stopMusic(true,false);
				}
				render.enabled = false;
				Time.timeScale = 0;
				vidplayer.Play();
				playing = true;
				//Debug.Log("!!! REMOVE THIS: Cutscene started. Press Right shift to check current frame");
				vidplayer.loopPointReached += CheckIfVideoEnded;
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
		else
		{
			gameObject.SetActive(false);
		}
	}
	void Update()
	{
		/*if(printSubtitles)
		{
			printSubtitles = false;
			string s = GetComponent<VideoPlayer>().clip.name+'\n';
			for(int i = 0;i<subtitleFrames.Count;i++)
			{
				s+=subtitleFrames[i];
				s+="\n";
			}
			print(s);
		}*/
		if(vidplayer.frame>0 && !render.enabled)
			render.enabled = true;
		if(Time.timeScale!=0&&playing)
		{
			//Debug.Log("Forced time to stop");
			Time.timeScale = 0;
		}
		if(subtitlesFile!=null&&subtitleFrames.Count!=0)
		{
			if(currentLine<textAsLines.Count&&currentLine<subtitleFrames.Count
			&&vidplayer.frame>=subtitleFrames[currentLine])
			{
				if(printSubtitles)
				Debug.Log("Frame "+vidplayer.frame+": "+textAsLines[currentLine]);
				if(textWait!=null)StopCoroutine(textWait);
				textWait = StartCoroutine(textStop());
				text.text = textAsLines[currentLine];
				currentLine++;
				//presses=currentLine;
			}
		}
		//Debug feature for subtitles.
		/*if(Input.GetKeyDown(KeyCode.RightShift))
		{
			Debug.Log(vidplayer.frame);
			presses++;
			if(subtitleFrames.Count>presses)
			{
				subtitleFrames[presses] = vidplayer.frame;
			}
			else subtitleFrames.Add(vidplayer.frame);
		}*/
		if(SuperInput.GetKeyDown("Jump"))
		{
			vidplayer.Stop();
		//print ("Skipped video");
		Time.timeScale = 1;
		text.text = "";
		playing = false;
		if(data!=null)
		data.restoreAudioEffects();
		if(playMusicOnEnd&&data!=null)
		data.stopMusic(false,true);
		gameObject.SetActive(false);
		}
	}
	void CheckIfVideoEnded(UnityEngine.Video.VideoPlayer vp)
	{
     	//print  ("Video Is Over");
		Time.timeScale = 1;
		text.text = "";
		playing = false;
		if(data!=null)
		data.restoreAudioEffects();
		if(playMusicOnEnd&&data!=null)
		data.stopMusic(false,true);
		gameObject.SetActive(false);
	}
}
