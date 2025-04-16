using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    public static PhysicsMaterial2D groundMat;

    PoolableObject poolable;
    Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out poolable);
        TryGetComponent(out rb);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent(out MarioController mario)) mario.Death();
        //if (collision.gameObject.name == "BarrelEnd") poolable.Disable();
        //Changed from checking for gameobject name to tag name.
        if (collision.gameObject.CompareTag("Respawn")) poolable.Disable();
        if (collision.collider.sharedMaterial == groundMat)
        {
            rb.Cast(Vector2.down, out RaycastHit2D hit);
            rb.velocity = rb.velocity.ProjectAndScale(hit.normal);
        }
    }
}
