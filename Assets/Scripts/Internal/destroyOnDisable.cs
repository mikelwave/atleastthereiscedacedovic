using UnityEngine;

public class destroyOnDisable : MonoBehaviour
{
    void OnDisable()
    {
        Destroy(gameObject);
    }
}
