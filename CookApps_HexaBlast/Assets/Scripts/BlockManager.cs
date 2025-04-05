using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using static Unity.Collections.AllocatorManager;

namespace ProjectPuzzle
{
    public class BlockManager : MonoBehaviour
    {
        [System.Serializable]
        public class MissionSlot
        {
            public GameObject objRoot;
            public Image missionIcon;
            public TextMeshProUGUI textCount;

            private BlockMission blockMission;
            private int missionCount;

            public void Set(BlockMission blockMission)
            {
                objRoot.SetActive(true);
                if (string.IsNullOrEmpty(blockMission.block.name))
                {
                    missionIcon.sprite = null;
                    return;
                }
                else
                {
                    Texture2D texture = (Texture2D)BlockManager.Instance.puzzleBlockSetting.dicPuzzleBlock[blockMission.block.name].blockTexture;
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    missionIcon.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                }

                missionCount = blockMission.count;
                textCount.text = $"{missionCount}";
            }
            public void MissionCount()
            {
                missionCount = Math.Max(0, missionCount - 1);
                textCount.text = $"{missionCount}";
            }

            public bool CheckMissionClear()
            {
                return missionCount <= 0;
            }
        }

        public static BlockManager Instance;

        public GridLayoutGroup grid;
        public Transform blockAttach;
        public TextMeshProUGUI textPoint;
        public Button buttonGameEnd;
        public int testStage = 1;

        public BlockItem[,] blockSlotMatrix;
        public GameObject[,] backSlotMatrix;

        private int Point;

        public bool isClick;
        public bool isProcess;
        public BlockItem selectBlock;

        private bool isInit;
        private StageSettingData stageData;

        private List<BlockItem> blockList = new List<BlockItem>();
        private GameObjectPool gameobjectPool;
        public PuzzleBlockSetting puzzleBlockSetting;

        public List<MissionSlot> missionList = new List<MissionSlot>();
        private Dictionary<string, MissionSlot> dicMission = new Dictionary<string, MissionSlot>();

        private const string BACKSLOT_PATH = "Prefab/BackSlot";
        private const string BLOCKSLOT_PATH = "Prefab/BlockItem";
        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            Init();
            buttonGameEnd.onClick.AddListener(BackToIntro);
        }

        private void BackToIntro()
        {
            if (isProcess) return;
            Manager.Instance.OnClickBackIntro();
        }

        public void Init()
        {
            puzzleBlockSetting = Resources.Load<PuzzleBlockSetting>("ScriptableObject/PuzzleBlockSetting");
            puzzleBlockSetting.Set();

            gameobjectPool = new GameObjectPool("BlockObjectPool");
            SettingMap();
            SetMission();
            Point = 0;
            textPoint.text = Point.ToString();
            isInit = true;
        }

