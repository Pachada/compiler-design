/*
  Falak compiler - This class performs the lexical analysis,
  (a.k.a. scanning).
  Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Falak {

    class Scanner {

        readonly string input;

        static readonly Regex regex = new Regex(
            @"
                (?<Compare>    [=][=]    )
              | (?<Assign>     [=]       )
              | (?<MultiComment> [<][\#](.|\n)*?[\#][>]$  )
              | (?<Comment>     [\#].*$  )
              | (?<LessEqual>  [<][=]    )
              | (?<MoreEqual>  [>][=]    )
              | (?<Different> [!][=]     )
              | (?<Var>        var\b     )
              | (?<And>         [&][&]   )
              | (?<Or>          [|][|]|[\^]   )
              | (?<Less>        [<]      ) # Agregar todas las variantes antes que este
              | (?<More>        [>]      )
              | (?<Plus>       [+]       )  
              | (?<Mul>        [*]       )
              | (?<Minus>      [-]       )
              | (?<Div>        [/]       )
              | (?<Mod>        [%]       )
              | (?<Comma>      [,]       )
              | (?<Semicolon>  [;]       )
              | (?<ParLeft>    [(]       )
              | (?<ParRight>   [)]       )
              | (?<SquareBracketLeft> [\[]  )
              | (?<SquareBracketRight> [\]] )
              | (?<CurlyRigth>  [}]      )
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
              | (?<Else>       else\b    )
              | (?<Break>      break\b   )
              | (?<Inc>        inc\b     )
              | (?<Dec>        dec\b     )
              | (?<Do>         do\b      )
              | (?<Print>      print\b   )
              | (?<Newline>     \n       )
              | (?<WhiteSpace>  \s       )     # Must go after Newline.
              | (?<Identifier> ([a-zA-Z]+)([_]?)([0-9]?)+ )     # Must go after all keywords
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
                {"LessEqual", TokenCategory.LESS_EQUAL},
                {"MoreEqual", TokenCategory.MORE_EQUAL},
                {"Different", TokenCategory.DIFFERENT},
                {"Var", TokenCategory.VAR},
                {"And", TokenCategory.AND},
                {"Or", TokenCategory.OR},
                {"Less", TokenCategory.LESS},
                {"More", TokenCategory.MORE},
                {"Plus", TokenCategory.PLUS},
                {"Mul", TokenCategory.MUL},
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
                {"CurlyRigth", TokenCategory.CURLY_RIGHT},
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
                {"Else", TokenCategory.ELSE},
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

        public Scanner(string input) {
            this.input = input;
        }

        public IEnumerable<Token> Scan() {

            var result = new LinkedList<Token>();
            var row = 1;
            var columnStart = 0;

            foreach (Match m in regex.Matches(input)) {

                if (m.Groups["Newline"].Success) {
                    row++;
                    columnStart = m.Index + m.Length;

                } else if (m.Groups["WhiteSpace"].Success
                    || m.Groups["Comment"].Success) {

                    // Skip white space and comments.

                } else if (m.Groups ["MultiComment"].Success){

                    MatchCollection multiCommentMatches = Regex.Matches(m.Groups ["MultiComment"].Value, "\n", RegexOptions.Multiline);

                    if(multiCommentMatches.Count > 0){
                        row += multiCommentMatches.Count;
                        Match lastMatch = multiCommentMatches[multiCommentMatches.Count - 1];
                        columnStart = m.Index + lastMatch.Index + lastMatch.Length;
                    }

                } else if (m.Groups["Other"].Success) {

                    // Found an illegal character.
                    result.AddLast(
                        new Token(m.Value,
                            TokenCategory.ILLEGAL_CHAR,
                            row,
                            m.Index - columnStart + 1));

                } else {

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

        Token FindToken(Match m, int row, int columnStart) {
            foreach (var name in tokenMap.Keys) {
                if (m.Groups[name].Success) {
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
