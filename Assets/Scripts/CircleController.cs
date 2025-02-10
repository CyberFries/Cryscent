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
    public float isConvex; //true生成凸多边形，false生成凹多边形

    private LineRenderer lineRenderer;
    private Vector3[] positions;
    private List<Vector2> vertices = new List<Vector2>();

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
            GenerateNGon();

        if (radiusConnection == true)
            GenerateLRPolygon();

        lineRenderer.SetPositions(positions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateNGon()
    {
        positions = new Vector3[sides];
        lineRenderer.positionCount = positions.Length;

        for (int i = 0; i < positions.Length; i++)
        {
            float phi = 2 * Mathf.PI / sides * i + (rotate * Mathf.Deg2Rad);
            positions[i] = new Vector3(transform.position.x + radius * Mathf.Cos(phi),
                                       transform.position.y + radius * Mathf.Sin(phi),
                                       0); //详情见宝石公式
        }
    }

    private void GenerateLRPolygon()
    {
        vertices.Clear();

        float RadRotateL = rotateL * Mathf.Deg2Rad;
        float RadRotateR = rotateR * Mathf.Deg2Rad;
    }

    private Vector3 CalculateIntersection(float theta)
    {
        float radRotate = rotate * Mathf.Deg2Rad; //公式中的γ
        float radTheta = theta * Mathf.Deg2Rad; //公式中的θ

        //计算边索引k
        float adjustedAngleL = (radTheta - radRotate + 2 * Mathf.PI) % (2 * Mathf.PI);
        int k = Mathf.FloorToInt(adjustedAngleL * sides / (2 * Mathf.PI)) + 1;
        k %= sides;

        //计算公式的分母
        float denominatorAngle = radTheta - radRotate - (2 * k - 1) * Mathf.PI / sides;
        float denominator = Mathf.Cos(denominatorAngle);

        //计算交点坐标
        float scale = Mathf.Cos(Mathf.PI / sides) / denominator;
        return new Vector3(
            transform.position.x + radius * Mathf.Cos(radTheta) * scale,
            transform.position.y + radius * Mathf.Sin(radTheta) * scale,
            0);
    }
}
