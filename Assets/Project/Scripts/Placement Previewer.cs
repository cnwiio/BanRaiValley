using Lean.Pool;
using UnityEngine;

public class PlacementPreviewer : MonoBehaviour
{
    GameObject _hologramPrefabs;
    MeshRenderer _meshRenderer;
    [SerializeField] Material validMaterial;
    [SerializeField] Material inValidMaterial;

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
        }
    }

    private void OnPreviewing(PreviewingEvent evt)
    {
        if (!_hologramPrefabs.activeSelf)
            _hologramPrefabs.SetActive(true);
        UpdateMeterial(evt.IsValid);
        UpdatePreview(evt.Position);
    }

    private void OnEndPreview(EndPreviewEvent evt)
    {
        if (_hologramPrefabs != null)
        {
            LeanPool.Despawn(_hologramPrefabs);
            _hologramPrefabs = null;
        }
    }

    private void TogglePreview(bool value)
    {

    }

    private void UpdateMeterial(bool isValid)
    {
        if (isValid)
        {
            _meshRenderer.material = validMaterial;
        }
        else
        {
            _meshRenderer.material = inValidMaterial;
        }
    }
    private void UpdatePreview(Vector3 pos)
    {
        _hologramPrefabs.transform.position = pos;
    }   
}
