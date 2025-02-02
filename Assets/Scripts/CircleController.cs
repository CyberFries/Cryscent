using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CircleController : MonoBehaviour
{
    public int sides;
    public float radius;
    public float rotate; //这里的角度为角度制 后期转弧度制了
    public bool radiusConnection; //是否启用L和R
    public float rotateL; //也是角度制
    public float rotateR; //也是角度制

    private float rRotate;
    private float rRotateL;
    private float rRotateR;
    private LineRenderer lineRenderer;
    private Vector3[] positions;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); //别删，不然喜提Unity Pink
        lineRenderer.startColor = Color.red; lineRenderer.endColor = Color.cyan;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.2f; lineRenderer.endWidth = 0.2f;

        if (radiusConnection == false)
            GenerateNGon(lineRenderer, sides);

        lineRenderer.SetPositions(positions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateNGon(LineRenderer lr, int sides)
    {
        positions = new Vector3[sides];
        lineRenderer.positionCount = positions.Length;

        for (int i = 0; i < positions.Length; i++)
        {
            float phi = 2 * math.PI / sides * i + (rotate * Mathf.Deg2Rad);
            positions[i] = new Vector3(transform.position.x + radius * math.cos(phi),
                                       transform.position.y + radius * math.sin(phi),
                                       0); //详情见宝石公式
        }
    }

    //private
}
