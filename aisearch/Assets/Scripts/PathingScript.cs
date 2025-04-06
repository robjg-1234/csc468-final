using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingScript : MonoBehaviour
{
    [SerializeField] GameObject cell;
    (int, int)[] path = new (int, int)[0];
    [SerializeField] int pathingType = 0;
    List<(int, int)> nodesExplored = new List<(int, int)>();
    CellScript[,] copiedGrid = null;
    public bool simulating = false;
    GridManager gm;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = GridManager.instance;
        gm.robots.Add(this);
    }

    // Update is called once per frame
    public void SetPath(bool[,] walkableMap, CellScript[,] originalGrid, (int,int)startingPos, (int,int) endPoint)
    {
        simulating = true;
        CreateGridFromTemplate(walkableMap, originalGrid);
        if (pathingType == 0)
        {
            Debug.Log("BFS");
            (nodesExplored, path) = AISearch.BreadthFirstSearch(startingPos.Item1, startingPos.Item2, endPoint.Item1, endPoint.Item2, walkableMap);
            Debug.Log(path.Length);
        }
        else if (pathingType == 1)
        {
            Debug.Log("DFS");
            (nodesExplored, path) = AISearch.NRDepthFirstSearch(startingPos, endPoint, walkableMap);
            Debug.Log(path.Length);
        }
        else if(pathingType == 2)
        {
            Debug.Log("Greedy");
            (nodesExplored, path) = AISearch.GreedySearch(startingPos, endPoint, walkableMap);
            Debug.Log(path.Length);
        }
        else if (pathingType == 3)
        {
            Debug.Log("Astar");
            (nodesExplored, path) = AISearch.AStar(startingPos, endPoint, walkableMap);
            Debug.Log(path.Length);
        }
        else if (pathingType == 4)
        {
            Debug.Log("Random");
            (nodesExplored, path) = AISearch.RandomSearch(startingPos, endPoint, walkableMap);
            Debug.Log(path.Length);
        }
    }
    public void SimulateRobot()
    {
        StartCoroutine(SimulateThisStep());
    }
    
    void CreateGridFromTemplate(bool[,] walkableMap, CellScript[,] template)
    {
        copiedGrid = new CellScript[walkableMap.GetLength(0), walkableMap.GetLength(1)];
        for (int i = 0; i < copiedGrid.GetLength(0); i++)
        {
            for (int j = 0; j < copiedGrid.GetLength(1); j++)
            {
                Vector3 newPos = new Vector3(transform.position.x + i, transform.position.y + j, transform.position.z);
                copiedGrid[i, j] = Instantiate(cell, newPos, Quaternion.identity).GetComponent<CellScript>();
                copiedGrid[i, j].Copy(template[i,j]);
            }
        }
    }
    public void DeleteCurrentGrid()
    {
        for (int i = 0;i < copiedGrid.GetLength(0);i++)
        {
            for (int j = 0; j < copiedGrid.GetLength(1); j++)
            {
                Destroy(copiedGrid[i,j].gameObject);
                copiedGrid[i,j] = null;
            }
        }
    }
    IEnumerator SimulateThisStep()
    {

        for (int i = 0; i< nodesExplored.Count;i++)
        {
            copiedGrid[nodesExplored[i].Item1, nodesExplored[i].Item2].Highlighted();
            yield return new WaitForSeconds(0.1f);
        }
        if (path[0] != (-1, -1))
        {
            
            for (int i = 0; i < path.Length; i++)
            {
                copiedGrid[path[i].Item1, path[i].Item2].painted();
                yield return new WaitForSeconds(0.2f);
            }
        }
        simulating = false;
    }

}
