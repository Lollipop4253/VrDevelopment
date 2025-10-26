using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Impact Effects")]
    [Tooltip("Префаб частиц при ударе (взрыв, искры и т.п.)")]
    public GameObject impactParticles; // Префаб частиц


    void OnCollisionEnter(Collision collision)
    {

        // Получаем точку контакта
        Vector3 contactPoint = collision.contacts[0].point;

        // Воспроизводим частицы
        if (impactParticles != null)
        {
            Instantiate(impactParticles, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}