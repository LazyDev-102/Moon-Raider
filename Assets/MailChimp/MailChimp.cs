using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net.Mail;

public enum MailChimpMode
{
    Add = 0,
    Check = 1
}

public class MailChimp : MonoBehaviour
{
    // https://us5.api.mailchimp.com/3.0/lists/211ec96a2a/members

    private const string UrlFormat = "https://{0}.api.mailchimp.com/3.0/lists/{1}/members";
    private const string DataFormat = "{{\"email_address\":\"{0}\", \"status\":\"subscribed\"}}";

    public GameObject Warning;

    [SerializeField]
    private string _apiKey;
    [SerializeField]
    private string _listId;

    [SerializeField]
    private UnityEvent _subscribeSuccess;
    [SerializeField]
    private UnityEvent _subscribeError;


    public void Subscribe()
    {
        var text = GetComponentInChildren<InputField>();

        if (text == null)
        {
            Debug.LogError("MailChimp — No UI Text found at this GameObject");
            return;
        }
        else
        {
            Subscribe(text.text);
        }
    }

    public void Subscribe(string email, MailChimpMode mode = MailChimpMode.Add)
    {
        string lowerEmail = email.ToLower();
        if (IsValidEmail(lowerEmail))
        {
            // Need to call as GET first, then on fail try POST
            var www = BuildWWW(lowerEmail, mode);

            if (www != null)
            {
                StartCoroutine(SendToMailChimp(www, mode));
            }
            else
            {
                _subscribeError.Invoke();
                StartCoroutine(Warn(2f));
            }
        }
        else
        {
            Debug.Log("MailChimp — Invalid email");
            _subscribeError.Invoke();

            StartCoroutine(Warn(2f));
        }
    }

    protected virtual IEnumerator Warn(float duration)
    {
        Warning.SetActive(true);

        yield return new WaitForSeconds(duration);

        Warning.SetActive(false);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var mailAdress = new MailAddress(email);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private IEnumerator SendToMailChimp(WWW www, MailChimpMode mode)
    {
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("MailChimp — Subscribe success");
            _subscribeSuccess.Invoke();
        }
        else
        {
            Debug.Log("MailChimp — Subscribe error: " + www.error);

            if (mode == MailChimpMode.Add)
            {
                var email = GetComponentInChildren<InputField>().text;
                Subscribe(email, MailChimpMode.Check);
            }
            else
            {
                _subscribeError.Invoke();
                StartCoroutine(Warn(2f));
            }
        }
    }

    private WWW BuildWWW(string email, MailChimpMode mode)
    {
        var headers = new Dictionary<string, string>();
        headers.Add("Authorization", "apikey " + _apiKey);

        var splittedApiKey = _apiKey.Split('-');

        if (splittedApiKey.Length != 2)
        {
            Debug.LogError("MailChimp — Invalid API Key format");
            return null;
        }

        var urlPrefix = splittedApiKey[1];

        // Need to do a GET on check, POST on create
        if (mode == MailChimpMode.Add)
        {
            var url = string.Format(UrlFormat, urlPrefix, _listId);

            Debug.Log("Add URL is " + url);

            var data = string.Format(DataFormat, email);
            var dataBytes = Encoding.ASCII.GetBytes(data);

            var www = new WWW(url, dataBytes, headers);
            return www;
        }
        else
        {
            var url = string.Format(UrlFormat, urlPrefix, _listId) + "/" + Md5Sum(email);

            Debug.Log("Check URL is " + url);

            var www = new WWW(url, null, headers);
            return www;
        }

    }


    public string Md5Sum(string strToEncrypt)
    {
        UTF8Encoding ue = new UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}
