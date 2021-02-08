using UnityEngine;

namespace VisualCanvasEditor
{
	public class TextureUtils
	{
		public static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];

			if (pix != null)
			{
				for (int i = 0; i < pix.Length; ++i)
				{
					pix[i] = col;
				}
			}

			Texture2D result = new Texture2D(width, height);

			result?.SetPixels(pix);
			result?.Apply();

			return result;
		}
	}
}