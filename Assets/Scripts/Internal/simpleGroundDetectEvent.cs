using UnityEngine;
using System;
using UnityEngine.Events;

public class simpleGroundDetectEvent : MonoBehaviour
{
    #region events
    // Start is called before the first frame update
    void OnCollisionEnter2D(Collision2D other)
    {
        //print(other.gameObject.tag);
        if(other.gameObject.tag=="Ground"
        ||other.gameObject.tag=="semiSolid")
        {
            eventTriggered();
        }
    }
    #endregion
    [Serializable]
    public class groundEvent : UnityEvent {}

    [SerializeField]
    private groundEvent groundE = new groundEvent();

    public groundEvent onGroundEvent
    { 
        get
        {
            return groundE;
        }
        set
        {
            groundE = value;
        }
    }
    public void eventTriggered()
    {
        onGroundEvent.Invoke();
    }
}
