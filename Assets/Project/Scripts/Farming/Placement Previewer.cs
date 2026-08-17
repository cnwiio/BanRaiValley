using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public enum PreviewState
{
    Build,
    Delete,
    Watering,
    Planting
}

public class PlacementPreviewer : MonoBehaviour
{
    // One shared lookup: PreviewState -> the rule that decides how the hologram looks.
    // Add a new PreviewState + strategy class and this class needs zero changes (OCP).
    private static readonly Dictionary<PreviewState, IPreviewVisualStrategy> _strategies =
        new Dictionary<PreviewState, IPreviewVisualStrategy>
        {
            { PreviewState.Build, new BuildPreviewStrategy() },
            { PreviewState.Delete, new DeletePreviewStrategy() },
            { PreviewState.Watering, new WateringPreviewStrategy() },
            { PreviewState.Planting, new PlantingPreviewStrategy() }
        };

    GameObject _hologramPrefabs;
    MeshRenderer _meshRenderer;
    [SerializeField] Material validMaterial;
    [SerializeField] Material inValidMaterial;

    private IPreviewVisualStrategy _currentStrategy;

    private void OnEnable()
    {
        EventBus<StartPreviewEvent>.Subscribe(OnStartPreview);
        EventBus<PreviewingEvent>.Subscribe(OnPreviewing);
        EventBus<EndPreviewEvent>.Subscribe(OnEndPreview);
    }

    private void OnDisable()
    {
        EventBus<StartPreviewEvent>.Unsubscribe(OnStartPreview);
        EventBus<PreviewingEvent>.Unsubscribe(OnPreviewing);
        EventBus<EndPreviewEvent>.Unsubscribe(OnEndPreview);
    }

    private void OnStartPreview(StartPreviewEvent evt)
    {
        if (_hologramPrefabs == null)
        {
            _hologramPrefabs = LeanPool.Spawn(evt.prefabs);
            _hologramPrefabs.SetActive(false);
            _meshRenderer = _hologramPrefabs.GetComponent<MeshRenderer>();

            if (!_strategies.TryGetValue(evt.previewState, out _currentStrategy))
                Debug.LogWarning($"PlacementPreviewer: no visual strategy registered for {evt.previewState}");
        }
    }

    private void OnPreviewing(PreviewingEvent evt)
    {
        if (_hologramPrefabs == null || _currentStrategy == null) return;

        _currentStrategy.Apply(_hologramPrefabs, _meshRenderer, validMaterial, inValidMaterial, evt.IsValid);
        UpdatePreview(evt.Position, evt.YRotation);
    }

    private void OnEndPreview(EndPreviewEvent evt)
    {
        if (_hologramPrefabs != null)
        {
            LeanPool.Despawn(_hologramPrefabs);
            _hologramPrefabs = null;
            _currentStrategy = null;
        }
    }

    private void UpdatePreview(Vector3 pos, float yRotation)
    {
        _hologramPrefabs.transform.position = pos;
        _hologramPrefabs.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}