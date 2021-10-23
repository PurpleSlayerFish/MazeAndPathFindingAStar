using System;
using System.Collections.Generic;

using UnityEngine;

public class MazeController : MonoBehaviour
{
    public static MazeController Instance {get; private set;}
    public GameObject WallPrefab                                    =   null;
    public int WallsPoolStartCapacity                               =   50;
    public Vector3 Center                                           =   new Vector3(-1, 0, -1);
    public Vector3 StartPosition {get; private set;}

    private int _length;
    private int _width; 
    private Vector3 _center;
    private List<GameObject> _wallObjectsPool;
    private GameObject _tempWall;
    private float _wallY;
    private bool _isInit                                            =   false;
    private MazeCell[,] _cellsArray;
    private List<MazeCell> _walls;
    private List<MazeCell> _freeCells;
    private GameObject _eastExternalWall;
    private GameObject _westExternalWall;
    private GameObject _northExternalWall;
    private GameObject _southExternalWall;
    
    void Start()
    {
        if ( WallPrefab == null)
        {
            throw new NullReferenceException("WallPrefab bounds not initialized.");
        }
        Init();
    }

    private void Awake()
    {
        if ( Instance != null )
        {
            Debug.LogError("Another instance of MazeController already exists!");
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

    private void InitSize()
    {
        _length = Mathf.RoundToInt(GameController.Instance.MaxPos.z - GameController.Instance.MinPos.z);
        _width = Mathf.RoundToInt(GameController.Instance.MaxPos.x - GameController.Instance.MinPos.x);
    }

    private void GenerateMazeMatrix()
    {        
        _cellsArray = new MazeCell[_width, _length];
        for (int x = 0; x < _cellsArray.GetLength(0); x++)
        {
            for (int z = 0; z < _cellsArray.GetLength(1); z++)
            {
                if ( x % 2 == 0 )
                {
                    if ( z % 2 == 0 )
                        _cellsArray[x, z] = CreateMazeCell(x, z);
                    else
                        _cellsArray[x, z] = CreateMazeWall(x, z);
                }
                else
                {
                    _cellsArray[x, z] = CreateMazeWall(x, z);
                }
            }
        }
    }
    
    private void RemoveWallsWithBacktracker()
    {
        Stack<MazeCell> stack = new Stack<MazeCell>();
        MazeCell next;
        List<MazeCell> unvisitedNeighbours;
        
        MazeCell current = _cellsArray[0, 0];
        current.IsVisited = true;

        do
        {
            unvisitedNeighbours = GetUnvisitedNeighbours(current);
            if  ( unvisitedNeighbours.Count > 0 )
            {
                next = unvisitedNeighbours[UnityEngine.Random.Range(0, unvisitedNeighbours.Count)];
                RemoveWall(current, next);
                next.IsVisited = true;
                stack.Push(next);
                current = next;
            }
            else
            {
                current = stack.Pop();
            }

        } while (stack.Count > 0);
    }

    private List<MazeCell> GetUnvisitedNeighbours(MazeCell current)
    {
        var unvisitedNeighbours= new List<MazeCell>();

        var x = current.X;
        var z = current.Z;

        if ( x > 0 && !_cellsArray[x - 2, z].IsVisited ) 
            unvisitedNeighbours.Add(_cellsArray[x - 2, z]);
        if ( z > 0 && !_cellsArray[x, z - 2].IsVisited ) 
            unvisitedNeighbours.Add(_cellsArray[x, z - 2]);
        if ( x < _cellsArray.GetLength(0) - 2 && !_cellsArray[x + 2, z].IsVisited ) 
            unvisitedNeighbours.Add(_cellsArray[x + 2, z]);
        if ( z < _cellsArray.GetLength(1) - 2 && !_cellsArray[x, z + 2].IsVisited ) 
            unvisitedNeighbours.Add(_cellsArray[x, z + 2]);
            
        return unvisitedNeighbours;
    }

    private void RemoveWall(MazeCell current, MazeCell next)
    {
        if ( current.X == next.X )
            _cellsArray[current.X, Mathf.Min(current.Z, next.Z) + 1].IsWall = false;
        else
            _cellsArray[Mathf.Min(current.X, next.X) + 1, current.Z].IsWall = false;
    }

    private MazeCell CreateMazeCell(int x, int z)
    {
        return new MazeCell {X = x, Z = z};
    }

    private MazeCell CreateMazeWall(int x, int z)
    {
        return new MazeCell {X = x, Z = z, IsWall = true};
    }

    private void InitWallsPool()
    {
        _wallObjectsPool = new List<GameObject>(WallsPoolStartCapacity);
        for (int i = 0; i < WallsPoolStartCapacity; i++)
            GenerateWall();
    }

    private GameObject GenerateWall()
    {
        _tempWall = CreateWall();
        _tempWall.SetActive(false);
        _wallObjectsPool.Add(_tempWall);
        return _tempWall;
    }

    private GameObject GetWall(){
        _tempWall = GetFreeWall();
        return _tempWall == null ? GenerateWall() : _tempWall;
    }

    private GameObject GetFreeWall()
    {
        return _wallObjectsPool.Find ( x => !x.activeSelf );
    }

    private void ClearExistedWalls()
    {
        foreach (var pathPoint in _wallObjectsPool.FindAll( x => x.activeSelf ))
        {
            pathPoint.SetActive(false);
        }
    }

    private void FilterWalls()
    {
        MazeCell cell;
        _walls = new List<MazeCell>();
        for (int x = 0; x < _cellsArray.GetLength(0); x++)
        {
            for (int z = 0; z < _cellsArray.GetLength(1); z++)
            {
                cell = _cellsArray[x, z];
                if ( cell.IsWall )
                    _walls.Add(cell);
            }
        }
    }

    private void FilterFreeCells()
    {
        MazeCell cell;
        _freeCells = new List<MazeCell>();
        for (int x = 0; x < _cellsArray.GetLength(0); x++)
        {
            for (int z = 0; z < _cellsArray.GetLength(1); z++)
            {
                cell = _cellsArray[x, z];
                if (!cell.IsWall)
                    _freeCells.Add(cell);
            }
        }
    }
    
    private void DrawWalls()
    {
        foreach(var wall in _walls)
        {
            _tempWall = GetWall();
            _tempWall.transform.position = InitWorldPosition(wall);
            _tempWall.SetActive(true);
        }
    }

    private void InitStartPosition()
    {
        var cell = _freeCells[UnityEngine.Random.Range(0, _freeCells.Count)];
        StartPosition = InitWorldPosition(cell);
    }

    private Vector3 InitWorldPosition(MazeCell cell)
    {
        return new Vector3(
                Mathf.RoundToInt(GameController.Instance.MinPos.x + cell.X * GameController.TILE_SIZE)
                , _wallY
                , Mathf.RoundToInt(GameController.Instance.MinPos.z + cell.Z * GameController.TILE_SIZE)
            );
    }

    private GameObject CreateWall()
    {
        return Instantiate(WallPrefab, transform);
    }


    private void GenerateExternalWalls()
    {
        _eastExternalWall = CreateWall();
        _westExternalWall = CreateWall();
        _northExternalWall = CreateWall();
        _southExternalWall = CreateWall();
    }

    private void InitExternalWalls()
    {
        InitExternalWallsPosition();
        InitExternalWallsScale();
    }

    private void InitExternalWallsPosition()
    {
        var minPos = GameController.Instance.MinPos;
        var maxPos = GameController.Instance.MaxPos;

        _westExternalWall.transform.position = new Vector3(minPos.x - GameController.TILE_SIZE, _wallY , _center.z);
        _eastExternalWall.transform.position = new Vector3(maxPos.x, _wallY, _center.z);
        _northExternalWall.transform.position = new Vector3(_center.x, _wallY, maxPos.z);
        _southExternalWall.transform.position = new Vector3(_center.x, _wallY, minPos.z - GameController.TILE_SIZE);
    }

    private void InitExternalWallsScale()
    {
        var widthSmall = WallPrefab.transform.localScale.x;
        var lengthSmall = WallPrefab.transform.localScale.z;
        var height = WallPrefab.transform.localScale.y;
        var widthLarge = _width;
        var lengthLarge = _length + GameController.TILE_SIZE * 2;

        _eastExternalWall.transform.localScale = new Vector3(widthSmall, height, lengthLarge);
        _westExternalWall.transform.localScale = new Vector3(widthSmall, height, lengthLarge);
        _northExternalWall.transform.localScale = new Vector3(widthLarge, height, lengthSmall);
        _southExternalWall.transform.localScale = new Vector3(widthLarge, height, lengthSmall);
    }

    private void InitMazeCenter()
    {
        _center = Center;
    }

    public void Init()
    {
        if (!_isInit)
        {
            InitWallsPool();
            GenerateExternalWalls();
            InitMazeCenter();
            _wallY = WallPrefab.transform.position.y;
            _isInit = true;
        }
    }

    public void Generate()
    {
        ClearExistedWalls();
        InitSize();
        GenerateMazeMatrix();
        RemoveWallsWithBacktracker();
        FilterWalls();
        FilterFreeCells();
        DrawWalls();
        InitStartPosition();
        InitExternalWalls();
    }
}
