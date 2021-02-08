using System;
using UnityEngine;

namespace VisualCanvasEditor
{
	public enum ConnectionPointType { In, Out }

	public class ConnectionPoint
	{
		public Rect rect;
		public ConnectionPointType type;
		public ScreenNode node;
		public GUIStyle style;

		public Action<ConnectionPoint> onClickConnectionPoint;

		private Texture2D img = Resources.Load("Icons/icon_connection_point") as Texture2D;

		public ConnectionPoint(ScreenNode node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> onClickConnectionPoint)
		{
			this.node = node;
			this.type = type;
			this.style = style;
			this.onClickConnectionPoint = onClickConnectionPoint;
			rect = new Rect(0, 0, 10f, 20f);
		}

		public void Draw()
		{
			rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

			switch (type)
			{
				case ConnectionPointType.In:
					rect.x = node.rect.x - rect.width;
					break;

				case ConnectionPointType.Out:
					rect.x = node.rect.x + node.rect.width;
					break;
			}

			if (GUI.Button(rect, string.Empty, style))
			{
				onClickConnectionPoint?.Invoke(this);
			}

			if (img != null)
			{
				GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 7.5f, 6, 6), img);
			}
		}
	}
}