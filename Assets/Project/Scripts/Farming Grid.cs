using Lean.Pool;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;


public class FarmingGrid : MonoBehaviour
{
    public enum TileState
    {
        Untillable,   // พื้นที่นอกโซน / มีสิ่งกีดขวาง ปลูกไม่ได้
        Tillable,     // ดินว่าง พรวนได้
        Tilled        // พรวนแล้ว
    }


    [SerializeField] private Grid grid;
    [SerializeField] private GameObject prefabs;
    LayerMask obstacleLayerMask;


    private void OnEnable()
    {
        EventBus<OnHoePrimaryActionEvent>.Subscribe(OnHoePrimaryAction);
        EventBus<OnHoeRaycastEvent>.Subscribe(OnHoeRaycast);
        EventBus<OnHoeTillingEvent>.Subscribe(OnHoeTilling);
    }

    private void OnDisable()
    {
        EventBus<OnHoePrimaryActionEvent>.Unsubscribe(OnHoePrimaryAction);
        EventBus<OnHoeRaycastEvent>.Unsubscribe(OnHoeRaycast);
        EventBus<OnHoeTillingEvent>.Unsubscribe(OnHoeTilling);
    }

    private void Awake()
    {
        //obstacleLayerMask = LayerMask.GetMask("Obstacle");
        obstacleLayerMask = ~LayerMask.GetMask("Ground", "Player");
    }

    void OnHoePrimaryAction(OnHoePrimaryActionEvent evt)
    {
        //Grid(evt.Position);
    }

    private void OnHoeTilling(OnHoeTillingEvent evt)
    {
        if (IsValidForTilling(_cellWorldPos))
        {
            LeanPool.Spawn(prefabs, _cellWorldPos, Quaternion.identity);
            //Instantiate(prefabs, _cellWorldPos, Quaternion.identity);
        } 
    }

    void OnHoeRaycast(OnHoeRaycastEvent evt)
    {
        if (evt.IsHit)
        {
            Grid(evt.Position);
        }
    }

    // cached
    Vector3Int _cellPos;
    Vector3 _cellWorldPos;
    void Grid(Vector3 selectedPos)
    {
        _cellPos = grid.WorldToCell(selectedPos);
        _cellWorldPos = grid.GetCellCenterWorld(_cellPos);
        if (IsValidForTilling(_cellWorldPos))
        {
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _cellWorldPos , IsValid = true});
        }
        else
        {
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _cellWorldPos , IsValid = false});
        }
    }

    private const float CELL_SIZE_MODIFIER = 0.45f;

    /// <summary>
    /// คืนค่า true ถ้าช่องนี้อยู่ในโซนและยังไม่ได้พรวน / ไม่มีสิ่งกีดขวาง
    /// </summary>
    public bool IsValidForTilling(Vector3 cellWorldPos)
    {
        return !Physics.CheckBox(cellWorldPos, grid.cellSize * CELL_SIZE_MODIFIER, Quaternion.identity, obstacleLayerMask);
    }
}
