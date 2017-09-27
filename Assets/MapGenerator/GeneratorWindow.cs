﻿///-----------------------------------------------------------------
///   Class:          CaveMapGenerator
///   Description:    Unity window class, It arranges every button, sliders and displays needed for this tool to work properlly
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System;

namespace CaveMapGenerator
{
    /// <summary>
    /// This class is used by unity to generate the tool's window
    /// the main purpose of this class is to manage all interactive elements (buttons, sliders..)
    /// it also sends informations across other modules.
    /// </summary>
    public class GeneratorWindow : EditorWindow
    {
        /// <summary>
        /// This object process the buffers and generates a new cave
        /// </summary>
        public Generator Generator { get; private set; }

        /// <summary>
        /// This object exports the object into a new game object
        /// </summary>
        public Loader Loader { get; private set; }

        /// <summary>
        /// This object controls the display screen inside the window
        /// </summary>
        public Display Display { get; private set; }

        /// <summary>
        /// Variable used for the ui to hold important UI data
        /// </summary>
        private int
            newMapSize = 128,
            refinementSteps = 4;

        /// <summary>
        /// Variable used for the ui to hold important UI data
        /// </summary>
        private float
            initialDensity = 0.4f,
            minThreshold = 3.0f,
            maxThreshold = 4.0f;

        /// <summary>
        /// Base colors used for the UI
        /// </summary>
        private Color
            colorMapExportColor = new Color(090f / 255f, 176f / 255f, 131f / 255f),
            colorDisplaySetup   = new Color(237f / 255f, 118f / 255f, 072f / 255f),
            colorMapSetUpColor  = new Color(206f / 255f, 255f / 255f, 129f / 255f),
            colorMapGeneration  = new Color(255f / 255f, 192f / 255f, 071f / 255f);

        /// <summary>
        /// This method happens when the window is opened
        /// </summary>
        [MenuItem("Window/Cave Generator")]
        public static void ShowWindow()
        {
            GeneratorWindow window = (GeneratorWindow)EditorWindow.GetWindow(typeof(GeneratorWindow));
            if (window.Generator == null || window.Display == null || window.Loader == null)
                window.InitializeTool();

        }

        /// <summary>
        /// initialize every module nescessary for this tool
        /// </summary>
        private void InitializeTool()
        {
            wantsMouseMove = true;
            Generator = new Generator();
            Loader = new Loader();
            Display = new Display(512);
        }
       
        /// <summary>
        /// Draw a label
        /// </summary>
        /// <param name="color">label color</param>
        /// <param name="name">lavel tag</param>
        /// <param name="tooltip">label tooltip</param>
        private void DrawLabel(ref float heightPadding, Color color, string name, string tooltip)
        {
            heightPadding += 2;
            GUI.color = color;
            EditorGUI.LabelField(new Rect(8, heightPadding, position.width, 26), new GUIContent(name, tooltip));
            heightPadding += 1;
        }

        /// <summary>
        /// Draw the map display and hook the click events into it
        /// </summary>
        private void DrawMapDisplay(ref float heightPadding)
        {
            //move down a bit
            heightPadding += 26;

            //draw the display ui and return its port size
            Rect displayPort = Display.GUIDisplay(heightPadding, position.width);

            //if the user click the display
            if ((Event.current.type == EventType.MouseDown) &&
                (displayPort.Contains(Event.current.mousePosition)))
            {
                //get that cell he clicked in
                float ratio = Generator.Size / displayPort.width;
                int mouseX = Mathf.CeilToInt((Event.current.mousePosition.x - displayPort.x) * ratio);
                int mouseY = Mathf.CeilToInt((Event.current.mousePosition.y - displayPort.y) * ratio);

                //run the flood routine
                Generator.RemoveIsle(new Coordinate(Generator.Size - mouseY, mouseX));

                //update the display
                Display.UpdateDisplay(Generator.GetBufferCopy());

                //redraw the entire menu
                Repaint();
            }

            //move down until the end of the display
            heightPadding += position.width;
        }

