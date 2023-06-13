using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Boid : MonoBehaviour
{
    private Vector3 _velocity;
    public float maxSpeed;
    [Range(0, 0.1f)]
    public float maxForce;

    [Header("Flocking")]
    public float viewRadius;
    public float separationRadius;
    public static List<Boid> allBoids = new List<Boid>();
    [Range(0f, 2.5f)]
    public float separationWeight = 1;
    private float _separationWeight;
    [Range(0f, 2.5f)]
    public float cohesionWeight = 1;
    private float _cohesionWeight;
    [Range(0f, 2.5f)]
    public float alignmentWeight = 1;
    private float _alignmentWeight;

    [Header("Arrive")]
    public Transform target;
    public GameObject gameTarget;
    public float arriveRadius;
    [Range(0f, 2.5f)]
    public float arriveWeight;
    private float _arriveWeight;
    public bool arriving;

    [Header("Evade")]   
    public float evadeRadius;
    bool _evading;
    public Agent evadeTarget;
    public Transform Hunter;
    [Range(0f, 2.5f)]
    public float evadeWeight;
    private float _evadeWeight;



    void Start()
    {
        allBoids.Add(this);

        Vector3 random = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        AddForce(random.normalized * maxSpeed);

       
    }


    void Update()
    {
        //AddForce(Separation() * separationWeight);
        //AddForce(Alignment() * alignamentWeight);
        //AddForce(Cohesion() * cohesionWeight);
        //Esto se puede hacer en una linea de codigo ;)
        var evade = -Evade() * _evadeWeight;
        var arrive = Arrive() * _arriveWeight;
        /*if (evade != Vector3.zero)
        {
            AddForce(evade);
        }
        if (arrive != Vector3.zero)
        {
            AddForce(arrive);
        }*/
        if (evade != Vector3.zero || arrive != Vector3.zero)
        {
            _alignmentWeight = 0;
            _cohesionWeight = 0;
            _separationWeight = 0;
            if(evade != Vector3.zero)
            {
                AddForce(evade);
            }
            if (arrive != Vector3.zero)
            {
                AddForce(arrive);
            }
        }
        else
        {
            _alignmentWeight = alignmentWeight;
            _cohesionWeight = cohesionWeight;
            _separationWeight = separationWeight;
        }



        AddForce(Separation() * _separationWeight + Alignment() * _alignmentWeight + Cohesion() * _cohesionWeight);

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;

        CheckBounds();
    }

    Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;

        desired = allBoids.Where(boid => (boid.transform.position - transform.position).magnitude <= viewRadius).Aggregate(desired,(curent, boids) => curent + (boids.transform.position - transform.position));

        /* anterior
        foreach (Boid boid in allBoids)
        {

            if (boid == this) continue;

            Vector3 dist = boid.transform.position - transform.position;
            if (dist.magnitude <= viewRadius)
            {
                desired += dist;
            }
        }
        */
        if (desired == Vector3.zero) return desired;
        

        desired *= -1;

        return CalculateSteering(desired);
    }

    Vector3 Alignment()
    {
        Vector3 desired = Vector3.zero;

        desired = allBoids.Where(boids => Vector3.Distance(transform.position, boids.transform.position) <= viewRadius).Select(boids => boids.GetVelocity()).Aggregate(desired, (current, boids) => current + boids);
        /* anterior
        int count = 0;
        foreach (var item in allBoids)
        {
            if (item == this) continue;
            if (Vector3.Distance(transform.position, item.transform.position) <= viewRadius)
            {
                desired += item._velocity;
                count++;
            }
        }
        if (count == 0) return desired;
        desired /= count;
        */

        if (desired == Vector3.zero) return desired;
   
        desired /= allBoids.Count;

        return CalculateSteering(desired);
    }

    Vector3 Cohesion()
    {
        Vector3 desired = Vector3.zero;

        desired = allBoids.Where(boids => Vector3.Distance(transform.position, boids.transform.position) <= viewRadius).Select(bois => bois.transform.position).Aggregate(desired, (current, boids) => current + boids);

        /*
        int count = 0;
        foreach (var item in allBoids)
        {
            if (item == this) continue;

            if (Vector3.Distance(transform.position, item.transform.position) <= viewRadius)
            {
                desired += item.transform.position;
                count++;
            }
        }       
        if (count == 0) return desired;
        */
       
        desired -= transform.position;

        return CalculateSteering(desired);
    }

    

    Vector3 Arrive()
    {
        foreach (var item in GameManager.instance.allfoods)
        {
            if (Vector3.Distance(transform.position, item.transform.position) <= arriveRadius && item != null)
            {

                target = item.transform;
                gameTarget = item;
                _arriveWeight = arriveWeight;
                /*
                                transform.position += _velocity * Time.deltaTime;
                                transform.forward = _velocity;


                                AddForce(Arrive(target.position));

                               */

                CheckBounds();

                break;
            }
        }
        if (target == null)
        {
            _arriveWeight = 0;
            return Vector3.zero;
        }
        else
        {
            Vector3 desired = target.position - transform.position;

            float speed = maxSpeed * (desired.magnitude / arriveRadius);
            desired.Normalize();
            desired *= speed;


            if (desired.magnitude <= 0.3f)
            {
                GameManager.instance.DestroyFood(gameTarget);
            }

            return CalculateSteering(desired);
        }
        /*
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
        */
    }

    Vector3 Evade()
    {
        if (Vector3.Distance(transform.position, Hunter.transform.position) <= evadeRadius)
        {
            _evadeWeight = evadeWeight;
            Vector3 futurePos = evadeTarget.transform.position + evadeTarget.velocity * Time.deltaTime;

            Vector3 desired = futurePos - transform.position;

            //Debug.Log("esquivar");
            Debug.DrawLine(transform.position, futurePos, Color.white);
            desired.Normalize();
            desired *= maxSpeed;
        
       
     
       

            return CalculateSteering(desired);  
        }
        else
        {
            _evadeWeight = 0;
            return Vector3.zero;
        }
             
        
    }

    Vector3 CalculateSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude((desired.normalized * maxSpeed) - _velocity, maxForce);
    }

    void CheckBounds()
    {
        transform.position = GameManager.instance.SetObjectBoundPosition(transform.position);
    }

    void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, maxSpeed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        //arrive
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, arriveRadius);

        //Evade
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);

    }
    public Vector3 GetVelocity()
    {
        return _velocity;
    }

    public void Death(Boid c)
    {
        Destroy(c.gameObject);
        allBoids.Remove(c);
    }
}
