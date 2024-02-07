using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ObjectBehavior
{
    /// <summary>
    /// Perform some operation according to the level of confidence. 
    /// </summary>
    /// <param name="LoC">An estimate between [0, 1]</param>
    public void GazeOperation(float LoC); 
}
