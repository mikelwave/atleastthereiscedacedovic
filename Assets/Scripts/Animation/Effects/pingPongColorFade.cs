using System.Collections;
using UnityEngine;

// Script for a simple ping pong color fading effect on sprites
public class pingPongColorFade : MonoBehaviour
{

    public Color[] colors = new Color[0];
    public float speed = 1;
    public bool loop = true;
    float progress = 0;

    Coroutine cor;
    SpriteRenderer render;

    // Color flash coroutine
    IEnumerator colorTick()
    {
        if(colors.Length!=0)
        {
            progress = 0;
            int targetInt = 1;
            Color c = render.color;
            Color target = colors[targetInt];
            loopA: while(progress<1)
            {
                progress=Mathf.Clamp(progress+=Time.deltaTime*speed,0,1);
                render.color = Color.Lerp(c,target,progress);
                //render.color = new Color(c.r,c.b,c.g,Mathf.Lerp(startFade,target,progress));
                yield return 0;
            }
            if(loop)
            {
                targetInt++;
                if(targetInt>colors.Length-1)targetInt=0;
                //print(targetInt);
                target = colors[targetInt];
                c = render.color;
                progress = 0;
                goto loopA;

            }
            cor = null;
        }
    }

    // Called when the object is enabled
    void OnEnable()
    {
        if(render==null) render = GetComponent<SpriteRenderer>();
        if(colors.Length>0)
        render.color = colors[0];
        if(cor!=null)StopCoroutine(cor);
        cor = StartCoroutine(colorTick());
    }

    // Called when the object is disabled
    void OnDisable()
    {
        if(cor!=null)StopCoroutine(cor);
    }

    // Called when the object is destroyed
    void Destroy()
    {
        Destroy(gameObject);
    }
}
