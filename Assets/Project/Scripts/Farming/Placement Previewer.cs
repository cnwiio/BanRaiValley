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

    [SerializeField] private Material validMaterial;
    [SerializeField] private Material inValidMaterial;

    private readonly List<GameObject> _activeHolograms = new List<GameObject>(25);
    private readonly List<MeshRenderer> _activeRenderers = new List<MeshRenderer>(25);
    private readonly TilePreviewData[] _singleTileBuffer = new TilePreviewData[1];

    private GameObject _currentPrefabSource;
    private IPreviewVisualStrategy _currentStrategy;

    private void OnEnable()
    {
        EventBus<StartPreviewEvent>.Subscribe(OnStartPreview);
        EventBus<PreviewingEvent>.Subscribe(OnPreviewing);
        EventBus<MultiPreviewingEvent>.Subscribe(OnMultiPreviewing);
        EventBus<EndPreviewEvent>.Subscribe(OnEndPreview);
    }

    private void OnDisable()
    {
        EventBus<StartPreviewEvent>.Unsubscribe(OnStartPreview);
        EventBus<PreviewingEvent>.Unsubscribe(OnPreviewing);
        EventBus<MultiPreviewingEvent>.Unsubscribe(OnMultiPreviewing);
        EventBus<EndPreviewEvent>.Unsubscribe(OnEndPreview);
        DespawnAllHolograms();
    }

    private void OnStartPreview(StartPreviewEvent evt)
    {
        _currentPrefabSource = evt.prefabs;
        if (!_strategies.TryGetValue(evt.previewState, out _currentStrategy))
        {
            Debug.LogWarning($"PlacementPreviewer: no visual strategy registered for {evt.previewState}");
        }
    }

    private void OnPreviewing(PreviewingEvent evt)
    {
        _singleTileBuffer[0] = new TilePreviewData
        {
            Position = evt.Position,
            IsValid = evt.IsValid
        };

        OnMultiPreviewing(new MultiPreviewingEvent
        {
            PreviewTiles = _singleTileBuffer,
            TileCount = 1,
            YRotation = evt.YRotation
        });
    }

    private void OnMultiPreviewing(MultiPreviewingEvent evt)
    {
        if (_currentPrefabSource == null || _currentStrategy == null) return;
        if (evt.PreviewTiles == null || evt.TileCount <= 0)
        {
            HideAllActive();
            return;
        }

        // Synchronize pooled hologram instance count
        while (_activeHolograms.Count < evt.TileCount)
        {
            GameObject spawned = LeanPool.Spawn(_currentPrefabSource);
            _activeHolograms.Add(spawned);
            _activeRenderers.Add(spawned != null ? spawned.GetComponent<MeshRenderer>() : null);
        }

        while (_activeHolograms.Count > evt.TileCount)
        {
            int lastIndex = _activeHolograms.Count - 1;
            if (_activeHolograms[lastIndex] != null)
            {
                LeanPool.Despawn(_activeHolograms[lastIndex]);
            }
            _activeHolograms.RemoveAt(lastIndex);
            _activeRenderers.RemoveAt(lastIndex);
        }

        // Apply position, rotation, and strategy to each tile hologram
        for (int i = 0; i < evt.TileCount; i++)
        {
            GameObject instance = _activeHolograms[i];
            if (instance == null) continue;

            instance.transform.position = evt.PreviewTiles[i].Position;
            instance.transform.rotation = Quaternion.Euler(0f, evt.YRotation, 0f);

            _currentStrategy.Apply(instance, _activeRenderers[i], validMaterial, inValidMaterial, evt.PreviewTiles[i].IsValid);
            if (!instance.activeSelf)
            {
                instance.SetActive(true);
            }
        }
    }

    private void OnEndPreview(EndPreviewEvent evt)
    {
        DespawnAllHolograms();
    }

    private void HideAllActive()
    {
        for (int i = 0; i < _activeHolograms.Count; i++)
        {
            if (_activeHolograms[i] != null && _activeHolograms[i].activeSelf)
            {
                _activeHolograms[i].SetActive(false);
            }
        }
    }

    private void DespawnAllHolograms()
    {
        for (int i = 0; i < _activeHolograms.Count; i++)
        {
            if (_activeHolograms[i] != null)
            {
                LeanPool.Despawn(_activeHolograms[i]);
            }
        }
        _activeHolograms.Clear();
        _activeRenderers.Clear();
        _currentPrefabSource = null;
        _currentStrategy = null;
    }
}