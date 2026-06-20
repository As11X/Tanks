using UnityEngine;
using UnityEngine.AI;

public class TankAI : MonoBehaviour
{
    private TankMovement m_Movement;
    private TankShooting m_Shooting;
    private Rigidbody m_Rigidbody;
    private GameObject[] m_AllTanks;

    private Transform m_Target;
    private NavMeshPath m_Path;
    private int m_CurrentCorner;
    private bool m_IsMoving;
    private bool m_IsFleeing;

    private float m_PathTimer;
    private float m_PathInterval;
    private float m_ShotCooldown;
    private float m_TimeBetweenShots = 2f;
    private float m_CloseTimer;
    private float m_MaxShootingDistance;

    private void Awake()
    {
        m_Movement = GetComponent<TankMovement>();
        m_Shooting = GetComponent<TankShooting>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_Movement.m_IsComputerControlled = true;
        m_Shooting.m_IsComputerControlled = true;

        m_AllTanks = new GameObject[0];
        m_Path = new NavMeshPath();
        m_PathInterval = Random.Range(0.3f, 0.6f);
        m_MaxShootingDistance = Vector3.Distance(m_Shooting.GetProjectilePosition(1.0f), transform.position);
    }

    public void Setup(GameManager manager)
    {
        m_AllTanks = new GameObject[manager.m_SpawnPoints.Length];

        for (int i = 0; i < manager.m_SpawnPoints.Length; i++)
        {
            m_AllTanks[i] = manager.m_SpawnPoints[i].m_Instance;
        }
    }

    private void Update()
    {
        if (m_ShotCooldown > 0f)
        {
            m_ShotCooldown -= Time.deltaTime;
        }

        if (m_IsFleeing)
        {
            if (m_CurrentCorner >= m_Path.corners.Length)
            {
                m_IsFleeing = false;
            }
            return;
        }

        m_PathTimer += Time.deltaTime;
        if (m_PathTimer > m_PathInterval)
        {
            m_PathTimer = 0f;
            FindTarget();
        }

        if (m_Target != null)
        {
            AttackTarget();
        }
    }

    private void FindTarget()
    {
        float shortestLength = float.MaxValue;
        Transform bestTarget = null;
        NavMeshPath bestPath = null;

        for (int i = 0; i < m_AllTanks.Length; i++)
        {
            GameObject tank = m_AllTanks[i];

            if (tank == null || tank == gameObject || !tank.activeInHierarchy)
            {
                continue;
            }

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, tank.transform.position, NavMesh.AllAreas, path))
            {
                float length = GetPathLength(path);
                if (length < shortestLength)
                {
                    shortestLength = length;
                    bestTarget = tank.transform;
                    bestPath = path;
                }
            }
        }

        if (bestTarget != null)
        {
            m_Target = bestTarget;
            m_Path = bestPath;
            m_CurrentCorner = 1;
        }
    }

    private void AttackTarget()
    {
        Vector3 toTarget = m_Target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        float facing = Vector3.Dot(toTarget.normalized, transform.forward);

        if (distance < 3f)
        {
            m_CloseTimer += Time.deltaTime;
            if (m_CloseTimer > 2f)
            {
                StartFleeing();
                return;
            }
        }
        else
        {
            m_CloseTimer = 0f;
        }

        NavMeshHit hit;
        bool lineOfSight = !NavMesh.Raycast(transform.position, m_Target.position, out hit, NavMesh.AllAreas);

        if (distance < m_MaxShootingDistance && lineOfSight)
        {
            m_IsMoving = false;

            if (m_Shooting.IsCharging())
            {
                Vector3 landingSpot = m_Shooting.GetProjectilePosition(m_Shooting.GetChargeRatio());
                float shotDistance = Vector3.Distance(landingSpot, transform.position);

                if (shotDistance >= distance - 2f && facing > 0.99f)
                {
                    m_Shooting.StopCharging();
                    m_ShotCooldown = m_TimeBetweenShots;
                }
            }
            else if (m_ShotCooldown <= 0f)
            {
                m_Shooting.StartCharging();
            }
        }
        else
        {
            m_IsMoving = true;
        }
    }

    private void StartFleeing()
    {
        Vector3 awayDirection = (transform.position - m_Target.position).normalized;
        awayDirection = Quaternion.AngleAxis(Random.Range(-90f, 90f), Vector3.up) * awayDirection;
        Vector3 destination = transform.position + awayDirection * Random.Range(5f, 20f);

        if (NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, m_Path))
        {
            m_IsFleeing = true;
            m_IsMoving = true;
            m_CurrentCorner = 1;
            m_CloseTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (m_Path.corners.Length == 0)
        {
            return;
        }

        if (!m_IsMoving && m_Target == null)
        {
            return;
        }

        Vector3 lookAt;
        if (m_IsMoving)
        {
            int corner = Mathf.Min(m_CurrentCorner, m_Path.corners.Length - 1);
            lookAt = m_Path.corners[corner];
        }
        else
        {
            lookAt = m_Target.position;
        }

        Vector3 toLookAt = lookAt - transform.position;
        toLookAt.y = 0f;
        toLookAt.Normalize();

        Vector3 forward = m_Rigidbody.rotation * Vector3.forward;
        float angle = Vector3.SignedAngle(toLookAt, forward, Vector3.up);

        if (m_IsMoving)
        {
            float moveAmount = Mathf.Clamp01(Vector3.Dot(forward, toLookAt)) * m_Movement.m_Speed * Time.deltaTime;
            m_Rigidbody.MovePosition(m_Rigidbody.position + forward * moveAmount);
        }

        float turnAmount = Mathf.Min(Mathf.Abs(angle), m_Movement.m_TurnSpeed * Time.deltaTime);
        turnAmount = turnAmount * Mathf.Sign(angle);
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * Quaternion.AngleAxis(-turnAmount, Vector3.up));

        if (Vector3.Distance(m_Rigidbody.position, lookAt) < 0.5f)
        {
            m_CurrentCorner++;
        }
    }

    private float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }
}
