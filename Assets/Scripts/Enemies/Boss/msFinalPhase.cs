using System.Collections;
using UnityEngine;

public class msFinalPhase : MonoBehaviour
{
    public float stompPointOffset;
    public GameObject[] projectiles;
    public ParticleSystem[] particles;
    public AudioClip[] sounds;
    public FireBarScript[] barScripts;
    public GameObject AutoScroll;
    Transform faceTr;
    Animator anim;
    PlayerScript pScript;
    MGCameraController cam;
    GameData data;
    Coroutine cor;
    AudioSource aSource;
    bool exploding = false;
    bool reset = false;
    bool dead = false;
    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>(); 
        faceTr = transform.GetChild(0).GetChild(0);  
        aSource = GetComponent<AudioSource>();
        pScript = GameObject.Find("Player_main").GetComponent<PlayerScript>();
        data = GameObject.Find("_GM").GetComponent<GameData>();
        cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        cor = StartCoroutine(attackLoop());
    }
    IEnumerator attackLoop()
    {
        int atk = 1;
        yield return new WaitForSeconds(3f);
        bool inPhase = true;
        while(inPhase)
        {
            reset = false;
            atk++; if(atk>1)atk = 0;
            anim.SetInteger("AttackInt",atk);
            anim.SetTrigger("AttackTrigger");
            yield return new WaitForSeconds(2.5f);
        }
    }
    IEnumerator playerCor()
    {
        pScript.goToCutsceneMode(true);
        pScript.knockedBack = true;
        pScript.rb.velocity = new Vector2(-8,0.1f);
        yield return new WaitUntil(()=>pScript.grounded);
        pScript.knockedBack = false;
    }
    IEnumerator explodeSoundLoop()
	{
        exploding = true;
		float delay = 1/particles[3].emission.rateOverTime.constant;
		particles[3].Play();
		while(exploding)
		{
            cam.shakeCamera(0.2f,delay);
            aSource.PlayOneShot(sounds[2+Random.Range(0,3)]);
            smallBoneBit();
			yield return new WaitForSeconds(delay);
		}
        particles[3].Stop(false,ParticleSystemStopBehavior.StopEmitting);
	}
    void smallBoneBit()
    {
        ParticleSystem usedPart = null;
        if(!particles[1].isPlaying)usedPart = particles[1];
        else if(!particles[2].isPlaying)usedPart = particles[2];

        if(usedPart!=null)
        {
            Transform main = transform.GetChild(0);
            usedPart.transform.position = main.position+new Vector3(Random.Range(-2,2.1f),Random.Range(-2.5f,2.6f),0);
            data.playUnlistedSoundPoint(sounds[7+Random.Range(0,2)],usedPart.transform.position);
            usedPart.Play();
        }
    }
    public void spawnProjectile(int index)
    {
        Vector3 add = new Vector3(0.1f,-0.2f,0);
        if(index==0)
        {
            add = new Vector3(-0.3f,-0.3f,0);
            aSource.PlayOneShot(sounds[5]);
            aSource.PlayOneShot(sounds[10]);
            //shoot non tracking trashbag
            if(!reset) reset = true;
            else index = 2;
        }
        Instantiate(projectiles[index],faceTr.position+add,Quaternion.identity);
    }
    public void exitEvent()
    {
        exploding = false;
        cam.shakeCamera(0,0);
        cam.shakeCamera(0.5f,5);
        aSource.PlayOneShot(sounds[6]);
        aSource.PlayOneShot(sounds[11]);
        particles[0].Play();
        StartCoroutine(turnOff());
    }
    IEnumerator turnOff()
    {
        transform.GetChild(4).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        cam.easeShake = true;
        yield return new WaitForSeconds(1f);
        pScript.walkAfterGoal = false;
        pScript.anim.SetTrigger("goal");
        dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        yield return new WaitForSeconds(2f);
        cam.fadeScreen(true);
        yield return new WaitUntil(()=>cam.fadeAnim>=1);
        bool newClear = false;
        if(data.currentLevelProgress!="")
        {
			char c = data.currentLevelProgress[0];
			if(c=='N')
				newClear = true;
			if(c!='D')
            {
                data.currentLevelProgress = data.currentLevelProgress.Remove(0, 1);
                if(data.cheated||DataS.difficulty!=2)
                    data.currentLevelProgress = data.currentLevelProgress.Insert(0,"F");
                else data.currentLevelProgress = data.currentLevelProgress.Insert(0,"D");
            }

            DataS.coins+=data.coins;
            data.saveLevelProgress(newClear,false);
            yield return new WaitUntil(()=> data.finishedSaving);
            DataS.resetValues();
        }
        if(newClear)
        DataS.lastLoadedWorld = 0;
        StartCoroutine(DataS.saveData(true));
        yield return 0;
        yield return new WaitUntil(()=> !DataS.saving);
        if(DataS.mode!=1)
        {
            if(newClear)
            {
            //Load credits level here.
            DataS.playCutscene = true;
            DataS.loadSceneWithoutLoadScreen(52);
            }
            else DataS.loadWorldWithLoadScreen(DataS.currentWorld);
        }
        else
        {
            if(newClear)
            {
                DataS.AndreMissionProgress = 1;
                DataS.MiroslavMissionProgress = 1;
                DataS.DjoleMissionProgress = 1;
                DataS.loadWorldWithLoadScreen(7);
            }
            else DataS.loadWorldWithLoadScreen(DataS.currentWorld);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag=="Player"&&!dead)
        {
            Vector3 pos = transform.position;
            Vector3 playerPos = pScript.transform.position;
			if(pScript.rb.velocity.y<-0.1f&&playerPos.y>pos.y+stompPointOffset)
			{
                dead = true;
                StopCoroutine(cor);
                anim.SetTrigger("Die");
                //cam.shakeCamera(0.2f,10f);
                Destroy(AutoScroll);
                if(data.mode!=1) aSource.PlayOneShot(sounds[0]);
                else aSource.PlayOneShot(sounds[12]);
                aSource.PlayOneShot(sounds[9]);
                data.playUnlistedSoundPoint(sounds[1],pos);
                //particles[3].Play();
                data.stopAllMusic();
				pScript.stompBoss(gameObject,false);
                StartCoroutine(playerCor());
                StartCoroutine(explodeSoundLoop());
                barScripts[0].DestroyFires();
                barScripts[1].DestroyFires();
			}
			else
			{
				if(Time.timeScale!=0 && pScript.invFrames==0)
				{
					pScript.Damage(false,false);
				}
				if(pScript.knockbackCor !=null)
				StopCoroutine(pScript.knockbackCor);
				pScript.knockbackCor = StartCoroutine(pScript.knockBack(-1,1,0.5f,true));
			}
        }
    }
}
