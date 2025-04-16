using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelSpawner : MonoBehaviour
{
    public ObjectPool barrels = new();

    public Vector2 timerRange;
    public float initialVelocity;
    public PhysicsMaterial2D groundMat;
    private Timer.Loop timer;

    private void Awake()
    {
        SetTimer();
        Barrel.groundMat = groundMat;
    }

    private void FixedUpdate() => timer.Tick(SpawnBarrel);

    private void SetTimer() => timer.rate = timerRange.RandomBetween();
    private void SpawnBarrel()
    {
        PoolableObject barrel = barrels.Pump();
        //barrel.GetComponent<Rigidbody2D>().velocity = Vector2.right * initialVelocity;
        //Swapped position of float and vector with respect to Orders of Operations Efficieny.
        barrel.GetComponent<Rigidbody2D>().velocity = initialVelocity * Vector2.right;
        SetTimer();
    }
}
