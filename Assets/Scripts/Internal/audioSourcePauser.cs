using UnityEngine;

public class audioSourcePauser : MonoBehaviour
{
    public AudioSource[] aSources;
    bool active = true;
    public bool onlyLooping = false;
    // Start is called before the first frame update
    void Start()
    {
        if(aSources.Length==0||aSources[0]==null)
        {
            aSources = new AudioSource[1];
            aSources[0]=GetComponent<AudioSource>();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(active&&Time.timeScale==0)
        {
            toggleAudioSources(false);
        }
        else if(!active&&Time.timeScale!=0)
        {
            toggleAudioSources(true);
        }
    }
    void toggleAudioSources(bool toggle)
    {
        active = toggle;
        foreach(AudioSource a in aSources)
        {
            if(!toggle)
            {
                if(a.isPlaying)
                {
                    if(!onlyLooping||a.loop)
                    {
                        a.Pause();
                        //print("Paused "+a);
                    }
                }
            }
            else
            {
                a.UnPause();
                //print("Unpaused "+a);
            }
        }
    }
}
