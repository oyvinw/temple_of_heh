using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private bool _isGrounded = false;
    private PlayerController _player;

    private void Start()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Enviroment"))
        //{
            _isGrounded = true;
            _player.NotifyHitGround();
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Enviroment"))
        //{
            _isGrounded = false;
        //}
    }

    public bool GetIsGrounded()
    {
        return _isGrounded;
    }
}
