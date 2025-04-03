using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

namespace ProjectPuzzle
{
    public class BlockManager : MonoBehaviour
    {
        public static BlockManager Instance;
        public GridLayoutGroup grid;

        public BlockItem[,] blockSlotMatrix;
        public GameObject[,] backSlotMatrix;

        public int Point;
        public int testStage = 1;

        public bool isClick;
        public bool isProcess;
        public BlockItem selectBlock;

        private bool isInit;
        private StageSettingData stageData;

        private List<BlockItem> blockList = new List<BlockItem>();
        private GameObjectPool gameobjectPool;

        private const string BACKSLOT_PATH = "Prefab/BackSlot";
        private const string BLOCKSLOT_PATH = "Prefab/BlockItem";
        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            Init();
        }

        private void Update()
        {
            if (isInit == false) return;

            for (int i = 0; i < blockList.Count; i++)
            {
                blockList[i].UpdateFrame(Time.deltaTime);
            }
        }

        public void Init()
        {
            gameobjectPool = new GameObjectPool("BlockObjectPool");
            SettingMap();
            Point = 0;
            isInit = true;
        }

        private void SettingMap()
        {
            if(Manager.Instance.curStage == null)
                Manager.Instance.SetStage(testStage);

            stageData = Manager.Instance.curStage;
            int mapSize = stageData.mapSize.x * stageData.mapSize.y;
            grid.constraintCount = stageData.mapSize.x;

            blockSlotMatrix = new BlockItem[stageData.mapSize.y, stageData.mapSize.x];
            backSlotMatrix = new GameObject[stageData.mapSize.y, stageData.mapSize.x];

            for (int i = 0; i < stageData.mapSize.y; i++)
            {
                for (int j = 0; j < stageData.mapSize.x; j++)
                {
                    var obj = gameobjectPool.Get(BACKSLOT_PATH);
                    obj.transform.SetParent(grid.transform);
                    obj.transform.position = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    backSlotMatrix[i, j] = obj;
                    BlockItem blockItem = new BlockItem();
                    blockItem.blockPrefab = stageData.blockList[i * stageData.mapSize.x + j];
                    blockSlotMatrix[i, j] = blockItem;
                }
            }

            InitStartBlock();
        }

        private void InitStartBlock()
        {
            int mapSize = stageData.mapSize.x * stageData.mapSize.y;


        }

        public void SetBlock(int x, int y, BlockItem blockItem)
        {

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

            if((selectBlock.X == swapBlock.X && (selectBlock.Y == swapBlock.Y -1 || selectBlock.Y == swapBlock.Y + 1)) ||
               (selectBlock.Y == swapBlock.X && (selectBlock.X == swapBlock.X - 1 || selectBlock.X == swapBlock.X + 1)))
            {
                SwapPuzzleAsync(swapBlock).Forget();
            }
        }

        private async UniTask SwapPuzzleAsync(BlockItem swapBlock)
        {
            isProcess = true;
            selectBlock.SetAndMove(swapBlock.X, swapBlock.Y);
            swapBlock.SetAndMove(selectBlock.X, selectBlock.Y);
        }

        #region 퍼즐 채우기, 터트리기 탐색

        Coroutine fillco = null;

        //채우고 생성하는 함수
        public void Fill()
        {
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
                    //내려오는 시간 0.1초 기다려줘야함
                    yield return new WaitForSeconds(0.1f);
                }

                yield return CheckPuzzleCo((isNeedFill) =>
                {
                    needFill = isNeedFill;
                });
            }

            isProcess = false;
            fillco = null;
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

                    if (belowPuzzle == null) //무언가 없다면 그냥 내림
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


            //최상단 퍼즐 생성해줌
            for (int i = 0; i < stageData.mapSize.x; i++)
            {
                if (blockSlotMatrix[i, 0] == null)
                {
                    BlockItem newPuzzle = MakeNewBlock(i, -1, eBlockType.Normal);

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

            newBlock.Init(x,y);
            return newBlock;
        }

        //퍼즐 이동 후 배열,x,y값 바꾸기
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


        //시계방향으로 검사 배열
        private int[] dx = new int[] { 0, 1, 0, -1 };
        private int[] dy = new int[] { -1, 0, 1, 0 };


        IEnumerator CheckPuzzleCo(Action<bool> callBack)
        {
            bool isDestroyBlock = false;

            List<BlockItem> destroyPuzzles = new List<BlockItem>();
            Queue<BlockItem> searchQueue = new Queue<BlockItem>();

            for (int j = 0; j < stageData.mapSize.x; j++)
            {
                for (int i = 0; i < stageData.mapSize.y; i++)
                {

                    if (blockSlotMatrix[i, j] == null || blockSlotMatrix[i, j].blockPrefab.blockColor == eBlockColor.None) continue;

                    HashSet<BlockItem> visitPuzzles = new HashSet<BlockItem>();
                    searchQueue.Enqueue(blockSlotMatrix[i, j]);
                    visitPuzzles.Add(blockSlotMatrix[i, j]);

                    eBlockType rewardType = eBlockType.Empty;

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

                        //현재 퍼즐에서 상하좌우 탐색
                        for (int k = 0; k < 4; k++)
                        {
                            int newX = curPuzzle.X + dx[k];
                            int newY = curPuzzle.Y + dy[k];
                            
                            do
                            {
                                if (IsOutOfIndex(newX, newY)) break;
                                BlockItem newPuzzle = blockSlotMatrix[newX, newY];

                                if (newPuzzle == null || curPuzzle.blockPrefab.blockColor != newPuzzle.blockPrefab.blockColor) break;

                                //방문하지 않은 퍼즐이라면 큐에 넣어줌
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

                        //여기서부터 아이템 생성조건에 부합한지 체크

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
                    //bfs끝


                    if (destroyPuzzles.Count >= 1)
                    {

                        isDestroyBlock = true;
                        PopRoutine(destroyPuzzles);

                        destroyPuzzles.Clear();

                    }
                }

                yield return null;
            }

            //터치는 시간만큼 기다려줘야함.

            if (isDestroyBlock)
            {
                //내려오는 소리
                //soundManager.PlayEffect(6);
                yield return new WaitForSeconds(0.1f);
            }

            callBack?.Invoke(isDestroyBlock);

        }

        // 발견한 퍼즐들 터트리는 루틴
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
            return backSlotMatrix[x, y].transform.position;
        }
    }
}
