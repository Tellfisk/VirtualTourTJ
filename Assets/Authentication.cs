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

        ////
        //string localDLPath = Path.Combine(Application.streamingAssetsPath);
        //string tourFolder = localDLPath + "/" + "nice/";
        //StartCoroutine(LoadVirtualTour(tourFolder));
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
            Debug.Log("User authenticated, attempting download.");
            FindAndDownloadFirebaseFolders();
            //SceneManager.LoadScene("Lobby");
        }
    }



    //IEnumerator LoadVirtualTour(CloudImageReference imageRef)
    //{
    //    string jsonPath = Path.Combine(imageRef.localDLPath, tourFileName);

    //    HashSet<string> imageNames = new HashSet<string>();
    //    string dataAsJson = "";
    //    jsonPath += tourFileName;
    //    Debug.Log(jsonPath);
    //    VirtualTour vt = null;

    //    UnityWebRequest www = UnityWebRequest.Get(jsonPath);
    //    yield return www.SendWebRequest();
    //    dataAsJson = www.downloadHandler.text;

    //    try
    //    {
    //        vt = JsonConvert.DeserializeObject<VirtualTour>(dataAsJson);
    //    }
    //    catch (JsonException je)
    //    {
    //        // do nothing, try to continue
    //        Debug.Log("QUESTDEB: error with deserialization : " + je.Message);
    //    }

    //    if (vt != null)
    //    {
    //        Dictionary<int, VirtualState> states = vt.states;
    //        foreach (VirtualState state in states.Values)
    //        {
    //            Debug.Log("ÅJADDAAA!   " + state.img + " ,  " + state.img2);
    //            imageNames.Add(state.img);
    //            imageNames.Add(state.img2);

    //            (imageRef.dirReference.Child(state.img)).GetFileAsync(imageRef.localDLPath);
    //            (imageRef.dirReference.Child(state.img2)).GetFileAsync(imageRef.localDLPath);

    //        }
    //    }
    //    yield return imageNames;
    //}

    //async Task DownloadImages(CloudImageReference imageRef)
    //{
    //    string jsonPath = Path.Combine(imageRef.localDLPath, tourFileName);

    //    HashSet<string> imageNames = new HashSet<string>();
    //    string dataAsJson = "";
    //    Debug.Log(jsonPath);
    //    VirtualTour vt = null;

    //    UnityWebRequest www = UnityWebRequest.Get(jsonPath);
    //    www.SendWebRequest();
    //    dataAsJson = www.downloadHandler.text;
    //    Debug.Log("jsondata: " + dataAsJson);
    //    try
    //    {
    //        vt = JsonConvert.DeserializeObject<VirtualTour>(dataAsJson);
    //    }
    //    catch (JsonException je)
    //    {
    //        // do nothing, try to continue
    //        Debug.Log("QUESTDEB: error with deserialization : " + je.Message);
    //    }

    //    if (vt != null)
    //    {
    //        Dictionary<int, VirtualState> states = vt.states;
    //        foreach (VirtualState state in states.Values)
    //        {
    //            Debug.Log("ÅJADDAAA!   " + state.img + " ,  " + state.img2);
    //            imageNames.Add(state.img);
    //            imageNames.Add(state.img2);

    //            await (imageRef.dirReference.Child(state.img)).GetFileAsync(imageRef.localDLPath);
    //            await (imageRef.dirReference.Child(state.img2)).GetFileAsync(imageRef.localDLPath);

    //        }
    //    }
        
    //}

    private Task DownloadJson(StorageReference dirReference, string localDLPath)
    {

        string filename = tourFileName;
        StorageReference jsonReference = dirReference.Child(filename);

        string localJsonPath = Path.Combine(localDLPath, filename);

        Debug.Log("Download to: " + localJsonPath);

        // Download tour.json to the local filesystem
        //System.Threading.Tasks.Task downloadTask = jsonReference.GetFileAsync(localDLPath);
        //yield return new WaitUntil(() => downloadTask.IsCompleted);

        Task taskman = jsonReference.GetFileAsync(localJsonPath);

        //taskman.ContinueWith(task => { 
        //    Debug.Log("done with download!! -- " + Task.CurrentId); 
        //    Debug.Log(String.Format("Tour download for {0} was faulty? --> {1}", localJsonPath, task.IsFaulted));
        //    Debug.Log(task.Exception);
        //    });
        return taskman;

    }

    private Task<DocumentSnapshot> getSnapAsync(string tourName)
    {
        DocumentReference tourRef = db.Collection("tours").Document(tourName);
        return tourRef.GetSnapshotAsync();
    }

    private async void FindAndDownloadFirebaseFolders()
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
            //
            DocumentSnapshot snap = await getSnapAsync(tourName);
            //
            Dictionary<string, object> tourDic = snap.ToDictionary();
            IList imageNames = (IList) tourDic["images"];

            CloudTourReference tourRef = new CloudTourReference(storage.GetReference("Tours/" + tourName), tourName);

            foreach (string imgName in imageNames)
            {
                tourRef.AddImage(imgName);
            }

            tourRefs.Add(tourRef);

        }

        foreach (CloudTourReference tourRef in tourRefs)
        {
            string tourName = tourRef.tourName;
            StorageReference tourReference = tourRef.tourReference;
            List<string> imgNames = tourRef.imgNames;

            Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Tours"), true);
            string createdDir = Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "Tours")).FullName;

            string localDLPath = Path.Combine(createdDir, tourName);

            string tourJsonName = "tour.json";

            try
            {
                if (!Directory.Exists(localDLPath))
                {
                    Debug.Log("Path does not exist for: " + tourName);
                    Directory.CreateDirectory(localDLPath);

                    

                    // download json
                    string localJsonPath = Path.Combine(localDLPath, tourJsonName);
                    Debug.Log(localJsonPath);
                    string localJsonUri = new System.Uri(localJsonPath).AbsoluteUri;
                    localJsonUri = "file:///C:/Users/jovli/OneDrive/Documents/UnityProjects/TJFreshAFVirtualTour/VirtualTourTJ/Assets/StreamingAssets/Tours/ooOOOOHcountry/tour.json";
                    //localJsonUri = localJsonPath;


                    StorageReference childref = tourReference.Child(tourJsonName);

                    Debug.Log("DOWNLOADING TO " + localJsonUri);
                    Task oioi = childref.GetFileAsync(localJsonUri, new Firebase.Storage.StorageProgress<DownloadState>((DownloadState state) =>
                    {
                        // called periodically during the download
                        Debug.Log(String.Format(
                          "Progress: {0} of {1} bytes transferred.",
                          state.BytesTransferred,
                          state.TotalByteCount
                        ));
                    }), CancellationToken.None)
                        ;
                     //   .ContinueWith(task => { Debug.Log(task.Exception); Debug.Log(task.IsCompleted); Debug.Log(task.IsFaulted); });


                    await oioi;

                    // download images
                    //foreach (string imgName in imgNames)
                    //{
                    //    string localImgPath = Path.GetFullPath(Path.Combine(localDLPath, imgName));
                    //    string localImgUri = new System.Uri(localImgPath).AbsoluteUri;
                    //    Debug.Log(localImgUri);

                    //    await tourReference.Child(imgName).GetFileAsync(localImgUri, new Firebase.Storage.StorageProgress<DownloadState>((DownloadState state) => {
                    //        // called periodically during the download
                    //        Debug.Log(String.Format(
                    //          "Progress: {0} of {1} bytes transferred.",
                    //          state.BytesTransferred,
                    //          state.TotalByteCount
                    //        ));
                    //    }), CancellationToken.None);
                    //}

                    Debug.Log("downloaded lmao");

                }
                else
                {
                    Debug.Log(String.Format("Directory for {0} already exists", tourName));
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


            //foreach (CloudTourReference tourRef in tourRefs) {
            //    await tourRef.DownloadReference();
            //}


            

        //userRef.GetSnapshotAsync().ContinueWith(task =>
        //{
        //DocumentSnapshot document = task.Result;
        //    List<CloudTourReference> tourRefs = new List<CloudTourReference>();

        //    
        //    Debug.Log(document);
        //    Dictionary<string, object> documentDictionary = document.ToDictionary();
        //    IList docTours = (IList)documentDictionary["tours"];

        //    foreach (string tourName in docTours)
        //    {
        //        Task<DocumentSnapshot> taskman = getSnapAsync(tourName);
        //        Task<IList<string>> newTask = taskman.ContinueWith(taskInner =>
        //        {
        //            DocumentSnapshot snap = taskInner.Result;

        //            Dictionary<string, object> xxx = snap.ToDictionary();
        //            IList<string> imageNames = (IList<string>)documentDictionary["images"];
        //            return imageNames;
        //        });

        //        CloudTourReference tourRef = new CloudTourReference(storage.GetReference("Tours/" + tourName), tourName);

        //        foreach (string imgName in imageNames)
        //        {
        //            tourRef.addImage(imgName);
        //        }

        //        tourRefs.Add(tourRef);

        //    }

        //    Debug.Log(tourRefs);
        //});

        //CollectionReference usersRef = db.Collection("users");
        //usersRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        //{
        //    List<string> tourNames = new List<string>();

        //    QuerySnapshot snapshot = task.Result;
        //    foreach (DocumentSnapshot document in snapshot.Documents)
        //    {
        //        var aaa = document;
        //        Dictionary<string, object> documentDictionary = document.ToDictionary();
        //        IList docTours = (IList)documentDictionary["tours"];
        //        foreach (string lol in docTours)
        //        {
        //            tourNames.Add(lol);
        //        }
        //    }


        //    return tourNames;

        //})
        //    .ContinueWithOnMainThread(task => {
        //    List<string> tourNames = task.Result;  //TODO: Contains duplicates


        //    var tasks = new List<Task>();

        //    Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Tours"), true);
        //    Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "Tours"));

        //    List<CloudImageReference> imageRefs = new List<CloudImageReference>();

        //    foreach (string tourName in tourNames)
        //    {
        //        string localDLPath = Path.Combine(Application.streamingAssetsPath, "Tours", tourName);

        //        Debug.Log(tourName);

        //        StorageReference dirReference = storage.GetReference("Tours/" + tourName);
        //        imageRefs.Add(new CloudImageReference(localDLPath, dirReference));

        //        try
        //        {
        //            if (!Directory.Exists(localDLPath))
        //            {
        //                Debug.Log("Path does not exist for: " + tourName);
        //                Directory.CreateDirectory(localDLPath);

        //                Task taskman = DownloadJson(dirReference, localDLPath);
        //                taskman.Wait();
        //                tasks.Add(taskman);
        //                //Debug.Log("penis " + taskman.Status);

        //            }
        //            else
        //            {
        //                Debug.Log(String.Format("Directory for {0} already exists", tourName));
        //            }
        //        }
        //        catch (IOException ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }

        //    //Task.WaitAny(tasks.ToArray());
        //    //Debug.Log("en ferdig");
        //    //Debug.Log(tasks.Count);

        //    //Task.WaitAll(tasks.ToArray());

        //    return imageRefs;

        //// After jsons finished downloading.
        //}).ContinueWithOnMainThread(task => { 
        //    Debug.Log("spaghetti");

        //    Debug.Log(task);
        //    List<CloudImageReference> tourLocalPaths = task.Result;
        //    //List<CloudImageReference> tourLocalPaths = null;
        //    Debug.Log("AAAAAAAA");
        //    Debug.Log(tourLocalPaths);
        //    Debug.Log("spaaaa");

        //    var tasks = new List<Task>();

        //    foreach (CloudImageReference imageRef in tourLocalPaths)
        //    {
        //        Debug.Log(imageRef.localDLPath);
        //        Task taskman = DownloadImages(imageRef);
        //        taskman.Start();
        //        tasks.Add(taskman);
        //    }

        //    Task.WaitAll(tasks.ToArray());
        //    //vt = JsonConvert.DeserializeObject<VirtualTour>(dataAsJson);

        //});
        //Debug.Log("UTENFOOOR");
    }
}

