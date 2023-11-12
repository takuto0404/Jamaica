using UnityEngine;

namespace Jamaica
{
    public class GameInitialData : SingletonMonoBehaviour<GameInitialData>
    {
        [SerializeField] public int shuffleLength;
        [SerializeField] public int diceMaxValue;
        [SerializeField] public int numberOfDice;
    }
}