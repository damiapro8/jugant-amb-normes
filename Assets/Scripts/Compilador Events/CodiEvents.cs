using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class CanviadorColor : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI textVisualColors;
    [SerializeField] private LlistaEvents llistaEvents;

    [SerializeField] private Color colorSimbols = Color.gray;
    [SerializeField] private Color colorTextDesconegut = Color.white;

    private Gramatica gramatica;

    private void Start()
    {
        llistaEvents = Resources.Load<LlistaEvents>("LlistaEvents");
        TerminalRule.ConfiguracioDades = llistaEvents;
        gramatica = new Gramatica();
        gramatica.DefinirGramatica();

        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(ProcessarText);
        }
        gameObject.SetActive(false);
    }

    public void CanviaText(string nouText)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        inputField.text = nouText;
    }

    private void ProcessarText(string textEntrat)
    {
        if (string.IsNullOrEmpty(textEntrat))
        {
            textVisualColors.text = "";
            return;
        }

        List<Token> tokens = ParsejadorSintaxi.Tokenitzar(textEntrat);

        TerminalRule.InicialitzarSessioParseig();
        TerminalRule.IndexSuggeriment = -1;
        gramatica.ReglaFinalDeLaCarta.Parse(tokens, 0);

        StringBuilder textColorejat = new StringBuilder();

        for (int i = 0; i < tokens.Count; i++)
        {
            Token token = tokens[i];
            string hexColor = "";

            if (token.tipus == TipusToken.Simbol)
            {
                hexColor = ColorUtility.ToHtmlStringRGB(colorSimbols);
            }
            else
            {
                if (TerminalRule.TipusParsejatsPerIndex.TryGetValue(i, out TipusToken tipusValidat))
                {
                    GrupEfecte grupTrobat = llistaEvents.grupEfectes.Find(g => 
                        g.grup.Equals(tipusValidat.ToString(), System.StringComparison.OrdinalIgnoreCase)
                    );

                    if (grupTrobat != null)
                        hexColor = ColorUtility.ToHtmlStringRGB(grupTrobat.color);
                    else
                        hexColor = ColorUtility.ToHtmlStringRGB(colorTextDesconegut);
                }
                else
                {
                    hexColor = ColorUtility.ToHtmlStringRGB(colorTextDesconegut);
                }
            }

            textColorejat.Append($"<color=#{hexColor}>{token.text}</color>");

            if (i < tokens.Count - 1)
            {
                Token seguentToken = tokens[i + 1];
                if (token.text == "(" || seguentToken.text == "(" || seguentToken.text == ")" || seguentToken.text == "." || seguentToken.text == ",")
                {
                }
                else
                {
                    textColorejat.Append(" ");
                }
            }
        }

        textVisualColors.text = textColorejat.ToString();
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(ProcessarText);
        }
    }
}