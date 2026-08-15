using UnityEngine;

/// <summary>
/// Encapsulates "how a hologram should look/behave" for one PreviewState.
/// Adding a new preview mode (e.g. Water, Harvest) means adding a new class here -
/// PlacementPreviewer itself never needs to change (Open/Closed).
/// </summary>
public interface IPreviewVisualStrategy
{
    void Apply(GameObject hologram, MeshRenderer renderer, Material validMaterial, Material invalidMaterial, bool isValid);
}

/// <summary>Build preview: always visible while active, color signals valid/invalid.</summary>
public class BuildPreviewStrategy : IPreviewVisualStrategy
{
    public void Apply(GameObject hologram, MeshRenderer renderer, Material validMaterial, Material invalidMaterial, bool isValid)
    {
        renderer.sharedMaterial = isValid ? validMaterial : invalidMaterial;

        if (!hologram.activeSelf)
            hologram.SetActive(true);
    }
}

/// <summary>Delete preview: same visibility rule as Build - visible while aiming, color signals waterable/not.</summary>
public class DeletePreviewStrategy : IPreviewVisualStrategy
{
    public void Apply(GameObject hologram, MeshRenderer renderer, Material validMaterial, Material invalidMaterial, bool isTilled)
    {
        if (isTilled)
        {
            if (!hologram.activeSelf)
                hologram.SetActive(true);
            renderer.sharedMaterial = invalidMaterial;
        }
        else if (hologram.activeSelf)
        {
            hologram.SetActive(false);
        }
    }
}
/// <summary>Watering preview: only visible when aiming at something waterable.</summary>
public class WateringPreviewStrategy : IPreviewVisualStrategy
{
    public void Apply(GameObject hologram, MeshRenderer renderer, Material validMaterial, Material invalidMaterial, bool isWaterable)
    {
        if (isWaterable)
        {
            if (!hologram.activeSelf)
                hologram.SetActive(true);
            renderer.sharedMaterial = validMaterial;
        }
        else if (hologram.activeSelf)
        {
            hologram.SetActive(false);
        }
    }
}
