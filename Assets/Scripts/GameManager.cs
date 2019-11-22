using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    // 定数定義
    // オーブ最大数、オーブが発生する秒数、最大お寺レベル
    private const int MAX_ORB = 10;
    private const int RESPAWN_TIME = 5;
    private const int MAX_LEVEL = 2;
    
    // データセーブ用キー
    // スコア、レベル、オーブ数、時間
    private const string KEY_SCORE = "SCORE";
    private const string KEY_LEVEL = "LEVEL";
    private const string KEY_ORB = "ORB";
    private const string KEY_TIME = "TIME";

    // オブジェクト参照
    // オーブプレハブ、煙プレハブ、くす玉プレハブ
    public GameObject orbPrefab;
    public GameObject smokePrefab;
    public GameObject kusudamaPrefab;
    // ゲームキャンバス、スコアテキスト、お寺、木魚
    public GameObject canvasGame;
    public GameObject textScore;
    public GameObject imageTemple;
    public GameObject imageMokugyo;
    
    // 効果音
    // スコアゲット、レベルアップ、クリア
    public AudioClip getScoreSE;
    public AudioClip levelUpSE;
    public AudioClip clearSE;
    
    // メンバ変数
    // 現在のスコア、レベルアップまでに必要なスコア、現在のオーブ数
    private int score = 0;
    private int nextScore = 10;
    private int currentOrb = 0;
    // 前回オーブを生成した時間、寺のレベル、レベルアップ値
    private DateTime lastDateTime;
    private int templeLevel = 0;
    private int[] nextScoreTable = new int[]{10, 100, 1000};
    // オーディオソース、まとめて生成するオーブの数
    private AudioSource audioSource;
    private int numOfOrb;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // オーディオソース取得
        audioSource = this.gameObject.GetComponent<AudioSource>();
        
        // 初期設定
        score = PlayerPrefs.GetInt(KEY_SCORE, 0);
        templeLevel = PlayerPrefs.GetInt(KEY_LEVEL, 0);

        nextScore = nextScoreTable[templeLevel];
        imageTemple.GetComponent<TempleManager>()   .SetTemplePicture(templeLevel);
        imageTemple.GetComponent<TempleManager>()   .SetTempleScale(score, nextScore);
        RefreshScoreText();
    }

    
    // Update is called once per frame
    void Update()
    {
        // まとめて生成するオーブがあれば生成
        while(numOfOrb > 0){
            Invoke("CreateNewOrb", 0.1f * numOfOrb);
            numOfOrb--;
        }
    }
    
    
    // バックグラウンドへの移行時と復帰次（アプリ起動時も含む）に呼び出される
    void OnApplicationPause(bool pauseStatus)
    {
        if(pauseStatus){
            // アプリがバックグラウンドへ移行
        }
        else{
            // バックグラウンドから復帰
            // 時間の復元
            string time = PlayerPrefs.GetString(KEY_TIME, "");
            if(time == ""){
                lastDateTime = DateTime.UtcNow;
            }
            else{
                long temp = Convert.ToInt64(time);
                lastDateTime = DateTime.FromBinary(temp);
            }
            
            numOfOrb = 0;
            // 時間によるオーブの自動生成
            TimeSpan timeSpan = DateTime.UtcNow - lastDateTime;
            if(timeSpan >= TimeSpan.FromSeconds(RESPAWN_TIME)){
                while(timeSpan > TimeSpan.FromSeconds(RESPAWN_TIME)){
                    if(numOfOrb < MAX_ORB){
                        numOfOrb++;
                    }
                    timeSpan -= TimeSpan.FromSeconds(RESPAWN_TIME);
                }
            }
        }
    }
    
    
    // 新しいオーブの生成
    public void CreateNewOrb()
    {
        // オーブ生成時間の更新
        lastDateTime = DateTime.UtcNow;
        
        if(currentOrb >= MAX_ORB){
            return;
        }
        CreateOrb();
        currentOrb++;
        
        SaveGameData();
    }
    
    
    // オーブ生成
    public void CreateOrb()
    {
        // オーブプレハブから新しいインスタンス(オーブ)を生成
        GameObject orb = (GameObject)Instantiate(orbPrefab);
        // 新しいオーブの親をcanvasGameに設定
        orb.transform.SetParent(canvasGame.transform, false);
        // オーブの位置を設定
        orb.transform.localPosition = new Vector3(
            UnityEngine.Random.Range(-100.0f, 100.0f),
            UnityEngine.Random.Range(-300.0f, -450.0f),
            0f);
            
        // オーブの種類を設定
        int kind = UnityEngine.Random.Range(0, templeLevel + 1);
        switch(kind){
            case 0:
                orb.GetComponent<OrbManager>().SetKind(OrbManager   .ORB_KIND.BLUE);
                break;
            case 1:
                orb.GetComponent<OrbManager>().SetKind(OrbManager   .ORB_KIND.GREEN);
                break;
            case 2:
                orb.GetComponent<OrbManager>().SetKind(OrbManager   .ORB_KIND.PURPLE);
                break;
        }
        
        orb.GetComponent<OrbManager>().FlyOrb();
        
        audioSource.PlayOneShot(getScoreSE);
        
        // 木魚アニメ再生
        AnimatorStateInfo stateInfo =
            imageMokugyo.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        if(stateInfo.fullPathHash ==
            Animator.StringToHash("Base Layer.get@ImageMokugyo")){
            // すでに再生中なら最初から
            imageMokugyo.GetComponent<Animator>().Play(stateInfo.fullPathHash, 0, 0.0f);
        }
        else{
            imageMokugyo.GetComponent<Animator>().SetTrigger("isGetScore");
        }
        
    }
    
    
    // オーブ入手
    public void GetOrb(int getScore)
    {
        if(score < nextScore){
            score += getScore;
            
            // レベルアップ値を超えないように制限
            if(score > nextScore){
                score = nextScore;
            }
        
            TempleLevelUp();
            RefreshScoreText();
            imageTemple.GetComponent<TempleManager>()   .SetTempleScale(score, nextScore);
        
            // ゲームクリア判定
            if((score == nextScore) && (templeLevel == MAX_LEVEL)){
                ClearEffect();
            }
        }
        
        currentOrb--;
        
        SaveGameData();
    }
    
    
    // スコアテキスト更新
    void RefreshScoreText()
    {
        textScore.GetComponent<Text>().text =
            "徳：" + score + " / " + nextScore;
    }
    
    
    // 寺のレベル管理
    void TempleLevelUp()
    {
        if(score >= nextScore){
            if(templeLevel < MAX_LEVEL){
                templeLevel++;
                score = 0;
                
                TempleLevelUpEffect();
                
                nextScore = nextScoreTable[templeLevel];
                imageTemple.GetComponent<TempleManager>()   .SetTemplePicture(templeLevel);
            }
        }
    }
    
    
    // レベルアップ時の演出
    void TempleLevelUpEffect()
    {
        GameObject smoke = (GameObject)Instantiate(smokePrefab);
        smoke.transform.SetParent(canvasGame.transform, false);
        smoke.transform.SetSiblingIndex(2);
        
        audioSource.PlayOneShot(levelUpSE);
        
        Destroy(smoke, 0.5f);
    }
    
    
    // 寺が最後まで育った時の演出
    void ClearEffect()
    {
        GameObject kusudama = (GameObject)Instantiate(kusudamaPrefab);
        kusudama.transform.SetParent(canvasGame.transform, false);
    
        audioSource.PlayOneShot(clearSE);
    }
    
    
    // ゲームデータをセーブ
    void SaveGameData()
    {
        PlayerPrefs.SetInt(KEY_SCORE, score);
        PlayerPrefs.SetInt(KEY_LEVEL, templeLevel);
        PlayerPrefs.SetInt(KEY_ORB, currentOrb);
        PlayerPrefs.SetString(KEY_TIME, lastDateTime.ToBinary().ToString());
        
        PlayerPrefs.Save();
    }
    
}
