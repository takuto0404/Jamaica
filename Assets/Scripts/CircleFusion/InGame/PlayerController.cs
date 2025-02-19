using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CircleFusion.Share;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CircleFusion.InGame
{
    public class PlayerController : SingletonMonoBehaviour<PlayerController>
    {
        [SerializeField] private UGUILineRenderer lineRenderer;

        private static Vector2 AlignPosition(Vector2 position)
        {
            return new Vector2(position.x + Screen.width / 2f, position.y + Screen.height / 2f);
        }

        private static NumberBox GetHoveredNumberBox()
        {
            var pointData = new PointerEventData(EventSystem.current);
            var rayResult = new List<RaycastResult>();
            pointData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointData, rayResult);
            var box = rayResult.Select(x => x.gameObject.GetComponent<NumberBox>()).Where(x => x != null)
                .Where(x => !x.isAnswerBox);
            var numberBoxes = box as NumberBox[] ?? box.ToArray();
            return !numberBoxes.Any() ? null : numberBoxes.First();
        }
        
        private Vector2 AlignPositionWithScreenPoint(Vector2 position)
        {
            var screenSizeHalf = new Vector2(Screen.width / 2f,Screen.height / 2f);
            var position1 = position - screenSizeHalf;
            var a = 960f / Screen.width;
            var position2 = new Vector2(position1.x * a, position1.y * a);
            var position3 = position2 + screenSizeHalf;
            return position3;
        }
        
        public async UniTask ProcessPlayerActionAsync(CancellationToken gameCt)
        {
            gameCt.ThrowIfCancellationRequested();
            try
            {
                while (true)
                {
                    NumberBox firstNumberBox = null;
                    NumberBox secondNumberBox = null;
                    var selectedOperator = OperatorSymbol.None;
                    
                    while (selectedOperator == OperatorSymbol.None)
                    {
                        secondNumberBox = null;
                        while (secondNumberBox == null)
                        {
                            lineRenderer.ClearLine();
                            firstNumberBox = null;
                            while (firstNumberBox == null)
                            {
                                await MouseInputProvider.Instance.OnHoldDownAsync(gameCt);
                                firstNumberBox = GetHoveredNumberBox();
                            }

                            var boxInitialPosition = firstNumberBox.initialPosition;
                            using (UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ =>
                                       lineRenderer.DrawLine(AlignPosition(boxInitialPosition), AlignPositionWithScreenPoint(MouseInputProvider.Instance.mousePosition))))
                            {
                                await MouseInputProvider.Instance.OnHoldUpAsync(gameCt);
                                secondNumberBox = GetHoveredNumberBox();
                            }
                        }
                        
                        if(firstNumberBox == secondNumberBox)continue;

                        lineRenderer.DrawLine(AlignPosition(firstNumberBox.initialPosition),
                            AlignPosition(secondNumberBox.initialPosition));
                        var isCalculable =
                            GameUIPresenter.Instance.CheckCalculations(firstNumberBox, secondNumberBox);
                        selectedOperator = await GameUIPresenter.Instance.SelectOperatorAsync(isCalculable, gameCt);
                    }
                    lineRenderer.ClearLine();
                    GameUIPresenter.Instance.Calculation(firstNumberBox, secondNumberBox, selectedOperator);
                    PuzzleHistory.SetHist(DiceCalculator.GetAllDices(), 
                        GameUIPresenter.Instance.CreateFormulaText(DiceCalculator.FetchCurrentFormula()));
                    GameUIPresenter.Instance.UpdateFormulaText(PuzzleHistory.LastHist().FormulaText);
                }
            }
            catch (OperationCanceledException e)
            {
                return;
            }
        }
    }
}