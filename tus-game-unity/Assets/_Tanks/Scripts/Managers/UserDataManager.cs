using UnityEngine;
using System.Text.RegularExpressions;

namespace Tanks.Complete
{
    public class UserDataManager : MonoBehaviour
    {
        // どこからでもアクセスできるようにする（シングルトン）
        public static UserDataManager Instance { get; private set; }

        public string UserID { get; private set; }
        public string UserName { get; private set; }

        // 保存用のキー
        private const string KEY_USER_ID = "UserID";
        private const string KEY_USER_NAME = "UserName";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // シーン遷移しても消えないようにする
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// ログイン処理（アカウントがなければ新規作成）
        /// </summary>
        public void LoginOrSignUp()
        {
            if (PlayerPrefs.HasKey(KEY_USER_ID))
            {
                // アカウントあり：ロードしてログイン
                UserID = PlayerPrefs.GetString(KEY_USER_ID);
                UserName = PlayerPrefs.GetString(KEY_USER_NAME);
                Debug.Log($"ログイン: ID={UserID}, Name={UserName}");
            }
            else
            {
                // アカウントなし：新規作成
                UserID = "1";          // 指定通り IDは 1
                UserName = "NoName";   // 指定通り 初期名は NoName
                Save();
                Debug.Log($"新規登録: ID={UserID}, Name={UserName}");
            }
        }

        /// <summary>
        /// ユーザー名を変更して保存する
        /// </summary>
        public void UpdateUserName(string newName)
        {
            UserName = newName;
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetString(KEY_USER_ID, UserID);
            PlayerPrefs.SetString(KEY_USER_NAME, UserName);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// ユーザー名のバリデーション（仕様チェック）
        /// </summary>
        public bool IsValidUserName(string name, out string errorMessage)
        {
            errorMessage = "";

            // 1. 文字数制限 (3〜15文字)
            if (name.Length < 3 || name.Length > 15)
            {
                errorMessage = "ユーザー名は3文字以上15文字以内で入力してください。";
                return false;
            }

            // 2. 記号禁止 (英数字、ひらがな、カタカナ、漢字のみ許可)
            // ^ と $ は文字列の先頭と末尾、[]内は許可する文字の範囲
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FAF]+$"))
            {
                errorMessage = "記号は使用できません。";
                return false;
            }

            return true;
        }
    }
}