using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineDraw : MonoBehaviour {
    public LineRenderer lineRenderer;
	// Use this for initialization

    public float alphaDelta = 0.1f;

    public float lineLength = 5;
    public float lineWidthStart = 0.01f;
    public float lineWidthEnd = 0.02f;

    public List<Vector3> vertices;

    public Color colorStart = Color.white;
    public Color colorEnd = Color.blue;
    private Color currentColor;
    private float currentWidth;
    private float currentTime;


    public float timeToLerp = 1.0f;


	void Awake () 
    {
        lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetColors(Color.white, Color.white);
        lineRenderer.SetWidth(lineWidthStart, lineWidthStart);
        
        
        currentColor = colorStart;
        currentWidth = lineWidthStart;
	}

    void Start()
    {
        lineRenderer.SetVertexCount(vertices.Count);
    }
	
	// Update is called once per frame
	void Update () 
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            
            lineRenderer.SetPosition(i, vertices[i]);
        }
        
        /*
        float lerpTime = Mathf.PingPong(Time.time, timeToLerp) / timeToLerp;

        
        currentWidth = Mathf.Lerp(lineWidthStart, lineWidthEnd, lerpTime);
        //Debug.Log(lerpTime);
        //Debug.Log(currentWidth);

        if (currentColor == colorStart)
        {
            StartCoroutine(lerpColor(true));
        }

        if (currentColor == colorEnd)
        {
            StopCoroutine("lerpColor");

            StartCoroutine(lerpColor(false));
        }

        if (currentWidth == lineWidthStart)
        {
            StartCoroutine(lerpWidth(true));
        }

        if (currentWidth == lineWidthEnd)
        {
            StopCoroutine("lerpWidth");
            StartCoroutine(lerpWidth(false));
        }

        
        
        //currentColor = Color.Lerp(currentColor, colorEnd, Time.time/100f);
        lineRenderer.SetColors(currentColor, currentColor);
        lineRenderer.SetWidth(currentWidth, currentWidth);

        */
	}

    IEnumerator lerpColor(bool fromStart)
    {
        float tColor = 0.0f;
        float lerpRate = 1.0f / timeToLerp;

        while (tColor < 1.0f)
        {
            tColor += (Time.deltaTime * lerpRate);
            if (fromStart == true)
            {
                currentColor = Color.Lerp(colorStart, colorEnd, tColor);
            }

            else
            {
                currentColor = Color.Lerp(colorEnd, colorStart, tColor);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator lerpWidth(bool fromStart)
    {
        float tWidth = 0.0f;
        float lerpRate = 1.0f / timeToLerp;

        while (tWidth < 1.0f)
        {
            tWidth += (Time.deltaTime * lerpRate);
            if (fromStart == true)
            {
                currentWidth = Mathf.Lerp(lineWidthStart, lineWidthEnd, tWidth);
            }

            else
            {
                currentWidth = Mathf.Lerp(lineWidthEnd, lineWidthStart, tWidth);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    


}
