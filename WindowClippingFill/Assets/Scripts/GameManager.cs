using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    
    [SerializeField] private Image img;

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
        
        img.gameObject.SetActive(false);
        clearTexture(img.sprite.texture);
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
            img.gameObject.SetActive(false);
            clearTexture(img.sprite.texture);
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
        
        // Debug.Log(n);
        // Debug.Log(Vector2.Dot(n, midToS));
        
        if(Vector3.Dot(n, midToS) < 0) // dedans
            return true;
        if(Vector3.Dot(n, midToS) > 0) // dehors
            return false;
        // sur le bord de la fenêtre
        return true;
    }
    
    // Remplissage RectEG
    public void remplissageRectEG()
    {
        // Recover data
        Vector3[] Poly = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(Poly);
        int nb = 0;
        Vector2[] rectEG = rectangleEnglobant(Poly);
        int xmin = (int) rectEG[0].x;
        int ymin = (int) rectEG[0].y;
        int xmax = (int) rectEG[1].x;
        int ymax = (int) rectEG[1].y;

        for (int x = xmin; x < xmax; x++)
        {
            for (int y = ymin; y < ymax; y++)
            {
                if (interieur(x, y, Poly))
                {
                    affichePixel(x, y);
                    nb++;
                }
            }
        }
        
        img.sprite.texture.Apply();
        img.gameObject.SetActive(true);
    }

    private void clearTexture(Texture2D tex)
    {
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
    }
    
    private Vector2[] rectangleEnglobant(Vector3[] Poly)
    {
        int xmin = 1920, xmax = 0, ymin = 1080, ymax = 0;
        
        for (int i = 0; i < Poly.Length; i++)
        {
            Vector2 polyPixel = new Vector2(worldPosXToPixel(Poly[i].x), worldPosYToPixel(Poly[i].y));
            Debug.Log("worldpos: " + Poly[i].x + " " + Poly[i].y + ", pixel: " + worldPosXToPixel(Poly[i].x) + " " + worldPosYToPixel(Poly[i].y));
            if (polyPixel.x < xmin)
              xmin = (int) polyPixel.x;
            if (polyPixel.x > xmax)
                xmax = (int) polyPixel.x;
            if (polyPixel.y < ymin)
                ymin = (int) polyPixel.y;
            if (polyPixel.y > ymax)
                ymax = (int) polyPixel.y;
        }
        
        Debug.Log("xmin: " + xmin + ", xmax: " + xmax + ", ymin: " + ymin + ", ymax: " + ymax);
        Vector2[] rectEG = new Vector2[2];
        rectEG[0] = new Vector2(xmin, ymin); // P1
        rectEG[1] = new Vector2(xmax, ymax); // P2
        return rectEG;
    }

    private bool interieur(int x, int y, Vector3[] poly)
    {
        // visible pour chaque côté
        for (int i = 0; i < poly.Length-1; i++)
        {
            if(!visible(new Vector3(x, y), new Vector3(worldPosXToPixel(poly[i].x), worldPosYToPixel(poly[i].y)), new Vector3(worldPosXToPixel(poly[i+1].x), worldPosYToPixel(poly[i+1].y)))) 
                return false;
        }
        
        return true;
    }

    private void affichePixel(int x, int y)
    {
        img.sprite.texture.SetPixel(x, y, Color.yellow);
    }
    
    private int worldPosXToPixel(float v)
    {
        return (int) (((v + 5.33) * 1920) / 10.66);
    }
    
    private int worldPosYToPixel(float v)
    {
        return (int) (((v + 3) * 1080) / 6);
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

        if (lrPolygon.positionCount > 1 && Input.GetMouseButtonDown(1))
        {
            lrPolygon.positionCount = polygonIndex + 1;
            lrPolygon.SetPosition(polygonIndex, lrPolygon.GetPosition(0));
            polygonIndex++;
            drawingPolygon = false;
            textPolygon.SetActive(false);
        }
    }
    
    public static void DrawLine(Vector2 p1, Vector2 p2, Color col)
    {
        Vector2 t = p1;
        float frac = 1/Mathf.Sqrt (Mathf.Pow (p2.x - p1.x, 2) + Mathf.Pow (p2.y - p1.y, 2));
        float ctr = 0;
     
        while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y) {
            t = Vector2.Lerp(p1, p2, ctr);
            ctr += frac;
            // img.sprite.texture.SetPixel((int)t.x, (int)t.y, col);
        }
    }

    public void drawShape()
    {
        for (int i = 0; i < lrPolygon.positionCount - 1; i++)
        {
            DrawLine( lrPolygon.GetPosition(i), lrPolygon.GetPosition(i+1), Color.blue);
        }
    }
}
