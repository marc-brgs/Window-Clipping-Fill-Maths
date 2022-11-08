using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField] private GameObject polygon;
    [SerializeField] private GameObject window;

    private LineRenderer lrPolygon;
    private LineRenderer lrWindow;

    private bool drawingPolygon;
    private bool drawingWindow;
    private int polygonIndex;
    private int windowIndex;

    [SerializeField] private GameObject textPolygon;
    [SerializeField] private GameObject textWindow;
    
    // Start is called before the first frame update
    void Start()
    {
        lrPolygon = polygon.GetComponent<LineRenderer>();
        lrWindow = window.GetComponent<LineRenderer>();
        drawingPolygon = false;
        drawingWindow = true;
        polygonIndex = 0;
        windowIndex = 0;
        
        Debug.Log("(0,1) à (1,1)(1,-1) " + visible(new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, -1f, 0f)));
        Debug.Log("(3,1) à (1,1)(1,-1) " + visible(new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, -1f, 0f)));
        Debug.Log("(0,1) à (1,-1)(1,1) " + visible(new Vector3(0f, 1f, 0f), new Vector3(1f, -1f, 0f), new Vector3(1f, 1f, 0f)));
        Debug.Log("(3,1) à (1,-1)(1,1) " + visible(new Vector3(0f, 1f, 0f), new Vector3(1f, -1f, 0f), new Vector3(1f, 1f, 0f)));
    }
    
    // Update is called once per frame
    void Update()
    {
        if (drawingWindow)
            drawWindow();
        
        if (drawingPolygon)
            drawPolygon();

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            lrPolygon.SetPositions(Array.Empty<Vector3>());
            lrPolygon.positionCount = 0;
            polygonIndex = 0;
            lrWindow.SetPositions(Array.Empty<Vector3>());
            lrWindow.positionCount = 0;
            windowIndex = 0;
            drawingPolygon = false;
            drawingWindow = true;
            textPolygon.SetActive(false);
            textWindow.SetActive(true);
        }
    }

    void PrintMousePosition()
    {
        // Print 2D mouse position to world pos
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;
        Debug.Log(mouseWorldPosition);
    }

    public void ClipPolygon(int method) // 0 = cyrus beck | 1 = sutherland
    {
        if (drawingPolygon || drawingWindow) return; // Wait for fully drawn polygons
        Debug.Log("Clip");
        
        if(method == 0)
            CyrusBeck();
        else
            SutherlandHodgmann();
    }

    public void CyrusBeck()
    {
        
    }
    
    public void SutherlandHodgmann()
    {
        // Recover data
        int N1 = lrPolygon.positionCount;
        Vector3[] PL = new Vector3[N1];
        lrPolygon.GetPositions(PL);
        // Debug.Log(PL);
        int N3 = lrWindow.positionCount;
        Vector3[] PW = new Vector3[N3];
        lrWindow.GetPositions(PW);

        Vector3 S = new Vector3();
        Vector3 F = new Vector3();
        Vector3 I; // point d'intersection
        
        int N2;
        List<Vector3> PS;

        for (int i = 0; i < N3-1; i++) // i = 1 dans l'algo
        {
            N2 = 0;
            PS = new List<Vector3>();

            for (int j = 0; j < N1; j++) // j = 1 dans l'algo
            {
                if (j == 0)
                    F = PL[j]; /* Sauver le premier = dernier sommet */
                else
                {
                    if(coupe(S, PL[j], PW[i], PW[i+1]))
                    {
                        I = intersection(S, PL[j], PW[i], PW[i + 1]);
                        PS.Add(I);
                        N2++;
                    }
                }

                S = PL[j];
                if(visible(S, PW[i], PW[i+1])) {
                    PS.Add(S);
                    N2++;
                }
            }

            if (N2 > 0)
            {
                /* Traitement du dernier côté de PL */
                if(coupe(S, F, PW[i], PW[i+1]))
                {
                    I = intersection(S, F, PW[i], PW[i + 1]);
                    PS.Add(I);
                    N2++;
                }
                
                /* Découpage pour chacun des polygones */
                lrPolygon.positionCount = N2;
                Vector3[] ArrayPS = PS.ToArray();
                lrPolygon.SetPositions(ArrayPS);
                
                PL = ArrayPS;
                N1 = N2;
            }

            
        }
    }

    private bool coupe(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        Vector3 b = a2 - a1;
        Vector3 d = b2 - b1;
        float bDotDPerp = b.x * d.y - b.y * d.x;

        // if b dot d == 0, it means the lines are parallel so have infinite intersection points
        if (bDotDPerp == 0)
            return false;

        Vector3 c = b1 - a1;
        float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
        if (t < 0 || t > 1)
            return false;

        float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
        if (u < 0 || u > 1)
            return false;
        
        return true;
    }

    private Vector3 intersection(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        Vector3 intersection;

        Vector3 b = a2 - a1;
        Vector3 d = b2 - b1;
        float bDotDPerp = b.x * d.y - b.y * d.x;

        Vector3 c = b1 - a1;
        float t = (c.x * d.y - c.y * d.x) / bDotDPerp;

        intersection = a1 + (b * t);
        return intersection;
    }

    private bool visible(Vector3 S, Vector3 F1, Vector3 F2)
    {
        // Vector2 midF = new Vector2(F1.x + F2.x, F1.y+ F2.y) / 2;
        Vector2 midToS = new Vector2(S.x - F1.x, S.y - F1.y);
        
        Vector2 n = new Vector2(-(F2.y - F1.y), F2.x - F1.x);
        Vector2 m = -n;
        
        Debug.Log(n);
        Debug.Log(Vector2.Dot(n, midToS));
        
        if(Vector3.Dot(n, midToS) < 0) // dedans
            return true;
        if(Vector3.Dot(n, midToS) > 0) // dehors
            return false;
        // sur le bord de la fenêtre
        return true;
    }
    
    private void drawWindow()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            lrWindow.positionCount = windowIndex + 1;
            lrWindow.SetPosition(windowIndex, mouseWorldPosition);
            windowIndex++;
        }

        if (Input.GetMouseButtonDown(1))
        {
            lrWindow.positionCount = windowIndex + 1;
            lrWindow.SetPosition(windowIndex, lrWindow.GetPosition(0));
            windowIndex++;
            drawingWindow = false;
            drawingPolygon = true;
            textWindow.SetActive(false);
            textPolygon.SetActive(true);
        }
    }

    private void drawPolygon()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            lrPolygon.positionCount = polygonIndex + 1;
            lrPolygon.SetPosition(polygonIndex, mouseWorldPosition);
            polygonIndex++;
        }

        if (lrPolygon.positionCount != 0 && Input.GetMouseButtonDown(1))
        {
            lrPolygon.positionCount = polygonIndex + 1;
            lrPolygon.SetPosition(polygonIndex, lrPolygon.GetPosition(0));
            polygonIndex++;
            drawingPolygon = false;
            textPolygon.SetActive(false);
        }
    }
}
