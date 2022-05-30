using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchControllerMobile : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public Vector2 TouchDist;
    private Vector2 _pointerOld;
    private int _pointerId;
    public bool IsPressed;
    public void OnPointerDown(PointerEventData eventData) 
    {
        IsPressed = true;
        _pointerId = eventData.pointerId;
        _pointerOld = eventData.position;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }
    private void Update()
    {
        if (IsPressed)
        {
            if (_pointerId >= 0 && _pointerId <= Input.touches.Length)
            {
                TouchDist = Input.touches[_pointerId].position - _pointerOld;
                _pointerOld = Input.touches[_pointerId].position;
                Debug.Log("x" + TouchDist.x + "/y" + TouchDist.y);
            }
            else
            {
                TouchDist = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                _pointerOld = Input.mousePosition;
            }
        }
        else
            TouchDist = new Vector2();
    }
}
