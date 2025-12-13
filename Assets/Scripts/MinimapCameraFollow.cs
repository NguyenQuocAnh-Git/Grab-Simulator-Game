using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapCameraFollow : MonoBehaviour
{
    public Transform target;        // vehicle transform
    public float height = 50f;      // orthographic camera height (for perspective use distance)
    public bool rotateWithTarget = false;
    public float smoothSpeed = 10f;

    Camera cam;

    private void OnEnable()
    {
        EventManager.Instance.OnBikeSpawn += OnBikeRespawn;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnBikeSpawn -= OnBikeRespawn;
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = height; // tune this for zoom
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);

        if (rotateWithTarget)
        {
            Vector3 rot = transform.eulerAngles;
            float targetY = target.eulerAngles.y;
            transform.rotation = Quaternion.Euler(90f, targetY, 0f); // keep top-down angle
        }
    }

    // Call this to change zoom
    public void SetOrthoSize(float size)
    {
        cam.orthographicSize = Mathf.Clamp(size, 5f, 200f);
    }

    public void OnBikeRespawn(GameObject bike)
    {
        target = bike.transform;
    }
}
