using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Authentication : MonoBehaviour
{
    private FirebaseAuth auth;

    public GameObject usernameField;
    public GameObject passwordField;

    // Start is called before the first frame update
    void Awake()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }

    public void AuthUser(string email, string password)
    {
        Debug.Log(auth);
        // "dummy@gmail.com", "123456"
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            Debug.Log(newUser.DisplayName);
            return;
        });


    }
    public void Hei()
    {
        auth.SignOut();
        auth.StateChanged += PostAuth;
        AuthUser(
            usernameField.GetComponent<TMPro.TMP_InputField>().text, 
            passwordField.GetComponent<TMPro.TMP_InputField>().text
            );
        
    }

    public void PostAuth(object sender, System.EventArgs eventArgs)
    {
        if(auth.CurrentUser == null)
        {
            Debug.Log("Login failed, try again.");
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
        
    }
}
