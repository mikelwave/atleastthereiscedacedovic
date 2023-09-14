using UnityEngine;

public class boomStaticScript : MonoBehaviour
{
    GameData data;
    public void assign(GameData a)
    {
        data = a;
    }
    public void scanForParallels()
    {
        data.spawnBoomStatic(transform.position);
    }
    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
