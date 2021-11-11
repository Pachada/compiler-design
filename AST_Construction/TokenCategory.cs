/*
  Falak compiler - Token categories for the scanner.
  Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM

*/

namespace Falak {

    enum TokenCategory {
        COMPARE,
        ASSIGN,
        MULTILINECOMMENT,
        COMMENT,
        MORE_EQUAL,
        LESS_EQUAL,
        DIFFERENT,
        VAR,
        AND,
        OR,
        NOT,
        LESS,
        MORE,
        PLUS,
        MUL,
        MINUS,
        DIV,
        MOD,
        COMMA,
        SEMICOLON,
        PAR_LEFT,
        PAR_RIGHT,
        SQUARE_BRACE_LEFT,
        SQUARE_BRACE_RIGHT,
        CURLY_LEFT,
        CURLY_RIGHT,
        CHAR,
        STRING,
        TRUE,
        FALSE,
        INT_LITERAL,
        BOOL,
        IF,
        ELSEIF,
        RETURN,
        WHILE,
        ELSE,
        BREAK,
        INC,
        DEC,
        DO,
        INT,
        PRINT,
        NEWLINE,
        WHITESPACE,
        IDENTIFIER,
        EOF,
        ILLEGAL_CHAR
        
    }
}
