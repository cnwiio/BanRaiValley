
using System;
using UnityEngine;

public interface IDetection
{
    public event Action<Transform> OnTargetDetectedEvent; 
    public event Action OnTargetLostEvent; 
    public void Initialize(IWalkable movement);
}