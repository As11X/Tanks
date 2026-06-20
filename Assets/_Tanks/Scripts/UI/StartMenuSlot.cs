using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuSlot : MonoBehaviour
{
    public Color m_SlotColor;
    public RectTransform m_TankPreviewPosition;
    public TextMeshProUGUI m_TankStats;
    public Button m_AddControlButton;
    public RectTransform m_ControlChoiceRoot;
    public Button m_P1ControlButton;
    public Button m_P2ControlButton;
    public Button m_ComputerControlButton;
    public Button m_OffControlButton;
    public Image BackgroundImage;
    public Sprite OpenSlotBackground;
    public Sprite UsedSlotBackground;

    public GameObject TankPreview { get; set; }
    public GameObject TankPrefab { get; private set; }
    public int PlayerControlling { get; set; }
    public bool IsOpen { get; set; }
    public bool IsComputer { get; set; }

    private Camera m_MenuCamera;

    private void Awake()
    {
        m_MenuCamera = GetComponentInParent<Camera>();
        IsOpen = true;
        BackgroundImage.sprite = OpenSlotBackground;
    }

    private void Update()
    {
        if (TankPreview != null)
        {
            TankPreview.transform.Rotate(Vector3.up, 45.0f * Time.deltaTime);
        }
    }

    public void AddTank()
    {
        m_AddControlButton.gameObject.SetActive(false);
        m_ControlChoiceRoot.gameObject.SetActive(true);

        IsOpen = false;
        BackgroundImage.sprite = UsedSlotBackground;
    }

    public void RemoveTank()
    {
        m_AddControlButton.gameObject.SetActive(true);
        m_ControlChoiceRoot.gameObject.SetActive(false);

        SetPlayerControlling(-1);

        IsOpen = true;
        BackgroundImage.sprite = OpenSlotBackground;
    }

    public void SetPlayerControlling(int playerNumber)
    {
        if (PlayerControlling == 1)
        {
            m_P1ControlButton.interactable = true;
        }
        else if (PlayerControlling == 2)
        {
            m_P2ControlButton.interactable = true;
        }
        else if (PlayerControlling == -1)
        {
            m_ComputerControlButton.interactable = true;
        }

        PlayerControlling = playerNumber;

        if (playerNumber == 1)
        {
            m_P1ControlButton.interactable = false;
            IsComputer = false;
        }
        else if (playerNumber == 2)
        {
            m_P2ControlButton.interactable = false;
            IsComputer = false;
        }
        else if (playerNumber == -1)
        {
            m_ComputerControlButton.interactable = false;
            IsComputer = true;
        }
    }

    public void SetTankPreview(GameObject prefab)
    {
        if (TankPreview != null)
        {
            Destroy(TankPreview);
        }

        TankPrefab = prefab;
        TankPreview = Instantiate(prefab);

        TankMovement move = TankPreview.GetComponent<TankMovement>();
        TankShooting shoot = TankPreview.GetComponent<TankShooting>();
        TankHealth health = TankPreview.GetComponent<TankHealth>();

        move.enabled = false;
        shoot.enabled = false;

        m_TankStats.text = "Speed " + move.m_Speed + "\nDamage " + shoot.m_MaxDamage + "\nHealth: " + health.m_StartingHealth;

        Vector3 position = m_MenuCamera.WorldToScreenPoint(m_TankPreviewPosition.position);
        TankPreview.transform.position = m_MenuCamera.ScreenToWorldPoint(position) + Vector3.back * 3.0f;

        MeshRenderer[] renderers = TankPreview.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                if (renderers[i].materials[j].name.Contains("TankColor"))
                {
                    renderers[i].materials[j].color = m_SlotColor;
                }
            }
        }

        AudioSource[] audioSources = TankPreview.GetComponentsInChildren<AudioSource>();
        for (int i = 0; i < audioSources.Length; i++)
        {
            Destroy(audioSources[i]);
        }
    }
}
