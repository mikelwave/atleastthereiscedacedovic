using System.Collections;
using UnityEngine;

public class BrickSpringTag : MonoBehaviour
{
    playerSprite pSprite;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(load());
    }
    IEnumerator load()
    {
        yield return 0;
        pSprite = GetComponent<SpringScript>().pSprite;
    }
    // Update is called once per frame
    void Update()
    {
        if(pSprite!=null)
        {
            if(pSprite.state==0&&transform.tag!="Spring") transform.tag = "Spring";
            else if(pSprite.state!=0&&transform.tag=="Spring") transform.tag = "Ground";
        }
    }
}
