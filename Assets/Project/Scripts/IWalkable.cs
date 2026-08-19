using UnityEngine;

public interface IWalkable
{
    public void Initialize(Vector3 startPosition, float stoppingDistance);
    public void MoveTo(Vector3 targetPos);
    public void ReturnToStart();
}