using UnityEditor;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(TextBox))]
public class EmotionPropertyDrawer : Editor {

	public override void OnInspectorGUI()
	{
		var TextBox = target as TextBox;
		EditorGUILayout.LabelField("Text options", EditorStyles.boldLabel);
		TextBox.TextFile = (TextAsset)EditorGUILayout.ObjectField("Text file ",TextBox.TextFile,typeof(TextAsset),true);
		TextBox.startLine = EditorGUILayout.IntField("Start line ",TextBox.startLine);
		TextBox.eventInt = EditorGUILayout.IntField("Event Integer ",TextBox.eventInt);
		TextBox.restoreDefaultSpeeds = EditorGUILayout.Toggle("Restore def. text speeds: ",TextBox.restoreDefaultSpeeds);
		EditorGUILayout.LabelField("Choice options set by NPCs.", EditorStyles.boldLabel);
		TextBox.option1StartLine = EditorGUILayout.IntField("Option 1 StartLine:",TextBox.option1StartLine);
		TextBox.option2StartLine = EditorGUILayout.IntField("Option 2 StartLine:",TextBox.option2StartLine);
		TextBox.option3StartLine = EditorGUILayout.IntField("Option 3 StartLine:",TextBox.option3StartLine);
		TextBox.option4StartLine = EditorGUILayout.IntField("Option 4 StartLine:",TextBox.option4StartLine);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Text speeds [SLOW, NORMAL, FAST]", EditorStyles.boldLabel);
		TextBox.letterSpeeds = EditorGUILayout.Vector3Field("Letter Speeds: ",TextBox.letterSpeeds);
		TextBox.comaSpeeds = EditorGUILayout.Vector3Field("Coma Speeds: ",TextBox.comaSpeeds);
		TextBox.dotSpeeds = EditorGUILayout.Vector3Field("Dot Speeds: ",TextBox.dotSpeeds);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);
		TextBox.text_type = (AudioClip)EditorGUILayout.ObjectField("Text type ",TextBox.text_type,typeof(AudioClip),true);
		TextBox.letter_type = (AudioClip)EditorGUILayout.ObjectField("Letter type ",TextBox.letter_type,typeof(AudioClip),true);
		TextBox.letter_type_angry = (AudioClip)EditorGUILayout.ObjectField("Letter type alt ",TextBox.letter_type_angry,typeof(AudioClip),true);
		TextBox.special_sound = (AudioClip)EditorGUILayout.ObjectField("Special sound ",TextBox.special_sound,typeof(AudioClip),true);
		TextBox.shake_Sound = (AudioClip)EditorGUILayout.ObjectField("Shake sound ",TextBox.shake_Sound,typeof(AudioClip),true);
		TextBox.selection = (AudioClip)EditorGUILayout.ObjectField("Selection sound ",TextBox.selection,typeof(AudioClip),true);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
		TextBox.colors[0] = (Color32)EditorGUILayout.ColorField("Highlight color",TextBox.colors[0]);
		TextBox.colors[1] = (Color32)EditorGUILayout.ColorField("Normal color",TextBox.colors[1]);
	}
}