public class CloudTourReference
{
    public StorageReference tourReference;

    public string tourName;
    public List<string> imgNames;


    public CloudTourReference(StorageReference tourReference, string tourName)
    {
        this.tourReference = tourReference;
        this.tourName = tourName;
        this.imgNames = new List<string>();
    }

    public CloudTourReference(StorageReference tourReference, string tourName, List<string> imgNames)
    {
        this.tourReference = tourReference;
        this.tourName = tourName;
        this.imgNames = imgNames;
    }

    public void AddImage(string imageName)
    {
        this.imgNames.Add(imageName);
    }

    public async 
    Task DownloadReference()
    {
        Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Tours"), true);
        Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "Tours"));

        string localDLPath = Path.Combine(Application.streamingAssetsPath, "Tours", tourName);

        string tourJsonName = "tour.json";

        

        try
        {
            if (!Directory.Exists(localDLPath))
            {
                Debug.Log("Path does not exist for: " + tourName);
                Directory.CreateDirectory(localDLPath);

                // download json
                string localJsonPath = Path.Combine(localDLPath, tourJsonName);
                await tourReference.Child(tourJsonName).GetFileAsync(localJsonPath);

                // download images
                foreach (string imgName in imgNames)
                {
                    string localImgPath = Path.Combine(localDLPath, imgName);
                    await tourReference.Child(imgName).GetFileAsync(localImgPath);
                }

                Debug.Log("downloaded lmao");

            }
            else
            {
                Debug.Log(String.Format("Directory for {0} already exists", tourName));
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

}
