using UnityEngine;

public enum SeedBagState
{
    Idle,
    Farm,
    Sowing
}

public class SeedBag : FarmingToolBase
{
    [SerializeField] private Animator seedBagAnimator;
    [SerializeField] private GameObject hologramPrefab;
    [SerializeField] private GameObject plantPrefab;

    private const int SEED_BAG_RANGE = 10;
    private int currentSeedAmount;
    private SeedBagState _currentState;

    public SeedBagState CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
        }
    }

    protected override void PrimaryAction()
    {
        throw new System.NotImplementedException();
    }

    protected override void SecondaryAction()
    {
        throw new System.NotImplementedException();
    }
    
    void Update()
    {
        if (CurrentState != SeedBagState.Farm) return;
        if (!TryGetGrid()) return;

        RunPreviewUpdate(SEED_BAG_RANGE, hologramPrefab, PreviewState.Watering, grid.IsWaterable, 0f);
    }
}
