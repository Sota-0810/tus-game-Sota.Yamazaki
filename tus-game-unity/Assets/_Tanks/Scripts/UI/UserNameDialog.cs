using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Tanks.Complete;
using System; // Actionを使うために必要

public class UserNameDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button changeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI warningText; // 警告メッセージ表示用

    // 名前が変わったことをホーム画面に知らせるイベント
    public event Action OnNameChanged;

    void Start()
    {
        changeButton.onClick.AddListener(OnChangeClicked);
        closeButton.onClick.AddListener(Close);
        
        // 最初は非表示にしておく
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        warningText.text = ""; // 警告リセット
        
        // 現在の名前を入力欄に入れておく
        if (UserDataManager.Instance != null)
        {
            nameInputField.text = UserDataManager.Instance.UserName;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnChangeClicked()
    {
        string newName = nameInputField.text;
        string errorMessage;

        // UserDataManagerのバリデーション機能を使う
        if (UserDataManager.Instance.IsValidUserName(newName, out errorMessage))
        {
            // OKなら保存して閉じる
            UserDataManager.Instance.UpdateUserName(newName);
            
            // ホーム画面へ通知
            OnNameChanged?.Invoke();
            
            Close();
        }
        else
        {
            // NGなら警告を表示
            warningText.text = errorMessage;
        }
    }
}