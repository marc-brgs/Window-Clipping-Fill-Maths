using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    private GameObject myObject;
    [SerializeField] private List<Vector2> nodePositions = new List<Vector2>();

    private MeshFilter mf;
    private MeshRenderer mr;
    
    // Start is called before the first frame update
    void Start()
    {
        myObject = this.gameObject;
        myObject.name = "PolygonToCrop";
        
        //Components
        mf = myObject.GetComponent<MeshFilter>();
        mr = myObject.GetComponent<MeshRenderer>();
        
        // Nodes def examples
        nodePositions.Add(new Vector2(0f, 0f));
        nodePositions.Add(new Vector2(0f, 1f));
        nodePositions.Add(new Vector2(1f, 1f));
        nodePositions.Add(new Vector2(3f, 2f));
        nodePositions.Add(new Vector2(3f, 0f));
        nodePositions.Add(new Vector2(0f, 0f));
    }

    // Update is called once per frame
    void Update()
    {
        updatePolygon();
    }
    
    void updatePolygon()
    {
        // Clear mesh
        myObject.GetComponent<MeshFilter>().mesh.Clear();
        
        // Create new mesh
        var mesh = new Mesh();
        mesh = createMesh();
            
        // Assign materials
        // mr.material = new Material();
            
        // Assign mesh to game object
        mf.mesh = mesh;
    }
    
    Mesh createMesh()
    {
        int x; //Counter
 
        //Create a new mesh
        var mesh = new Mesh();
        
        //Vertices
        var vertex = new Vector3[nodePositions.Count];
        Debug.Log(nodePositions.Count);
        for(x = 0; x < nodePositions.Count; x++)
        {
            vertex[x] = nodePositions[x];
        }
   
        //UVs
        var uvs = new Vector2[vertex.Length];
   
        for(x = 0; x < vertex.Length; x++)
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
        var tris = new int[3 * (vertex.Length - 2)];    //3 verts per triangle * num triangles
        int C1;
        int C2;
        int C3;
        
        C1 = 0;
        C2 = 1;
        C3 = 2;

        for (x = 0; x < tris.Length; x += 3)
        {
            tris[x] = C1;
            tris[x + 1] = C2;
            tris[x + 2] = C3;

            C2++;
            C3++;
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
        mesh.name = "CustomPolygonMesh";
        
        //Return the mesh
        return mesh;
    }
}
