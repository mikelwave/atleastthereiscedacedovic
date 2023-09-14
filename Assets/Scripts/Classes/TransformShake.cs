using UnityEngine;

[System.Serializable]
public class TransformShake{

	public Transform tr;
    public Coroutine cor;
    public Vector3 orgPos;

	public TransformShake (Transform trs,Vector3 orgP, Coroutine cr)
	{
        tr = trs;
        orgPos = orgP;
		cor = cr;
	}
    public void reset()
    {
        tr.localPosition = orgPos;
        cor = null;
        orgPos = Vector3.zero;
        tr = null;
    }
}
