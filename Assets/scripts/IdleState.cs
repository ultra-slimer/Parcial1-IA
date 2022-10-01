using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class IdleState : IState
{
    private float _rechargeTime, _rechargeRate;
    private float _time;
    private bool _canRecover = true;
    private Agent _agent;
    FiniteStateMachine _fsm;

    public IdleState(FiniteStateMachine fsm, Agent agent)
    {
        _fsm = fsm;
        _rechargeRate = agent.rechargeRate;
        _rechargeTime = agent.rechargeTime;
        _agent = agent;
    }

    public void OnEnter()
    {
        _agent.transform.position = _agent.transform.position;
        Debug.Log("Entre a Idle");
    }

    public void OnUpdate()
    {
        //Debug.Log("Actualizando Idle");
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            _fsm.ChangeState(AgentStates.Patrol);
        }*/
        _time += Time.deltaTime;
        if(_canRecover)
        {
            var a = _agent.RecoverEnergy();
            //print(a);
            if (a)
            {
                _time = 0;
                _canRecover = false;
            }
            else
            {
                _fsm.ChangeState(AgentStates.Patrol);
            }
        }
        else if(_time >= _rechargeTime)
        {
            _canRecover = true;
        }
    }

    public void OnExit()
    {
        Debug.Log("Sali de Idle");
    }

    private IEnumerator Cooldown()
    {
        _canRecover = false;
        yield return new WaitForSeconds(_rechargeTime);
        _canRecover = true;
    }
}
