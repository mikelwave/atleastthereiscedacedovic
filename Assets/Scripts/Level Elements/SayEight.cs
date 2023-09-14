using System.Collections;
using UnityEngine;

public class SayEight : MonoBehaviour
{
    public AudioClip[] sounds;
    AudioSource aSource;
    PlayerScript pScript;
    bool canPlay = true;
    Coroutine cor;
    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(GameObject.Find("Player_main").transform);
        transform.localPosition = Vector3.zero;
        aSource = GetComponent<AudioSource>();
        pScript = transform.parent.GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0)
        {
            if(pScript.mode==1)Destroy(this.gameObject);
            if(canPlay&&pScript.grounded&&Mathf.Abs(pScript.rb.velocity.x)<0.1f)
            {
                if(Input.GetKeyDown(KeyCode.Alpha8)||Input.GetKeyDown(KeyCode.Keypad8))
                {
                    //print("Eight.");
                    if(cor!=null)StopCoroutine(cor);
                    cor = StartCoroutine(Sequence());
                }
            }
            if(aSource.isPlaying&&Mathf.Abs(pScript.rb.velocity.x)>0.1f)
            {
                //print("Interrupted");
                if(cor!=null)StopCoroutine(cor);
                pScript.anim.SetBool("Talk",false);
                aSource.Stop();
                canPlay = true;
            }
        }
    }
    IEnumerator Sequence()
    {
        canPlay = false;
        aSource.clip = sounds[Random.Range(0,sounds.Length)];
        aSource.Play();
        yield return 0;
        pScript.anim.SetBool("Talk",true);
        yield return new WaitForSeconds(0.2f);
        pScript.anim.SetBool("Talk",false);
        yield return new WaitUntil(()=>!aSource.isPlaying);
        canPlay = true;
        //print("Sequence ended.");
    }
}
