using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class newAgent : MonoBehaviour
{
    public enum PlayerInputs { IDLE, PATROL, CHASE }
    private NewFSM<PlayerInputs> _myFsm;

    
    [Range(0, 0.1f)]
    public float maxForce = 0.1f;
    private float _energy;
    //public float consumptionTime = 5;
    //[HideInInspector]
    //public float consumptionRate;
    public float maxEnergy = 50;
    //public float rechargeTime = 2.5f;
    [HideInInspector]
    public float rechargeRate;
    public float speed = 5;
    private Vector3 _velocity;
    public Transform[] allWaypoints;
    [Range(1.5f, 3)]
    public float range;
    [Range(0.5f, 1.5f)]
    public float chaseChangeRange;
    private Boid chasedBoid;

    //idle
    private float _timeIdle;
    public float rechargeTime = 2.5f;
    private bool _canRecover = false;

    //patrol
    private float _timePatrol;
    public float consumptionRatePatrol;
    [HideInInspector]
    public float consumptionTimePatrol = 5;
    private int _currentWaypoint = 0;
    private bool _canConsumePatrol = false;

    //chase
    private float _timeChase;
    public float _consumptionTimeChase;
    [HideInInspector]
    public float _consumptionRateChase;
    private bool _canConsumeChase = false;
    private Boid _boid;

    //private Rigidbody _myRb;
    //public Renderer _myRen;

    //public Vector3 velocity { get { return _velocity; } }

    List<Boid> boisCloseEnough = new List<Boid>();

    private void Start()
    {
        _energy = maxEnergy;
        consumptionRatePatrol = _energy / consumptionTimePatrol;
        _consumptionRateChase = _energy / _consumptionTimeChase;
        rechargeRate = _energy / rechargeTime;
    }

    private void Awake()
    {
        var idle = new States<PlayerInputs>("IDLE");
        var patrol = new States<PlayerInputs>("Patrol");
        var chase = new States<PlayerInputs>("Chase");

        //transiciones
        StateConfiguration.Create(idle)
            .SetTransition(PlayerInputs.PATROL, patrol)
            .SetTransition(PlayerInputs.CHASE, chase)
            .Done(); //aplico y asigno

        StateConfiguration.Create(patrol)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.CHASE, chase)
            .Done();

        StateConfiguration.Create(chase)
            .SetTransition(PlayerInputs.IDLE, idle)
            .SetTransition(PlayerInputs.PATROL, patrol)
            .Done();

        //idle
        idle.OnEnter += x =>
        {
            transform.position = transform.position;
            ChangeColor(Color.yellow);
            Debug.Log("Entre a idle");
        };

        idle.OnUpdate += () =>
        {
            //codigo original, se podria cambiar
            _timeIdle += Time.deltaTime;
            if (_canRecover)
            {
                //RecoverEnergy();
                //print(a);
                if (RecoverEnergy())
                {
                    _timeIdle = 0;
                    _canRecover = false;
                }
                else
                {
                    SendInputToFSM(PlayerInputs.PATROL);
                }
            }
            else if (_timeIdle >= rechargeTime)
            {
                _canRecover = true;
            }
        };

        idle.OnExit += x =>
        {
            Debug.Log("Sali de Idle");
        };

        //patrol
        patrol.OnEnter += x =>
        {
            ChangeColor(Color.blue);
            Debug.Log("Entre a Patrol");
        };

        patrol.OnUpdate += () =>
        {
            
            Transform nextWaypoint = allWaypoints[_currentWaypoint];
            Vector3 dir = nextWaypoint.position - transform.position;
            transform.forward = dir;

            AddForce(Seek(nextWaypoint.position));
            Move();
            CheckBounds();
            if (dir.magnitude <= 0.3f)
            {
                _currentWaypoint++;
                if (_currentWaypoint > allWaypoints.Length - 1)
                {
                    _currentWaypoint = 0;
                }
            }

            _timePatrol += Time.deltaTime;
            if (_canConsumePatrol)
            {
                var a = ConsumeEnergy(consumptionRatePatrol);
                if (a)
                {
                    _timePatrol = 0;
                    _canConsumePatrol = false;
                }
                else
                {
                    SendInputToFSM(PlayerInputs.IDLE);
                }
            }
            else if (_timePatrol >= consumptionTimePatrol)
            {
                _canConsumePatrol = true;
            }

            /* codigo anterior
            foreach (var item in Boid.allBoids)
            {

                if (Vector3.Distance(transform.position, item.transform.position) <= range)
                {
                    selectBoid(item);
                    SendInputToFSM(PlayerInputs.CHASE);
                }
            }
            */

            //nuevo
            //var boidGet = Boid.allBoids.Where().OrderBy().Take();
            foreach (var boids in Boid.allBoids.Where(a => Vector3.Distance(transform.position, a.transform.position) <= range).OrderBy(a => Vector3.Distance(transform.position, a.transform.position)))
            {
                boisCloseEnough.Add(boids);
            }

            if (boisCloseEnough.Count > 0)
            {
                selectBoid(boisCloseEnough[0]);
                boisCloseEnough.RemoveAt(0);
                SendInputToFSM(PlayerInputs.CHASE);
            }

        };

        idle.OnExit += x =>
        {
            Debug.Log("Sali de Patrol");
        };

        //Chase
        chase.OnEnter += x =>
        {
            ChangeColor(Color.black);
            _consumptionTimeChase *= 0.75f;
            Debug.Log("Entre a Chase");
        };

        chase.OnUpdate += () =>
        {
            AddForce(Pursuit());
            Move();
            CheckBounds();
            CheckClosestChase();
            CheckIfInRange();
            Consume();
        };

        chase.OnExit += x =>
        {
            Debug.Log("sali de Chase");
        };

        //asigno primer estado
        _myFsm = new NewFSM<PlayerInputs>(patrol);
    }

    private void SendInputToFSM(PlayerInputs inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();
        
        /*
        if (_energy > 0)
        {
            SendInputToFSM(PlayerInputs.PATROL);
        }
        else
        {
            SendInputToFSM(PlayerInputs.IDLE);
        }
        */
        
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        SendInputToFSM(PlayerInputs.IDLE);
    }
    public void ChangeColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    public bool ConsumeEnergy(float energyConsuption)
    {
        Debug.Log("Consumiendo " + energyConsuption);
        _energy -= energyConsuption;
        if (_energy > 0)
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

    public Vector3 Seek(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        desired.Normalize();
        desired *= speed;
        Vector3 steering = desired - GetVelocity();
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseChangeRange);
        Gizmos.color = Color.green;
        foreach (var w in allWaypoints)
        {
            Gizmos.DrawLine(transform.position, w.position);
        }
    }

    //Funciones traidas de chase ---------------------------------------------------------
    private void CheckIfInRange()
    {
        var dir = chasedBoid.transform.position - transform.position;

        if (dir.magnitude <= 0.5f)
        {
            var temp = getBoid();
            temp.Death(temp);
            ConsumeEnergy(maxEnergy);
            SendInputToFSM(PlayerInputs.IDLE);
        }

        if (Vector3.Distance(transform.position, chasedBoid.transform.position) > range)
        {
            SendInputToFSM(PlayerInputs.PATROL);
        }

    }
    private void CheckClosestChase()
    {
        foreach (var item in Boid.allBoids)
        {
            if (item == chasedBoid) continue;

            if (Vector3.Distance(transform.position, item.transform.position) <= chaseChangeRange)
            {
                selectBoid(item);
            }
        }
        if (Vector3.Distance(transform.position, chasedBoid.transform.position) > range)
        {
            SendInputToFSM(PlayerInputs.PATROL);
        }

    }

    private void Consume()
    {
        _timeChase += Time.deltaTime;
        if (_canConsumeChase)
        {
            var a = ConsumeEnergy(_consumptionRateChase);
            //print(a);
            if (a)
            {
                _timeChase = 0;
                _canConsumeChase = false;
            }
            else
            {
                //_agent.selectBoid(null);
                SendInputToFSM(PlayerInputs.IDLE); 
            }
        }
        else if (_timeChase >= _consumptionTimeChase)
        {
            _canConsumeChase = true;
        }
    }

    Vector3 Pursuit()
    {
        //print(_boid);
        Vector3 futurePos = chasedBoid.transform.position + chasedBoid.GetVelocity() * Time.deltaTime;

        Vector3 desired = futurePos - transform.position;
        if (futurePos.magnitude >= desired.magnitude) //todo depende que es lo que necesiten
        {
            Debug.DrawLine(transform.position, chasedBoid.transform.position, Color.red);
            return Seek(chasedBoid.transform.position);
        }
        Debug.DrawLine(transform.position, futurePos, Color.white);
        desired.Normalize();
        desired *= speed;

        //Steering
        Vector3 steering = desired - GetVelocity();
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
    }
}
