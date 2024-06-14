using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RootController : MonoBehaviour
{
    public bool useJobs;

    public GameObject prefab;
    public Vector2Int gridSize;

    public Button AStarBtn;
    public Button SetBlockBtn;

    private Node[] map;
    private Node start;
    private bool isStart;
    private bool isSetBlock;


    private void Start()
    {
        CreateGrid();
        AStarBtn.onClick.AddListener(() =>
        {
            ClearBtnColor();
            isStart = true;
            isSetBlock = false;
            AStarBtn.GetComponent<Image>().color = Color.green;
        });
        SetBlockBtn.onClick.AddListener(() =>
        {
            ClearColor(Color.red);
            ClearBtnColor();
            isStart = false;
            isSetBlock = true;
            SetBlockBtn.GetComponent<Image>().color = Color.green;
        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (isSetBlock)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(ray, out var obj)) return;

                obj.collider.GetComponent<MeshRenderer>().material.color = Color.red;

                var index = int.Parse(obj.collider.gameObject.name);

                var node = map[index];
                node.type = NodeType.Structure;
                map[index] = node;
            }
            else
            {
                if (isStart)
                {
                    ClearColor(Color.red);

                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (!Physics.Raycast(ray, out var obj)) return;

                    obj.collider.GetComponent<MeshRenderer>().material.color = Color.yellow;

                    start = map[int.Parse(obj.collider.gameObject.name)];

                    isStart = false;
                }
                else
                {
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (!Physics.Raycast(ray, out var obj)) return;
                    var end = map[int.Parse(obj.collider.gameObject.name)];

                    Node[] path;

                    if (useJobs)
                    {
                        float startTime = Time.realtimeSinceStartup;

                        using var nativeMap = new NativeArray<Node>(map, Allocator.TempJob);
                        using var returnPath = new NativeList<Node>(Allocator.TempJob);

                        var jop = new AStarJob(returnPath, nativeMap, new int2(gridSize.x, gridSize.y), start, end);
                        jop.Schedule().Complete();

                        path = returnPath.ToArray();

                        Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
                    }
                    else
                    {
                        float startTime = Time.realtimeSinceStartup;

                        var objectMap = new List<ObjectNode>();
                        foreach (var item in map)
                        {
                            var node = new ObjectNode()
                            {
                                index = item.index,
                                pos = new Vector2Int(item.pos.x, item.pos.y),
                                type = item.type,
                                parentIndex = item.parentIndex,
                                g = item.g,
                                h = item.h,
                            };
                            objectMap.Add(node);
                        }
                        var returnPath = new List<ObjectNode>();

                        var aStar = new AStar(returnPath, objectMap, gridSize, objectMap[start.index], objectMap[end.index]);
                        aStar.Execute();

                        path = new Node[returnPath.Count];

                        for (int i = 0; i < returnPath.Count; i++)
                        {
                            var node = new Node()
                            {
                                index = returnPath[i].index,
                            };
                            path[i] = node;
                        }

                        Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
                    }

                    foreach (var step in path)
                    {
                        var go = transform.Find(step.index.ToString());
                        go.GetComponent<MeshRenderer>().material.color = Color.green;
                    }

                    isStart = true;
                }
            }
        }
    }

    private void CreateGrid()
    {
        map = new Node[gridSize.x * gridSize.y];

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                var obj = Instantiate(prefab, transform);
                obj.transform.position = new Vector3(x * 2, y * 2, 0);
                var index = GetIndex(new Vector2Int(x, y));
                obj.name = index.ToString();
                map[index] = new Node()
                {
                    index = index,
                    pos = new int2(x, y),
                    type = NodeType.Road,
                    g = int.MaxValue,
                    parentIndex = -1,
                };
            }
        }
    }

    private int GetIndex(Vector2Int pos)
    {
        return pos.x + pos.y * gridSize.x;
    }

    private void ClearColor(Color exception)
    {
        foreach (Transform go in transform)
        {
            var mat = go.GetComponent<MeshRenderer>().material;
            if (mat.color == exception) continue;
            mat.color = Color.white;
        }
    }

    private void ClearBtnColor()
    {
        AStarBtn.GetComponent<Image>().color = Color.white;
        SetBlockBtn.GetComponent<Image>().color = Color.white;
    }
}
