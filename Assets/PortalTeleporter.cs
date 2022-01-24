using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public enum Power
    {
        Speed, Jump, Dash
    }

    [Header("Effects")]
    public Power power;

    [Header("Setup")]
    public Transform destinationPortal;

    private PlayerController player;
    private bool playerIsOverlapping = false;
    private EndRoomController _endRoomController;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        _endRoomController = FindObjectOfType<EndRoomController>();
    }

    void Update()
    {
        if (playerIsOverlapping)
        {
            float dotProduct = Vector3.Dot(transform.up, (player.transform.position - transform.position));

            if (dotProduct < 0f)
            {
                float rotationDiff = -Quaternion.Angle(transform.rotation, destinationPortal.rotation);
                //player.Rotate(Vector3.up, rotationDiff);

                rotationDiff += 180;
                Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * (player.transform.position - transform.position);
                player.transform.position = destinationPortal.position + positionOffset;

                playerIsOverlapping = false;

                player.GivePower(power);
                _endRoomController.Restart();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerIsOverlapping = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerIsOverlapping = false;
        }
    }
}
