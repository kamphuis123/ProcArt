using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FishSpawnerUIController : MonoBehaviour
{
    [Header("Slider References")]
    [SerializeField] private Slider globalScaleSlider;
    [SerializeField] private Slider headScaleSlider;
    [SerializeField] private Slider bodyScaleSlider;
    [SerializeField] private Slider jointCountSlider;
    [SerializeField] private Slider neckLengthSlider;
    [SerializeField] private Slider movementSpeedSlider;

    [Header("Value Displays (Optional)")]
    [SerializeField] private Text globalScaleValueText;
    [SerializeField] private Text headScaleValueText;
    [SerializeField] private Text bodyScaleValueText;
    [SerializeField] private Text jointCountValueText;
    [SerializeField] private Text neckLengthValueText;
    [SerializeField] private Text speedValueText;

    private fishSpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<fishSpawner>();
        InitializeSliders();
    }

    void InitializeSliders()
    {
        // Global Scale (0.1 to 10)
        SetupSlider(globalScaleSlider, 0.1f, 10f, spawner.globalScale, v => {
            spawner.globalScale = v;
            if (globalScaleValueText) globalScaleValueText.text = v.ToString("0.0");
        });

        // Head Scale (0.1 to 10)
        SetupSlider(headScaleSlider, 0.1f, 10f, spawner.headScale, v => {
            spawner.headScale = v;
            if (headScaleValueText) headScaleValueText.text = v.ToString("0.0");
        });

        // Body Scale (0.1 to 10)
        SetupSlider(bodyScaleSlider, 0.1f, 10f, spawner.bodyScale, v => {
            spawner.bodyScale = v;
            if (bodyScaleValueText) bodyScaleValueText.text = v.ToString("0.0");
        });

        // Joint Count (0 to 100, integers only)
        SetupSlider(jointCountSlider, 0, 150, spawner.jointcount, v => {
            spawner.jointcount = (int)v;
            if (jointCountValueText) jointCountValueText.text = ((int)v).ToString();
        }, true);

        // Neck Length (0 to 30, integers only)
        SetupSlider(neckLengthSlider, 0, 100, spawner.neckLength, v => {
            spawner.neckLength = (int)v;
            if (neckLengthValueText) neckLengthValueText.text = ((int)v).ToString();
        }, true);

        // Movement Speed (0 to 10)
        SetupSlider(movementSpeedSlider, 0, 10, spawner.movementSpeed, v => {
            spawner.movementSpeed = v;
            if (speedValueText) speedValueText.text = v.ToString("0.0");
        });
    }

    void SetupSlider(Slider slider, float min, float max, float initial, UnityAction<float> callback, bool wholeNumbers = false)
    {
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.value = initial;
        slider.onValueChanged.AddListener(callback);

        // Force UI update for initial values
        callback.Invoke(initial);
    }

    // Optional: Toggle UI with Tab key
    [SerializeField] private GameObject controlsPanel;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            controlsPanel.SetActive(!controlsPanel.activeSelf);
        }
    }
}