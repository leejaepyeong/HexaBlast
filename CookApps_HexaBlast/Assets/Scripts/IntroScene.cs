using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectPuzzle
{
    public class IntroScene : MonoBehaviour
    {
        [SerializeField] private Button buttonEnd;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private StageSlot stageSlotPrefab;

        private List<StageSlot> slotList = new List<StageSlot>();

        private void Awake()
        {
            buttonEnd.onClick.AddListener(OnClickEndGame);
        }

        private void Start()
        {
            Manager.Instance.LoadJson();
            SetStageSlots();
        }

        private void SetStageSlots()
        {
            for (int i = 0; i < Manager.Instance.DataList.Count; i++)
            {
                var slot = Instantiate(stageSlotPrefab, scroll.content);
                slot.Init(Manager.Instance.DataList[i]);
                slotList.Add(slot);
            }
        }

        private void OnClickEndGame()
        {
            Application.Quit();
        }
    }
}
