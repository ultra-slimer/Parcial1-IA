using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointAgent : MonoBehaviour
{
    public float speed = 5;
    public Transform[] allWaypoints;

    private int _currentWaypoint;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Transform nextWaypoint = allWaypoints[_currentWaypoint];
        Vector3 dir = nextWaypoint.position - transform.position;
        dir.y = 0;
        transform.forward = dir;
        transform.position += transform.forward * speed * Time.deltaTime;

        if (dir.magnitude <= 0.3f)
        {
            _currentWaypoint++;
            if (_currentWaypoint > allWaypoints.Length - 1)
            {
                _currentWaypoint = 0;
            }
        }

    }
}

