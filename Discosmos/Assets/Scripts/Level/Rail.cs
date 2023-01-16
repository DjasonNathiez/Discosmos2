using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Rail : MonoBehaviour
{
    [Header("Curve")]
    [SerializeField] private List<RailPoint> railPoints;
    [SerializeField] private  float nbPoints;
    public  float distBetweenNodes;
    private List<Vector3> pointsOnCurve =new List<Vector3>(0);
    public List<Vector3> distancedNodes = new List<Vector3>(0);
    [SerializeField] private bool loop;
    [SerializeField] private List<Transform> forms;

    [Header("Ramp")] 
    [SerializeField] private Vector2[] railVertices;
    public MeshFilter meshFilter;
    public int nbRails;
    public float space;
    public Mesh endPointsMesh;
    public float endPointMeshSize = 1;

    [Header("Tool")] 
    [SerializeField] private bool generateRail;
    [SerializeField] private bool generateRessources;
    public bool addNewPoint;
    public bool removeLastPoint;
    public bool updatePoints;
    public bool inversePoints;
    [SerializeField] private GameObject railPoint;
    [SerializeField] private bool visualizeExitDirections;
    [SerializeField] private bool exitDirectionAuto = true;
    [SerializeField] private float exitDirectionSize = 5;

    [Header("Gameplay")] 
    [SerializeField] private bool playerOnRamp;
    public AnimationCurve speedBoost;
    private float _progressOnRamp;
    private bool down;
    private float downTimer;
    [SerializeField] private float downDelay;
    [SerializeField] private float entryZoneLenght;
    [SerializeField] private PlayerController player;
    public float heightOnRamp;
    private bool transition;
    private float transitionTimer;
    [SerializeField] private float transitionDelay;
    [SerializeField] public Vector3 exitDirectionFirstNode = Vector3.right;
    [SerializeField] public Vector3 exitDirectionLastNode = Vector3.left;

    [Header("Ressources")] 
    [SerializeField] private GameObject ressource;
    [SerializeField] private int ressourcesNb;
    private List<GameObject> ressources;

    [Header("Graphics")] 
    [SerializeField] private Material material;

    [SerializeField] private MeshRenderer meshRenderer;

    private void Start()
    {
        material = new Material(meshRenderer.material);
        material.name = gameObject.name + " Material";
        meshRenderer.material = material;

        if(!player) player = GameAdministrator.localPlayer.controller;
    }

    public void OnExitRamp()
    {
        downTimer = downDelay;
        down = true;
        playerOnRamp = false;
        // Quand le joueur sort de la rampe, on passe en mode downTime
        // On set le slider du material a 1 automatiquement
    }
    
    void OnRampUp()
    {
        down = false;
        transitionTimer = transitionDelay;
        transition = true;
    }

    private void Update()
    {
        if (!playerOnRamp)
        {
            if (down && downTimer <= 0)
            {
                OnRampUp();
            }
            else if (down)
            {

                downTimer -= Time.deltaTime;
                material.SetFloat("_Slider",Mathf.Lerp(material.GetFloat("_Slider"),1.2f,Time.deltaTime*2));
            }
            else
            {
                if (transition)
                {
                    if (transitionTimer > 0)
                    {
                        transitionTimer -= Time.deltaTime;
                        material.SetFloat("_NextColorRevive",1- (transitionTimer / transitionDelay));
                    }
                    else
                    {
                        material.SetFloat("_NextColorRevive",0);
                        material.SetFloat("_Slider",-0.2f);
                        transition = false;
                    }
                }
                else
                {
                    if(Vector3.SqrMagnitude(new Vector3(player.transform.position.x,0,player.transform.position.z) - new Vector3(distancedNodes[0].x,0,distancedNodes[0].z)) <= entryZoneLenght * entryZoneLenght)
                    {
                        Debug.Log("Contact Rail");
                        player.OnEnterRail(this,true,0);
                        material.SetFloat("_Reverse",1);
                        material.SetFloat("_Slider",-0.2f);
                        playerOnRamp = true;
                    }
                    else if (Vector3.SqrMagnitude(new Vector3(player.transform.position.x, 0, player.transform.position.z) - new Vector3(distancedNodes[distancedNodes.Count - 1].x, 0, distancedNodes[distancedNodes.Count - 1].z)) <= entryZoneLenght * entryZoneLenght)
                    {
                        Debug.Log("Contact Rail");
                        player.OnEnterRail(this,false,distancedNodes.Count - 1);
                        material.SetFloat("_Reverse",0);
                        material.SetFloat("_Slider",-0.2f);
                        playerOnRamp = true;
                    }   
                }
            }
        }
        else
        {
            if (playerOnRamp)
            {
                float progress;
                if (player.forwardOnRamp)
                {
                    progress = Mathf.Lerp(-0.1f,0.85f,(player.rampIndex + player.rampProgress) / (distancedNodes.Count-1));   
                }
                else
                {
                    progress = Mathf.Lerp(-0.03f,0.88f,(((distancedNodes.Count-1) - player.rampIndex) + player.rampProgress) / (distancedNodes.Count-1));   
                }
                material.SetFloat("_Slider",progress);   
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 pos in distancedNodes)
        {
            Gizmos.DrawSphere(pos,0.1f);
        }

        if (generateRail)
        {
            meshFilter.mesh = GenerateRail();
            generateRail = false;
        }
        if (addNewPoint)
        {
            AddNewPoint();
            addNewPoint = false;
        }
        if (removeLastPoint)
        {
            RemoveLastPoint();
            removeLastPoint = false;
        }

        if (updatePoints)
        {
            UpdatePoints();
            updatePoints = false;
        }
        
        if (inversePoints)
        {
            InversePoints();
            inversePoints = false;
        }
        DrawRailPoints();
        CreateDistancedNodes();
        
        if (generateRessources)
        {
            generateRessources = false;
        }

        if (visualizeExitDirections)
        {
            Gizmos.DrawRay(distancedNodes[0],exitDirectionFirstNode.normalized*exitDirectionSize);
            Gizmos.DrawRay(distancedNodes[distancedNodes.Count-1],exitDirectionLastNode.normalized*exitDirectionSize);
        }

        if (exitDirectionAuto)
        {
            exitDirectionFirstNode = (distancedNodes[0] - distancedNodes[1]).normalized*exitDirectionSize;
            exitDirectionLastNode = (distancedNodes[distancedNodes.Count - 1] - distancedNodes[distancedNodes.Count - 2]).normalized*exitDirectionSize;
        }
    }

    void AddNewPoint()
    {
        GameObject obj = Instantiate(railPoint, transform.position, quaternion.identity,transform);
        obj.name = "Point" + railPoints.Count;
        RailPoint newPoint = new RailPoint();
        newPoint.point = obj.transform;
        newPoint.previousHandle = obj.transform.GetChild(0);
        newPoint.nextHandle = obj.transform.GetChild(1);
        railPoints.Add(newPoint);
    }
    
    void RemoveLastPoint()
    {
        DestroyImmediate(railPoints[railPoints.Count - 1].point.gameObject);
        railPoints.RemoveAt(railPoints.Count - 1);
    }
    
    void UpdatePoints()
    {
        railPoints.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            RailPoint point = new RailPoint();
            point.point = transform.GetChild(i);
            point.previousHandle = transform.GetChild(i).GetChild(0);
            point.nextHandle = transform.GetChild(i).GetChild(1);
            railPoints.Add(point);
        }
    }
    void InversePoints()
    {
        for (int i = 0; i < railPoints.Count; i++)
        {
            railPoints[i].point.localPosition = railPoints[i].point.localPosition * -1;
            railPoints[i].previousHandle.localPosition = railPoints[i].previousHandle.localPosition * -1;
            railPoints[i].nextHandle.localPosition = railPoints[i].nextHandle.localPosition * -1;
        }
    }


    Mesh GenerateRail()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>(0);
        List<Color> colors = new List<Color>(0);
        List<int> triangles = new List<int>(0);
        int nbPt = 0;

        for (int x = 0; x < nbRails; x++)
        {
            for (int i = 0; i < distancedNodes.Count; i++)
            {
                Vector3 yAxis = Vector3.up;
                Vector3 xAxis;
                if (i != distancedNodes.Count - 1)
                {
                    xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[i + 1] - distancedNodes[i]);
                }
                else if (!loop)
                {
                    xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[i] - distancedNodes[i-1]);
                }
                else
                {
                    xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[0] - distancedNodes[i]);
                }

                xAxis = xAxis.normalized;

                Color color = Color.Lerp(Color.black, Color.white, i / (float) distancedNodes.Count);
                
                for (int j = 0; j < railVertices.Length; j++)
                {
                    Vector3 pos = distancedNodes[i] + (xAxis * railVertices[j].x) + (-xAxis * (space * (nbRails-1 )/ 2) + xAxis * x * (space)) + yAxis * railVertices[j].y;
                    pos = transform.InverseTransformPoint(pos);
                    vertices.Add(pos);
                    colors.Add(color);
                }
            }

            for (int i = 0; i < distancedNodes.Count-1; i++)
            {
                for (int j = 0; j < railVertices.Length; j++)
                {
                    int a = nbPt + (i * railVertices.Length) + j;
                    int b = nbPt + (i * railVertices.Length) + (j+1) % railVertices.Length;
                    int c = nbPt + ((i+1) * railVertices.Length) + j;
                    int d = nbPt + ((i+1) * railVertices.Length) + (j+1) % railVertices.Length;
                    triangles.AddRange(GetTrianglesForQuad(a,b,c,d));
                }
            }

            if (loop)
            {
                for (int j = 0; j < railVertices.Length; j++)
                {
                    int a = nbPt + (distancedNodes.Count-1) * railVertices.Length + j;
                    int b = nbPt + (distancedNodes.Count-1) * railVertices.Length + (j+1)%railVertices.Length;
                    int c = nbPt + j;
                    int d = nbPt + (j+1)%railVertices.Length; 
                    triangles.AddRange(GetTrianglesForQuad(a,b,c,d));
                }
            }

            nbPt = vertices.Count;
        }

        int verticeCount = vertices.Count;
        
        for (int i = 0; i < endPointsMesh.vertexCount; i++)
        {
            vertices.Add(distancedNodes[0] + endPointsMesh.vertices[i] * endPointMeshSize);
            colors.Add(Color.black);
        }
        
        for (int i = 0; i < endPointsMesh.triangles.Length; i++)
        {
            triangles.Add(endPointsMesh.triangles[i] + verticeCount);
        }
        
        verticeCount = vertices.Count;
        
        for (int i = 0; i < endPointsMesh.vertexCount; i++)
        {
            vertices.Add(distancedNodes[distancedNodes.Count-1] + endPointsMesh.vertices[i] * endPointMeshSize);
            colors.Add(Color.white);
        }
        
        for (int i = 0; i < endPointsMesh.triangles.Length; i++)
        {
            triangles.Add(endPointsMesh.triangles[i] + verticeCount);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.colors = colors.ToArray();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            
            Debug.DrawRay(transform.position + mesh.vertices[i],mesh.normals[i],Color.green,10);
        }
        return mesh;
    }

    int[] GetTrianglesForQuad(int a,int b,int c,int d)
    {
        List<int> triangles = new List<int>(0);
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
        triangles.Add(b);
        triangles.Add(d);
        triangles.Add(c);
        return triangles.ToArray();
    }
    
    void CreateDistancedNodes()
    {
        if (distBetweenNodes <= 0) return;
        distancedNodes.Clear();
        distancedNodes.Add(railPoints[0].point.position);
        float totalDist = 0;
        for (int i = 1; i < pointsOnCurve.Count; i++)
        {
            totalDist += Vector3.Distance(pointsOnCurve[i], pointsOnCurve[i - 1]);
        }
        int numberOfNodes =  Mathf.RoundToInt(totalDist / distBetweenNodes);
        float distNode = totalDist / numberOfNodes;
        numberOfNodes--;

        int index = 1;
        Vector3 current = pointsOnCurve[0];
        for (int i = 0; i < numberOfNodes; i++)
        {
            if (Vector3.SqrMagnitude(pointsOnCurve[index] - current) < distNode * distNode)
            {
                float dist = distNode - Vector3.Distance(pointsOnCurve[index], current);
                index++;
                for (int j = 0; j < 500; j++)
                {
                    if (Vector3.SqrMagnitude(pointsOnCurve[index] - pointsOnCurve[index - 1]) < dist * dist)
                    {
                        dist -= Vector3.Distance(pointsOnCurve[index], pointsOnCurve[index - 1]);
                        index++;
                    }
                    else
                    {
                        Vector3 pos = pointsOnCurve[index-1] + (pointsOnCurve[index] - pointsOnCurve[index-1]).normalized * dist;
                        distancedNodes.Add(pos);
                        current = pos;
                        break;
                    }
                }
            }
            else
            {
                Vector3 pos = current + (pointsOnCurve[index] - current).normalized * distBetweenNodes;
                distancedNodes.Add(pos);
                current = pos;
            }
        }
        if(!loop)distancedNodes.Add(railPoints[railPoints.Count-1].point.position);
    }
    
    private void DrawRailPoints()
    {
        pointsOnCurve.Clear();
        for (int i = 0; i < railPoints.Count-1; i++)
        {
            DrawPoints(railPoints[i].point.position,railPoints[i].nextHandle.position,railPoints[i+1].previousHandle.position,railPoints[i+1].point.position);
        }
        if (loop)
        {
            DrawPoints(railPoints[railPoints.Count-1].point.position,railPoints[railPoints.Count-1].nextHandle.position,railPoints[0].previousHandle.position,railPoints[0].point.position);
            pointsOnCurve.Add(railPoints[0].point.position);
        }
        else
        {
            pointsOnCurve.Add(railPoints[railPoints.Count-1].point.position);   
        }
    }

    void DrawPoints(Vector3 a,Vector3 b,Vector3 c,Vector3 d)
    {
        for (int i = 0; i < nbPoints; i++)
        {
            Vector3 pos = QuadraticLerp(a, b, c, d, (1 / nbPoints) * i);
            pointsOnCurve.Add(pos);
        }
    }

    Vector3 DoubleLerp(Vector3 a,Vector3 b,Vector3 c,float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);
        Vector3 abc = Vector3.Lerp(ab, bc, t);
        return abc;
    }

    Vector3 QuadraticLerp(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
    {
        Vector3 abc = DoubleLerp(a, b, c, t);
        Vector3 bcd = DoubleLerp(b, c, d, t);
        Vector3 quadratic = Vector3.Lerp(abc, bcd, t);
        return quadratic;
    }
}

[Serializable]
public class RailPoint
{
    public Transform point;
    public Transform previousHandle;
    public Transform nextHandle;
}

