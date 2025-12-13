using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokeMoto : MonoBehaviour
{
    [SerializeField] private BikeController bikeController;
    [SerializeField] private Rigidbody frontWheelRb;
    [SerializeField] private Rigidbody backWheelRb;
    [SerializeField] private Rigidbody bodyRb;
    [SerializeField] private Rigidbody frontBodyRb;

    [SerializeField] private float explodeForce = 5f;
    [SerializeField] private float torqueForce = 3f;
    bool broken = false;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collider");
        if (broken) return;

        if(bikeController.currentSpeed <= 15f)
        {
            return;
        }
        if (collision.collider.CompareTag("Wall"))
        {
            Debug.Log("Collider with wall");
            broken = true;
            BreakMoto(collision.GetContact(0).point).Forget();
            GameManager.Instance.GameOver();
        }
    }
    async UniTaskVoid BreakMoto(Vector3 hitPoint)
    {
        if (bikeController != null) bikeController.enabled = false;

        // detach parts (do first so transforms world positions are preserved)
        frontWheelRb.transform.SetParent(null, true);
        backWheelRb.transform.SetParent(null, true);
        bodyRb.transform.SetParent(null, true);
        frontBodyRb.transform.SetParent(null, true);

        // enable physics properly
        EnablePhysics(frontWheelRb);
        EnablePhysics(backWheelRb);
        EnablePhysics(bodyRb);
        EnablePhysics(frontBodyRb);

        // small tweak: ensure RB interpolation for visual smoothness
        SetInterpolation(frontWheelRb, RigidbodyInterpolation.Interpolate);
        SetInterpolation(backWheelRb, RigidbodyInterpolation.Interpolate);
        SetInterpolation(bodyRb, RigidbodyInterpolation.Interpolate);
        SetInterpolation(frontBodyRb, RigidbodyInterpolation.Interpolate);

        // apply blast
        AddBlast(frontWheelRb, hitPoint);
        AddBlast(backWheelRb, hitPoint);
        AddBlast(bodyRb, hitPoint);
        AddBlast(frontBodyRb, hitPoint);

        // slow motion for a short unscaled duration
        Time.timeScale = 0.5f;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.6f), false, cancellationToken: this.GetCancellationTokenOnDestroy(), delayTiming: PlayerLoopTiming.Update);
        }
        finally
        {
            Time.timeScale = 1f;
        }

        // optionally disable scripts / show gameover here
    }
    void EnsureColliderForPiece(Rigidbody rb)
    {
        if (rb == null) return;

        // 1) try find existing collider
        Collider c = rb.GetComponent<Collider>();
        if (c == null)
        {
            // if no collider, add a simple BoxCollider sized to renderer bounds
            var rend = rb.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var box = rb.gameObject.AddComponent<BoxCollider>();
                // approximate size & center from renderer bounds (local space)
                Vector3 size = rb.transform.InverseTransformVector(rend.bounds.size);
                box.size = new Vector3(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y), Mathf.Max(0.01f, size.z));
                Vector3 centerLocal = rb.transform.InverseTransformPoint(rend.bounds.center);
                box.center = centerLocal;
                c = box;
            }
            else
            {
                // fallback: add small sphere
                c = rb.gameObject.AddComponent<SphereCollider>();
            }
        }

        // 2) ensure collider usable for dynamic rigidbody
        if (c is MeshCollider mc)
        {
            mc.convex = true;                 // required for non-kinematic rigidbodies
            mc.isTrigger = false;
        }
        else
        {
            c.isTrigger = false;
        }

        c.enabled = true;
    }

    // gọi trong EnablePhysics thay vì chỉ bật rigidbody
    void EnablePhysics(Rigidbody rb)
    {
        if (rb == null) return;

        // make sure collider exists and enabled
        EnsureColliderForPiece(rb);

        // lift slightly to avoid initial overlap with ground
        rb.transform.position += Vector3.up * 0.05f;

        // force transform sync so physics sees correct start positions
        Physics.SyncTransforms();

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;
        rb.constraints = RigidbodyConstraints.None;
    }
    void SetInterpolation(Rigidbody rb, RigidbodyInterpolation mode)
    {
        if (rb == null) return;
        rb.interpolation = mode;
    }
    void AddBlast(Rigidbody rb, Vector3 hitPoint)
    {
        if (rb == null) return;
        Vector3 dir = (rb.worldCenterOfMass - hitPoint);
        if (dir.sqrMagnitude < 0.001f) dir = (rb.transform.position - hitPoint).normalized + Vector3.up * 0.2f;
        else dir = dir.normalized;
        rb.AddForce(dir * explodeForce * 1.5f, ForceMode.VelocityChange);
        rb.AddTorque(UnityEngine.Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
    }
}
