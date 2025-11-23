using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System; // 変更: Actionを使用するために追加

namespace Tanks.Complete
{
    public class TankShooting : MonoBehaviour
    {
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
        [Tooltip("The speed in unit/second the shell have when fired at minimum charge")]
        public float m_MinLaunchForce = 5f;        // The force given to the shell if the fire button is not held.
        [Tooltip("The speed in unit/second the shell have when fired at max charge")]
        public float m_MaxLaunchForce = 20f;        // The force given to the shell if the fire button is held for the max charge time.
        [Tooltip("The maximum time spent charging. When charging reach that time, the shell is fired at MaxLaunchForce")]
        public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
        [Tooltip("The time that must pass before being able to shoot again after a shot")]
        public float m_ShotCooldown = 1.0f;         // The time required between 2 shots
        [Header("Shell Properties")]
        [Tooltip("The amount of health removed to a tank if they are exactly on the landing spot of a shell")]
        public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a tank.
        [Tooltip("The force of the explosion at the shell position. Keep it 50 and below")]
        public float m_ExplosionForce = 50f;              // The amount of force added to a tank at the centre of the explosion.
        [Tooltip("The radius of the explosion in Unity unit. Force decrease with distance to the center, and an tank further than this from the shell explosion won't be impacted by the explosion")]
        public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion tanks can be and are still affected.

        // === 砲弾システム (指示1) ===
        [Header("Shell Ammunition")]
        [Tooltip("ゲーム開始時の砲弾の所持数")]
        public int m_StartingShells = 10;
        [Tooltip("所持可能な砲弾の最大数")]
        public int m_MaxShells = 50;
        [Tooltip("砲弾カートリッジを取得したときに補充する数")]
        public int m_ShellsPerCartridge = 10;
        public int m_CurrentShells;                // 現在の砲弾の所持数

        // ▼▼▼ 追加箇所：手順1 ▼▼▼
        // 砲弾数が変化したことを通知するイベント
        public event Action<int> OnShellStockChanged;
        // ▲▲▲ 追加箇所ここまで ▲▲▲

        // === ここまで ===
        
        [HideInInspector]
        public TankInputUser m_InputUser;           // The Input User component for that tanks. Contains the Input Actions. 
        
        public float CurrentChargeRatio =>
            (m_CurrentLaunchForce - m_MinLaunchForce) / (m_MaxLaunchForce - m_MinLaunchForce); //The charging amount between 0-1
        public bool IsCharging => m_IsCharging;

        public bool m_IsComputerControlled { get; set; } = false;

        private string m_FireButton;                // The input axis that is used for launching shells.
        private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
        private bool m_Fired;                       // Whether or not the shell has been launched with this button press.
        private bool m_HasSpecialShell;             // has the tank a shell that makes extra damage?
        private float m_SpecialShellMultiplier;     // The amount that the special shell will multiply the damage.
        private InputAction fireAction;             // The Input Action for shooting, retrieve from TankInputUser
        private bool m_IsCharging = false;          // Are we currently charging the shot
        private float m_BaseMinLaunchForce;         // The initial value of m_MinLaunchForce
        private float m_ShotCooldownTimer;          // The timer counting down before a shot is allowed again

        // ▼▼▼ 追加箇所：飛距離ゲージの増減方向を管理する変数 ▼▼▼
        private bool m_ChargingForward;             // trueなら伸びる、falseなら縮む
        // ▲▲▲ 追加箇所ここまで ▲▲▲
        
        private void OnEnable()
        {
            // When the tank is turned on, reset the launch force, the UI and the power ups
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_BaseMinLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_BaseMinLaunchForce;
            m_HasSpecialShell = false;
            m_SpecialShellMultiplier = 1.0f;

            m_AimSlider.minValue = m_MinLaunchForce;
            m_AimSlider.maxValue = m_MaxLaunchForce;
        }

        private void Awake()
        {
            m_InputUser = GetComponent<TankInputUser>();
            if (m_InputUser == null)
                m_InputUser = gameObject.AddComponent<TankInputUser>();
        }