        /// <summary>
        /// Draw a full sized button, with preset size especifications
        /// </summary>
        /// <param name="buttonColor">button's color</param>
        /// <param name="name">button's tag</param>
        /// <param name="tooltip">button's tooltip</param>
        /// <param name="buttonAction">button's action</param>
        private void DrawButton(ref float heightPadding, Color buttonColor, string name, string tooltip, Action buttonAction)
        {
            //displace down a bit
            heightPadding += 24;

            //paint the button with its preselected color
            GUI.color = buttonColor;

            //hook the interaction method
            if (GUI.Button(new Rect(1, heightPadding, position.width - 2, 26), new GUIContent(name, tooltip)))
                buttonAction();

            //displace down a bit further
            heightPadding += 4;
        }

        /// <summary>
        /// Draw an arraw of adjacent buttons
        /// </summary>
        /// <typeparam name="T">the type of info stored in each button</typeparam>
        /// <param name="name">the label name</param>
        /// <param name="tooltip">a tooltip</param>
        /// <param name="buttonColor">color for all the buttons</param>
        /// <param name="action">what those buttons do</param>
        /// <param name="values">the values stored inside those buttons</param>
        private void DrawButtonArray<T>(ref float heightPadding, string name, string tooltip, Color buttonColor, Action<T> action, params T[] values)
        {
            //Draw the label naming the button array
            GUI.color = Color.white;
            heightPadding += 1;
            EditorGUI.LabelField(
                new Rect(1, heightPadding, position.width - 3, 26),
                new GUIContent(name, tooltip));

            //draw the buttons
            GUI.color = buttonColor;

            //move down a bit
            heightPadding += 16;

            //find the real size of a button
            float buttonSize = position.width / values.Length;

            //for each options create a button
            for (int i = 0; i < values.Length; i++)

                //Hook the button events
                if (GUI.Button(new Rect(buttonSize * i + 1, heightPadding, buttonSize - 2, 26), new GUIContent(values[i].ToString(), tooltip)))
                    action(values[i]);

            //displace down a bit further
            heightPadding += 4;
        }

        /// <summary>
        /// Call the Threshold GUI slider function, and attach it to the variables of the generator class
        /// </summary>
        /// <param name="color">slider color</param>
        private void ThresholdSlider(ref float heightPadding, Color color)
        {
            //displace down a bit
            heightPadding += 17;

            //change color
            GUI.color = color;

            //hook the slider
            EditorGUI.MinMaxSlider(
                new GUIContent(
                    "Threshold",
                    "The minumum and the maximum amount of filled cells needed to change the state of the current cell"),
                new Rect(12, heightPadding, position.width - 40, 18),
                ref minThreshold, ref maxThreshold, 1, 6);
            heightPadding -= 3;
        }

        /// <summary>
        /// Call the Threshold GUI slider function, and attach it to the variables of the generator class
        /// </summary>
        /// <param name="color">slider color</param>
        private void DrawDualSlider(ref float heightPadding, string name, string tooltip, Color color, float limitMin, float limitMax, ref float min, ref float max)
        {
            //displace down a bit
            heightPadding += 17;

            //change color
            GUI.color = color;

            //hook the slider
            EditorGUI.MinMaxSlider(new GUIContent(name, tooltip),
                new Rect(12, heightPadding, position.width - 40, 18),
                ref min, ref max, limitMin, limitMax);
            heightPadding -= 3;
        }

        /// <summary>
        /// Draw a menu sized int slider 
        /// </summary>
        /// <param name="color">color of the slider</param>
        /// <param name="name">name of the slider's label</param>
        /// <param name="tooltip">slider's tooltip</param>
        /// <param name="variable">the variable atached to the slider</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        private void DrawSlider(ref float heightPadding, Color color, string name, string tooltip, ref int variable, int min, int max)
        {
            //displace down
            heightPadding += 24;

            //change colors
            GUI.color = color;

            //hook slider
            variable = EditorGUI.IntSlider(
                new Rect(12, heightPadding, position.width - 20, 18),
                new GUIContent(name, tooltip), variable, min, max);

            //displace down
            heightPadding -= 4;
        }

