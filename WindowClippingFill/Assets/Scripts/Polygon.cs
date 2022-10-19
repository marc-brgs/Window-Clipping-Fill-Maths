using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    [SerializeField]private GameObject[] myObject;
    [SerializeField] private Vector2[] nodePositions;
        
    // Start is called before the first frame update
    void Start()
    {
        nodePositions[0] = new Vector2(0f, 0f);
        nodePositions[1] = new Vector2(0f, 1f);
        nodePositions[2] = new Vector2(1f, 1f);
        nodePositions[3] = new Vector2(0f,0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void updatePolygon()
    {
        for(int x = 0; x < 2; x++)
        {
            //Destroy old game object
            Destroy(myObject[x]);
 
            //New mesh and game object
            myObject[x] = new GameObject();
            myObject[x].name = "MousePolygon";
            var mesh = new Mesh();
       
            //Components
            var mf = myObject[x].AddComponent<MeshFilter>();
            var mr = myObject[x].AddComponent<MeshRenderer>();
            //myObject[x].AddComponent();
       
            //Create mesh
            // mesh = createMesh(x);
            
            //Assign materials
            // mr.material = new Material();
            
            //Assign mesh to game object
            mf.mesh = mesh;
        }
    }
    
    /*Mesh createMesh(int num)
    {
        int x; //Counter
 
        //Create a new mesh
        var mesh = new Mesh();
        
        //Vertices
        var vertex = new Vector3[nodePositions.length];
   
        for(x = 0; x < nodePositions.length; x++)
        {
            vertex[x] = nodePositions[x];
        }
   
        //UVs
        var uvs = new Vector2[vertex.length];
   
        for(x = 0; x < vertex.length; x++)
        {
            if((x%2) == 0)
            {
                uvs[x] = new Vector2(0,0);
            }
            else
            {
                uvs[x] = new Vector2(1,1);
            }
        }
   
        //Triangles
        var tris = new int[3 * (vertex.length - 2)];    //3 verts per triangle * num triangles
        int C1;
        int C2;
        int C3;
   
        if(num == 0)
        {
            C1 = 0;
            C2 = 1;
            C3 = 2;
       
            for(x = 0; x < tris.length; x+=3)
            {
                tris[x] = C1;
                tris[x+1] = C2;
                tris[x+2] = C3;
           
                C2++;
                C3++;
            }
        }
        else
        {
            C1 = 0;
            C2 = vertex.length - 1;
            C3 = vertex.length - 2;
       
            for(x = 0; x < tris.length; x+=3)
            {
                tris[x] = C1;
                tris[x+1] = C2;
                tris[x+2] = C3;
           
                C2--;
                C3--;
            }  
        }
   
        //Assign data to mesh
        mesh.vertices = vertex;
        mesh.uv = uvs;
        mesh.triangles = tris;
   
        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();  
        mesh.Optimize();
   
        //Name the mesh
        mesh.name = "MyMesh";
   
        //Return the mesh
        return mesh;
    }*/
}
