using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ProjectPuzzle
{
    public class BlockItem : MonoBehaviour
    {
        private int x;
        private int y;

        public int X => x;
        public int Y => y;

        public RectTransform myRect;
        public Image myImage;

        public MoveablePuzzle moveable;
        private Coroutine coFlicker = null;

        public PuzzleBlockSetting.PuzzleBlockPrefab blockPrefab;
        public int blockHP;

        //���� ��ǥ ����
        public void SetCoordinate(int newX, int newY)
        {
            this.x = newX;
            this.y = newY;
        }

        //������ġ ����
        public void SetPos(Vector2 pos)
        {
            myRect.anchoredPosition = pos;
        }

        //x,y��ǥ�� �̵� -> �ݹ� �Լ�
        public void Move(int x, int y, float fillTime, UnityAction callback)
        {
            moveable.Move(x, y, fillTime, callback);
        }


        //���� ����x,y ��ġ�� �����̱�
        public void Move(float fillTime)
        {
            moveable.Move(fillTime);
        }

        //���� x,y��ġ�� �����ϰ� �����̱�
        public void SetAndMove(int newX, int newY)
        {
            SetCoordinate(newX, newY);
            Move(0.1f);
        }

        //���� �ʱ�ȭ
        public void Init(int x, int y)
        {
            SetCoordinate(x, y);
            SetPos(BlockManager.Instance.GetPos(x, y));

            if(string.IsNullOrEmpty(blockPrefab.name))
            {
                myImage.sprite = null;
                return;
            }
            else
            {
                Texture2D texture = (Texture2D)BlockManager.Instance.puzzleBlockSetting.dicPuzzleBlock[blockPrefab.name].blockTexture;
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                myImage.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            }
            blockHP = blockPrefab.blockHP;
        }

        public virtual void Pop(UnityAction callBack = null)
        {
            blockHP -= 1;
            if (blockHP > 0) return;

            if (BlockManager.Instance.GetBlock(X, Y) == this)
            {
                BlockManager.Instance.SetBlock(X, Y, null);
            }

            BlockManager.Instance.ReturnBlock(this);

            BlockManager.Instance.PointUpdate(100);
            BlockManager.Instance.CheckMission(this);
            callBack?.Invoke();
        }

    }
}
