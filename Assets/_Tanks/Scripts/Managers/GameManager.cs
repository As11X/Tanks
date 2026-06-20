using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public class PlayerData
    {
        public bool IsComputer;
        public Color TankColor;
        public GameObject UsedPrefab;
        public int ControlIndex;
    }

    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public CameraControl m_CameraControl;
    public GameObject m_Tank1Prefab;
    public GameObject m_Tank2Prefab;
    public GameObject m_Tank3Prefab;
    public GameObject m_Tank4Prefab;
    public TankManager[] m_SpawnPoints;

    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;
    private PlayerData[] m_TankData;
    private int m_PlayerCount;
    private TextMeshProUGUI m_TitleText;

    private void Start()
    {
        MessageTextReference textRef = FindAnyObjectByType<MessageTextReference>(FindObjectsInactive.Include);

        if (textRef == null)
        {
            Debug.LogError("You need to add the Menus prefab in the scene to use the GameManager!");
            return;
        }

        m_TitleText = textRef.Text;
        m_TitleText.text = "";

        if (m_Tank1Prefab == null || m_Tank2Prefab == null || m_Tank3Prefab == null || m_Tank4Prefab == null)
        {
            Debug.LogError("You need to assign 4 tank prefab in the GameManager!");
        }
    }

    public void StartGame(PlayerData[] playerData)
    {
        m_TankData = playerData;
        m_PlayerCount = playerData.Length;

        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_PlayerCount; i++)
        {
            TankManager tank = m_SpawnPoints[i];
            PlayerData data = m_TankData[i];

            tank.m_Instance = Instantiate(data.UsedPrefab, tank.m_SpawnPoint.position, tank.m_SpawnPoint.rotation);
            tank.m_PlayerNumber = i + 1;
            tank.m_ControlIndex = data.ControlIndex;
            tank.m_PlayerColor = data.TankColor;
            tank.m_ComputerControlled = data.IsComputer;
        }

        for (int i = 0; i < m_PlayerCount; i++)
        {
            m_SpawnPoints[i].Setup(this);
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_PlayerCount];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_SpawnPoints[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        while (m_GameWinner == null)
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());
            yield return StartCoroutine(RoundEnding());
        }

        SceneManager.LoadScene(0);
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_TitleText.text = "ROUND " + m_RoundNumber;

        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        m_TitleText.text = "";

        while (!OneTankLeft())
        {
            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        m_RoundWinner = GetRoundWinner();

        if (m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++;
        }

        m_GameWinner = GetGameWinner();

        m_TitleText.text = EndMessage();

        yield return m_EndWait;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_PlayerCount; i++)
        {
            if (m_SpawnPoints[i].m_Instance.activeSelf)
            {
                numTanksLeft++;
            }
        }

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_PlayerCount; i++)
        {
            if (m_SpawnPoints[i].m_Instance.activeSelf)
            {
                return m_SpawnPoints[i];
            }
        }

        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_PlayerCount; i++)
        {
            if (m_SpawnPoints[i].m_Wins == m_NumRoundsToWin)
            {
                return m_SpawnPoints[i];
            }
        }

        return null;
    }

    private string EndMessage()
    {
        string message = "DRAW!";

        if (m_RoundWinner != null)
        {
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";
        }

        message += "\n\n\n\n";

        for (int i = 0; i < m_PlayerCount; i++)
        {
            message += m_SpawnPoints[i].m_ColoredPlayerText + ": " + m_SpawnPoints[i].m_Wins + " WINS\n";
        }

        if (m_GameWinner != null)
        {
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
        }

        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_PlayerCount; i++)
        {
            m_SpawnPoints[i].Reset();
        }
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < m_PlayerCount; i++)
        {
            m_SpawnPoints[i].EnableControl();
        }
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < m_PlayerCount; i++)
        {
            m_SpawnPoints[i].DisableControl();
        }
    }
}
