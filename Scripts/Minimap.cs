using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Transform player;
    private float cam_y = 0;

    private void Start()
    {
        cam_y = transform.position.y;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Move the camera 
        transform.position = new Vector3(player.position.x, cam_y, player.position.z);
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, player.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
    }
}
