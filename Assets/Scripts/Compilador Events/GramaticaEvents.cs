using System;
using System.Collections.Generic;

public class ParseResult
{
    public bool Success { get; set; }
    public int NextIndex { get; set; }
    public string ErrorMessage { get; set; }
    public object Data;
}

public abstract class Rule
{
    public abstract ParseResult Parse(List<Token> tokens, int index);

    // Seqüència
    public static Rule Sequence(params Rule[] rules) => new SequenceRule(rules);

    // Elecció (|)
    public static Rule Choice(params Rule[] rules) => new ChoiceRule(rules);

    // Opcional ([])
    public static Rule Optional(Rule rule) => new OptionalRule(rule);

    // Zero o més ({})
    public static Rule Many(Rule rule) => new ManyRule(rule);

    // Terminal: Coincideix amb un tipus de token concret
    public static Rule Terminal(TipusToken type, string valorExacte = null) => new TerminalRule(type, valorExacte);
}

public class Gramatica
{
    public Rule ReglaFinalDeLaCarta;

    public void DefinirGramatica()
    {
        Rule iniciador = Rule.Terminal(TipusToken.Iniciador);
        Rule actuador  = Rule.Terminal(TipusToken.Actuador);
        Rule condicional = Rule.Terminal(TipusToken.Condicional);
        Rule accio      = Rule.Terminal(TipusToken.Accio);
        Rule selector   = Rule.Terminal(TipusToken.Selector);
        Rule operacio   = Rule.Terminal(TipusToken.Operacio);
        
        Rule oberturaParentesi = Rule.Terminal(TipusToken.Simbol, "(");
        Rule tancamentParentesi = Rule.Terminal(TipusToken.Simbol, ")");
        Rule punt = Rule.Terminal(TipusToken.Simbol, ".");
        Rule coma = Rule.Terminal(TipusToken.Simbol, ",");

        Rule opcionalOperacio = Rule.Optional(Rule.Sequence(operacio, actuador));

        Rule blocCondicional = Rule.Sequence(condicional, actuador, opcionalOperacio);

        // 5. actuador "." selector {"," actuador "." selector}
        Rule argumentAccio = Rule.Sequence(actuador, punt, selector);
        Rule llistaArgumentsAccio = Rule.Sequence(argumentAccio, Rule.Many(Rule.Sequence(coma, argumentAccio)));

        // 6. acció actuador "." selector ...
        Rule blocAccio = Rule.Sequence(accio, llistaArgumentsAccio);

        Rule condicionaliAccio = Rule.Sequence(blocCondicional, blocAccio);
        Rule bucleEfectes = Rule.Sequence(condicionaliAccio, Rule.Many(condicionaliAccio));

        ReglaFinalDeLaCarta = Rule.Sequence(
            iniciador, blocAccio
        );
    }
}