using UnityEngine;
using System.Collections.Generic;

public class ProjectileEnemy : MonoBehaviour
{
    [Header("Target Colliders")]
    public GameObject impactParticles;
    public GameObject SmokeParticles;

    void Start()
    {
        Instantiate(SmokeParticles, transform.position, transform.rotation);
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject hit = collision.collider.gameObject;

        if (hit.tag == "HullCollider")
        {
            Debug.Log("Корпус");

            Tank tank = hit.GetComponentInParent<Tank>();
            tank?.ApplyGlobalSlow(0.3f, 10f);
        }
        else if (hit.tag == "TurretCollider")
        {
            Debug.Log("Башня");

            KnobToRotation turretControl = hit.GetComponentInParent<KnobToRotation>();
            turretControl.UpdateSpeedToTime(0.01f, 10f);
        }
        else if (hit.tag == "GunCollider")
        {
            Debug.Log("Пушка");

            KnobToRotation turretControl = hit.GetComponent<KnobToRotation>();
            turretControl.UpdateSpeedToTime(0.001f, 10f);
        }
        else if (hit.tag == "LeftTrackCollider")
        {
            Debug.Log("Левая гусеница");

            Tank tank = hit.GetComponentInParent<Tank>();
            tank.ApplyTrackDamage("Left", 10f);
        }
        else if (hit.tag == "RightTrackCollider")
        {
            Debug.Log("Правая гусеница");

            Tank tank = hit.GetComponentInParent<Tank>();
            tank.ApplyTrackDamage("Right", 10f);
        }
        else
        {
            Debug.Log("Куда?");
        }

        if (impactParticles != null)
        {
            ContactPoint contact = collision.contacts[0];
            Instantiate(impactParticles, contact.point, Quaternion.LookRotation(contact.normal));
        }

        Destroy(gameObject);
    }
}