using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AttackHitBox : MonoBehaviour
{
    [SerializeField] private BoxCollider boxCollider;

    public void EnableHitBox(bool value)
    {
        boxCollider.enabled = value;
    }
}
