using System;
using System.Collections;
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
    public int requestedFPS = 40;

    [Header("Scanner")]
    public float scanInterval = 0.25f;
    public bool continuousScan = false;

    [Header("Eventos")]
    public QRDetectedEvent onQRDetected;

    private WebCamTexture camTexture;
    private IBarcodeReader barcodeReader;
    private bool scanning = false;
    private bool isProcessing = false;

    private void Awake()
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
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(StartCamera());
    }

    /// <summary>
    /// Inicia a câmera e o loop de leitura
    /// </summary>
    public IEnumerator StartCamera()
    {
        if (scanning) yield break;

#if UNITY_ANDROID || UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            var req = Application.RequestUserAuthorization(UserAuthorization.WebCam);
            yield return req;
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogWarning("Permissão de câmera não concedida.");
                yield break;
            }
        }
#endif

        string camName = null;
        foreach (var device in WebCamTexture.devices)
        {
            if (!device.isFrontFacing)
            {
                camName = device.name;
                break;
            }
        }
        if (camName == null && WebCamTexture.devices.Length > 0)
            camName = WebCamTexture.devices[0].name;

        if (string.IsNullOrEmpty(camName))
        {
            Debug.LogError("Nenhuma câmera encontrada!");
            yield break;
        }

        camTexture = new WebCamTexture(camName, requestedWidth, requestedHeight, requestedFPS);
        camTexture.Play();

        scanning = true;
        StartCoroutine(ScanLoop());
    }

    /// <summary>
    /// Para a câmera
    /// </summary>
    public void StopCamera()
    {
        scanning = false;
        if (camTexture != null && camTexture.isPlaying)
        {
            camTexture.Stop();
            Destroy(camTexture);
            camTexture = null;
        }
    }

    /// <summary>
    /// Loop que chama o scanner a cada scanInterval
    /// </summary>
    private IEnumerator ScanLoop()
    {
        while (scanning)
        {
            if (!isProcessing && camTexture != null && camTexture.isPlaying && camTexture.width > 100)
            {
                StartCoroutine(ProcessFrame());
            }
            yield return new WaitForSeconds(scanInterval);
        }
    }

    /// <summary>
    /// Processa um frame da câmera
    /// </summary>
    private IEnumerator ProcessFrame()
    {
        isProcessing = true;

        try
        {
            var pixels = camTexture.GetPixels32();
            int width = camTexture.width;
            int height = camTexture.height;

            var result = barcodeReader.Decode(pixels, width, height);

            if (result != null)
            {
                Debug.Log("QR Detectado: " + result.Text);
                onQRDetected?.Invoke(result.Text);

                if (!continuousScan)
                {
                    StopCamera();
                    yield break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Erro ao processar QR: " + ex.Message);
        }
        finally
        {
            isProcessing = false;
        }
    }

    private void OnDisable()
    {
        StopCamera();
    }
}
