using UnityEngine;

public class LaserScript : MonoBehaviour
{
    [Header ("Editor values")]
    [Space]
    public bool shoot = false;
    public bool offbeat = false, beatBased = true;
    [Space]
	public LayerMask whatIsHittable;
    PlayerScript player;
	public Vector3 offset = new Vector3(0,0.1f,0);
	public bool debug = false;
    bool active = false;
    PlayerMusicBounce beat;
    public AudioClip[] clips = new AudioClip[2];
    AudioSource aSource;
    bool firing = false;
    LineRenderer render;
    public float sinMultiplier = 10;
    public Texture2D[] laserSprites;
    public Gradient[] flashColors;
    public bool colorFlashing = false;
    int curSprite = 0, waitframes = 4, curWaitFrame = 0,curFlashFrames = 0,curColor = 0;
    public int flashFrames = 4;
    public float sinMin = 0.2f,sinWidth = 0.32f;
    float startRotation;
    public ParticleSystem particles;
    public Vector3 particleImpactOffset = new Vector3(0,0,0);
    bool impact = false;

    // Start is called before the first frame update
    void Start()
    {
		player = GameObject.Find("Player_main").GetComponent<PlayerScript>();
        beat = player.GetComponent<PlayerMusicBounce>();
        aSource = GetComponent<AudioSource>();
        render = GetComponent<LineRenderer>();
        render.useWorldSpace = true;
        curWaitFrame = waitframes;
        curFlashFrames = flashFrames;
        startRotation = transform.eulerAngles.z;
        if(GetComponent<Collider2D>()==null)
        {
            active = true; toggleVisible(active);
        }
	}
    void Update()
    {
        if(active)
        {
            if(Time.timeScale!=0&&firing)
            {
                if(laserSprites.Length!=0)
                {
                   curWaitFrame--;
                   if(curWaitFrame==0)
                    {
                        curSprite++; if(curSprite>=laserSprites.Length)curSprite = 0;
                        render.material.mainTexture = laserSprites[curSprite];
                        curWaitFrame = waitframes;
                    }
                }
                if(flashColors.Length!=0&&colorFlashing)
                {
                    curFlashFrames--;
                    if(curFlashFrames==0)
                    {
                        curColor++; if(curColor>=flashColors.Length)curColor = 0;
                        render.colorGradient = flashColors[curColor];
                        curFlashFrames = flashFrames;
                    }
                }
                if(particles!=null&&impact)
                {
                    particles.transform.position = render.GetPosition(1)+particleImpactOffset;
                    //print(particles.transform.position+" shoot: "+shoot+" impact: "+impact);
                }
            }
            if(beatBased)
            {
                if(!offbeat)
                {
                    if(beat.frame==0)
                    {
                       fireRay();
                       if(!firing)
                       {
                          firing = true;
                          if(clips.Length>0)
                          aSource.PlayOneShot(clips[0]);
                      }
                   }
                   else
                   {
                       if(firing)
                      {
                        firing = false;
                        render.enabled = firing;
                        disableParticles();
                      }
                    }
                }
                else if(offbeat)
                {
                   if(beat.frame==1)
                   {
                       fireRay();
                      if(!firing)
                       {
                           firing = true;
                            if(clips.Length>1)
                          aSource.PlayOneShot(clips[1]);
                        }
                    }
                    else
                    {
                        if(firing)
                        {
                            firing = false;
                            render.enabled = firing;
                            disableParticles();
                        }
                    }
                }
            }
            else
            {
                if(shoot)
                {
                    fireRay();
                    if(!firing)
                    {
                        firing = true;
                        if(clips.Length>1)
                        aSource.PlayOneShot(clips[1]);

                    }
                }
                else
                {
                    if(firing)
                    {
                        firing = false;
                        impact = false;
                        render.enabled = firing;
                        if(flashColors.Length!=0)
                        {
                            render.colorGradient = flashColors[0];
                        }
                        disableParticles();
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,startRotation);

                    }
                }
            }
            if(render.enabled)
            {
                float lineWidth = Mathf.Clamp(sinWidth*Mathf.Abs(Mathf.Sin(Time.timeSinceLevelLoad*sinMultiplier)),sinMin,sinWidth);
                render.startWidth = lineWidth;
            }
        }
    }
    void toggleVisible(bool visible)
    {
        render.enabled = visible;
    }
    void disableParticles()
    {
        if(particles!=null&&particles.isPlaying)
            particles.Stop(false,ParticleSystemStopBehavior.StopEmitting);
    }
    void fireRay()
	{
		RaycastHit2D ray = Physics2D.Raycast(transform.position,-transform.up,50f,whatIsHittable);
        //draw line
        if(ray.collider!=null)
        {
            if(!render.enabled)
            {
                render.enabled = true;
            }
            Vector3[] linePositions = new Vector3[render.positionCount];
            linePositions[0] = transform.position;
            linePositions[1] = new Vector3(ray.point.x,ray.point.y,transform.position.z); 
            render.SetPositions(linePositions);
            impact = true;
        }
        else
        {
            if(!render.enabled)
            {
                render.enabled = true;
            }
            Vector3[] linePositions = new Vector3[render.positionCount];
            linePositions[0] = transform.position;
            linePositions[1] = transform.position-(transform.up*50);
            render.SetPositions(linePositions);
            impact = false;
        }
		if(ray.collider!=null&&ray.collider.tag=="Player")
		{
			if(player.invFrames==0)
			player.Damage(true,false);
		}
        if(particles!=null&&!particles.isPlaying)
        {
            particles.Play();
        }

		if(debug)
		{
		if(ray.collider!=null)
		Debug.DrawLine(transform.position,ray.point,Color.red);
		else
		Debug.DrawLine(transform.position,transform.position-(transform.up*20),Color.red);
		}
	}
    void OnDisable()
    {
        if(flashColors.Length!=0)
        {
            render.colorGradient = flashColors[0];
        }
        if(particles!=null)
        {
            particles.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            particles.transform.localPosition = new Vector3(0,-6,0);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name=="ObjectActivator"&&!active)
        {
            active = true;
            toggleVisible(active);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.name=="ObjectActivator"&&active)
        {
            active = false;
            toggleVisible(active);
        }
    }
}
