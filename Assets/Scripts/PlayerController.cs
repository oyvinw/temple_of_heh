using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PortalTeleporter;

public class PlayerController : MonoBehaviour
{
    [Header("Mouse Settings")]
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));
    [Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;
    [Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;
    public float mousePitchLimit = 85;

    [Header("Movement Settings")]
    public AnimationCurve movementCurve;
    public float acceleration = 10000f;
    public float maxSpeed = 200f;
    public float strafeRatio = 0.75f;
    public float sprintSpeed = 1.5f;

    [Header("Dash Settings")]
    public float dashForce = 1000f;
    public float dashWindow = 0.5f;
    public float dashDuration = 3f;

    [Header("Jump Settings")]
    public float fallMultiplier = 2f;
    public float jumpVelocity = 4f;
    public float dashJumpMultiplier = 1.5f;
    public float airStrafeFactor = 0.2f;
    public float airStrafeAmount = 20000f;

    [Header("PowerUps")]
    public float jumpGive = 1000f;
    public float speedGive = 3000f;
    public float dashGive = 1f;

    [Header("Sound")]
    public float timeBetweenSteps;
    public AudioClip[] run;
    public AudioClip dash;
    public AudioClip land;
    public AudioClip jump;
    public AudioClip endMusic;

    [Header("Setup")]
    public Animator fadeToWhite;
    public PhysicMaterial noFriction;
    public PhysicMaterial friction;
    public ParticleSystem[] dashParticles;
    public Transform startPortal;

    //Private
    private bool _gameOver;
    private float timeBetweenStepsOG;
    private float timebetweenSprintSteps;
    private AudioClip _lastClip;
    private AudioSource _audioSource;
    private float timer;
    private bool dashWindowOpen;
    private bool isDashing;
    private Rigidbody _rigi;
    private float leftOverAirStrafe;
    private Camera _playerCamera;
    private GroundChecker _groundChecker;
    private bool _canMove = true;
    private CapsuleCollider _collider;
    private CameraState targetCameraState = new CameraState();
    private CameraState interpolatingCameraState = new CameraState();

    void Start()
    {
        _rigi = GetComponent<Rigidbody>();
        _playerCamera = GetComponentInChildren<Camera>();
        _groundChecker = GetComponentInChildren<GroundChecker>();
        _collider = GetComponent<CapsuleCollider>();
        _audioSource = GetComponent<AudioSource>();
        timeBetweenStepsOG = timeBetweenSteps;
        timebetweenSprintSteps = timeBetweenSteps / sprintSpeed;
    }
    void OnEnable()
    {
        targetCameraState.SetFromTransform(transform);
        targetCameraState.mousePitchLimit = mousePitchLimit;

        interpolatingCameraState.SetFromTransform(transform);
        interpolatingCameraState.mousePitchLimit = mousePitchLimit;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        if (!_gameOver)
        {
            Cursor.lockState = CursorLockMode.Locked;

            if (Input.GetKeyDown(KeyCode.R))
            {
                transform.position = startPortal.position;
                _playerCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            MouseLook();
            Move();
            Jump();
        }
    }

    private void Move()
    {
        var totalSpeed = acceleration;

        //Sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalSpeed *= sprintSpeed;
            timeBetweenSteps = timebetweenSprintSteps;
        }
        else
        {
            timeBetweenSteps = timeBetweenStepsOG;
        }

        //Dash
        if (Input.GetKey(KeyCode.LeftControl))
        {
            _collider.material = noFriction;
            _canMove = false;
        }

        else
        {
            _collider.material = friction;
            _canMove = true;
        }

        if (_canMove)
        {
            var localVelocity = transform.InverseTransformDirection(_rigi.velocity);

            var zAccAttack = movementCurve.Evaluate(Mathf.Abs(localVelocity.z) / maxSpeed);
            var xAccAttack = movementCurve.Evaluate(Mathf.Abs(localVelocity.x) / maxSpeed);

            float verticalMove = Input.GetAxisRaw("Vertical") * (totalSpeed * zAccAttack) * Time.deltaTime;
            float horizontalMove = Input.GetAxisRaw("Horizontal") * ((acceleration * strafeRatio) * xAccAttack) * Time.deltaTime;

            var forwardDirection = _playerCamera.transform.forward;
            forwardDirection.y = 0;
            forwardDirection.Normalize();

            var rightDirection = _playerCamera.transform.right;
            rightDirection.y = 0;
            rightDirection.Normalize();

            var movement = (forwardDirection * verticalMove) + (rightDirection * horizontalMove);

            //Air
            if (_groundChecker.GetIsGrounded())
            {
                leftOverAirStrafe = airStrafeAmount;
            }

            var clampedXForStrafe = Mathf.Clamp(movement.x, -leftOverAirStrafe, leftOverAirStrafe);
            leftOverAirStrafe -= Mathf.Abs(clampedXForStrafe);
            var clampedYForStrafe = Mathf.Clamp(movement.z, -leftOverAirStrafe, leftOverAirStrafe);
            leftOverAirStrafe -= Mathf.Abs(clampedYForStrafe);

            movement.x = clampedXForStrafe;
            movement.z = clampedYForStrafe;

            _rigi.AddForce(movement, ForceMode.Force);
        }

        //Sound
        timer += Time.deltaTime;
        if ((timer > timeBetweenSteps) &&
            (_groundChecker.GetIsGrounded() &&
            (_rigi.velocity.x > 0.2 ||
            _rigi.velocity.x < -0.2) ||
            (_rigi.velocity.z > 0.2 ||
            _rigi.velocity.z < -0.2)))
        {
            _audioSource.PlayOneShot(RandomRunClip());
            timer = 0;
        }
    }

    private AudioClip RandomRunClip()
    {
        int attempts = 3;
        AudioClip newClip = run[Random.Range(0, run.Length)];

        while (newClip == _lastClip && attempts > 0)
        {
            newClip = run[Random.Range(0, run.Length)];
            attempts--;
        }

        _lastClip = newClip;
        return newClip;
    }

    private IEnumerator ApplyDash()
    {
        var t = 0f;
        isDashing = true;
        foreach (var particle in dashParticles)
        {
            particle.Play();
        }

        while (t < dashDuration && Input.GetKey(KeyCode.LeftControl))
        {
            var boostDirection = Vector3.Normalize(_rigi.velocity);
            _rigi.AddForce(boostDirection * dashForce, ForceMode.Force);

            t += Time.deltaTime;
            yield return null;
        }

        foreach (var particle in dashParticles)
        {
            particle.Stop();
        }

        isDashing = false;

        Debug.Log("dashing over");
    }
    public void NotifyHitGround()
    {
        if (!dashWindowOpen)
        {
            _audioSource.PlayOneShot(land);
            dashWindowOpen = true;
            StartCoroutine(DashWindow());
        }
    }

    private IEnumerator DashWindow()
    {
        var t = 0f;
        while (t < dashWindow)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) && dashWindowOpen && !isDashing)
            {
                _audioSource.PlayOneShot(dash);
                StartCoroutine(ApplyDash());
            }
            t += Time.deltaTime;
            yield return null;
        }
        dashWindowOpen = false;
    }

    private void Jump()
    {
        var appliedJumpVelocity = jumpVelocity;

        if (Input.GetKeyDown(KeyCode.Space) && _groundChecker.GetIsGrounded())
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                appliedJumpVelocity *= dashJumpMultiplier;
            }
            _rigi.AddForce(transform.up * appliedJumpVelocity);
            _audioSource.PlayOneShot(jump);
        }

        if (_rigi.velocity.y < 0)
        {
            _rigi.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void MouseLook()
    {
        var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * -1);

        var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

        targetCameraState.x = transform.position.x;
        targetCameraState.y = transform.position.y;
        targetCameraState.z = transform.position.z;

        targetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
        targetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;

        var clampedPitch = Mathf.Clamp(targetCameraState.pitch, -mousePitchLimit, mousePitchLimit);
        targetCameraState.pitch = clampedPitch;

        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        interpolatingCameraState.LerpTowards(targetCameraState, positionLerpPct, rotationLerpPct);

        interpolatingCameraState.UpdateTransform(_playerCamera.transform, transform);
    }


    public void EndGame()
    {
        fadeToWhite.Play("fade");
        _audioSource.clip = endMusic;
        _audioSource.Play();
        _gameOver = true;
    }

    public void GivePower(Power power)
    {
        switch (power)
        {
            case Power.Speed:
                acceleration += speedGive;
                break;
            case Power.Jump:
                jumpVelocity += jumpGive;
                break;
            case Power.Dash:
                dashForce += dashGive;
                break;
            default:
                break;
        }
    }
    class CameraState
    {
        public float mousePitchLimit = 90;
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t, Transform b)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            //b.eulerAngles = new Vector3(0, yaw, 0);
            t.position = new Vector3(x, y, z);
        }
    }
}
