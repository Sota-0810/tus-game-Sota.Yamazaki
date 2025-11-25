using System.Collections; // コルーチン (IEnumerator) を使うために必要
using UnityEngine;

// (指示6のヒント: 名前空間の衝突に注意)
// 以下の行を追加します。Tanks.Complete 名前空間の GameManager を参照するために必要です。
using Tanks.Complete;

namespace Tanks.Complete
{
    // 指示1, 2: CartridgeSpawner クラスの定義とフィールド
    public class CartridgeSpawner : MonoBehaviour
    {
        // === 指示4: CartridgeData型の変数を定義し、古い変数を削除 ===
        // 削除: m_ShellCartridgePrefab, m_SpawnInterval は削除しました。
        
        [Header("Cartridge Settings")]
        [Tooltip("砲弾カートリッジのデータ")]
        [SerializeField]
        private CartridgeData shellCartridgeData;

        [Tooltip("地雷カートリッジのデータ")]
        [SerializeField]
        private CartridgeData mineCartridgeData;
        // === ここまで ===

        [Tooltip("砲弾カートリッジを生成する範囲を指定するための変数")]
        public Vector3 m_SpawnArea = new Vector3(40f, 1.09f, 40f);

        // === ここまで ===

        // === ここから追加 (指示) ===
        [Tooltip("生成をチェックする際の、カートリッジのおおよその半径 (m)")]
        public float m_SpawnCheckRadius = 0.5f;

        [Tooltip("安全な場所を見つけるための最大試行回数")]
        public int m_MaxSpawnAttempts = 10;
        // === ここまで追加 ===

        // === レイヤーマスクのフィールドを追加 ===
        [Tooltip("障害物として検知するレイヤー")]
        public LayerMask m_BlockingLayerMask;
        // === ここまで追加 ===

        // === 課題6: GameManager オブジェクトへの参照を保持する変数 ===
        [Tooltip("GameManager への参照")]
        public GameManager m_GameManager;
        // === ここまで ===


        // === 課題7: Start メソッドでイベントの購読（待ち受け）設定を行う ===
        private void Start()
        {
            // (指示8) で GameManager がインスペクタから設定されているかチェック
            if (m_GameManager == null)
            {
                Debug.LogError("CartridgeSpawner: GameManager が割り当てられていません！インスペクタを確認してください。", this);
                return; // GameManager がないと動作できない
            }

            // GameManager の OnGameStateChanged イベント（通知）に対して、
            // 私たちの HandleGameStateChanged メソッドを「登録（購読）」します
            m_GameManager.OnGameStateChanged += HandleGameStateChanged;

            // (注意: 以前ここにあった StartCoroutine(SpawnRoutine()) は削除します。
            //  GameManager からの通知で開始するように変更するためです)
        }
        // === ここまで ===


        // === 課題5: GameManager の状態変化を処理するメソッド ===
        /// <summary>
        /// GameManager の OnGameStateChanged イベントから呼び出されるメソッド
        /// </summary>
        /// <param name="newState">GameManager から通知された新しい状態</param>
        private void HandleGameStateChanged(GameManager.GameLoopState newState)
        {
            // ログを出力して、イベントが届いたことを確認 (デバッグ用)
            // Debug.Log("CartridgeSpawner が新しい状態を受信: " + newState);

            // まず、実行中かもしれない古いコルーチンをすべて停止する
            // (例: RoundEnding になった時、または次の RoundPlaying に備えるため)
            StopAllCoroutines();

            // 新しい状態が「プレイ中」の場合のみ、新しいスポーンコルーチンを開始する
            if (newState == GameManager.GameLoopState.RoundPlaying)
            {
                // 砲弾と地雷、それぞれの生成処理(コルーチン)を開始する
                StartCoroutine(SpawnRoutine(shellCartridgeData));
                StartCoroutine(SpawnRoutine(mineCartridgeData));
            }
        }
        // === ここまで ===

        private void OnDestroy()
        {
        // GameManager が null でないことを確認
            if (m_GameManager != null)
            {
            // 購読（+=）したイベントは、必ず購読解除（-=）する
            m_GameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        // 指示4: 砲弾カートリッジを定期的に生成する SpawnRoutine コルーチン
        private IEnumerator SpawnRoutine(CartridgeData data)
        {
            // 無限ループ（ゲームが続く限り実行）
            while (true)
            {
                // 指示3のメソッドを呼び出してカートリッジを生成する
                SpawnCartridge(data);

                // "yield return" で処理を一時停止する
                // m_SpawnInterval (例: 10秒) 待ってから、ループの最初に戻る
                yield return new WaitForSeconds(data.spawnInterval);
            }
        }

        // 指示3: 砲弾カートリッジを生成する SpawnCartridge メソッド
        private void SpawnCartridge(CartridgeData data)
        {
            // m_MaxSpawnAttempts の回数だけ、安全な場所を試行する
            for (int i = 0; i < m_MaxSpawnAttempts; i++)
            {
                // 1. ランダムな座標を計算
                // -m_SpawnArea.x / 2 から +m_SpawnArea.x / 2 までのランダムな値
                float randomX = Random.Range(-m_SpawnArea.x / 2, m_SpawnArea.x / 2);
                float randomZ = Random.Range(-m_SpawnArea.z / 2, m_SpawnArea.z / 2);
                
                // スポーンする高さ (m_SpawnArea.y の値を使う)
                float fixedY = m_SpawnArea.y;

                // スポーンする位置を計算
                // (CartridgeSpawnerオブジェクト自身の位置を基準にする)
                Vector3 spawnPosition = new Vector3(randomX, fixedY, randomZ) + transform.position;

                // 2. その座標が安全か (他の物と重なっていないか) チェックする
                // Physics.CheckSphere は、指定した位置に指定した半径の球を置き、
                // 何かと衝突する(true)か、しない(false)かを瞬時に判定する
                //bool isBlocked = Physics.CheckSphere(spawnPosition, m_SpawnCheckRadius
                // 2. その座標が安全か (指定したレイヤーの物と重なっていないか) チェックする
                bool isBlocked = Physics.CheckSphere(spawnPosition, m_SpawnCheckRadius, m_BlockingLayerMask);

                // 3. もし衝突しない (isBlocked が false) なら、そこは安全な場所
                if (!isBlocked)
                {
                    // 安全なので、オブジェクトを生成
                    Quaternion spawnRotation = Quaternion.identity;
                    Instantiate(data.cartridgePrefab, spawnPosition, spawnRotation);

                    // 生成に成功したので、このメソッドを終了する (forループからも抜ける)
                    return;
                }

                // もし isBlocked が true だった場合、for ループの次の回に進み、
                // 新しいランダムな座標で再試行する
            }

            // 4. for ループが最大回数実行されても安全な場所が見つからなかった場合
            // (今回は生成をあきらめ、次の m_SpawnInterval を待つ)
            Debug.LogWarning("CartridgeSpawner: 安全なスポーン場所が見つからなかったため、今回の生成をスキップします。");
        }

        // === ↑↑ このメソッドを丸ごと置き換えてください ↑↑ ===


        
    }
}
