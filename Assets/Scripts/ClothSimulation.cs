using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SpringType
{
    manhattan,
    structural,
    shear,
    bend
}

public class ClothSimulation : MonoBehaviour
{
    public Vector3 GRAVITY_VECTOR = new Vector3(0f, -2f, 0f);
    public Vector3 air_velocty = new Vector3(1f, 0, 0); // might not use this
    public const float TIME_STEP = 0.03f; // may not need this
    public float density = 1f;
    public float dragCoefficient = 0.5f;

    public bool isAnchored = true;
    

    public int clothWidth;
    public int clothHeight;

    public float nodeMass;

    public float manhattanSpringConstant;
    public float manhattanSpringRestLength;
    public float manhattanDampingConstant;

    public float structuralSpringConstant;
    public float structuralSpringRestLength;
    public float structuralDampingConstant;

    public float shearSpringConstant;
    public float shearSpringRestLength;
    public float shearDampingConstant;

    public float bendSpringConstant;
    public float bendSpringRestLength;
    public float bendDampingConstant;

    public GameObject nodePrefab;
    public GameObject springPrefab;
    public GameObject trianglePrefab;
    public GameObject debugRenderer;

    private List<NodeInfo> nodes;


    private List<SpringInfo> springs;

    private List<TriangleInfo> triangles;

    public bool initSpringRenderers;
    public bool initTriangleRenderers;

    private GameObject nodesParent;
    private GameObject springsParent;
    private GameObject trianglesParent;

    public GameObject anchor1;
    public GameObject anchor2;
    public GameObject anchor3;
    public GameObject anchor4;
    public GameObject anchorPrefab;

    private Dictionary<SpringInfo, LineDraw> springLineRenderers;
    private Dictionary<TriangleInfo, LineDraw> triangleLineRenderers;

    private float timeStepCounter = 0.0f;

    private void Awake()
    {

        nodes = new List<NodeInfo>();
        springs = new List<SpringInfo>();
        triangles = new List<TriangleInfo>();

        nodesParent = new GameObject("Nodes");
        springsParent = new GameObject("Springs");
        trianglesParent = new GameObject("Triangles");

        springLineRenderers = new Dictionary<SpringInfo, LineDraw>();
        triangleLineRenderers = new Dictionary<TriangleInfo, LineDraw>();

        nodesParent.transform.parent = transform;
        springsParent.transform.parent = transform;
        trianglesParent.transform.parent = transform;

        initNodes();
        initSprings();
        initTriangles();

        Debug.Log("Node Count: " + nodes.Count.ToString());
        Debug.Log("Spring Count: " + springs.Count.ToString());
        Debug.Log("Triangle Count: " + triangles.Count.ToString());


    }

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        foreach (NodeInfo node in nodes)
        {
            computeNodeForces(node);
        }

        foreach (SpringInfo spring in springs)
        {
            SpringInfo springInfo = spring;
            NodeInfo node1Info = springInfo.Node1;
            NodeInfo node2Info = springInfo.Node2;
            //Debug.Log(node1Info.gameObject.transform.position);
            if (initSpringRenderers)
            {

                springLineRenderers[spring].vertices[0] = node1Info.WorldPosition;

                springLineRenderers[spring].vertices[1] = node2Info.WorldPosition;
            }

            computeSpringForces(spring);
        }

        foreach (TriangleInfo triangle in triangles)
        {
            computeTriangleForces(triangle);
        }

