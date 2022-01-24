using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    [Header("Left")]
    public Camera leftPortalCamera;
    public Material leftCameraMat;

    [Header("Middle")]
    public Camera middlePortalCamera;
    public Material middleCameraMat;

    [Header("Right")]
    public Camera rightPortalCamera;
    public Material rightCameraMat;

    void Start()
    {
        if (leftPortalCamera.targetTexture != null)
        {
            leftPortalCamera.targetTexture.Release();
        }
        leftPortalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        leftCameraMat.mainTexture = leftPortalCamera.targetTexture;
        
        if (middlePortalCamera.targetTexture != null)
        {
            middlePortalCamera.targetTexture.Release();
        }
        middlePortalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        middleCameraMat.mainTexture = middlePortalCamera.targetTexture;

        if (rightPortalCamera.targetTexture != null)
        {
            rightPortalCamera.targetTexture.Release();
        }
        rightPortalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        rightCameraMat.mainTexture = rightPortalCamera.targetTexture;
    }
}
