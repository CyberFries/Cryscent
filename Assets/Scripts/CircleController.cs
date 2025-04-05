using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Mathematics;
using UnityEngine;

public class CircleController : MonoBehaviour
{
    [Header("Polygon Parameters")]
    public int sides;
    public float radius;
    public float rotate; //这里的角度为角度制 后期转弧度制了
    public bool radiusConnection; //是否启用L和R
    public float rotateL; //也是角度制
    public float rotateR; //也是角度制

    [Header("Track Parameters")] 
    public float trackLength = 5.0f;
    public float trackThickness = 0.1f;
    public Material trackMaterial;
    public bool clearOldTracks = true;

    private LineRenderer lineRenderer;
    private Vector3[] polyPositions;
    private Vector3[] LRPositions;

    // Start is called before the first frame update
    void Start()
    {
        InitializeLineRenderer();
        if (radiusConnection) GenerateLRPolygon();
        else GenerateNGon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); //别删，不然喜提Unity Pink
        lineRenderer.startColor = Color.white; lineRenderer.endColor = Color.white;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.2f; lineRenderer.endWidth = 0.2f;
    }

    private void GenerateNGon()
    {
        polyPositions = new Vector3[sides];
        lineRenderer.positionCount = polyPositions.Length;

        for (int i = 0; i < polyPositions.Length; i++)
        {
            float phi = 2 * Mathf.PI / sides * i + (rotate * Mathf.Deg2Rad);
            polyPositions[i] = new Vector3(radius * Mathf.Cos(phi),
                                           radius * Mathf.Sin(phi),
                                           0) + transform.position; //宝石公式改编
        }

        lineRenderer.positionCount = sides;
        lineRenderer.SetPositions(polyPositions);

        GenerateTracks();
    }

    private void GenerateLRPolygon()
    {
        List<Vector2> vertices = new List<Vector2>();

        // --- 参数标准化（删除强制覆盖逻辑）---
        float thetaL = NormalizeAngle(rotateL * Mathf.Deg2Rad);
        float thetaR = NormalizeAngle(rotateR * Mathf.Deg2Rad);

        // --- 计算交点 ---
        Vector2 pointL = CalculateIntersection(thetaL);
        Vector2 pointR = CalculateIntersection(thetaR);

        // --- 生成正多边形顶点 ---
        GenerateNGon();
        List<Vector2> polygonVertices = polyPositions
            .Select(p => new Vector2(p.x, p.y))
            .ToList();

        // --- 计算顶点角度 ---
        List<float> polygonAngles = polygonVertices
            .Select(v =>
            {
                Vector2 delta = v - new Vector2(transform.position.x, transform.position.y);
                return NormalizeAngle(Mathf.Atan2(delta.y, delta.x));
            })
            .ToList();

        // --- 筛选中间顶点（支持角度环绕）---
        List<Vector2> middleVertices = new List<Vector2>();
        for (int i = 0; i < polygonVertices.Count; i++)
        {
            float angle = polygonAngles[i];
            if (angle > thetaL)
                middleVertices.Add(polygonVertices[i]);
        }

        for (int i = 0; i < polygonVertices.Count; i++)
        {
            float angle = polygonAngles[i];
            if(angle < thetaR)
                middleVertices.Add(polygonVertices[i]);
        }

        // --- 按逆时针顺序构建顶点 ---
        vertices.Add(transform.position); // 圆心
        vertices.Add(pointL);             // 右侧交点
        vertices.AddRange(middleVertices); // 中间顶点
        vertices.Add(pointR);             // 左侧交点
        vertices.Add(transform.position); // 闭合

        // 转换为Unity坐标数组
        LRPositions = vertices.ConvertAll(v => new Vector3(v.x, v.y, 0)).ToArray();

        //调试
        //Debug.Log($"thetaL: {thetaL}, thetaR: {thetaR}");
        //Debug.Log($"Point L: {pointL}, Point R: {pointR}");
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    Debug.Log($"Vertex {i}: {vertices[i]}");
        //}
        //for(int i = 0; i < polygonVertices.Count; i++)
        //{
        //    Debug.Log($"PolygonPoint {i}: {polygonVertices[i]}");
        //}
        //for(int i = 0; i < polygonAngles.Count; i++)
        //{
        //    Debug.Log($"PolygonAngle {i}: {polygonAngles[i]}");
        //}
        //for (int i = 0; i < middleVertices.Count; i++)
        //{
        //    Debug.Log($"middlePoint {i}: {middleVertices[i]}");
        //}
    }

    private Vector2 CalculateIntersection(float radTheta)
    {
        float radRotate = NormalizeAngle(rotate * Mathf.Deg2Rad);
        radTheta = NormalizeAngle(radTheta);

        // 计算边索引k
        float adjustedAngle = NormalizeAngle(radTheta - radRotate);
        int k = Mathf.FloorToInt(adjustedAngle * sides / (2 * Mathf.PI)); // 删除 +1
        k %= sides;

        // 计算分母角度
        float denominatorAngle = adjustedAngle - (2 * k + 1) * Mathf.PI / sides;
        float denominator = Mathf.Cos(denominatorAngle);

        // 保护分母不为零
        if (Mathf.Abs(denominator) < 1e-6f)
            denominator = Mathf.Sign(denominator) * 1e-6f;

        // 计算缩放系数
        float scale = Mathf.Cos(Mathf.PI / sides) / denominator;

        return new Vector2(
            transform.position.x + radius * Mathf.Cos(radTheta) * scale,
            transform.position.y + radius * Mathf.Sin(radTheta) * scale
        );
    }

    //使角度更改为弧度制 并且标准化角度到[0,2π]范围
    private float NormalizeAngle(float angle)
    {
        angle %= 2 * Mathf.PI;
        return angle < 0 ? angle + 2 * Mathf.PI : angle;
    }

    private void GenerateTracks()
    {
        ClearOldTracks();
        for(int i = 0; i < sides; i++)
        {
            Vector3 start = polyPositions[i];
            Vector3 end = polyPositions[(i + 1) % sides];
            CreateTrack(start, end, i);
        }
    }

    private void CreateTrack(Vector3 start, Vector3 end, int index)
    {
        GameObject track = new GameObject($"Track_{index}");
        track.transform.SetParent(transform);

        Vector3 midPoint = (start + end) / 2;
        track.transform.position = midPoint;
        track.transform.rotation = Quaternion.LookRotation(end - start, Vector3.up);

        BoxCollider collider = track.GetComponent<BoxCollider>();
        collider.size = new Vector3(
            Vector3.Distance(start, end),
            trackThickness,
            trackLength);

        //可视化 下周末(2025.4.12)回来写
    }

    private void ClearOldTracks()
    {
        //下周末(2025.4.12)回来写
    }
}
