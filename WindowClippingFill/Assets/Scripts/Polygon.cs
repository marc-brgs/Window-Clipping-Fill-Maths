using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    private GameObject myObject;
    private LineRenderer lr;
    
    // Start is called before the first frame update
    void Start()
    {
        myObject = this.gameObject;
        myObject.name = "PolygonToCrop";
        
        //Components
        lr = myObject.GetComponent<LineRenderer>();
        
        // Position def examples
        lr.positionCount = 6;
        lr.SetPosition(0, new Vector2(0f, 0f));
        lr.SetPosition(1, new Vector2(0f, 1f));
        lr.SetPosition(2, new Vector2(1f, 1f));
        lr.SetPosition(3, new Vector2(3f, 2f));
        lr.SetPosition(4, new Vector2(3f, 0f));
        lr.SetPosition(5, new Vector2(0f, 0f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
