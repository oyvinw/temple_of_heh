using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndRoomController : MonoBehaviour
{
    [Tooltip("Time duration in seconds")]
    public float duration;

    public Transform leftDoor;
    public Transform rightDoor;
    public float doorWidth = 8000;

    private Vector3 leftStartPos;
    private Vector3 leftEndPos;

    private Vector3 rightEndPos;
    private Vector3 rightStartPos;


    private void Start()
    {
        leftEndPos = leftDoor.position;
        rightEndPos = rightDoor.position;

        leftStartPos = new Vector3(leftEndPos.x - doorWidth, leftEndPos.y, leftEndPos.z);
        rightStartPos = new Vector3(rightEndPos.x + doorWidth, rightEndPos.y, rightEndPos.z);

        Restart();
    }

    public void Restart()
    {
        leftDoor.position = leftStartPos;
        rightDoor.position = rightStartPos;

        StartCoroutine(DoorCloseCoroutine());
    }

    private IEnumerator DoorCloseCoroutine()
    {
        var t = 0f;
        while (t < duration)
        {
            leftDoor.position = Vector3.Lerp(leftDoor.position, leftEndPos, t/duration);
            rightDoor.position = Vector3.Lerp(rightDoor.position, rightEndPos, t/duration);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
