using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameUIHandler : MonoBehaviour
{
    public GameManager m_GameManager;
    public RectTransform m_StartMenuRoot;
    public Button m_StartButton;
    public StartMenuSlot[] m_PlayerSlots;

    private TextMeshProUGUI m_StartButtonText;
    private CanvasScaler m_CanvasScaler;
    private int m_SlotUsed = 0;

    private void Awake()
    {
        Camera cam = GetComponentInParent<Camera>();
        m_CanvasScaler = GetComponentInParent<CanvasScaler>();

        UniversalAdditionalCameraData cameraData = Camera.main.GetUniversalAdditionalCameraData();
        if (!cameraData.cameraStack.Contains(cam))
        {
            cameraData.cameraStack.Add(cam);
        }
    }

    private void Start()
    {
        m_StartButton.onClick.AddListener(StartGame);
        m_StartButtonText = m_StartButton.GetComponentInChildren<TextMeshProUGUI>();
        UpdateStartButton();

        GameObject[] tankPrefabs = new GameObject[]
        {
            m_GameManager.m_Tank1Prefab,
            m_GameManager.m_Tank2Prefab,
            m_GameManager.m_Tank3Prefab,
            m_GameManager.m_Tank4Prefab
        };

        for (int i = 0; i < m_PlayerSlots.Length; i++)
        {
            StartMenuSlot slot = m_PlayerSlots[i];

            if (i < tankPrefabs.Length)
            {
                slot.SetTankPreview(tankPrefabs[i]);
            }
            else
            {
                slot.SetTankPreview(tankPrefabs[0]);
            }

            slot.m_AddControlButton.onClick.AddListener(() => OnAddClicked(slot));
            slot.m_OffControlButton.onClick.AddListener(() => OnOffClicked(slot));
            slot.m_P1ControlButton.onClick.AddListener(() => OnPlayerClicked(slot, 1));
            slot.m_P2ControlButton.onClick.AddListener(() => OnPlayerClicked(slot, 2));
            slot.m_ComputerControlButton.onClick.AddListener(() => slot.SetPlayerControlling(-1));
        }
    }

    private void Update()
    {
        float ratio = Screen.width / (float)Screen.height;

        if (ratio > 1.0f)
        {
            m_CanvasScaler.matchWidthOrHeight = 1.0f;
        }
        else
        {
            m_CanvasScaler.matchWidthOrHeight = 0.0f;
        }
    }

    private void OnAddClicked(StartMenuSlot slot)
    {
        slot.AddTank();
        m_SlotUsed++;

        if (IsPlayerTakenByOtherSlot(slot, 1))
        {
            slot.SetPlayerControlling(-1);
        }
        else
        {
            slot.SetPlayerControlling(1);
        }

        UpdateStartButton();
    }

    private void OnOffClicked(StartMenuSlot slot)
    {
        slot.RemoveTank();
        m_SlotUsed--;

        UpdateStartButton();
    }

    private void OnPlayerClicked(StartMenuSlot slot, int player)
    {
        slot.SetPlayerControlling(player);

        for (int i = 0; i < m_PlayerSlots.Length; i++)
        {
            StartMenuSlot other = m_PlayerSlots[i];

            if (other.IsOpen || other == slot)
            {
                continue;
            }

            if (other.PlayerControlling == player)
            {
                other.SetPlayerControlling(-1);
            }
        }
    }

    private bool IsPlayerTakenByOtherSlot(StartMenuSlot slot, int player)
    {
        for (int i = 0; i < m_PlayerSlots.Length; i++)
        {
            if (m_PlayerSlots[i] != slot && m_PlayerSlots[i].PlayerControlling == player)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateStartButton()
    {
        if (m_SlotUsed >= 2)
        {
            m_StartButtonText.text = "Start";
            m_StartButton.interactable = true;
        }
        else
        {
            m_StartButtonText.text = "2 Tanks required";
            m_StartButton.interactable = false;
        }
    }

    private void StartGame()
    {
        m_StartMenuRoot.gameObject.SetActive(false);

        List<GameManager.PlayerData> playerData = new List<GameManager.PlayerData>();
        for (int i = 0; i < m_PlayerSlots.Length; i++)
        {
            StartMenuSlot slot = m_PlayerSlots[i];

            if (!slot.IsOpen)
            {
                GameManager.PlayerData data = new GameManager.PlayerData();
                data.TankColor = slot.m_SlotColor;
                data.IsComputer = slot.IsComputer;
                data.ControlIndex = slot.PlayerControlling;
                data.UsedPrefab = slot.TankPrefab;
                playerData.Add(data);
            }
        }

        m_GameManager.StartGame(playerData.ToArray());

        for (int i = 0; i < m_PlayerSlots.Length; i++)
        {
            Destroy(m_PlayerSlots[i].TankPreview);
        }
    }
}
