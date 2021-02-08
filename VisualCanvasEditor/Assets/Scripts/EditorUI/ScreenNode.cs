using System;
using UnityEditor;
using UnityEngine;

namespace VisualCanvasEditor
{
	public class ScreenNode
	{
		public Rect rect;
		public string title;
		public bool isDragged;
		public bool isSelected;

		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;

		private GUIStyle style;
		private GUIStyle defaultNodeStyle;
		private GUIStyle selectedNodeStyle;
		private GUIStyle centeredStyle;
		private GUIStyle titleStyle;
		private GUIStyle inPointStyle;
		private GUIStyle outPointStyle;

		public Action<ScreenNode> onRemoveNode;

		public ScreenNode(string titleText, Vector2 position, float width, float height, 
			Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint, Action<ScreenNode> onClickRemoveNode, 
			GUIStyle style, GUIStyle selectedNodeStyle, GUIStyle titleStyle, 
			GUIStyle inPointStyle, GUIStyle outPointStyle)
		{
			this.style = style;
			this.defaultNodeStyle = style;
			this.selectedNodeStyle = selectedNodeStyle;
			this.titleStyle = titleStyle;
			this.inPointStyle = inPointStyle;
			this.outPointStyle = outPointStyle;

			title = titleText;
			rect = new Rect(position.x, position.y, width, height);
			inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
			outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
			onRemoveNode = onClickRemoveNode;
		}

		public void SetMainStyles(GUIStyle style, GUIStyle selectedNodeStyle, GUIStyle titleStyle)
		{
			this.style = style;
			this.titleStyle = titleStyle;
			this.selectedNodeStyle = selectedNodeStyle;
		}

		public void Drag(Vector2 delta)
		{
			rect.position += delta;
		}

		public void Draw()
		{
			inPoint.Draw();
			outPoint.Draw();

			if (selectedNodeStyle != null && isSelected)
			{
				GUI.Box(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), string.Empty, selectedNodeStyle);
			}

			if (style != null)
			{
				GUI.Box(rect, string.Empty, style);
			}

			if (titleStyle != null)
			{
				GUI.Box(new Rect(rect.x, rect.y, rect.width, 25), title, titleStyle);
			}
		}

		public bool ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						if (rect.Contains(e.mousePosition))
						{
							isDragged = true;
							GUI.changed = true;
							isSelected = true;
						}
						else
						{
							GUI.changed = true;
							isSelected = false;
						}
					}

					if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
					{
						ProcessContextMenu();
						e.Use();
					}
					break;

				case EventType.MouseUp:
					isDragged = false;
					break;

				case EventType.MouseDrag:
					if (e.button == 0 && isDragged)
					{
						Drag(e.delta);
						e.Use();
						return true;
					}
					break;
			}

			return false;
		}

		private void ProcessContextMenu()
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
			genericMenu.ShowAsContext();
		}

		private void OnClickRemoveNode()
		{
			onRemoveNode?.Invoke(this);
		}
	}
}