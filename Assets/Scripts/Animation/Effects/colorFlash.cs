using System.Collections;
using UnityEngine;

// Script for a color flash effect
public class colorFlash : MonoBehaviour
{
    public Color[] colors = new Color[0];
    public float timeDelay = 0.05f;
    Coroutine cor;
    bool objenabled = false;
    public float alpha;
    SpriteRenderer render;


    // Color flash coroutine
    IEnumerator colorTick()
    {
        if(colors.Length!=0)
        while(objenabled)
        {
            for(int i = 0; i<colors.Length;i++)
            {
                render.color = colors[i];
                yield return new WaitForSeconds(timeDelay);
            }
        }
    }

    // Called when the object is enabled
    void OnEnable()
    {
        if(render==null) render = GetComponent<SpriteRenderer>();
        objenabled = true;
        if(cor!=null)StopCoroutine(cor);
        cor = StartCoroutine(colorTick());
    }

    // Called when the object is disabled
    void OnDisable()
    {
        objenabled = false;
        if(cor!=null)StopCoroutine(cor);
    }
    
    // Called when the object is destroyed
    void Destroy()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame update
    void Update()
    {
        if(Time.timeScale!=0&&enabled)
        if(render.color.a!=alpha)
        {
            render.color = new Color(render.color.r,render.color.g,render.color.b,alpha);
        }
    }
}
