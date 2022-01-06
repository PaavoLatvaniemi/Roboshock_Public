using UnityEngine;


public class RocketProjectile : MonoBehaviour
{
    Rigidbody projectileRigidBody;
    float projectileSpeed = 55;
    private void OnEnable()
    {
        projectileRigidBody = GetComponent<Rigidbody>();
        projectileRigidBody.velocity = projectileRigidBody.transform.TransformDirection(Vector3.forward * projectileSpeed);
    }
    private void OnTriggerEnter(Collider other)
    {

    }
}