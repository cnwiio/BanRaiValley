using UnityEngine;

/// <summary>
/// Abstract base class for plant animation triggering.
/// </summary>
public abstract class AIAnimationControllerBase : MonoBehaviour
{
    #region Public Methods
    /// <summary>Fires the awaken/uprooting animation trigger.</summary>
    public abstract void PlayAwaken();

    /// <summary>Fires the attack animation trigger.</summary>
    public abstract void PlayAttack();

    /// <summary>Fires the hit-react / flinch animation trigger.</summary>
    public abstract void PlayHit();

    /// <summary>Fires the death animation trigger.</summary>
    public abstract void PlayDie();

    /// <summary>Updates locomotion animation speed parameter.</summary>
    /// <param name="speed">Current movement speed magnitude.</param>
    public abstract void SetMoving(float speed);

    /// <summary>Resets the animation state machine to default.</summary>
    public abstract void Reset();
    #endregion
}
