using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private LineRenderer lineRenderer;
    private Vector3[] polyPositions;
    private Vector3[] LRPositions;

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
        {
            GenerateNGon();
            lineRenderer.SetPositions(polyPositions);
        }

        if (radiusConnection == true)
        {
            GenerateLRPolygon();
            lineRenderer.SetPositions(LRPositions);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateNGon()
    {
        polyPositions = new Vector3[sides];
        lineRenderer.positionCount = polyPositions.Length;

        for (int i = 0; i < polyPositions.Length; i++)
        {
            float phi = 2 * Mathf.PI / sides * i + (rotate * Mathf.Deg2Rad);
            polyPositions[i] = new Vector3(transform.position.x + radius * Mathf.Cos(phi),
                                       transform.position.y + radius * Mathf.Sin(phi),
                                       0); //详情见宝石公式
        }
    }

    private void GenerateLRPolygon()
    {
        List<Vector2> vertices = new List<Vector2>();
        List<Vector2> polygonVertices = new List<Vector2>();
        List<float> polygonAngles = new List<float>();

        //参数设置
        float thetaL = NormalizeAngle(rotateL * Mathf.Deg2Rad);
        float thetaR = NormalizeAngle(rotateR * Mathf.Deg2Rad);
        if (thetaL < thetaR) thetaL = thetaR;

        //计算交点
        Vector2 pointL = CalculateIntersection(thetaL);
        Vector2 pointR = CalculateIntersection(thetaR);

        //获取多边形顶点
        GenerateNGon();
        for (int i = 0; i < sides; i++)
        {
            float phi = 2 * Mathf.PI / sides * i + (rotate * Mathf.Deg2Rad);
            Vector2 v = new Vector2(transform.position.x + radius * Mathf.Cos(phi),
                                    transform.position.y + radius * Mathf.Sin(phi));
            polygonVertices.Add(v);
            polygonAngles.Add(NormalizeAngle(phi));
        }

        //筛选顶点并按角度排序
        List<Vector2> middleVertices = new List<Vector2>();

        for (int i = 0; i < polygonVertices.Count; i++)
        {
            float angle = polygonAngles[i];
            if (angle < thetaR || angle > thetaL)
            {
                middleVertices.Add(polygonVertices[i]);
            }
        }

        //构建最终顶点顺序
        vertices.Add(new Vector2(transform.position.x, transform.position.y));
        vertices.Add(pointL);
        if (middleVertices.Count > 0)
            vertices.AddRange(middleVertices);
        vertices.Add(pointR);

        //移到LRPositions中
        LRPositions = new Vector3[vertices.Count];
        lineRenderer.positionCount = LRPositions.Length;
        for (int i = 0; i < LRPositions.Length; i++)
        {
            LRPositions[i] = new Vector3(vertices[i].x, vertices[i].y, 0);
        }

        //调试
        Debug.Log($"thetaL: {thetaL}, thetaR: {thetaR}");
        Debug.Log($"Point L: {pointL}, Point R: {pointR}");
        for (int i = 0; i < vertices.Count; i++)
        {
            Debug.Log($"Vertex {i}: {vertices[i]}");
        }
        for(int i = 0; i < polygonVertices.Count; i++)
        {
            Debug.Log($"PolygonPoint {i}: {polygonVertices[i]}");
        }
        for (int i = 0; i < middleVertices.Count; i++)
        {
            Debug.Log($"middlePoint {i}: {middleVertices[i]}");
        }
    }

    private Vector2 CalculateIntersection(float radTheta)
    {
        float radRotate = NormalizeAngle(rotate * Mathf.Deg2Rad); //公式中的γ

        //计算边索引k
        float adjustedAngleL = NormalizeAngle(radTheta - radRotate);
        int k = Mathf.FloorToInt(adjustedAngleL * sides / (2 * Mathf.PI)) + 1;
        k %= sides;

        //计算公式的分母
        float denominatorAngle = radTheta - radRotate - (2 * k - 1) * Mathf.PI / sides;
        float denominator = Mathf.Cos(denominatorAngle);

        //计算交点坐标
        float scale = Mathf.Cos(Mathf.PI / sides) / denominator;
        return new Vector2(
            transform.position.x + radius * Mathf.Cos(radTheta) * scale,
            transform.position.y + radius * Mathf.Sin(radTheta) * scale);
    }

    //使角度更改为弧度制 并且标准化角度到[0,2π]范围
    private float NormalizeAngle(float angle)
    {
        angle %= 2 * Mathf.PI;
        return angle < 0 ? angle + 2 * Mathf.PI : angle;
    }
}
