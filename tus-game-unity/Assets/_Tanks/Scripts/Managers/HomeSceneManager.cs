using UnityEngine;
using TMPro; // TextMeshProを使う場合
using UnityEngine.UI;
using Tanks.Complete;

public class HomeSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI userNameText; // 左上のユーザー名表示
    [SerializeField] private Button registrationButton;    // User Registrationボタン
    [SerializeField] private UserNameDialog userNameDialog; // 後述するダイアログ

    void Start()
    {
        // 起動時に現在のユーザー名を表示
        UpdateNameDisplay();

        // ボタンにダイアログを開く処理を登録
        registrationButton.onClick.AddListener(() => 
        {
            userNameDialog.Open();
        });

        // ダイアログで名前が変更されたら、表示を更新するイベントを登録
        userNameDialog.OnNameChanged += UpdateNameDisplay;
    }

    // 画面左上の表示を更新
    private void UpdateNameDisplay()
    {
        if (UserDataManager.Instance != null)
        {
            userNameText.text = "User: " + UserDataManager.Instance.UserName;
        }
    }
}