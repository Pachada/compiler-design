/*
    Falak compiler 
    Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/
using System;

namespace Falak
{

    class SemanticError : Exception
    {

        public SemanticError(string message, Token token) :
            base($"Semantic Error: {message} \n"
                    + $"at row {token.Row}, column {token.Column}.")
        {
        }

        public SemanticError(string message) :
            base($"Semantic Error: {message} \n")
        {
        }
    }
}
