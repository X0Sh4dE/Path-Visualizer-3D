using UnityEngine;
using System.Collections;

public class UIToggle : MonoBehaviour
{
    public RectTransform controlPanel;  // Drag ControlPanel here
    public RectTransform infoPanel;     // Drag InfoPanel here
    public RectTransform legends;       // Drag Legends here

    private bool isUIVisible = true;

    private Vector2 controlPanelVisiblePos;
    private Vector2 controlPanelHiddenPos;
    private Vector2 infoPanelVisiblePos;
    private Vector2 infoPanelHiddenPos;
    private Vector2 legendsVisiblePos;
    private Vector2 legendsHiddenPos;

    void Start()
    {
        // Store visible positions
        controlPanelVisiblePos = controlPanel.anchoredPosition;
        infoPanelVisiblePos = infoPanel.anchoredPosition;
        legendsVisiblePos = legends.anchoredPosition;

        // Calculate hidden positions (off-screen left)
        controlPanelHiddenPos = new Vector2(controlPanelVisiblePos.x - 400, controlPanelVisiblePos.y);
        infoPanelHiddenPos = new Vector2(infoPanelVisiblePos.x - 450, infoPanelVisiblePos.y);
        legendsHiddenPos = new Vector2(legendsVisiblePos.x - 400, legendsVisiblePos.y);  
    }

    void Update()
    {
        // Press H to toggle UI
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleUI();
        }
    }

    void ToggleUI()
    {
        isUIVisible = !isUIVisible;

        if (isUIVisible)
            ShowUI();
        else
            HideUI();
    }

    void ShowUI()
    {
        controlPanel.anchoredPosition = controlPanelVisiblePos;
        infoPanel.anchoredPosition = infoPanelVisiblePos;
        legends.anchoredPosition = legendsVisiblePos;
    }

    void HideUI()
    {
        controlPanel.anchoredPosition = controlPanelHiddenPos;
        infoPanel.anchoredPosition = infoPanelHiddenPos;
        legends.anchoredPosition = legendsHiddenPos;
    }


}
