
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GridManager : MonoBehaviour
{
    [SerializeField] GameObject editorMenu;
    [SerializeField] TMP_InputField widthModifier;
    [SerializeField] TMP_InputField heightModifier;
    [SerializeField] GameObject gridDisplay;
    public static GridManager instance;
    [SerializeField] GameObject cellPrefab;
    CellScript[,] grid;
    [SerializeField] int width;
    [SerializeField] int height;
    public List<(int, int)> checkPoints = new List<(int, int)>();
    public (int, int) startingPoint;
    public (int, int) goalPoint;
    public List<PathingScript> robots;
    Vector3 defaultPosition;
    CellScript hoverCell;
    public bool runningSimulation = false;
    public bool simulating = false;
    bool selectingNewStart = false;
    bool selectingNewEnd = false;
    bool addingNewEndPoints = false;
    int wallState = 0;
    bool[,] map;
    (int, int)[] path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        instance = this;
    }
    void Start()
    {
        defaultPosition = transform.position;
        GenerateGrid();
    }
    void GenerateGrid()
    {
        checkPoints.Clear();
        map = new bool[width, height];
        transform.position = new Vector3(defaultPosition.x + 20 - width, defaultPosition.y + (20 % height), defaultPosition.z);
        if (grid != null)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Destroy(grid[i, j].gameObject);
                    grid[i, j] = null;
                }
            }
        }
        grid = new CellScript[width, height];
        startingPoint = (0, 0);
        goalPoint = (width - 1, height - 1);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 pos = new Vector3(transform.position.x + i, transform.position.y + j, transform.position.z);
                grid[i, j] = Instantiate(cellPrefab, pos, Quaternion.identity).GetComponent<CellScript>();
                grid[i, j].cellX = i;
                grid[i, j].cellY = j;
                if (i == startingPoint.Item1 && j == startingPoint.Item2)
                {
                    grid[i, j].SetStart();
                }
                else if (i == goalPoint.Item1 && j == goalPoint.Item2)
                {
                    grid[i, j].SetGoal();
                }
                grid[i, j].transform.SetParent(gridDisplay.transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!runningSimulation)
        {
            Vector2 mousePos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.BoxCast(mousePos, new Vector2(0.05f, 0.05f), 0, transform.up, float.MaxValue, LayerMask.GetMask("cell"));
            if (hit)
            {
                CellScript selectedCell = hit.collider.GetComponent<CellScript>();
                if (hoverCell != null)
                {
                    if (selectedCell != hoverCell)
                    {
                        hoverCell.UnHover();
                        hoverCell = selectedCell;
                    }
                }
                else
                {
                    hoverCell = selectedCell;
                }
                hoverCell.Hover();
                if (Input.GetMouseButtonDown(0))
                {
                    if (selectingNewStart)
                    {
                        grid[startingPoint.Item1, startingPoint.Item2].BackToDefault();
                        hoverCell.SetStart();
                        if (checkPoints.Contains((hoverCell.cellX, hoverCell.cellY)))
                        {
                            checkPoints.Remove((hoverCell.cellX, hoverCell.cellY));
                        }
                        startingPoint = (hoverCell.cellX, hoverCell.cellY);
                        hoverCell.UnHover();
                        UnselectOptions();
                    }
                    else if (selectingNewEnd)
                    {
                        grid[goalPoint.Item1, goalPoint.Item2].BackToDefault();
                        hoverCell.SetGoal();
                        if (checkPoints.Contains((hoverCell.cellX, hoverCell.cellY)))
                        {
                            checkPoints.Remove((hoverCell.cellX, hoverCell.cellY));
                        }
                        goalPoint = (hoverCell.cellX, hoverCell.cellY);
                        UnselectOptions();
                        hoverCell.UnHover();
                    }
                    else if (addingNewEndPoints)
                    {
                        if (hoverCell.SetCheckPoint())
                        {
                            hoverCell.ColorMe();
                            if (!checkPoints.Contains((hoverCell.cellX, hoverCell.cellY)))
                            {
                                checkPoints.Add((hoverCell.cellX, hoverCell.cellY));
                            }
                        }
                        hoverCell.UnHover();
                        UnselectOptions();
                    }
                    else
                    {
                        hoverCell.ChangeWall(0);
                        if (hoverCell.walkable)
                        {
                            wallState = 2;
                        }
                        else
                        {
                            wallState = 1;
                        }
                        hoverCell.UnHover();
                    }
                }
                if (Input.GetMouseButton(0))
                {
                    hoverCell.ChangeWall(wallState);
                    hoverCell.UnHover();
                }
            }
            else
            {
                if (hoverCell != null)
                {
                    hoverCell.UnHover();
                    hoverCell = null;
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CheckForWalkableMapUpdates();
                runningSimulation = true;
                for (int i = 0; i < robots.Count; i++)
                {
                    robots[i].SetPath(map, grid, startingPoint, goalPoint, checkPoints);
                }
                StartCoroutine(RunSimulation());
            }
            if (Input.GetMouseButtonDown(1))
            {
                UnselectOptions();
            }
        }
    }

    void CheckForWalkableMapUpdates()
    {
        map = new bool[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = grid[i, j].walkable;
            }
        }
    }
    void UnselectOptions()
    {
        foreach (Selectable selectableUI in Selectable.allSelectablesArray)
        {
            selectableUI.gameObject.SetActive(false);
            selectableUI.gameObject.SetActive(true);
        }
        selectingNewStart = false;
        selectingNewEnd = false;
        addingNewEndPoints = false;
    }
    IEnumerator RunSimulation()
    {
        editorMenu.SetActive(false);
        gridDisplay.SetActive(false);
        for (int i = 0; i < robots.Count; i++)
        {
            robots[i].SimulateRobot();
        }
        simulating = true;
        while (simulating)
        {
            int simulationsDone = 0;
            for (int i = 0; i < robots.Count; i++)
            {
                if (!robots[i].simulating)
                {
                    simulationsDone++;
                }
            }
            if (simulationsDone == robots.Count)
            {
                simulating = false;
            }
            yield return null;
        }
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        for (int i = 0; i < robots.Count; i++)
        {
            robots[i].DeleteCurrentGrid();
        }
        gridDisplay.SetActive(true);
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j].UnHover();
            }
        }
        runningSimulation = false;
        editorMenu.SetActive(true);
    }
    public void SetNewStart()
    {
        selectingNewStart = true;
    }
    public void SetNewGoal()
    {
        selectingNewEnd = true;
    }
    public void AddNewCheckPoint()
    {
        addingNewEndPoints = true;
    }
    public void ChangeValueOfHeight()
    {
        if (heightModifier.text.Length > 0)
        {
            height = int.Parse(heightModifier.text);
            height = Mathf.Clamp(height, 2, 20);
        }
        else
        {
            heightModifier.text = height.ToString();
        }
        
    }
    public void ChangeValueOfWidth()
    {
        if (widthModifier.text.Length > 0)
        {
            width = int.Parse(widthModifier.text);
            width = Mathf.Clamp(width, 2, 20);
            widthModifier.text = width.ToString();
        }
        else
        {
            widthModifier.text = width.ToString();
        }
    }
    public void ReMakeGrid()
    {
        GenerateGrid();
    }
}
