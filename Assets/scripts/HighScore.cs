using System;
using System.Collections.Generic;

[Serializable]
public class HighScore
{
    public string nickname;
    public int score;
    public string game;

    public HighScore() { }
    public HighScore(string n, int s, string g) 
    {
        nickname = n;
        score = s;
        game = g;
    }

    public Dictionary<string, Object> ToDictionary()
    {
        Dictionary<string, Object> result = new Dictionary<string, Object>();
        result["nickname"] = nickname;
        result["score"] = score;
        result["game"] = game;

        return result;
    }
}
