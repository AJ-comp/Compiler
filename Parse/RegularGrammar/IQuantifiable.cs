namespace Parse.FrontEnd.RegularGrammar
{
    public interface IQuantifiable
    {
        /// <summary>Convert to the NonTerminal type that included the * information.</summary>
        /// <returns>Converted NonTerminal</returns>
        /// <see cref="https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/0"/>
        /// <seealso cref="https://www.lucidchart.com/documents/edit/62030320-0871-4549-95cc-980cff7ab9cf/4"/>
        /// <seealso cref="https://www.lucidchart.com/documents/edit/62030320-0871-4549-95cc-980cff7ab9cf/2"/>
        NonTerminal ZeroOrMore();

        /// <summary>Convert to the NonTerminal type that included + information.</summary>
        /// <returns>Converted NonTerminal</returns>
        /// <see cref="https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/1"/>
        /// <see cref="https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/1"/>
        NonTerminal OneOrMore();

        /// <summary> Convert to the NonTerminal type that included ? information. </summary>
        /// <returns>Converted NonTerminal</returns>
        /// <see cref="https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/2"/>
        /// <see cref="https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/2"/>
        NonTerminal Optional();
    }
}
