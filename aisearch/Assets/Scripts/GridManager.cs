
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] GameObject gridDisplay;
    public static GridManager instance;
    [SerializeField] GameObject cellPrefab;
    CellScript[,] grid;
    [SerializeField] int width;
    [SerializeField] int height;
    public (int, int) startingPoint;
    public (int, int) goalPoint;
    public List<PathingScript> robots;
    CellScript hoverCell;
    bool runningSimulation = false;
    bool[,] map;
    (int, int)[] path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        instance = this;
    }
    void Start()
    {

        map = new bool[width, height];
        startingPoint = (0, 0);
        goalPoint = (9, 4);
        grid = new CellScript[width, height];
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
                    hoverCell.ChangeWall();
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
                for (int i = 0; i < robots.Count; i++)
                {
                    runningSimulation = true;
                    CheckForWalkableMapUpdates();
                    robots[i].SetPath(map, grid, startingPoint, goalPoint);
                }
                StartCoroutine(RunSimulation());
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
    IEnumerator RunSimulation()
    {
        gridDisplay.SetActive(false);
        for (int i = 0; i < robots.Count; i++)
        {
            robots[i].SimulateRobot();
        }
        bool simulating = true;
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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j].UnHover();
            }
        }
        runningSimulation = false;
    }
}
