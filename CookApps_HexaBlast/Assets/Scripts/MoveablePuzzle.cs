using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ProjectPuzzle
{
    public class MoveablePuzzle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        [SerializeField] private BlockItem blockItem;
        private Coroutine coMove;

        private int X => blockItem.X;
        private int Y => blockItem.Y;


        public void Move(float fillTime)
        {
            //manager.SetPuzzle(X, Y, myPuzzle);

            if (coMove != null)
            {
                StopCoroutine(coMove);
            }

            coMove = StartCoroutine(MoveCoroutine(X, Y, fillTime));
        }
        public void Move(int x, int y, float fillTime, UnityAction callback)
        {
            if (coMove != null)
            {
                StopCoroutine(coMove);
            }

            coMove = StartCoroutine(MoveCoroutine(x, y, fillTime, callback));
        }


        IEnumerator MoveCoroutine(int x, int y, float fillTime, UnityAction callback = null)
        {
            float curtime = 0.0f;
            Vector2 startPos = blockItem.myRect.anchoredPosition;
            Vector2 targetPos = new Vector2();//manager.GetPos(x, y);

            while (curtime < fillTime)
            {
                curtime += Time.deltaTime;
                blockItem.myRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curtime / fillTime);

                yield return null;
            }

            blockItem.SetPos(targetPos);

            callback?.Invoke();
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (BlockManager.Instance.isProcess) return;

            BlockManager.Instance.selectBlock = blockItem;
            BlockManager.Instance.isClick = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            BlockManager.Instance.isClick = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            if (BlockManager.Instance.isProcess == true || BlockManager.Instance.isClick == false || 
                BlockManager.Instance.selectBlock == blockItem || BlockManager.Instance.selectBlock == null) return;

            //manager.SwapPuzzle(this.myPuzzle);

        }
    }
}