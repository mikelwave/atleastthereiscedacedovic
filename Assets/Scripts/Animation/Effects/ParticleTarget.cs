using System.Collections;
using UnityEngine;

// Script for assigning a follow target for the particles spawned by particle system
public class ParticleTarget : MonoBehaviour
{
	ParticleSystem pS; // Particle system
	public Transform Target; // Target transform
    Vector3 lastTTargetPos = Vector3.zero; // Last recorded target position

    // Called when the object is enabled
    void OnEnable()
	{
		pS = GetComponent<ParticleSystem>();
        StartCoroutine(IToTarget());
    }

    // Move particles towards target coroutine
    IEnumerator IToTarget()
    {
        // If particle system is not playing, begin playing
        if (!pS.isPlaying) pS.Play();
         
        // Wait before moving particles
        yield return new WaitForSeconds(0.8f);
         
         
        // Allocate reference array
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[pS.particleCount];
         
         
        // This loop executes over several frames
        for (float t = 0f; t < 0.5f; t += 0.025f)
        {
            // Get the particle list
            int count = pS.GetParticles(particles);

            // Update each particle's position
            for (int i=0; i<count; i++)
            {
                if(Target!=null)
                {
                    lastTTargetPos = Target.position;
                }

                particles[i].position = Vector3.Lerp(particles[i].position, lastTTargetPos, t*Time.timeScale);

                if(particles[i].position==lastTTargetPos)
                {
                    particles[i].remainingLifetime = 0;
                    if(i==count-1) break;
                }
            }

            // Set the particle list
            pS.SetParticles(particles, count);
            
            // Wait until the next particle update (maybe change this to 'yield return 0')
            yield return new WaitForSeconds(0.00001f);
        }
         
        //once loop is finished, clear particles
        pS.Clear();
        Destroy(gameObject);
    }
}
