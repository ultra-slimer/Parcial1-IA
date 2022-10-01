using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Range(0f, 2.5f)]
    public float cohesionWeight = 1;
    [Range(0f, 2.5f)]
    public float alignmentWeight = 1;

    [Header("Arrive")]
    public Transform target;
    public float arriveRadius;
    public bool arriving;


   
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

        if (arriving && GameManager.instance.foodPrefab != null)
        {
            
            AddForce(Arrive(GameManager.instance.foodPrefab.transform.position));

            transform.position += _velocity * Time.deltaTime;
            transform.forward = _velocity;

            CheckBounds();

            return;
        }

        AddForce(Separation() * separationWeight + Alignment() * alignmentWeight + Cohesion() * cohesionWeight);

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;

        CheckBounds();
    }

    Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;

        foreach (Boid boid in allBoids)
        {
            if (boid == this) continue;

            Vector3 dist = boid.transform.position - transform.position;
            if (dist.magnitude <= viewRadius)
            {
                desired += dist;
            }
        }
        if (desired == Vector3.zero) return desired;

        desired *= -1;

        return CalculateSteering(desired);
    }

    Vector3 Alignment()
    {
        Vector3 desired = Vector3.zero;
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

        return CalculateSteering(desired);
    }

    Vector3 Cohesion()
    {
        Vector3 desired = Vector3.zero;
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
        desired /= count;
       
        desired -= transform.position;

        return CalculateSteering(desired);
    }

    Vector3 Arrive(Vector3 target)
    {
        Vector3 desired = target - transform.position;
     
        
        if (desired.magnitude <= arriveRadius)
        {
            float speed = maxSpeed * (desired.magnitude / arriveRadius);
            desired.Normalize();
            desired *= speed;
        }
        else
        {
            //return Seek(target);
            desired.Normalize();
            desired *= maxSpeed;
        }
        
        
        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
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

    }
    public Vector3 GetVelocity()
    {
        return _velocity;
    }
}
