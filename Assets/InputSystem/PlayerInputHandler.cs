using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputHandler : MonoBehaviour
{
    #region PrivateVar
    private int _UpPressed;
    private int _DownPressed;
    private int _lastNumPressed;
    private bool _mouse0Pressed;
    private bool _mouse1Pressed;
    #endregion PrivateVar
    #region PublicAccess
    public Vector2 HorizontalDirection;
    public Vector2 LookDirection;
    public int VerticalUpDown { get { return _UpPressed - _DownPressed; } }
    public int LastNumPressed {
        get {
            int num = _lastNumPressed;
            _lastNumPressed = -1;
            return num;
        }
    }
    public bool Mouse0Pressed {
        get {
            bool pressed = _mouse0Pressed;
            _mouse0Pressed = false;
            return pressed;
        }
    }
    public bool Mouse1Pressed {
        get {
            bool pressed = _mouse1Pressed;
            _mouse1Pressed = false;
            return pressed;
        }
    }
    #endregion PublicAccess

    #region InputSystem
    public void OnMove(InputValue value)
    {
        HorizontalDirection = value.Get<Vector2>();
    }
    public void OnUp(InputValue value)
    {
        if(value.isPressed) { _UpPressed = 1; }
        else { _UpPressed = 0;}
    }
    public void OnDown(InputValue value)
    {
        if(value.isPressed) { _DownPressed = 1; }
        else { _DownPressed = 0;}
    }
    public void OnLook(InputValue value)
    {
        LookDirection = value.Get<Vector2>();
    }
    public void OnNum1(InputValue value)
    {
        _lastNumPressed = 1;
    }
    public void OnNum2(InputValue value)
    {
        _lastNumPressed = 2;
    }
    public void OnNum3(InputValue value)
    {
        _lastNumPressed = 3;
    }
    public void OnNum4(InputValue value)
    {
        _lastNumPressed = 4;
    }
    public void OnMouse0(InputValue value)
    {
        _mouse0Pressed = value.isPressed;
    }
    public void OnMouse1(InputValue value)
    {
        _mouse1Pressed = value.isPressed;
    }
    public void OnReload(InputValue value)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion InputSystem
    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
