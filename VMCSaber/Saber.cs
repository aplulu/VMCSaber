using UnityEngine;

public class Saber
{
    private AssetBundle _assetBundle;

    public string AssetBundleFile { get; }

    public GameObject SaberGO { get; }

    public Saber(string assetBundleFile)
    {
        AssetBundleFile = assetBundleFile;
        _assetBundle = AssetBundle.LoadFromFile(assetBundleFile);
        SaberGO = _assetBundle.LoadAsset<GameObject>("_CustomSaber");
    }

    public void Dispose()
    {
        if (_assetBundle != null)
        {
            _assetBundle.Unload(true);
        }
    }
}