using UnityEngine;
using System.Collections;

public class megaSatanHandEvent : MonoBehaviour
{
    megaSatan_Boss mBoss;
    Transform handsWarning;
    ParticleSystem[] pSys = new ParticleSystem[2];
    // Start is called before the first frame update
    void Start()
    {
        mBoss = transform.parent.parent.GetComponent<megaSatan_Boss>();
        handsWarning = transform.GetChild(3);
        pSys[0] = transform.GetChild(4).GetComponent<ParticleSystem>();
        pSys[1] = transform.GetChild(5).GetComponent<ParticleSystem>();
    }
    public void enableWarningPos()
    {
        handsWarning.gameObject.SetActive(true);
    }
    IEnumerator handsWarningCor()
    {
        yield return new WaitForSeconds(0.2f);
        enableWarningPos();
    }
    public void setWarningPos(float Xpos)
    {
        handsWarning.position = new Vector3(Xpos,handsWarning.position.y,handsWarning.position.z);
        mBoss.playWarnSound(handsWarning.position,3);
        StartCoroutine(handsWarningCor());
    }
    public void bossEvent(int i)
    {
        mBoss.handEvent(i);
    }
    public void disableHand(int i)
    {
        mBoss.handEvent(9);
        if(i==0) transform.GetChild(0).gameObject.SetActive(false);
        else transform.GetChild(1).gameObject.SetActive(false);
    }
    public void enableParticles()
    {
        if(transform.GetChild(0).gameObject.activeInHierarchy)
        {
            pSys[0].gameObject.SetActive(true);
            pSys[0].Play();
        }
        if(transform.GetChild(1).gameObject.activeInHierarchy)
        {
            pSys[1].gameObject.SetActive(true);
            pSys[1].Play();
        }
    }
    public void disableParticles()
    {
        pSys[0].Stop(false,ParticleSystemStopBehavior.StopEmitting);
        pSys[1].Stop(false,ParticleSystemStopBehavior.StopEmitting);
    }
}
