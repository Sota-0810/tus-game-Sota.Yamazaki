using System; // Serializable属性を使うために必要
using UnityEngine;

namespace Tanks.Complete
{
    // 指示1, 2: CartridgeData クラスの作成と Serializable 属性の付与
    [Serializable]
    public class CartridgeData
    {
        // 指示3: 必要な変数をpublicで定義
        // ※画像の箇条書きには2つしかありませんが、文言の「3つの変数」は
        //   指示作成時のミスの可能性が高いため、表示されている2つを実装します。

        [Tooltip("生成するカートリッジのプレハブ")]
        public GameObject cartridgePrefab;

        [Tooltip("生成頻度 (秒)")]
        public float spawnInterval;
    }
}
