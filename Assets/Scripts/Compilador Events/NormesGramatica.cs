using System.Collections.Generic;
using UnityEngine;

public class TerminalRule : Rule
{
    private TipusToken type;
    private string valorExacte;
    public static LlistaEvents ConfiguracioDades;

    public static int IndexSuggeriment = -1;
    public static HashSet<string> SuggerimentsAcumulats = new HashSet<string>();

    public static Dictionary<int, Efecte> EfectesParsejatsPerIndex = new Dictionary<int, Efecte>();
    public static Dictionary<int, TipusToken> TipusParsejatsPerIndex = new Dictionary<int, TipusToken>();

    public static void NetejarEfectesDesDe(int desDeIndex)
    {
        List<int> clausAEliminar = new List<int>();
        foreach (int k in EfectesParsejatsPerIndex.Keys)
        {
            if (k >= desDeIndex) clausAEliminar.Add(k);
        }
        foreach (int k in clausAEliminar)
        {
            EfectesParsejatsPerIndex.Remove(k);
            TipusParsejatsPerIndex.Remove(k);
        }
    }

    public static void InicialitzarSessioParseig()
    {
        EfectesParsejatsPerIndex.Clear();
        TipusParsejatsPerIndex.Clear();
    }

    private string ObtenirGrupD_Efecte(Efecte e)
    {
        if (ConfiguracioDades == null || e == null) return "";
        foreach (var g in ConfiguracioDades.grupEfectes)
        {
            if (g.efectes != null)
            {
                if (g.efectes.Exists(x => x == e || (x != null && string.Equals(x.nomEfecte, e.nomEfecte, System.StringComparison.OrdinalIgnoreCase))))
                {
                    return g.grup;
                }
            }
        }
        return "";
    }

    public TerminalRule(TipusToken t, string v = null)
    {
        type = t;
        valorExacte = v;
    }

    // Mètode auxiliar per recollir quines paraules permetria aquesta regla concreta
    private void RecollirSuggerimentsValids(int index)
    {
        Efecte ultimEfecte = null;
        if (type == TipusToken.Selector)
        {
            if (index - 2 >= 0) EfectesParsejatsPerIndex.TryGetValue(index - 2, out ultimEfecte);
        }
        else
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (EfectesParsejatsPerIndex.TryGetValue(i, out Efecte eff) && eff != null)
                {
                    string grupPare = ObtenirGrupD_Efecte(eff);
                    if (type == TipusToken.Actuador && (grupPare == "Iniciador" || grupPare == "Condicional" || grupPare == "Operacio" || grupPare == "Accio")) { ultimEfecte = eff; break; }
                    else if (type == TipusToken.Accio && (grupPare == "Condicional" || grupPare == "Iniciador")) { ultimEfecte = eff; break; }
                    else if ((type == TipusToken.Operacio || type == TipusToken.Condicional) && (grupPare == "Iniciador" || grupPare == "Condicional")) { ultimEfecte = eff; break; }
                }
            }
        }

        if (type == TipusToken.Simbol)
        {
            if (!string.IsNullOrEmpty(valorExacte)) SuggerimentsAcumulats.Add(valorExacte);
        }
        else
        {
            if (ultimEfecte != null && ultimEfecte.efectesRelacionats != null && ultimEfecte.efectesRelacionats.Count > 0)
            {
                foreach (var e in ultimEfecte.efectesRelacionats)
                    if (e != null) SuggerimentsAcumulats.Add(e.nomEfecte);
            }
            else if (ConfiguracioDades != null)
            {
                string nomGrupEsperat = type.ToString();
                GrupEfecte grup = ConfiguracioDades.grupEfectes.Find(g => g.grup.Equals(nomGrupEsperat, System.StringComparison.OrdinalIgnoreCase));
                if (grup != null && grup.efectes != null)
                {
                    foreach (var e in grup.efectes)
                        if (e != null) SuggerimentsAcumulats.Add(e.nomEfecte);
                }
            }
        }
    }

    public override ParseResult Parse(List<Token> tokens, int index)
    {
        if (index == IndexSuggeriment)
        {
            RecollirSuggerimentsValids(index);
        }

        if (index >= tokens.Count) 
            return new ParseResult { Success = false, ErrorMessage = "Fi de l'arxiu inesperat." };

        Token t = tokens[index];
        
        if (type == TipusToken.Simbol)
        {
            if (t.tipus == TipusToken.Simbol && (valorExacte == null || t.text == valorExacte))
            {
                return new ParseResult { Success = true, NextIndex = index + 1 };
            }
            return new ParseResult { Success = false, ErrorMessage = $"S'esperava el símbol '{valorExacte}' i s'ha trobat '{t.text}'" };
        }

        if (t.tipus == TipusToken.Identificador)
        {
            if (ConfiguracioDades == null)
                return new ParseResult { Success = false, ErrorMessage = "Error del sistema: ScriptableObject no assignat a TerminalRule." };

            Efecte efecteTrobat = null;
            Efecte ultimEfecte = null;

            if (type == TipusToken.Selector)
            {
                if (index - 2 >= 0) { EfectesParsejatsPerIndex.TryGetValue(index - 2, out ultimEfecte); }
            }
            else
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (EfectesParsejatsPerIndex.TryGetValue(i, out Efecte eff) && eff != null)
                    {
                        string grupPare = ObtenirGrupD_Efecte(eff);
                        if (type == TipusToken.Actuador)
                        {
                            if (grupPare == "Iniciador" || grupPare == "Condicional" || grupPare == "Operacio" || grupPare == "Accio") { ultimEfecte = eff; break; }
                        }
                        else if (type == TipusToken.Accio)
                        {
                            if (grupPare == "Condicional" || grupPare == "Iniciador") { ultimEfecte = eff; break; }
                        }
                        else if (type == TipusToken.Operacio || type == TipusToken.Condicional)
                        {
                            if (grupPare == "Iniciador" || grupPare == "Condicional") { ultimEfecte = eff; break; }
                        }
                    }
                }
            }

            if (ultimEfecte != null && ultimEfecte.efectesRelacionats != null && ultimEfecte.efectesRelacionats.Count > 0)
            {
                efecteTrobat = ultimEfecte.efectesRelacionats.Find(e => e != null && string.Equals(e.nomEfecte, t.text, System.StringComparison.OrdinalIgnoreCase));
                if (efecteTrobat == null)
                {
                    return new ParseResult { Success = false, ErrorMessage = $"La paraula '{t.text}' no és un efecte relacionat permès per a '{ultimEfecte.nomEfecte}'." };
                }
            }
            else
            {
                string nomGrupEsperat = type.ToString();
                GrupEfecte grup = ConfiguracioDades.grupEfectes.Find(g => g.grup.Equals(nomGrupEsperat, System.StringComparison.OrdinalIgnoreCase));
                if (grup != null && grup.efectes != null)
                {
                    efecteTrobat = grup.efectes.Find(e => e != null && string.Equals(e.nomEfecte, t.text, System.StringComparison.OrdinalIgnoreCase));
                }
            }

            if (efecteTrobat != null)
            {
                EfectesParsejatsPerIndex[index] = efecteTrobat;
                TipusParsejatsPerIndex[index] = type;
                return new ParseResult { Success = true, NextIndex = index + 1 };
            }

            return new ParseResult { Success = false, ErrorMessage = $"La paraula '{t.text}' no és un {type} vàlid." };
        }

        return new ParseResult { Success = false, ErrorMessage = $"S'esperava un {type} i s'ha trobat un {t.tipus}." };
    }
}

