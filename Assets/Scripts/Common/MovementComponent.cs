using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    public float TimeToReachTarget                     =   0.3f;
    private PathNode _currentTarget;
    private int _nodeIndex;
    private Transform _transform;
    private float _elapsedTime;
    private List<PathNode> _path;
    private double _tempX;
    private double _tempZ;
    private bool _needToDrawPath;
    
    
    private void FixedUpdate()
    {
        UpdateLerpTime();
        TryToMove();
        UpdateCurrentTarget();
    }

    private void UpdateCurrentTarget()
    {
        _tempX = Math.Round( (double) _transform.position.x, 2);
        _tempZ = Math.Round( (double) _transform.position.z, 2);

        if ( _currentTarget == null )
            return;
        if ( _tempX != _currentTarget.Position.x || _tempZ != _currentTarget.Position.z )
            return;

        _nodeIndex++;

        if ( _nodeIndex < _path.Count )   
        {
            _currentTarget = _path[_nodeIndex];
            if ( _needToDrawPath ) 
            {
                PathIllustrator.Instance.DisablePathPoint( _currentTarget.PathPoint );
            }
        }
        else
        {
            _currentTarget = null;
        }
        _elapsedTime = 0;    
    }

    private void TryToMove()
    {
        if ( _currentTarget == null )
            return;
        
        _transform.position = Vector3.Lerp(_transform.position, _currentTarget.Position, _elapsedTime);
    }

    private void UpdateLerpTime()
    {
        if ( _currentTarget != null )
            _elapsedTime += Time.fixedDeltaTime/TimeToReachTarget;
    }
    
    public void Init(Transform playerTransform)
    {
        _transform = playerTransform;
    }

    public void InitPath(List<PathNode> path, bool NeedToDrawPath)
    {
        if ( path == null)
        {   
            _currentTarget = null;
            _elapsedTime = 0;
            return;
        }
        _path = path;
        _nodeIndex = 1; 
        _currentTarget = _path[_nodeIndex];
        _elapsedTime = 0;
        _needToDrawPath = NeedToDrawPath;

        if ( _needToDrawPath )
        {
            PathIllustrator.Instance.DisablePathPoint(_path[0].PathPoint);
            PathIllustrator.Instance.DisablePathPoint(_currentTarget.PathPoint);

        }
    }
}
