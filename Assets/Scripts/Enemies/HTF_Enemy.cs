using System.Collections;
using UnityEngine;

public class HTF_Enemy : MonoBehaviour {

	public Sprite[] sprites = new Sprite[6];
	public AudioClip[] sounds;
	int type = 0;
	Rigidbody2D rb;
	AudioSource asc;
	bool landed = false;
	public ParticleSystem sys;
	bool visible = false;
	bool soundPaused = false;
	Coroutine cor;
	// Use this for initialization
	void OnEnable () 
	{
		transform.parent = null;
		type = Random.Range(0,3);
		if(type!=0)
		GetComponent<SpriteRenderer>().sprite = sprites[0+(type*2)];
		else GetComponent<SpriteRenderer>().sprite = sprites[0];
		rb = GetComponent<Rigidbody2D>();
		asc = GetComponent<AudioSource>();
		asc.clip = sounds[0];
		asc.Play();
		rb.angularVelocity = 0;
		rb.velocity = new Vector2(0,-GetComponent<Gravity>().maxVelocities.y);
		if(cor!=null)StopCoroutine(cor);
		cor = StartCoroutine(disableTimer());
	}
	void OnDisable()
	{
		if(cor!=null)StopCoroutine(cor);
		transform.GetChild(0).GetComponent<Collider2D>().enabled = true;
		GetComponent<Collider2D>().isTrigger = false;
		GetComponent<SpriteRenderer>().sortingOrder = 0;
		transform.localEulerAngles = Vector3.zero;
		rb.velocity = Vector2.zero;
		landed = false;
	}
	void Update()
	{
		if(Time.timeScale==0&&asc.isPlaying)
		{
			asc.Pause();
			soundPaused = true;
		}
		else if(Time.timeScale!=0&&soundPaused)
		{
			asc.UnPause();
			soundPaused = false;
		}
	}
	IEnumerator disableTimer()
	{
		yield return new WaitForSeconds(2f);
		if(!landed)
		{
			visible = false;
			gameObject.SetActive(false);
		}
		cor = null;
	}
	void OnCollisionEnter2D(Collision2D other)
	{
		if(visible)
		{
		asc.Stop();
		if(sys!=null)
		{
			sys.transform.position = transform.position;
			if(sys.isPlaying)
				sys.Stop(false,ParticleSystemStopBehavior.StopEmitting);
			sys.Play();
		}
		int soundInt = Random.Range(0,4);
		asc.clip = sounds[1+soundInt];
		asc.Play();
		GetComponent<Collider2D>().isTrigger = true;
		transform.GetChild(0).GetComponent<Collider2D>().enabled = false;
		landed = true;
		if(type!=0)
			GetComponent<SpriteRenderer>().sprite = sprites[1+(type*2)];
		else GetComponent<SpriteRenderer>().sprite = sprites[1];
		GetComponent<SpriteRenderer>().sortingOrder = 10;
		rb.velocity = Vector2.zero;
		float sideVelo = 5f;
		float angVelo = -1000f;
		int rando = Random.Range(0,2);
		if(rando==0)
		{
			sideVelo = -sideVelo;
			angVelo = -angVelo;
		}
		rb.velocity = new Vector2(sideVelo,15f);
		rb.angularVelocity = angVelo;
		}
		else
		{
			rb.velocity = Vector2.zero;
			gameObject.SetActive(false);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&landed)
		{
			visible = false;
			gameObject.SetActive(false);
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
			visible = true;
	}
}
