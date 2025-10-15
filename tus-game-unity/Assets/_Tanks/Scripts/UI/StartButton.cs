using UnityEngine;
using UnityEngine.UI;          // Buttonクラスを使用するために必要
using UnityEngine.SceneManagement; // SceneManagerクラスを使用するために必要

/// <summary>
/// スタートボタンのクリックイベントを処理し、ホーム画面へ遷移させるクラス
/// </summary>
public class StartButton : MonoBehaviour
{
    // シーン遷移用のシーン名定数クラス（前の質問で作成したものを使用）
    // このコードがSceneNamesクラスと同じプロジェクト内にあることを想定します。
    /* public static class SceneNames
    {
        public static readonly string TitleScene = "TitleScene";
        public static readonly string HomeScene = "HomeScene";
        public static readonly string Demo_Game_Moon = "Demo_Game_Moon";
    }
    */
    
    // インスペクターからアサインできるようにするButton型の変数
    // [SerializeField]を付けることで、privateな変数でもUnityエディタから設定可能になります。
    [SerializeField] 
    private Button startButton;

    /// <summary>
    /// オブジェクトが初めてアクティブになったフレームで呼び出されます。
    /// </summary>
    void Start()
    {
        // ボタンのクリックイベントに、OnClickedメソッドを登録します。
        // これで、ボタンがクリックされるたびにOnClickedメソッドが実行されます。
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnClicked);
        }
        else
        {
            Debug.LogError("StartButtonのButtonコンポーネントがアサインされていません。インスペクターで設定してください。");
        }
    }

    /// <summary>
    /// ボタンがクリックされたときに呼び出されるメソッド
    /// </summary>
    private void OnClicked()
    {
        // SceneManager.LoadSceneを使用して、ホーム画面へ遷移します。
        // シーン名は定数クラスSceneNamesから取得することで、タイプミスを防ぎます。
        SceneManager.LoadScene(SceneNames.HomeScene);
        
        Debug.Log($"シーン遷移: {SceneNames.HomeScene}");
    }
}