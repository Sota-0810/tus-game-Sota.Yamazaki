using UnityEngine;
using UnityEngine.UI;

public class PlayerStock : MonoBehaviour
{
    // 手順2: Shell1～9のImageコンポーネント用配列
    [SerializeField] private Image[] m_SingleShells; // Shell1-9

    // 手順2: Shells10～50のImageコンポーネント用配列
    // 0番目:Shells10, 1番目:Shells20, ... 4番目:Shells50 と割り当てます
    [SerializeField] private Image[] m_TenShells;    // Shells10-50

    // 手順3: 弾数を受け取り表示を制御するメソッド
    public void UpdatePlayerStock(int stockCount)
    {
        // --- 1の位 (Shell1～9) の制御 ---
        // ルール: 弾数が9発以上の時は常にShell1～9を表示する
        bool showAllSingles = stockCount >= 9;

        for (int i = 0; i < m_SingleShells.Length; i++)
        {
            // 配列は0始まりなので、iがstockCount未満なら表示
            // 例: 残り5発 -> i=0,1,2,3,4 (5個)を表示
            if (showAllSingles || i < stockCount)
            {
                m_SingleShells[i].gameObject.SetActive(true);
            }
            else
            {
                m_SingleShells[i].gameObject.SetActive(false);
            }
        }

        // --- 10の位 (Shells10～50) の制御 ---
        // 例: 16発 -> 1つ表示 (Shells10)
        // 例: 43発 -> 4つ表示 (Shells10, 20, 30, 40)
        int tenCount = stockCount / 10; // 整数除算で10の位の数を取得

        for (int i = 0; i < m_TenShells.Length; i++)
        {
            if (i < tenCount)
            {
                m_TenShells[i].gameObject.SetActive(true);
            }
            else
            {
                m_TenShells[i].gameObject.SetActive(false);
            }
        }
    }
}