        /// <summary>
        /// Draw a menu sized float slider 
        /// </summary>
        /// <param name="color">color of the slider</param>
        /// <param name="name">name of the slider's label</param>
        /// <param name="tooltip">slider's tooltip</param>
        /// <param name="variable">the variable atached to the slider</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        private void DrawSlider(ref float heightPadding, Color color, string name, string tooltip, ref float variable, float min, float max)
        {
            //displace down
            heightPadding += 24;

            //change colors
            GUI.color = color;

            //hook slider
            variable = EditorGUI.Slider(
                new Rect(12, heightPadding, position.width - 20, 18),
                new GUIContent(name, tooltip), variable, min, max);

            //displace down
            heightPadding -= 4;
        }
        
        /// <summary>
        /// Base structural method, it holds the windows infos, buttons and layout
        /// </summary>
        private void OnGUI()
        {
            //if the game is playing ignore this block
            if (EditorApplication.isPlaying)
                return;

            //if its not playing and one of the main components are null, initialize the tool
            else if (Generator == null || Display == null || Loader == null)
                InitializeTool();

            //this variable is used to controll the y position of every element in this panel
            float heightDisplace = 0;

            //RESOLUTION
            //draw an array of buttons to display different resolution to the map display
            DrawButtonArray(ref heightDisplace, "Display Resolution", "Change the display image's resolution", colorDisplaySetup, (resolution) =>
            {
                Display.Resolution = resolution;
                Display.UpdateDisplay(Generator.GetBufferCopy());
            }, 256, 512, 1024);

            //GENERATE MAP
            //draw a button that will entirely generate a new map using default info
            DrawButton(ref heightDisplace, colorMapGeneration, "Automatically Generate Level", "Generate a new map, this map will suffer 4 iterations and will have its holes removed", () =>
            {
                Generator.GenerateMap(refinementSteps, minThreshold, maxThreshold, initialDensity, newMapSize);
                Display.UpdateDisplay(Generator.GetBufferCopy());
            });

            //MAP SIZE SLIDER
            //draw a slider that controlls thew size of the map
            DrawSlider(ref heightDisplace, colorMapGeneration, "Cave Map Size", "Change the size of the map that is going to be generated next", ref newMapSize, 15, 250);

            //NEW NOISE MAP
            //draw a button that generates a new noise map to be processed further
            DrawButton(ref heightDisplace, colorMapSetUpColor, "Generate New Noise Map", "Generates a brand new noise map to be further processed", () =>
            {
                Generator.NewMap(newMapSize, initialDensity);
                Display.UpdateDisplay(Generator.GetBufferCopy());
            });

            //MAP DENSITY
            //draw a slider that controlls the density of the noise map 
            DrawSlider(ref heightDisplace, colorMapSetUpColor, "Map Density", "Change the density of the map that will be generated next.", ref initialDensity, 0.0f, 1.0f);

            //MAP DISPLAY
            //draw a map display
            DrawMapDisplay(ref heightDisplace);

            //FILL LABEL
            //This label inform the user of the fill tool functionality
            DrawLabel(ref heightDisplace, Color.white, "Fill tool ON: Click on the map dislpay to fill", "Flood tool is turned on, a click on the map will fill holes with black ink");

            //THRESHOLDSLIDER
            //Draw a dual slider that controls the birth/death values
            ThresholdSlider(ref heightDisplace, colorMapSetUpColor);

            //ITERATE
            //Draw a button that will make the generator to iterate once
            DrawButton(ref heightDisplace, colorMapSetUpColor, "Iterate", "Force the celular automata to iterate once", () =>
            {
                Generator.Refine(minThreshold, maxThreshold);
                Display.UpdateDisplay(Generator.GetBufferCopy());
            });

            //FILL HOLES
            //Draw a button that will remove all holes from the displayed map
            DrawButton(ref heightDisplace, colorMapSetUpColor, "Fill Holes", "Remove every holes on the map", () =>
            {
                Generator.AutoIdentifyHoles();
                Display.UpdateDisplay(Generator.GetBufferCopy());
            });

            //EXPORT
            //Draw a button that will export the generated map into a scriptable object
            DrawButton(ref heightDisplace, colorMapExportColor, "Export", "Export the map into a scriptable object to be used later",
                () => Loader.Export(Generator.GetBufferCopy()));
        }

    }
}
#endif
