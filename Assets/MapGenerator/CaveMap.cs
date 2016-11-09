///-----------------------------------------------------------------
///   Class:          CaveMap
///   Description:    Scriptable Object holding a data structure containing a map.
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using UnityEngine;

/// <summary>
/// Data structure wich holds a map's data
/// </summary>
public class CaveMap : ScriptableObject
{
    /// <summary>
    /// The map information stored in this object
    /// the map is stored in a id array form due the fact that 2d arrays are not serealized in unity
    /// </summary>
    [SerializeField][HideInInspector]
    private bool[] map;

    /// <summary>
    /// The lateral size of the map
    /// </summary>
    [SerializeField]
    private int size;

    /// <summary>
    /// Getter to the size element;
    /// </summary>
    public int Size { get { return size; } }

    public bool[] Map { get { return map; } }

    /// <summary>
    /// Indexer that provides a better get/set to the map object
    /// it maps d1 arrays into 2d arrays
    /// </summary>
    public bool this[int x, int y] {
        get { return map[x * size + y];  }
        set { map[x * size + y] = value; }
    }

    /// <summary>
    /// Initialize a map objct
    /// </summary>
    /// <param name="map">the map stored here</param>
    public void InitializeMap(bool[,] map) {
        size = map.GetLength(0);
        this.map = new bool[size * size];
        Tools.Foreach2D(map, (int x, int y, ref bool cell) => this.map[x * size + y] = cell);
    }
}