        private void SettingMap()
        {
            if(Manager.Instance.curStage == null)
                Manager.Instance.SetStage(testStage);

            stageData = Manager.Instance.curStage;
            int mapSize = stageData.mapSize.x * stageData.mapSize.y;
            grid.constraintCount = stageData.mapSize.y;

            blockSlotMatrix = new BlockItem[stageData.mapSize.x, stageData.mapSize.y];
            backSlotMatrix = new GameObject[stageData.mapSize.x, stageData.mapSize.y];

            for (int i = 0; i < stageData.mapSize.x; i++)
            {
                for (int j = 0; j < stageData.mapSize.y; j++)
                {
                    var obj = gameobjectPool.Get(BACKSLOT_PATH);
                    obj.transform.SetParent(grid.transform);
                    obj.transform.position = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    backSlotMatrix[i, j] = obj;
                }
            }

            SettingMapAsync().Forget();
        }
        private async UniTask SettingMapAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.LastUpdate);
            await UniTask.Yield(PlayerLoopTiming.LastUpdate);

            for (int i = 0; i < stageData.mapSize.x; i++)
            {
                for (int j = 0; j < stageData.mapSize.y; j++)
                {
                    BlockItem blockItem;
                    if (stageData.blockList[i * stageData.mapSize.y + j].blockType == eBlockType.Empty)
                        blockItem = MakeNewBlock(i, j, eBlockType.Normal);
                    else
                        blockItem = MakeNewBlock(i, j, stageData.blockList[i * stageData.mapSize.y + j]);

                    blockSlotMatrix[i, j] = blockItem;
                }
            }
            Fill();
        }

        private void SetMission()
        {
            dicMission.Clear();
            for (int i = 0; i < stageData.missionList.Count; i++)
            {
                missionList[i].Set(stageData.missionList[i]);
                dicMission.TryAdd(stageData.missionList[i].block.name, missionList[i]);
            }
        }
        private void CheckGameEnd()
        {
            foreach (var mission in dicMission)
            {
                if (mission.Value.CheckMissionClear() == false) return;
            }
            BackToIntro();
        }
        public void CheckMission(BlockItem block)
        {
            if (dicMission.TryGetValue(block.blockPrefab.name, out var missionSlot))
                missionSlot.MissionCount();
        }
        public void PointUpdate(int point)
        {
            Point += point;
            textPoint.text = Point.ToString();
        }


        public BlockItem GetBlock(int x, int y)
        {
            if (IsOutOfIndex(x, y)) return null;

            return blockSlotMatrix[x, y];
        }
        public void SetBlock(int x, int y, BlockItem blockItem)
        {
            if (IsOutOfIndex(x, y)) return;

            blockSlotMatrix[x, y] = blockItem;
        }
        public void ReturnBlock(BlockItem block)
        {
            gameobjectPool.Return(block.gameObject);
        }

        public bool IsOutOfIndex(int x, int y)
        {
            if (x < 0 || y < 0 || x >= stageData.mapSize.x || y >= stageData.mapSize.y)
                return true;
            return false;
        }

        public void SwapPuzzle(BlockItem swapBlock)
        {
            isClick = false;
            FindMatchable(false);

            if ((selectBlock.X == swapBlock.X && (selectBlock.Y == swapBlock.Y -1 || selectBlock.Y == swapBlock.Y + 1)) ||
               (selectBlock.Y == swapBlock.Y && (selectBlock.X == swapBlock.X - 1 || selectBlock.X == swapBlock.X + 1)))
            {
                SwapPuzzleAsync(swapBlock).Forget();
            }
        }

        private async UniTask SwapPuzzleAsync(BlockItem swapBlock)
        {
            int tempX = selectBlock.X;
            int tempY = selectBlock.Y;

            isProcess = true;
            selectBlock.SetAndMove(swapBlock.X, swapBlock.Y);
            swapBlock.SetAndMove(tempX, tempY);

            StartCoroutine(CheckPuzzleCo((isNeedFill) => 
            {
                if (isNeedFill)
                    Fill();
                else
                {
                    swapBlock.SetAndMove(selectBlock.X, selectBlock.Y);
                    selectBlock.SetAndMove(tempX, tempY);
                    isProcess = false;
                    selectBlock = null;
                    FindMatchable(true);
                }
            }));

            await UniTask.Yield(PlayerLoopTiming.LastUpdate);
        }

        #region MatchPuzzle, Fill

        Coroutine fillco = null;

        public void Fill()
        {
            FindMatchable(false);
            if (fillco == null)
            {
                fillco = StartCoroutine(FillCor());
            }
        }

        IEnumerator FillCor()
        {

            isProcess = true;
            bool needFill = true;

            while (needFill)
            {
                while (FillRoutine())
                {
                    yield return new WaitForSeconds(0.1f);
                }

                yield return CheckPuzzleCo((isNeedFill) =>
                {
                    needFill = isNeedFill;
                });
            }

            isProcess = false;
            fillco = null;

            FindMatchable(true);
        }


        public bool FillRoutine()
        {
            bool isBlockMove = false;

            for (int j = stageData.mapSize.y - 2; j >= 0; j--)
            {
                for (int i = 0; i < stageData.mapSize.x; i++)
                {
                    if (blockSlotMatrix[i, j] == null) continue;

                    BlockItem curPuzzle = blockSlotMatrix[i, j];
                    BlockItem belowPuzzle = blockSlotMatrix[i, j + 1];

                    if (belowPuzzle == null)
                    {
                        PuzzleChange(curPuzzle, i, j + 1);
                        isBlockMove = true;
                    }
                    else
                    {

                        if (CheckIsObstacle(i - 1, j) || CheckIsObstacle(i + 1, j))
                        {
                            for (int diag = -1; diag <= 1; diag += 2)
                            {
                                if (i + diag < 0 || i + diag >= stageData.mapSize.x) continue;

                                BlockItem newDiagPuzzle = blockSlotMatrix[i + diag, j + 1];

                                if (newDiagPuzzle == null)
                                {
                                    PuzzleChange(curPuzzle, i + diag, j + 1);
                                    isBlockMove = true;

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < stageData.mapSize.x; i++)
            {
                if (blockSlotMatrix[i, 0] == null)
                {
                    BlockItem newPuzzle = MakeNewBlock(i, 0, eBlockType.Normal);
                    newPuzzle.transform.position += new Vector3(0,grid.cellSize.y,0);

                    newPuzzle.SetCoordinate(i, 0);
                    newPuzzle.Move(0.1f);

                    isBlockMove = true;
                }
            }

            return isBlockMove;
        }

        public BlockItem MakeNewBlock(int x, int y, eBlockType blockType)
        {
            BlockItem newBlock = null;
            newBlock = gameobjectPool.Get(BLOCKSLOT_PATH).GetComponent<BlockItem>();
            newBlock.transform.SetParent(blockAttach);
            newBlock.transform.localScale = Vector3.one;
            newBlock.blockPrefab = GetBlockPrefab(blockType);
            newBlock.Init(x,y);

            return newBlock;
        }
        public BlockItem MakeNewBlock(int x, int y, PuzzleBlockSetting.PuzzleBlockPrefab prefab)
        {
            BlockItem newBlock = null;
            newBlock = gameobjectPool.Get(BLOCKSLOT_PATH).GetComponent<BlockItem>();
            newBlock.transform.SetParent(blockAttach);
            newBlock.transform.localScale = Vector3.one;
            newBlock.blockPrefab = prefab;
            newBlock.Init(x, y);

            return newBlock;
        }
        private PuzzleBlockSetting.PuzzleBlockPrefab GetBlockPrefab(eBlockType blockType)
        {
            var list = puzzleBlockSetting.blockList.FindAll(_ => _.blockType == blockType);

            int ranNum = UnityEngine.Random.Range(0, list.Count);
            return list[ranNum];
        }

        void PuzzleChange(BlockItem curBlock, int newX, int newY)
        {
            blockSlotMatrix[curBlock.X, curBlock.Y] = null;
            curBlock.SetCoordinate(newX, newY);
            curBlock.Move(0.1f);
        }

        bool CheckIsObstacle(int x, int y)
        {
            if (IsOutOfIndex(x, y))
                return false;

            if (blockSlotMatrix[x, y] == null || blockSlotMatrix[x, y].blockPrefab.blockType != eBlockType.Block)
                return false;

            return true;
        }

        private int[] dx = new int[] { 0, 1, 0, -1 };
        private int[] dy = new int[] { -1, 0, 1, 0 };


        IEnumerator CheckPuzzleCo(Action<bool> callBack)
        {
            bool isDestroyBlock = false;

            List<BlockItem> destroyPuzzles = new List<BlockItem>();
            Queue<BlockItem> searchQueue = new Queue<BlockItem>();

            for (int j = 0; j < stageData.mapSize.y; j++)
            {
                for (int i = 0; i < stageData.mapSize.x; i++)
                {

                    if (blockSlotMatrix[i, j] == null || blockSlotMatrix[i, j].blockPrefab.blockColor == eBlockColor.None) continue;

                    HashSet<BlockItem> visitPuzzles = new HashSet<BlockItem>();
                    searchQueue.Enqueue(blockSlotMatrix[i, j]);
                    visitPuzzles.Add(blockSlotMatrix[i, j]);

                    while (searchQueue.Count != 0)
                    {
                        BlockItem curPuzzle = searchQueue.Dequeue();

                        List<List<BlockItem>> findPuzzles = new List<List<BlockItem>>();

                        List<BlockItem> up = new List<BlockItem>();
                        List<BlockItem> right = new List<BlockItem>();
                        List<BlockItem> down = new List<BlockItem>();
                        List<BlockItem> left = new List<BlockItem>();

                        findPuzzles.Add(up);
                        findPuzzles.Add(right);
                        findPuzzles.Add(down);
                        findPuzzles.Add(left);

                        for (int k = 0; k < 4; k++)
                        {
                            int newX = curPuzzle.X + dx[k];
                            int newY = curPuzzle.Y + dy[k];
                            
                            do
                            {
                                if (IsOutOfIndex(newX, newY)) break;
                                BlockItem newPuzzle = blockSlotMatrix[newX, newY];

                                if (newPuzzle == null || curPuzzle.blockPrefab.blockColor != newPuzzle.blockPrefab.blockColor) break;

                                if (visitPuzzles.Add(newPuzzle))
                                {
                                    searchQueue.Enqueue(newPuzzle);
                                }

                                if (!destroyPuzzles.Contains(newPuzzle))
                                    findPuzzles[k].Add(newPuzzle);


                                newX += dx[k];
                                newY += dy[k];

                            } while (true);
                        }

                        if ((findPuzzles[0].Count + findPuzzles[1].Count + findPuzzles[2].Count + findPuzzles[3].Count) < 2) continue;

                        if (findPuzzles[0].Count + findPuzzles[2].Count >= 2)
                        {
                            if (!destroyPuzzles.Contains(curPuzzle))
                                destroyPuzzles.Add(curPuzzle);

                            destroyPuzzles.AddRange(findPuzzles[0]);
                            destroyPuzzles.AddRange(findPuzzles[2]);
                        }

                        if (findPuzzles[1].Count + findPuzzles[3].Count >= 2)
                        {
                            if (!destroyPuzzles.Contains(curPuzzle))
                                destroyPuzzles.Add(curPuzzle);

                            destroyPuzzles.AddRange(findPuzzles[1]);
                            destroyPuzzles.AddRange(findPuzzles[3]);
                        }
                    }


                    if (destroyPuzzles.Count >= 1)
                    {

                        isDestroyBlock = true;
                        PopRoutine(destroyPuzzles);

                        destroyPuzzles.Clear();

                    }
                }

                yield return null;
            }

            if (isDestroyBlock)
            {
                yield return new WaitForSeconds(0.1f);
            }

            callBack?.Invoke(isDestroyBlock);

        }
        public void PopRoutine(List<BlockItem> destroyPuzzles)
        {
            foreach (BlockItem puzzle in destroyPuzzles)
            {
                if (puzzle != null)
                {
                    if (puzzle.blockPrefab.blockType == eBlockType.Normal)
                    {
                        for (int obstacleIndex = 0; obstacleIndex < 4; obstacleIndex++)
                        {
                            if (CheckIsObstacle(puzzle.X + dx[obstacleIndex], puzzle.Y + dy[obstacleIndex]))
                            {
                                blockSlotMatrix[puzzle.X + dx[obstacleIndex], puzzle.Y + dy[obstacleIndex]].Pop();
                            }
                        }
                    }

                    puzzle.Pop();
                }
            }

        }
        #endregion

        public Vector2 GetPos(int x, int y)
        {
            return backSlotMatrix[x, y].transform.localPosition;
        }

        #region Find Can Match
        private float hintTime = 2f;
        private Coroutine coHintTimeCheck;

        public void FindMatchable(bool isCheck)
        {
            if (coHintTimeCheck != null)
            {
                StopCoroutine(coHintTimeCheck);
            }

            if(isCheck)
                coHintTimeCheck = StartCoroutine(CoFindMatchable());

            CheckGameEnd();
        }

        private IEnumerator CoFindMatchable()
        {
            float time = 0.0f;

            while (time < hintTime)
            {
                time += Time.deltaTime;

                yield return null;
            }

            try
            {
                if (FindMatch(3))
                    yield break;

                for (int j = 0; j < stageData.mapSize.y; j++)
                {
                    for (int i = 0; i < stageData.mapSize.x; i++)
                    {
                        if (blockSlotMatrix[i, j] != null && blockSlotMatrix[i, j].blockPrefab.blockType != eBlockType.Block)
                        {
                            blockSlotMatrix[i, j].Pop();
                        }
                    }
                }
                Fill();
            }
            catch
            {
                yield break;
            }

            yield return null;

        }

        public bool FindMatch(int MatchCount)
        {

            for (int j = 0; j < stageData.mapSize.y; j++)
            {
                for (int i = 0; i < stageData.mapSize.x; i++)
                {
                    List<BlockItem> findPuzzle = new List<BlockItem>();
                    BlockItem curPuzzle = blockSlotMatrix[i, j];

                    if (curPuzzle == null || curPuzzle.blockPrefab.blockType == eBlockType.Block) continue;

                    if (!IsOutOfIndex(i + MatchCount - 1, j))
                    {
                        for (int k = 1; k < MatchCount; k++)
                        {
                            findPuzzle.Add(GetBlock(i + k, j));
                        }

                        if (findPuzzle.FindAll(x => x.blockPrefab.blockColor == curPuzzle.blockPrefab.blockColor).Count == MatchCount - 2)
                        {
                            BlockItem anotherPuzzle = findPuzzle.Find(x => x.blockPrefab.blockColor != curPuzzle.blockPrefab.blockColor);
                            findPuzzle.Remove(anotherPuzzle);
                            if (anotherPuzzle.blockPrefab.blockType == eBlockType.Block) continue;

                            for (int h = 0; h < 2; h++)
                            {
                                if (FindSameColor(anotherPuzzle, 1, curPuzzle.blockPrefab.blockColor, h == 0 ? eDir.Up : eDir.Down) != null)
                                {
                                    return true;
                                }
                            }

                            if (anotherPuzzle.X == curPuzzle.X + 2)
                            {
                                if (FindSameColor(anotherPuzzle, 1, curPuzzle.blockPrefab.blockColor, eDir.Right) != null)
                                {
                                    return true;
                                }

                            }
                        }
                    }

                    findPuzzle.Clear();

                    if (!IsOutOfIndex(i, j + MatchCount - 1))
                    {
                        for (int k = 1; k < MatchCount; k++)
                        {
                            findPuzzle.Add(GetBlock(i, j + k));
                        }

                        if (findPuzzle.FindAll(x => x.blockPrefab.blockColor == curPuzzle.blockPrefab.blockColor).Count == MatchCount - 2)
                        {
                            BlockItem anotherPuzzle = findPuzzle.Find(x => x.blockPrefab.blockColor != curPuzzle.blockPrefab.blockColor);
                            findPuzzle.Remove(anotherPuzzle);
                            if (anotherPuzzle.blockPrefab.blockType == eBlockType.Block) continue;

                            for (int h = 0; h < 2; h++)
                            {
                                if (FindSameColor(anotherPuzzle, 1, curPuzzle.blockPrefab.blockColor, h == 0 ? eDir.Right : eDir.Left) != null)
                                {
                                    return true;
                                }
                            }

                            if (anotherPuzzle.Y == curPuzzle.Y + 2)
                            {
                                if (FindSameColor(anotherPuzzle, 1, curPuzzle.blockPrefab.blockColor, eDir.Down) != null)
                                {
                                    return true;
                                }

                            }
                        }
                    }
                }
            }

            return false;
        }

        public enum eDir
        {
            Up,
            Down,
            Left,
            Right
        }
        public BlockItem FindSameColor(BlockItem puzzle, int index, eBlockColor color,eDir dir)
        {
            BlockItem findPuzzle = null;

            switch (dir)
            {
                case eDir.Up:
                    findPuzzle = GetBlock(puzzle.X, puzzle.Y - index);
                    break;

                case eDir.Right:
                    findPuzzle = GetBlock(puzzle.X + index, puzzle.Y);
                    break;

                case eDir.Down:
                    findPuzzle = GetBlock(puzzle.X, puzzle.Y + index);
                    break;

                case eDir.Left:
                    findPuzzle = GetBlock(puzzle.X - index, puzzle.Y);
                    break;
            }

            if (findPuzzle == null || findPuzzle.blockPrefab.blockType == eBlockType.Block || findPuzzle.blockPrefab.blockColor != color) return null;

            return findPuzzle;

        }
        #endregion
    }
}
