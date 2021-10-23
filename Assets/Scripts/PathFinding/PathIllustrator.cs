using System;
using System.Collections.Generic;

using UnityEngine;

public class PathIllustrator : MonoBehaviour
{
    private const string USED_PATH_POINT_NAME                       =   "Used Path Point";
    private const string FREE_PATH_POINT_NAME                       =   "Free Path Point";
    public static PathIllustrator Instance {get; private set;}
    public GameObject PathPointPrefab                               =   null;
    public int PathPointsPoolStartCapacity                          =   20;
    
    private List<GameObject> _pathPointPool;
    private GameObject _tempPathPoint;
    private bool _isPathExist;
    
    void Start()
    {
        if ( PathPointPrefab == null)
        {
            throw new NullReferenceException("PathPointPrefab not initialized.");
        }
        InitPathPointsPool();
        _isPathExist = false;
    }

    private void Awake()
    {
        if ( Instance != null )
        {
            Debug.LogError("Another instance of PathIllustrator already exists!");
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if ( Instance == this )
        {
            Instance = null;
        }
    }
    
    private void DrawPathPoints(List<PathNode> path)
    {
        foreach (var node in path)
        {
            _tempPathPoint = node.PathPoint;
            _tempPathPoint.transform.position = node.Position;
            _tempPathPoint.SetActive(true);
        }
        _isPathExist = true;
    }

    private void InitPathPointsPool()
    {
        _pathPointPool = new List<GameObject>(PathPointsPoolStartCapacity);
        for (int i = 0; i < PathPointsPoolStartCapacity; i++)
            GeneratePathPoint();
    }

    private GameObject GeneratePathPoint()
    {
        _tempPathPoint = Instantiate(PathPointPrefab, transform);
        _tempPathPoint.name = FREE_PATH_POINT_NAME;
        _tempPathPoint.SetActive(false);
        _pathPointPool.Add(_tempPathPoint);
        return _tempPathPoint;
    }

    private GameObject GetFreePathPointObj()
    {
        return _pathPointPool.Find ( x => x.name == FREE_PATH_POINT_NAME );
    }

    public GameObject GetPathPoint(){
        _tempPathPoint = GetFreePathPointObj();
        if ( _tempPathPoint == null)
        {
            _tempPathPoint = GeneratePathPoint();
        }
        _tempPathPoint.name = USED_PATH_POINT_NAME;
        return _tempPathPoint;
    }

    public void DisablePathPoint(GameObject pathPoint)
    {
        pathPoint.SetActive(false);
        pathPoint.name = FREE_PATH_POINT_NAME;
    }

    public void DrawPath(List<PathNode> path)
    {
        ClearExistedPath();
        DrawPathPoints(path);
    }

    public void ClearExistedPath()
    {
        if (!_isPathExist)
            return;

        foreach (var pathPoint in _pathPointPool.FindAll( x => x.activeSelf ))
        {
            DisablePathPoint(pathPoint);
        }

        _isPathExist = false;
    }
}
