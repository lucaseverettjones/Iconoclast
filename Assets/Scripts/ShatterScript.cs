using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ShatterScript : MonoBehaviour
{
    private Rigidbody rigidBody;

    public GameObject breakableObj;

    private float explosiveForce = 200;
    private float explosiveRadius = 1f;
    private float PieceFadeSpeed = 0.25f;
    private float PieceDestroyDelay = 5f;
    private float PieceSleepCheckDelay = 0.5f;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

    }

    public void Explode()
    {
        if (rigidBody != null)
        {
            Destroy(rigidBody);
        }

        if (TryGetComponent<Collider>(out Collider collider))
        {
            collider.enabled = false;
        }
        if (TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.enabled = false;
        }
        //swap objects with broken version
        GameObject brokenInstance = Instantiate(breakableObj, transform.position, transform.rotation);

        Rigidbody[] rigidbodies = brokenInstance.GetComponentsInChildren<Rigidbody>();

        foreach(Rigidbody body in rigidbodies)
        {
            if (rigidBody != null)
            {
                body.velocity = rigidBody.velocity;
            }

            body.AddExplosionForce(explosiveForce, transform.position, explosiveRadius);
        }

        StartCoroutine(FadeOutRigidBodies(rigidbodies));
    }

    private IEnumerator FadeOutRigidBodies(Rigidbody[] Rigidbodies)
    {
        WaitForSeconds Wait = new WaitForSeconds(PieceSleepCheckDelay);
        int activeRigidbodies = Rigidbodies.Length;

        while (activeRigidbodies > 0)
        {
            yield return Wait;

            foreach (Rigidbody rigidbody in Rigidbodies)
            {
                if (rigidbody.IsSleeping())
                {
                    activeRigidbodies--;
                }
            }
        }

        yield return new WaitForSeconds(PieceDestroyDelay);

        float time = 0;
        Renderer[] renderers = Array.ConvertAll(Rigidbodies, GetRendererFromRigidbody);

        foreach(Rigidbody body in Rigidbodies)
        {
            Destroy(body.GetComponent<Collider>());
            Destroy(body);
        }

        while (time < 1)
        {
            float step = Time.deltaTime * PieceFadeSpeed;
            foreach (Renderer renderer in renderers)
            {
                renderer.transform.Translate(Vector3.down * (step / renderer.bounds.size.y), Space.World);

            }
            time += step;
            yield return null;
        }

        foreach(Renderer renderer in renderers)
        {
            Destroy(renderer.gameObject);
        }

        Destroy(gameObject);
    }
    private Renderer GetRendererFromRigidbody(Rigidbody Rigidbody)
    {
        return Rigidbody.GetComponent<Renderer>();
    }
}
