using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCombatManager : MonoBehaviour
{
    [SerializeField] private Item bareHand;
    [SerializeField] private Transform playerCamTransform;
    [SerializeField] private LayerMask enemyLayer;
    
    private Item _item;
    private ItemAttackData _attackData;
    private Collider[] hitCollider = new Collider[10];
    private readonly HashSet<IDamageable> hitTargets = new();
    
    private Vector3 attackPos;
    private Quaternion hitboxOrientation;
    private const float HALF_EXTENTS_MODIFY = 0.5f;
    private int hitCount;
    private Vector3 hitboxSize;

    private void Awake()
    {
        _item = bareHand;
        _attackData = _item.attackData;
    }

    private void OnEnable()
    {
        EventBus<OnPlayerRequestAttackEvent>.Subscribe(OnPlayerAttack);
        EventBus<OnHotbarChangeEvent>.Subscribe(OnHotbarChange);
    }

    private void OnDisable()
    {
        EventBus<OnPlayerRequestAttackEvent>.Unsubscribe(OnPlayerAttack);
        EventBus<OnHotbarChangeEvent>.Unsubscribe(OnHotbarChange);
    }

    private void OnHotbarChange(OnHotbarChangeEvent evt)
    {
        if (evt.slotData.IsEmpty)
        {
            _item = bareHand;
            _attackData = _item.attackData;
        }
        else
        {
            _item = evt.slotData.item;
            _attackData = _item.attackData;
        }
    }
    
    private void OnPlayerAttack(OnPlayerRequestAttackEvent evt)
    {
        SpawnHitBox(_attackData);
    }

    private IDamageable target;
    private void SpawnHitBox(ItemAttackData attackData)
    {
        attackPos = playerCamTransform.position +
                    playerCamTransform.TransformDirection(attackData.hitboxOffset);
        // attackPos.y = playerCamTransform.position.y;
        hitboxOrientation = playerCamTransform.rotation;
        // hitboxOrientation.y = 0;
        hitboxSize = attackData.hitBoxSize * HALF_EXTENTS_MODIFY;
        hitCount = Physics.OverlapBoxNonAlloc(
            attackPos,
            hitboxSize,
            hitCollider,
            hitboxOrientation,
            enemyLayer,
            QueryTriggerInteraction.Ignore
        );
    
        hitTargets.Clear();
        for (int i = 0; i < hitCount; i++)
        {
            target = hitCollider[i].GetComponentInParent<IDamageable>();
            if (target != null && hitTargets.Add(target))
                target.TakeDamage(attackData.damage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.indianRed;
        Gizmos.DrawWireCube(attackPos, hitboxSize); 
    }
}
