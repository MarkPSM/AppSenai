using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using ZXing;
using ZXing.Common;

[Serializable]
public class QRDetectedEvent : UnityEvent<string> { }

public class QRManager : MonoBehaviour
{
    [Header("Configurações da Câmera")]
    public int requestedWidth = 1280;
    public int requestedHeight = 720;
    public int requestedFPS = 30;

    [Header("Scanner")]
    public bool continuousScan = false;
    public float scanInterval = 0.25f;

    [Header("UI")]
    public TextMeshProUGUI logText;

    [Header("Eventos")]
    public QRDetectedEvent onQRDetected;

    private WebCamTexture camTexture;
    private BarcodeReader barcodeReader;
    private bool scanning = false;
    private bool isDecoding = false;
    private float nextScanTime = 0f;

    void Start()
    {
        barcodeReader = new BarcodeReader
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new[] { BarcodeFormat.QR_CODE }
            }
        };

        if (logText != null)
            onQRDetected.AddListener(UpdateResultText);

        StartCamera();
    }

    public void StartCamera()
    {
        if (scanning) return;

        if (WebCamTexture.devices.Length == 0)
        {
            LogOutput("Nenhuma câmera encontrada!");
            return;
        }

        string camName = WebCamTexture.devices[0].name;
        camTexture = new WebCamTexture(camName, requestedWidth, requestedHeight, requestedFPS);
        camTexture.Play();

        scanning = true;
        LogOutput($"Câmera iniciada: {camName} ({requestedWidth}x{requestedHeight}@{requestedFPS}fps)");
    }

    public void StopCamera()
    {
        scanning = false;
        if (camTexture != null && camTexture.isPlaying)
        {
            camTexture.Stop();
            Destroy(camTexture);
            camTexture = null;
        }
        LogOutput("Câmera parada.");
    }

    void Update()
    {
        if (!scanning || camTexture == null || camTexture.width < 100) return;

        if (!isDecoding && Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;
            DecodeAsync(camTexture.GetPixels32(), camTexture.width, camTexture.height);
        }
    }

    private async void DecodeAsync(Color32[] pixels, int width, int height)
    {
        isDecoding = true;

        var result = await Task.Run(() => barcodeReader.Decode(pixels, width, height));

        if (result != null)
        {
            LogOutput("QR Detectado: " + result.Text);
            onQRDetected?.Invoke(result.Text);

            if (!continuousScan) StopCamera();
        }

        isDecoding = false;
    }

    void OnDisable()
    {
        StopCamera();
    }

    private void UpdateResultText(string scannedText)
    {
        if (logText != null)
            logText.text = $"QR: {scannedText}";
    }

    private void LogOutput(string msg)
    {
        Debug.Log(msg);
        if (logText != null)
            logText.text = msg;
    }
}