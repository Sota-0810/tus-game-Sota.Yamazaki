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
                    // インデックスが勝利数未満なら表示(true)、それ以上なら非表示(false)
                    // 例: 2勝の場合、i=0,1 が true になり、i=2,3,4 が false になる
                    m_WinImages[i].gameObject.SetActive(i < winCount);
                }
            }
        }
    }
}