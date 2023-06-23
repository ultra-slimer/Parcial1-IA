using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : GridEntity
{
    // se tuvo que crear nuevo codigo, para poder distinguir la comida de los boids
    //se intento usar tags, pero no funcionaba...
    //IA2 - P2 -----------------------------------------------------------------------
    private void Start()
    {
        GameManager.instance.SG.AddEntity(this);
    }
}
