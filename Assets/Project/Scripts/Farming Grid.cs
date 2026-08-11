using Lean.Pool;
using System.Collections.Generic;
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
    LayerMask obstacleLayerMask;

    private readonly Dictionary<Vector3Int, TileState> _overrides = new Dictionary<Vector3Int, TileState>();

    private const float CELL_SIZE_MODIFIER = 0.48f;

    private float currentYRotation = 0;
    #region SubscribeEvent

    private void OnEnable()
    {
        EventBus<OnHoeRaycastEvent>.Subscribe(OnHoeRaycast);
        EventBus<OnHoeTillingEvent>.Subscribe(OnHoeTilling);
        EventBus<OnRotateFarmEvent>.Subscribe(OnRotateFarm);
    }

    private void OnDisable()
    {
        EventBus<OnHoeRaycastEvent>.Unsubscribe(OnHoeRaycast);
        EventBus<OnHoeTillingEvent>.Unsubscribe(OnHoeTilling);
        EventBus<OnRotateFarmEvent>.Unsubscribe(OnRotateFarm);
    }

    private void Awake()
    {
        obstacleLayerMask = ~LayerMask.GetMask("Ground", "Player");
    }
    #endregion

    private void OnHoeTilling(OnHoeTillingEvent evt)
    {
        if (IsValidForTilling(_cellPos, _cellWorldPos))
        {
            RegisterTilledSoil(_cellPos);
            EventBus<OnValidGridEvent>.Raise(new OnValidGridEvent() { Position = _cellWorldPos});
        } 
    }

    void OnHoeRaycast(OnHoeRaycastEvent evt)
    {
        if (evt.IsHit)
        {
            Grid(evt.Position);
        }
    }

    private void OnRotateFarm(OnRotateFarmEvent evt)
    {
        currentYRotation = evt.YRotation;
    }

    // cached
    Vector3Int _cellPos;
    Vector3 _cellWorldPos;
    void Grid(Vector3 selectedPos)
    {
        _cellPos = grid.WorldToCell(selectedPos);
        _cellWorldPos = grid.GetCellCenterWorld(_cellPos);
        if (IsValidForTilling(_cellPos, _cellWorldPos))
        {
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _cellWorldPos , IsValid = true, YRotation = currentYRotation });
        }
        else
        {
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _cellWorldPos , IsValid = false, YRotation = currentYRotation });
        }
    }

    /// <summary>
    /// คืนค่า true ถ้าช่องนี้อยู่ในโซนและยังไม่ได้พรวน / ไม่มีสิ่งกีดขวาง
    /// </summary>
    public bool IsValidForTilling(Vector3Int cellPos,Vector3 cellWorldPos)
    {
        if (GetTileState(cellPos) != TileState.Tillable) return false;
        return !Physics.CheckBox(cellWorldPos, grid.cellSize * CELL_SIZE_MODIFIER, Quaternion.identity, obstacleLayerMask);
    }

    /// <summary>บันทึกว่าช่องนี้ถูกพรวนดินเรียบร้อยแล้ว เรียกจาก FarmingActionBehaviour</summary>
    public void RegisterTilledSoil(Vector3Int gridPos)
    {
        _overrides[gridPos] = TileState.Tilled;
    }

    public TileState GetTileState(Vector3Int gridPos)
    {
        return _overrides.TryGetValue(gridPos, out var state) ? state : TileState.Tillable;
    }
}
