using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using UnityEditor;
using SimpleJSON;

public class DataBridge : MonoBehaviour
{
    public InputField nickname, score, game;
    public Text outputText, userText;
    private bool isDB = false;
    private bool isCheckedForDB = false;
    private bool isLoaded = false;
    private Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseUser user;
    private bool signedIn;
    private DataSnapshot snapshot;
    private Firebase.Database.DatabaseReference table_ref;
    private Dictionary<string, HighScore> userHS;

    private string DB_URL = "https://unitydb-5a31c.firebaseio.com/";
    Firebase.FirebaseApp app;
    DatabaseReference db_ref;

    private void Start()
    {
        InitializeFirebase();
        DBConnect();
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void DBConnect()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                app = Firebase.FirebaseApp.DefaultInstance;
                db_ref = FirebaseDatabase.DefaultInstance.RootReference;
                isDB = true;
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                userText.text = "Log in as " + user.Email;
            }
        }
    }
    private void Update()
    {
        if (isDB && !isCheckedForDB)
        {
            outputText.text = "DB connected";
            isCheckedForDB = true;
            LoadData();
        }
        if (isLoaded && snapshot.Exists && snapshot.ChildrenCount > 0)
        {
            isLoaded = false;
            string output = "Highscores \n";
            output += "------------------ \n";
            foreach (var child in snapshot.Children)
            {
                try
                {
                    Debug.Log("The Key : " + child.Key);
                    output += "------------------ \n";
                    output += "nickname : " + child.Child("nickname").GetValue(true) + "\n";
                    output += "score : " + child.Child("score").GetValue(true) + "\n";
                    output += "game : " + child.Child("game").GetValue(true) + "\n";
                    output += "------------------ \n";
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            output += "------------------ \n";
            outputText.text = output;
        }
    }

    public void SaveData()
    {
        if (isDB)
        {
            string nn = nickname.text;
            int s = Int32.Parse(score.text);
            string g = game.text;

            if (!(nn.Equals("") ||
                s == 0 ||
                g.Equals("")))
            {
                string uid = db_ref.Child("Highscore").Push().Key;
                HighScore hs = new HighScore(nn, s, g);
                Dictionary<string, System.Object> hss = hs.ToDictionary();
                Dictionary<string, System.Object> childUpdate = new Dictionary<string, System.Object>();
                childUpdate["/highscores/" + uid] = hss;

                string hsJSON = JsonUtility.ToJson(hs);
                if (signedIn)
                {
                    //db_ref.Child("Highscore").Child(user.UserId).Child("highscores").Child(uid).SetRawJsonValueAsync(hsJSON);
                    db_ref.Child("Highscore").Child(user.UserId).UpdateChildrenAsync(childUpdate);
                }
                else
                {
                    db_ref.Child("Highscore").Child("guest").Child("highscores").Child(uid).SetRawJsonValueAsync(hsJSON);
                }

                nickname.text = "";
                score.text = "";
                game.text = "";

                outputText.text = "Saved";
            }
        }
    }
    public void LoadData()
    {
        string DBPath = "";
        if (signedIn)
        {
            DBPath = "Highscore/" + user.UserId;
        }
        else
        {
            DBPath = "Highscore/guest/highscores";
        }
        Debug.Log("LoadData called " + DBPath);
        table_ref = db_ref.Child("Highscore").Child(user.UserId).Child("highscores");
        db_ref.Child("Highscore").Child(user.UserId).Child("highscores").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return;
            }
            else if (task.IsCompleted)
            {
                snapshot = task.Result;
                isLoaded = true;
                Debug.Log("Loaded : " + isLoaded);
            }
        });

        //Event listeners

        //table_ref.ChildAdded += HandleChildAdded;
        //table_ref.ChildChanged += HandleChildChanged;
        //table_ref.ChildRemoved += HandleChildRemoved;
        //table_ref.ChildMoved += HandleChildMoved;

        table_ref.ValueChanged += HandleValueChanged;
    }
    void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        UpdateOutputValues(args);
    }

    void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        UpdateOutput(args);
    }

    void HandleChildChanged(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleChilMoved called" + args.DatabaseError.Message);
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        UpdateOutput(args);
    }

    void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleChilMoved called" + args.DatabaseError.Message);
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        UpdateOutput(args);
    }

    void HandleChildMoved(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("HandleChilMoved called" + args.DatabaseError.Message);
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        UpdateOutput(args);
    }

    private void UpdateOutput(ChildChangedEventArgs args)
    {
        string output = "Highscores \n";
        output += "------------------ \n";
        foreach (var child in args.Snapshot.Children)
        {
            try
            {
                Debug.Log("The Key : " + child.Key);
                //output += "nickname : " + child.Child("nickname").GetValue(true);
                //output += " : score : " + child.Child("score").GetValue(true);
                //output += " : game : " + child.Child("game").GetValue(true);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        output += "------------------ \n";
    }
    private void UpdateOutputValues(ValueChangedEventArgs args)
    {
        string output = "Highscores \n";
        output += "------------------ \n";
        foreach (var child in args.Snapshot.Children)
        {
            try
            {
                Debug.Log("from UpdateOutputValues");
                Debug.Log("The Key : " + child.Key);
                output += "------------------ \n";
                output += "nickname : " + child.Child("nickname").GetValue(true) + "\n";
                output += "score : " + child.Child("score").GetValue(true) + "\n";
                output += "game : " + child.Child("game").GetValue(true) + "\n";
                output += "------------------ \n";
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        output += "------------------ \n";
        outputText.text = output;
    }

    public void ExitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