        private void Start ()
        {
            // The fire axis is based on the player number.
            m_FireButton = "Fire";
            fireAction = m_InputUser.ActionAsset.FindAction(m_FireButton);
            
            fireAction.Enable();

            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

            // === 砲弾の初期化 (指示2) ===
            m_CurrentShells = m_StartingShells;

            // ▼▼▼ 追加箇所：手順1 ▼▼▼
            // 初期化時に現在の弾数を通知する
            if (OnShellStockChanged != null)
            {
                OnShellStockChanged(m_CurrentShells);
            }
            // ▲▲▲ 追加箇所ここまで ▲▲▲

            // === ここまで ===
        }


        private void Update ()
        {
            // Computer and Human control Tank use 2 different update functions 
            if (!m_IsComputerControlled)
            {
                HumanUpdate();
            }
            else
            {
                ComputerUpdate();
            }
        }

        /// <summary>
        /// Used by AI to start charging
        /// </summary>
        public void StartCharging()
        {
            m_IsCharging = true;
            // ... reset the fired flag and reset the launch force.
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            // Change the clip to the charging clip and start it playing.
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play ();
        }

        public void StopCharging()
        {
            if (m_IsCharging)
            {
                Fire();
                m_IsCharging = false;
            }
        }

        void ComputerUpdate()
        {
            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_BaseMinLaunchForce;

            // If the max force has been exceeded and the shell hasn't yet been launched...
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire ();
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (m_IsCharging && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (fireAction.WasReleasedThisFrame() && !m_Fired)
            {
                // ... launch the shell.
                Fire ();
                m_IsCharging = false;
            }
        }
        
        void HumanUpdate()
        {
            // if there is a cooldown timer, decrement it
            if (m_ShotCooldownTimer > 0.0f)
            {
                m_ShotCooldownTimer -= Time.deltaTime;
            }
            
            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_BaseMinLaunchForce;

            // ▼▼▼ 修正箇所：飛距離ゲージの往復ロジック ▼▼▼

            // 1. ボタンを押し始めたとき (Start Pressing)
            if (m_CurrentShells > 0 && m_ShotCooldownTimer <= 0 && fireAction.WasPressedThisFrame())
            {
                // フラグをリセットし、最小値から開始
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                // 最初はゲージが「伸びる」方向にする
                m_ChargingForward = true;

                // チャージ音を再生
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play ();
            }
            // 2. ボタンを押し続けているとき (Charging)
            else if (fireAction.IsPressed() && !m_Fired)
            {
                // 伸びる方向の場合
                if (m_ChargingForward)
                {
                    m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                    // 最大を超えたら、方向を「縮む」に反転
                    if (m_CurrentLaunchForce >= m_MaxLaunchForce)
                    {
                        m_CurrentLaunchForce = m_MaxLaunchForce;
                        m_ChargingForward = false;
                    }
                }
                // 縮む方向の場合
                else
                {
                    m_CurrentLaunchForce -= m_ChargeSpeed * Time.deltaTime;

                    // 最小を下回ったら、方向を「伸びる」に反転
                    if (m_CurrentLaunchForce <= m_MinLaunchForce)
                    {
                        m_CurrentLaunchForce = m_MinLaunchForce;
                        m_ChargingForward = true;
                    }
                }

                // スライダーに反映
                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // 3. ボタンを離したとき (Release / Fire)
            else if (fireAction.WasReleasedThisFrame() && !m_Fired)
            {
                // 発射
                Fire ();
            }
            
            // ▲▲▲ 修正箇所ここまで ▲▲▲
        }


        private void Fire ()
        {
            // Set the fired flag so only Fire is only called once.
            m_Fired = true;

            // === 砲弾を消費 (指示2) ===
            // (HumanUpdate/StartChargingでチャージ開始時に弾数チェックが済んでいる前提)
            m_CurrentShells--;

            // ▼▼▼ 追加箇所：手順1 ▼▼▼
            // 発射して弾数が減ったことを通知する
            if (OnShellStockChanged != null)
            {
                OnShellStockChanged(m_CurrentShells);
            }
            // ▲▲▲ 追加箇所ここまで ▲▲▲

            // === ここまで ===

            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance =
                Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.linearVelocity = m_CurrentLaunchForce * m_FireTransform.forward;

            ShellExplosion explosionData = shellInstance.GetComponent<ShellExplosion>();
            explosionData.m_ExplosionForce = m_ExplosionForce;
            explosionData.m_ExplosionRadius = m_ExplosionRadius;
            explosionData.m_MaxDamage = m_MaxDamage;
            
            // Increase the damage if extra damage PowerUp is active
            if (m_HasSpecialShell)
            {
                explosionData.m_MaxDamage *= m_SpecialShellMultiplier;
                // Reset the default values after increasing the damage of the fired shell
                m_HasSpecialShell = false;
                m_SpecialShellMultiplier = 1f;
                
                PowerUpDetector powerUpDetector = GetComponent<PowerUpDetector>();
                if (powerUpDetector != null)
                    powerUpDetector.m_HasActivePowerUp = false;

                PowerUpHUD powerUpHUD = GetComponentInChildren<PowerUpHUD>();
                if (powerUpHUD != null)
                    powerUpHUD.DisableActiveHUD();
            }

            // Change the clip to the firing clip and play it.
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play ();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = m_MinLaunchForce;

            m_ShotCooldownTimer = m_ShotCooldown;
        }


        public void EquipSpecialShell(float damageMultiplier)
        {
            m_HasSpecialShell = true;
            m_SpecialShellMultiplier = damageMultiplier;
        }

        /// <summary>
        /// Return the estyimated position the projectile will have with the charging level (between 0 & 1)
        /// </summary>
        /// <param name="chargingLevel">The fire charging level between 0 - 1</param>
        /// <returns>The position at which the projectile will be (ignore obstacle)</returns>
        public Vector3 GetProjectilePosition(float chargingLevel)
        {
            float chargeLevel = Mathf.Lerp (m_MinLaunchForce, m_MaxLaunchForce, chargingLevel);
            Vector3 velocity = chargeLevel * m_FireTransform.forward; 
            
            float a = 0.5f * Physics.gravity.y;
            float b = velocity.y;
            float c = m_FireTransform.position.y;
            
            float sqrtContent = b * b - 4 * a * c;
            //no solution
            if (sqrtContent <= 0)
            {
                return m_FireTransform.position;
            }

            float answer1 = (-b + Mathf.Sqrt(sqrtContent)) / (2 * a);
            float answer2 = (-b - Mathf.Sqrt(sqrtContent)) / (2 * a);

            float answer = answer1 > 0 ? answer1 : answer2;
            
            Vector3 position = m_FireTransform.position +
                               new Vector3(velocity.x, 0, velocity.z) *
                               answer;
            position.y = 0;

            return position;
        }

        // === 砲弾を補充する (指示3) ===
        /// <summary>
        /// 砲弾カートリッジを取得したときに砲弾を補充します。
        /// このメソッドは外部（例：砲弾カートリッジの衝突判定スクリプト）から呼ばれます。
        /// </summary>
        public void AddShells()
        {
            // m_ShellsPerCartridge の分だけ砲弾を増やし、m_MaxShells を上限とする
            m_CurrentShells = Mathf.Min(m_CurrentShells + m_ShellsPerCartridge, m_MaxShells);

            // ▼▼▼ 追加箇所：手順1 ▼▼▼
            // 補充されて弾数が増えたことを通知する
            if (OnShellStockChanged != null)
            {
                OnShellStockChanged(m_CurrentShells);
            }
            // ▲▲▲ 追加箇所ここまで ▲▲▲
            
        }
        // === ここまで ===

        // === 他のオブジェクトとの衝突時に呼ばれる (指示5) ===
        private void OnCollisionEnter(Collision other)
        {
            // 衝突したオブジェクトのタグが "ShellCartridge" かどうかをチェック
            if (other.gameObject.CompareTag("ShellCartridge"))
            {
                // 砲弾を補充する
                AddShells();

                // 衝突したカートリッジをシーンから削除する
                Destroy(other.gameObject);
            }
        }
        // === ここまで ===
    }
}