using System;
using UnityEngine;
using UnityEditor;

namespace VisualCanvasEditor
{
	public class InputPopup : EditorWindow
	{
		private static Action onCancel;
		private static Action<string> onOk;
		private static InputPopup popup;
		private static bool isOpen;

		public static void Show(Action<string> onOkAction, Action onCancelAction = null)
		{
			isOpen = true;

			onOk = onOkAction;
			onCancel = onCancelAction;

			int popupWidth = 350;
			int popupHeight = 150;

			popup = CreateInstance<InputPopup>();

			if (popup != null)
			{
				GUIContent title = new GUIContent("Create Canvas Controller");
				popup.titleContent  = title;
				popup.position = new Rect(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2, popupWidth, popupHeight);
				popup.Focus();
				popup.ShowUtility();
			}
		}

		protected virtual void OnLostFocus()
		{
			if (isOpen)
			{
				popup?.Focus();
			}
		}

		string inputText = string.Empty;

		private void OnGUI()
		{
			GUIContent title = new GUIContent("Insert the name of your Canvas Controller");
			EditorGUILayout.LabelField(title);

			GUILayout.Space(10);

			inputText = EditorGUILayout.TextField(inputText);

			GUILayout.Space(40);

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Ok"))
			{
				isOpen = false;
				Close();
				onOk?.Invoke(inputText);				
			}

			if (GUILayout.Button("Cancel"))
			{
				isOpen = false;
				Close();
				onCancel?.Invoke();
			}

			GUILayout.EndHorizontal();
		}
	}
}