using UnityEngine;
using System;
using UnityEngine.Events;

// Script for executing an event on particle system stopping
public class particleStopEvent : MonoBehaviour
{
    #region main

    // Internal unity function called when the particle system is stopped, the ParticleSystem stop action must be set to Callback to activate this function
    void OnParticleSystemStopped()
    {
        EventTriggered();
    }
    #endregion
	// Event serialization
    [Serializable] public class ParticleStopEvent : UnityEvent { }
    [SerializeField] private ParticleStopEvent particleStop = new ParticleStopEvent();
     
     public ParticleStopEvent onParticleStopEvent{ get { return particleStop; } set { particleStop = value; } }
 
     public void EventTriggered()
     {
        onParticleStopEvent.Invoke();
     }
}
