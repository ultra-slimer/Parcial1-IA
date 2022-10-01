using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentStates
{
    Idle,
    Patrol,
    Chase
}

public class Agent : MonoBehaviour
{
    private float _energy;
    public float consumptionTime;
    [HideInInspector]
    public float consumptionRate;
    public float maxEnergy = 50;
    public float rechargeTime = 5;
    [HideInInspector]
    public float rechargeRate;
    public float speed = 5;
    private FiniteStateMachine _FSM;
    public Transform[] allWaypoints;
    [Range(0.5f, 3)]
    public float range;

    // Start is called before the first frame update
    void Start()
    {
        _energy = maxEnergy;
        consumptionRate = _energy / consumptionTime;
        print(consumptionRate);
        rechargeRate = _energy / rechargeTime;
        print(rechargeRate);
        _FSM = new FiniteStateMachine();
        var idle = new IdleState(_FSM, this);
        var chase = new ChaseState(_FSM, this);
        _FSM.AddState(AgentStates.Idle, idle);
        _FSM.AddState(AgentStates.Patrol, new PatrolState(this, _FSM));
        _FSM.AddState(AgentStates.Chase, chase);

        _FSM.ChangeState(AgentStates.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        _FSM.Update();
    }

    public void ChangeColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        foreach(var w in allWaypoints){
            Gizmos.DrawLine(transform.position, w.position);
        }
    }

    public bool ConsumeEnergy(float energyConsuption)
    {
        Debug.Log("Consumiendo " + energyConsuption);
        _energy -= energyConsuption;
        if(_energy > 0)
        {
            Debug.Log("Consumiendo " + true);
            return true; //on true it continues
        }
        else
        {
            Debug.Log("Consumiendo " + false);
            _energy = 0;
            return false;
        }
    }

    public bool RecoverEnergy()
    {
        Debug.Log("Recuperando " + rechargeRate);
        _energy += rechargeRate;
        if (_energy > maxEnergy)
        {
            _energy = maxEnergy;
            Debug.Log("Recuperando " + false);
            return false;
        }
        else
        {
            Debug.Log("Recuperando " + true);
            return true; //on true it continues
        }
    }
}
