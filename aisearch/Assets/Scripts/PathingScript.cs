using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PathingScript : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject indSummary;
    [SerializeField] string algorithmName;
    [SerializeField] GameObject cell;
    (int, int)[] path = new (int, int)[0];
    [SerializeField] int pathingType = 0;
    List<(int, int)> nodesExplored = new List<(int, int)>();
    List<(int, int)> checkPoints = new List<(int, int)>();
    List<(List<(int, int)>, (int, int)[])> orderedPaths = new List<(List<(int, int)>, (int, int)[])>();
    CellScript[,] copiedGrid = null;
    public bool simulating = false;
    int pathSize = 0;
    int nodesVisited = 0;
    double calculatedTime = 0;
    SummaryScript child = null;
    Stopwatch stopwatch = new Stopwatch();
    Vector3 originalPos;
    GridManager gm;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stopwatch.Start();
        originalPos = transform.position;
        gm = GridManager.instance;
        gm.robots.Add(this);
        stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.Elapsed.Milliseconds);
    }

    // Update is called once per frame
    private void Update()
    {
        if (child != null)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                child.gameObject.SetActive(!child.gameObject.activeSelf);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Destroy(child.gameObject);
                child = null;
            }
        }
    }
    public void SetPath(bool[,] walkableMap, CellScript[,] originalGrid, (int, int) startingPos, (int, int) endPoint, List<(int, int)> mandatoryCheckpoints)
    {
        foreach ((int, int) checkPoint in mandatoryCheckpoints)
        {
            checkPoints.Add(checkPoint);
        }
        simulating = true;
        CreateGridFromTemplate(walkableMap, originalGrid);
        FindPath(walkableMap, originalGrid, startingPos, endPoint);
    }
    public void SimulateRobot()
    {
        StartCoroutine(FindTheSteps());
    }

    void CreateGridFromTemplate(bool[,] walkableMap, CellScript[,] template)
    {
        transform.position = new Vector3(originalPos.x + 20 - template.GetLength(0), originalPos.y + (20 % template.GetLength(1)), originalPos.z);
        copiedGrid = new CellScript[walkableMap.GetLength(0), walkableMap.GetLength(1)];
        for (int i = 0; i < copiedGrid.GetLength(0); i++)
        {
            for (int j = 0; j < copiedGrid.GetLength(1); j++)
            {
                Vector3 newPos = new Vector3(transform.position.x + i, transform.position.y + j, transform.position.z);
                copiedGrid[i, j] = Instantiate(cell, newPos, Quaternion.identity).GetComponent<CellScript>();
                copiedGrid[i, j].Copy(template[i, j]);
            }
        }
    }
    public void DeleteCurrentGrid()
    {
        for (int i = 0; i < copiedGrid.GetLength(0); i++)
        {
            for (int j = 0; j < copiedGrid.GetLength(1); j++)
            {
                Destroy(copiedGrid[i, j].gameObject);
                copiedGrid[i, j] = null;
            }
        }
    }
    void FindPath(bool[,] walkableMap, CellScript[,] originalGrid, (int, int) startingPos, (int, int) endPoint)
    {
        stopwatch = Stopwatch.StartNew();
        int checks = 0;
        orderedPaths = new List<(List<(int, int)>, (int, int)[])>();
        (int, int) temporaryStarter = startingPos;
        while (checkPoints.Count > 0 && checks < checkPoints.Count+1)
        {
            checks++;
            (int, int)[] tempPath = new (int, int)[1] { (-1, -1) };
            (int, int) possibleNewStart = (-1, -1);
            List<(int, int)> nodesExploredInThisSection = new List<(int, int)>();
            for (int i = 0; i < checkPoints.Count; i++)
            {
                (int, int)[] newPath = new (int, int)[1] { (-1, -1) };
                if (pathingType == 0)
                {
                    (nodesExploredInThisSection, newPath) = AISearch.BreadthFirstSearch(temporaryStarter.Item1, temporaryStarter.Item2, checkPoints[i].Item1, checkPoints[i].Item2, walkableMap);
                }
                else if (pathingType == 1)
                {
                    (nodesExploredInThisSection, newPath) = AISearch.NRDepthFirstSearch(temporaryStarter, checkPoints[i], walkableMap);
                }
                else if (pathingType == 2)
                {
                    (nodesExploredInThisSection, newPath) = AISearch.GreedySearch(temporaryStarter, checkPoints[i], walkableMap);
                }
                else if (pathingType == 3)
                {
                    (nodesExploredInThisSection, newPath) = AISearch.AStar(temporaryStarter, checkPoints[i], walkableMap);
                }
                else if (pathingType == 4)
                {
                    (nodesExploredInThisSection, newPath) = AISearch.RandomSearch(temporaryStarter, checkPoints[i], walkableMap);
                }
                if (tempPath != null && tempPath[0] != (-1,-1))
                {
                    if (tempPath.Length > newPath.Length && newPath[0] != (-1, -1))
                    {
                        checks = 0;
                        nodesExplored = nodesExploredInThisSection;
                        possibleNewStart = checkPoints[i];
                        tempPath = newPath;
                    }
                }
                else
                {
                    if (newPath[0] != (-1, -1))
                    {
                        checks = 0;
                        tempPath = newPath;
                        nodesExplored = nodesExploredInThisSection;
                        possibleNewStart = checkPoints[i];
                    }
                }
            }
            if (tempPath?[0] != (-1, -1))
            {
                orderedPaths.Add((nodesExplored, tempPath));
            }
            if (possibleNewStart != (-1, -1))
            {
                temporaryStarter = possibleNewStart;
            }
            if (checkPoints.Contains(temporaryStarter))
            {
                checkPoints.Remove(temporaryStarter);
            }
            if (checkPoints.Count == 0)
            {
                if (pathingType == 0)
                {
                    orderedPaths.Add(AISearch.BreadthFirstSearch(temporaryStarter.Item1, temporaryStarter.Item2, endPoint.Item1, endPoint.Item2, walkableMap));
                }
                else if (pathingType == 1)
                {
                    orderedPaths.Add(AISearch.NRDepthFirstSearch(temporaryStarter, endPoint, walkableMap));
                }
                else if (pathingType == 2)
                {
                    orderedPaths.Add(AISearch.GreedySearch(temporaryStarter, endPoint, walkableMap));
                }
                else if (pathingType == 3)
                {
                    orderedPaths.Add(AISearch.AStar(temporaryStarter, endPoint, walkableMap));
                }
                else if (pathingType == 4)
                {
                    orderedPaths.Add(AISearch.RandomSearch(temporaryStarter, endPoint, walkableMap));
                }
            }
            if (checks == checkPoints.Count && tempPath[0] == (-1, -1)) 
            {
                orderedPaths.Add((nodesExploredInThisSection, tempPath));
            }
        }
        if (orderedPaths.Count == 0)
        {
            if (pathingType == 0)
            {
                orderedPaths.Add(AISearch.BreadthFirstSearch(startingPos.Item1, startingPos.Item2, endPoint.Item1, endPoint.Item2, walkableMap));
            }
            else if (pathingType == 1)
            {
                orderedPaths.Add(AISearch.NRDepthFirstSearch(startingPos, endPoint, walkableMap));
            }
            else if (pathingType == 2)
            {
                orderedPaths.Add(AISearch.GreedySearch(startingPos, endPoint, walkableMap));
            }
            else if (pathingType == 3)
            {
                orderedPaths.Add(AISearch.AStar(startingPos, endPoint, walkableMap));
            }
            else if (pathingType == 4)
            {
                orderedPaths.Add(AISearch.RandomSearch(startingPos, endPoint, walkableMap));
            }
        }
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        calculatedTime = ts.Seconds + (ts.TotalMilliseconds / 10d);

        UnityEngine.Debug.Log(calculatedTime.ToString());
        stopwatch.Reset();
    }
    IEnumerator FindTheSteps()
    {
        pathSize = 0;
        nodesVisited = 0;
        int checkPointVisited = 1;
        for (int k = 0; k < orderedPaths.Count; k++)
        {
            for (int i = 0; i < orderedPaths[k].Item1.Count; i++)
            {
                nodesVisited++;
                copiedGrid[orderedPaths[k].Item1[i].Item1, orderedPaths[k].Item1[i].Item2].Highlighted();
                yield return new WaitForSeconds(0.05f);
            }
            if (orderedPaths[k].Item2[0] != (-1, -1))
            {
                for (int i = 0; i < orderedPaths[k].Item2.Length; i++)
                {
                    pathSize++;
                    copiedGrid[orderedPaths[k].Item2[i].Item1, orderedPaths[k].Item2[i].Item2].painted(checkPointVisited);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            checkPointVisited++;
            yield return new WaitForSeconds(1f);
            if (k != orderedPaths.Count - 1)
            {
                for (int i = 0; i < orderedPaths[k].Item1.Count; i++)
                {
                    copiedGrid[orderedPaths[k].Item1[i].Item1, orderedPaths[k].Item1[i].Item2].UnSearch();
                }
            }
        }
        simulating = false;
        PresentSummary();
    }
    
    void PresentSummary()
    {
        if (child == null)
        {
            child = Instantiate(indSummary, Camera.main.WorldToScreenPoint(originalPos), Quaternion.identity).GetComponent<SummaryScript>();
            child.transform.position = new Vector3(child.transform.position.x+250, child.transform.position.y+250);
            child.SetValues(algorithmName, calculatedTime, nodesVisited, pathSize);
            child.transform.SetParent(canvas.transform);
        }

    }
}
