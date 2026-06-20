using UnityEngine;

[DefaultExecutionOrder(-10)]
public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;
    public float m_Speed = 12f;
    public float m_TurnSpeed = 180f;
    public AudioSource m_MovementAudio;
    public AudioClip m_EngineIdling;
    public AudioClip m_EngineDriving;
    public float m_PitchRange = 0.2f;
    public bool m_IsComputerControlled = false;
    [HideInInspector] public int m_ControlIndex = -1;

    private Rigidbody m_Rigidbody;
    private float m_MovementInputValue;
    private float m_TurnInputValue;
    private Vector3 m_ExplosionForceValue;
    private float m_OriginalPitch;
    private ParticleSystem[] m_ParticleSystems;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
        m_ExplosionForceValue = Vector3.zero;

        m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < m_ParticleSystems.Length; i++)
        {
            m_ParticleSystems[i].Play();
        }
    }

    private void OnDisable()
    {
        m_Rigidbody.isKinematic = true;

        for (int i = 0; i < m_ParticleSystems.Length; i++)
        {
            m_ParticleSystems[i].Stop();
        }
    }

    private void Start()
    {
        if (m_ControlIndex == -1 && !m_IsComputerControlled)
        {
            m_ControlIndex = m_PlayerNumber;
        }

        if (m_MovementAudio != null)
        {
            m_OriginalPitch = m_MovementAudio.pitch;
        }
    }

    private void Update()
    {
        if (!m_IsComputerControlled)
        {
            ReadKeyboard();
        }

        if (m_MovementAudio != null)
        {
            EngineAudio();
        }
    }

    private void ReadKeyboard()
    {
        KeyCode forward;
        KeyCode back;
        KeyCode left;
        KeyCode right;

        if (m_ControlIndex == 1)
        {
            forward = KeyCode.W;
            back = KeyCode.S;
            left = KeyCode.A;
            right = KeyCode.D;
        }
        else
        {
            forward = KeyCode.UpArrow;
            back = KeyCode.DownArrow;
            left = KeyCode.LeftArrow;
            right = KeyCode.RightArrow;
        }

        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;

        if (Input.GetKey(forward))
        {
            m_MovementInputValue += 1f;
        }
        if (Input.GetKey(back))
        {
            m_MovementInputValue -= 1f;
        }
        if (Input.GetKey(right))
        {
            m_TurnInputValue += 1f;
        }
        if (Input.GetKey(left))
        {
            m_TurnInputValue -= 1f;
        }
    }

    private void EngineAudio()
    {
        if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
        {
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
        Turn();
    }

    private void Move()
    {
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed;

        m_Rigidbody.linearVelocity = movement + m_ExplosionForceValue;
        m_ExplosionForceValue = Vector3.Lerp(m_ExplosionForceValue, Vector3.zero, Time.deltaTime * 3f);
    }

    private void Turn()
    {
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

    public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        Vector3 explosionDir = transform.position - explosionPosition;
        float explosionDistance = explosionDir.magnitude;
        explosionDir = explosionDir.normalized;

        float attenuation = 1f - Mathf.Clamp01(explosionDistance / explosionRadius);

        m_ExplosionForceValue = explosionDir * (explosionForce * attenuation);
    }
}
