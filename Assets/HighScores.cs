using System;
using System.Collections.Generic;

[Serializable]
public class HighScores
{
    //highscores is case sensitive and must match the string "highscores" in the JSON.
    public List<HighScore> highscores;
}
