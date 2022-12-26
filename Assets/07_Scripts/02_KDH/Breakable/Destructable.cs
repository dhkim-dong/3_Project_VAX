using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour                      // 폭파 구현 외국버전인대 현재 사용 안함!!
{
    private Rigidbody rigid;
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private float ExplosiveForce = 1000;
    [SerializeField] float ExplosivarRadius = 2f;
    [SerializeField] private float PieceFadeSpeed = 0.25f;
    [SerializeField] private float PieceDestroyDelay = 5f;
    [SerializeField] private float PieceSleepCheckDelay = 0.5f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public void Explode()
    {
        if (rigid != null)
            Destroy(rigid);

        if(TryGetComponent<Collider>(out Collider collider))
        {
            collider.enabled = false;
        }
        if (TryGetComponent<Renderer>(out Renderer renderer))
            renderer.enabled = false;

        GameObject brokenInstance = Instantiate(brokenObject, transform.position, transform.rotation);

        Rigidbody[] rigidbodies = brokenInstance.GetComponentsInChildren<Rigidbody>();

        foreach(Rigidbody body in rigidbodies)
        {
            if(rigid != null)
            {
                body.velocity = rigid.velocity;
            }

            body.AddExplosionForce(ExplosiveForce, transform.position, ExplosivarRadius);
        }

        StartCoroutine(FadeOutRigidBodies(rigidbodies));
    }

    IEnumerator FadeOutRigidBodies(Rigidbody[] Rigidbodies)
    {
        WaitForSeconds Wait = new WaitForSeconds(PieceSleepCheckDelay);
        int activeRigidbodies = Rigidbodies.Length;

        while(activeRigidbodies > 0)
        {
            yield return Wait;

            foreach(Rigidbody rigidbody in Rigidbodies)
            {
                if (rigidbody.IsSleeping())
                {
                    activeRigidbodies--;
                }
            }
        }

        yield return new WaitForSeconds(PieceDestroyDelay);

        float time = 0;
        Renderer[] renderers = Array.ConvertAll(Rigidbodies, GetRendrerFromRigidbody);

        foreach(Rigidbody body in Rigidbodies)
        {
            Destroy(body.GetComponent<Collider>());
            Destroy(body);
        }

        while(time < 1)
        {
            float step = Time.deltaTime * PieceFadeSpeed;
            foreach(Renderer renderer in renderers)
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

    private Renderer GetRendrerFromRigidbody(Rigidbody Rigidbody)
    {
        return Rigidbody.GetComponent<Renderer>();
    }
}
