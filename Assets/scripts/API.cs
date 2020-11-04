using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class API : MonoBehaviour
{
    // further REST API authentication and security best practices
    private string secretKey = "APIKey";
    // REST API URL
    [SerializeField]
    private string myURL = "unity.test";
    private string addScoreURL = "/scores/add";
    private string getScoreURL = "/scores/get";

    //Text to display the result on
    public Text statusText;
    public Text dataText;
    public InputField nn;
    public InputField score;
    public InputField game;
    public Button submitButton;
    public Button getButton;

    void Start()
    {
        // start by displaying data from database
        StartCoroutine(GetScores());
        submitButton.onClick.AddListener(SubmitNewScore);
        getButton.onClick.AddListener(GetScoresList);
    }

    public void SubmitNewScore()
    {
        StartCoroutine(PostScores());
    }
    public void GetScoresList()
    {
        StartCoroutine(GetScores());
    }

    // this function will send out an object converted to JSON data to the API
    public IEnumerator PostScores()
    {
        statusText.text = "Sending new score";
        // get name and score from input field
        string name = nn.text;
        int s = Int32.Parse(score.text);
        string g = game.text;
        // create the object
        HighScore hs = new HighScore();
        hs.nickname = name;
        hs.score = s;
        hs.game = g;
        // convert it to JSON
        string hsJSON = JsonUtility.ToJson(hs);

        // carry out the request to API using HTTP_POST method
        UnityWebRequest www = UnityWebRequest.Put(myURL + addScoreURL, hsJSON);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            statusText.text = "Send not OK!";
            GetScores();
        }
        else
        {
            statusText.text = "Send OK!";
        }
    }

    // retrieving data from the API using HTTP_GET method
    IEnumerator GetScores()
    {
        statusText.text = "Loading Scores";
        // clear out the output
        dataText.text = "";
        string output = "HIGHSCORES \n";

        // carry out the request to the API
        var request = UnityWebRequest.Get(myURL + getScoreURL);

        // Wait for the response and then get our data
        yield return request.SendWebRequest();
        var data = request.downloadHandler.text;

        if (request.error != null)
        {
            statusText.text = "ERROR";
        }
        else
        {
            // format the data for output
            output += "---------- \n";

            // deserialised JSON data to match object class properties
            HighScores highScoreJson = JsonUtility.FromJson<HighScores>(data);

            foreach (HighScore hs in highScoreJson.highscores)
            {
                output += "> " + hs.nickname + " - " + hs.score + " - " + hs.game + "\n";
            }
            output += "---------- \n";

            dataText.text = output;
            statusText.text = "Complete";
        }
    }

    // for future development
    // hashing password or other sensitive data
    public string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}
