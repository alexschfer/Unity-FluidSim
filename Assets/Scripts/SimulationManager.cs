using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    FluidVolume FluidVolume;

    void Start()
    {
        FluidVolume = new FluidVolume(256, 0, 0, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
