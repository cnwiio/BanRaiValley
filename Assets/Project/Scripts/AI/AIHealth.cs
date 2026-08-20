using UnityEngine;

public class AIHealth : MonoBehaviour, IDamageable
{
    private int _hp;
    
    public void TakeDamage(int amount)
    {
        Debug.Log(gameObject.name  + " Take Damage : " + amount);
        _hp -= amount;
    }
}