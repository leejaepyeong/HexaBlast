using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace ProjectPuzzle
{
    public class StageSlot : MonoBehaviour
    {
        [SerializeField] private Button buttonStart;
        [SerializeField] private TextMeshProUGUI textStage;
        [SerializeField] private GameObject objLock;

        private StageSettingData stageData;

        public void Init(StageSettingData data)
        {
            stageData = data;
            gameObject.SetActive(true);
            buttonStart.onClick.AddListener(OnClickStartGame);
            textStage.text = $"Stage {data.floor}";
        }

        private void OnClickStartGame()
        {
            Manager.Instance.OnClickPlay(stageData);
        }
    }
}
