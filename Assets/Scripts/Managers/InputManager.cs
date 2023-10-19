using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; } = null;

    bool IsGameRunning => GameManager.Instance?.IsGameRunning ?? false;

    public Hex HoveredHex { get; private set; }

    [SerializeField] LayerMask inputMovementLayerMask;

    public static UnityAction<Hex> OnHoveredHexChanged;
    public static UnityAction<Hex> OnPlaceHexInputClicked;

    public Vector3 InputPosition { get; private set; } = Vector3.zero;

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

        if (Input.GetMouseButtonDown(0))
        {
            CheckPlaceHexInput();
        }
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

        bool isHit = Physics.Raycast(r, out hit, 1000f, inputMovementLayerMask);

        if (!isHit)
            return;

        Vector3 hitPos = hit.point;
        hitPos.y = 0f;

        InputPosition = hitPos;

        AxialCoordinate coord = HexUtils.GetAxialCoordinateFromWorldPosition(hitPos, HexGridManager.Instance.HexSize);

        Hex newHex = HexGridManager.Instance.GetHex(coord);

        if (newHex != null && newHex != HoveredHex) 
        {
            HoveredHex = newHex;
            OnHoveredHexChanged?.Invoke(HoveredHex);
        }  
    }

    private void CheckPlaceHexInput() 
    {
        if (!GameManager.Instance.IsGameRunning)
            return;

        if (HoveredHex == null)
            return;

        OnPlaceHexInputClicked?.Invoke(HoveredHex);
    }
}
