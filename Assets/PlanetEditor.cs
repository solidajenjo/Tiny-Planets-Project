using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{

    Vector3 p1, p2;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Planet planet = (Planet)target;
        if (GUILayout.Button("Reset Planet"))
        {
            planet.Reset();
        }        

        if (GUILayout.Button("Get path"))
        {
            planet.GetPath(planet.begin, planet.end);
        }
    }

    

    private void OnSceneGUI()
    {        
        if (Event.current.type == EventType.MouseDown)
        {
            Debug.Log("HELLO");
            Planet planet = (Planet)target;
            planet.pathEnd = null;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                planet.p2 = hit.point;
            }
           
        }
    }

}
