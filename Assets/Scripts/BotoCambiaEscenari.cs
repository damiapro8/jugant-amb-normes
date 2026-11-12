using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CanviadorEscenes : MonoBehaviour
{
    public AudioSource audiosource;
    public AudioClip efectesonor;
    public void CarregarEscena(string nomEscena)
    {
        if (audiosource != null && efectesonor != null)
        {
            StartCoroutine(EsperarFiSoroll(nomEscena));
        }
        else
        {
            SceneManager.LoadScene(nomEscena);
        }
    }
    public void SortirDelJoc()
    {
        if (audiosource != null && efectesonor != null)
        {
            StartCoroutine(EsperarFiSoroll(null));
        }
        else
        {
            TancarJoc();
        }
    }

    private IEnumerator EsperarFiSoroll(string nomEscena)
    {
        audiosource.PlayOneShot(efectesonor);
        
        yield return new WaitForSeconds(efectesonor.length);
        if (nomEscena != null)
        {
        SceneManager.LoadScene(nomEscena);
        }
        else
        {
            TancarJoc();
        }
    }

    private void TancarJoc()
    {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    }