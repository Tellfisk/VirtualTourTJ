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
using System.Threading;

public class Authentication : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseStorage storage;

    public GameObject usernameField;
    public GameObject passwordField;

    private const string tourFileName = "tour.json";

    // Start is called before the first frame update
    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;
    }

    public void AuthUser(string email, string password)
    {
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

    public async void PostAuth(object sender, System.EventArgs eventArgs)
    {
        if(auth.CurrentUser == null)
        {
            Debug.Log("Login failed, try again.");
        }
        else
        {
            Debug.Log("User authenticated, attempting download.");

            string toursPath = Path.Combine(Application.persistentDataPath, "Tours");
            //Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Tours"), true);
            if (!Directory.Exists(toursPath))
            {
                Debug.Log("Creating tour main folder...");
                Directory.CreateDirectory(toursPath);
            }

            List<CloudTourReference> tourRefs = await FindAndDownloadFirebaseFolders(toursPath);

            foreach (CloudTourReference tourRef in tourRefs)
            {
                await tourRef.DownloadReference();
            }
            //SceneManager.LoadScene("Lobby");
        }
    }

    private Task<DocumentSnapshot> getSnapAsync(string tourName)
    {
        DocumentReference tourRef = db.Collection("tours").Document(tourName);
        return tourRef.GetSnapshotAsync();
    }

    private async Task<List<CloudTourReference>> FindAndDownloadFirebaseFolders(string toursPathLocal)
    {
        // TODO: only get tours for specific user.
        DocumentReference userRef = db.Collection("users").Document("CiJuY2f6tLavraUPTTSRoHnm3Km2");

        //
        DocumentSnapshot document = await userRef.GetSnapshotAsync();
        //
        List<CloudTourReference> tourRefs = new List<CloudTourReference>();

        Debug.Log(document);
        Dictionary<string, object> documentDictionary = document.ToDictionary();
        IList docTours = (IList)documentDictionary["tours"];

        foreach (string tourName in docTours)
        {

            string tempTourxoxo = tourName.Substring(0, 1).ToUpper() + tourName.Substring(1, tourName.Length-1).ToLower();
            Debug.Log(tempTourxoxo);
            string tourDLPath = Path.Combine(toursPathLocal, tempTourxoxo);
            if (!Directory.Exists(tourDLPath))
            {
                await Task.Run(() => (Directory.CreateDirectory(tourDLPath)));
            }
            else { Debug.Log("tour folder exists!"); }
            //
            DocumentSnapshot snap = await getSnapAsync(tourName);
            //
            Dictionary<string, object> tourDic = snap.ToDictionary();
            IList imageNames = (IList) tourDic["images"];

            CloudTourReference tourRef = new CloudTourReference(storage.GetReference("Tours/" + tourName), tourName, tourDLPath);

            foreach (string imgName in imageNames)
            {
                tourRef.AddImage(imgName);
            }

            tourRefs.Add(tourRef);

        }

        return tourRefs;

    }
}


public class CloudTourReference
{
    private StorageReference tourReference;

    private string tourName;
    private List<string> imgNames;

    private string localDLPath;


    public CloudTourReference(StorageReference tourReference, string tourName, string localDLPath)
    {
        this.tourReference = tourReference;
        this.tourName = tourName;
        this.localDLPath = localDLPath;
        this.imgNames = new List<string>();
    }

    // Optimal constructor

    //public CloudTourReference(StorageReference tourReference, string tourName, List<string> imgNames)
    //{
    //    this.tourReference = tourReference;
    //    this.tourName = tourName;
    // this.localDLPath = localDLPath;
    //    this.imgNames = imgNames;
    //}

    public void AddImage(string imageName)
    {
        this.imgNames.Add(imageName);
    }

    public async 
    Task DownloadReference()
    {

        string tourJsonName = "tour.json";

        Debug.Log(localDLPath);

        // download json
        string localJsonPath = Path.Combine(localDLPath, tourJsonName);
        Debug.Log(localJsonPath);
        //localJsonPath = Path.Combine(Application.streamingAssetsPath, "Tours", tourJsonName);
        //localJsonPath = Path.Combine(Application.streamingAssetsPath, "Tours", "Spaghettitall", tourJsonName);
        //Debug.Log(localJsonPath);
        //localJsonPath = Path.Combine(Application.streamingAssetsPath, tourJsonName);

        Debug.Log(localJsonPath);

        StorageReference childref = tourReference.Child(tourJsonName);

        Debug.Log("DOWNLOADING TO " + localJsonPath);
        await childref.GetFileAsync(localJsonPath);

        // download images
        foreach (string imgName in imgNames)
        {
            string localImgPath = Path.Combine(Application.streamingAssetsPath, imgName);
            Debug.Log(localImgPath);

            await tourReference.Child(imgName).GetFileAsync(localImgPath)
                .ContinueWith(resultTask =>
                {
                    if (!resultTask.IsFaulted && !resultTask.IsCanceled)
                    {
                        Debug.Log("Download finished.");
                    }
                    else
                    {
                        Debug.Log(resultTask);
                        Debug.Log("XXXXXXXx");
                    }
                });

        }

        Debug.Log("downloaded lmao");
    }

}

