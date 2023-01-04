using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    private Color fillColor;

    // Start is called before the first frame update
    void Start()
    {
        lrPolygon = polygon.GetComponent<LineRenderer>();
        lrWindow = window.GetComponent<LineRenderer>();
        //lrPolygon.loop = true;
        //lrWindow.loop = true;
        drawingPolygon = false;
        drawingWindow = true;
        polygonIndex = 0;
        windowIndex = 0;
        
        img.gameObject.SetActive(false);
        clearTexture(img.sprite.texture);
        fillColor = Color.yellow;
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
    
    public void ClipPolygon(int method) // 0 = cyrus beck | 1 = sutherland
    {
        if (drawingPolygon || drawingWindow) return; // Wait for fully drawn polygons
        Debug.Log("Clip");
        
        if(method == 0)
            CyrusBeck();
        else
            SutherlandHodgmann();
        
        clearTexture(img.sprite.texture);
    }

    public void CyrusBeck()
    {
        // Recover data
        int N1 = lrPolygon.positionCount;
        Vector3[] Poly = new Vector3[N1];
        lrPolygon.GetPositions(Poly);
        
        int N3 = lrWindow.positionCount;
        Vector3[] Window = new Vector3[N3];
        lrWindow.GetPositions(Window);

        Vector3[] Normale = new Vector3[N3];
        Vector3[] Poly_copy = new Vector3[N1];

        int idx = 0;

        for(int i = 0; i < Window.Length-1; i++)
        {
            Normale[i] = new Vector3(Window[i + 1][1] - Window[i][1], -(Window[i + 1][0] - Window[i][0]), 0);
        }

        float X1, Y1, X2, Y2, t, DX, DY, WN, DN;
        int Nbsom = Window.Length-1;
        Vector3 C;

        for (var i = 0; i < Poly.Length - 1; i++)
        {
            float tinf = Single.MinValue, tsup = Single.MaxValue;

            X1 = Poly[i][0];
            Y1 = Poly[i][1];
            X2 = Poly[i+1][0];
            Y2 = Poly[i+1][1];

            DX = X2 - X1;
            DY = Y2 - Y1;
            for (int j = 0; j < Nbsom; j++)
            {
                C = Window[j];
                DN = DX * Normale[j][0] + DY * Normale[j][1];
                WN = (X1 - C[0]) * Normale[j][0] + (Y1 - C[1]) * Normale[j][1];

                if (DN == 0)
                {
                    return;
                }
                else
                {
                    t = -WN / DN;
                    if (DN > 0)
                    {
                        if (t > tinf)
                            tinf = t;
                    }
                    else
                    {
                        if (t < tsup)
                            tsup = t;
                    }
                }
            }

            if (tinf < tsup)
            {
                if (tinf < 0)
                {
                    tinf = 0;
                }
                else
                {
                    if (tsup > 1)
                    {
                        tsup = 1;
                    }
                }

                X2 = X1 + DX * tsup;
                Y2 = Y1 + DY * tsup;
                X1 += DX * tinf;
                Y1 += DY * tinf;

                Poly_copy[idx] = new Vector3(X1, Y1, 0);
                idx++;
                Poly_copy[idx] = new Vector3(X2, Y2, 0);
                idx++;
            }

            idx += 2;
        }
        lrPolygon.SetPositions(Poly_copy);
    }
    
    /**
     * Clip polygon to window (polygon must be convex and drawn clockwise)
     */
    public void SutherlandHodgmann()
    {
        // Recover data
        int N1 = lrPolygon.positionCount;
        Vector3[] PL = new Vector3[N1];
        lrPolygon.GetPositions(PL);
        
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
                
                /* Ferme le polygone si non fermé */
                if(ArrayPS[0] != ArrayPS[ArrayPS.Length - 1])
                {
                    lrPolygon.positionCount++;
                    lrPolygon.SetPosition(lrPolygon.positionCount-1, lrPolygon.GetPosition(0));
                }
                
                PL = ArrayPS;
                N1 = N2;
            }

            
        }
    }

    /**
     * Determine if 2 sides are intersecting
     * Used by Sutherland algorithm
     */
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

    /**
     * Return the intersection point of 2 sides
     * Used by Sutherland algorithm
     */
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
    
    /*
     * Determine if a point is inside of a side of polygon with clockwise normal
     * Used by Sutherland and RemplissageRectEG algorithms
     */
    private bool visible(Vector3 S, Vector3 F1, Vector3 F2)
    {
        Vector2 midToS = new Vector2(S.x - F1.x, S.y - F1.y);
        
        Vector2 n = new Vector2(-(F2.y - F1.y), F2.x - F1.x);
        Vector2 m = -n;
        
        if(Vector3.Dot(n, midToS) < 0) // dedans
            return true;
        if(Vector3.Dot(n, midToS) > 0) // dehors
            return false;
        // sur le bord de la fenêtre
        return true;
    }
    
    /*
     * Reset sprite texture used for filling to transparent
     */
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
    
    public void ChangeColor(int color=0)
    {
        switch (color)
        {
            case 0:
                fillColor = Color.yellow;
                break;
            case 1:
                fillColor = Color.magenta;
                break;
            case 2:
                fillColor = Color.green;
                break;
            case 3:
                fillColor = Color.cyan;
                break;
        }
        RemplissageRectEG();
    }
    
    // Remplissage RectEG
    public void RemplissageRectEG()
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

    /*
     * Used by RemplissageRectEG to determine x y min max for pixel loop optimization
     */
    private Vector2[] rectangleEnglobant(Vector3[] Poly)
    {
        int xmin = Screen.width, xmax = 0, ymin = Screen.height, ymax = 0;
        
        for (int i = 0; i < Poly.Length; i++)
        {
            Vector2 polyPixel = new Vector2(worldPosXToPixel(Poly[i].x), worldPosYToPixel(Poly[i].y));
            //Debug.Log("worldpos: " + Poly[i].x + " " + Poly[i].y + ", pixel: " + worldPosXToPixel(Poly[i].x) + " " + worldPosYToPixel(Poly[i].y));
            if (polyPixel.x < xmin)
              xmin = (int) polyPixel.x;
            if (polyPixel.x > xmax)
                xmax = (int) polyPixel.x;
            if (polyPixel.y < ymin)
                ymin = (int) polyPixel.y;
            if (polyPixel.y > ymax)
                ymax = (int) polyPixel.y;
        }
        
        //Debug.Log("xmin: " + xmin + ", xmax: " + xmax + ", ymin: " + ymin + ", ymax: " + ymax);
        Vector2[] rectEG = new Vector2[2];
        rectEG[0] = new Vector2(xmin, ymin); // P1
        rectEG[1] = new Vector2(xmax, ymax); // P2
        return rectEG;
    }

    /*
     * Used by RemplissageRectEG to determine if a point is inside polygon
     */
    private bool interieur(int x, int y, Vector3[] poly)
    {
        // Only working for convex polygons
        for (int i = 0; i < poly.Length-1; i++)
        {
            if(!visible(new Vector3(x, y), new Vector3(worldPosXToPixel(poly[i].x), worldPosYToPixel(poly[i].y)), new Vector3(worldPosXToPixel(poly[i+1].x), worldPosYToPixel(poly[i+1].y)))) 
                return false;
        }
        
        return true;
    }

    /*
     * Change pixel color of sprite texture used for filling display
     */
    private void affichePixel(int x, int y)
    {
        img.sprite.texture.SetPixel(x, y, fillColor);
    }
    
    /*
     * Convert world position x axis value to pixel
     */
    private int worldPosXToPixel(float v)
    {
        return (int) (((v + 5.33) * Screen.width) / 10.66);
    }
    
    /*
     * Convert world position y axis value to pixel
     */
    private int worldPosYToPixel(float v)
    {
        return (int) (((v + 3) * Screen.height) / 6);
    }
    
    /*
     * Click actions event for drawing window
     */
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

    /*
     * Click actions event for drawing polygon
     */
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

    /**
     * Remplissage par ligne
     */
    public void RemplissageLigne()
    {
        // Liste des points du polygone
        Vector3[] points = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(points);

        // Trier les points par angle polaire croissant
        points = SortPointsByPolarAngle(points);

        // Tracer une ligne entre chaque paire de points consécutifs
        for (int i = 0; i < points.Length - 1; i++)
        {
            DrawLine(points[i], points[i + 1], Color.red);
        }

        // Tracer une ligne entre le dernier et le premier point pour fermer le polygone
        DrawLine(points[points.Length - 1], points[0], Color.red);
    }

    public void RemplissageLigne2()
    {
        Vector3[] points = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(points);
        points = SortPointsByPolarAngle(points);

        // Inverser l'ordre des points
        Array.Reverse(points);

        // Nombre de lignes parallèles à tracer
        int numLines = 5;

        // Distance entre chaque ligne parallèle
        float lineSpacing = 0.1f;

        // Tracer les lignes parallèles
        for (int i = 0; i < numLines; i++)
        {
            // Calculer la distance entre le bord du polygone et la ligne parallèle
            float distance = lineSpacing * (i + 1);

            // Tracer une ligne parallèle pour chaque point du bord du polygone
            for (int j = 0; j < points.Length - 1; j++)
            {
                // Calculer le vecteur normal au bord du polygone
                Vector3 normal = Vector3.Cross(points[j + 1] - points[j], Vector3.forward).normalized;

                // Calculer les points de départ et d'arrivée de la ligne parallèle
                Vector3 start = points[j] + normal * distance;
                Vector3 end = points[j + 1] + normal * distance;

                // Vérifier si la ligne intersecte un bord du polygone
                Vector3 intersection = GetLineIntersection(start, end, points[j], points[j + 1]);
                if (intersection != Vector3.zero)
                {
                    // La ligne intersecte un bord du polygone, mettre à jour l'extrémité de la ligne
                    end = intersection;
                }

                // Tracer la ligne parallèle
                DrawLine(start, end, Color.red);
            }
        }
    }
    
    Vector3 GetLineIntersection(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
    {
        Vector3 intersection = Vector3.zero;

        // Calculer les vecteurs s et t
        Vector3 s = line1End - line1Start;
        Vector3 t = line2End - line2Start;

        // Calculer la valeur de u
        float u = (-t.y * s.x + s.y * t.x) / (-t.x * s.y + s.x * t.y);

        // Vérifier si les lignes s'intersectent
        if (u >= 0 && u <= 1)
        {
            // Calculer l'intersection
            intersection = line1Start + u * s;
        }

        return intersection;
    }

    
    public void RemplissageLigne3()
    {
        Vector3[] points = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(points);
        points = SortPointsByPolarAngle(points);
        
        // Créer une RenderTexture temporaire pour stocker le rendu de la caméra
        RenderTexture tempRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        mainCamera.targetTexture = tempRenderTexture;
        mainCamera.Render();

        // Créer un texture2D vide pour stocker les pixels lus depuis la RenderTexture
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);

        // Lire les pixels de la RenderTexture dans le texture2D
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        texture.ReadPixels(rect, (int)points[0].x, (int)points[0].y);
        texture.Apply();
        
        Color CC = Color.red, CR = Color.blue;
        int x = 0, y = 0;
        // Couleur pixel courant, droite et gauche
        Color CP, CPd, CPg;

        // Pile pour stocker les germes
        Stack<Vector2Int> p = new Stack<Vector2Int>();

        // Abscisses extrêmes droite et gauche de la ligne de balayage
        int xd, xg;

        // Empiler le germe (x,y)
        p.Push(new Vector2Int(x, y));

        // Tant qu'il y a des germes à traiter
        while (p.Count > 0)
        {
            // Récupérer le sommet de la pile
            Vector2Int point = p.Peek();
            x = point.x;
            y = point.y;

            // Dépiler le germe
            p.Pop();

            // Récupérer la couleur du pixel courant
            CP = texture.GetPixel(x, y);

            // Rechercher xd : extrême à droite
            xd = x + 1;
            CPd = texture.GetPixel(xd, y);
            while (CPd != CC && xd < texture.width)
            {
                xd++;
                CPd = texture.GetPixel(xd, y);
            }
            xd--;

            // Rechercher xg : extrême à gauche
            xg = x - 1;
            CPg = CP;
            while (CPg != CC && xg >= 0)
            {
                xg--;
                CPg = texture.GetPixel(xg, y);
            }
            xg++;

            // Tracer la ligne de balayage de xg à xd avec la couleur CR
            DrawLine(new Vector3(xg, y, 0), new Vector3(xd, y, 0), CR);

            // Rechercher de nouveaux germes sur la ligne de balayage au-dessus
            x = xd;
            CP = texture.GetPixel(x, y + 1);
            while (x > xg)
            {
                while (((CP == CC) || (CP == CR)) && (x > xg))
                {
                    x--;
                    CP = texture.GetPixel(x, y + 1);
                }
                if ((x > xg) && (CP != CC) && (CP != CR))
                {
                    // Empiler le nouveau germe au-dessus trouvé
                    p.Push(new Vector2Int(x, y + 1));
                }
                while ((CP != CC) && (x > xg))
                {
                    x--;
                    CP = texture.GetPixel(x, y + 1);
                }
            }

            // Rechercher de nouveaux germes sur la ligne de balayage au-dessous
            x = xd;
            CP = texture.GetPixel(x, y - 1);
            while (x > xg)
            {
                while (CP == CC)
                {
                    while ((CP != CC) && (x > xg))
                    {
                        x--;
                        CP = texture.GetPixel(x, y - 1);
                    }
                }
            }
        }
    }



    
    /**
     * Tri les points par angle polaire
     */
    Vector3[] SortPointsByPolarAngle(Vector3[] points)
    {
        // Trouver le point le plus à gauche (avec le plus petit abscisse)
        int minXIndex = 0;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].x < points[minXIndex].x)
            {
                minXIndex = i;
            }
        }

        // Déplacer le point le plus à gauche en tête de liste
        (points[0], points[minXIndex]) = (points[minXIndex], points[0]);

        // Trier les points restants par angle polaire croissant
        Array.Sort(points, 1, points.Length - 1, new PolarAngleComparer(points[0]));

        return points;
    }
    
    /**
     * Remplir une ligne
     */
    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        // Créer un nouvel objet "Line"
        GameObject line = new GameObject("Line");

        // Ajouter un composant LineRenderer à l'objet
        LineRenderer lr = line.AddComponent<LineRenderer>();

        // Configurer le LineRenderer
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}
