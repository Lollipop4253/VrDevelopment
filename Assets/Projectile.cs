using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Impact Effects")]
<<<<<<< HEAD
    [Tooltip("пїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅ (пїЅпїЅпїЅпїЅпїЅ, пїЅпїЅпїЅпїЅпїЅ пїЅ пїЅ.пїЅ.)")]
    public GameObject impactParticles; // пїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅ
    public GameObject SmokeParticles;

    void Start(){
        Instantiate(SmokeParticles, transform.position, transform.rotation);
    }
=======
    [Tooltip("Префаб частиц при ударе (взрыв, искры и т.п.)")]
    public GameObject impactParticles; // Префаб частиц

>>>>>>> dfedd42dd06e3f80c5104bdc3314a8ea8724a950

    void OnCollisionEnter(Collision collision)
    {

<<<<<<< HEAD
        // пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ
        Vector3 contactPoint = collision.contacts[0].point;

        // пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅпїЅ
=======
        // Получаем точку контакта
        Vector3 contactPoint = collision.contacts[0].point;

        // Воспроизводим частицы
>>>>>>> dfedd42dd06e3f80c5104bdc3314a8ea8724a950
        if (impactParticles != null)
        {
            Instantiate(impactParticles, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}