using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculadora_lógica {
    class FormulaSyntaxException : Exception {
        public FormulaSyntaxException(string message) : base(message) { }
    }

    static class ListExtensions {
        public static T Relative<T>(this List<T> list, int i, int delta) {
            return list[i + delta];
        }

        public static T After<T>(this List<T> list, int i) {
            return list.Relative(i, 1);
        }

        public static T Before<T>(this List<T> list, int i) {
            return list.Relative(i, -1);
        }

        public static bool HasAfter<T>(this List<T> list, int i) {
            return i < list.Count - 1;
        }

        public static bool HasBefore<T>(this List<T> list, int i) {
            return i > 0;
        }

        public static bool Surrounded<T>(this List<T> list, int i) {
            return list.HasBefore(i) && list.HasAfter(i);
        }
    }

    class Formula {
        public List<Element> Elements { get; set; }
        public List<char> Variables {
            get {
                List<char> variables = new List<char>();
                Elements.ForEach(element => {
                    if (element.Type == ElementTypes.Variable) //If it's a variable
                        if (!variables.Exists(variable => variable == element.Variable)) //And it's not present in the toResolve[checking] known variables
                            variables.Add(element.Variable); //Add to known variables
                });
                variables.Sort();
                return variables;
            }
        }

        public Formula() {
            Elements = new List<Element>();
        }

        public Formula(string input) {
            Elements = Parse(input).Elements;
        }

        public static Formula Parse(string input) {
            List<char> characters = new List<char>(input.ToCharArray());
            characters.RemoveAll(character => character == ' ');
            Formula formula = new Formula();
            characters.ForEach(character => formula.Elements.Add(Element.Parse(character)));
            formula.CheckErrors();
            return formula;
        }

        public void CheckErrors() {
            int parenthesis = 0; //Variable that will be used to check if the parenthesis are being used correctly
            for (int i = 0; i < Elements.Count; i++) {
                if (Elements[i].Type == ElementTypes.Operation) {
                    if (Elements[i].Operation == Operations.NOT) {
                        if (Elements.HasBefore(i))
                            if (Elements.Before(i).Type != ElementTypes.Operation &&
                                Elements.Before(i).Type != ElementTypes.OpeningParenthesis)
                                throw new FormulaSyntaxException("Antes do NOT é permitido apenas nada, operação ou '('");
                        if (!Elements.HasAfter(i))
                            throw new FormulaSyntaxException("A fórmula não pode terminar com um NOT");
                        else
                            if (Elements.After(i).Type != ElementTypes.OpeningParenthesis
                                && Elements.After(i).Type != ElementTypes.Variable
                                && Elements.After(i).Type != ElementTypes.Constant
                                && !(Elements.After(i).Type == ElementTypes.Operation && Elements.After(i).Operation == Operations.NOT))
                            throw new FormulaSyntaxException("Após um NOT é permitido apenas constante, variável, outro NOT ou '('");
                    } else {
                        if (Elements.HasAfter(i) && Elements.After(i).Type == ElementTypes.Operation && Elements.After(i).Operation == Operations.NOT) continue; //If the current operation element is followed by a NOT operation it's valid and there's no need to check the rest
                        if (Elements.HasBefore(i) && Elements.HasAfter(i)) {
                            if (Elements.Before(i).Type != ElementTypes.Variable && Elements.Before(i).Type != ElementTypes.Constant && Elements.Before(i).Type != ElementTypes.ClosingParenthesis)
                                throw new FormulaSyntaxException($"Operação {Elements[i].Operation.ToString()} tem elemento inválido a esquerda");
                            if (Elements.After(i).Type != ElementTypes.Variable && Elements.After(i).Type != ElementTypes.Constant && Elements.After(i).Type != ElementTypes.OpeningParenthesis)
                                throw new FormulaSyntaxException($"Operação {Elements[i].Operation.ToString()} tem elemento inválido a direita");
                        } else {
                            throw new FormulaSyntaxException($"Operação {Elements[i].Operation.ToString()} não pode estar no começo ou final da fórmula");
                        }
                    }
                } else if (Elements[i].Type == ElementTypes.OpeningParenthesis) {
                    if (Elements.After(i).Type == ElementTypes.ClosingParenthesis)
                        throw new FormulaSyntaxException("É necessário uma expressão entre parenteses");
                    parenthesis++;
                } else if (Elements[i].Type == ElementTypes.ClosingParenthesis) {
                    parenthesis--;
                    if (parenthesis <= -1) throw new FormulaSyntaxException("Um ou mais parenteses fecharam sem correspondentes abertos");
                } else if (Elements[i].Type == ElementTypes.Variable || Elements[i].Type == ElementTypes.Constant) {
                    if (Elements.HasAfter(i))
                        if (Elements.After(i).Type != ElementTypes.Operation && Elements.After(i).Type != ElementTypes.ClosingParenthesis)
                            throw new FormulaSyntaxException("Após variáveis ou constantes é permitido apenas operações ou fecha-parenteses");
                    if (Elements.HasBefore(i))
                        if (Elements.Before(i).Type != ElementTypes.Operation && Elements.Before(i).Type != ElementTypes.OpeningParenthesis)
                            throw new FormulaSyntaxException("Antes de variáveis ou constantes é permitido apenas operações e abre-parenteses");
                }
            }
            if (parenthesis > 0) throw new FormulaSyntaxException("Um ou mais parenteses foram abertos sem correspondentes fechados");
        }

        public bool[,] ResolveAll() {
            var variables = this.Variables; //Gets the formula variables
            int varCount = variables.Count; //Creates a dedicated variable to store the amount of variables
            bool[,] matrix = new bool[varCount + 1, Convert.ToInt32(Math.Pow(2, varCount))]; //Creates a matrix that will store every possible variable value combination with an extra column for the results

            int pow2;
            for (int i = 0; i < matrix.GetLength(0) - 1; i++) {
                pow2 = Convert.ToInt32(Math.Pow(2, varCount - i));
                for (int j = 0; j < matrix.GetLength(1); j++)
                    matrix[i, j] = j % pow2 >= pow2 / 2;
            }

            for (int i = 0; i < matrix.GetLength(1); i++) { //For all the possible combinations of variable values
                Elements.FindAll(element => element.Type == ElementTypes.Variable //Find elements that are variables
                ).ForEach(element => { //And goes through them
                    element.Value = matrix[variables.IndexOf(element.Variable), i]; //And replaces the variable value with the values of the toResolve[checking] combination
                });
                matrix[matrix.GetLength(0) - 1, i] = Resolve(Elements); //Then resolves the combination
            }

            return matrix;
        }

        public bool Resolve(List<Element> toResolve) {
            toResolve = toResolve.ConvertAll(element => new Element(element)); //Makes a copy of the objects elements
            toResolve.FindAll(element => element.Type == ElementTypes.Variable)
                .ForEach(element => element.Type = ElementTypes.Constant); //Turns all the variables into constants
            int checking = 0;
            Operations precedence = 0;

            void NextPrecedence() {
                switch (precedence) {
                    case Operations.XOR:
                        precedence = Operations.AND;
                        break;
                    case Operations.AND:
                        precedence = Operations.OR;
                        break;
                    case Operations.OR:
                        precedence = Operations.XOR;
                        break;
                }
            }

            void Reset() {
                precedence = Operations.XOR;
                checking = 0;
            }

            while (true) {
                if (toResolve[checking].Type == ElementTypes.Constant) {
                    if (toResolve.Surrounded(checking)) { //Not the first element nor the last element
                        if (toResolve.Before(checking).Type == ElementTypes.OpeningParenthesis && toResolve.After(checking).Type == ElementTypes.ClosingParenthesis) //If constant is surrounded by parenthesis
                        {
                            toResolve.RemoveAt(checking + 1); //Removes parenthesis
                            toResolve.RemoveAt(checking - 1);
                            Reset();
                            continue;
                        }
                    } else if (toResolve.Count == 1) //Resolved
                        return toResolve[checking].Value;
                } else if (toResolve[checking].Type == ElementTypes.Operation) {
                    if (toResolve[checking].Operation == Operations.NOT && toResolve.After(checking).Type == ElementTypes.Constant) //Is a NOT operator and it's followed by a constant
                    {
                        toResolve.Remove(toResolve[checking]); //Removes the toResolve[checking] element (NOT) and the constant takes its place
                        toResolve[checking].Value = !toResolve[checking].Value; //Negates the value of the constant
                        Reset();
                        continue;
                    } else if (toResolve.Surrounded(checking))
                        if (toResolve.Before(checking).Type == ElementTypes.Constant && toResolve.After(checking).Type == ElementTypes.Constant) //Can do the operation
                            if (toResolve[checking].Operation == precedence) {
                                if (toResolve[checking].Operation == Operations.XOR)
                                    toResolve.Before(checking).Value = toResolve.Before(checking).Value == !toResolve.After(checking).Value; //Resolves the operation and puts the result in the place of the first operand
                                else if (toResolve[checking].Operation == Operations.AND)
                                    toResolve.Before(checking).Value = toResolve.Before(checking).Value && toResolve.After(checking).Value;
                                else //OR
                                    toResolve.Before(checking).Value = toResolve.Before(checking).Value || toResolve.After(checking).Value;
                                toResolve.Remove(toResolve[checking]); //Removes the operation signal and the second operand takes its place
                                toResolve.Remove(toResolve[checking]); //Removes the second operand
                                Reset();
                                continue;
                            }
                }
                checking++;
                if (!toResolve.HasAfter(checking)) {
                    checking = 0;
                    NextPrecedence();
                }
            }
        }
    }
}