using UnityEngine;

/// <summary>
/// Concrete animation driver component for plant monsters interfacing with Unity Animator.
/// </summary>
public class AIAnimationController : AIAnimationControllerBase
{
    #region Serialized Fields
    [Header("Animation")]
    [Tooltip("Reference to the Animator component driving skeletal animations.")]
    [SerializeField] private Animator _animator;

    [Tooltip("Animator float parameter name used to drive the movement blend tree.")]
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
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _movingHash = Animator.StringToHash(_movingParamName);
        _awakenHash = Animator.StringToHash(_awakenParamName);
        _attackHash = Animator.StringToHash(_attackParamName);
        _hitHash    = Animator.StringToHash(_hitParamName);
        _dieHash    = Animator.StringToHash(_dieParamName);
    }
    #endregion

    #region Public Methods
    /// <summary>Fires the awakening / uprooting trigger on the Animator.</summary>
    public override void PlayAwaken()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_awakenHash);
        }
    }

    /// <summary>Fires the attack trigger on the Animator.</summary>
    public override void PlayAttack()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_attackHash);
        }
    }

    /// <summary>Fires the hit-react / flinch trigger on the Animator.</summary>
    public override void PlayHit()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_hitHash);
        }
    }

    /// <summary>Fires the death trigger on the Animator.</summary>
    public override void PlayDie()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_dieHash);
        }
    }

    /// <summary>Updates locomotion animation speed parameter.</summary>
    /// <param name="speed">Current movement speed magnitude.</param>
    public override void SetMoving(float speed)
    {
        if (_animator != null)
        {
            _animator.SetFloat(_movingHash, speed);
        }
    }

    /// <summary>Resets the animation state machine to default rebinding.</summary>
    public override void Reset()
    {
        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }
    }
    #endregion
}
