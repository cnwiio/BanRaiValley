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

    GameObject _hologramPrefabsSource;
    MeshRenderer _meshRenderer;
    private List<GameObject> _spawnedHologram = new List<GameObject>();
    private List<MeshRenderer> _spawnedMeshRenderers = new List<MeshRenderer>();
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
        if (!_hologramPrefabsSource)
        {
            _hologramPrefabsSource = evt.prefabs;

            for (int i = 0; i < evt.TileCount; i++)
            {
                _spawnedHologram.Add(LeanPool.Spawn(_hologramPrefabsSource));
            }
        
            foreach (var hologram in _spawnedHologram)
            {
                hologram.SetActive(false);
                if (hologram.TryGetComponent<MeshRenderer>(out var meshRenderer))
                {
                    _spawnedMeshRenderers.Add(meshRenderer);
                }
                else
                {
                    Debug.LogError(name + " cannot access meshRenderer");
                }
            }
            
            if (!_strategies.TryGetValue(evt.previewState, out _currentStrategy))
                Debug.LogWarning($"PlacementPreviewer: no visual strategy registered for {evt.previewState}");
        }
        
        
    }

    private void OnPreviewing(PreviewingEvent evt)
    {
        if (_spawnedHologram.Count < 1 || _currentStrategy == null) return;

        for (int i = 0; i < _spawnedHologram.Count; i++)
        {
            _currentStrategy.Apply(_spawnedHologram[i], _spawnedMeshRenderers[i], validMaterial, inValidMaterial, evt.PreviewTileData[i].IsValid);
            UpdatePreview(_spawnedHologram[i].transform, evt.PreviewTileData[i].Position, evt.YRotation);
        }
    }

    private void OnMultiPreviewing()
    {
        if (_hologramPrefabsSource == null || _currentStrategy == null) return;

        for (int i = 0; i < _spawnedHologram.Count; i++)
        {
            _currentStrategy.Apply(_spawnedHologram[i], _spawnedMeshRenderers[i], validMaterial, inValidMaterial, false);
        }
    }

    private void OnEndPreview(EndPreviewEvent evt)
    {
        foreach (var hologram in _spawnedHologram)
        {
            LeanPool.Despawn(hologram);
        }
        _spawnedHologram.Clear();
        _spawnedMeshRenderers.Clear();
        _hologramPrefabsSource = null;
        _currentStrategy = null;
    }

    private void UpdatePreview(Transform targetTransform, Vector3 pos, float yRotation)
    {
        targetTransform.position = pos;
        targetTransform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}