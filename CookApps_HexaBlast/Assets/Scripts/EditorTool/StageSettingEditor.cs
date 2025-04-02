using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using System.Linq;

namespace ProjectPuzzle
{
    [System.Serializable]
    public class StageSettingData
    {
        public Vector2Int mapSize;
        public List<PuzzleBlockSetting.PuzzleBlockPrefab> blockList = new List<PuzzleBlockSetting.PuzzleBlockPrefab>();
        public int floor;
        public string name;

        public StageSettingData(string name, int floor)
        {
            this.name = name;
            this.floor = floor;
        }
    }
    public class StageSettingEditor : EditorWindow
    {
        bool isInit;
        private PuzzleBlockSetting puzzleBlockSetting;
        private PuzzleBlockSetting.PuzzleBlockPrefab brushBlock;
        private string path = "./Assets/Resources/DataJsons/StageSettingData.json";
        private int squareSize = 40;
        private List<StageSettingData> dataList = new List<StageSettingData>();
        private StageSettingData data;

        [MenuItem("Tools/StageSettingEditor")]
        private static void Init()
        {
            StageSettingEditor editor = (StageSettingEditor)EditorWindow.GetWindow(typeof(StageSettingEditor));
            editor.Show();
        }

        private void Setting()
        {
            puzzleBlockSetting = Resources.Load<PuzzleBlockSetting>("ScriptableObject/PuzzleBlockSetting");
            puzzleBlockSetting.Set();
            LoadJson();
            data = dataList[0];
            isInit = true;
        }

        private void OnGUI()
        {
            if (!isInit)
                Setting();

            GUILayout.BeginVertical();
            GUITopMenu();
            GUIStage();
            GUIBlockPalette();
            GUIBlockEditor();
            GUIBottom();
            GUILayout.EndVertical();
        }

        private void GUITopMenu()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Save"), GUILayout.Width(80), GUILayout.Height(30)))
            {
                SaveJson();
            }
            if (GUILayout.Button(new GUIContent("AddStage"), GUILayout.Width(80), GUILayout.Height(30)))
            {
                AddStage();
            }
            if (GUILayout.Button(new GUIContent("Refesh"), GUILayout.Width(80), GUILayout.Height(30)))
            {
                Refresh();
            }
            if (GUILayout.Button(new GUIContent("Remove"), GUILayout.Width(80), GUILayout.Height(30)))
            {
                Remove();
            }
            GUILayout.EndHorizontal();
        }
        public Vector2 scrollPos1;
        public int mapSizeX;
        public int mapSizeY;
        public int floor;
        public string mapName;

