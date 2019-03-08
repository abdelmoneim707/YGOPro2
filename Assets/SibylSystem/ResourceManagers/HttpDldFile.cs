using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using UnityEngine;

public class HttpDldFile
{
    public bool Download(string url, string filename)
    {
        bool flag = false;
        try
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }

            using (var client = new TimeoutWebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

                //authorization needed to acces github
                if (Path.GetExtension(filename).Contains("png"))
                {
                    client.Headers.Add(HttpRequestHeader.Authorization, string.Concat("token ", RepoData.GetToken()));
                    client.Timeout = 10000;
                }
                client.DownloadFile(new Uri(url), filename + ".tmp");
            }
            flag = true;
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            File.Move(filename + ".tmp", filename);
        }
        catch (Exception)
        {
            flag = false;
        }
        return flag;
    }
    public static bool MyRemoteCertificateValidationCallback(System.Object sender,
    X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    continue;
                }
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }
}
//Added to let the picture download timeout after 5 seconds instead of a minute.
public class TimeoutWebClient : WebClient
{
    public int Timeout { get; set; }

    public TimeoutWebClient()
    {
        Timeout = 2500;
    }

    public TimeoutWebClient(int timeout)
    {
        Timeout = timeout;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest request = base.GetWebRequest(address);
        request.Timeout = Timeout;
        return request;
    }
}