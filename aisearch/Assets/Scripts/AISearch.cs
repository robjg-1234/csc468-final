
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AISearch
{
    //Grabs all the neighbors directly to the sides of the starting node which means that it doesn't move diagonally 
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
    // Uses a stack to go to the node that was added at last, due to how it is added the algorithm has a bias of going up first.
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
                    // A node gets added to the stack only if it is not in the stack and it hasn't been visited.
                    if (!nodeStack.Contains(i) && !seen.Contains(i))
                    {
                        relations.Add((currentNode, i));
                        nodeStack.Push(i);
                    }
                }
            }
        }
        if (found)
        {
            // Backtracks to show the final path.
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
            //It returns the nodes visited and the path
            return (seen, path);
        }
        else
        {
            //If the path was not found it returns all the nodes visited and an array with a value of (-1,-1) to indicate that it didn't found a valid path.
            return (seen, new (int, int)[] { (-1, -1) });
        }
    }

    // BFS
    //The breadth-first search algorithm uses a queue to keep track of the nodes in a first in first out basis.
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
                // if the goal is found the queue gets cleared
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
                    // A node is added to the queue if it is not in the queue and it hasn't been seen.
                    if (!seen.Contains(newNodes[i]) && !nodeQueue.Contains(newNodes[i]))
                    {
                        nodeQueue.Enqueue(newNodes[i]);
                        relations.Add((currentnode, newNodes[i]));
                    }
                }
            }
        }
        //Backtracks to see if there is a valid path, if there isn'y it returns a path with the (-1.-1) value in the other case it returns the valid path. It also returns the nodes explored.
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
    // This algorithm uses a dictionary as a priority queue to pick the one with the lowest fscore.
    static (int, int)[] BacktrackPath((int, int)[,] relationMap, (int, int) currentSpot)
    {
        // This backtracks using a relationship map and is used for both the greedy search and the Astar algorithm.
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
        // the hueristic formula used is the Manhattan distance.
        return Mathf.Abs(currentNode.Item1 - endPoint.Item1) + Mathf.Abs(currentNode.Item2 - endPoint.Item2);
    }

    public static (List<(int, int)>, (int, int)[]) AStar((int, int) startingPoint, (int, int) goalPoint, bool[,] map)
    {
        List<(int, int)> nodesSeen = new List<(int, int)>();
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
        // Initialize the fscore and gscore map with the maximum possible value
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                gScore[i, j] = float.MaxValue;
                fScore[i, j] = float.MaxValue;
            }
        }
        // Set the starting point gScore and fScore value to the starting values.
        gScore[startingPoint.Item1, startingPoint.Item2] = 0f;
        fScore[startingPoint.Item1, startingPoint.Item2] = HeuristicFormula(startingPoint, goalPoint);

        nodeQueue.Add(startingPoint, fScore[startingPoint.Item1, startingPoint.Item2]);
        while (nodeQueue.Count > 0)
        {
            nodesVisits++;
            (int, int) currentNode = (-1, -1);
            // This finds which node has the lowest fScore of the available nodes
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
                    // This helps the algorithm speed up by also choosing the one that is physically closest to the endpoint in case there is a tie.
                    else if ((keys.Value == nodeQueue[currentNode]) && (HeuristicFormula(currentNode, goalPoint) - HeuristicFormula(keys.Key, goalPoint) > 0) && (HeuristicFormula(currentNode, goalPoint) - HeuristicFormula(keys.Key, goalPoint) < 3))
                    {
                        currentNode = keys.Key;
                    }
                }
            }
            if (!nodesSeen.Contains(currentNode))
            {
                nodesSeen.Add(currentNode);
            }
            if (currentNode == goalPoint)
            {
                nodesVisited.Add(currentNode);
                foundPath = BacktrackPath(relationMap, currentNode);
                return (nodesSeen, foundPath);
            }
            else
            {
                nodesVisited.Add(currentNode);
                nodeQueue.Remove(currentNode);
                foreach ((int, int) i in GetNeighbors(currentNode.Item1, currentNode.Item2, map))
                {
                    if (i != (-1, -1))
                    {
                        //Establishes the gScore and fScore if it is lower than its existing value.
                        // The cost for moving is always 1 + the gScore of the current node that is visiting it.
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
                        if (!nodesSeen.Contains(i))
                        {
                            nodesSeen.Add(i);
                        }
                    }
                    //This helps guarantee that there are no blank spots in the exploration process
                    foreach ((int, int) j in GetNeighbors(i.Item1, i.Item2, map))
                    {
                        if (j != (-1, -1))
                        {
                            // Establishes the gScore of the nieghbors in case we visit it from a different position later in the exploration process
                            float tentativeGScore = gScore[i.Item1, i.Item2] + 1;
                            if (tentativeGScore < gScore[j.Item1, j.Item2])
                            {
                                relationMap[j.Item1, j.Item2] = i;
                                gScore[j.Item1, j.Item2] = tentativeGScore;
                                fScore[j.Item1, j.Item2] = tentativeGScore + HeuristicFormula(j, goalPoint);
                            }
                        }
                        if (!nodesSeen.Contains(j))
                        {
                            nodesSeen.Add(j);
                        }
                    }
                }
            }
        }
        // returns all the nodes that had there gScore modified and the actual path to the end goal or (-1,-1) if it didn't find anything.
        return (nodesSeen, foundPath);
    }

    //Greedy Search
    public static (List<(int, int)>, (int, int)[]) GreedySearch((int, int) startingPoint, (int, int) goalPoint, bool[,] map)
    {
        // This utilizes a similar approach to the A* when it comes to looking for the lowest cost but the cost is only calculated at the immediate position which means that it only considers the calculated distance to the end point
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
            // Chooses the node that is the closest to the endpoint from the available nodes
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
                // If it reaches the end point it returns all the nodes visited and the path
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
        // If it doesnt find the end point it returns an array with (-1,-1) alongside all the nodes visited
        return (nodesVisited, foundPath);
    }

    //Random Exploration Search
    // This algorithm randomly chooses which direction to go as long as it hasn't already been visited
    public static (List<(int, int)>, (int, int)[]) RandomSearch((int, int) startingPoint, (int, int) goalPoint, bool[,] map)
    {
        int nodesVisits = 0;
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);
        List<(int, int)> nodesVisited = new List<(int, int)>();
        (int, int)[] foundPath = new (int, int)[1] { (-1, -1) };
        (int, int)[,] relationMap = new (int, int)[mapWidth, mapHeight];
        relationMap[startingPoint.Item1, startingPoint.Item2] = (-1, -1);
        List<(int, int)> nodeQueue = new List<(int, int)>() { startingPoint };
        while (nodeQueue.Count > 0)
        {
            nodesVisits++;
            int randomNode = Random.Range(0, nodeQueue.Count);
            (int, int) currentNode = nodeQueue[randomNode];
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
