using TMPro;
using UnityEngine;


namespace Fire_Pixel.Networking
{
    public class PlayerNameHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TextMeshProUGUI previewTextField;

        [SerializeField] private RandomPlayerNamesSO randomPlayerNames;

        private const string PLAYERNAME_PATH = "PlayerName.fpx";


        private async void Awake()
        {
            (bool success, ValueWrapper<string> savedPlayerName) = await FileManager.LoadInfoAsync<ValueWrapper<string>>(PLAYERNAME_PATH);

            if (success)
            {
                nameInputField.text = savedPlayerName.Value;
                ClientManager.SetLocalUsername(savedPlayerName.Value);
            }
            else
            {
                string funnyName = randomPlayerNames.GetRandomFunnyName();

                previewTextField.text = funnyName;
                ClientManager.SetLocalUsername(funnyName);
            }

            nameInputField.onValueChanged.AddListener(OnInputFieldChanged);
        }


        public async void OnInputFieldChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                string funnyName = randomPlayerNames.GetRandomFunnyName();

                previewTextField.text = funnyName;
                ClientManager.SetLocalUsername(funnyName);

                FileManager.TryDeleteFile(PLAYERNAME_PATH);
                return;
            }

            ClientManager.SetLocalUsername(newValue);
            await FileManager.SaveInfoAsync(new ValueWrapper<string>(newValue), PLAYERNAME_PATH);
        }
    }
}