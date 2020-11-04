using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.UI;
using System.Threading.Tasks;
using Firebase;

public class AuthController : MonoBehaviour
{
    public InputField emailInput, passwordInput;
    public Button loginButton, loginAsGuestButton, registerButton;
    public Text outputText;
    public Text loginButtonText;
    private Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseUser user;
    private string msg;

    private void Start()
    {
        InitializeFirebase();
    }
    private void Update()
    {
        
    }
    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                DisplayOutput("Log in as " + user.Email);
            }
        }
    }
    public void LogInOrOut() 
    {
        if (loginButtonText.text == "login")
        {
            if (emailInput.text.Equals("") && passwordInput.text.Equals(""))
            {
                DisplayOutput("Please input your email and password to login");
            }
            else
            {
                Login();
            }
        }
        else if (loginButtonText.text == "logout")
        {
            Logout();
        }
    }

    public void Login() 
    {
        outputText.text = "Login processing";
        auth.SignInWithEmailAndPasswordAsync(
            emailInput.text, passwordInput.text)
            .ContinueWith(( task => {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e =
                            task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e =
                        task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    return;
                }
                user = task.Result;
                print("Login successful " + user.Email);
            }));

        loginButtonText.text = "logout";
        DisplayOutput("Login successful");
        DisplayOutput("Hi " + user.Email);
    }
    public void Logout() 
    {
        loginButtonText.text = "login";
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
        DisplayOutput("Logout successful");
    }
    public void LoginAsGuest() 
    {
        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync()
            .ContinueWith((task =>
            {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e =
                        task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e =
                        task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    return;
                }
                if (task.IsCompleted)
                {
                    print("Guest login successful");
                    RegisterSuccessful();
                    return;
                }
            }));
    }
    public void RegisterNewUser() 
    {
        if (emailInput.text.Equals("") || passwordInput.text.Equals(""))
        {
            DisplayOutput("Please input your email and password");
            return;
        }
        else
        {
            FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(
                emailInput.text, passwordInput.text)
                .ContinueWith((task =>
                {
                    if (task.IsCanceled)
                    {
                        Firebase.FirebaseException e =
                            task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                        GetErrorMessage((AuthError)e.ErrorCode);
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Firebase.FirebaseException e =
                            task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                        GetErrorMessage((AuthError)e.ErrorCode);
                        return;
                    }
                    if (task.IsCompleted)
                    {
                        Firebase.Auth.FirebaseUser newUser = task.Result;
                        Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                            newUser.Email, newUser.UserId);
                    }
                }));
            DisplayOutput("Registered");
        }
    }

    private void RegisterSuccessful()
    {
        emailInput.text = "";
        passwordInput.text = "";
        DisplayOutput("New user registered");
    }

    private void GetErrorMessage(AuthError err)
    {
        switch (err)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                print("Account already exists, please use another one");
                break;
            case AuthError.MissingPassword:
                print("Missing password");
                break;
            case AuthError.WrongPassword:
                print("Wrong password");
                break;
            case AuthError.InvalidEmail:
                print("Invalid email");
                break;
        }
        print(err.ToString());
        outputText.text = err.ToString();
    }
    private void DisplayOutput(string t)
    {
        outputText.text = t;
    }
}
