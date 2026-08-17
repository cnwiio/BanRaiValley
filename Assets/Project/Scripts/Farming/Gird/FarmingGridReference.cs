using UnityEngine;
/// <summary>
/// Runtime "mailbox" asset that lets any number of Hoe instances find the active
/// FarmingGrid without holding a direct scene reference to it.
///
/// Why this instead of [SerializeField] private FarmingGrid grid;
/// - Order of creation doesn't matter: FarmingGrid registers on OnEnable,
///   Hoe only *reads* Grid when it's actually needed (Update / PrimaryAction),
///   never at Awake/serialize time.
/// - FarmingGrid and Hoe don't need to be in the same GameObject or even know
///   about each other's hierarchy — they only both point at this one asset.
/// - Works for any number of Hoe instances (or future tools like WateringCan)
///   at once, since they all read from the same shared asset.
///
/// Setup: Assets > Create > Farming > Farming Grid Reference (make ONE asset),
/// then drag that same asset into the FarmingGrid component and every Hoe component.
/// </summary>
[CreateAssetMenu(fileName = "FarmingGridReference", menuName = "Scriptable Objects/FarmingGridReference")]
public class FarmingGridReference : ScriptableObject
{
    // Not serialized on purpose - this is a runtime-only pointer, never baked into the asset file.
    private IFarmingGrid _grid;

    public IFarmingGrid Grid => _grid;

    public void Register(IFarmingGrid grid) => _grid = grid;

    public void Unregister(IFarmingGrid grid)
    {
        // guard against a second/old grid instance clearing a newer one's registration
        if (ReferenceEquals(_grid, grid))
            _grid = null;
    }

    // Safety net: if "Enter Play Mode Options" has domain reload disabled,
    // a stale reference from the previous play session could otherwise leak in.
    private void OnDisable() => _grid = null;
}
