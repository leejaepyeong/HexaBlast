using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectPuzzle
{
    [CreateAssetMenu(fileName = "PuzzleBlockSetting", menuName = "ScriptableObject/PuzzleBlockSetting")]
    public class PuzzleBlockSetting : ScriptableObject
    {
        [System.Serializable]
        public class PuzzleBlockPrefab : ISearchFilterable
        {
            public string name;
            public Texture blockTexture;
            public eBlockType blockType;
            public eBlockColor blockColor;
            [ShowIf("@blockType == eBlockType.Block")]
            public int blockHP;

            public bool IsMatch(string searchString)
            {
                string searchTemp = searchString.ToLower();
                string nameTemp = name.ToLower();

                return nameTemp.Contains(searchTemp);
            }
        }

        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        public List<PuzzleBlockPrefab> blockList;

        public Dictionary<string, PuzzleBlockPrefab> dicPuzzleBlock = new Dictionary<string, PuzzleBlockPrefab>();

        public void Set()
        {
            dicPuzzleBlock.Clear();

            for (int i = 0; i < blockList.Count; i++)
            {
                dicPuzzleBlock.TryAdd(blockList[i].name, blockList[i]);
            }
        }
    }
}
