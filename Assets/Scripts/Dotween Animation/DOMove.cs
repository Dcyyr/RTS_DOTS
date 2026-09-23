using DG.Tweening;
using UnityEngine;

public class DOMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //transform.DOMoveX(3, 1).OnComplete(() =>
        //{
        //    transform.DOMoveY(3, 1);

        //});

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMoveX(4, 2).SetDelay(1f).SetEase(Ease.OutBounce))
            .Append(transform.DOScale(new Vector3(2, 5, 8), 5))
            .Append(transform.DORotate(new Vector3(0, 360, 60), 2f).SetLoops(5,LoopType.Incremental))
            .Join(transform.GetComponent<Renderer>().material.DOColor(Color.blue,5f));

        
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

 
        

}
