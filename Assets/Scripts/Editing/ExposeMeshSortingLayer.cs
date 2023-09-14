using UnityEngine;

[ExecuteInEditMode]
public class ExposeMeshSortingLayer : MonoBehaviour {
	MeshRenderer m;
	public string sortingLayerName;
	public int sortOrder = 0;
	// Use this for initialization
	void Start () {
		m = GetComponent<MeshRenderer>();
		m.sortingLayerName = sortingLayerName;
		m.sortingOrder = sortOrder;
		//print(m.sortingOrder);
	}
}
