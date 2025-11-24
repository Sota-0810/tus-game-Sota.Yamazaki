using UnityEngine;

namespace Tanks.Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask m_TankMask;                        // Used to filter what the explosion affects, this should be set to "Players".
        public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
        public AudioSource m_ExplosionAudio;                // Reference to the audio that will play on explosion.
        [HideInInspector] public float m_MaxLifeTime = 2f;  // The time in seconds before the shell is removed.

        // All those are hidden in inspector as they will actually come from the TankShooting scripts
        public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a tank.
        public float m_ExplosionForce = 50f;                // The amount of force added to a tank at the centre of the explosion.
        public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion tanks can be and are still affected.

        // ▼▼▼ 追加: 生成時刻を記録する変数 ▼▼▼
        private float m_SpawnTime;
        // ▲▲▲ ここまで ▲▲▲


        private void Start ()
        {
            // ▼▼▼ 追加: 生成時刻を記録 ▼▼▼
            m_SpawnTime = Time.time;
            // ▲▲▲ ここまで ▲▲▲
            
            // ▼▼▼ 修正箇所：地雷(Mine)でない場合のみ、時間経過で破壊する ▼▼▼
            // 画像の指示: Startメソッド内でのDestroyメソッドの使用をタグで条件分岐する
            // タグが "Mine" でない場合（通常の砲弾の場合）のみ、寿命(m_MaxLifeTime)が来たら削除する
            if (!gameObject.CompareTag("Mine"))
            {
                Destroy (gameObject, m_MaxLifeTime);
            }
            // "Mine" の場合は、誰かが踏むまで（OnTriggerEnterが呼ばれるまで）削除されない
            // ▲▲▲ 修正箇所ここまで ▲▲▲
        }


        private void OnTriggerEnter (Collider other)
        {
			// ▼▼▼ 修正: 地雷の安全装置（アーミングタイム） ▼▼▼
            // もしこれが「地雷」で、かつ「生成から1秒以内」なら、衝突を無視して爆発しない
            // (OnTriggerEnterは「入った瞬間」にしか呼ばれないため、
            //  これで設置時の自爆を防ぎつつ、一度離れてから踏めば爆発するようになります)
            if (gameObject.CompareTag("Mine") && Time.time < m_SpawnTime + 1.0f)
            {
                return;
            }
            // ▲▲▲ 修正箇所ここまで ▲▲▲

            // 地雷の場合のチェック
            if (gameObject.CompareTag("Mine"))
            {
                // 相手が戦車（Playersレイヤー）か？
                bool isTank = (m_TankMask.value & (1 << other.gameObject.layer)) > 0;
                
                // 相手が砲弾（Shellタグ または ShellExplosion持ち）か？
                bool isShell = other.gameObject.CompareTag("Shell") || other.GetComponent<ShellExplosion>() != null;

                // 戦車でも砲弾でもない（地面や壁）なら、爆発せずにスルーする
                if (!isTank && !isShell)
                {
                    return;
                }
            }
            // ▲▲▲ ここまで ▲▲▲
            
            // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
            Collider[] colliders = Physics.OverlapSphere (transform.position, m_ExplosionRadius, m_TankMask);

            // Go through all the colliders...
            for (int i = 0; i < colliders.Length; i++)
            {
                // ... and find their rigidbody.
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody> ();

                // If they don't have a rigidbody, go on to the next collider.
                if (!targetRigidbody)
                    continue;

                // Add an explosion force.
                targetRigidbody.GetComponent<TankMovement>().AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

                // Find the TankHealth script associated with the rigidbody.
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth> ();

                // If there is no TankHealth script attached to the gameobject, go on to the next collider.
                if (!targetHealth)
                    continue;

                // Calculate the amount of damage the target should take based on it's distance from the shell.
                float damage = CalculateDamage (targetRigidbody.position);

                // Deal this damage to the tank.
                targetHealth.TakeDamage (damage);
            }

            // Unparent the particles from the shell.
            m_ExplosionParticles.transform.parent = null;

            // Play the particle system.
            m_ExplosionParticles.Play();

            // Play the explosion sound effect.
            m_ExplosionAudio.Play();

            // Once the particles have finished, destroy the gameobject they are on.
            ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
            Destroy (m_ExplosionParticles.gameObject, mainModule.duration);

            // Destroy the shell.
            Destroy (gameObject);
        }


        private float CalculateDamage (Vector3 targetPosition)
        {
            // Create a vector from the shell to the target.
            Vector3 explosionToTarget = targetPosition - transform.position;

            // Calculate the distance from the shell to the target.
            float explosionDistance = explosionToTarget.magnitude;

            // Calculate the proportion of the maximum distance (the explosionRadius) the target is away.
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

            // Calculate damage as this proportion of the maximum possible damage.
            float damage = relativeDistance * m_MaxDamage;

            // Make sure that the minimum damage is always 0.
            damage = Mathf.Max (0f, damage);

            return damage;
        }
    }
}