using UniRx;

/// <summary>
/// いろいろなところからアクセスしたいゲームのデータを入れるstaticクラス
/// </summary>
public static class GameData
{
    /// <summary>
    /// ゲーム中のタイマーの数値
    /// </summary>
    public static readonly ReactiveProperty<float> Timer = new();

    /// <summary>
    /// ゲーム中のスコア
    /// </summary>
    public static int Score = 0;

    /// <summary>
    /// 連続正解数
    /// </summary>
    public static int Combo = 0;

    public static void Win()
    {
        Combo++;
        Score += Combo;
    }

    public static void Lose()
    {
        Combo = 0;
    }
}