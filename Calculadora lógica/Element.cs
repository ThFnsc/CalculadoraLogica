using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculadora_lógica {
    class Element {
        public ElementTypes Type { get; set; }
        public char Variable { get; set; }
        public Operations Operation { get; set; }
        public bool Value { get; set; }

        public Element(ElementTypes Type) {
            this.Type = Type;
        }

        public Element(Element element) {
            this.Type = element.Type;
            this.Variable = element.Variable;
            this.Operation = element.Operation;
            this.Value = element.Value;
        }

        internal static Element Parse(char character) {
            if (Char.IsLetter(character))
                return new Element(ElementTypes.Variable) { Variable = Char.ToUpper(character) };
            switch (character) {
                case '(':
                    return new Element(ElementTypes.OpeningParenthesis);
                case ')':
                    return new Element(ElementTypes.ClosingParenthesis);
                case '.':
                    return new Element(ElementTypes.Operation) { Operation = Operations.AND };
                case '+':
                    return new Element(ElementTypes.Operation) { Operation = Operations.OR };
                case '*':
                    return new Element(ElementTypes.Operation) { Operation = Operations.XOR };
                case '~':
                    return new Element(ElementTypes.Operation) { Operation = Operations.NOT };
                case '0':
                case '1':
                    return new Element(ElementTypes.Constant) { Value = character == '1' };
                default:
                    throw new FormulaSyntaxException($"Caractere '{character}' não reconhecido.");
            }
        }
    }

    enum ElementTypes {
        Variable,
        Operation,
        OpeningParenthesis,
        ClosingParenthesis,
        Constant
    }

    enum Operations {
        XOR,
        AND,
        OR,
        NOT
    }
}
