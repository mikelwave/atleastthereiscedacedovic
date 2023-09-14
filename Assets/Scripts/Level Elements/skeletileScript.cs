using UnityEngine;

public class skeletileScript : MonoBehaviour
{
    ParticleSystem boneParticles;
    ParticleSystem boneDust;
    Animator anim;
    GameData data;
    Transform cam;
    public AudioClip[] sounds;
    AudioSource aSource;
    // Start is called before the first frame update
    void Start()
    {
        boneParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
        boneDust = transform.GetChild(1).GetComponent<ParticleSystem>();
        aSource = GetComponent<AudioSource>();
        data = GameObject.Find("_GM").GetComponent<GameData>();
        cam = GameObject.Find("Main Camera").transform;
        anim = GetComponent<Animator>();
        gameObject.SetActive(false);
    }
    public void Spawn(Vector3Int pos,int crushing)
    {
        transform.position = pos+new Vector3(0.5f,0.5f+data.ssmap.transform.position.y);
        gameObject.SetActive(true);
        if(crushing==0)
        {
            anim.SetTrigger("crush");
        }
        else if(crushing==1)
        {
            //print("cam: "+Mathf.Abs(Mathf.Abs(cam.position.x)-Mathf.Abs(transform.position.x))+" pos: "+Mathf.Abs(Mathf.Abs(cam.position.y)-Mathf.Abs(transform.position.y)));
            if(Mathf.Abs(Mathf.Abs(cam.position.x)-Mathf.Abs(transform.position.x))<=16
            &&Mathf.Abs(Mathf.Abs(cam.position.y)-Mathf.Abs(transform.position.y))<=9)
            anim.SetTrigger("spawn");
            else
            {
                setTile(1);
            }
        }
        else
        {
            anim.SetTrigger("crushNoRespawn");
        }
    }
    public void Disable()
    {
        gameObject.SetActive(false);
    }
    public void particlePlay(int mode)
    {
        if(mode==0)
        boneDust.Play();
        else boneParticles.Play();
    }
    public void playSound(int id)
    {
        //print("play "+id);
        aSource.PlayOneShot(sounds[Mathf.Clamp(id,0,sounds.Length)]);
    }
    public void setTile(int placeDown)
    {
        Vector3Int pos = new Vector3Int(Mathf.RoundToInt(transform.position.x-0.5f)-Mathf.RoundToInt(data.ssmap.transform.position.x),
                                        Mathf.RoundToInt(transform.position.y-0.5f)-Mathf.RoundToInt(data.ssmap.transform.position.y),
                                        Mathf.RoundToInt(data.ssmap.transform.position.z));
        if(placeDown == 0)
        {
            data.ssmap.SetTile(pos,null);
            data.addSkeletileCor(pos);
        }
        else if(placeDown == 2)
        {
            data.ssmap.SetTile(pos,null);
        }
        else
        {
            data.ssmap.SetTile(pos,data.replacementBlocks[6]);
            gameObject.SetActive(false);
        }
    }
}
