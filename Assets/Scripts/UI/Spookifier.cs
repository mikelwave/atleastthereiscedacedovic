using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Spookifier : MonoBehaviour
{
    Grayscale grayscale;
    VignetteAndChromaticAberration aber;
    NoiseAndGrain noise;
    AudioSource[] musics = new AudioSource[2];
    GlitchEffect glitch;
    public float nospookchance = 100,spookPeriod = 1f;
    public float vol = 0.7f;
    bool midEffect = false;
    public bool modifyMusic = true,random = true;
    Coroutine cor;
    GameObject textBox;
    IEnumerator creepyMode()
    {
        midEffect = true;
        toggle(true);
        yield return new WaitForSeconds(Random.Range(0.1f,spookPeriod));
        toggle(false);
        midEffect = false;

    }
    public void toggle(bool enabled)
    {
        grayscale.enabled = enabled;
        noise.enabled = enabled;
        aber.enabled = enabled;
        glitch.enabled = enabled;
        if(modifyMusic)
        {
            if(enabled)
            {
                musics[1].volume = vol;
                musics[0].volume = 0;
            }
            else
            {
                musics[0].volume = vol;
                musics[1].volume = 0;
            }
        }
    }
    public void disable()
    {
        random = false;
        toggle(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        grayscale = GetComponent<Grayscale>();
        aber = GetComponent<VignetteAndChromaticAberration>();
        noise = GetComponent<NoiseAndGrain>();
        glitch = GetComponent<GlitchEffect>();
        textBox = GameObject.Find("Textbox_Canvas").transform.GetChild(0).gameObject;
        if(modifyMusic)
        {
            musics[0]= GameObject.Find("_GM").transform.GetChild(0).GetComponent<AudioSource>();
            musics[1]=transform.GetChild(0).GetComponent<AudioSource>();
        }
        //vol = musics[0].volume;
    }

    // Update is called once per frame
    void Update()
    {
        if(random)
        {
            if(Time.timeScale!=0&&!midEffect&&!textBox.activeInHierarchy)
            {
                int i = (int)Random.Range(0,nospookchance+1);
                //rolled spooky
                if(i==0)
                {
                    cor = StartCoroutine(creepyMode());
                }
            }
            else if(Time.timeScale==0||textBox.activeInHierarchy)
            {
                if(cor!=null||midEffect)
                {
                    midEffect = false;
                    StopCoroutine(cor);
                    toggle(false);
                }
            }
        }
    }
}
