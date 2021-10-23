using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const string FLOOR_LAYER_MASK           =   "Floor";
    public Transform Player                         =   null;
    public bool NeedToDrawPath                      =   true;
    public AlgorithmAAsterisk AlgorithmAAsterisk {get; private set;}

    private int _layerMask;
    private Ray _ray;
    private RaycastHit _hit;
    private MovementComponent _movementComponent;
    
    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if ( Input.GetMouseButtonDown(0) && TryRaycastFloor() )
        {
            BuildPath();
        }
    }

    private bool TryRaycastFloor()
    {
        _ray = GameController.Instance.PlayerCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(_ray, out _hit , GameController.Instance.PlayerCamera.farClipPlane, _layerMask);
    }

    private void BuildPath()
    {
        if (AlgorithmAAsterisk.TryToFindPath(Player.position, _hit.point))
        {
            if (NeedToDrawPath) 
            {
                PathIllustrator.Instance.DrawPath(AlgorithmAAsterisk.Path);
            }
            _movementComponent.InitPath(AlgorithmAAsterisk.Path, NeedToDrawPath);
        }
    }

    public void Init()
    {
        _movementComponent = GetComponent<MovementComponent>();

        if ( _movementComponent == null)
        {
            throw new NullReferenceException("MovementComponent not initialized.");
        }

        _movementComponent.Init(Player);
        AlgorithmAAsterisk = new AlgorithmAAsterisk();
        _layerMask = LayerMask.GetMask(FLOOR_LAYER_MASK);
    }

    public void ResetPathPath()
    {
        if ( NeedToDrawPath )
            PathIllustrator.Instance.ClearExistedPath();
        _movementComponent.InitPath(null, NeedToDrawPath);
    }
}