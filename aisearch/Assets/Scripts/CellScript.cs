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
    
    public Color defaultColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultColor = cellMat.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeWall()
    {
        if (!isGoal && !isStart)
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
    public void painted()
    {
        cellMat.color = Color.cyan;
    }
    public void Highlighted()
    {
        cellMat.color = Color.grey;
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
