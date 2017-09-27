using UnityEngine;
using System.Collections;

/// <summary>
/// This object just displays some gizmos around to test if the scriptable object is working
/// </summary>
public class MapTestObject : MonoBehaviour {

    public CaveMap loadedMap;

    void Start()
    {
        Debug.Log(loadedMap.Size);
    }

    void OnDrawGizmos()
    {
        if(loadedMap != null)
            Tools.Foreach2D(loadedMap.Map, loadedMap.Size,(int x, int y, ref bool cell) => {
                if(cell)
                    Gizmos.DrawCube(new Vector3(x, 0.0f, y), Vector3.one);
            });
    }

}
