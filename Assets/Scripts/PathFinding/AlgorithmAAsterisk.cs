using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using JetBrains.Annotations;

public class AlgorithmAAsterisk
{
    private const string WALLS_LAYER_MASK                           =   "Walls";
    private const float COLLISION_SPHERE_RADIUS                     =   .1f;
    private const float PATH_POINT_Y                                =   1f;
    public List<PathNode> Path {get;  private set;}

    private Vector3 _target;
    private Vector3 _origin;
    private int _layerMask;

    public AlgorithmAAsterisk()
    {
        _layerMask = LayerMask.GetMask(WALLS_LAYER_MASK);
    }
   
    private bool GeneratePath()
    {
        PathNode currentNode;
        PathNode openNode;
        var closedSet = new List<PathNode>();
        var openSet = new List<PathNode>();

        PathNode startNode = InitPathNode(null, _origin);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // var cond = false;
            // if (cond)
            //     return false;

            currentNode = openSet.OrderBy(node => node.EstimateFullPathLength).First();

            if ( currentNode.Position.x == _target.x && currentNode.Position.z == _target.z)
            {
                Path = GetPathForNode(currentNode);
                return true;
            }
                
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            foreach (var neighbourNode in GetNeighbours(currentNode, _target))
            {  
                if ( closedSet.Count(node => node.Position == neighbourNode.Position) > 0 )
                    continue;

                openNode = openSet.FirstOrDefault(node => node.Position == neighbourNode.Position);
                    
                if ( openNode == null )
                {
                    openSet.Add(neighbourNode);
                }
                else if ( openNode.PathLengthFromOrigin > neighbourNode.PathLengthFromOrigin )
                {  
                    openNode.Source = currentNode;
                    openNode.PathLengthFromOrigin = neighbourNode.PathLengthFromOrigin;
                }
            }
        }
        Path = null;
        return false;
    }

    private List<PathNode> GetNeighbours(PathNode node, Vector3 target)
    {
        Vector3 neighbourPos;
        var neighboursPositions = InitNeighboursPositions(node);
        var result = new List<PathNode>();
        
        
        foreach (var position in neighboursPositions)
        {
            if ( !IsInBoundsX(position.x) || !IsInBoundsZ(position.y))
                continue;

            neighbourPos = new Vector3(position.x, PATH_POINT_Y, position.y);
            
            if ( !IsFreePlace(neighbourPos) )
                continue;

            var neighbourNode = InitPathNode(node, neighbourPos);
            result.Add(neighbourNode);
        }
        return result;
    }

    private bool IsInBoundsX(float x)
    {
        return x > GameController.Instance.MinPos.x || x < GameController.Instance.MaxPos.x;
    }

    private bool IsInBoundsZ(float z)
    {
        return z > GameController.Instance.MinPos.z || z < GameController.Instance.MaxPos.z;
    }

    private bool IsFreePlace(Vector3 position){
        return Physics.OverlapSphere(position, COLLISION_SPHERE_RADIUS, _layerMask).Length == 0;
    }

    private List<PathNode> GetPathForNode(PathNode pathNode)
    {
        var result = new List<PathNode>();
        var currentNode = pathNode;
        while (currentNode != null)
        {
            currentNode.PathPoint = PathIllustrator.Instance.GetPathPoint();
            result.Add(currentNode);
            currentNode = currentNode.Source;
        }
        result.Reverse();
        return result;
    }

    private Vector2Int[] InitNeighboursPositions(PathNode node)
    {
        Vector2Int[] neighboursPositions = 
        {
            new Vector2Int(Mathf.RoundToInt(node.Position.x + GameController.TILE_SIZE),    Mathf.RoundToInt(node.Position.z)),
            new Vector2Int(Mathf.RoundToInt(node.Position.x - GameController.TILE_SIZE),    Mathf.RoundToInt(node.Position.z)),
            new Vector2Int(Mathf.RoundToInt(node.Position.x),                                       Mathf.RoundToInt(node.Position.z + GameController.TILE_SIZE)),
            new Vector2Int(Mathf.RoundToInt(node.Position.x),                                       Mathf.RoundToInt(node.Position.z - GameController.TILE_SIZE))
        };

        return neighboursPositions;
    }

    private PathNode InitPathNode([ItemCanBeNull] PathNode node, Vector3 position)
    {
        return new PathNode()
            {
                Position = position,
                Source = node,
                PathLengthFromOrigin = GetPathLength(node),
                HeuristicEstimatePathLength = GetHeuristicPathLength(position, _target)
            };
    }

    private float GetHeuristicPathLength(Vector3 from, Vector3 to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.z - to.z);
    }

    private float GetPathLength(PathNode node)
    {
        return node == null ? 0 : node.PathLengthFromOrigin + GameController.TILE_SIZE;
    }

    public bool TryToFindPath(Vector3 origin, Vector3 target)
    {
        if ( !IsFreePlace(target) )
        {
            Path = null;
            return false;
        }
        _origin = new Vector3(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y), Mathf.RoundToInt(origin.z));
        _target = new Vector3(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Mathf.RoundToInt(target.z));
        return GeneratePath();
    }
}
