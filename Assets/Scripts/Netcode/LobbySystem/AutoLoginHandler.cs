using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Fire_Pixel.Networking
{
    public class AutoLoginHandler : MonoBehaviour
    {
        [SerializeField] private string mainSceneName = "Main Menu";


        private async void Awake()
        {
            await UnityServices.InitializeAsync();

            //AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            //DebugLogger.Log("Logged into saved account!");
            //DebugLogger.Log("Player ID: " + AuthenticationService.Instance.PlayerId);

            SceneManager.LoadScene(mainSceneName);
        }
    }
}