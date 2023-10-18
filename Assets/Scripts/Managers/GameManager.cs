using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } = null;

    public enum States { None, InitScene, Play }
    public States State { get; private set; } = States.None;

    public bool IsGameRunning => State == States.Play;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeState(States.InitScene);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ChangeState(States stateNew) 
    {

        switch (stateNew)
        {
            case States.InitScene:
                EnterStateInitScene();
                break;
        }

        State = stateNew;
    }

    #region States

    #region State Init Scene

    private void EnterStateInitScene() 
    {
        HexGridManager.Instance.InitHexSettings();
        HexGridManager.Instance.SetHexes();
        HexGridManager.Instance.SpawnCenterHex();
    }

    #endregion

    #endregion
}
