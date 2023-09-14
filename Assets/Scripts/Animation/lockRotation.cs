using UnityEngine;

public class lockRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.eulerAngles = Vector3.zero;
    }
}
