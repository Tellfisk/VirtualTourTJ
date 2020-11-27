using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System;

public class Authentication : MonoBehaviour
{
    private FirebaseAuth auth;
    FirebaseFirestore db;

    public GameObject usernameField;
    public GameObject passwordField;

    // Start is called before the first frame update
    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
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
        AuthUser(usernameField.GetComponent<TMPro.TMP_InputField>().text, passwordField.GetComponent<TMPro.TMP_InputField>().text);
    }

    public void PostAuth(object sender, System.EventArgs eventArgs)
    {
        if(auth.CurrentUser == null)
        {
            Debug.Log("Login failed, try again.");
        }
        else
        {
            FindAndDownloadFirebaseFolders();
            //SceneManager.LoadScene("Lobby");
        }
    }

    private void /*IEnumerator*/ DownloadRoutine(string firebaseBucketName)
    {
        var storage = FirebaseStorage.DefaultInstance;
        var texreference = storage.GetReference("tours/" + firebaseBucketName);
        Debug.Log("tours/" + firebaseBucketName);

        //System.Threading.Tasks.Task<byte[]> downloadTask = texreference.GetBytesAsync(long.MaxValue);
        //yield return new WaitUntil(() => downloadTask.IsCompleted);


        //TODO: Display whether downloads were successful
    }

    private void FindAndDownloadFirebaseFolders()
    {
        List<string> folderNames = new List<string>();

        CollectionReference usersRef = db.Collection("users");
        usersRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                Debug.Log(String.Format("User: {0} \nCompany: {1} \tName: {2}", document.Id, documentDictionary["company"], documentDictionary["name"]));
                List<string> list = (List<string>) documentDictionary["tours"];
                foreach (string sss in list)
                {
                    Debug.Log(sss);
                }
            }
        });

        foreach (string folderName in folderNames) 
        {
            DownloadRoutine(folderName);
        }
    }
}