public class SequenceRule : Rule
{
    private Rule[] subRules;
    public SequenceRule(Rule[] rules)
    {
        subRules = rules;
    }

    public override ParseResult Parse(List<Token> tokens, int index)
    {
        int indexActual = index;
        foreach (Rule r in subRules)
        {
            ParseResult result = r.Parse(tokens, indexActual);
            if (!result.Success) return result; 
            indexActual = result.NextIndex;     
        }
        return new ParseResult { Success = true, NextIndex = indexActual };
    }
}

public class ChoiceRule : Rule
{
    private Rule[] subRules;
    public ChoiceRule(Rule[] rules)
    {
        subRules = rules;
    }

    public override ParseResult Parse(List<Token> tokens, int index)
    {
        string errorsAcumulats = "";
        foreach (Rule r in subRules)
        {
            ParseResult result = r.Parse(tokens, index);
            if (result.Success) return result;
            
            TerminalRule.NetejarEfectesDesDe(index);
            
            errorsAcumulats += $" [{result.ErrorMessage}]";
        }
        return new ParseResult { Success = false, ErrorMessage = $"Cap camí de la tria és vàlid: {errorsAcumulats}" };
    }
}

public class OptionalRule : Rule
{
    private Rule subRule;
    public OptionalRule(Rule rule)
    {
        subRule = rule;
    }

    public override ParseResult Parse(List<Token> tokens, int index)
    {
        ParseResult result = subRule.Parse(tokens, index);
        if (result.Success) return result; 
        return new ParseResult { Success = true, NextIndex = index };
    }
}

public class ManyRule : Rule
{
    private Rule subRule;
    public ManyRule(Rule rule)
    {
        subRule = rule;
    }

    public override ParseResult Parse(List<Token> tokens, int index)
    {
        int indexActual = index;
        while (indexActual < tokens.Count)
        {
            ParseResult result = subRule.Parse(tokens, indexActual);
            if (!result.Success) 
            {
                TerminalRule.NetejarEfectesDesDe(indexActual);
                break; 
            }
            
            indexActual = result.NextIndex; 
        }
        
        return new ParseResult { Success = true, NextIndex = indexActual };
    }
}