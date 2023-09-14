using UnityEngine;
using System;
using UnityEngine.Events;

public class gameMusicChanger : MonoBehaviour
{
    #region main
    bool changed = false;
    GameData data;
    public int BPM = 120,prevBPM;
	public AudioClip newMusicIntro,newMusicLoop,prevMusicIntro,prevMusicLoop;
    // Start is called before the first frame update
    void Start()
    {
        data = GameObject.Find("_GM").GetComponent<GameData>();
        prevMusicIntro = data.Intro;
        prevMusicLoop = data.MainLoop;
		prevBPM = GameObject.Find("LevelGrid").GetComponent<storeLevelData>().BPM;
    }
    public void outsideTriggerIn()
    {
        if(!changed)
        {
            changed = true;
            EnterEventTriggered();
            data.changeMusicWithIntro(newMusicIntro,newMusicLoop,BPM);
        }
    }
    public void outsideTriggerOut()
    {
        if(!changed)
        {
            changed = false;
            ExitEventTriggered();
            data.changeMusicWithIntro(prevMusicIntro,prevMusicLoop,BPM);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name=="ObjectActivator")
        {
            outsideTriggerIn();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.name=="ObjectActivator")
        {
            outsideTriggerOut();
        }
    }
    #endregion
	//my event
     [Serializable]
     public class MainEvent : UnityEvent { }
     [Serializable]
     public class MainExitEvent : UnityEvent { }
 
     [SerializeField]
     private MainEvent mainEvent = new MainEvent();
     [SerializeField]
     private MainExitEvent mainExitEvent = new MainExitEvent();
     public MainEvent onMainEvent { get { return mainEvent; } set { mainEvent = value; } }
     public MainExitEvent onMainExitEvent { get { return mainExitEvent; } set { mainExitEvent = value; } }
 
     public void EnterEventTriggered()
     {
         onMainEvent.Invoke();
     }
     public void ExitEventTriggered()
     {
         onMainExitEvent.Invoke();
     }
}
