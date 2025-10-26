using UnityEngine;

public class CollisionDetected : MonoBehaviour
{
    [Header("Частицы взрыва")]
    public ParticleSystem particleSystemExplotion;
    public ParticleSystem particleSystemSmoke;
    public float explosionRadius = 5f;
    public float explosionForce = 1000f;

    void Start()
    {
        //GameObject shotPoint = GameObject.Find("ShotPoint");
        Instantiate(particleSystemSmoke, transform.position, transform.rotation);
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1.0f, ForceMode.Impulse);
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {   
            Vector3 contactPoint = collision.contacts[0].point;

            Instantiate(particleSystemExplotion, contactPoint, transform.rotation);
            Explode();
            Destroy(gameObject);
        }
    }
}