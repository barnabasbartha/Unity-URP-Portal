using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouseLook : MonoBehaviour
{

    [SerializeField] float mouseSensitivity = 100f;
    public Transform player;
    private float xRotation = 0f;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        float mouseDeltaX = GetMouseAxis("Mouse X");
        float mouseDeltaY = GetMouseAxis("Mouse Y");

        xRotation = Mathf.Clamp(xRotation - mouseDeltaY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.Rotate(Vector3.up * mouseDeltaX);
    }

    private float GetMouseAxis(string axis)
    {
        return Input.GetAxis(axis) * mouseSensitivity * Time.deltaTime;
    }
}
