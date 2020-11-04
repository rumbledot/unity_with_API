using System;
using System.Collections.Generic;
using System.Diagnostics;

[Serializable]
public class HighScoresDictionary
{
    public string id;
    public HighScore highscore;

    public HighScoresDictionary() { }
    public HighScoresDictionary(string i, HighScore hs)
    {
        id = i;
        highscore = hs;
    }
}
