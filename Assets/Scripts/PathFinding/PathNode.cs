using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class PathNode
{
    public PathNode Source { get; set; }
    public Vector3 Position { get; set; }
    public GameObject PathPoint { get; set; }
    public float HeuristicEstimatePathLength { get; set; }
    public float PathLengthFromOrigin { get; set; }
    public float EstimateFullPathLength {
        get {
            return this.PathLengthFromOrigin + this.HeuristicEstimatePathLength;
        }
    }
    
}
