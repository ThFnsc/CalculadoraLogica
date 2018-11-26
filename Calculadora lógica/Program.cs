using System;
using System.Collections.Generic;

namespace Calculadora_lógica {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(". = AND\n+ = OR\n* = XOR\n~ = NOT\n" +
                "Precedência é respeitada: XOR > AND > OR\n" +
                "Exemplo: S=A.B+A.~C+D.(A+B)"); //Intruções
            while (true) {
                Console.Write("\nDigite o calculo: S=");
                try {
                    var formula = new Formula(Console.ReadLine());
                    bool[,] results = formula.ResolveAll();

                    Console.WriteLine();
                    formula.Variables.ForEach(variable => Console.Write($"| {variable} "));
                    Console.WriteLine("-> S\n");

                    for (int j = 0; j < results.GetLength(1); j++)
                        for (int i = 0; i < results.GetLength(0); i++)
                            if (i == results.GetLength(0) - 1)
                                Console.WriteLine($"-> {(results[i, j] ? 1 : 0)}");
                            else
                                Console.Write($"| {(results[i, j] ? 1 : 0)} ");
                } catch (FormulaSyntaxException e) {
                    Console.WriteLine($"Erro de sintaxe: {e.Message}");
                }
            }
        }
    }
}