using UnityEngine;
using System.Collections;

public class SpringInfo
{
    private SpringType springType;
    public SpringType SpringType
    {
        get
        {
            return springType;
        }

        set
        {
            springType = value;
        }
    }

    private NodeInfo node1;

    public NodeInfo Node1
    {
        get
        {
            return node1;
        }
        set
        {
            node1 = value;
        }
    }

    private NodeInfo node2;

    public NodeInfo Node2
    {
        get
        {
            return node2;
        }
        set
        {
            node2 = value;
        }
    }

    private float springConstant;

    public float SpringConstant
    {

        get
        {
            return springConstant;
        }
        set
        {
            springConstant = value;
        }
    }

    private float restLength;

    public float RestLength
    {

        get
        {
            return restLength;
        }
        set
        {
            restLength = value;
        }
    }

    private float dampingConstant;

    public float DampingConstant
    {
        get
        {
            return dampingConstant;
        }

        set
        {
            dampingConstant = value;
        }
    }

}
