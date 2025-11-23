using UnityEngine;
using Tanks.Complete; // GameManager と GameLoopState を使うために必要

namespace Tanks.Complete
{
    /// <summary>
    /// HUDCanvas にアタッチされ、ゲームの状態に応じてHUDの表示を管理します。 (指示5)
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        // === ⬇️ 課題6: 参照するオブジェクトの変数 ===
        [Header("UI Object References")]
        [Tooltip("Player 1 のHUDオブジェクト (Player1Stock)")]
        [SerializeField]
        private GameObject m_Player1StockObject;

        [Tooltip("Player 2 のHUDオブジェクト (Player2Stock)")]
        [SerializeField]
        private GameObject m_Player2StockObject;

        [Header("Manager References")]
        [Tooltip("GameManager への参照")]
        [SerializeField]
        private GameManager m_GameManager;
        // === ⬆️ ここまで ===


        // === ⬇️ 課題8: Start メソッドでの初期化処理 ===
        private void Start()
        {
            // GameManager がインスペクタで設定されているか確認
            if (m_GameManager == null)
            {
                Debug.LogError("HUDManager: GameManager が設定されていません！インスペクタを確認してください。", this);
                return;
            }

            // 指示8a: Player1 と Player2 のHUDを（最初は）非表示にする
            if (m_Player1StockObject != null)
                m_Player1StockObject.SetActive(false);
            
            if (m_Player2StockObject != null)
                m_Player2StockObject.SetActive(false);

            // 指示8b: GameManager の状態変更イベントを購読（待ち受け）する
            m_GameManager.OnGameStateChanged += HandleGameStateChanged;

            // ▼▼▼ 追加箇所：手順4 ▼▼▼
            // 全てのタンク（TankManager）のイベントを購読する
            // ★重要: GameManager.cs の変数名 'm_SpawnPoints' を使用します
            if (m_GameManager.m_SpawnPoints != null)
            {
                for (int i = 0; i < m_GameManager.m_SpawnPoints.Length; i++)
                {
                    // 配列の中身が空でないか確認してから購読
                    if (m_GameManager.m_SpawnPoints[i] != null)
                    {
                        m_GameManager.m_SpawnPoints[i].OnWeaponStockChanged += HandleWeaponStockChanged;
                    }
                }
            }
            // ▲▲▲ 追加箇所ここまで ▲▲▲
        }
        // === ⬆️ ここまで ===


        // === ⬇️ 課題7: GameManager の状態変更イベントを処理するメソッド ===
        /// <summary>
        /// GameManager から状態変更の通知（イベント）を受け取ったときに実行されます。
        /// </summary>
        /// <param name="newState">GameManager から通知された新しいゲーム状態</param>
        private void HandleGameStateChanged(GameManager.GameLoopState newState)
        {
            // 新しい状態が「プレイ中」かどうかを判断
            bool showHUD = (newState == GameManager.GameLoopState.RoundPlaying);

            // 状態に応じて Player1 と Player2 のHUDの表示/非表示を切り替える
            if (m_Player1StockObject != null)
                m_Player1StockObject.SetActive(showHUD);
            
            if (m_Player2StockObject != null)
                m_Player2StockObject.SetActive(showHUD);
        }
        // === ⬆️ ここまで ===

        // ▼▼▼ 追加箇所：手順3 ▼▼▼
        /// <summary>
        /// タンクの弾数が変化したときに呼ばれるメソッド
        /// </summary>
        /// <param name="stockCount">現在の弾数</param>
        /// <param name="controlIndex">プレイヤー番号 (1 or 2)</param>
        private void HandleWeaponStockChanged(int stockCount, int controlIndex)
        {
            // プレイヤー番号に応じて操作する対象のオブジェクトを決める
            GameObject targetObject = null;

            if (controlIndex == 1)
            {
                targetObject = m_Player1StockObject;
            }
            else if (controlIndex == 2)
            {
                targetObject = m_Player2StockObject;
            }

            // 対象が見つかった場合、その中の PlayerStock コンポーネントを取得して更新する
            if (targetObject != null)
            {
                // GetComponentを使って PlayerStock スクリプトを取得
                PlayerStock playerStock = targetObject.GetComponent<PlayerStock>();

                if (playerStock != null)
                {
                    // UIを更新するメソッドを呼び出す
                    playerStock.UpdatePlayerStock(stockCount);
                }
            }
        }
        // ▲▲▲ 追加箇所ここまで ▲▲▲


        /// <summary>
        /// オブジェクトが破棄されるときに、イベントの購読を解除します (安全対策)
        /// </summary>
        private void OnDestroy()
        {
            //メモリリークを防ぐため、購読したイベントは必ず解除する
           if (m_GameManager != null)
           {
                m_GameManager.OnGameStateChanged -= HandleGameStateChanged;

                // ▼▼▼ 追加箇所：手順4の解除処理（推奨） ▼▼▼
                // 登録したタンクのイベントも解除しておく
                // ★重要: 変数名 'm_SpawnPoints' を使用
                if (m_GameManager.m_SpawnPoints != null)
                {
                    for (int i = 0; i < m_GameManager.m_SpawnPoints.Length; i++)
                    {
                        if (m_GameManager.m_SpawnPoints[i] != null)
                        {
                            m_GameManager.m_SpawnPoints[i].OnWeaponStockChanged -= HandleWeaponStockChanged;
                        }
                    }
                }
                // ▲▲▲ 追加箇所ここまで ▲▲▲
           }
        }
    }
}
