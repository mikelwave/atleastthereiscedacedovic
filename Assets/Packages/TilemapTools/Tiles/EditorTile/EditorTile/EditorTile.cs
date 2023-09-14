using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace UnityEngine.Tilemaps
{
	[Serializable]
	public class EditorTile : Tile
	{
		[SerializeField]
		public Sprite m_SpriteEdit;
		public Sprite m_SpriteGame;
		public Sprite m_SpriteCustomGame;

		public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
		{
			base.GetTileData(location, tileMap, ref tileData);
			//Debug.Log(location);
				if(Application.isPlaying)
				{
					if(m_SpriteCustomGame==null)
					tileData.sprite = m_SpriteGame;
					else tileData.sprite = m_SpriteCustomGame;
				}
				else
				{
					if(m_SpriteCustomGame!=null)
					m_SpriteCustomGame=null;
					tileData.sprite = m_SpriteEdit;
				}

				if(tileData.sprite != null)
				tileData.colliderType = Tile.ColliderType.Grid;
				else tileData.colliderType = Tile.ColliderType.None;
		}

#if UNITY_EDITOR
		[MenuItem("Assets/Create/Editor Tile")]
		public static void CreateEditorTile()
		{
			string path = EditorUtility.SaveFilePanelInProject("Save Editor Tile", "New Editor Tile", "asset", "Save Editor Tile", "Assets");

			if (path == "")
				return;

			AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<EditorTile>(), path);
		}
#endif
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(EditorTile))]
	public class EditorTileEditor : Editor
	{
		private EditorTile tile { get { return (target as EditorTile); } }

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

				tile.m_SpriteEdit = (Sprite) EditorGUILayout.ObjectField("Editor Sprite ", tile.m_SpriteEdit, typeof(Sprite),false,null);
				tile.m_SpriteGame = (Sprite) EditorGUILayout.ObjectField("Game Sprite ", tile.m_SpriteGame, typeof(Sprite),false,null);

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(tile);
		}
	}
#endif
}
