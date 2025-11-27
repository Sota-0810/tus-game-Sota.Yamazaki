using UnityEngine;
using Tanks.Complete; 

namespace Tanks.Complete
{
    public class HUDManager : MonoBehaviour
    {
        [Header("UI Object References")]
        [Tooltip("Player 1 のHUDオブジェクト (Player1Stock)")]
        [SerializeField]
        private GameObject m_Player1StockObject;

        [Tooltip("Player 2 のHUDオブジェクト (Player2Stock)")]
        [SerializeField]
        private GameObject m_Player2StockObject;

        // ▼▼▼ 追加: ミニマップのUI画像自体を制御するための変数 ▼▼▼
        [Tooltip("ミニマップを表示しているUIオブジェクト (Raw Image)")]
        [SerializeField]
        private GameObject m_MinimapUI;
        // ▲▲▲ ここまで ▲▲▲

        [Tooltip("Player 1 のミニマップ用カメラ (自動取得されます)")]
        private Camera player1Camera;

        [Header("Manager References")]
        [Tooltip("GameManager への参照")]
        [SerializeField]
        private GameManager m_GameManager;

        private void Start()
        {
            if (m_GameManager == null)
            {
                Debug.LogError("HUDManager: GameManager が設定されていません！", this);
                return;
            }

            if (m_Player1StockObject != null) m_Player1StockObject.SetActive(false);
            if (m_Player2StockObject != null) m_Player2StockObject.SetActive(false);

            m_GameManager.OnGameStateChanged += HandleGameStateChanged;

            if (m_GameManager.m_SpawnPoints != null)
            {
                for (int i = 0; i < m_GameManager.m_SpawnPoints.Length; i++)
                {
                    if (m_GameManager.m_SpawnPoints[i] != null)
                    {
                        m_GameManager.m_SpawnPoints[i].OnWeaponStockChanged += HandleWeaponStockChanged;
                    }
                }
            }

            // ▼▼▼ 追加: プレハブのカメラを直接取得してOFFにする試み ▼▼▼
            // 配列にしてループ処理で一括設定します
            GameObject[] tankPrefabs = new GameObject[] 
            { 
                m_GameManager.m_Tank1Prefab, 
                m_GameManager.m_Tank2Prefab, 
                m_GameManager.m_Tank3Prefab, 
                m_GameManager.m_Tank4Prefab 
            };

            foreach (var prefab in tankPrefabs)
            {
                if (prefab != null)
                {
                    // プレハブの中からカメラを探す (true = 非アクティブでも探す)
                    Camera cam = prefab.GetComponentInChildren<Camera>(true);
                    
                    if (cam != null)
                    {
                        // プレハブの設定を変更しようとする
                        // (※注意: Unityの仕様によっては実行時エラーになるか、インスタンスに反映されない場合があります)
                        cam.gameObject.SetActive(false);
                    }
                }
            }
            // ▲▲▲ ここまで ▲▲▲
        }

        private void HandleGameStateChanged(GameManager.GameLoopState newState)
        {
            bool isPlaying = (newState == GameManager.GameLoopState.RoundPlaying);

            if (m_Player1StockObject != null) m_Player1StockObject.SetActive(isPlaying);
            if (m_Player2StockObject != null) m_Player2StockObject.SetActive(isPlaying);

            // ▼▼▼ 追加: ミニマップUIの表示・非表示を切り替える ▼▼▼
            if (m_MinimapUI != null)
            {
                m_MinimapUI.SetActive(isPlaying);
            }
            // ▲▲▲ ここまで ▲▲▲

            FindPlayer1Camera();

            if (player1Camera != null)
            {
                player1Camera.gameObject.SetActive(isPlaying);
            }
        }

        private void FindPlayer1Camera()
        {
            if (m_GameManager == null || m_GameManager.m_SpawnPoints == null) return;

            for (int i = 0; i < m_GameManager.m_SpawnPoints.Length; i++)
            {
                // nullチェック
                if (m_GameManager.m_SpawnPoints[i] != null)
                {
                    // ▼▼▼ 修正: PlayerNumber ではなく ControlIndex で判定する ▼▼▼
                    // ControlIndex が 1 の戦車（＝プレイヤー1の入力を持つ戦車）を探す
                    if (m_GameManager.m_SpawnPoints[i].ControlIndex == 1)
                    {
                        GameObject tankInstance = m_GameManager.m_SpawnPoints[i].m_Instance;
                        if (tankInstance != null)
                        {
                            // 非アクティブなものも含めてカメラを探す
                            Camera cam = tankInstance.GetComponentInChildren<Camera>(true);
                            if (cam != null)
                            {
                                player1Camera = cam;
                                // 見つけたら一旦OFFにする（HandleGameStateChangedで制御するため）
                                player1Camera.gameObject.SetActive(false);
                            }
                        }
                        break; // 見つかったのでループ終了
                    }
                    // ▲▲▲ 修正箇所ここまで ▲▲▲
                }
            }
        }

        private void HandleWeaponStockChanged(WeaponStockData weaponData, int controlIndex)
        {
            GameObject targetObject = null;

            if (controlIndex == 1) targetObject = m_Player1StockObject;
            else if (controlIndex == 2) targetObject = m_Player2StockObject;

            if (targetObject != null)
            {
                PlayerStock playerStock = targetObject.GetComponent<PlayerStock>();
                if (playerStock != null)
                {
                    playerStock.UpdatePlayerStock(weaponData);
                }
            }
        }

        private void OnDestroy()
        {
            if (m_GameManager != null)
            {
                m_GameManager.OnGameStateChanged -= HandleGameStateChanged;

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
            }
        }
    }
}