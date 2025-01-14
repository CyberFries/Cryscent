using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CircleController : MonoBehaviour
{
    public int sides;
    public float radius;
    public float rotate;

    private LineRenderer lineRenderer;
    private Vector3[] positions;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        positions = new Vector3[sides];
        lineRenderer.positionCount = positions.Length;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); //别删 不然喜提Unity Pink
        lineRenderer.startColor = Color.red; lineRenderer.endColor = Color.cyan;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.2f; lineRenderer.endWidth = 0.2f;

        for (int i = 0; i < positions.Length; i++)
        {
            float phi = 2 * math.PI / sides * i + rotate;
            positions[i] = new Vector3(transform.position.x + radius * math.cos(phi),
                                       transform.position.y + radius * math.sin(phi),
                                       0); //详见宝石公式
        }

        lineRenderer.SetPositions(positions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
