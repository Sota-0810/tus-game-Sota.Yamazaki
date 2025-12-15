using UnityEngine;
using UnityEngine.UI; // Sliderコンポーネントを扱うために必要です

namespace Tanks.Complete
{
    public class PlayerHP : MonoBehaviour
    {
        // 指示2: SerializeField 属性を持つ Slider 型の変数 HPSlider
        [Tooltip("HPバーのSliderコンポーネントを割り当ててください")]
        [SerializeField]
        private Slider HPSlider;

        // 指示2: HPの値を引数に HPSlider.value を更新する public メソッド UpdateHPSlider
        public void UpdateHPSlider(float hp)
        {
            if (HPSlider != null)
            {
                // 受け取った正規化されたHP値(0.0〜1.0)をスライダーに反映
                HPSlider.value = hp;
            }
        }
    }
}