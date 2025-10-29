using UnityEngine;

public class EnemyTankAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 5f;

    // Параметры обнаружения
    public float detectionRadius = 15f;
    public float detectionAngle = 60f;
    public float fireAngle = 10f;
    public float maxSpreadAngle = 5f;
    public LayerMask targetLayer = 1;
    public Transform detectionDirection;

    public Transform target;
    public Transform turret;
    public Transform shotPoint;
    public float turnSpeed = 1f;
    public float hullTurnSpeed = 2f;
    public GameObject bulletPrefab;
    public float shootCooldown = 2f;
    public float shootForce = 50f;

    public Transform playerTurret;
    public float threatDetectionAngle = 30f;
    public float threatDetectionRadius = 20f;
    public LayerMask coverLayer;

    private int currentWaypointIndex = 0;
    private float lastShotTime;
    private Quaternion initialTurretRotation;

    private Transform currentCover;
    private Vector3 coverHidingPosition;
    private bool isTakingCover = false;
    private float coverStartTime;
    public float coverDuration = 5f;

    // Для состояния Evade
    private float evadeStartTime;
    private bool isEvading = false;
    private float lastCoverCheckTime = 0f;
    public float evadeDuration = 4f;
    public float coverCheckInterval = 1f;

    enum AIState { Patrol, Attack, TakeCover, Evade }
    AIState currentState = AIState.Patrol;

    void Start()
    {
        initialTurretRotation = turret.rotation;
    }

    void Update()
    {
        bool isUnderThreat = IsPlayerAimingAtMe();

        if (isUnderThreat)
        {
            Debug.Log("⚠️ Бот под прицелом!");
            Transform cover = FindBestCover();
            if (cover != null)
            {
                Debug.Log("✅ Укрытие найдено — прячемся");
                currentState = AIState.TakeCover;
            }
            else
            {
                Debug.Log("🔄 Укрытие не найдено — отступаем");
                currentState = AIState.Evade;
                isEvading = false; // сброс для инициализации
            }
        }
        else
        {
            if (target == null)
            {
                FindTarget();
            }
            currentState = (target != null) ? AIState.Attack : AIState.Patrol;
        }

        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                ReturnTurretToForward();
                break;

            case AIState.Attack:
                LookAtPlayer();
                Shoot();
                if (!IsTargetInSight())
                {
                    target = null;
                    lastShotTime = Time.time;
                }
                break;

            case AIState.TakeCover:
                TakeCoverBehavior();
                break;

            case AIState.Evade:
                Evade();
                break;
        }
    }

    // === ОСНОВНЫЕ МЕТОДЫ ПОВЕДЕНИЯ ===

    void TakeCoverBehavior()
    {
        if (!isTakingCover)
        {
            currentCover = FindBestCover();
            if (currentCover != null)
            {
                Vector3 dirFromPlayer = (transform.position - playerTurret.position).normalized;
                dirFromPlayer.y = 0;
                coverHidingPosition = currentCover.position - dirFromPlayer * 2f;
                coverHidingPosition.y = transform.position.y;

                isTakingCover = true;
                coverStartTime = Time.time;
            }
            else
            {
                // На всякий случай — переключаемся в Evade
                currentState = AIState.Evade;
                isEvading = false;
                return;
            }
        }

        // Движение к укрытию
        transform.position = Vector3.MoveTowards(transform.position, coverHidingPosition, moveSpeed * Time.deltaTime);

        // Поворот корпуса к укрытию
        Vector3 dirToCover = (coverHidingPosition - transform.position).normalized;
        if (dirToCover != Vector3.zero)
        {
            dirToCover.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirToCover), hullTurnSpeed * Time.deltaTime);
        }

        // Башня смотрит на игрока
        if (playerTurret != null)
        {
            Vector3 dirToPlayer = playerTurret.position - turret.position;
            dirToPlayer.y = 0;
            if (dirToPlayer != Vector3.zero)
            {
                turret.rotation = Quaternion.Slerp(turret.rotation, Quaternion.LookRotation(dirToPlayer), turnSpeed * Time.deltaTime);
            }
        }

        // Выход из укрытия
        if (Time.time - coverStartTime > coverDuration)
        {
            isTakingCover = false;
            currentCover = null;
            target = null;
        }
    }

    void Evade()
    {
        if (!isEvading)
        {
            evadeStartTime = Time.time;
            isEvading = true;
        }

        if (playerTurret == null) return;

        // Отступаем от игрока
        Vector3 dirFromPlayer = (transform.position - playerTurret.position).normalized;
        dirFromPlayer.y = 0;
        Vector3 evadePosition = transform.position + dirFromPlayer * 3f;

        transform.position = Vector3.MoveTowards(transform.position, evadePosition, moveSpeed * Time.deltaTime);

        // Поворот корпуса
        if (dirFromPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirFromPlayer), hullTurnSpeed * Time.deltaTime);
        }

        // Башня смотрит на игрока
        Vector3 dirToPlayer = playerTurret.position - turret.position;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
        {
            turret.rotation = Quaternion.Slerp(turret.rotation, Quaternion.LookRotation(dirToPlayer), turnSpeed * Time.deltaTime);
        }

        // Периодическая проверка укрытия
        if (Time.time - lastCoverCheckTime > coverCheckInterval)
        {
            lastCoverCheckTime = Time.time;
            Transform newCover = FindBestCover();
            if (newCover != null)
            {
                Debug.Log("🛡️ Укрытие найдено во время уклонения!");
                currentCover = newCover;
                isTakingCover = false;
                currentState = AIState.TakeCover;
                return;
            }
        }

        // Возврат к обычному поведению
        if (Time.time - evadeStartTime > evadeDuration)
        {
            isEvading = false;
            target = null;
        }
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    bool IsPlayerAimingAtMe()
    {
        if (playerTurret == null) return false;

        Vector3 toBot = transform.position - playerTurret.position;
        float distance = toBot.magnitude;

        if (distance > threatDetectionRadius) return false;

        float angle = Vector3.Angle(playerTurret.forward, toBot);
        if (angle > threatDetectionAngle / 2f) return false;

        if (Physics.Raycast(playerTurret.position, toBot.normalized, out RaycastHit hit, distance))
        {
            if (hit.transform == transform)
            {
                return true;
            }
        }

        return false;
    }

    Transform FindBestCover()
    {
        if (playerTurret == null) return null;

        Vector3 toBot = transform.position - playerTurret.position;
        float distance = toBot.magnitude;

        if (Physics.Raycast(playerTurret.position, toBot.normalized, out RaycastHit hit, distance))
        {
            if (hit.transform != transform && 
                (coverLayer & (1 << hit.transform.gameObject.layer)) != 0)
            {
                return hit.transform;
            }
        }

        return null;
    }

    // --- ОСТАЛЬНЫЕ МЕТОДЫ (без изменений) ---

    Vector3 RandomDirectionInCone(Vector3 forward, float angle)
    {
        float spreadRad = Mathf.Deg2Rad * angle;
        float theta = Random.Range(0f, 2f * Mathf.PI);
        float cosAngle = Mathf.Cos(spreadRad);
        float u = Random.Range(cosAngle, 1f);
        float sinAngle = Mathf.Sqrt(1f - u * u);

        Vector3 localDir = new Vector3(
            Mathf.Cos(theta) * sinAngle,
            Mathf.Sin(theta) * sinAngle,
            u
        );

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
        return rotation * localDir;
    }

    bool IsTargetInSight()
    {
        if (target == null) return false;
        Vector3 dir = target.position - turret.position;
        float dist = dir.magnitude;
        if (dist > detectionRadius) return false;
        float angle = Vector3.Angle((detectionDirection != null ? detectionDirection.forward : turret.forward), dir);
        if (angle >= detectionAngle / 2f) return false;
        if (Physics.Raycast(turret.position, dir.normalized, out RaycastHit hit, dist, -1, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == target;
        }
        return false;
    }

    bool IsTargetInFireRange()
    {
        if (target == null) return false;
        Vector3 dir = target.position - turret.position;
        float dist = dir.magnitude;
        if (dist > detectionRadius) return false;
        float angle = Vector3.Angle((detectionDirection != null ? detectionDirection.forward : turret.forward), dir);
        if (angle >= fireAngle / 2f) return false;
        if (Physics.Raycast(turret.position, dir.normalized, out RaycastHit hit, dist, -1, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == target;
        }
        return false;
    }

    void FindTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(turret.position, detectionRadius, targetLayer);
        foreach (Collider col in colliders)
        {
            Transform t = col.transform;
            Vector3 dir = t.position - turret.position;
            float dist = dir.magnitude;
            if (dist <= detectionRadius)
            {
                float angle = Vector3.Angle((detectionDirection != null ? detectionDirection.forward : turret.forward), dir);
                if (angle < detectionAngle / 2f)
                {
                    if (Physics.Raycast(turret.position, dir.normalized, out RaycastHit hit, dist, -1, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform == t)
                        {
                            target = t;
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
        Vector3 dir = (wp.position - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), hullTurnSpeed * Time.deltaTime * 0.7f);
        }
        if (Vector3.Distance(transform.position, wp.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void LookAtPlayer()
    {
        if (target == null) return;
        Vector3 dir = target.position - turret.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            turret.rotation = Quaternion.Slerp(turret.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
        }
    }

    void ReturnTurretToForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        if (forward == Vector3.zero) forward = Vector3.forward;
        turret.rotation = Quaternion.Slerp(turret.rotation, Quaternion.LookRotation(forward), turnSpeed * Time.deltaTime * 0.5f);
    }

    void Shoot()
    {
        if (target == null) return;
        if (!IsTargetInFireRange()) return;
        if (Time.time - lastShotTime >= shootCooldown)
        {
            GameObject ammo = Instantiate(bulletPrefab, shotPoint.position, shotPoint.rotation);
            ammo.transform.position += shotPoint.forward * 0.1f;
            ShootAmmo(ammo);
            lastShotTime = Time.time;
        }
    }

    void ShootAmmo(GameObject ammo)
    {
        Rigidbody rb = ammo.GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.isKinematic = false;
        Vector3 dir = RandomDirectionInCone(shotPoint.forward, maxSpreadAngle);
        rb.linearVelocity = dir * shootForce;
    }

    // === GIZMOS ===

    void OnDrawGizmos()
    {
        if (turret == null || shotPoint == null) return;

        Vector3 detectForward = detectionDirection != null ? detectionDirection.forward : turret.forward;

        // Обнаружение
        Gizmos.color = Color.yellow;
        DrawWireArc(turret.position, turret.up, detectForward, detectionAngle, detectionRadius, Color.yellow);
        Vector3 leftDetect = Quaternion.Euler(0, -detectionAngle / 2f, 0) * detectForward * detectionRadius;
        Vector3 rightDetect = Quaternion.Euler(0, detectionAngle / 2f, 0) * detectForward * detectionRadius;
        Gizmos.DrawLine(turret.position, turret.position + leftDetect);
        Gizmos.DrawLine(turret.position, turret.position + rightDetect);

        // Выстрел
        Gizmos.color = Color.green;
        DrawWireArc(turret.position, turret.up, detectForward, fireAngle, detectionRadius, Color.green);
        Vector3 leftFire = Quaternion.Euler(0, -fireAngle / 2f, 0) * detectForward * detectionRadius;
        Vector3 rightFire = Quaternion.Euler(0, fireAngle / 2f, 0) * detectForward * detectionRadius;
        Gizmos.DrawLine(turret.position, turret.position + leftFire);
        Gizmos.DrawLine(turret.position, turret.position + rightFire);

        // Разброс
        Gizmos.color = Color.red;
        float spreadRadius = detectionRadius * 0.3f;
        DrawWireArc(shotPoint.position, shotPoint.up, shotPoint.forward, maxSpreadAngle, spreadRadius, Color.red);
        Vector3 leftSpread = Quaternion.Euler(0, -maxSpreadAngle / 2f, 0) * shotPoint.forward * spreadRadius;
        Vector3 rightSpread = Quaternion.Euler(0, maxSpreadAngle / 2f, 0) * shotPoint.forward * spreadRadius;
        Gizmos.DrawLine(shotPoint.position, shotPoint.position + leftSpread);
        Gizmos.DrawLine(shotPoint.position, shotPoint.position + rightSpread);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(shotPoint.position, shotPoint.position + shotPoint.forward * spreadRadius);

        // Цель
        if (target != null)
        {
            Vector3 dir = target.position - turret.position;
            float dist = dir.magnitude;
            bool inDetection = dist <= detectionRadius && Vector3.Angle(detectForward, dir) < detectionAngle / 2f;
            bool inFire = inDetection && Vector3.Angle(detectForward, dir) < fireAngle / 2f;
            if (inDetection)
            {
                Gizmos.color = inFire ? Color.green : Color.yellow;
                Gizmos.DrawLine(turret.position, target.position);
            }
        }

        // Зона угрозы
        if (playerTurret != null)
        {
            Vector3 toBot = transform.position - playerTurret.position;
            float dist = toBot.magnitude;
            if (dist <= threatDetectionRadius)
            {
                toBot.y = 0;
                Vector3 playerForward = playerTurret.forward;
                playerForward.y = 0;
                if (toBot != Vector3.zero && playerForward != Vector3.zero)
                {
                    float angle = Vector3.Angle(playerForward, toBot);
                    Color color = (angle <= threatDetectionAngle / 2f) ? new Color(1, 0, 0, 0.7f) : new Color(0.5f, 0.5f, 0.5f, 0.4f);
                    Gizmos.color = color;
                    DrawWireArc(playerTurret.position, Vector3.up, playerForward, threatDetectionAngle, threatDetectionRadius, color);
                    Vector3 left = Quaternion.Euler(0, -threatDetectionAngle / 2f, 0) * playerForward * threatDetectionRadius;
                    Vector3 right = Quaternion.Euler(0, threatDetectionAngle / 2f, 0) * playerForward * threatDetectionRadius;
                    Gizmos.DrawLine(playerTurret.position, playerTurret.position + left);
                    Gizmos.DrawLine(playerTurret.position, playerTurret.position + right);
                    Gizmos.DrawLine(playerTurret.position, transform.position);
                }
            }
        }

        // Укрытие
        if (currentCover != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentCover.position, 0.5f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(coverHidingPosition, 0.3f);
            Gizmos.DrawLine(currentCover.position, coverHidingPosition);
        }
    }

    void DrawWireArc(Vector3 center, Vector3 up, Vector3 forward, float angle, float radius, Color color)
    {
        if (radius <= 0 || angle <= 0) return;
        Gizmos.color = color;
        forward = forward.normalized;
        up = Vector3.Cross(forward, Vector3.Cross(up, forward)).normalized;
        float half = angle * 0.5f;
        int seg = Mathf.Max(10, Mathf.RoundToInt(angle / 5f));
        Vector3 last = center + Quaternion.AngleAxis(-half, up) * forward * radius;
        for (int i = 1; i <= seg; i++)
        {
            float a = -half + (angle * i / seg);
            Vector3 p = center + Quaternion.AngleAxis(a, up) * forward * radius;
            Gizmos.DrawLine(last, p);
            last = p;
        }
    }
}