using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;

public class Authentication : MonoBehaviour
{
    private FirebaseAuth auth;

    // Start is called before the first frame update
    void Awake()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }

    public void InitializeFirebase(string email, string password)
    {
        Debug.Log(auth);
        auth.SignInWithEmailAndPasswordAsync("dummy@gmail.com", "123456").ContinueWith(task => {
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
            SceneChange.ChangeSceneToLobby();
        });
    }
    public void Hei()
    {
        Debug.Log("off");
    }

}
