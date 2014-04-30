using UnityEngine;
using System.Collections;

public class NodeInfo
{
    private Vector2 gridPosition;
    
    public Vector2 GridPosition
    {
        get
        {
            return gridPosition;
        }
        set
        {
            gridPosition = value;
        }
    }

    private Vector3 worldPosition;

    public Vector3 WorldPosition
    {
        get
        {
            return worldPosition;
        }
        set
        {
            worldPosition = value;
			//transform.position = worldPosition;
        }
    }

    private Vector3 velocity;

    public Vector3 Velocity
    {
        get
        {
            return velocity;
        }
        set
        {
            velocity = value;
        }
    }

    private float mass;

    public float Mass
    {
        get
        {
            return mass;
        }
        set
        {
            mass = value;
        }
    }

	private Vector3 force;

	public Vector3 Force
	{
		get 
		{
			return force;
		}
		set
		{
			force = value;
		}
	}

	private bool isAnchor;

	public bool IsAnchor
	{
		get 
		{
			return isAnchor;
		}
		set
		{
			isAnchor = value;
		}
	}
	
    void Update()
    {
        //worldPosition = transform.position;
    }
}