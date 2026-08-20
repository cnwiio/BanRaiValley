using Lean.Pool;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHandVisualizer : MonoBehaviour
{
    [SerializeField] private Transform _spawnTransform;

    [Header("Bare Hand")]
    [SerializeField] private GameObject _bareHandPrefab;

    private SlotData _currentSlotData;
    private GameObject _currentItem;
    private Animator _currentAnimator;
    private Vector3 _initialSpawnPosition;

    // ── Public Accessors ──────────────────────────────────────────────────────

    /// <summary>Animator on the currently visible hand or item model.</summary>
    public Animator CurrentAnimator => _currentAnimator;

    /// <summary>The currently spawned hand / item GameObject.</summary>
    public GameObject CurrentItemInstance => _currentItem;

    /// <summary>True when the active hotbar slot contains a valid item stack.</summary>
    public bool IsHoldingItem =>
        !_currentSlotData.IsUnityNull() &&
        !_currentSlotData.IsEmpty &&
        _currentSlotData.count > 0;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _initialSpawnPosition = _spawnTransform.localPosition;
    }

    private void OnEnable()
    {
        EventBus<OnHotbarChangeEvent>.Subscribe(OnHotbarChanged);
    }

    private void OnDisable()
    {
        EventBus<OnHotbarChangeEvent>.Unsubscribe(OnHotbarChanged);
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnHotbarChanged(OnHotbarChangeEvent evt)
    {
        _currentSlotData = evt.slotData;
        SpawnSlotItem(_currentSlotData);
    }

    // ── Core Spawn Logic ──────────────────────────────────────────────────────

    private void SpawnSlotItem(SlotData slotData)
    {
        DespawnCurrentItem();

        _spawnTransform.localPosition = _initialSpawnPosition;

        bool slotIsEmpty = slotData.IsUnityNull() || slotData.IsEmpty || slotData.count == 0;

        if (slotIsEmpty)
        {
            SpawnBareHand();
        }
        else
        {
            SpawnItemModel(slotData);
        }
    }

    private void SpawnBareHand()
    {
        if (_bareHandPrefab == null) return;

        _currentItem = LeanPool.Spawn(_bareHandPrefab, _spawnTransform);
        _currentAnimator = _currentItem.GetComponentInChildren<Animator>();
    }

    private void SpawnItemModel(SlotData slotData)
    {
        if (slotData.item == null || slotData.item.prefab == null) return;

        _spawnTransform.localPosition += slotData.item.spawnOffset;
        _currentItem = LeanPool.Spawn(slotData.item.prefab, _spawnTransform);
        _currentAnimator = _currentItem.GetComponentInChildren<Animator>();
    }

    private void DespawnCurrentItem()
    {
        if (_currentItem == null) return;

        LeanPool.Despawn(_currentItem);
        _currentItem = null;
        _currentAnimator = null;
    }

    // ── Animation Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Triggers the "Attack" animation on the current hand or item Animator.
    /// Safe to call when no Animator is present.
    /// </summary>
    public void TriggerAttackAnimation()
    {
        if (_currentAnimator == null) return;
        _currentAnimator.SetTrigger("Attack");
    }
}
