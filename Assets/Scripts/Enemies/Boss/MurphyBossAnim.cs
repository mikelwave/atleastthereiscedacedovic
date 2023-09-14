using System.Collections;
using UnityEngine;

public class MurphyBossAnim : MonoBehaviour
{
    MGCameraController cam;
    public bool canShake = false;
    public bool zap = false;
    public bool dmgEnd = false;
    MurphyBossScript mBoss;
    ParticleSystem part;
    AnimatorSoundPlayer aSound;
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        aSound = GetComponent<AnimatorSoundPlayer>();
        part = GetComponent<ParticleSystem>();
        mBoss = transform.parent.GetComponent<MurphyBossScript>();
    }

    void Shake()
    {
        if(canShake)
        cam.shakeCamera(0.1f,0.2f);
    }
    void bigShake()
    {
        cam.shakeCamera(0.4f,0.5f);
    }
    void playParticles()
    {
        StartCoroutine(playParticlesCor(true));
    }
    void playParticlesNoExplosion()
    {
        StartCoroutine(playParticlesCor(false));
    }
    IEnumerator playParticlesCor(bool withExplosion)
    {
        part.Play(false);
        if(withExplosion)
        {
        transform.GetChild(5).gameObject.SetActive(true);
        aSound.playSound(8);
        }
        yield return new WaitForSeconds(0.05f);
        part.Stop(false,ParticleSystemStopBehavior.StopEmitting);
    }
    void setZap()
    {
        zap = true;
    }
    void explodeLoop(int play)
    {
        if(play==1)
        mBoss.exploding = true; else mBoss.exploding = false;
        if(play==1)
        mBoss.StartCoroutine(mBoss.explodeSoundLoop());
    }
}
