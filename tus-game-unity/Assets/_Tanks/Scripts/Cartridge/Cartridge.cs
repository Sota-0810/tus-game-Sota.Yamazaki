using UnityEngine;

namespace Tanks.Complete
{
    // MonoBehaviour を継承
    public class Cartridge : MonoBehaviour
    {
        // === フィールド定義 (指示2) ===
        [Tooltip("砲弾カートリッジが点滅してから消滅するまでの時間 (秒)")]
        public float m_LifeTime = 15f;

        [Tooltip("点滅の間隔 (秒)")]
        public float m_BlinkInterval = 0.5f;

        // === ここから追加 ===
        [Tooltip("消滅する何秒前から点滅を開始するか")]
        public float m_BlinkStartTime = 5f; // 例: 消える5秒前から点滅
        // === ここまで追加 ===

        private float m_BlinkTimer; // 点滅のためのタイマー
        private Renderer m_Renderer; // Rendererコンポーネントの参照
        // === ここまで ===


        private void Start()
        {
            // === Rendererコンポーネントの参照を取得 (指示2) ===
            m_Renderer = GetComponent<Renderer>();
            // === ここまで ===

            // タイマーを初期化
            m_BlinkTimer = m_BlinkInterval;
        }

        private void Update()
        {
            // 1. 残り時間を減らす
            m_LifeTime -= Time.deltaTime;

            // 2. 残り時間が0になったら消滅させる
            if (m_LifeTime <= 0f)
            {
                Destroy(gameObject);
                return; // 処理を終了
            }

            // 3. 点滅開始時間（例: 残り5秒）になったかどうかを判断
            if (m_LifeTime <= m_BlinkStartTime)
            {
                // --- 点滅処理 ---
                // 点滅タイマーを減らす
                m_BlinkTimer -= Time.deltaTime;

                // タイマーが0になったら
                if (m_BlinkTimer <= 0f)
                {
                    // Renderer の有効/無効を切り替えて点滅させる
                    m_Renderer.enabled = !m_Renderer.enabled;

                    // タイマーをリセット
                    m_BlinkTimer = m_BlinkInterval;
                }
                // --- ここまで点滅処理 ---
            }
            // 4. まだ点滅開始時間ではない場合
            else
            {
                // 点滅させず、確実に表示させておく
                // (万が一、点滅の途中で非表示のまま止まらないようにするため)
                if (!m_Renderer.enabled)
                {
                    m_Renderer.enabled = true;
                }
            }
        }
    }
}