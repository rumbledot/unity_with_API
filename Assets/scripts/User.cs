using System;
using UnityEditor.Build.Player;

[Serializable]
public class User
{
    public string Username;
    public string Password;

    public User() { }
    public User(string name, string password)
    {
        Username = name;
        Password = password;
    }
}
