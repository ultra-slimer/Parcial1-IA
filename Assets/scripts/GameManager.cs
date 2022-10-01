using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float boundHeight = 10;
    public float boundWidth = 18;

    public GameObject foodPrefab;


    public static GameManager instance;
    public bool canSpawn;
    public float TimeSpawn;

   
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Update()
    {

        if (canSpawn && foodPrefab != null) StartCoroutine(SpawnerTime());
        else StopCoroutine(SpawnerTime());

      
    }

    public Vector2 SetObjectBoundPosition(Vector2 pos)
    {
        float y = boundHeight / 2;
        float x = boundWidth / 2;
        if (pos.y > y) pos.y = -y;
        if (pos.y < -y) pos.y = y;
        if (pos.x < -x) pos.x = x;
        if (pos.x > x) pos.x = -x;

        return pos;
    }

    

    private void OnDrawGizmos()
    {
        float x = boundWidth / 2;
        float y = boundHeight / 2;
        Vector3 topLeft = new Vector2(-x, y);
        Vector3 topRight = new Vector2(x, y);
        Vector3 botRight = new Vector2(x, -y);
        Vector3 botLeft = new Vector2(-x, -y);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, botRight);
        Gizmos.DrawLine(botRight, botLeft);
        Gizmos.DrawLine(botLeft, topLeft);
    }

    public void Spawn()
    {
        Vector3 randowSpawn = new Vector3(Random.Range(boundWidth / 2, -boundWidth / 2), Random.Range(boundHeight / 2, -boundHeight / 2));
        //Instantiate(foodPrefab, randowSpawn, Quaternion.identity);
        Debug.Log("creando");
    }

    public void DestroyFood(GameObject food)
    {
        Destroy(food);
    }

    

    IEnumerator SpawnerTime()
    {
        yield return new WaitForSeconds(TimeSpawn);
        Spawn();
        yield return new WaitForSeconds(TimeSpawn);
        /*
        Vector3 randowSpawn = new Vector3(Random.Range(boundWidth / 2, -boundWidth / 2), Random.Range(boundHeight / 2, -boundHeight / 2));
        Instantiate(foodPrefab, randowSpawn, Quaternion.identity);      
        Debug.Log("creando");
        */

       
    }
}
