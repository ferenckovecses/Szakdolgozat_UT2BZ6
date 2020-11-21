using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCard : MonoBehaviour
{
	protected bool isDragged;
	protected Vector2 startingPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {

    }

    public virtual void OnCollisionExit2D(Collision2D collision)
    {

    }

    public virtual void PressCard()
    {

    }

    public virtual void ReleaseCard()
    {

    }

    public virtual void StartDrag()
    {
    	isDragged = true;
    	startingPosition = gameObject.transform.position;
    }

    public virtual void EndDrag()
    {
    	isDragged = false;
    	gameObject.transform.position = startingPosition;
    }
}
