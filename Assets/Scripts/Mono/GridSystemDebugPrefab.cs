using DG.Tweening;
using UnityEngine;

public class GridSystemDebugPrefab : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_SpriteRenderer;

    private int m_X;
    private int m_Y;
    public void Setup(int x,int y,float gridSize)
    {
        m_X = x;
        m_Y = y;

        transform.position = GridSystem.GetWorldPosition(x, y, gridSize);
    }


    public void SetColor(Color color)
    {
        m_SpriteRenderer.color = color;
    }

    public void SetSprite(Sprite sprite)
    {
        m_SpriteRenderer.sprite = sprite;
    }

    public void SetSpriteRotation(Quaternion rotation)
    {
        m_SpriteRenderer.transform.rotation = rotation;
        m_SpriteRenderer.transform.rotation *= Quaternion.Euler(90, 0, 90);
    }


}
