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

        //퍼즐 좌표 세팅
        public void SetCoordinate(int newX, int newY)
        {
            this.x = newX;
            this.y = newY;
        }
        public void UpdateFrame(float deltaTime)
        {

        }
        //퍼즐위치 세팅
        public void SetPos(Vector2 pos)
        {
            myRect.anchoredPosition = pos;
        }

        //x,y좌표로 이동 -> 콜백 함수
        public void Move(int x, int y, float fillTime, UnityAction callback)
        {
            moveable.Move(x, y, fillTime, callback);
        }


        //퍼즐 현재x,y 위치로 움직이기
        public void Move(float fillTime)
        {
            moveable.Move(fillTime);
        }

        //퍼즐 x,y위치로 세팅하고 움직이기
        public void SetAndMove(int newX, int newY)
        {
            SetCoordinate(newX, newY);
            Move(0.1f);
        }

        //퍼즐 초기화
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



        //퍼즐 터트릴때
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

        //애니메이션이 끝난 후 처리
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
