using UnityEngine;
using UnityEngine.UI;

namespace Tanks.Complete
{
    public class PlayerWinCount : MonoBehaviour
    {
        // 指示3: Win1からWin5のImageを参照する配列
        [Header("UI References")]
        [Tooltip("Win1 から Win5 までの Image オブジェクトを順番に割り当ててください")]
        [SerializeField]
        private Image[] m_WinImages;

        private const float INACTIVE_ALPHA = 0.15f;

        // 指示3: 勝利数を引数に、アイコンを点灯させるメソッド
        /// <summary>
        /// 勝利数を受け取り、その数だけアイコンを表示（点灯）させます
        /// </summary>
        /// <param name="winCount">現在の勝利数</param>
        public void UpdateWinCount(int winCount)
        {
            // 配列に登録された全ての画像を確認
            for (int i = 0; i < m_WinImages.Length; i++)
            {
                if (m_WinImages[i] != null)
                {
                    // ▼▼▼ 修正: 常に表示状態にする（非表示にしない） ▼▼▼
                    m_WinImages[i].gameObject.SetActive(true);

                    // 現在の色を取得
                    Color currentColor = m_WinImages[i].color;

                    if (i < winCount)
                    {
                        // 獲得済みなら「不透明（くっきり）」
                        currentColor.a = 1.0f;
                    }
                    else
                    {
                        // 未獲得なら「半透明（薄く）」
                        currentColor.a = INACTIVE_ALPHA;
                    }

                    // 色を適用する
                    m_WinImages[i].color = currentColor;
                    // ▲▲▲ ここまで ▲▲▲
                }
            }
        }
    }
}