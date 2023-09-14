using System.Collections;
using UnityEngine;

// Script for the impact star effect when hitting walls or enemies
public class impactScript : MonoBehaviour
{
    SpriteRenderer render; // Sprite renderer
    Coroutine c;

    // Animation coroutine
    IEnumerator animate()
    {
        render.flipX = false;
        for (int i = 0;i<3;i++)
        {
            render.flipX = !render.flipX;
            yield return 0;
            yield return 0;
        }
        render.flipX = false;
        gameObject.SetActive(false);
        c = null;
    }

    // Called during object pool generation
    public void initialize()
    {
        render = GetComponent<SpriteRenderer>();
    }

    // Manually set the object position
    public void setPos(Vector3 pos)
    {
        transform.position = pos;
    }

    // Spawn the object and play the animation
    public void spawn()
    {
        gameObject.SetActive(true);
        if(c!=null)StopCoroutine(c);
        c = StartCoroutine(animate());
    }
}
