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
    private TextField bgmDuration;
    private Button signInButton;
    private Button bgmButton;
    private Button signUpButton;
    private Button insta;
    private Button discord;
    private Button facebook;
    private Button ticktok;
    private Button bilibili;
    private Button twitter;
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
        bgmDuration = root.Q<TextField>("duration");
        ve = root.Q<VisualElement>("Info");
        bgmButton = root.Q<Button>("bgmsubmit");
        signInButton = root.Q<Button>("signIn");
        signUpButton = root.Q<Button>("signUp");
        insta = root.Q<Button>("Insta");
        discord = root.Q<Button>("Discord");
        facebook = root.Q<Button>("Facebook");
        ticktok = root.Q<Button>("TikTok");
        bilibili = root.Q<Button>("Bili");
        twitter = root.Q<Button>("Twitter");
        signInButton.clicked += JWTSignIn;
        bgmButton.clicked += BGMGenerate;
        signUpButton.clicked += signUp;
        insta.clicked += instagram;
        discord.clicked += disc;
        facebook.clicked += faceb;
        ticktok.clicked += tik;
        bilibili.clicked += bili;
        twitter.clicked += twitt;
    }

    private async void BGMGenerate()
    {
        
        string api_str = "http://203.132.92.92:46336/music/generate";
        BGMPrompt bGMPrompt = new BGMPrompt();
        bGMPrompt.descriptions = bgmInput.text;
        bGMPrompt.duration = Int32.Parse(bgmDuration.text);

        string file_name = bgmInput.text.Substring(0, 10) + ".wav";
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
                AddHelpBox(ve, "Authorization Failed, Please Sign In", "Warning");
            }
            else
            {
                httpClient.DefaultRequestHeaders.Accept.Add(contentType);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT.access_token);

                var postData = JsonUtility.ToJson(bGMPrompt);
                var contentData = new StringContent(postData, Encoding.UTF8, "application/json");
                Debug.Log(postData);
                Debug.Log(contentData);
                AddHelpBox(ve, "Downloading", "Info"); 
                var response = await httpClient.PostAsync(api_str, contentData);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using (var fs = new FileStream(savePath, FileMode.CreateNew))
                        {
                        Debug.Log(fs);
                        
                            await response.Content.CopyToAsync(fs);
                            AddHelpBox(ve, "Download Finished", "Info");
                        }
                       
                    }
                    catch (Exception e)
                    {
                        AddHelpBox(ve, e.Message, "Warning");
                    }
                }
                else
                {
                    AddHelpBox(ve, response.ReasonPhrase, "Warning");
                }
            }

        }

    }
    private void AddHelpBox(VisualElement ve, string message, string type)
    {
        if (type == "Info") {
            ve.Clear();
            var helpBox = new HelpBox(message, HelpBoxMessageType.Info);
            ve.Add(helpBox);
        }
        else if (type == "Warning")
        {
            ve.Clear();
            var helpBox = new HelpBox(message, HelpBoxMessageType.Warning);
            ve.Add(helpBox);
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
                AddHelpBox(ve, response.ReasonPhrase, "Warning");
                Debug.Log(response.ReasonPhrase);
                Debug.Log(ve);
                ve.Add(new Label("Authorization Failed, Please Sign Up If Haven't"));
            }
            else
            {
                try
                {
                    Debug.Log(response.Content);
                    Debug.Log("JWT" + JWT.access_token);
                    Debug.Log(response.Content);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JWT.access_token = JsonUtility.FromJson<JWTOKEN>(responseContent).access_token;
                    Debug.Log(JWT.access_token);
                    AddHelpBox(ve, "Signed In Successfully", "Info");
                }
                catch (Exception e)
                {
                    AddHelpBox(ve, e.Message, "Warning");
                }
                
            }
        }
    }
    private void signUp()
    {
        OpenURL("www.youtube.com");
    }
    private void instagram()
    {
        OpenURL("https://www.instagram.com/");
    }
    private void disc()
    {
        OpenURL("https://discord.com/");
    }
    private void faceb()
    {
        OpenURL("https://www.facebook.com/");
    }
    private void tik()
    {
        OpenURL("https://www.tiktok.com/");
    }
    private void twitt()
    {
        OpenURL("https://www.twitter.com/");
    }
    private void bili()
    {
        OpenURL("https://www.bilibili.com/");
    }
    private void OpenURL(string link)
    {
        Application.OpenURL(link);
    }
}

public class LoginPackage
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
    public int duration;
}