using System;
using UnityEditor;
using UnityEngine;

namespace VisualCanvasEditor
{
	public class Connection
	{
		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;
		public Action<Connection> onClickRemoveConnection;

		private Color color;

		public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> onClickRemoveConnection)
		{
			this.inPoint = inPoint;
			this.outPoint = outPoint;
			this.onClickRemoveConnection = onClickRemoveConnection;

			color = new Color(0.6f, 1f, 0);
		}

		public void Draw()
		{
			Handles.DrawBezier(
				inPoint.rect.center,
				outPoint.rect.center,
				inPoint.rect.center + Vector2.left * 50f,
				outPoint.rect.center - Vector2.left * 50f,
				color,
				null,
				4f
			);

			if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 5, 5, Handles.RectangleHandleCap))
			{
				onClickRemoveConnection?.Invoke(this);
			}
		}
	}
}