using UnityEngine;
using UnityEngine.UI;
using Tanks.Complete;

public class PlayerStock : MonoBehaviour
{
    // 手順2: Shell1～9のImageコンポーネント用配列
    [SerializeField] private Image[] m_SingleShells; // Shell1-9

    // 手順2: Shells10～50のImageコンポーネント用配列
    // 0番目:Shells10, 1番目:Shells20, ... 4番目:Shells50 と割り当てます
    [SerializeField] private Image[] m_TenShells;    // Shells10-50

    // ▼▼▼ 追加箇所：手順1（地雷用UIの配列定義） ▼▼▼
    [Header("Mine UI")]
    [Tooltip("地雷のアイコン (Mine1, Mine2, Mine3)")]
    [SerializeField] private Image[] mineImages;
    // ▲▲▲ 追加箇所ここまで ▲▲▲

    private const float INACTIVE_ALPHA = 0.15f;

    // 手順3: 弾数を受け取り表示を制御するメソッド
    public void UpdatePlayerStock(WeaponStockData weaponData)
    {
        // 受け取ったデータの名前を見て、処理を分岐する
        // (※プレハブの設定で Weapon Name を "Shell" や "Mine" にしている前提です)
        
        // --- 砲弾 (Shell) の場合 ---
        // 名前が "Shell" か、もしくは空文字なら従来の砲弾処理を行う（互換性のため）
        if (weaponData.m_WeaponName == "Shell" || string.IsNullOrEmpty(weaponData.m_WeaponName))
        {
            UpdateShellUI(weaponData.CurrentQuantity);
        }
        // --- 地雷 (Mine) の場合 ---
        else if (weaponData.m_WeaponName == "Mine")
        {
            UpdateMineUI(weaponData.CurrentQuantity);
        }
    }

    // 従来の砲弾表示ロジックを別メソッドに切り出しました
    // private void UpdateShellUI(int stockCount)
    // {
    //     // --- 1の位 (Shell1～9) の制御 ---
    //     bool showAllSingles = stockCount >= 9;

    //     for (int i = 0; i < m_SingleShells.Length; i++)
    //     {
    //         if (showAllSingles || i < stockCount)
    //         {
    //             m_SingleShells[i].gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             m_SingleShells[i].gameObject.SetActive(false);
    //         }
    //     }

    //     // --- 10の位 (Shells10～50) の制御 ---
    //     int tenCount = stockCount / 10; 

    //     for (int i = 0; i < m_TenShells.Length; i++)
    //     {
    //         if (i < tenCount)
    //         {
    //             m_TenShells[i].gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             m_TenShells[i].gameObject.SetActive(false);
    //         }
    //     }
    // }

    // 従来の砲弾表示ロジック（半透明対応版）
    private void UpdateShellUI(int stockCount)
    {
        // --- 1の位 (Shell1～9) の制御 ---
        // 9発以上ある場合は、1の位のゲージはすべて点灯（不透明）扱い
        bool showAllSingles = stockCount >= 9;

        for (int i = 0; i < m_SingleShells.Length; i++)
        {
            if (m_SingleShells[i] != null)
            {
                // 持っている分は「有効」、持っていない分は「無効」として色を変更
                bool isActive = (showAllSingles || i < stockCount);
                SetImageAlpha(m_SingleShells[i], isActive);
            }
        }

        // --- 10の位 (Shells10～50) の制御 ---
        int tenCount = stockCount / 10; 

        for (int i = 0; i < m_TenShells.Length; i++)
        {
            if (m_TenShells[i] != null)
            {
                // 持っている分は「有効」、持っていない分は「無効」
                bool isActive = (i < tenCount);
                SetImageAlpha(m_TenShells[i], isActive);
            }
        }
    }

    // ▼▼▼ 追加箇所：手順2（地雷の表示ロジック） ▼▼▼
    // private void UpdateMineUI(int stockCount)
    // {
    //     // mineImages配列の要素数だけループ
    //     for (int i = 0; i < mineImages.Length; i++)
    //     {
    //         // インデックス(i)が所持数(stockCount)未満なら表示、それ以外は非表示
    //         // 例: 所持2個 -> i=0(表示), i=1(表示), i=2(非表示)
    //         if (i < stockCount)
    //         {
    //             mineImages[i].gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             mineImages[i].gameObject.SetActive(false);
    //         }
    //     }
    // }
    // ▲▲▲ 追加箇所ここまで ▲▲▲

    // ▼▼▼ 追加箇所：手順2（地雷の表示ロジック 半透明対応版） ▼▼▼
    private void UpdateMineUI(int stockCount)
    {
        for (int i = 0; i < mineImages.Length; i++)
        {
            if (mineImages[i] != null)
            {
                // インデックス(i)が所持数(stockCount)未満なら有効（不透明）
                bool isActive = (i < stockCount);
                SetImageAlpha(mineImages[i], isActive);
            }
        }
    }
    // ▲▲▲ 追加箇所ここまで ▲▲▲

    // ▼▼▼ 共通処理：画像の透明度を変更するヘルパーメソッド ▼▼▼
    private void SetImageAlpha(Image targetImage, bool isActive)
    {
        // まずオブジェクト自体は必ず表示状態にする（非表示だと色を変えても見えないため）
        targetImage.gameObject.SetActive(true);

        // 現在の色を取得
        Color c = targetImage.color;

        // 有効なら不透明(1.0)、無効なら半透明(INACTIVE_ALPHA)
        c.a = isActive ? 1.0f : INACTIVE_ALPHA;

        // 色を適用
        targetImage.color = c;
    }
    // ▲▲▲ ここまで ▲▲▲
}