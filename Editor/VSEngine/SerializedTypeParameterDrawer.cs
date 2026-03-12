using Core.VSEngine;
using UnityEditor;
using UnityEngine;


namespace Core.Editor.VSEngine
{
	// [CustomPropertyDrawer(typeof(SerializedTypeParameter))]
	// public class SerializedTypeParameterDrawer : PropertyDrawer
	// {
	// 	private static readonly SerializedTypeParameter nameofRef;
	// 	
	// 	public override void OnGUI(Rect position, SerializedProperty? prop, GUIContent label)
	// 	{
	// 		if (prop == null) {
	// 			return;
	// 		}
	// 	
	// 		SerializedProperty serializedType = prop.FindPropertyRelative(nameof(nameofRef.Type));
	// 		// SerializedProperty parameterName = prop.FindPropertyRelative(nameof(nameofRef.ParameterName));
	// 	
	// 		EditorGUI.BeginProperty(position, label, prop);
	// 		
	// 		
	// 		
	// 		EditorGUI.PropertyField(
	// 								new Rect(position.x, position.y, position.width / 2 - 5, position.height), 
	// 								serializedType, 
	// 								GUIContent.none
	// 								);
	// 	
	// 		// EditorGUI.PropertyField(
	// 		// 						new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height), 
	// 		// 						parameterName, 
	// 		// 						GUIContent.none
	// 		// 						);
	// 	
	// 		EditorGUI.EndProperty();
	// 	}
	// }
}