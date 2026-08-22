using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log(gameObject.name);
    }
}