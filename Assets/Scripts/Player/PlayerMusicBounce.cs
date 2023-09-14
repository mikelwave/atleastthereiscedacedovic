using System.Collections;
using UnityEngine;

public class PlayerMusicBounce : MonoBehaviour {

	public float BPM = 120;
	public int frame = 0;
	public int startFrame = 0;
	public bool stop = false;
	public GameData data;
	public Coroutine bouncing;
	public AudioSource music;
	public colorFade colorPulse;
	playerMusicBounceTimer timer;
	float startDelay = 0;
	public void setStartDelay(float v)
	{
		startDelay = v;
	}
	public IEnumerator bounce()
	{
		while(!stop||this.enabled)
		{
			if(BPM!=0)
			{
				float wait = (120/(BPM*music.pitch));
				yield return new WaitForSeconds(wait);
				frame++;
				if(frame == 2) frame = 0;
				if(colorPulse!=null)colorPulse.triggerFade(0);
			}
			else
			{ 
				stop = true;
				frame = 0;
				yield return 0;
			}
		}
		stop = false;

	}
	public IEnumerator bounce2()
	{
		yield return new WaitForSeconds(startDelay);
		while(!stop||this.enabled)
		{
			if(BPM!=0)
			{
				float wait = (120/(BPM*music.pitch));
				yield return new WaitForSeconds(wait);
				timer.reset();
				frame++;
				if(frame == 2) frame = 0;
				if(colorPulse!=null)colorPulse.triggerFade(0);
			}
			else
			{ 
				stop = true;
				frame = 0;
				yield return 0;
			}
		}
		stop = false;

	}
	public float getDelay()
	{
		float t = timer.reset();
		//print("Waited for: "+t);
		return t;
	}
	// Use this for initialization
	void Start () {
		if(music==null)
		{
			data = GameObject.Find("_GM").GetComponent<GameData>();
			music = data.sources[0];
			timer = GetComponent<playerMusicBounceTimer>();
			if(timer==null)
			bouncing = StartCoroutine(bounce());
			else 
			bouncing = StartCoroutine(bounce2());
		}
	}
	void OnEnable()
	{
		if(music!=null)
		{
			stop=false;
			frame = startFrame;
			if(bouncing!=null)
			StopCoroutine(bouncing);

			if(timer==null)
			bouncing = StartCoroutine(bounce());
			else 
			bouncing = StartCoroutine(bounce2());
		}
	}
	void OnDisable()
	{
		if(bouncing!=null)
			StopCoroutine(bouncing);
	}
}
