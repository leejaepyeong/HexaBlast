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

        public eBlockType type;
        public eBlockColor color;
        public RectTransform myRect;
        public Image myImage;

        private MoveablePuzzle moveable;
        private Coroutine coFlicker = null;

        //���� ��ǥ ����
        public void SetCoordinate(int newX, int newY)
        {
            this.x = newX;
            this.y = newY;
        }
        public void UpdateFrame(float deltaTime)
        {

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
        public void Init(int x, int y, eBlockColor newColor = eBlockColor.None)
        {
            if (this.color != eBlockColor.None)
            {
                if (newColor == eBlockColor.None)
                {
                }
                else
                {
                }
            }

            SetCoordinate(x, y);
            //SetPos(manager.Maker.GetPos(x, y));
        }



        //���� ��Ʈ����
        public virtual void Pop(UnityAction callBack = null)
        {
            //if (manager.GetPuzzle(X, Y) == this)
            //{
            //    manager.SetPuzzle(X, Y, null);
            //}

            Destroy(this.gameObject);

            BlockManager.Instance.Point += 100;
            callBack?.Invoke();
        }

        //�ִϸ��̼��� ���� �� ó��
        public void EndDestroyAnimation()
        {
            if (this.type == eBlockType.Normal)
            {
                //manager.Maker.PuzzlePool.ReturnToPool(this);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}
