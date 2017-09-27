///-----------------------------------------------------------------
///   Class:          Display
///   Description:    Map editor display class, it generates
///                   a texture holding a map visualisation.
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace CaveMapGenerator {
	public class Display {
		/// <summary>
		/// The image stored inside this display
		/// </summary>
		public Texture2D Image { get; set; }

		/// <summary>
		/// Local color buffer
		/// </summary>
		private Color[] textureColors;

		/// <summary>
		/// The actual resolution of the display
		/// </summary>
		public int Resolution {
			get { return Image.width; }
			set {
				textureColors = new Color[value * value];
				Image = new Texture2D(value, value);
			}
		}

		/// <summary>
		/// Set the pixels at the display and summit them
		/// </summary>
		private void FlushTexture() {
			//set the buffer to the texture
			Image.SetPixels(0, 0, Resolution, Resolution, textureColors);

			//apply changes onto the texture
			Image.Apply();
		}

		/// <summary>
		/// This sets the display texture to full white, its used for initialization
		/// </summary>
		private void SetTextureToWhite() {
			for ( int i = 0; i < textureColors.Length; i++ )
				textureColors[i] = Color.white;

			FlushTexture();
		}

		public void UpdateDisplay( bool[,] map ) {
			float ratio = map.GetLength(0) / (float)Resolution;
			Tools.Foreach2D(textureColors, Resolution, ( int x, int y, ref Color color ) =>
			    color = ( map[Mathf.FloorToInt(ratio * x), Mathf.FloorToInt(ratio * y)] ) ?
				  Color.black : Color.white
			    );

			FlushTexture();
		}

		public void UpdateDisplay( CaveMap map ) {
			float ratio = map.Size / (float)Resolution;
			Tools.Foreach2D(textureColors, Resolution, ( int x, int y, ref Color color ) =>
			    color = ( map[Mathf.FloorToInt(ratio * x), Mathf.FloorToInt(ratio * y)] ) ?
				  Color.black : Color.white
			    );

			FlushTexture();
		}

		/// <summary>
		/// Initialize the display
		/// </summary>
		/// <param name="resolution">the initial resolution the display is going to use</param>
		public Display( int resolution ) {
			//set the resolution variable
			Resolution = resolution;

			//initialize the texture to white
			SetTextureToWhite();
		}

		public Rect GUIDisplay( float height, float width ) {
			//set display color to white
			GUI.color = Color.white;

			//the display port properties
			Rect displayPort = new Rect(1, height, width, width);

			//draw the texture
			EditorGUI.DrawPreviewTexture(displayPort, Image, null, ScaleMode.ScaleToFit, 1.0f);

			return displayPort;
		}

	}
}