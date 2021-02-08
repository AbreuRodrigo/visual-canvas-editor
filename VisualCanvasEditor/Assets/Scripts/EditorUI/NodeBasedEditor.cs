using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VisualCanvasEditor
{
	public class NodeBasedEditor : EditorWindow
	{
		private string ASSETS_PATH = "Assets";
		private string VISUAL_CANVAS_EDITOR_PATH = "VisualCanvasEditor";
		private string CANVAS_CONTROLLER_SUFIX = "CanvasController";
		private string SCENE_EXTENSION = ".unity";

		private List<ScreenNode> nodes;
		private List<Connection> connections;

		private ConnectionPoint selectedInPoint;
		private ConnectionPoint selectedOutPoint;

		private Vector2 offset;
		private Vector2 drag;

		private Color bezierColor;
		private Texture2D screenBackgroundTexture;

		private Vector2 lastMousePosition;

		//Styles
		private GUIStyle style;
		private GUIStyle selectedStyle;
		private GUIStyle titleStyle;
		private GUIStyle inPointStyle;
		private GUIStyle outPointStyle;

		//Dynamic Textures
		private Texture2D nodeBgTexture;
		private Texture2D nodeBgTitleTexture;
		private Texture2D nodeSelectedTexture;
		private Texture2D connectionBgNormal;
		private Texture2D connectionBgActive;

		[MenuItem("Visual Canvas Editor/Create Canvas Controller")]
		private static void CreateCanvasController()
		{

		}

		[MenuItem("Visual Canvas Editor/Manage Canvas Controllers")]
		private static void OpenWindow()
		{
			Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
			NodeBasedEditor window = GetWindow<NodeBasedEditor>("Visual Canvas Editor", gameViewType);
		}

		private void OnEnable()
		{
			if (nodes == null)
			{
				nodes = new List<ScreenNode>();
			}

			CreateCanvasEditorDirIfNonExistant();

			InitializeTextures();
			InitializeStyles();

			bezierColor = new Color(0.6f, 1f, 0);
		}

		private void OnGUI()
		{
			InitializeTextures();
			InitializeStyles();

			DrawBackground();

			DrawGrid(20, 0.1f, Color.gray);
			DrawGrid(100, 0.15f, Color.gray);

			DrawNodes();
			DrawConnections();

			DrawConnectionLine(Event.current);

			ProcessNodeEvents(Event.current);
			ProcessEvents(Event.current);

			if (GUI.changed)
			{
				Repaint();
			}
		}

		private void InitializeTextures()
		{
			if (screenBackgroundTexture == null)
			{
				screenBackgroundTexture = TextureUtils.MakeTex(1, 1, new Color(0, 0.15f, 0.23f));
			}

			if (nodeBgTexture == null)
			{
				nodeBgTexture = TextureUtils.MakeTex(1, 1, new Color(0.168f, 0.20f, 0.211f));
			}

			if (nodeSelectedTexture == null)
			{
				nodeSelectedTexture = TextureUtils.MakeTex(1, 1, new Color(0.584f, 0.976f, 0));
			}

			if (nodeBgTitleTexture == null)
			{
				nodeBgTitleTexture = TextureUtils.MakeTex(1, 1, new Color(0.219f, 0.258f, 0.266f));
			}

			if (connectionBgNormal == null)
			{
				connectionBgNormal = TextureUtils.MakeTex(1, 1, new Color(0.168f, 0.20f, 0.211f));
			}

			if (connectionBgActive == null)
			{
				connectionBgActive = TextureUtils.MakeTex(1, 1, new Color(0.168f, 0.20f, 0.211f));
			}
		}

		private void InitializeStyles()
		{
			if (inPointStyle == null)
			{
				inPointStyle = new GUIStyle();		
			}
			if (inPointStyle != null && inPointStyle.normal.background == null)
			{
				inPointStyle.normal.background = connectionBgNormal;
			}
			if (inPointStyle != null && inPointStyle.active.background == null)
			{
				inPointStyle.active.background = connectionBgActive;
			}

			if (outPointStyle == null)
			{
				outPointStyle = new GUIStyle();								
			}
			if (outPointStyle != null && outPointStyle.normal.background == null)
			{
				outPointStyle.normal.background = connectionBgNormal;
			}
			if (outPointStyle != null && outPointStyle.active.background == null)
			{
				outPointStyle.active.background = connectionBgActive;
			}

			if (style == null)
			{
				style = new GUIStyle();				
			}
			if (style != null && style.normal.background == null)
			{
				style.normal.background = nodeBgTexture;
			}

			if (selectedStyle == null)
			{
				selectedStyle = new GUIStyle();
			}
			if (selectedStyle != null && selectedStyle.normal.background == null)
			{
				selectedStyle.normal.background = nodeSelectedTexture;
			}

			if (titleStyle == null)
			{
				titleStyle = new GUIStyle();				
			}
			if (titleStyle != null && titleStyle.normal.background == null)
			{
				titleStyle.normal.background = nodeBgTitleTexture;
				titleStyle.normal.textColor = Color.white;
				titleStyle.padding.bottom = 5;
				titleStyle.fontStyle = FontStyle.Bold;
				titleStyle.alignment = TextAnchor.MiddleCenter;
			}
		}

		private void DrawBackground()
		{
			if (screenBackgroundTexture != null)
			{
				GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), screenBackgroundTexture, ScaleMode.StretchToFill);
			}
		}

		private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
		{
			int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
			int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

			Handles.BeginGUI();
			Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

			offset += drag * 0.5f;
			Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

			for (int i = 0; i < widthDivs; i++)
			{
				Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
			}

			for (int j = 0; j < heightDivs; j++)
			{
				Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
			}

			Handles.color = Color.white;
			Handles.EndGUI();
		}

		private void DrawNodes()
		{
			if (nodes != null)
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					nodes[i].Draw();
				}
			}
			else
			{
				Debug.Log("DrawNodes -> nodes list is null");
			}
		}

		private void DrawConnections()
		{
			if (connections != null)
			{
				for (int i = 0; i < connections.Count; i++)
				{
					connections[i].Draw();
				}
			}
		}

		private void ProcessEvents(Event e)
		{
			drag = Vector2.zero;

			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						ClearConnectionSelection();
					}

					if (e.button == 1)
					{
						ProcessContextMenu(e.mousePosition);
					}
					break;

				case EventType.MouseDrag:
					if (e.button == 0)
					{
						OnDrag(e.delta);
					}
					break;
			}
		}

		private void ProcessNodeEvents(Event e)
		{
			if (nodes != null)
			{
				for (int i = nodes.Count - 1; i >= 0; i--)
				{
					bool guiChanged = nodes[i].ProcessEvents(e);

					if (guiChanged)
					{
						GUI.changed = true;
					}
				}
			}
			else
			{
				Debug.Log("ProcessNodeEvents -> nodes list is null");
			}
		}

		private void DrawConnectionLine(Event e)
		{
			if (selectedInPoint != null && selectedOutPoint == null)
			{
				Handles.DrawBezier(
					selectedInPoint.rect.center,
					e.mousePosition,
					selectedInPoint.rect.center + Vector2.left * 50f,
					e.mousePosition - Vector2.left * 50f,
					bezierColor,
					null,
					2f
				);

				GUI.changed = true;
			}

			if (selectedOutPoint != null && selectedInPoint == null)
			{
				Handles.DrawBezier(
					selectedOutPoint.rect.center,
					e.mousePosition,
					selectedOutPoint.rect.center - Vector2.left * 50f,
					e.mousePosition + Vector2.left * 50f,
					bezierColor,
					null,
					2f
				);

				GUI.changed = true;
			}
		}

		private void ProcessContextMenu(Vector2 mousePosition)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
			genericMenu.ShowAsContext();
		}

		private void OnDrag(Vector2 delta)
		{
			drag = delta;

			if (nodes != null)
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					nodes[i].Drag(delta);
				}
			}

			GUI.changed = true;
		}

		private void OnClickAddNode(Vector2 mousePosition)
		{
			lastMousePosition = mousePosition;
			InputPopup.Show(DoOnInputPopupOkClicked);
		}

		private void DoOnInputPopupOkClicked(string inputText)
		{
			CreateCanvasEditorDirIfNonExistant();

			ScreenNode node = new ScreenNode(inputText, lastMousePosition, 200, 75, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode,
				style, selectedStyle, titleStyle, inPointStyle, outPointStyle);
			
			nodes?.Add(node);

			CreateNewSceneAndMoveToIt(inputText);
		}

		private void CreateCanvasEditorDirIfNonExistant()
		{
			string path = Application.dataPath + "/" + VISUAL_CANVAS_EDITOR_PATH;

			if (!Directory.Exists(path))
			{
				string guid = AssetDatabase.CreateFolder(ASSETS_PATH, VISUAL_CANVAS_EDITOR_PATH);
				AssetDatabase.GUIDToAssetPath(guid);
			}
		}

		private void CreateNewSceneAndMoveToIt(string sceneName)
		{
			Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

			if (newScene != null)
			{
				newScene.name = sceneName;

				string savigPath = string.Format("{0}/{1}/{2}", ASSETS_PATH, VISUAL_CANVAS_EDITOR_PATH, sceneName + CANVAS_CONTROLLER_SUFIX + SCENE_EXTENSION);
				EditorSceneManager.SaveScene(newScene, savigPath);

				//Create a canvas in the new Scene

				CanvasController canvasController = 
					new GameObject(sceneName + CANVAS_CONTROLLER_SUFIX, typeof(CanvasController)).GetComponent<CanvasController>();

				Canvas canvas = new GameObject("Canvas", typeof(Canvas)).GetComponent<Canvas>();
				canvas.transform.SetParent(canvasController.transform);

				canvasController.Canvas = canvas;

				GameObject camera = new GameObject("Camera", typeof(Camera));
			}

			GUIUtility.ExitGUI();
		}

		private void OnClickInPoint(ConnectionPoint inPoint)
		{
			selectedInPoint = inPoint;

			if (selectedOutPoint != null)
			{
				if (selectedOutPoint.node != selectedInPoint.node)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		private void OnClickOutPoint(ConnectionPoint outPoint)
		{
			selectedOutPoint = outPoint;

			if (selectedInPoint != null)
			{
				if (selectedOutPoint.node != selectedInPoint.node)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		private void OnClickRemoveNode(ScreenNode node)
		{
			if (connections != null)
			{
				List<Connection> connectionsToRemove = new List<Connection>();

				for (int i = 0; i < connections.Count; i++)
				{
					if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
					{
						connectionsToRemove.Add(connections[i]);
					}
				}

				for (int i = 0; i < connectionsToRemove.Count; i++)
				{
					connections.Remove(connectionsToRemove[i]);
				}

				connectionsToRemove = null;
			}

			nodes.Remove(node);
		}

		private void OnClickRemoveConnection(Connection connection)
		{
			connections.Remove(connection);
		}

		private void CreateConnection()
		{
			if (connections == null)
			{
				connections = new List<Connection>();
			}

			connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
		}

		private void ClearConnectionSelection()
		{
			selectedInPoint = null;
			selectedOutPoint = null;
		}
	}
}