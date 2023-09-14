using UnityEngine;

public class boundsResizer : MonoBehaviour
{
    public Vector2 newOffset,newSize;
    public void changeBoundsSize()
    {
        BoxCollider2D col=GetComponent<BoxCollider2D>();
        col.size = newSize;
        col.offset = newOffset;
    }
}
