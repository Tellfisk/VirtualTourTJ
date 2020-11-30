using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class Authentication : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseStorage storage;

    public GameObject usernameField;
    public GameObject passwordField;

    // Start is called before the first frame update
    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;

        ////
        string localDLPath = Path.Combine(Application.streamingAssetsPath);
        string tourFolder = localDLPath + "/" + "nice/";
        StartCoroutine(LoadVirtualTour(tourFolder));
        ////
    }

    public void AuthUser(string email, string password)
    {
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

            FirebaseUser newUser = task.Result;
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
            FindAndDownloadFirebaseFolders();
            //SceneManager.LoadScene("Lobby");
        }
    }

   

    IEnumerator LoadVirtualTour(string jsonPath)
    {
        HashSet<string> imageNames = new HashSet<string>();
        string dataAsJson = "";
        jsonPath += "tour.json";
        Debug.Log(jsonPath);
        VirtualTour vt = null;

        UnityWebRequest www = UnityWebRequest.Get(jsonPath);
        yield return www.SendWebRequest();
        dataAsJson = www.downloadHandler.text;

        try
        {
            vt = JsonConvert.DeserializeObject<VirtualTour>(dataAsJson);
        }
        catch (JsonException je)
        {
            // do nothing, try to continue
            Debug.Log("QUESTDEB: error with deserialization : " + je.Message);
        }

        if (vt != null)
        {
            Dictionary<int, VirtualState> states = vt.states;
            foreach (VirtualState state in states.Values)
            {
                Debug.Log("ÅJADDAAA!   " + state.img + " ,  " + state.img2);
                imageNames.Add(state.img);
                imageNames.Add(state.img2);
            }
        }
        yield return imageNames;
    }

    private Task DownloadJson(string firebaseFolderName, string localDLPath)
    {
        string filename = "tour.json";

        StorageReference dirReference = storage.GetReference("Tours/" + firebaseFolderName);
        StorageReference jsonReference = dirReference.Child(filename);

        string localJsonPath = Path.Combine(localDLPath, filename);

        Debug.Log("Download to: " + localJsonPath);

        // Download tour.json to the local filesystem
        //System.Threading.Tasks.Task downloadTask = jsonReference.GetFileAsync(localDLPath);
        //yield return new WaitUntil(() => downloadTask.IsCompleted);

        Task taskman = jsonReference.GetFileAsync(localJsonPath);

        taskman.ContinueWith(task => { Debug.Log("done with download!! -- " + Task.CurrentId); Debug.Log(task.IsFaulted); });
        return taskman;

    }

    private void FindAndDownloadFirebaseFolders()
    {
        // TODO: only get tours for specific user.
        CollectionReference usersRef = db.Collection("users");
        

        usersRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            List<string> tourNames = new List<string>();

            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                var aaa = document;
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                IList xxx = (IList)documentDictionary["tours"];
                foreach (string lol in xxx)
                {
                    tourNames.Add(lol);
                }
            }
            return tourNames;

        }).ContinueWith(task => {
            List<string> tourNames = task.Result;  //TODO: Contains duplicates
            

            var tasks = new List<Task>();
            

            Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Tours"), true);

            foreach (string tourName in tourNames)
            {
                string localDLPath = Path.Combine(Application.streamingAssetsPath, "Tours", tourName);

                try
                {
                    if (!Directory.Exists(localDLPath))
                    {
                        //Debug.Log("Path does not exist for: " + tourName);
                        Directory.CreateDirectory(localDLPath);
                        Task taskman = DownloadJson(tourName, localDLPath);
                        tasks.Add(taskman);
                        
                        Debug.Log("penis " + taskman.Status);

                    }
                    else
                    {
                        //Debug.Log(String.Format("Directory for {0} already exists", tourName));
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Task.WaitAny(tasks.ToArray());

            Debug.Log("en ferdig");
            Debug.Log(tasks.Count);
            
            Task.WaitAll(tasks.ToArray());

        }).ContinueWith(task => { 
            Debug.Log("spaghetti"); 
        });
        Debug.Log("UTENFOOOR");
    }
}
