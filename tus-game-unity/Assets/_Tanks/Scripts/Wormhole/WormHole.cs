using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tanks.Complete; // TankHealthを参照するために必要

namespace Tanks.Complete
{
    public class WormHole : MonoBehaviour
    {
        // === 手順2: SerializeField属性付きのフィールド ===
        [Tooltip("移動先のワームホール")]
        [SerializeField]
        private WormHole m_Destination;

        [Tooltip("ワープまでの時間（待機時間）")]
        [SerializeField]
        private float m_WarpDuration = 2.0f;

        // === 手順3: 全ワームホールで共有するクールダウン管理 ===
        // TankHealthをキー、クールダウン解除時刻(float)を値とする辞書
        private static Dictionary<TankHealth, float> m_TankCoolDowns;

        private void Awake()
        {
            // 辞書がまだ作られていなければ初期化する
            if (m_TankCoolDowns == null)
            {
                m_TankCoolDowns = new Dictionary<TankHealth, float>();
            }
        }

        // === 手順5: 衝突判定 ===
        private void OnTriggerEnter(Collider other)
        {
            // 衝突した相手からTankHealthを取得
            TankHealth targetHealth = other.GetComponent<TankHealth>();

            // 戦車であればワープ処理を開始
            if (targetHealth != null)
            {
                StartCoroutine(WarpTank(other, targetHealth));
            }
        }

        // === 手順4: ワープ処理のコルーチン ===
        private IEnumerator WarpTank(Collider tank, TankHealth tankHealth)
        {
            // --- 1. クールダウンチェック ---
            // 辞書に登録されており、かつ現在時刻が「記録された時刻」より前なら、まだクールダウン中
            if (m_TankCoolDowns.ContainsKey(tankHealth) && Time.time < m_TankCoolDowns[tankHealth])
            {
                // 処理を中断
                yield break;
            }

            // ▼▼▼ 追加: ワープ開始時に物理的な動きを完全停止させる ▼▼▼
            Rigidbody tankRb = tank.GetComponent<Rigidbody>();
            if (tankRb != null)
            {
                // 移動速度をゼロにする (Unity 6以降は linearVelocity, 古いバージョンは velocity)
                // あなたの環境に合わせて linearVelocity を使用します
                tankRb.linearVelocity = Vector3.zero; 
                
                // 回転速度もゼロにする
                tankRb.angularVelocity = Vector3.zero;

                // ▼▼▼ 追加: CPU戦車だった場合、思考(経路)をリセットさせる ▼▼▼
                TankAI tankAI = tank.GetComponent<TankAI>();
                if (tankAI != null)
                {
                    tankAI.ResetAI();
                }
                // ▲▲▲ 追加箇所ここまで ▲▲▲
            }
            // ▲▲▲ 追加箇所ここまで ▲▲▲

            // --- 2. ワープ待機（演出） ---
            // ワープまでの間、無敵状態にする
            tankHealth.ActivateInvincibility(m_WarpDuration);

            // 指定時間待つ
            yield return new WaitForSeconds(m_WarpDuration);

            // --- 3. 移動処理 ---
            if (m_Destination != null)
            {
                // // 目的地へ移動（Transformを直接書き換え）
                // tank.transform.position = m_Destination.transform.position;

                //▼▼▼ 修正: 位置と回転の計算を厳密にする ▼▼▼
                
                // 1. 位置の計算: ワームホールの「真上」ではなく「同じ高さ」にする
                // ワームホールのXZ座標だけ使い、Y座標（高さ）は戦車の「今の高さ」を維持する
                Vector3 targetPos = m_Destination.transform.position;
                targetPos.y = tank.transform.position.y; 
                
                tank.transform.position = targetPos;
                
                // 向きも合わせる場合は以下を追加（今回は位置のみでOKですが、あると自然です）
                tank.transform.rotation = m_Destination.transform.rotation;

                // ▼▼▼ 念のため: ワープ先に出た瞬間も速度をゼロにしておく ▼▼▼
                if (tankRb != null)
                {
                    tankRb.linearVelocity = Vector3.zero;
                    tankRb.angularVelocity = Vector3.zero;
                }
                // ▲▲▲ ここまで ▲▲▲
            }

            // --- 4. クールダウン記録 ---
            // ワープ完了後、少しの間（例: 3秒間）は再ワープできないように未来の時刻を記録する
            // これがないと、出口に出た瞬間にまた入り口に戻される無限ループが起きます
            float cooldownTime = 3.0f; // 再転送防止のためのバッファ時間

            if (m_TankCoolDowns.ContainsKey(tankHealth))
            {
                m_TankCoolDowns[tankHealth] = Time.time + cooldownTime;
            }
            else
            {
                m_TankCoolDowns.Add(tankHealth, Time.time + cooldownTime);
            }

            // --- 5. ワープ後の無敵 ---
            // 出口に出た直後も少しだけ無敵にして、出待ち攻撃を防ぐ
            tankHealth.ActivateInvincibility(1.0f);
        }
    }
}
