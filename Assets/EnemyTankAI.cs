using UnityEngine;

public class EnemyTankAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 5f;

    // Параметры обнаружения
    public float detectionRadius = 15f;
    public float detectionAngle = 60f;
    public LayerMask targetLayer = 1; // Слой, на котором находится target

    public Transform target; // <-- Указывается в инспекторе или будет найден автоматически
    public Transform turret; // <-- Объект башни
    public Transform shotPoint;
    public float turnSpeed = 5f;
    public GameObject bulletPrefab;
    public float shootCooldown = 2f;
    public float shootForce = 50f;

    private int currentWaypointIndex = 0;
    private float lastShotTime;
    enum AIState { Patrol, Attack }
    AIState currentState = AIState.Patrol;

    void Update()
    {
        // Пытаемся обнаружить target, если его ещё нет
        if (target == null)
        {
            FindTarget();
        }

        if (target != null)
        {
            currentState = AIState.Attack;
        }
        else
        {
            currentState = AIState.Patrol;
        }

        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Attack:
                LookAtPlayer();
                Shoot();
                // Если target вышел из зоны видимости, переключаемся обратно
                if (!IsTargetInSight())
                {
                    target = null;
                }
                break;
        }
    }

    // Проверка, видит ли бот цель
    bool IsTargetInSight()
    {
        if (target == null) return false;

        Vector3 directionToTarget = target.position - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget <= detectionRadius)
        {
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < detectionAngle / 2f)
            {
                // Проверяем, есть ли что-то между ботом и целью
                if (Physics.Raycast(transform.position, directionToTarget.normalized, out RaycastHit hit, distanceToTarget, -1, QueryTriggerInteraction.Ignore))
                {
                    // Если луч попал в цель, а не в стену
                    if (hit.transform == target)
                        return true;
                }
            }
        }
        return false;
    }

    // Поиск цели в зоне видимости
    void FindTarget()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);

        foreach (Collider col in nearbyColliders)
        {
            Transform potentialTarget = col.transform;

            Vector3 directionToTarget = potentialTarget.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget <= detectionRadius)
            {
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                if (angleToTarget < detectionAngle / 2f)
                {
                    // Проверяем, есть ли что-то между ботом и потенциальной целью
                    if (Physics.Raycast(transform.position, directionToTarget.normalized, out RaycastHit hit, distanceToTarget, -1, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform == potentialTarget)
                        {
                            target = potentialTarget;
                            return;
                        }
                    }
                }
            }
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform wp = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, wp.position, moveSpeed * Time.deltaTime);

        // Поворот в сторону следующей точки
        Vector3 directionToNext = (wp.position - transform.position).normalized;
        if (directionToNext != Vector3.zero)
        {
            directionToNext.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(directionToNext);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, wp.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void LookAtPlayer()
    {
        if (target == null) return;

        // Направление от башни к цели
        Vector3 directionToTarget = target.position - turret.position;
        directionToTarget.y = 0; // Игнорируем высоту

        // Создаём поворот, смотрящий в направлением directionToTarget
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Плавно поворачиваем башню
        turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (target == null) return;

        if (Time.time - lastShotTime >= shootCooldown)
        {
            GameObject newAmmo = Instantiate(bulletPrefab, shotPoint.position, shotPoint.rotation);
            newAmmo.transform.position += shotPoint.forward * 0.1f;

            ShootAmmo(newAmmo);
            lastShotTime = Time.time;
        }
    }

    private void ShootAmmo(GameObject ammo)
    {
        Rigidbody rb = ammo.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = false;
        rb.linearVelocity = shotPoint.forward * shootForce;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // Используем transform для рисования конуса
        Vector3 leftDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * (transform != null ? transform.forward : transform.forward) * detectionRadius;
        Vector3 rightDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * (transform != null ? transform.forward : transform.forward) * detectionRadius;

        Gizmos.DrawLine(transform.position, transform.position + leftDirection);
        Gizmos.DrawLine(transform.position, transform.position + rightDirection);

        DrawWireArc(transform.position, transform.up, transform != null ? transform.forward : transform.forward, detectionAngle, detectionRadius, Color.yellow);

        // Рисуем луч, если цель обнаружена
        if (target != null)
        {
            Vector3 directionToTarget = target.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget <= detectionRadius)
            {
                float angleToTarget = Vector3.Angle((transform != null ? transform.forward : transform.forward), directionToTarget);

                if (angleToTarget < detectionAngle / 2f)
                {
                    // Проверяем, видит ли луч цель (аналогично IsTargetInSight)
                    if (Physics.Raycast(transform.position, directionToTarget.normalized, out RaycastHit hit, distanceToTarget, -1, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform == target)
                        {
                            Gizmos.color = Color.green; // Зелёный — цель видна
                        }
                        else
                        {
                            Gizmos.color = Color.red; // Красный — между ботом и целью что-то есть
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.red; // Луч не достиг цели
                    }

                    Gizmos.DrawLine(transform.position, target.position);
                }
            }
        }
    }

    void DrawWireArc(Vector3 center, Vector3 up, Vector3 forward, float angle, float radius, Color color)
    {
        Gizmos.color = color;
        Vector3 right = Vector3.Cross(up, forward).normalized;
        float step = angle / 20f;

        Vector3 lastPoint = center + Quaternion.Euler(0, -angle / 2, 0) * forward * radius;
        for (int i = 1; i <= 20; i++)
        {
            float currentAngle = -angle / 2 + step * i;
            Vector3 point = center + (Quaternion.AngleAxis(currentAngle, up) * forward) * radius;
            Gizmos.DrawLine(lastPoint, point);
            lastPoint = point;
        }
    }
}