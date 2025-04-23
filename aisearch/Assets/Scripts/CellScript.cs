using Unity.VisualScripting;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    [SerializeField] SpriteRenderer cellMat;
    public int cellX;
    public int cellY;
    public bool walkable = true;
    bool isGoal = false;
    bool isStart = false;
    bool partOfThePath = false;
    public Color defaultColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultColor = cellMat.color;
    }

    // Update is called once per frame
    public void BackToDefault()
    {
        isGoal = false;
        isStart = false;
        walkable = true;
        cellMat.color = Color.white;
        defaultColor = cellMat.color;
    }
    public void ChangeWall(int state)
    {
        if (!isGoal && !isStart && defaultColor != Color.blue)
        {
            if (state == 0)
            {
                walkable = !walkable;
                if (!walkable)
                {
                    cellMat.color = Color.black;
                }
                else
                {
                    cellMat.color = Color.white;
                }
                defaultColor = cellMat.color;
            }
            else if (state == 1)
            {
                walkable = false;
                cellMat.color = Color.black;
                defaultColor = cellMat.color;
            }
            else
            {
                walkable = true;
                cellMat.color = Color.white;
                defaultColor = cellMat.color;
            }
        }
    }
    public void SetGoal()
    {
        cellMat.color = Color.red;
        defaultColor = cellMat.color;
        isGoal = true;
        walkable = true;
    }
    public void SetStart()
    {
        cellMat.color = Color.green;
        defaultColor = cellMat.color;
        walkable = true;
        isStart = true;
    }
    public void Hover()
    {
        cellMat.color = Color.gray;
    }
    public void UnHover()
    {
        cellMat.color = defaultColor;
    }
    public void painted(int passes)
    {
        partOfThePath = true;
        cellMat.color = Color.cyan * 1/passes;
        cellMat.color = new Color(cellMat.color.r, cellMat.color.g, cellMat.color.b, 1);
    }
    public void Highlighted()
    {
        if (!partOfThePath)
        {
            cellMat.color = Color.grey;
        }
    }
    public void UnSearch()
    {
        if (!partOfThePath)
        {
            cellMat.color = defaultColor;
        }
    }
    public bool SetCheckPoint()
    {
        bool success = false;
        if (!isGoal && !isStart)
        {
            success = true;
        }
        return success;
    }
    public void ColorMe()
    {
        cellMat.color = Color.blue;
        defaultColor = Color.blue;
        walkable = true;
        isStart = false;
        isGoal = false;
    }
    public void Copy(CellScript target)
    {
        cellX = target.cellX;
        cellY = target.cellY;
        walkable = target.walkable;
        isGoal = target.isGoal;
        isStart = target.isStart;
        defaultColor = target.defaultColor;
        cellMat.color = defaultColor;
    }
}
