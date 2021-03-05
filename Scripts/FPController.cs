using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPController : MonoBehaviour
{
    public float mouseSensitivity = 1f;
    public Transform player;
    float xRotation = 0f;
    //public Photon.Pun.PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        player = transform.parent;

    }


    float mouseX;
    float mouseY;
    // Update is called once per frame
    void LateUpdate()
    {
        // When the cursor is locked we want to use the First person camera
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.GetChild(0).localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
