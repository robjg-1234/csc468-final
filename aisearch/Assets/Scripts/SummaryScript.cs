using TMPro;
using UnityEngine;

public class SummaryScript : MonoBehaviour
{
    [SerializeField] TMP_Text algorithmName;
    [SerializeField] TMP_Text time;
    [SerializeField] TMP_Text nodesExp;
    [SerializeField] TMP_Text pathLen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetValues(string algName, double timer, int nodeCount, int pathSize)
    {
        algorithmName.text = algName;
        time.text = "Calculation time: " + timer.ToString();
        nodesExp.text = "Nodes explored: " + nodeCount.ToString();
        pathLen.text = "Path length: " + pathSize.ToString();
    }
}
