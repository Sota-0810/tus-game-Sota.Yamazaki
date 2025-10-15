using UnityEngine;
using UnityEngine.UI;          // Buttonクラスを使用するために必要
using UnityEngine.SceneManagement; // SceneManagerクラスを使用するために必要

/// <summary>
/// 「Versus Player」ボタンのクリックイベントを処理し、Tanksのゲーム画面へ遷移させるクラス
/// </summary>
public class VersusPlayerButton : MonoBehaviour
{
    // [SerializeField]を付けることで、privateな変数でもUnityエディタから設定可能になります。
    // インスペクターからアサインするためのButton型の変数
    [SerializeField] 
    private Button versusPlayerButton;

    /// <summary>
    /// オブジェクトが初めてアクティブになったフレームで呼び出されます。
    /// </summary>
    void Start()
    {
        // Buttonコンポーネントがアサインされているかチェック
        if (versusPlayerButton != null)
        {
            // ボタンのクリックイベントに、OnClickedメソッドを登録します。
            versusPlayerButton.onClick.AddListener(OnClicked);
        }
        else
        {
            Debug.LogError("VersusPlayerButtonのButtonコンポーネントがアサインされていません。インスペクターで設定してください。");
        }
    }

    /// <summary>
    /// ボタンがクリックされたときに呼び出されるメソッド
    /// </summary>
    private void OnClicked()
    {
        // SceneManager.LoadSceneを使用して、指定されたゲーム画面へ遷移します。
        // SceneNamesクラスのDemo_Game_Moonを使用します。
        SceneManager.LoadScene(SceneNames.Demo_Game_Moon);
        
        Debug.Log($"シーン遷移: {SceneNames.Demo_Game_Moon}");
    }
}
