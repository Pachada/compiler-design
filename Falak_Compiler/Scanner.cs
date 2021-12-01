/*
    Falak compiler 
    Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Falak
{

    class Scanner
    {

        readonly string input;

        static readonly Regex regex = new Regex(
            @"
                (?<Compare>    [=][=]    )
              | (?<Assign>     [=]       )
              | (?<MultiComment> [<][\#](.|\n)*?[\#][>]$  )
              | (?<Comment>     [\#].*$  )
              | (?<Less_Equal>  [<][=]    )
              | (?<More_Equal>  [>][=]    )
              | (?<Different> [!][=]     )
              | (?<Var>        var\b     )
              | (?<And>         [&][&]   )
              | (?<Or>          [|][|]|[\^]   )
              | (?<Xor>        \^        )
              | (?<Not>         [!]      )
              | (?<Less>        [<]      ) # Agregar todas las variantes antes que este
              | (?<More>        [>]      )
              | (?<Plus>       [+]       )  
              | (?<Multiply>        [*]       )
              | (?<Minus>      [-]       )
              | (?<Div>        [/]       )
              | (?<Mod>        [%]       )
              | (?<Comma>      [,]       )
              | (?<Semicolon>  [;]       )
              | (?<ParLeft>    [(]       )
              | (?<ParRight>   [)]       )
              | (?<SquareBracketLeft> [\[]  )
              | (?<SquareBracketRight> [\]] )
              | (?<CurlyRight>  [}]      )
              | (?<CurlyLeft>   [{]      )
              | (?<Char>    [']([\\]([nrt\'\\""\\]|u[\dA-Fa-f]{6})|[^\\])['] )
              | (?<String>) ""([^""\n\\]|\\([nrt\\'""]|u[0-9a-fA-F]{6}))*""
              | (?<True>       true\b    )
              | (?<False>      false\b   )
              | (?<IntLiteral> \d+       )
              | (?<Int>        int\b     )
              | (?<Bool>       bool\b    )
              | (?<If>         if\b      )
              | (?<Elseif>     elseif\b  )
              | (?<Return>     return\b  )
              | (?<While>      while\b   )
              | (?<Node_Else>       else\b    )
              | (?<Break>      break\b   )
              | (?<Inc>        inc\b     )
              | (?<Dec>        dec\b     )
              | (?<Do>         do\b      )
              | (?<Print>      print\b   )
              | (?<Newline>     \n       )
              | (?<WhiteSpace>  \s       )     # Must go after Newline.
              | (?<Identifier> ([a-zA-Z_]+)([0-9a-zA-Z_]?)+ )     # Must go after all keywords
              | (?<Other>      .         )     # Must be last: match any other character.
            ",
            RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled
                | RegexOptions.Multiline
            );

        static readonly IDictionary<string, TokenCategory> tokenMap =
            new Dictionary<string, TokenCategory>() {
                {"Compare", TokenCategory.COMPARE},
                {"Assign", TokenCategory.ASSIGN},
                {"MultiComment", TokenCategory.MULTILINECOMMENT},
                {"Comment", TokenCategory.COMMENT},
                {"Less_Equal", TokenCategory.LESS_EQUAL},
                {"More_Equal", TokenCategory.MORE_EQUAL},
                {"Different", TokenCategory.DIFFERENT},
                {"Var", TokenCategory.VAR},
                {"And", TokenCategory.AND},
                {"Or", TokenCategory.OR},
                {"Xor", TokenCategory.XOR},
                {"Not", TokenCategory.NOT},
                {"Less", TokenCategory.LESS},
                {"More", TokenCategory.MORE},
                {"Plus", TokenCategory.PLUS},
                {"Multiply", TokenCategory.MUL},
                {"Minus", TokenCategory.MINUS},
                {"Div", TokenCategory.DIV},
                {"Mod", TokenCategory.MOD},
                {"Comma", TokenCategory.COMMA},
                {"Semicolon", TokenCategory.SEMICOLON},
                {"ParLeft", TokenCategory.PAR_LEFT},
                {"ParRight", TokenCategory.PAR_RIGHT},
                {"SquareBracketLeft", TokenCategory.SQUARE_BRACE_LEFT},
                {"SquareBracketRight", TokenCategory.SQUARE_BRACE_RIGHT},
                {"CurlyLeft", TokenCategory.CURLY_LEFT},
                {"CurlyRight", TokenCategory.CURLY_RIGHT},
                {"Char", TokenCategory.CHAR},
                {"String", TokenCategory.STRING},
                {"True", TokenCategory.TRUE},
                {"False", TokenCategory.FALSE},
                {"IntLiteral", TokenCategory.INT_LITERAL},
                {"Bool", TokenCategory.BOOL},
                {"If", TokenCategory.IF},
                {"Elseif", TokenCategory.ELSEIF},
                {"Return", TokenCategory.RETURN},
                {"While", TokenCategory.WHILE},
                {"Node_Else", TokenCategory.ELSE},
                {"Break", TokenCategory.BREAK},
                {"Inc", TokenCategory.INC},
                {"Dec", TokenCategory.DEC},
                {"Do", TokenCategory.DO},
                {"Int", TokenCategory.INT},
                {"Print", TokenCategory.PRINT},
                {"Newline", TokenCategory.NEWLINE},
                {"WhiteSpace", TokenCategory.WHITESPACE},
                {"Identifier", TokenCategory.IDENTIFIER}
            };

        public Scanner(string input)
        {
            this.input = input;
        }

        public IEnumerable<Token> Scan()
        {

            var result = new LinkedList<Token>();
            var row = 1;
            var columnStart = 0;

            foreach (Match m in regex.Matches(input))
            {

                if (m.Groups["Newline"].Success)
                {

                    row++;
                    columnStart = m.Index + m.Length;

                }
                else if (m.Groups["WhiteSpace"].Success
                  || m.Groups["Comment"].Success)
                {

                    // Skip white space and comments.

                }
                else if (m.Groups["MultiComment"].Success)
                {

                    MatchCollection newMatches = Regex.Matches(m.Groups["MultiComment"].Value, "\n", RegexOptions.Multiline);

                    if (newMatches.Count > 0)
                    {
                        Match lastMatch = newMatches[newMatches.Count - 1];
                        row += newMatches.Count;
                        columnStart = m.Index + lastMatch.Index + lastMatch.Length;
                    }

                }
                else if (m.Groups["Other"].Success)
                {

                    // Found an illegal character.
                    result.AddLast(
                        new Token(m.Value,
                            TokenCategory.ILLEGAL_CHAR,
                            row,
                            m.Index - columnStart + 1));

                }
                else
                {

                    // Must be any of the other tokens.
                    result.AddLast(FindToken(m, row, columnStart));
                }
            }

            result.AddLast(
                new Token(null,
                    TokenCategory.EOF,
                    row,
                    input.Length - columnStart + 1));

            return result;
        }

        Token FindToken(Match m, int row, int columnStart)
        {
            foreach (var name in tokenMap.Keys)
            {
                if (m.Groups[name].Success)
                {
                    return new Token(m.Value,
                        tokenMap[name],
                        row,
                        m.Index - columnStart + 1);
                }
            }
            throw new InvalidOperationException(
                "regex and tokenMap are inconsistent: " + m.Value);
        }
    }
}
