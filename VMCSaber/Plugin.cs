using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using VMC;
using VMCMod;

[VMCPlugin(
    Name: "VMCSaber",
    Version: "0.1.0",
    Author: "Aplulu",
    Description: "Draw Saber on Virtual Motion Capture",
    AuthorURL: "https://aplulu.me"
)]
public class VMCSaber: MonoBehaviour
{
    public Saber CurrentSaber { get; private set; }
    public Transform LeftHandTransform { get; private set; }
    public Transform RightHandTransform { get; private set; }
    
    public static VMCSaber Instance { get; private set; }

    private GameObject _currentModel = null;

    public void Start()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        VMCEvents.OnModelLoaded += OnModelLoaded;
        VMCEvents.OnModelUnloading += OnModelUnloading;
    }

    private void OnDisable()
    {
        VMCEvents.OnModelLoaded -= OnModelLoaded;
        VMCEvents.OnModelUnloading -= OnModelUnloading;
    }


    private void OnModelLoaded(GameObject model)
    {
        if (model == null)
        {
            return;
        }

        _currentModel = model;

        LeftHandTransform = model.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftHand);
        RightHandTransform = model.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        
        RegisterOscEvents();
    }

    private void OnModelUnloading(GameObject model)
    {
        _currentModel = null;
    }

    private void RegisterOscEvents()
    {
        foreach (var server in FindObjectsOfType<uOSC.uOscServer>())
        {
            server.onDataReceived.RemoveListener(OnDataReceived);
            server.onDataReceived.AddListener(OnDataReceived);
        }
    }

    private void OnDataReceived(uOSC.Message message)
    {
        switch (message.address)
        {
            case "/VMCSaber/Saber/On":
                if (LeftHandTransform != null && RightHandTransform != null)
                {
                    SaberLoader.Initialize();
                }
                break;
            case "/VMCSaber/Saber/Off":
                SaberLoader.Instance?.Dispose();
                break;
            case "/VMCSaber/Saber/Path":
                if (message.values != null && message.values.Length > 0 && message.values[0] is string)
                {
                    var path = (string)message.values[0];
                    if (CurrentSaber == null || CurrentSaber?.AssetBundleFile != path)
                    {
                        if (CurrentSaber != null)
                        {
                            CurrentSaber.Dispose();
                            CurrentSaber = null;
                        }

                        CurrentSaber = new Saber(path);
                        if (LeftHandTransform != null && RightHandTransform != null)
                        {
                            SaberLoader.Initialize();
                        }
                    }
                    else
                    {
                        if (LeftHandTransform != null && RightHandTransform != null)
                        {
                            SaberLoader.Initialize();
                        }
                    }
                }
                break;
            case "/VMCSaber/Saber/Color/Left":
            case "/VMCSaber/Saber/Color/Right":
                if (message.values != null && 
                    message.values.Length == 4 &&
                    message.values[0] is float &&
                    message.values[1] is float &&
                    message.values[2] is float &&
                    message.values[3] is float)
                {
                    var color = new Color((float)message.values[0], (float)message.values[1], (float)message.values[2], (float)message.values[3]);
                    SaberLoader.Instance?.SetSaberColor(message.address == "/VMCSaber/Saber/Color/Left" ? SaberType.Left : SaberType.Right, color);
                }
                break;
            case "/VMCSaber/Saber/Scale":
                if (message.values != null &&
                    message.values.Length == 1 &&
                    message.values[0] is float &&
                    SaberLoader.Instance != null)
                {
                    SaberLoader.Instance.Scale = (float)message.values[0];
                }
                break;
            case "/VMCSaber/Controller/Rot/Left":
            case "/VMCSaber/Controller/Rot/Right":
                if (message.values != null &&
                    message.values.Length == 3 &&
                    message.values[0] is float &&
                    message.values[1] is float &&
                    message.values[2] is float &&
                    SaberLoader.Instance != null)
                {
                    var rot = new Vector3((float)message.values[0], (float)message.values[1], (float)message.values[2]);
                    if (message.address == "/VMCSaber/Controller/Rot/Left")
                    {
                        SaberLoader.Instance.LeftControllerRot = rot;
                    }
                    else
                    {
                        SaberLoader.Instance.RightControllerRot = rot;
                    }
                }
                break;
            case "/VMCSaber/Controller/Pos/Left":
            case "/VMCSaber/Controller/Pos/Right":
                if (message.values != null &&
                    message.values.Length == 3 &&
                    message.values[0] is float &&
                    message.values[1] is float &&
                    message.values[2] is float &&
                    SaberLoader.Instance != null)
                {
                    var pos = new Vector3((float)message.values[0], (float)message.values[1], (float)message.values[2]);
                    Log($"Pos address={message.address}, pos={pos}");
                    if (message.address == "/VMCSaber/Controller/Pos/Left")
                    {
                        SaberLoader.Instance.LeftControllerPos = pos;
                    }
                    else
                    {
                        SaberLoader.Instance.RightControllerPos = pos;
                    }
                }
                break;
        }
    }

    public static void Log(string message)
    {
        File.AppendAllText(Path.Combine(Application.dataPath, "..\\VMCSaber.txt"), $"[{DateTime.Now:hh:mm:ss}] {message}\n");
    }
}