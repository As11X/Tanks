using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
    public Rigidbody m_Shell;
    public Transform m_FireTransform;
    public Slider m_AimSlider;
    public AudioSource m_ShootingAudio;
    public AudioClip m_ChargingClip;
    public AudioClip m_FireClip;
    public float m_MinLaunchForce = 5f;
    public float m_MaxLaunchForce = 20f;
    public float m_MaxChargeTime = 0.75f;
    public float m_ShotCooldown = 1.0f;
    public float m_MaxDamage = 100f;
    public float m_ExplosionForce = 50f;
    public float m_ExplosionRadius = 5f;
    [HideInInspector] public bool m_IsComputerControlled = false;

    private TankMovement m_Movement;
    private float m_CurrentLaunchForce;
    private float m_ChargeSpeed;
    private bool m_Fired;
    private bool m_IsCharging;
    private float m_ShotCooldownTimer;

    private void Awake()
    {
        m_Movement = GetComponent<TankMovement>();
    }

    private void OnEnable()
    {
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_Fired = false;
        m_IsCharging = false;

        m_AimSlider.minValue = m_MinLaunchForce;
        m_AimSlider.maxValue = m_MaxLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }

    private void Start()
    {
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }

    private void Update()
    {
        if (m_ShotCooldownTimer > 0f)
        {
            m_ShotCooldownTimer -= Time.deltaTime;
        }

        m_AimSlider.value = m_MinLaunchForce;

        if (m_IsComputerControlled)
        {
            ComputerUpdate();
        }
        else
        {
            HumanUpdate();
        }
    }

    private KeyCode GetFireKey()
    {
        if (m_Movement.m_ControlIndex == 1)
        {
            return KeyCode.Space;
        }

        return KeyCode.Return;
    }

    private void HumanUpdate()
    {
        KeyCode fireKey = GetFireKey();

        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            m_CurrentLaunchForce = m_MaxLaunchForce;
            Fire();
        }
        else if (m_ShotCooldownTimer <= 0f && Input.GetKeyDown(fireKey))
        {
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }
        else if (Input.GetKey(fireKey) && !m_Fired)
        {
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            m_AimSlider.value = m_CurrentLaunchForce;
        }
        else if (Input.GetKeyUp(fireKey) && !m_Fired)
        {
            Fire();
        }
    }

    private void ComputerUpdate()
    {
        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            m_CurrentLaunchForce = m_MaxLaunchForce;
            Fire();
        }
        else if (m_IsCharging && !m_Fired)
        {
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            m_AimSlider.value = m_CurrentLaunchForce;
        }
    }

    public void StartCharging()
    {
        if (m_ShotCooldownTimer > 0f)
        {
            return;
        }

        m_IsCharging = true;
        m_Fired = false;
        m_CurrentLaunchForce = m_MinLaunchForce;

        m_ShootingAudio.clip = m_ChargingClip;
        m_ShootingAudio.Play();
    }

    public void StopCharging()
    {
        if (m_IsCharging)
        {
            Fire();
        }
    }

    public bool IsCharging()
    {
        return m_IsCharging;
    }

    public float GetChargeRatio()
    {
        return (m_CurrentLaunchForce - m_MinLaunchForce) / (m_MaxLaunchForce - m_MinLaunchForce);
    }

    private void Fire()
    {
        m_Fired = true;
        m_IsCharging = false;

        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);
        shellInstance.linearVelocity = m_CurrentLaunchForce * m_FireTransform.forward;

        ShellExplosion explosionData = shellInstance.GetComponent<ShellExplosion>();
        explosionData.m_ExplosionForce = m_ExplosionForce;
        explosionData.m_ExplosionRadius = m_ExplosionRadius;
        explosionData.m_MaxDamage = m_MaxDamage;

        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        m_CurrentLaunchForce = m_MinLaunchForce;
        m_ShotCooldownTimer = m_ShotCooldown;
    }

    public Vector3 GetProjectilePosition(float chargingLevel)
    {
        float chargeLevel = Mathf.Lerp(m_MinLaunchForce, m_MaxLaunchForce, chargingLevel);
        Vector3 velocity = chargeLevel * m_FireTransform.forward;

        float a = 0.5f * Physics.gravity.y;
        float b = velocity.y;
        float c = m_FireTransform.position.y;

        float sqrtContent = b * b - 4 * a * c;
        if (sqrtContent <= 0)
        {
            return m_FireTransform.position;
        }

        float answer1 = (-b + Mathf.Sqrt(sqrtContent)) / (2 * a);
        float answer2 = (-b - Mathf.Sqrt(sqrtContent)) / (2 * a);

        float answer = answer2;
        if (answer1 > 0)
        {
            answer = answer1;
        }

        Vector3 position = m_FireTransform.position + new Vector3(velocity.x, 0, velocity.z) * answer;
        position.y = 0;

        return position;
    }
}
