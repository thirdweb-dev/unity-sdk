using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectSharp.Unity.Models;

public class ResultScreen : MonoBehaviour
{
    public Text resultText;
    
    public void OnMessageSigned(WCMessageSigned messageSigned)
    {
        resultText.text = "Signed Message: " + messageSigned.SignedMessage + "\nRecovered Signer Address: Not Implemented";
    }
}
