using UnityEngine;

/// <summary>
/// Thin wrapper around <see cref="Animator"/> that exposes semantic playback methods
/// for the plant AI. All animator parameter names are pre-hashed in <c>Awake</c>
/// via <see cref="Animator.StringToHash"/> to eliminate per-frame string lookup overhead.
/// </summary>
public class PlantAnimationController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Animation")]
    [Tooltip("Reference to the Animator component that drives the plant's skeletal animations.")]
    [SerializeField] private Animator _animator;

    [Tooltip("Animator boolean parameter name used to drive the movement blend tree.")]
    [SerializeField] private string _movingParamName = "IsMoving";

    [Tooltip("Animator trigger parameter name for the awakening / uprooting animation.")]
    [SerializeField] private string _awakenParamName = "Awaken";

    [Tooltip("Animator trigger parameter name for the attack animation.")]
    [SerializeField] private string _attackParamName = "Attack";

    [Tooltip("Animator trigger parameter name for the hit-react / flinch animation.")]
    [SerializeField] private string _hitParamName = "Hit";

    [Tooltip("Animator trigger parameter name for the death animation.")]
    [SerializeField] private string _dieParamName = "Die";

    #endregion

    #region Fields

    private int _movingHash;
    private int _awakenHash;
    private int _attackHash;
    private int _hitHash;
    private int _dieHash;

    #endregion

    #region Unity Messages

    private void Awake()
    {
        // Pre-hash all parameter names once to avoid string overhead at runtime.
        _movingHash = Animator.StringToHash(_movingParamName);
        _awakenHash = Animator.StringToHash(_awakenParamName);
        _attackHash = Animator.StringToHash(_attackParamName);
        _hitHash    = Animator.StringToHash(_hitParamName);
        _dieHash    = Animator.StringToHash(_dieParamName);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Fires the awakening / uprooting trigger on the Animator.
    /// </summary>
    public void PlayAwaken()
    {
        _animator.SetTrigger(_awakenHash);
    }

    /// <summary>
    /// Fires the attack trigger on the Animator.
    /// </summary>
    public void PlayAttack()
    {
        _animator.SetTrigger(_attackHash);
    }

    /// <summary>
    /// Fires the hit-react / flinch trigger on the Animator.
    /// </summary>
    public void PlayHit()
    {
        _animator.SetTrigger(_hitHash);
    }

    /// <summary>
    /// Fires the death trigger on the Animator.
    /// </summary>
    public void PlayDie()
    {
        _animator.SetTrigger(_dieHash);
    }

    /// <summary>
    /// Sets the movement boolean on the Animator to drive the movement blend tree.
    /// </summary>
    /// <param name="isMoving"><c>true</c> when the plant is actively navigating; <c>false</c> when idle or stopped.</param>
    public void SetMoving(bool isMoving)
    {
        _animator.SetBool(_movingHash, isMoving);
    }

    #endregion
}
