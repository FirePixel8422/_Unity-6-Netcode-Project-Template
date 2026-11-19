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


        private void Awake()
        {
            if (SaveData.TryGet(PLAYERNAME_PATH, out string savedPlayerName))
            {
                nameInputField.text = savedPlayerName;
                ClientManager.SetLocalUsername(savedPlayerName);
            }
            else
            {
                string funnyName = randomPlayerNames.GetRandomFunnyName();

                previewTextField.text = funnyName;
                ClientManager.SetLocalUsername(funnyName);
            }

            nameInputField.onValueChanged.AddListener(OnInputFieldChanged);
        }

        public void OnInputFieldChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                string funnyName = randomPlayerNames.GetRandomFunnyName();

                previewTextField.text = funnyName;
                ClientManager.SetLocalUsername(funnyName);

                SaveData.Delete(PLAYERNAME_PATH);
                return;
            }

            ClientManager.SetLocalUsername(newValue);
            SaveData.Set(PLAYERNAME_PATH, newValue);
        }
    }
}