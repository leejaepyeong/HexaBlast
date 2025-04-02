using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectPuzzle
{
    public class BlockManager : MonoBehaviour
    {
        public static BlockManager Instance;
        public GridLayoutGroup grid;
        public GameObject backBlockPrefab;

        private List<GameObject> backBlockList = new List<GameObject>();

        public int Point;
        public int mapSizeX;
        public int mapSizeY;

        public bool isClick;
        public bool isProcess;
        public BlockItem selectBlock;

        private bool isInit;
        private StageSettingData stageData;

        private List<BlockItem> blockList = new List<BlockItem>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            Init();
        }

        private void Update()
        {
            for (int i = 0; i < blockList.Count; i++)
            {
                blockList[i].UpdateFrame(Time.deltaTime);
            }
        }

        public void Init()
        {
            SettingMap();
            Point = 0;
        }

        private void SettingMap()
        {
            stageData = Manager.Instance.curStage;
            int mapSize = stageData.mapSize.x * stageData.mapSize.y;
            grid.constraintCount = stageData.mapSize.x;

            backBlockList.Clear();
            for (int i = 0; i < mapSize; i++)
            {
                var obj = Instantiate(backBlockPrefab);
                backBlockList.Add(obj);
            }
        }
    }
}
