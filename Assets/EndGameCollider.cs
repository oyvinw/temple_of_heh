using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerController>().EndGame();
        }
    }
}
