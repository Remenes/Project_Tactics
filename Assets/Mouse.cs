using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Mouse {
    
    public static bool LeftClicked { get { return Input.GetMouseButtonDown(0); } }
    public static bool RightClicked { get { return Input.GetMouseButtonDown(1); } }
    public static bool LeftHeld { get { return Input.GetMouseButton(0); } }
    public static bool Rightheld { get { return Input.GetMouseButton(1); } }

}
