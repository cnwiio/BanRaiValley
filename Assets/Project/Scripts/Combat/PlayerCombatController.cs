using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages the player's melee attack loop:
/// cooldown gating → animation → delayed OverlapBox hit detection → damage + events.
/// Attach to the Player root. Wire references in the Inspector.
/// </summary>
public class PlayerCombatController : MonoBehaviour
{
    // ── Inspector Configuration ───────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private PlayerHandVisualizer _handVisualizer;
    [SerializeField] private Transform _attackCameraTransform;
    [SerializeField] private LayerMask _hitLayers;

    [Header("Unarmed Fallback")]
    [SerializeField] private ItemAttackData _unarmedAttackData = ItemAttackData.DefaultUnarmed;

    [Header("Hit Detection Settings")]
    [Tooltip("Delay in seconds between animation trigger and OverlapBox check, to align with the swing peak.")]
    [SerializeField] private float _impactDelaySec = 0.1f;
    [SerializeField] private int _maxTargetCount = 10;

    // ── Runtime State ─────────────────────────────────────────────────────────

    private float _nextAttackTimeSec;
    private SlotData _currentSlotData;
    private Collider[] _hitColliderBuffer;
    private Coroutine _hitCheckCoroutine;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _hitColliderBuffer = new Collider[_maxTargetCount];

        if (_attackCameraTransform == null && Camera.main != null)
        {
            _attackCameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        EventBus<OnHotbarChangeEvent>.Subscribe(OnHotbarChanged);
        EventBus<OnPlayerRequestAttackEvent>.Subscribe(OnPlayerRequestAttack);
        EventBus<OnPrimaryActionEvent>.Subscribe(OnPrimaryAction);
    }

    private void OnDisable()
    {
        EventBus<OnHotbarChangeEvent>.Unsubscribe(OnHotbarChanged);
        EventBus<OnPlayerRequestAttackEvent>.Unsubscribe(OnPlayerRequestAttack);
        EventBus<OnPrimaryActionEvent>.Unsubscribe(OnPrimaryAction);

        CancelPendingHitCheck();
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnHotbarChanged(OnHotbarChangeEvent evt)
    {
        _currentSlotData = evt.slotData;
    }

    private void OnPlayerRequestAttack(OnPlayerRequestAttackEvent evt)
    {
        RequestAttack();
    }

    private void OnPrimaryAction(OnPrimaryActionEvent evt)
    {
        bool slotIsEmpty = _currentSlotData.IsUnityNull() || _currentSlotData.IsEmpty;
        bool itemCanAttack = !slotIsEmpty &&
                             _currentSlotData.item != null &&
                             _currentSlotData.item.AttackData.canAttack;

        if (slotIsEmpty || itemCanAttack)
        {
            RequestAttack();
        }
    }

    // ── Attack Execution ──────────────────────────────────────────────────────

    private void RequestAttack()
    {
        if (Time.time < _nextAttackTimeSec) return;

        ItemAttackData currentAttackData = ResolveAttackData();

        if (!currentAttackData.canAttack) return;

        _nextAttackTimeSec = Time.time + currentAttackData.attackCooldownSec;

        _handVisualizer.TriggerAttackAnimation();

        EventBus<OnPlayerAttackExecutedEvent>.Raise(new OnPlayerAttackExecutedEvent
        {
            EquippedItem = _currentSlotData.item,
            AttackData   = currentAttackData,
            AttackOrigin = _attackCameraTransform != null
                ? _attackCameraTransform.position
                : transform.position
        });

        CancelPendingHitCheck();
        _hitCheckCoroutine = StartCoroutine(DelayedHitDetection(currentAttackData));
    }

    private ItemAttackData ResolveAttackData()
    {
        bool hasItem = !_currentSlotData.IsUnityNull() &&
                       !_currentSlotData.IsEmpty &&
                       _currentSlotData.item != null;

        return hasItem ? _currentSlotData.item.AttackData : _unarmedAttackData;
    }

    // ── Hit Detection ─────────────────────────────────────────────────────────

    private IEnumerator DelayedHitDetection(ItemAttackData attackData)
    {
        yield return new WaitForSeconds(_impactDelaySec);
        PerformHitDetection(attackData);
        _hitCheckCoroutine = null;
    }

    private void PerformHitDetection(ItemAttackData attackData)
    {
        if (_attackCameraTransform == null) return;

        Vector3 boxCenter   = _attackCameraTransform.position +
                              _attackCameraTransform.TransformDirection(attackData.attackBoxOffset);
        Vector3 halfExtents = attackData.attackBoxSize * 0.5f;
        Quaternion orientation = _attackCameraTransform.rotation;

        int hitCount = Physics.OverlapBoxNonAlloc(
            boxCenter,
            halfExtents,
            _hitColliderBuffer,
            orientation,
            _hitLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            ProcessHit(_hitColliderBuffer[i], attackData, boxCenter);
        }
    }

    private void ProcessHit(Collider hitCollider, ItemAttackData attackData, Vector3 boxCenter)
    {
        IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
        if (damageable == null || !damageable.IsAlive) return;

        Vector3 hitPoint  = hitCollider.ClosestPoint(boxCenter);
        Vector3 hitNormal = (_attackCameraTransform.position - hitCollider.transform.position).normalized;

        DamageData damageData = new DamageData(
            amount:         attackData.damageAmount,
            type:           attackData.damageType,
            source:         gameObject,
            hitPoint:       hitPoint,
            hitNormal:      hitNormal,
            knockbackForce: attackData.knockbackForce);

        damageable.TakeDamage(damageData);

        EventBus<OnPlayerHitTargetEvent>.Raise(new OnPlayerHitTargetEvent
        {
            TargetInstance = hitCollider.gameObject,
            DamageData     = damageData,
            TargetDied     = !damageable.IsAlive
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void CancelPendingHitCheck()
    {
        if (_hitCheckCoroutine == null) return;
        StopCoroutine(_hitCheckCoroutine);
        _hitCheckCoroutine = null;
    }

    // ── Editor Gizmos ─────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_attackCameraTransform == null) return;

        ItemAttackData previewData = Application.isPlaying
            ? ResolveAttackData()
            : _unarmedAttackData;

        Vector3 boxCenter  = _attackCameraTransform.position +
                             _attackCameraTransform.TransformDirection(previewData.attackBoxOffset);
        Quaternion orientation = _attackCameraTransform.rotation;

        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.4f);
        UnityEditor.Handles.matrix = Matrix4x4.TRS(boxCenter, orientation, Vector3.one);
        UnityEditor.Handles.DrawWireCube(Vector3.zero, previewData.attackBoxSize);
        UnityEditor.Handles.matrix = Matrix4x4.identity;
    }
#endif
}
