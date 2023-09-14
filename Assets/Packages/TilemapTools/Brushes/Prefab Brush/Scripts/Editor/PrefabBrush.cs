using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace UnityEditor
{
	[CreateAssetMenu]
	[CustomGridBrush(false, false, false, "Prefab Brush")]
	public class PrefabBrush : GridBrushBase
	{
		private const float k_PerlinOffset = 100000f;
		public bool savePrefabsBackup = false;
		public bool loadPrefabsBackup = false;
		[Space]
		public bool spawnInverted = false;
		public Color32 spriteColor = new Color32(255,255,255,255);
		public GameObject[] m_Prefabs;
		public GameObject[] m_Prefabs_backUp;
		public float m_PerlinScale = 0.5f;
		public int m_Z = 0;
		public int prefabID = 0;
		public bool isRandom = false;
		public Vector3 PositionOffset = new Vector3(.5f,0f, 0f);


		public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			// Do not allow editing palettes
			if (brushTarget.layer == 31)
				return;

			int index;
			if(isRandom)
			index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, m_PerlinScale, k_PerlinOffset)*m_Prefabs.Length), 0, m_Prefabs.Length - 1);
			else index = prefabID;
			GameObject prefab = m_Prefabs[index];
			GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
			Undo.RegisterCreatedObjectUndo((Object)instance, "Paint Prefabs");
			if (instance != null)
			{
				instance.transform.SetParent(brushTarget.transform);
				instance.transform.localPosition = grid.LocalToWorld(grid.CellToLocalInterpolated(new Vector3Int(position.x, position.y, position.z+m_Z) + PositionOffset));

				//Debug.Log(instance.transform.localPosition);
				if(instance.transform.childCount!=0&&instance.transform.GetChild(0).GetComponent<SpriteRenderer>()!=null)
				{
					instance.transform.GetChild(0).GetComponent<SpriteRenderer>().color = spriteColor;
				}
				else if(instance.transform.GetComponent<SpriteRenderer>()!=null)
				{
					instance.transform.GetComponent<SpriteRenderer>().color = spriteColor;
				}
				if(spawnInverted&&instance.transform.childCount!=0)
				{
					instance.transform.position+=Vector3.up;
					instance.transform.eulerAngles=new Vector3(0,0,180);
					instance.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = !instance.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX;
				}

			}
		}
		public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			// Do not allow editing palettes
			if (brushTarget.layer == 31)
				return;
			//Debug.Log(position+" "+brushTarget.transform.localPosition.z);
			Transform erased = GetObjectInCell(grid, brushTarget.transform, new Vector3Int(position.x, position.y, position.z+m_Z));
			//Debug.Log(grid.name+" "+brushTarget.transform+" "+new Vector3Int(position.x, position.y, position.z+m_Z));
			//if(erased==null)Debug.Log("found null.");
			//else Debug.Log("found "+erased.name);
			if (erased != null)
				Undo.DestroyObjectImmediate(erased.gameObject);
		}

		private static Transform GetObjectInCell(GridLayout grid, Transform parent, Vector3Int position)
		{
			int childCount = parent.childCount;
			Vector3 min = grid.LocalToWorld(grid.CellToLocalInterpolated(position));
			Vector3 max = grid.LocalToWorld(grid.CellToLocalInterpolated(position + Vector3Int.one));
			Bounds bounds = new Bounds((max + min)*.5f, max - min);

			for (int i = 0; i < childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (bounds.Contains(child.position))
					return child;
			}
			return null;
		}

		private static float GetPerlinValue(Vector3Int position, float scale, float offset)
		{
			return Mathf.PerlinNoise((position.x + offset)*scale, (position.y + offset)*scale);
		}
	}

	[CustomEditor(typeof(PrefabBrush))]
	public class PrefabBrushEditor : GridBrushEditorBase
	{
		private PrefabBrush prefabBrush { get { return target as PrefabBrush; } }

		private SerializedProperty m_Prefabs;
		private SerializedObject m_SerializedObject;
		public override GameObject[] validTargets
		{
			get
			{
				return GameObject.FindObjectsOfType<Tilemap>().Select(x => x.gameObject).ToArray();
			}
		}
		protected void OnEnable()
		{
			m_SerializedObject = new SerializedObject(target);
			m_Prefabs = m_SerializedObject.FindProperty("m_Prefabs");
		}

		public override void OnPaintInspectorGUI()
		{
			m_SerializedObject.UpdateIfRequiredOrScript();
			prefabBrush.savePrefabsBackup = EditorGUILayout.Toggle("Save prefabs", prefabBrush.savePrefabsBackup);
			prefabBrush.loadPrefabsBackup = EditorGUILayout.Toggle("Load prefabs", prefabBrush.loadPrefabsBackup);
			prefabBrush.spawnInverted = EditorGUILayout.Toggle("Spawn Upside-down", prefabBrush.spawnInverted);
			prefabBrush.spriteColor = EditorGUILayout.ColorField("Object tint: ",prefabBrush.spriteColor);
			if(prefabBrush.isRandom)
				prefabBrush.m_PerlinScale = EditorGUILayout.Slider("Perlin Scale", prefabBrush.m_PerlinScale, 0.001f, 0.999f);
			prefabBrush.m_Z = EditorGUILayout.IntField("Position Z", prefabBrush.m_Z);
			prefabBrush.PositionOffset = EditorGUILayout.Vector3Field("Offset", prefabBrush.PositionOffset);
			prefabBrush.prefabID = EditorGUILayout.IntField("Prefab ID", prefabBrush.prefabID);
			prefabBrush.isRandom = EditorGUILayout.Toggle("Is random?", prefabBrush.isRandom);
				
			EditorGUILayout.PropertyField(m_Prefabs,true);
			m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
			if(prefabBrush.savePrefabsBackup)
			{
				prefabBrush.savePrefabsBackup = false;
				prefabBrush.m_Prefabs_backUp = new GameObject[prefabBrush.m_Prefabs.Length];
				for(int i = 0; i<prefabBrush.m_Prefabs.Length;i++)
				{
					prefabBrush.m_Prefabs_backUp[i]=prefabBrush.m_Prefabs[i];
				}
				Debug.Log("Saved "+prefabBrush.m_Prefabs_backUp.Length+" prefabs.");
			}
			if(prefabBrush.loadPrefabsBackup)
			{
				prefabBrush.loadPrefabsBackup = false;
				prefabBrush.m_Prefabs = new GameObject[prefabBrush.m_Prefabs_backUp.Length];
				for(int i = 0; i<prefabBrush.m_Prefabs.Length;i++)
				{
					prefabBrush.m_Prefabs[i]=prefabBrush.m_Prefabs_backUp[i];
				}
				Debug.Log("Loaded "+prefabBrush.m_Prefabs.Length+" prefabs.");
			}
		}
	}
}
