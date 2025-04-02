using UnityEngine;
using PlayFab;
using PlayFab.Multiplayer;
using PlayFab.ClientModels;
using System;
using Sirenix.OdinInspector;
using TMPro;

public class RegisterSystem : MonoBehaviour
{
    public TMP_InputField userEmail, userPassword, userName;
    public TextMeshProUGUI infoLabel;
  
    public GameObject loginPanel, registerPanel;

    [Button]
    public void RegisterUser()
    {
        if (userPassword.text.Length<6)
        {
           infoLabel.text=("Password is too short");
            return;
        }
        if (userName.text.Length <= 0)
        {
            infoLabel.text = ("No username");
            return;
        }
        if (string.IsNullOrEmpty(userEmail.text)) 
        {
            infoLabel.text = ("Mail is not valid");
            return;
        }
        var Request= new RegisterPlayFabUserRequest {  DisplayName= userName.text, Email = userEmail.text, Password = userPassword.text, RequireBothUsernameAndEmail = false };

        PlayFabClientAPI.RegisterPlayFabUser(Request, OnRegisterSuccess, OnError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        infoLabel.text = ("New User Registered");
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log("ERROR REGISTERING NEW USER" + error);
    }
}
