using System.Collections;
using System.Collections.Generic;
using Game.Orbit;
using Game.System;
using UnityEngine;

public class TestClick : MonoBehaviour
{
    public void OnMouseDown()
    {
        UserInput.SetSelection(GetComponentInParent<SatelliteOrbit>());
    }
}
