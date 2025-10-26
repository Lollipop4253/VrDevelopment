using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Impact Effects")]
    [Tooltip("������ ������ ��� ����� (�����, ����� � �.�.)")]
    public GameObject impactParticles; // ������ ������
    public GameObject SmokeParticles;

    void Start(){
        Instantiate(SmokeParticles, transform.position, transform.rotation);
    }

    void OnCollisionEnter(Collision collision)
    {

        // �������� ����� ��������
        Vector3 contactPoint = collision.contacts[0].point;

        // ������������� �������
        if (impactParticles != null)
        {
            Instantiate(impactParticles, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}
//asd