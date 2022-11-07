using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.TextCore.Text;
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

    public void ClipPolygon()
    {
        if (drawingPolygon || drawingWindow) return; // Wait for fully drawn polygons
        Debug.Log("Clip");
    }

    public void CyrusBeck()
    {
        
    }
    
    public void SutherlandHodgmann()
    {
        
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
