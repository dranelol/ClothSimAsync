using UnityEngine;
using System.Collections;

public class AnchorBehaviour : MonoBehaviour 
{
    public NodeInfo anchoredNode;
	
	
	// Update is called once per frame
	void Update () 
    {
        anchoredNode.WorldPosition = transform.position;
	}
}
