using UnityEngine;

/// <summary>
/// 键盘鼠标输入提供者实现
/// </summary>
public class KeyboardMouseInputProvider : MonoBehaviour, IInputProvider
{
    private void Start()
    {
        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public Vector2 GetMoveInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector2(horizontal, vertical).normalized;
    }

    public Vector2 GetLookInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }

    public float GetZoomInput()
    {
        return Input.GetAxis("Mouse ScrollWheel");
    }

    public bool GetJumpInput()
    {
        return Input.GetButtonDown("Jump");
    }

    public bool GetSprintInput()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    public bool GetInteractInput()
    {
        return Input.GetKeyDown(KeyCode.E);
    }
}
