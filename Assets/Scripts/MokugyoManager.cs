using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MokugyoManager : MonoBehaviour
{
    // オブジェクト参照
    // ゲームマネージャー
    public GameObject gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void TapMokugyo()
    {
        gameManager.GetComponent<GameManager>().CreateNewOrb();
    }
}
