using UnityEngine;
[RequireComponent (typeof (AudioSource))]
public class AnimatorSoundPlayer : MonoBehaviour {
	public AudioClip[] clips;
	AudioSource asc;
	public bool canPlay = true;
	public Vector2 randomPitchRange = new Vector2(0.8f,1f);
	public Vector2Int randomRange;
	void Start()
	{
		if(asc==null)
		{
			asc = GetComponent<AudioSource>();
			if(randomRange.y==0)
			randomRange = new Vector2Int(randomRange.x,clips.Length);
		}
	}
	public void playSound(int ID)
	{
		if(asc==null)Start();
		if(canPlay)
		asc.PlayOneShot(clips[Mathf.Clamp(ID,0,clips.Length-1)]);
	}
	public void playSoundVol(int ID,float Vol)
	{
		if(canPlay)
		asc.PlayOneShot(clips[Mathf.Clamp(ID,0,clips.Length-1)],Vol);
	}
	public void playSoundRandomPitch(int ID)
	{
		if(canPlay)
		{
			if(asc.isPlaying)asc.Stop();
			asc.clip = clips[0];
			asc.pitch = Random.Range(randomPitchRange.x,randomPitchRange.y);
			asc.Play();
		}
	}
	public void playSoundRandom()
	{
		int rand = Random.Range(0+randomRange.x,randomRange.y);
		//print(rand);
		if(canPlay)
		asc.PlayOneShot(clips[rand]);
	}
	public void setRangeX(int newX)
	{
		randomRange = new Vector2Int(newX,randomRange.y);
	}
	public void setRangeY(int newY)
	{
		randomRange = new Vector2Int(randomRange.x,newY);
	}
}
