using System;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance {get; private set;}
    public const int TILE_SIZE                                  =   1;
    public Slider WidthSlider                                   =   null;
    public Slider LengthSlider                                  =   null;
    public PlayerController PlayerController                    =   null;
    [Range(3, 20)]
    public int MazeWidth                                        =   10;
    [Range(3, 20)]
    public int MazeLength                                       =   10;
    public Vector3 MinPos {get; private set;}
    public Vector3 MaxPos {get; private set;}
    public Camera PlayerCamera {get; private set;}
    
    private void Start()
    {
        if ( WidthSlider == null)
        {
            throw new NullReferenceException("WidthSlider not initialized.");
        }
        if ( LengthSlider == null)
        {
            throw new NullReferenceException("LengthSlider not initialized.");
        }
        if ( PlayerController == null)
        {
            throw new NullReferenceException("PlayerController not initialized.");
        }
        
        InitGameProcess();
    }

    private void Awake()
    {
        if ( Instance != null )
        {
            Debug.LogError("Another instance of GameController already exists!");
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if ( Instance == this ) {
            Instance = null;
        }
    }

    private void InitBounds()
    {
        MinPos = new Vector3( Mathf.RoundToInt(- MazeWidth * TILE_SIZE), transform.position.y, Mathf.RoundToInt(- MazeLength * TILE_SIZE));
        MaxPos = new Vector3( Mathf.RoundToInt((MazeWidth - 1) * TILE_SIZE), transform.position.y, Mathf.RoundToInt((MazeLength - 1) * TILE_SIZE));
    }
    
    public void InitGameProcess()
    {
        PlayerCamera = Camera.main;
        MazeController.Instance.Init();
        InitBounds();
        MazeController.Instance.Generate();
        PlayerController.Player.position = MazeController.Instance.StartPosition;
    }

    public void RebuildMaze()
    {
        MazeWidth = Mathf.RoundToInt(WidthSlider.value);
        MazeLength = Mathf.RoundToInt(LengthSlider.value);
        InitBounds();
        MazeController.Instance.Generate();
        PlayerController.Player.transform.position = MazeController.Instance.StartPosition;

        PlayerController.ResetPathPath();
    }

}
