using UnityEngine;
using Tanks.Complete; 

namespace Tanks.Complete
{
    public class TPSCameraControl : MonoBehaviour
    {
        [Tooltip("GameManagerクラスのインスタンス")]
        public GameManager gameManager;

        [Tooltip("追従対象のタンク（GameManagerから代入されます）")]
        public Transform target;

        [Header("Camera Settings")]
        [SerializeField, Tooltip("カメラとタンクの相対的な位置 (X, Y, Z)")]
        private Vector3 posOffset;

        // GameManagerからアクセス・変更できるように public に変更しました
        [Tooltip("カメラとタンクの相対的な回転 (X, Y, Z)")]
        public Vector3 rotOffset;

        private bool isRoundPlaying;

        private void Start()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameManager.GameLoopState newState)
        {
            isRoundPlaying = (newState == GameManager.GameLoopState.RoundPlaying);
        }

        private void FixedUpdate()
        {
            if (target == null) return;

            // ▼▼▼ 修正: 相手のスケール(大きさ)を無視して位置計算する ▼▼▼
            
            // ターゲットの「位置」に、「ターゲットの向き」に合わせて回転させた「オフセット(距離)」を足す
            // これならターゲットが豆粒のようなサイズでも、距離は縮まりません
            transform.position = target.position + (target.rotation * posOffset);

            // ▲▲▲ 修正箇所ここまで ▲▲▲

            // 回転の更新（こちらは以前のままでOK）
            transform.rotation = target.rotation * Quaternion.Euler(rotOffset);
        }
    }
}