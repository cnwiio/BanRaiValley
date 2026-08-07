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
        EventBus<OnHoeDoAction1Event>.Subscribe(OnHoeAction1);
        EventBus<OnHoeFarmingMode>.Subscribe(OnHoeFarmingMode);
    }

    private void OnDisable()
    {
        EventBus<OnHoeDoAction1Event>.Unsubscribe(OnHoeAction1);
        EventBus<OnHoeFarmingMode>.Unsubscribe(OnHoeFarmingMode);
    }

    void OnHoeAction1(OnHoeDoAction1Event evt)
    {
        Grid(evt.Position);
    }

    void OnHoeAction2()
    {

    }

    void OnHoeFarmingMode(OnHoeFarmingMode evt)
    {
        Grid(evt.Position);
    }

    void Grid(Vector3 selectedPos)
    {
        Vector3Int cellPos = grid.WorldToCell(selectedPos);
        PreviewPrefab.transform.position = grid.GetCellCenterWorld(cellPos);

        //LeanPool.Spawn(DirtPrefab, PreviewPrefab.transform.position, Quaternion.identity);
    }
}
