using UnityEngine;

//Simple spinning of an object by a set amount with audio support
public class Spinner : MonoBehaviour {
	public float speed = 1f;
	AudioSource asc;
	bool soundPaused = false;
	public bool playSoundAtRotatePoint = false;
	public int soundRotation = 0;
	int rot = 0;
	public bool pingpong = false;
	float curRotation = 0;
	public float sinMult = 1;
	public float toSub = 0;
	public float startRotation = 0;
	void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nSpeed: "+speed);
	}
	void Start()
	{
		//print("Start "+transform.eulerAngles.z);
		if(dataShare.debug)
		printer();
		asc = GetComponent<AudioSource>();
		startRotation = transform.eulerAngles.z;
	}
	// Update is called once per frame
	void Update ()
	{
		Vector3 toAdd = Vector3.forward*speed*-1*Time.timeScale;
		if(!pingpong)
		transform.Rotate(toAdd);
		else
		{
			curRotation = curRotation+(toAdd.z/2);
			Vector3 t = transform.eulerAngles;
			transform.eulerAngles = new Vector3(t.x,t.y,Mathf.Sin(curRotation)*sinMult);
		}
		if(Time.timeScale!=0&&toSub!=0&&speed!=0)
		{
			if(speed>0)
			speed = Mathf.Clamp(speed-=toSub,0,speed+1);
			else speed = Mathf.Clamp(speed-=toSub,speed-1,0);
		}
		if(asc!=null)
		{
			if(Time.timeScale==0&&!soundPaused)
			{
				soundPaused = true;
				asc.Pause();
			}
			else if(Time.timeScale!=0)
			{
				if(soundPaused)
				{
					soundPaused = false;
					asc.UnPause();
				}
				rot += Mathf.FloorToInt(toAdd.z);
				if(!soundPaused&&playSoundAtRotatePoint)
				if(rot>=soundRotation)
				{
					//soundRotation=(int)Mathf.Repeat(soundRotation+180,360);
					rot -= soundRotation*2;
					if(asc.isPlaying)asc.Stop();
					asc.Play();
				}
			}
		}
	}
}
