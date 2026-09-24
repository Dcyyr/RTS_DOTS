using UnityEngine;

//开始游戏让UI回到初始位置，可以把UI单独拖到外面（方便控制单个UI编辑）
public class ResetPositionUI : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        Destroy(this);
    }
}
