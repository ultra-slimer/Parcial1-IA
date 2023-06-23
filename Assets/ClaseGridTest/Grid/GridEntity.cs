using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
	public event Action<GridEntity> OnMove = delegate {};
	public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;
    Renderer _rend;
    public IEnumerable<GridEntity> close;

    private void Awake()
    {
        //_rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        //GameManager.instance.SG.AddEntity(this);
    }

    void Update() {
        if (onGrid)
            _rend.material.color = Color.red;
        //else
            //_rend.material.color = Color.gray;
		//Optimization: Hacer esto solo cuando realmente se mueve y no en el update
		//transform.position += velocity * Time.deltaTime;
	    //OnMove(this);
	}

    public void ChangePosition()
    {
        OnMove(this);
    }

    //Esto es mas que nada para la comida, ya que en el GameManager no encontraba manera de mandar la comida como una entidad...
    public void DestroyEntity()
    {
        print("muerto");
        GameManager.instance.SG.EraseEntity(this);
    }


}
