using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Viewer : MonoBehaviour
{
    [Range(0,1)]
    [SerializeField] private float _acceleration;
    [SerializeField] private float _speed;
    private ViewerActions _input;

    private Vector3 _movementInput;
    private Vector3 _movement;



    private void Start()
    {
        _input = new ViewerActions();
        _input.Movement.Enable();

        _input.Movement.Move.started += OnMoveInput;
        _input.Movement.Move.performed += OnMoveInput;
        _input.Movement.Move.canceled += OnMoveInput;
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        //Debug.Log("OnMoveInput");
        _movementInput = context.ReadValue<Vector2>();
        _movementInput = new Vector3(_movementInput.x, 0f, _movementInput.y);
    }

    private void Update()
    {
        _movement = Vector3.Lerp(_movement, _movementInput, _acceleration);
        transform.position += _movement * _speed * Time.deltaTime;



    }
}


