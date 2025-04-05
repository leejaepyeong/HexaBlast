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
        private const string INTROSCENE = "IntroScene";
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
            dataList.Clear();

            var data = Resources.Load("DataJsons/StageSettingData");
            if (data == null)
                Debug.LogError("There is No stage ");
            string jsonText = data.ToString();

            var block = Resources.Load("Prefab/BackSlot");
            if(block == null)
                Debug.LogError("There is No block Data");

            SaveStageData saveData = new();
            if (string.IsNullOrEmpty(jsonText) == false)
            {
                //string decryptString = File.ReadAllText(path);
                //string jsonTextT = decryptString;
                saveData = JsonUtility.FromJson<SaveStageData>(jsonText);

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
            ScneMoveAsync(PLAYSCENE).Forget();
        }
        public void SetStage(int floor)
        {
            if (dataList.Count == 0)
                LoadJson();

            var stage = dataList.Find(_ => _.floor == floor);
            curStage = stage;
        }

        private async UniTask ScneMoveAsync(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
        public void OnClickBackIntro()
        {
            curStage = null;
            ScneMoveAsync(INTROSCENE).Forget();
        }
    }
}
