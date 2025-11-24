using System; // 手順2: Serializable属性のために必要
using UnityEngine;

namespace Tanks.Complete
{
    // 手順1, 2: クラスの作成と Serializable 属性の付与
    [Serializable]
    public class WeaponStockData
    {
        // === 手順3: SerializeField 属性付きの private 変数定義 ===
        
        [Tooltip("武器の名前")]
        [SerializeField]
        public string m_WeaponName;

        [Tooltip("武器の所持数の初期値")]
        [SerializeField]
        private int m_InitialQuantity;

        [Tooltip("所持できる武器の最大数")]
        [SerializeField]
        private int m_MaxCapacity;

        [Tooltip("武器カートリッジを取得した際に補充される数")]
        [SerializeField]
        private int m_ReplenishQuantity;


        // === 手順4: 現在の所持数とGetter ===
        
        // 現在の武器の所持数
        private int m_CurrentQuantity;

        // private変数を返すpublicなgetter
        public int CurrentQuantity
        {
            get { return m_CurrentQuantity; }
        }


        // === 手順5: 増減を行うpublicメソッド ===

        /// <summary>
        /// 現在所持数を初期化する
        /// </summary>
        public void InitializeQuantity()
        {
            m_CurrentQuantity = m_InitialQuantity;
        }

        /// <summary>
        /// 現在の所持数を増やす (最大値を超えないようにする)
        /// </summary>
        public void Replenish()
        {
            // 現在数 + 補充数 と 最大値 の小さい方を採用する（最大値キャップ）
            m_CurrentQuantity = Mathf.Min(m_CurrentQuantity + m_ReplenishQuantity, m_MaxCapacity);
        }

        /// <summary>
        /// 現在の所持数をデクリメントする (ゼロを下回らないようにする)
        /// </summary>
        public void Use()
        {
            // 1減らした後、0と比較して大きい方を採用する（0で止まる）
            m_CurrentQuantity = Mathf.Max(m_CurrentQuantity - 1, 0);
        }
    }
}