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
    private int cores;

    public int clothWidth;
    public int clothHeight;

    public int nodeCount;

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

    private Dictionary<int, NodeInfo> vertexNodeDict;

    public UnityThreadManager threadManager;

    private Dictionary<SpringInfo, LineDraw> springLineRenderers;
    private Dictionary<TriangleInfo, LineDraw> triangleLineRenderers;

    private float timeStepCounter = 0.0f;

    bool nodeMutex = true;
    bool springMutex = true;
    bool triMutex = true;

    bool integrateMutex = true;

    bool renderMutex = true;

    Mesh clothMesh;

    private void Awake()
    {
        threadManager = GetComponent<UnityThreadManager>();
        cores = SystemInfo.processorCount;
        Debug.Log("cores: " + cores);
        nodes = new List<NodeInfo>();
        springs = new List<SpringInfo>();
        triangles = new List<TriangleInfo>();

        nodeCount = clothWidth * clothHeight;

        vertexNodeDict = new Dictionary<int, NodeInfo>();

        nodesParent = new GameObject("Nodes");
        springsParent = new GameObject("Springs");
        trianglesParent = new GameObject("Triangles");

        springLineRenderers = new Dictionary<SpringInfo, LineDraw>();
        triangleLineRenderers = new Dictionary<TriangleInfo, LineDraw>();

        nodesParent.transform.parent = transform;
        springsParent.transform.parent = transform;
        trianglesParent.transform.parent = transform;

        clothMesh = new Mesh();

        clothMesh.vertices = initNodes().ToArray();

        initSprings();
        
        Vector2[] uvs = new Vector2[clothMesh.vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(clothMesh.vertices[i].x, clothMesh.vertices[i].y);
        }

        clothMesh.uv = uvs;

        clothMesh.triangles = initTriangles().ToArray();

        clothMesh.RecalculateNormals();

        gameObject.AddComponent<MeshRenderer>().renderer.material.shader = Shader.Find("Diffuse");
        gameObject.AddComponent<MeshFilter>().mesh = clothMesh;

        clothMesh = GetComponent<MeshFilter>().mesh;

        clothMesh.RecalculateNormals();
        /*

        GameObject plane = new GameObject("meshPlane");

        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));

        meshFilter.mesh = clothMesh;

        MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material.shader = Shader.Find("Diffuse");
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.green);
        tex.Apply();
        renderer.material.mainTexture = tex;
        renderer.material.color = Color.green;
         */
        

        Debug.Log("Node Count: " + nodes.Count.ToString());
        Debug.Log("Spring Count: " + springs.Count.ToString());
        Debug.Log("Triangle Count: " + triangles.Count.ToString());

        


    }

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        //if (integrateMutex == true)
        //{
            nodeMutex = false;
            springMutex = false;
            triMutex = false;
            integrateMutex = false;
            renderMutex = false;

           

            //Loom.RunAsync(() =>
            //{
                foreach (NodeInfo node in nodes)
                {
                    computeNodeForces(node);


                }

                //nodeMutex = true;
            //});


            //Loom.RunAsync(() =>
            //{
                foreach (SpringInfo spring in springs)
                {
                    NodeInfo node1Info = spring.Node1;
                    NodeInfo node2Info = spring.Node2;
                    //Debug.Log(node1Info.gameObject.transform.position);
                    if (initSpringRenderers)
                    {

                        springLineRenderers[spring].vertices[0] = node1Info.WorldPosition;

                        springLineRenderers[spring].vertices[1] = node2Info.WorldPosition;
                    }

                    computeSpringForces(spring);

                }

                //springMutex = true;
            //});

            //Loom.RunAsync(() =>
            //{
                foreach (TriangleInfo triangle in triangles)
                {

                    computeTriangleForces(triangle);

                }

                //triMutex = true;
            //});

            //Loom.QueueOnMainThread(() =>
            //{
                //while (nodeMutex == false
              //          && springMutex == false
              //          && triMutex == false
             //           && renderMutex == false)
             //   {
             //   }
                Vector3[] editVertices = new Vector3[nodeCount];
                
                foreach (NodeInfo node in nodes)
                {
                    IntegrateMotion(node);
                    editVertices[node.Vertex] = node.WorldPosition;
                }

                clothMesh.vertices = editVertices;
                clothMesh.RecalculateNormals();

                
                //integrateMutex = true;
           // });

                //Loom.QueueOnMainThread(() =>
                //{
                //Mesh editMesh = GetComponent<MeshFilter>().mesh;
                // = clothMesh.vertices;
                /*
                for (int i = 0; i < nodeCount; i++)
                {
                    editVertices[i] = vertexNodeDict[i].WorldPosition;
                    //editVectices[i] += new Vector3(0, Random.Range(-0.3f, 0.3f), 0);
                }
                clothMesh.vertices = editVertices;
                clothMesh.RecalculateNormals();
                 */
            //editMesh.vertices = editVectices;
            //editMesh.RecalculateNormals();

            //renderMutex = true;

            //});

            


       // }

        
    }

    private List<Vector3> initNodes()
    {
        List<Vector3> retVector = new List<Vector3>();
        int nodeCount = 0;
        for (int i = 0; i < clothWidth; i++)
        {
            for (int j = 0; j < clothHeight; j++)
            {
                Vector3 newPosition = new Vector3(j, i, 0);

                retVector.Add(newPosition);

                //GameObject newNode = (GameObject)Instantiate(nodePrefab, newPosition, transform.rotation);
                NodeInfo newNodeInfo = new NodeInfo();

                newNodeInfo.GridPosition = new Vector2(j, i);
                newNodeInfo.WorldPosition = new Vector3(j, i, 0);
                newNodeInfo.Mass = nodeMass;
                newNodeInfo.Velocity = Vector3.zero;
                newNodeInfo.Vertex = nodeCount;

                //newNode.transform.parent = nodesParent.transform;
                //newNode.name = "Node " + newNodeInfo.GridPosition;

                nodes.Add(newNodeInfo);

                vertexNodeDict[nodeCount] = newNodeInfo;
                nodeCount = nodeCount + 1;
                
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

        return retVector;
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

    private List<int> initTriangles()
    {
        List<int> triangleVerts = new List<int>();

        for (int i = 0; i < nodes.Count; i++)
        {
            if ((i < (clothWidth * clothHeight) - clothHeight) && ((i % clothWidth) != (clothWidth - 1)))
            {

                //NW, NE, SW
                makeTriangle(nodes[i], nodes[i + 1], nodes[i + clothWidth]);

                triangleVerts.Add(i);
                triangleVerts.Add(i+1);
                triangleVerts.Add(i + clothWidth);

                //NW, NE, SE
                makeTriangle(nodes[i], nodes[i + 1], nodes[i + clothWidth + 1]);

                //NE, SW, SE
                makeTriangle(nodes[i + 1], nodes[i + clothWidth], nodes[i + clothWidth + 1]);

                triangleVerts.Add(i+1);
                triangleVerts.Add(i + clothWidth);
                triangleVerts.Add(i + clothWidth + 1);

                //NW, SW, SE
                makeTriangle(nodes[i], nodes[i + clothWidth], nodes[i + clothWidth + 1]);

            }

        }

        return triangleVerts;
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
            //node.Velocity += acceleration * Time.deltaTime;
            //node.Velocity += acceleration * TIME_STEP;
            node.WorldPosition += node.Velocity * Time.fixedDeltaTime;
            //node.WorldPosition += node.Velocity * Time.deltaTime;
            //node.WorldPosition += node.Velocity * TIME_STEP;
        }
    }
}