        private void GUIStage()
        {
            GUILayout.BeginHorizontal();
            {
                scrollPos1 = GUILayout.BeginScrollView(scrollPos1, GUILayout.Width(200), GUILayout.Height(100));
                {
                    GUILayout.BeginVertical();
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        if (GUILayout.Button($"{dataList[i].name} (F {dataList[i].floor})"))
                        {
                            int index = i;
                            data = dataList[index];
                            Refresh();
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Cur Floor", GUILayout.Width(80));
                floor = EditorGUILayout.IntField(floor, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(80));
                mapName = EditorGUILayout.TextField(mapName, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Width", GUILayout.Width(80));
                mapSizeX = EditorGUILayout.IntField(mapSizeX, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Height", GUILayout.Width(80));
                mapSizeY = EditorGUILayout.IntField(mapSizeY, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void Refresh()
        {
            floor = data.floor;
            mapName = data.name;
            mapSizeX = data.mapSize.x;
            mapSizeY = data.mapSize.y;

            dataList.Sort(delegate(StageSettingData a, StageSettingData b) 
            {
                return a.floor > b.floor ? 1 : -1;
            });
        }
        private void Remove()
        {
            dataList.Remove(data);

            if (dataList.Count == 0)
            {
                StageSettingData data = new StageSettingData("1", 1);
                dataList.Add(data);
            }
            data = dataList[0];
            Refresh();
        }

        private void GUIBlockPalette()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            int i;
            for (i = 0; i < puzzleBlockSetting.blockList.Count; i++)
            {
                var lvBlock = puzzleBlockSetting.blockList[i];
                if (brushBlock != null && brushBlock == lvBlock)
                {
                    GUI.backgroundColor = Color.gray;
                }

                if (GUILayout.Button(new GUIContent("", lvBlock.name), GUILayout.Width(squareSize), GUILayout.Height(squareSize)))
                {
                    brushBlock = lvBlock;
                }

                var lastRect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(lastRect, lvBlock.blockTexture);
                GUI.backgroundColor = Color.white;
            }

            if (brushBlock != null && brushBlock.blockType == eBlockType.Empty)
                GUI.backgroundColor = Color.gray;

            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
        }

        private void GUIBlockEditor()
        {
            GUILayout.Space(30);
            EditorGUI.BeginChangeCheck();
            var sizeX = data.mapSize.x;
            var sizeY = data.mapSize.y;

            for (int y = 0; y < sizeY; y++)
            {
                GUILayout.BeginHorizontal();
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        var size = sizeX * sizeY;
                        if (data.blockList.Count != size)
                            InitBlocks(size);

                        if (GUILayout.Button("", GUILayout.Width(squareSize), GUILayout.Height(squareSize)))
                        {
                            if (brushBlock != null)
                            {
                                data.blockList[y * sizeX + x] = brushBlock;
                            }
                        }

                        if (data.blockList[y * sizeX + x].blockTexture != null)
                        {
                            var lastRect = GUILayoutUtility.GetLastRect();
                            GUI.DrawTexture(lastRect, puzzleBlockSetting.dicPuzzleBlock[data.blockList[y * sizeX + x].name].blockTexture);
                        }
                    }

                    GUI.backgroundColor = Color.white;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void GUIBottom()
        {
            if (GUILayout.Button("Clear", GUILayout.Width(squareSize * 2)))
            {
                InitBlocks(data.mapSize.x * data.mapSize.y);
            }

            EditorGUILayout.Separator();
        }

        private void InitBlocks(int size)
        {
            data.blockList.Clear();
            for (int i = 0; i < size; i++)
            {
                data.blockList.Add(new PuzzleBlockSetting.PuzzleBlockPrefab());
            }
        }

        private void AddStage()
        {
            StageSettingData data = new StageSettingData((dataList.Count + 1).ToString(), dataList.Count + 1);
            dataList.Add(data);
        }

        #region Json
        [System.Serializable]
        public class SaveStageData
        {
            public List<StageSettingData> dataList = new();
        }
        private void SaveJson()
        {
            data.name = mapName;
            data.mapSize.x = mapSizeX;
            data.mapSize.y = mapSizeY;

            SaveStageData saveData = new();

            dataList.Sort(delegate (StageSettingData a, StageSettingData b)
            {
                return a.floor > b.floor ? 1 : -1;
            });

            for (int i = 0; i < dataList.Count; i++)
            {
                saveData.dataList = dataList;
            }

            string jsonText = JsonUtility.ToJson(saveData, true);
            string encryptString = jsonText;
            File.WriteAllText(path, encryptString);
        }
        private void LoadJson()
        {
            SaveStageData saveData = new();
            if (File.Exists(path))
            {
                string decryptString = File.ReadAllText(path);
                string jsonText = decryptString;
                saveData = JsonUtility.FromJson<SaveStageData>(jsonText);

                for (int i = 0; i < saveData.dataList.Count; i++)
                {
                    dataList.Add(saveData.dataList[i]);
                }
            }
            else
            {
                StageSettingData data = new StageSettingData("1", 1);
                dataList.Add(data);
                SaveJson();
            }
        }
        #endregion
    }
}
