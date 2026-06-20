using System;
using UnityEngine;

[Serializable]
public class TankManager
{
    [HideInInspector] public Color m_PlayerColor;
    public Transform m_SpawnPoint;
    [HideInInspector] public int m_PlayerNumber;
    [HideInInspector] public string m_ColoredPlayerText;
    [HideInInspector] public GameObject m_Instance;
    [HideInInspector] public int m_Wins;
    [HideInInspector] public bool m_ComputerControlled;
    [HideInInspector] public int m_ControlIndex = 1;

    private TankMovement m_Movement;
    private TankShooting m_Shooting;
    private TankAI m_AI;
    private GameObject m_CanvasGameObject;

    public void Setup(GameManager manager)
    {
        m_Movement = m_Instance.GetComponent<TankMovement>();
        m_Shooting = m_Instance.GetComponent<TankShooting>();
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

        m_Movement.m_IsComputerControlled = m_ComputerControlled;
        m_Shooting.m_IsComputerControlled = m_ComputerControlled;
        m_Movement.m_PlayerNumber = m_PlayerNumber;
        m_Movement.m_ControlIndex = m_ControlIndex;

        if (m_ComputerControlled)
        {
            m_AI = m_Instance.AddComponent<TankAI>();
            m_AI.Setup(manager);
        }

        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                if (renderers[i].materials[j].name.Contains("TankColor"))
                {
                    renderers[i].materials[j].color = m_PlayerColor;
                }
            }
        }
    }

    public void DisableControl()
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;
        if (m_ComputerControlled)
        {
            m_AI.enabled = false;
        }

        m_CanvasGameObject.SetActive(false);
    }

    public void EnableControl()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;
        if (m_ComputerControlled)
        {
            m_AI.enabled = true;
        }

        m_CanvasGameObject.SetActive(true);
    }

    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }
}
