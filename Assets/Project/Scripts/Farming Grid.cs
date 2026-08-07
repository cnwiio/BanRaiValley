using Lean.Pool;
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
    LayerMask obstacleLayerMask;


    private void OnEnable()
    {
        EventBus<OnHoePrimaryActionEvent>.Subscribe(OnHoePrimaryAction);
        EventBus<OnHoeRaycastEvent>.Subscribe(OnHoeRaycast);
    }

    private void OnDisable()
    {
        EventBus<OnHoePrimaryActionEvent>.Unsubscribe(OnHoePrimaryAction);
        EventBus<OnHoeRaycastEvent>.Unsubscribe(OnHoeRaycast);
    }

    private void Awake()
    {
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
    }

    void OnHoePrimaryAction(OnHoePrimaryActionEvent evt)
    {
        Grid(evt.Position);
    }

    //void OnHoeSecondaryAction()
    //{

    //}

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


    /// <summary>
    /// คืนค่า true ถ้าช่องนี้อยู่ในโซนและยังไม่ได้พรวน / ไม่มีสิ่งกีดขวาง
    /// </summary>
    public bool IsValidForTilling(Vector3 cellWorldPos)
    {
        return !Physics.CheckBox(cellWorldPos, grid.cellSize * 0.5f, Quaternion.identity, obstacleLayerMask);
    }
}
