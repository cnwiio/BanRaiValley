using Lean.Pool;
using UnityEngine;

public enum PreviewState
{
    Build,
    Delete
}

public class PlacementPreviewer : MonoBehaviour
{
    GameObject _hologramPrefabs;
    MeshRenderer _meshRenderer;
    [SerializeField] Material validMaterial;
    [SerializeField] Material inValidMaterial;

    private PreviewState currentState;

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
            currentState = evt.previewState;
        }
    }

    private void OnPreviewing(PreviewingEvent evt)
    {
        if (_hologramPrefabs == null) return;
        
        if (currentState == PreviewState.Build)
        {
            UpdateBuildMeterial(evt.IsValid);
            if (!_hologramPrefabs.activeSelf)
                _hologramPrefabs.SetActive(true);
        } 
        else if (currentState == PreviewState.Delete)
        {
            UpdateDeleteMeterial(evt.IsValid);
        }

        UpdatePreview(evt.Position, evt.YRotation);
    }

    private void OnEndPreview(EndPreviewEvent evt)
    {
        if (_hologramPrefabs != null)
        {
            LeanPool.Despawn(_hologramPrefabs);
            _hologramPrefabs = null;
        }
    }

    private void UpdateBuildMeterial(bool isValid)
    {
        if (isValid)
        {
            _meshRenderer.sharedMaterial = validMaterial;
        }
        else
        {
            _meshRenderer.sharedMaterial = inValidMaterial;
        }
    }

    private void UpdateDeleteMeterial(bool isTilled)
    {
        if (isTilled)
        {
            if (!_hologramPrefabs.activeSelf)
                _hologramPrefabs.SetActive(true);
            _meshRenderer.sharedMaterial = inValidMaterial;
        } 
        else
        {
            if (_hologramPrefabs.activeSelf)
                _hologramPrefabs.SetActive(false);
        }
    }

    private void UpdatePreview(Vector3 pos, float Yrotation)
    {
        _hologramPrefabs.transform.position = pos;
        _hologramPrefabs.transform.rotation = Quaternion.Euler(0, Yrotation, 0);
    }   
}
