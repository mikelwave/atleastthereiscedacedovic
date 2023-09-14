using UnityEngine;
using System;
using UnityEngine.Events;

public class eventTrigger : MonoBehaviour
{
    #region main
    public bool destroyOnTrigger = false;
    public bool ObjectActivatorEnabled = false;
    public bool onlyTriggerOnce = false;
    bool triggered = false,triggered2 = false,triggered3 = false;
    public bool disablePhysicalTrigger = false;
    public bool killOnExit = false;
    public bool disableOnExit = false;
    public bool keepTriggeringWhileInside = false;
    void trigFunc(Collider2D other)
    {
        if(!disablePhysicalTrigger)
        {
            //print(other.name);
            if((other.name=="PlayerCollider"||other.name=="WarpBox")&&!ObjectActivatorEnabled)
            {
                EventTriggered();
                if(destroyOnTrigger)Destroy(gameObject);
            }
            else if(other.name=="ObjectActivator"&&ObjectActivatorEnabled)
            {
                EventTriggered();
                if(destroyOnTrigger)Destroy(gameObject);
            }
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if(keepTriggeringWhileInside)
        {
            trigFunc(other);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        trigFunc(other);
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(!disablePhysicalTrigger)
        {
            if((other.name=="PlayerCollider"||other.name=="WarpBox")&&!ObjectActivatorEnabled)
            {
                ExitEventTriggered();
                if(killOnExit)Destroy(gameObject);
                if(disableOnExit)gameObject.SetActive(false);
            }
            else if(other.name=="ObjectActivator"&&ObjectActivatorEnabled)
            {
                ExitEventTriggered();
                if(killOnExit)Destroy(gameObject);
                if(disableOnExit)gameObject.SetActive(false);
            }
        }
    }

    #endregion
	//my event
     [Serializable]
     public class MainEvent : UnityEvent { }
 
     [SerializeField]
     private MainEvent mainEvent = new MainEvent(),Event2 = new MainEvent(),ExitEvent = new MainEvent();
     
     public MainEvent onMainEvent { get { return mainEvent; } set { mainEvent = value; } }
     public MainEvent onEvent2 { get { return Event2; } set { Event2 = value; } }
     public MainEvent onEvent3 { get { return ExitEvent; } set { ExitEvent = value; } }
 
     public void EventTriggered()
     {
         if(!triggered&&onlyTriggerOnce||!onlyTriggerOnce)
         {
            triggered = true;
            onMainEvent.Invoke();
         }
     }
     public void Event2Triggered()
     {
         if(!triggered2&&onlyTriggerOnce||!onlyTriggerOnce)
         {
            triggered2 = true;
            Event2.Invoke();
        }
     }
     public void ExitEventTriggered()
     {
        if(!triggered3&&onlyTriggerOnce||!onlyTriggerOnce)
        {
            triggered3 = true;
            ExitEvent.Invoke();
        }
     }
}
