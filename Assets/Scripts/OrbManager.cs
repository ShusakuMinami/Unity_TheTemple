using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using DG.Tweening;

public class OrbManager : MonoBehaviour
{
    // オブジェクト参照
    // ゲームマネージャー
    private GameObject gameManager;
    
    // オーブの絵
    public Sprite[] orbPicture = new Sprite[3];
    
    // オーブの種類を定義
    public enum ORB_KIND{
        BLUE,
        GREEN,
        PURPLE
    }
    // オーブの種類
    private ORB_KIND orbKind;
    
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    // オーブ取得
    public void TouchOrb()
    {
        if(Input.GetMouseButton(0) == false){
            return;
        }
        
        RectTransform rect = GetComponent<RectTransform>();
        
        // オーブの軌跡設定
        Vector3[] path = {
            // 中間点
            new Vector3(rect.localPosition.x * 1.5f, 300f, 0f),
            // 終点
            new Vector3(0f, 150f, 0f)
        };
        
        // DOTweenを使ったアニメ作成
        rect.DOLocalPath(path, 0.5f, PathType.CatmullRom)
            .SetEase(Ease.OutQuad)
            .OnComplete(AddOrbPoint);
        // 同時にサイズも変更
        rect.DOScale(
            new Vector3(0.5f, 0.5f, 0f),
            0.5f
        );
    }
    
    
    // オーブアニメ終了後にポイント加算処理をする
    void AddOrbPoint()
    {
        switch(orbKind){
            case ORB_KIND.BLUE:
                gameManager.GetComponent<GameManager>().GetOrb(1);
                break;
            case ORB_KIND.GREEN:
                gameManager.GetComponent<GameManager>().GetOrb(5);
                break;
            case ORB_KIND.PURPLE:
                gameManager.GetComponent<GameManager>().GetOrb(10);
                break;
        }
        
        Destroy(this.gameObject);
    }
    
    
    // オーブの種類を設定
    public void SetKind(ORB_KIND kind)
    {
        orbKind = kind;
        
        switch(orbKind){
            case ORB_KIND.BLUE:
                GetComponent<Image>().sprite = orbPicture[0];
                break;
            case ORB_KIND.GREEN:
                GetComponent<Image>().sprite = orbPicture[1];
                break;
            case ORB_KIND.PURPLE:
                GetComponent<Image>().sprite = orbPicture[2];
                break;
        }
    }
}
