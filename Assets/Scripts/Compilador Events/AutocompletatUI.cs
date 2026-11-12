using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class AutocompletatUI : MonoBehaviour
{
    [Header("Components de connexió")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private ParsejadorEvent parsejadorEvent;
    [SerializeField] private TextMeshProUGUI textSuggerimentsInterficie; // Etiqueta opcional a sota del input per veure les opcions

    private ParsejadorSintaxi parserIntern;
    private List<string> suggerimentsDisponibles = new List<string>();
    private string prefixActual = "";
    private int indexSuggerimentActual = 0;

    private void Start()
    {
        if (parsejadorEvent == null || inputField == null)
        {
            Debug.LogError("Falten assignacions a l'AutocompletatUI.");
            return;
        }

        // Inicialitzem un analitzador sintàctic en segon pla per als suggeriments
        parserIntern = new ParsejadorSintaxi();
        parserIntern.Inicialitza(parsejadorEvent.configuracioTokens);

        inputField.onValueChanged.AddListener(OnTextModificat);
    }

    private void OnTextModificat(string text)
    {
        // Cada vegada que l'usuari tecleja, actualitzem la llista en temps real
        suggerimentsDisponibles = parserIntern.ObtenirSuggerimentsContextuals(text, out prefixActual);
        indexSuggerimentActual = 0; 

        DibuixarSuggerimentsUI();
    }

    private void DibuixarSuggerimentsUI()
    {
        if (textSuggerimentsInterficie == null) return;

        if (suggerimentsDisponibles.Count == 0)
        {
            textSuggerimentsInterficie.text = "";
            return;
        }

        StringBuilder sb = new StringBuilder("");
        for (int i = 0; i < suggerimentsDisponibles.Count; i++)
        {
            if (i == indexSuggerimentActual)
                sb.Append($"<color=#00ffffff><b>{suggerimentsDisponibles[i]}</b></color>\n");
            else
                sb.Append($"{suggerimentsDisponibles[i]}\n");
        }

        textSuggerimentsInterficie.text = sb.ToString();
    }

    public void ExecutarAutocompletat()
    {
        if (!inputField.isFocused) return;
        if (suggerimentsDisponibles.Count == 0) return;

        string paraulaTriada = suggerimentsDisponibles[indexSuggerimentActual];
        string textOriginal = inputField.text;
        string nouText = "";

        if (string.IsNullOrEmpty(prefixActual))
        {
            // Càs A: L'usuari ha posat un espai, simplement concatenem la paraula suggerida
            nouText = textOriginal + paraulaTriada;
        }
        else
        {
            // Càs B: L'usuari ha començat a escriure, reemplacem el tros del prefix per la paraula completa
            int ultimIndexPrefix = textOriginal.LastIndexOf(prefixActual, System.StringComparison.OrdinalIgnoreCase);
            if (ultimIndexPrefix >= 0)
            {
                nouText = textOriginal.Substring(0, ultimIndexPrefix) + paraulaTriada;
            }
        }
        nouText += " ";
        // Desvinculem el Listener un moment per evitar que es torni a avaluar a mig procés
        inputField.onValueChanged.RemoveListener(OnTextModificat);
        
        inputField.text = nouText;
        inputField.caretPosition = inputField.text.Length; // Forcem el cursor al final
        inputField.ActivateInputField(); // Manté el focus obert al camp de text
        
        inputField.onValueChanged.AddListener(OnTextModificat);

        // Si hi ha múltiples suggeriments, prémer TAB repetidament anirà alternant cíclicament entre les opcions
        OnTextModificat(nouText);
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnTextModificat);
        }
    }
}