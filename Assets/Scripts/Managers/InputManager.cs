using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; } = null;

    bool IsGameRunning => GameManager.Instance?.IsGameRunning ?? false;

    public Hex HoveredHex { get; private set; }

    private readonly LayerMask InputMovementLayerMask = LayerMask.NameToLayer("HexInputHit");

    public static UnityAction<Hex> OnHoveredHexChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsGameRunning)
            return;

        CheckInput();
    }

    private void CheckInput() 
    {
        CheckInputMovement();
    }

    Vector3 lastInputPos = Vector3.zero;
    private void CheckInputMovement() 
    {
        Vector3 currentInputPos = Input.mousePosition;

        if ((currentInputPos - lastInputPos).sqrMagnitude <= 0.01f)
            return;

        lastInputPos = currentInputPos;

        RaycastHit hit;
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

        bool isHit = Physics.Raycast(r, out hit, 1000f, InputMovementLayerMask);

        if (!isHit)
            return;

        Vector3 hitPos = hit.point;
        hitPos.y = 0f;

        AxialCoordinate coord = HexUtils.GetAxialCoordinateFromWorldPosition(hitPos, HexGridManager.Instance.HexSize);

        Hex newHex = HexGridManager.Instance.GetHex(coord);

        if (newHex != HoveredHex) 
        {
            HoveredHex = newHex;
            OnHoveredHexChanged?.Invoke(HoveredHex);
        }  
    }
}
