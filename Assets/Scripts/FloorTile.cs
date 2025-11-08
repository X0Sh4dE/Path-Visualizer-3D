using UnityEngine;

public class FloorTile : MonoBehaviour
{
    private Renderer tileRenderer;
    private Material originalMaterial;
    private Material trailMaterial;

    public Color trailColor = new Color(0.3f, 0.6f, 1f, 0.7f); // Light blue trail

    void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial = tileRenderer.material;
            trailMaterial = new Material(originalMaterial);
            trailMaterial.color = trailColor;
        }
    }

    public void MarkAsTrail()
    {
        if (tileRenderer != null)
        {
            tileRenderer.material = trailMaterial;
        }
    }

    public void ResetTile()
    {
        if (tileRenderer != null)
        {
            tileRenderer.material = originalMaterial;
        }
    }
}
