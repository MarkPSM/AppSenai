using UnityEngine;
using UnityEngine.UI;

public class FrontManager : MonoBehaviour
{
    public int sceneNum;

    public GameObject canvasPatrimonio;

    public GameObject inputName;

    public GameObject inputPatrimonio;

    void Start()
    {
        canvasPatrimonio.SetActive(false);
    }

    void Update()
    {
        
    }

    public void SceneLoader()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneNum);
    }

    public void CallPopUp()
    {
        canvasPatrimonio.SetActive(true);
    }

    public void ClosePopUp()
    {
        canvasPatrimonio.SetActive(false);
    }

    public void Enviar()
    {
        InputField inputFieldName = inputName.GetComponent<InputField>();
        InputField inputFieldPatrimonio = inputPatrimonio.GetComponent<InputField>();

        string NomePatrimonio = inputFieldName.text.ToString();

        int NumeroPatrimonio = int.Parse(inputFieldPatrimonio.text.ToString());

        Debug.Log("Nome: " + NomePatrimonio + "; Patrimônio: " + NumeroPatrimonio);

        canvasPatrimonio.SetActive(false);
    }

}
