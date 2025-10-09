using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Impact Effects")]
    [Tooltip("������ ������ ��� ����� (�����, ����� � �.�.)")]
    public GameObject impactParticles; // ������ ������


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