        foreach (NodeInfo node in nodes)
        {
            IntegrateMotion(node);
        }
    }

    private void initNodes()
    {
        for (int i = 0; i < clothWidth; i++)
        {
            for (int j = 0; j < clothHeight; j++)
            {
                Vector3 newPosition = new Vector3(j, i, 0);

                //GameObject newNode = (GameObject)Instantiate(nodePrefab, newPosition, transform.rotation);
                NodeInfo newNodeInfo = new NodeInfo();

                newNodeInfo.GridPosition = new Vector2(j, i);
                newNodeInfo.WorldPosition = new Vector3(j, i, 0);
                newNodeInfo.Mass = nodeMass;
                newNodeInfo.Velocity = Vector3.zero;

                //newNode.transform.parent = nodesParent.transform;
                //newNode.name = "Node " + newNodeInfo.GridPosition;

                nodes.Add(newNodeInfo);
                
                if (newNodeInfo.GridPosition == new Vector2(0, 0))
                {
                    newNodeInfo.IsAnchor = true;
                    anchor1 = (GameObject)Instantiate(anchorPrefab, newNodeInfo.WorldPosition, Quaternion.identity);
                    AnchorBehaviour anchorBehaviour1 = anchor1.GetComponent<AnchorBehaviour>();
                    anchorBehaviour1.anchoredNode = newNodeInfo;
                    //anchor1.transform.parent = newNode.transform;
                    //newNode.transform.parent = anchor1.transform;
                }
                    
                else if (newNodeInfo.GridPosition == new Vector2(clothWidth - 1, 0))
                {
                    newNodeInfo.IsAnchor = true;
                    anchor2 = (GameObject)Instantiate(anchorPrefab, newNodeInfo.WorldPosition, Quaternion.identity);
                    AnchorBehaviour anchorBehaviour2 = anchor2.GetComponent<AnchorBehaviour>();
                    anchorBehaviour2.anchoredNode = newNodeInfo;
                    //anchor2.transform.parent = newNode.transform;
                    //newNode.transform.parent = anchor2.transform;
                } 
                else if (newNodeInfo.GridPosition == new Vector2(0, clothHeight - 1))
                {
                    newNodeInfo.IsAnchor = true;
                    anchor3 = (GameObject)Instantiate(anchorPrefab, newNodeInfo.WorldPosition, Quaternion.identity);
                    AnchorBehaviour anchorBehaviour3 = anchor3.GetComponent<AnchorBehaviour>();
                    anchorBehaviour3.anchoredNode = newNodeInfo;
                    //anchor3.transform.parent = newNode.transform;
                    //newNode.transform.parent = anchor3.transform;
                }
                    
                else if (newNodeInfo.GridPosition == new Vector2(clothWidth - 1, clothHeight - 1))
                {
                    newNodeInfo.IsAnchor = true;
                    anchor4 = (GameObject)Instantiate(anchorPrefab, newNodeInfo.WorldPosition, Quaternion.identity);
                    AnchorBehaviour anchorBehaviour4 = anchor4.GetComponent<AnchorBehaviour>();
                    anchorBehaviour4.anchoredNode = newNodeInfo;
                    //anchor4.transform.parent = newNode.transform;
                    //newNode.transform.parent = anchor4.transform;
                }
            }
        }
    }

    private void initSprings()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            // horizontal springs
            if ((i % clothWidth) != (clothWidth - 1))
            {
                makeSpring(nodes[i], nodes[i + 1], SpringType.manhattan);
            }

            // bend springs horizontal
            if ((i % clothWidth) != (clothWidth - 1) && (i % clothWidth) != (clothWidth - 2))
            {
                makeSpring(nodes[i], nodes[i + 2], SpringType.bend);
            }

            // vertical springs
            if (i < ((clothWidth * clothHeight) - clothWidth))
            {
                makeSpring(nodes[i], nodes[i + clothWidth], SpringType.manhattan);
            }

            // bend springs vertical
            if (i < ((clothWidth * clothHeight) - clothWidth*2))
            {
                makeSpring(nodes[i], nodes[i + clothWidth*2], SpringType.bend);
            }

            // diagonal down springs
            if (((i % clothWidth) != (clothWidth - 1)) && (i < ((clothWidth * clothHeight) - clothHeight)))
            {
                makeSpring(nodes[i], nodes[i + clothWidth + 1], SpringType.structural);
            }

            // diagonal up springs
            if ((i % clothWidth != 0) && (i < (clothWidth * clothHeight) - clothHeight))
            {
                makeSpring(nodes[i], nodes[i + clothWidth - 1], SpringType.shear);
            }
        }
    }

    private void initTriangles()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if ((i < (clothWidth * clothHeight) - clothHeight) && ((i % clothWidth) != (clothWidth - 1)))
            {

                //NW, NE, SW
                makeTriangle(nodes[i], nodes[i + 1], nodes[i + clothWidth]);
                //NW, NE, SE
                makeTriangle(nodes[i], nodes[i + 1], nodes[i + clothWidth + 1]);
                //NE, SW, SE
                makeTriangle(nodes[i + 1], nodes[i + clothWidth], nodes[i + clothWidth + 1]);
                //NW, SW, SE
                makeTriangle(nodes[i], nodes[i + clothWidth], nodes[i + clothWidth + 1]);

            }

        }
    }

    private void makeSpring(NodeInfo node1, NodeInfo node2, SpringType springType)
    {
        SpringInfo newSpringInfo = new SpringInfo();
        newSpringInfo.Node1 = node1;
        newSpringInfo.Node2 = node2;
        newSpringInfo.SpringType = springType;


        springs.Add(newSpringInfo);


        if (initSpringRenderers == true)
        {
            GameObject nodeRender = (GameObject)Instantiate(debugRenderer, Vector3.zero, Quaternion.identity);
            LineDraw nodeDraw = nodeRender.GetComponent<LineDraw>();
            nodeDraw.vertices.Add(node1.WorldPosition);
            nodeDraw.vertices.Add(node2.WorldPosition);
            nodeDraw.colorStart = Color.white;
            nodeDraw.colorEnd = Color.red;

            nodeRender.transform.parent = transform;
            springLineRenderers[newSpringInfo] = nodeDraw;
        }
    }

    private void makeTriangle(NodeInfo node1, NodeInfo node2, NodeInfo node3)
    {
        TriangleInfo newTriangleInfo = new TriangleInfo();
        newTriangleInfo.Node1 = node1;
        newTriangleInfo.Node2 = node2;
        newTriangleInfo.Node3 = node3;

        triangles.Add(newTriangleInfo);

        if (initTriangleRenderers == true)
        {
            GameObject nodeRender1 = (GameObject)Instantiate(debugRenderer, Vector3.zero, Quaternion.identity);
            LineDraw nodeDraw1 = nodeRender1.GetComponent<LineDraw>();
            nodeDraw1.vertices.Add(node1.WorldPosition);
            nodeDraw1.vertices.Add(node2.WorldPosition);
            nodeDraw1.vertices.Add(node3.WorldPosition);
            nodeDraw1.vertices.Add(node1.WorldPosition);
            nodeDraw1.colorStart = Color.white;
            nodeDraw1.colorEnd = Color.green;

            nodeRender1.transform.parent = transform;
        }
    }

    private void computeNodeForces(NodeInfo node)
    {

        node.Force = GRAVITY_VECTOR * node.Mass; // apply gravity
    }

    private void computeSpringForces(SpringInfo springInfo)
    {
        NodeInfo node1Info = springInfo.Node1;

        NodeInfo node2Info = springInfo.Node2;

        // figure out the type of spring; set local spring constants relevant to that type

        float springConstant = 0.0f;
        float dampingConstant = 0.0f;
        float restLength = 0.0f;

        switch (springInfo.SpringType)
        {
            case SpringType.bend:
                springConstant = bendSpringConstant;
                dampingConstant = bendDampingConstant;
                restLength = bendSpringRestLength;
                break;
            case SpringType.manhattan:
                springConstant = manhattanSpringConstant;
                dampingConstant = manhattanDampingConstant;
                restLength = manhattanSpringRestLength;
                break;
            case SpringType.shear:
                springConstant = shearSpringConstant;
                dampingConstant = shearDampingConstant;
                restLength = shearSpringRestLength;
                break;
            case SpringType.structural:
                springConstant = structuralSpringConstant;
                dampingConstant = structuralDampingConstant;
                restLength = structuralSpringRestLength;
                break;
        }



        // Spring force: multiply the negative spring constant (the tendancy of the spring to remain at rest length) by 
        // the rest length minus the distance between the two nodes.

        Vector3 vectorBetween = node2Info.WorldPosition - node1Info.WorldPosition;
        Vector3 vectorBetweenNorm = vectorBetween.normalized;

        float nodeDistance = Vector3.Distance(node1Info.WorldPosition, node2Info.WorldPosition);
        float springForce = -springConstant * (restLength - nodeDistance);

        // Damping force: multiply the negative damping constant (i dunno what that means) by the velocity of the first node minus the
        // velocity of the second node.

        // Next, we find the 1D velocities
        float node1Velocity = Vector3.Dot(vectorBetweenNorm, node1Info.Velocity);
        float node2Velocity = Vector3.Dot(vectorBetweenNorm, node2Info.Velocity);

        float damperForce = -dampingConstant * (node1Velocity - node2Velocity);

        // Add the two forces to compute the spring-damper force on the first node, the spring-damper force on the second node is negative
        // spring-damper on the first. 

        float springDamperForce = springForce + damperForce;

        // Now, that we've found 1D force, map it back into 3D

        Vector3 force1 = springDamperForce * vectorBetweenNorm;
        Vector3 force2 = -force1;

        // Apply the forces
        node1Info.Force += force1;
        node2Info.Force += force2;

    }

    private void computeTriangleForces(TriangleInfo triangleInfo)
    {

        NodeInfo node1Info = triangleInfo.Node1;

        NodeInfo node2Info = triangleInfo.Node2;

        NodeInfo node3Info = triangleInfo.Node3;

        // apply aerodynamic forces
        Vector3 triangle_velocity = (node1Info.Velocity + node2Info.Velocity + node3Info.Velocity) / 3f;
        triangle_velocity -= air_velocty;

        // calulate triangle normal using positions (uses cross product)
        // (r2 - r1) X (r3 - r1)
        Vector3 r2r1crossr3r1 = Vector3.Cross((node2Info.WorldPosition - node1Info.WorldPosition), (node3Info.WorldPosition - node1Info.WorldPosition));

        Vector3 normal = r2r1crossr3r1 / r2r1crossr3r1.magnitude;

        // Vector3 aeroForce = 0.5f * density * dragCoefficient * triangle_velocity.sqrMagnitude * 0.5f * r2r1crossr3r1.magnitude * normal;

        Vector3 aeroForce = -0.5f * dragCoefficient * density * ((0.5f * Vector3.Dot(triangle_velocity, normal) * triangle_velocity.magnitude) / r2r1crossr3r1.magnitude) * r2r1crossr3r1;

        aeroForce /= 3f;

        node1Info.Force += aeroForce;
        node2Info.Force += aeroForce;
        node3Info.Force += aeroForce;
    }

    private void IntegrateMotion(NodeInfo node)
    {
        if (!node.IsAnchor || !isAnchored)
        {
            Vector3 acceleration = node.Force / node.Mass;
            node.Velocity += acceleration * Time.fixedDeltaTime;
            //node.Velocity += acceleration * TIME_STEP;
            node.WorldPosition += node.Velocity * Time.fixedDeltaTime;
            //node.WorldPosition += node.Velocity * TIME_STEP;
        }
    }
}
