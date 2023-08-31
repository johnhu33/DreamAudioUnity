using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Net.Http.Headers;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.PackageManager;
using System.IO;
using System.Net;

public class DreamAudio : EditorWindow
{
    private TextField userName;
    private TextField password;
    private TextField bgmInput;
    private Button signInButton;
    private Button bgmButton;
    public VisualElement ve;
    public JWTOKEN JWT = new JWTOKEN();
    [MenuItem("Window/UI Toolkit/DreamAudio")]
    public static void ShowExample()
    {
        DreamAudio wnd = GetWindow<DreamAudio>();
        wnd.titleContent = new GUIContent("DreamAudio");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/DreamAudio/DreamAudio.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
        userName = root.Q<TextField>("userName");
        password = root.Q<TextField>("password");
        bgmInput = root.Q<TextField>("bgmInput");
        ve = root.Q<VisualElement>("Info");
        bgmButton = root.Q<Button>("bgmsubmit");
        signInButton = root.Q<Button>("signIn");
        signInButton.clicked += JWTSignIn;
        bgmButton.clicked += BGMGenerate;
        


    }

    private async void BGMGenerate()
    {
        
        string api_str = "http://203.132.92.92:46336/music/generate";
        BGMPrompt bGMPrompt = new BGMPrompt();
        bGMPrompt.descriptions = bgmInput.text;
        string file_name = bgmInput.text + ".wav";
        Directory.CreateDirectory(Application.streamingAssetsPath);
        var savePath = Path.Combine(Application.streamingAssetsPath, file_name);
        var helpbox = new HelpBox("", HelpBoxMessageType.None);
        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(api_str);
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            Debug.Log(JWT.access_token);
            if (JWT.access_token == null)
            {
                helpbox = new HelpBox("Authorization Failed, Please Sign In", HelpBoxMessageType.Warning);
                ve.Add(helpbox);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Accept.Add(contentType);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT.access_token);

                var postData = JsonUtility.ToJson(bGMPrompt);
                var contentData = new StringContent(postData, Encoding.UTF8, "application/json");
                helpbox = new HelpBox("Started Downloading", HelpBoxMessageType.Info);
                ve.Add(helpbox);
                var response = await httpClient.PostAsync(api_str, contentData);

                if (response.IsSuccessStatusCode)
                {
                    using (var fs = new FileStream(savePath, FileMode.CreateNew))
                    {

                        await response.Content.CopyToAsync(fs);
                        helpbox = new HelpBox("Download Finished", HelpBoxMessageType.Info);
                        ve.Add(helpbox);
                    }
                }
                else
                {
                    helpbox = new HelpBox(response.ReasonPhrase, HelpBoxMessageType.Warning);
                    ve.Add(helpbox);
                }
            }

        }

    }

    private async void JWTSignIn()
    {
        
        LoginPackage payload = new LoginPackage();
        payload.email = userName.text;
        payload.password = password.text;
        Debug.Log(payload);
        string api_str = "http://203.132.92.92:46336/users/signin";
        var stringPayload = JsonUtility.ToJson(payload);
        Debug.Log(stringPayload);
        // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
        using (var httpClient = new HttpClient())
        {
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(api_str, httpContent);
            if (!response.IsSuccessStatusCode)
            {
                var helpbox = new HelpBox(response.ReasonPhrase, HelpBoxMessageType.Warning);
                Debug.Log(helpbox);
                Debug.Log(response.ReasonPhrase);
                Debug.Log(ve);
                ve.Add(helpbox);
            }
            else
            {
                var helpbox = new HelpBox("Signed In Successfully", HelpBoxMessageType.Info);
                Debug.Log(helpbox);
                Debug.Log(response.Content);
                Debug.Log("JWT" + JWT.access_token);
                Debug.Log(response.Content);
                string responseContent = await response.Content.ReadAsStringAsync();
                JWT.access_token = JsonUtility.FromJson<JWTOKEN>(responseContent).access_token;
                Debug.Log(JWT.access_token);
                ve.Add(helpbox);
            }
        }
    }
}public class LoginPackage
{
    public string email;
    public string password;
}
public class JWTOKEN
{
    public string access_token;
}
public class BGMPrompt
{
    public string descriptions;
}