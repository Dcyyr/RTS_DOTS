using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionManagerUI : MonoBehaviour
{
    
    [SerializeField]
    private RectTransform m_SelectionAreaRectTransform;
    [SerializeField]
    private Canvas m_Canvas;

    private void Start()
    {
        UnitSelectionManager.Instance.OnSelectionAreaStart += OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += OnSelectionAreaEnd;

        m_SelectionAreaRectTransform.gameObject.SetActive(false);

    }

    private void Update()
    {
        if(m_SelectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }
    private void OnSelectionAreaStart(object sender, System.EventArgs e)
    {
        m_SelectionAreaRectTransform.gameObject.SetActive(true);
        UpdateVisual();
    }

    private void OnSelectionAreaEnd(object sender, System.EventArgs e)
    {
        m_SelectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        Rect selectionArea = UnitSelectionManager.Instance.GetSelectionAreaRect();
        float canvasScale = m_Canvas.transform.localScale.x;
        m_SelectionAreaRectTransform.anchoredPosition = new Vector2(selectionArea.x, selectionArea.y)/canvasScale;
        m_SelectionAreaRectTransform.sizeDelta = new Vector2(selectionArea.width, selectionArea.height)/canvasScale;

    }
}
