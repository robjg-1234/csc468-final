
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AISearch
{

    private static (int, int)[] GetNeighbors(int posX, int posY, bool[,] map)
    {
        List<(int, int)> neighbors = new List<(int, int)>();
        if (!(posX - 1 < 0))
        {
            if (map[posX - 1, posY])
            {
                neighbors.Add((posX - 1, posY));
            }
        }
        if (!(posY - 1 < 0))
        {
            if (map[posX, posY - 1])
            {
                neighbors.Add((posX, posY - 1));
            }
        }
        if (posX + 1 < map.GetLength(0))
        {
            if (map[posX + 1, posY])
            {
                neighbors.Add((posX + 1, posY));
            }
        }
        if (posY + 1 < map.GetLength(1))
        {
            if (map[posX, posY + 1])
            {
                neighbors.Add((posX, posY + 1));
            }
        }
        if (neighbors.Count > 0)
        {
            return neighbors.ToArray();
        }
        else
        {
            return new (int, int)[] { (-1, -1) };
        }
    }


    //non-recursive dfs
    public static (List<(int, int)>, (int, int)[]) NRDepthFirstSearch((int, int) startingPoint, (int, int) goalPoint, bool[,] map)
    {
        int nodesVisits = 1;
        bool found = false;
        (int, int)[] path = new (int, int)[0];
        List<((int, int), (int, int))> relations = new List<((int, int), (int, int))>() { ((-1, -1), startingPoint) };
        List<(int, int)> seen = new List<(int, int)>() { startingPoint };
        Stack<(int, int)> nodeStack = new Stack<(int, int)>();
        foreach ((int, int) i in GetNeighbors(startingPoint.Item1, startingPoint.Item2, map))
        {
            if (i != (-1, -1))
            {
                relations.Add((startingPoint, i));
                nodeStack.Push(i);
            }
        }
        while (nodeStack.Count > 0)
        {
            nodesVisits++;
            (int, int) currentNode = nodeStack.Pop();
            if (currentNode == goalPoint)
            {
                seen.Add(currentNode);
                nodeStack.Clear();
                found = true;
            }
            else
            {
                seen.Add(currentNode);
                foreach ((int, int) i in GetNeighbors(currentNode.Item1, currentNode.Item2, map))
                {
                    if (!seen.Contains(i))
                    {
                        relations.Add((currentNode, i));
                        nodeStack.Push(i);
                    }
                }
            }
        }
        if (found)
        {
            List<(int, int)> curatedPath = new List<(int, int)>() { goalPoint };
            (int, int) currentNode = curatedPath[0];
            while (!curatedPath.Contains(startingPoint))
            {
                for (int i = 0; i < relations.Count; i++)
                {
                    if (relations[i].Item2 == currentNode)
                    {
                        curatedPath.Add(relations[i].Item1);
                        currentNode = relations[i].Item1;
                    }
                }
            }
            curatedPath.Reverse();
            path = curatedPath.ToArray();
            return (seen, path);
        }
        else
        {
            return (seen, new (int, int)[] { (-1, -1) });
        }
    }

    // BFS
    public static (List<(int, int)>, (int, int)[]) BreadthFirstSearch(int startingX, int startingY, int goalX, int goalY, bool[,] map)
    {
        int nodesVisited = 0;
        bool found = false;
        List<((int, int), (int, int))> relations = new List<((int, int), (int, int))>() { ((-1, -1), (startingX, startingY)) };
        (int, int)[] foundPath = new (int, int)[0];
        Queue<(int, int)> nodeQueue = new Queue<(int, int)>();
        (int, int)[] initialNeighbors = GetNeighbors(startingX, startingY, map);
        List<(int, int)> seen = new List<(int, int)>() { (startingX, startingY) };
        for (int i = 0; i < initialNeighbors.Length; i++)
        {
            if (initialNeighbors[i] != (-1, -1))
            {
                nodeQueue.Enqueue(initialNeighbors[i]);
                relations.Add(((startingX, startingY), initialNeighbors[i]));
            }
        }
        while (nodeQueue.Count > 0)
        {
            (int, int) currentnode = nodeQueue.Dequeue();
            nodesVisited++;
            if (currentnode.Item1 == goalX && currentnode.Item2 == goalY)
            {
                seen.Add(currentnode);
                nodeQueue.Clear();
                found = true;
            }
            else
            {
                (int, int)[] newNodes = GetNeighbors(currentnode.Item1, currentnode.Item2, map);
                seen.Add(currentnode);
                for (int i = 0; i < newNodes.Length; i++)
                {
                    if (!seen.Contains(newNodes[i]) && !nodeQueue.Contains(newNodes[i]))
                    {
                        nodeQueue.Enqueue(newNodes[i]);
                        relations.Add((currentnode, newNodes[i]));
                    }
                }
            }
        }
        if (found)
        {
            List<(int, int)> curatedPath = new List<(int, int)>() { (goalX, goalY) };
            (int, int) currentNode = curatedPath[0];
            while (!curatedPath.Contains((startingX, startingY)))
            {
                for (int i = 0; i < relations.Count; i++)
                {
                    if (relations[i].Item2 == currentNode)
                    {
                        curatedPath.Add(relations[i].Item1);
                        currentNode = relations[i].Item1;
                    }
                }
            }
            curatedPath.Reverse();
            foundPath = curatedPath.ToArray();
            return (seen, foundPath);
        }
        else
        {
            return (seen, new (int, int)[] { (-1, -1) });
        }
    }
    //AStar Implementation
    static (int, int)[] BacktrackPath((int, int)[,] relationMap, (int, int) currentSpot)
    {
        List<(int, int)> path = new List<(int, int)>();
        (int, int) currentNode = currentSpot;
        while (currentNode != (-1, -1))
        {
            path.Add(currentNode);
            currentNode = relationMap[currentNode.Item1, currentNode.Item2];
        }
        path.Reverse();
        return path.ToArray();
    }

    static float HeuristicFormula((int, int) currentNode, (int, int) endPoint)
    {
        return Mathf.Abs(currentNode.Item1 - endPoint.Item1) + Mathf.Abs(currentNode.Item2 - endPoint.Item2);
    }

    public static (List<(int, int)>, (int, int)[]) AStar((int, int) startingPoint, (int, int) goalPoint, bool[,] map)
    {
        int nodesVisits = 0;
        Dictionary<(int, int), float> nodeQueue = new Dictionary<(int, int), float>();
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);
        List<(int, int)> nodesVisited = new List<(int, int)>();
        (int, int)[] foundPath = new (int, int)[1] { (-1, -1) };
        float[,] gScore = new float[mapWidth, mapHeight];
        float[,] fScore = new float[mapWidth, mapHeight];
        (int, int)[,] relationMap = new (int, int)[mapWidth, mapHeight];
        relationMap[startingPoint.Item1, startingPoint.Item2] = (-1, -1);
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                gScore[i, j] = float.MaxValue;
                fScore[i, j] = float.MaxValue;
            }
        }
        gScore[startingPoint.Item1, startingPoint.Item2] = 0f;
        fScore[startingPoint.Item1, startingPoint.Item2] = HeuristicFormula(startingPoint, goalPoint);

        nodeQueue.Add(startingPoint, fScore[startingPoint.Item1, startingPoint.Item2]);
        while (nodeQueue.Count > 0)
        {
            nodesVisits++;
            (int, int) currentNode = (-1, -1);
            foreach (KeyValuePair<(int, int), float> keys in nodeQueue)
            {
                if (currentNode == (-1, -1))
                {
                    currentNode = keys.Key;
                }
                else
                {
                    if (keys.Value < nodeQueue[currentNode])
                    {
                        currentNode = keys.Key;
                    }
                    else if ((keys.Value == nodeQueue[currentNode])&& (HeuristicFormula(currentNode, goalPoint) - HeuristicFormula(keys.Key, goalPoint) > 0) && (HeuristicFormula(currentNode, goalPoint) - HeuristicFormula(keys.Key, goalPoint) < 3))
                    {
                        currentNode = keys.Key;
                    }
                }
            }

            if (currentNode == goalPoint)
            {
                nodesVisited.Add(currentNode);
                foundPath = BacktrackPath(relationMap, currentNode);
                return (nodesVisited, foundPath);
            }
            else
            {
                nodesVisited.Add(currentNode);
                nodeQueue.Remove(currentNode);
                foreach ((int, int) i in GetNeighbors(currentNode.Item1, currentNode.Item2, map))
                {
                    if (i != (-1, -1))
                    {

                        float tentativeGScore = gScore[currentNode.Item1, currentNode.Item2] + 1;
                        if (tentativeGScore < gScore[i.Item1, i.Item2])
                        {
                            relationMap[i.Item1, i.Item2] = currentNode;
                            gScore[i.Item1, i.Item2] = tentativeGScore;
                            fScore[i.Item1, i.Item2] = tentativeGScore + HeuristicFormula(i, goalPoint);
                        }
                        if (!nodesVisited.Contains(i))
                        {
                            if (!nodeQueue.ContainsKey(i))
                            {
                                nodeQueue.Add(i, fScore[i.Item1, i.Item2]);
                            }
                        }
                    }
                }
            }
        }
        return (nodesVisited, foundPath);
    }

    //Greedy Search
    public static (List<(int, int)>, (int, int)[]) GreedySearch((int, int) startingPoint, (int, int) goalPoint, bool[,] map)
    {
        int nodesVisits = 0;
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);
        List<(int, int)> nodesVisited = new List<(int, int)>();
        (int, int)[] foundPath = new (int, int)[1] { (-1, -1) };
        float[,] dToEndPoint = new float[mapWidth, mapHeight];
        (int, int)[,] relationMap = new (int, int)[mapWidth, mapHeight];
        relationMap[startingPoint.Item1, startingPoint.Item2] = (-1, -1);
        List<(int, int)> nodeQueue = new List<(int, int)>();
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                dToEndPoint[i, j] = HeuristicFormula((i, j), goalPoint);
            }
        }
        nodeQueue.Add(startingPoint);
        while (nodeQueue.Count > 0)
        {
            nodesVisits++;
            (int, int) currentNode = (-1, -1);
            for (int i = 0; i < nodeQueue.Count; i++)
            {
                if (currentNode != (-1, -1))
                {
                    if (dToEndPoint[currentNode.Item1, currentNode.Item2] > dToEndPoint[nodeQueue[i].Item1, nodeQueue[i].Item2])
                    {
                        currentNode = nodeQueue[i];
                    }
                }
                else
                {
                    currentNode = nodeQueue[i];
                }
            }
            if (currentNode == goalPoint)
            {
                nodesVisited.Add(currentNode);
                foundPath = BacktrackPath(relationMap, currentNode);
                return (nodesVisited, foundPath);
            }
            else
            {
                nodeQueue.Remove(currentNode);
                nodesVisited.Add(currentNode);
                foreach ((int, int) i in GetNeighbors(currentNode.Item1, currentNode.Item2, map))
                {
                    if (i != (-1, -1))
                    {
                        if (!nodesVisited.Contains(i) && !nodeQueue.Contains(i))
                        {
                            relationMap[i.Item1, i.Item2] = currentNode;
                            nodeQueue.Add(i);
                        }
                    }
                }
            }

        }

        return (nodesVisited, foundPath);
    }

    //Random Exploration Search
    public static (List<(int, int)>, (int, int)[]) RandomSearch((int, int) startingPoint, (int, int) goalPoint, bool[,] map)
    {
        int nodesVisits = 0;
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);
        List<(int, int)> nodesVisited = new List<(int, int)>();
        (int, int)[] foundPath = new (int, int)[1] { (-1, -1) };
        (int, int)[,] relationMap = new (int, int)[mapWidth, mapHeight];
        relationMap[startingPoint.Item1, startingPoint.Item2] = (-1, -1);
        List<(int, int)> nodeQueue = new List<(int, int)>() { startingPoint};
        while (nodeQueue.Count > 0)
        {
            nodesVisits++;
            int randomNode = Random.Range(0, nodeQueue.Count);
            (int,int) currentNode = nodeQueue[randomNode];
            if (currentNode == goalPoint)
            {
                foundPath = BacktrackPath(relationMap, currentNode);
                nodesVisited.Add(currentNode);
                return (nodesVisited, foundPath);
            }
            else
            {
                nodesVisited.Add(currentNode);
                nodeQueue.Remove(currentNode);
                foreach ((int, int) i in GetNeighbors(currentNode.Item1, currentNode.Item2, map))
                {
                    if (i != (-1, -1))
                    {
                        if (!nodesVisited.Contains(i) && !nodeQueue.Contains(i))
                        {
                            relationMap[i.Item1, i.Item2] = currentNode;
                            nodeQueue.Add(i);
                        }
                    }
                }
            }
        }
        return (nodesVisited, foundPath);
    }
}
