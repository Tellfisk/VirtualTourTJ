using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Storage;
using UnityEngine;
using Firebase.Firestore;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

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

    public void InitAuthorization()
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

            string toursPath = Path.Combine(Application.streamingAssetsPath, "Tours");

            //Directory.Delete(Path.Combine(Application.streamingAssetsPath, "Tours"), true);
            if (Directory.Exists(toursPath))
            {
                Directory.Delete(toursPath, true);
            }

            Debug.Log("Creating tour main folder...");
            Directory.CreateDirectory(toursPath);

            List<CloudTourReference> tourRefs = await FindAndDownloadFirebaseFolders(toursPath);

            if(tourRefs != null) {
                foreach (CloudTourReference tourRef in tourRefs)
                {
                    await tourRef.DownloadReference();
                }

                SceneManager.LoadScene("Lobby");
            }

        }
    }

    private Task<DocumentSnapshot> getSnapAsync(string tourName)
    {
        DocumentReference tourRef = db.Collection("tours").Document(tourName);
        return tourRef.GetSnapshotAsync();
    }

    private async Task<List<CloudTourReference>> FindAndDownloadFirebaseFolders(string toursPathLocal)
    {
        DocumentReference userRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        
        DocumentSnapshot userDocumentSnap = await userRef.GetSnapshotAsync();
        List<CloudTourReference> tourRefs = new List<CloudTourReference>();

        Dictionary<string, object> documentDictionary = userDocumentSnap.ToDictionary();
        IList docTours = (IList)documentDictionary["tours"];

        if(docTours.Count == 0)
        {
            // TODO: add this as a printout in the app.
            Debug.Log("USER HAS NO TOURS ASSIGNED.");
            return null;
        }

        foreach (string tourName in docTours)
        {
            string tourDLPath = Path.Combine(toursPathLocal, tourName);

            if (!Directory.Exists(tourDLPath))
            {
                await Task.Run(() => (Directory.CreateDirectory(tourDLPath)));
            }
            else { Debug.Log("tour folder exists!"); }
            
            DocumentSnapshot tourDocumentSnap = await getSnapAsync(tourName);
            Dictionary<string, object> tourDic = tourDocumentSnap.ToDictionary();
            IList imageNames = (IList) tourDic["images"];

            CloudTourReference tourRef = new CloudTourReference(
                storage.GetReference("Tours/" + tourName), 
                tourName, 
                tourDLPath
                );

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
    //    this.localDLPath = localDLPath;
    //    this.imgNames = imgNames;
    //}

    public void AddImage(string imageName)
    {
        this.imgNames.Add(imageName);
    }

    public async Task DownloadReference()
    {

        string tourJsonName = "tour.json";

        Debug.Log(localDLPath);

        // download json
        string localJsonPath = Path.Combine(localDLPath, tourJsonName);

        StorageReference childref = tourReference.Child(tourJsonName);

        Debug.Log("DOWNLOADING TO " + localJsonPath);
        await childref.GetFileAsync(localJsonPath);

        // download images
        foreach (string imgName in imgNames)
        {
            string localImgPath = Path.Combine(Application.streamingAssetsPath, imgName);
            localImgPath = Path.Combine(localDLPath, imgName);
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
                    }
                });
        }

        Debug.Log("All downloads complete");
    }

}

