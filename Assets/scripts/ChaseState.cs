using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IState
{
    private FiniteStateMachine _fsm;
    private Agent _agent;
    private Boid _boid;
    private float _time;
    private float _consumptionTime, _consumptionRate;
    private bool _canConsume = false;
    public ChaseState(FiniteStateMachine finiteStateMachine, Agent a)
    {
        _fsm = finiteStateMachine;
        _agent = a;
        _consumptionRate = a.consumptionRate;
        _consumptionTime = a.consumptionTime * 0.75f;
    }
    public void OnEnter()
    {
        _agent.ChangeColor(Color.black);
        _boid = _agent.getBoid();
        //Debug.LogWarning(_boid);
        //Debug.LogWarning(_agent.getBoid());
    }

    public void OnExit()
    {
        _agent.ChangeColor(Color.white);
    }

    public void OnUpdate()
    {
        //Chase();
        _agent.AddForce(Pursuit());
        _agent.Move();
        _agent.CheckBounds();
        CheckClosestChase();
        Consume();
    }

    private void Chase()
    {
        var dir = _boid.transform.position - _agent.transform.position;
        _agent.transform.forward = dir;

        _agent.transform.position += _agent.transform.forward * _agent.speed * Time.deltaTime;

        if (dir.magnitude <= 0.3f)
        {
            var temp = _agent.getBoid();
            _agent.selectBoid(null);
            temp.Death(temp.gameObject);
            _fsm.ChangeState(AgentStates.Patrol);
        }

        if(Vector3.Distance(_agent.transform.position, _boid.transform.position) > _agent.range)
        {
            //_agent.selectBoid(null);
            _fsm.ChangeState(AgentStates.Patrol);
        }

    }
    private void CheckClosestChase() {
        foreach (var item in Boid.allBoids)
        {
            if (item == _boid) continue;

            if (Vector3.Distance(_agent.transform.position, item.transform.position) <= _agent.chaseChangeRange)
            {
                //desired += item.transform.position;
                //count++;
                _agent.selectBoid(item);
            }
        }
        if (Vector3.Distance(_agent.transform.position, _boid.transform.position) > _agent.range)
        {
            //_agent.selectBoid(null);
            _fsm.ChangeState(AgentStates.Patrol);
        }
    }

    private void Consume()
    {
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
                //_agent.selectBoid(null);
                _fsm.ChangeState(AgentStates.Idle);
            }
        }
        else if (_time >= _consumptionTime)
        {
            _canConsume = true;
        }
    }

    Vector3 Pursuit()
    {
        //pos + velocity * Time
        Vector3 futurePos = _boid.transform.position + _boid.GetVelocity() * Time.deltaTime;

        Vector3 desired = futurePos - _agent.transform.position;
        if (futurePos.magnitude >= desired.magnitude) //todo depende que es lo que necesiten
          {
            Debug.DrawLine(_agent.transform.position, _boid.transform.position, Color.red);
           return _agent.Seek(_boid.transform.position);
        }
        Debug.DrawLine(_agent.transform.position, futurePos, Color.white);
        desired.Normalize();
        desired *= _agent.speed;

        //Steering
        Vector3 steering = desired - _agent.GetVelocity();
        steering = Vector3.ClampMagnitude(steering, _agent.maxForce); 

        //Locomotion
        //AddForce(steering);
        return steering;
    }
}
