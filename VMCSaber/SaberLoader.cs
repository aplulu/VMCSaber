using UnityEngine;

public class SaberLoader: MonoBehaviour
{
    public static SaberLoader Instance { private set; get; }

    private GameObject _saberContainer;
    private GameObject _leftSaber;
    private GameObject _rightSaber;
    private Color _leftColor = Color.red;
    private Color _rightColor = Color.blue;
    private float _scale = 1.0f;
    private Vector3 _leftControllerRot = Vector3.zero;
    private Vector3 _rightControllerRot = Vector3.zero;
    private Vector3 _leftControllerPos = Vector3.zero;
    private Vector3 _rightControllerPos = Vector3.zero;
    private bool _experimentalTrail = false;

    public static void Initialize()
    {
        if (Instance != null)
        {
            Instance.Dispose();
            Instance = null;
        }
        var loader = new GameObject("SaberLoader");
        Instance = loader.AddComponent<SaberLoader>();
    }
    
    public void Dispose()
    {
        Destroy(_leftSaber);
        Destroy(_rightSaber);
        Destroy(_saberContainer);
        Destroy(this);
    }

    private void Awake()
    {
        if (_saberContainer != null)
        {
            Destroy(_saberContainer);
            _saberContainer = null;
        }
        _saberContainer = Instantiate(VMCSaber.Instance.CurrentSaber.SaberGO);
        if (_saberContainer != null)
        {
            _leftSaber = _saberContainer.transform.Find("LeftSaber").gameObject;
            _rightSaber = _saberContainer.transform.Find("RightSaber").gameObject;
            
            _leftSaber.transform.parent = VMCSaber.Instance.LeftHandTransform;
            _leftSaber.transform.localPosition = new Vector3(-0.08f, -0.02f, 0.15f);
            ApplySaberColor(SaberType.Left);
            ApplySaberPosition(SaberType.Left);

            _rightSaber.transform.parent = VMCSaber.Instance.RightHandTransform;
            _rightSaber.transform.localPosition = new Vector3(0.08f, -0.02f, 0.15f);
            ApplySaberColor(SaberType.Right);
            ApplySaberPosition(SaberType.Right);
        }
    }

    public void SetSaberColor(SaberType saberType, Color color)
    {
        if (saberType == SaberType.Left)
        {
            _leftColor = color;
        }
        else
        {
            _rightColor = color;
        }
        ApplySaberColor(saberType);
    }

    private void ApplySaberColor(SaberType saberType)
    {
        var saber = saberType == SaberType.Left ? _leftSaber : _rightSaber;
        if (saber == null)
        {
            return;
        }

        var color = saberType == SaberType.Left ? _leftColor : _rightColor;

        foreach (var renderer in saber.GetComponentsInChildren<Renderer>())
        {
            if (renderer is not MeshRenderer && renderer is not SkinnedMeshRenderer)
            {
                continue;
            }
            
            var hasColor = false;
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null || !material.HasProperty("_Color"))
                {
                    continue;
                }

                if (material.HasProperty("_CustomColors") && material.GetFloat("_CustomColors") > 0)
                {
                    material.SetColor("_Color", color);
                    hasColor = true;
                } else if ((material.HasProperty("_Glow") && material.GetFloat("_Glow") > 0) || (material.HasProperty("_Bloom") && material.GetFloat("_Bloom") > 0))
                {
                    material.SetColor("_Color", color);
                    hasColor = true;
                }
            }
            
            if (hasColor)
            {
                var trailTrn = renderer.gameObject.transform.Find("_trail");
                if (_experimentalTrail)
                {
                    TrailRenderer trailRenderer = null;
                    GameObject trail;
                    if (trailTrn == null)
                    {
                        trail = new GameObject();
                        trail.name = "_trail";
                        trail.transform.localPosition = new Vector3(0, 0, 0);
                        trail.transform.parent = renderer.gameObject.transform;
                    }
                    else
                    {
                        trail = trailTrn.gameObject;
                        trailRenderer = trailTrn.GetComponent<TrailRenderer>();
                    }
                    
                    if (trailRenderer == null)
                    {
                        trailRenderer = trail.AddComponent<TrailRenderer>();
                    }

                    trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    trailRenderer.startColor = new Color(color.r, color.g, color.b, 0.3f);
                    trailRenderer.endColor = new Color(color.r, color.g, color.b, 0.0f);
                    trailRenderer.time = 0.2f;
                    trailRenderer.alignment = LineAlignment.View;
                    trailRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Object;
                    trailRenderer.minVertexDistance = 0.01f;
                }
                else if (trailTrn != null)
                {
                    GameObject.Destroy(trailTrn.gameObject);
                }
            }
        }
    }
    
    private void ApplySaberPosition(SaberType saberType)
    {
        var saber = saberType == SaberType.Left ? _leftSaber : _rightSaber;
        if (saber == null)
        {
            return;
        }

        saber.transform.localRotation = Quaternion.Euler(0, 0, saberType == SaberType.Left ? -90 : 90);
        saber.transform.Translate(saberType == SaberType.Left ? _leftControllerPos : _rightControllerPos);
        saber.transform.Rotate(saberType == SaberType.Left ? _leftControllerRot : _rightControllerRot);
        saber.transform.localScale = new Vector3(_scale, _scale, _scale);
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            ApplySaberPosition(SaberType.Left);
            ApplySaberPosition(SaberType.Right);
        }
    }

    public Vector3 LeftControllerRot
    {
        get => _leftControllerRot;
        set
        {
            _leftControllerRot = value;
            ApplySaberPosition(SaberType.Left);
        }
    }
    
    public Vector3 RightControllerRot
    {
        get => _rightControllerRot;
        set
        {
            _rightControllerRot = value;
            ApplySaberPosition(SaberType.Right);
        }
    }
    
    public Vector3 LeftControllerPos
    {
        get => _leftControllerPos;
        set
        {
            _leftControllerPos = value;
            ApplySaberPosition(SaberType.Left);
        }
    }
    
    public Vector3 RightControllerPos
    {
        get => _rightControllerPos;
        set
        {
            _rightControllerPos = value;
            ApplySaberPosition(SaberType.Right);
        }
    }
}