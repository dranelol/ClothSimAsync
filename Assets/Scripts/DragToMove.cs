using UnityEngine;
using System.Collections;

public class DragToMove : MonoBehaviour
{

	private Vector3 screenPoint;
	private Vector3 offset;
	private bool animated;
	
	private void OnMouseDown ()
	{
			screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
		
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		
	}
	
	private void OnMouseDrag ()
	{
			Vector3 curScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		
			Vector3 curPosition = Camera.main.ScreenToWorldPoint (curScreenPoint) + offset;
			transform.position = curPosition;
		
	}


		
}
