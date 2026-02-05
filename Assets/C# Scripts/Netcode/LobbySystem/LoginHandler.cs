using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace FirePixel.Networking
{
    public class LoginHandler : MonoBehaviour
    {
        public static LoginHandler Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }


        [SerializeField] private GameObject blackScreenCover;
        [SerializeField] private GameObject invisibleScreenCover;

        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private TMP_InputField passwordField;

        [SerializeField] private TextMeshProUGUI errorTextField;
        [SerializeField] private float errorFadeSpeed;

        [SerializeField] private string mainSceneName = "Main Menu";
        private string loginSceneName;

        private bool errorVisible;


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Header(">>DEBUG<<")]

        [Header("Make a temp auto account while testing stuff, login automatically")]
        [SerializeField] private bool devAutoLogin;

        [Header("Auto log out before starting the game and after ending it,\noverriden by devAutoLogin")]
        [SerializeField] private bool autoLogOut;

        [SerializeField] private bool printLoginErrors;
#endif


        private const string _invalidUsernameError = "Signing Up failed:\nUsername does not match requirements. Insert only letters, digits and symbols among {., -, _, @}. With a minimum of 3 characters and a maximum of 20";
        private const string _invalidPasswordError = "Signing Up failed:\nPassword does not match requirements. Insert at least 1 uppercase, 1 lowercase, 1 digit and 1 symbol. With minimum 8 characters and a maximum of 30";
        private const string _usernameTakenError = "Signing Up failed:\nUsername is already taken.";

        private const string _emptyFieldError = "Signing In/Up failed:\nPlease fill in all the fields.";
        private const string _wrongLoginInfoError = "Signing In failed:\nAccount doesnt exist or password is wrong.";


        private AsyncOperation mainSceneLoadOperation;


        private async void Start()
        {
            loginSceneName = SceneManager.CurrentSceneName;
            mainSceneLoadOperation = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive, false);

            mainSceneLoadOperation.completed += (_) =>
            {
                SceneManager.UnLoadSceneAsync(loginSceneName);
            };

            await UnityServices.InitializeAsync();

            TryAutoLoginWithSessionTokenAsync();
        }


        private async void TryAutoLoginWithSessionTokenAsync()
        {
            blackScreenCover.SetActive(true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD

            //while in dev mode, create a temp account and auto login everytime.
            if (devAutoLogin)
            {
                AuthenticationService.Instance.SignOut();

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                mainSceneLoadOperation.allowSceneActivation = true;

                DebugLogger.Log("Logged in with auto dev account automatically");

                return;
            }

            //logout of logged in account before any code runs.
            if (autoLogOut && AuthenticationService.Instance.SessionTokenExists)
            {
                AuthenticationService.Instance.SignOut();
                AuthenticationService.Instance.ClearSessionToken();

                DebugLogger.Log("Session Token cleared, logged out");

                blackScreenCover.SetActive(false);

                return;
            }
#endif

            //login with previously logged in account if that SessionToken is still valid
            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                mainSceneLoadOperation.allowSceneActivation = true;

                DebugLogger.Log("Logged in with cached account automatically, name: " + AuthenticationService.Instance.PlayerInfo.Username);

                return;
            }

            blackScreenCover.SetActive(false);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FadeErrorOut();

                if (usernameField.isFocused)
                {
                    usernameField.DeactivateInputField();
                    passwordField.ActivateInputField();
                }
                else
                {
                    passwordField.DeactivateInputField();
                    usernameField.ActivateInputField();
                }
            }
        }


        /// <summary>
        /// Reset errorField when username or password is being edited
        /// </summary>
        public void FadeErrorOut()
        {
            if (errorVisible)
            {
                StartCoroutine(FadeOutErrorCode());
            }
            errorVisible = false;
        }

        private void DisplayError()
        {
            StopAllCoroutines();

            errorTextField.color = new Color(errorTextField.color.r, errorTextField.color.g, errorTextField.color.b, 1);
            errorVisible = true;
        }

        private IEnumerator FadeOutErrorCode()
        {
            // Save color
            Color cColor = errorTextField.color;

            while (cColor.a > 0)
            {
                yield return null;
                //fade out by decreasing alpha
                cColor.a -= errorFadeSpeed * Time.deltaTime;

                //update textColor
                errorTextField.color = cColor;
            }

            // Clear text and set alpha back to 1
            errorTextField.text = "";
            errorTextField.color = new Color(cColor.r, cColor.g, cColor.b, 1);
        }


        public void UpdateUsername(string newUsername)
        {
            if (string.IsNullOrEmpty(newUsername))
            {
                return;
            }

            bool valid = IsCharacterValid(newUsername[^1]);

            if (valid == false)
            {
                usernameField.text = newUsername.Substring(0, newUsername.Length - 1);
            }
        }



        /// <summary>
        /// Return if char is an unvalid character
        /// </summary>
        private bool IsCharacterValid(char addedChar)
        {
            return char.IsLetterOrDigit(addedChar) || addedChar == '_' || addedChar == '-' || addedChar == '@' || addedChar == '.';
        }


        public async void TrySignIn() => await TrySignInAsync(usernameField.text, passwordField.text);

        /// <summary>
        /// Try to sign in with username and password
        /// </summary>
        private async Task TrySignInAsync(string username, string password)
        {
            invisibleScreenCover.SetActive(true);

            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

                mainSceneLoadOperation.allowSceneActivation = true;

                DebugLogger.Log("Player is signed in with name: " + AuthenticationService.Instance.PlayerInfo.Username);
            }
            catch (Exception ex)
            {
                string exString = ex.ToString();

                // Username doesnt exist or password is wrong
                if (exString.StartsWith("Unity.Services.Core.RequestFailedException: Invalid username or password"))
                {
                    errorTextField.text = _wrongLoginInfoError;
                    DisplayError();
                }
                // Username and/or Password are not in the correct format (one of the fields are empty)
                else if (exString.StartsWith("Unity.Services.Authentication.AuthenticationException: Username and/or Password are not in the correct format"))
                {
                    errorTextField.text = _emptyFieldError;
                    DisplayError();
                }

                invisibleScreenCover.SetActive(false);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
                DebugLogger.Log(ex, printLoginErrors);
#endif
            }
        }


        public async void TrySignUp() => await TrySignUpAsync(usernameField.text, passwordField.text);

        /// <summary>
        /// Try to sign up with username and password
        /// </summary>
        private async Task TrySignUpAsync(string username, string password)
        {
            invisibleScreenCover.SetActive(true);

            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

                mainSceneLoadOperation.allowSceneActivation = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

                DebugLogger.Log("Player is signed up with name: " + AuthenticationService.Instance.PlayerInfo.Username);
#endif
            }
            catch (AuthenticationException ex)
            {
                string exString = ex.ToString();

                //Username is already taken
                if (exString.StartsWith("Unity.Services.Authentication.AuthenticationException: username already exists"))
                {
                    errorTextField.text = _usernameTakenError;
                    DisplayError();
                }
                //Username and/or Password are not in the correct format (one of the fields are empty
                else if (exString.StartsWith("Unity.Services.Authentication.AuthenticationException: Username and/or Password are not in the correct format"))
                {
                    errorTextField.text = _emptyFieldError;
                    DisplayError();
                }

                invisibleScreenCover.SetActive(false);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (printLoginErrors)
                {
                    DebugLogger.Log(ex);
                }
#endif
            }
            catch (RequestFailedException ex)
            {
                string exString = ex.ToString();

                //Username does not match requirements. Insert only letters, digits and symbols among {., -, _, @}. With a minimum of 3 characters and a maximum of 20
                if (exString.StartsWith("Unity.Services.Core.RequestFailedException: Username does not match requirements"))
                {
                    errorTextField.text = _invalidUsernameError;
                    DisplayError();

                    DebugLogger.Log("DebugLogger.Loged");
                }
                //Password does not match requirements. Insert at least 1 uppercase, 1 lowercase, 1 digit and 1 symbol. With minimum 8 characters and a maximum of 30
                else if (exString.StartsWith("Unity.Services.Core.RequestFailedException: Password does not match requirements"))
                {
                    errorTextField.text = _invalidPasswordError;
                    DisplayError();
                }

                invisibleScreenCover.SetActive(false);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (printLoginErrors)
                {
                    DebugLogger.Log(ex);
                }
#endif
            }
        }



#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnApplicationQuit()
        {
            if ((devAutoLogin || autoLogOut) && AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                AuthenticationService.Instance.ClearSessionToken();

                Debug.Log("Session token cleared on application quit.");
            }
        }
#endif
    }
}