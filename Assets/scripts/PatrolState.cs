using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;


public class PatrolState : IState
{
    Agent _agent;
    private float _time;
    private float _consumptionTime, _consumptionRate;
    private int _currentWaypoint = 0;
    private FiniteStateMachine _fsm;
    private bool _canConsume = false;

    public PatrolState(Agent a, FiniteStateMachine fsm)
    {
        _agent = a;
        _fsm = fsm;
        _consumptionRate = a.consumptionRate;
        _consumptionTime = a.consumptionTime;
    }

    public void OnEnter()
    {
        _agent.ChangeColor(Color.blue);
        Debug.Log("Entre a Patrol");
    }

    public void OnUpdate()
    {

        Patrol();
        
    }

    void Patrol()
    {
        Transform nextWaypoint = _agent.allWaypoints[_currentWaypoint];
        Vector3 dir = nextWaypoint.position - _agent.transform.position;
        _agent.transform.forward = dir;
        //Debug.Log(nextWaypoint.position + "        " + _agent.transform.position + "        ");
        //_agent.transform.position += _agent.transform.forward * _agent.speed * Time.deltaTime;

        _agent.AddForce(_agent.Seek(nextWaypoint.position));
        _agent.Move();
        _agent.CheckBounds();
        if (dir.magnitude <= 0.3f)
        {
            _currentWaypoint++;
            //Debug.LogWarning("Changed Waypoint to " + _currentWaypoint);
            if (_currentWaypoint > _agent.allWaypoints.Length - 1)
            {
                _currentWaypoint = 0;
            }
        }/*
        if (_canConsume)
        {
            var a = _agent.ConsumeEnergy(_consumptionRate);
            if (a)
            {
                Cooldown();
            }
            else
            {
                _fsm.ChangeState(AgentStates.Idle);
            }
        }*/
        _time += Time.deltaTime;
        if (_canConsume)
        {
            var a = _agent.ConsumeEnergy(_consumptionRate);
            //print(a);
            if (a)
            {
                _time = 0;
                _canConsume = false;
            }
            else
            {
                _fsm.ChangeState(AgentStates.Idle);
            }
        }
        else if (_time >= _consumptionTime)
        {
            _canConsume = true;
        }

        foreach (var item in Boid.allBoids)
        {
            //if (item == this) continue;

            if (Vector3.Distance(_agent.transform.position, item.transform.position) <= _agent.range)
            {
                //desired += item.transform.position;
                //count++;
                _agent.selectBoid(item);
                //Debug.LogWarning(_agent.getBoid());
                _fsm.ChangeState(AgentStates.Chase);
            }
        }
    }

    public void OnExit()
    {
        _agent.ChangeColor(Color.white);
    }
    public IEnumerator Cooldown()
    {
        //Debug.Log("a");
        _canConsume = false;
        yield return new WaitForSeconds(_consumptionTime);
        _canConsume = true;
    }
}
