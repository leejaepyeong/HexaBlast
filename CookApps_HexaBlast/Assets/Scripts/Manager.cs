using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;

namespace ProjectPuzzle
{
    public class Manager : Singleton<Manager>
    {
        private const string path = "./Assets/Resources/DataJsons/StageSettingData.json";
        private const string PLAYSCENE = "PuzzleScene";
        private List<StageSettingData> dataList = new List<StageSettingData>();
        public List<StageSettingData> DataList => dataList;

        public StageSettingData curStage;

        protected virtual void Awake()
        {
            Init();
        }
        private void Init()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LoadJson()
        {
            StageSettingEditor.SaveStageData saveData = new();
            if (File.Exists(path))
            {
                string decryptString = File.ReadAllText(path);
                string jsonText = decryptString;
                saveData = JsonUtility.FromJson<StageSettingEditor.SaveStageData>(jsonText);

                for (int i = 0; i < saveData.dataList.Count; i++)
                {
                    dataList.Add(saveData.dataList[i]);
                }
            }
            else
            {
                Debug.LogError("There is No Stage Data");
                Application.Quit();
            }
        }

        public void OnClickPlay(StageSettingData data)
        {
            curStage = data;
            ScneMoveAsync().Forget();
        }
        public void SetStage(int floor)
        {
            if (dataList.Count == 0)
                LoadJson();

            var stage = dataList.Find(_ => _.floor == floor);
            curStage = stage;
        }

        private async UniTask ScneMoveAsync()
        {
            await SceneManager.LoadSceneAsync(PLAYSCENE, LoadSceneMode.Single);
        }
    }
}
