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
    [Range(0, 0.1f)]
    public float maxForce = 0.1f;
    private float _energy;
    public float consumptionTime = 5;
    [HideInInspector]
    public float consumptionRate;
    public float maxEnergy = 50;
    public float rechargeTime = 2.5f;
    [HideInInspector]
    public float rechargeRate;
    public float speed = 5;
    private Vector3 _velocity;
    private FiniteStateMachine _FSM;
    public Transform[] allWaypoints;
    [Range(1.5f, 3)]
    public float range;
    [Range(0.5f, 1)]
    public float chaseChangeRange;
    private Boid chasedBoid;

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
        if(_energy > 0)
        {
            _FSM.ChangeState(AgentStates.Patrol);
        }
        else
        {
            _FSM.ChangeState(AgentStates.Idle);
        }
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseChangeRange);
        Gizmos.color = Color.green;
        foreach (var w in allWaypoints){
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

    public void selectBoid(Boid b)
    {
        chasedBoid = b;
        print(chasedBoid);
    }
    public Boid getBoid()
    {
        return chasedBoid;
    }
    public void AddForce(Vector3 force)
    {
        _velocity += force;
        _velocity = Vector3.ClampMagnitude(_velocity, speed);
    }
    public Vector3 GetVelocity()
    {
        return _velocity;
    }


    public void Move()
    {
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
    }

    public void CheckBounds()
    {
        transform.position = GameManager.instance.SetObjectBoundPosition(transform.position);
    }
}
