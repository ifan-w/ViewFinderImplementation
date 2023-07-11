using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region PrivateVar
    // Components
    private PlayerInputHandler _input;
    // Camera
    private float _cinemachineCameraPitch;
    private float _cinemachineCameraPitchMin = -75.0f;
    private float _cinemachineCameraPitchMax = 75.0f;
    private float _lookMovementThresh = 0.01f;
    // Mesh Cut
    private CutterController _cutter;
    private GameObject[] _lastCuttedResult;
    #endregion PrivateVar
    #region PublicAccess
    // Camera
    public GameObject CinemachineCameraTarget;
    public Vector2 MouseSensitivity;
    // Movement
    public float HorizontalSpeed;
    public float VerticalSpeed;
    #endregion PublicAccess

    private void Awake()
    {
        _input = GetComponent<PlayerInputHandler>();
        _cutter = GetComponent<CutterController>();
    }
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    private void LateUpdate()
    {
        UpdateCamera();
    }
    private void UpdateCamera()
    {
        if(_input.LookDirection.sqrMagnitude > _lookMovementThresh)
        {
            _cinemachineCameraPitch += (_input.LookDirection.y * MouseSensitivity.y) % 360.0f;
            _cinemachineCameraPitch += (
                (_cinemachineCameraPitch < -360.0f) ? 360.0f:0.0f) - ((_cinemachineCameraPitch > 360.0f) ? 360.0f : 0.0f
            );
            _cinemachineCameraPitch = Mathf.Clamp(_cinemachineCameraPitch, _cinemachineCameraPitchMin, _cinemachineCameraPitchMax);
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineCameraPitch, 0.0f, 0.0f);
            transform.Rotate(Vector3.up * _input.LookDirection.x * MouseSensitivity.x);
        }
    }
    private void FixedUpdate()
    {
        if(_input.Mouse1Pressed)
        {
            if(_lastCuttedResult != null)
            {
                foreach(var obj in _lastCuttedResult)
                {
                    obj.transform.position = CinemachineCameraTarget.transform.TransformPoint(obj.transform.position);
                    obj.transform.rotation = CinemachineCameraTarget.transform.rotation * obj.transform.rotation;
                    obj.SetActive(true);
                }
            }
            _lastCuttedResult = null;
        }
        if(_input.Mouse0Pressed)
        {
            _lastCuttedResult = _cutter.OnCut();
            foreach(var obj in _lastCuttedResult)
            {
                obj.transform.position = CinemachineCameraTarget.transform.InverseTransformPoint(obj.transform.position);
                obj.transform.rotation = Quaternion.Inverse(CinemachineCameraTarget.transform.rotation) * obj.transform.rotation;
            }
        }
        Vector3 deltaPosition = transform.up * _input.VerticalUpDown * VerticalSpeed;
        deltaPosition += (
            transform.forward * _input.HorizontalDirection.y
            + transform.right * _input.HorizontalDirection.x
        ) * HorizontalSpeed;
        transform.position += deltaPosition * Time.fixedDeltaTime;
    }

}
