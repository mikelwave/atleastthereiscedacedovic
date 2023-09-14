using System.Collections;
using UnityEngine;

public class princessScareScript : MonoBehaviour
{
    SpriteRenderer render;
    IEnumerator disappear()
    {
        render.enabled = true;
        //print("active");
        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
        render.enabled = false;
    }
    public void princessAppear()
    {
        StartCoroutine(disappear());
    }
}
