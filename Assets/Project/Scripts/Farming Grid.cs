using Lean.Pool;
using Unity.VisualScripting;
using UnityEngine;


public class FarmingGrid : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject DirtPrefab;
    [SerializeField] private GameObject PreviewPrefab;


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
        EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _cellWorldPos });
        //LeanPool.Spawn(DirtPrefab, PreviewPrefab.transform.position, Quaternion.identity);
    }
}
