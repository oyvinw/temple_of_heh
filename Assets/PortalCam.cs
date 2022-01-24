using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCam : MonoBehaviour
{
    public Transform playerCam;
    public Transform startPortal;
    public Transform endPortal; 

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 playerOffset = playerCam.position - endPortal.position;
        transform.position = startPortal.position + playerOffset;

        float angularDiffPortals = Quaternion.Angle(endPortal.rotation, startPortal.rotation);

        //Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDiffPortals, Vector3.up);
        Vector3 newCamDirection = playerCam.forward; 
        transform.rotation = Quaternion.LookRotation(newCamDirection, Vector3.up);
    }
}
