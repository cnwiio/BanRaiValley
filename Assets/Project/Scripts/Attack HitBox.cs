using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AttackHitBox : MonoBehaviour
{
    [SerializeField] private BoxCollider collider;

    public void EnableHitBox(bool value)
    {
        collider.enabled = value;
    }
}
