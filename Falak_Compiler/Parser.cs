/*
    Falak compiler 
    Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Falak
{

    class Parser
    {

        static readonly ISet<TokenCategory> firstOfDef =
            new HashSet<TokenCategory>() {
                TokenCategory.VAR,
                TokenCategory.IDENTIFIER
            };

        static readonly ISet<TokenCategory> firstOfStatement =
            new HashSet<TokenCategory>() {
                TokenCategory.ASSIGN,
                TokenCategory.INC,
                TokenCategory.DEC,
                TokenCategory.IDENTIFIER,
                TokenCategory.IF,
                TokenCategory.WHILE,
                TokenCategory.DO,
                TokenCategory.BREAK,
                TokenCategory.RETURN,
                TokenCategory.SEMICOLON
            };
        static readonly ISet<TokenCategory> firstOfBinaryExpr =
            new HashSet<TokenCategory>() {
                TokenCategory.OR,
                TokenCategory.AND,
                TokenCategory.XOR
            };

        static readonly ISet<TokenCategory> firstOfCompare =
            new HashSet<TokenCategory>() {
                TokenCategory.COMPARE,
                TokenCategory.DIFFERENT
            };

        static readonly ISet<TokenCategory> firstOfRelation =
            new HashSet<TokenCategory>() {
                TokenCategory.LESS,
                TokenCategory.LESS_EQUAL,
                TokenCategory.MORE,
                TokenCategory.MORE_EQUAL
            };

        static readonly ISet<TokenCategory> firstOfAddition =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.MINUS
            };

        static readonly ISet<TokenCategory> firstOfMultiplication =
            new HashSet<TokenCategory>() {
                TokenCategory.MUL,
                TokenCategory.DIV,
                TokenCategory.MOD
            };

        static readonly ISet<TokenCategory> firstOfUnary =
            new HashSet<TokenCategory>() {
                TokenCategory.PLUS,
                TokenCategory.MINUS,
                TokenCategory.NOT
            };

        static readonly ISet<TokenCategory> firstOfPrimaryExpr =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.SQUARE_BRACE_LEFT,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.CHAR,
                TokenCategory.STRING,
                TokenCategory.INT_LITERAL,
                TokenCategory.PAR_LEFT
            };

        static readonly ISet<TokenCategory> firstOfSuperExpression =
            new HashSet<TokenCategory>() {
                TokenCategory.IDENTIFIER,
                TokenCategory.SQUARE_BRACE_LEFT,
                TokenCategory.TRUE,
                TokenCategory.FALSE,
                TokenCategory.CHAR,
                TokenCategory.STRING,
                TokenCategory.INT_LITERAL,
                TokenCategory.PAR_LEFT,
                TokenCategory.PLUS,
                TokenCategory.MINUS,
                TokenCategory.NOT
            };

        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream)
        {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
        }

        public TokenCategory CurrentToken
        {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect(TokenCategory category)
        {
            if (CurrentToken == category)
            {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            }
            else
            {
                throw new SyntaxError(category, tokenStream.Current);
            }
        }

        public Node Program()
        {
            var result = DefList();
            Expect(TokenCategory.EOF);
            var newNode = new Program();
            newNode.Add(result);
            return newNode;
        }

        public Node DefList()
        {
            var defList = new DefList();
            while (firstOfDef.Contains(CurrentToken))
            {
                var expr_ = Def();
                defList.Add(expr_);
            }
            return defList;
        }

        public Node Def()
        {
            switch (CurrentToken)
            {

                case TokenCategory.IDENTIFIER:
                    return FunDef();

                case TokenCategory.VAR:
                    return Var_Def();

                default:
                    throw new SyntaxError(firstOfDef,
                                        tokenStream.Current);
            }
        }

        public Node FunDef()
        {
            var idToken = Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.PAR_LEFT);
            var paramList = ParamList();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURLY_LEFT);

            // 
            var varDefList = VarDefList();

            var stmtList = StatementList();
            Expect(TokenCategory.CURLY_RIGHT);
            var result = new FunDef() { paramList, varDefList, stmtList };
            result.AnchorToken = idToken;
            return result;
        }

        public Node VarList()
        {
            var result = IdList();
            return result;
        }

        public Node Var_Def()
        {
            Expect(TokenCategory.VAR);
            var result = VarList();
            Expect(TokenCategory.SEMICOLON);
            return result;
        }

        public Node VarDefList()
        {
            var result = new Var_DefList();
            while (CurrentToken == TokenCategory.VAR)
            {
                var result_b = Var_Def();
                result.Add(result_b);
            }
            return result;
        }

        public Node ParamList()
        {
            var result = new ParamList();
            if (CurrentToken == TokenCategory.IDENTIFIER)
            {
                var idList = IdList();
                result.Add(idList);
            }
            return result;
        }

        public Node IdList()
        {
            var result = new VarList();
            var newNode = new Var_Def();
            newNode.AnchorToken = Expect(TokenCategory.IDENTIFIER);
            result.Add(newNode);
            while (CurrentToken == TokenCategory.COMMA)
            {
                newNode = new Var_Def();
                Expect(TokenCategory.COMMA);
                newNode.AnchorToken = Expect(TokenCategory.IDENTIFIER);
                result.Add(newNode);
            }
            return result;
        }

        public Node StatementList()
        {
            var stmtList = new StatementList();
            while (firstOfStatement.Contains(CurrentToken))
            {
                stmtList.Add(Statement());
            }
            return stmtList;
        }

        public Node Statement()
        {
            switch (CurrentToken)
            {
                case TokenCategory.IDENTIFIER:
                    var idToken = Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PAR_LEFT)
                    {
                        return StatementFunCall(idToken);
                    }
                    else
                    {
                        return StatementAssign(idToken);
                    }

                case TokenCategory.INC:
                    return StatementInc();

                case TokenCategory.DEC:
                    return StatementDec();

                case TokenCategory.IF:
                    return StatementIf();

                case TokenCategory.WHILE:
                    return StatementWhile();

                case TokenCategory.DO:
                    return StatementDo();

                case TokenCategory.BREAK:
                    return StatementBreak();

                case TokenCategory.RETURN:
                    return StatementReturn();

                case TokenCategory.SEMICOLON:
                    return StatementSemiColon();

                default:
                    throw new SyntaxError(firstOfStatement,
                                        tokenStream.Current);
            }
        }

        public Node StatementAssign(Token idToken)
        {
            var result = new StatementAssign();
            result.AnchorToken = idToken;
            Expect(TokenCategory.ASSIGN);
            var expr_ = Expr();
            Expect(TokenCategory.SEMICOLON);
            result.Add(expr_);
            return result;
        }

        public Node StatementInc()
        {
            var incToken = Expect(TokenCategory.INC);
            var newToken = Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.SEMICOLON);
            var result = new StatementInc();
            var newNode = new Var_Refer();
            result.AnchorToken = incToken;
            newNode.AnchorToken = newToken;
            result.Add(newNode);
            return result;
        }

        public Node StatementDec()
        {
            var decToken = Expect(TokenCategory.DEC);
            var newToken = Expect(TokenCategory.IDENTIFIER);
            Expect(TokenCategory.SEMICOLON);
            var result = new StatementDec();
            var newNode = new Var_Refer();
            result.AnchorToken = decToken;
            newNode.AnchorToken = newToken;
            result.Add(newNode);
            return result;
        }

        public Node StatementFunCall(Token idToken)
        {
            var result = Fun_Call(idToken);
            return result;
        }

        public Node Fun_Call(Token idToken)
        {
            Expect(TokenCategory.PAR_LEFT);
            var exprList = new Fun_Call();
            exprList.AnchorToken = idToken;
            exprList.Add(ExprList());
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.SEMICOLON);
            return exprList;
        }

        public Node StatementIf()
        {
            var result = new StatementIf();
            var ifToken = Expect(TokenCategory.IF);
            result.AnchorToken = ifToken;
            Expect(TokenCategory.PAR_LEFT);
            var expr = Expr();
            result.Add(expr);
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURLY_LEFT);
            var stmtList = StatementList();
            result.Add(stmtList);
            Expect(TokenCategory.CURLY_RIGHT);
            if (CurrentToken == TokenCategory.ELSEIF)
            {
                var elseIf = ElseIf();
                result.Add(elseIf);
            }
            else if (CurrentToken == TokenCategory.ELSE)
            {
                var else_ = Node_Else();
                result.Add(else_);
            }
            return result;
        }

        public Node ElseIf()
        {
            var result = new Else_If();
            var elifToken = Expect(TokenCategory.ELSEIF);
            result.AnchorToken = elifToken;
            Expect(TokenCategory.PAR_LEFT);
            var expr = Expr();
            result.Add(expr);
            
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURLY_LEFT);
            var stmtList = StatementList();
            result.Add(stmtList);
            Expect(TokenCategory.CURLY_RIGHT);
            if (CurrentToken == TokenCategory.ELSEIF)
            {
                var elseIf = ElseIf();
                result.Add(elseIf);
            }
            else if (CurrentToken == TokenCategory.ELSE)
            {
                var else_ = Node_Else();
                result.Add(else_);
            }
            
            return result;
        }

        public Node Node_Else()
        {
            var result = new Node_Else();
            if (CurrentToken == TokenCategory.ELSE)
            {
                var elseToken = Expect(TokenCategory.ELSE);
                Expect(TokenCategory.CURLY_LEFT);
                var stmtList = StatementList();
                result.AnchorToken = elseToken;
                result.Add(stmtList);
                Expect(TokenCategory.CURLY_RIGHT);
            }
            return result;
        }

        public Node StatementWhile()
        {
            var whileToken = Expect(TokenCategory.WHILE);
            Expect(TokenCategory.PAR_LEFT);
            var expr_ = Expr();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURLY_LEFT);
            var stmtList = StatementList();
            Expect(TokenCategory.CURLY_RIGHT);
            var result = new StatementWhile() { expr_, stmtList };
            result.AnchorToken = whileToken;
            return result;
        }

        public Node StatementDo()
        {
            var doToken = Expect(TokenCategory.DO);
            Expect(TokenCategory.CURLY_LEFT);
            var stmtList = StatementList();
            Expect(TokenCategory.CURLY_RIGHT);
            Expect(TokenCategory.WHILE);
            Expect(TokenCategory.PAR_LEFT);
            var expr_ = Expr();
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.SEMICOLON);
            var result = new StatementDo() { stmtList, expr_ };
            result.AnchorToken = doToken;
            return result;
        }

        public Node StatementBreak()
        {
            var breakToken = Expect(TokenCategory.BREAK);
            Expect(TokenCategory.SEMICOLON);
            var result = new StatementBreak();
            result.AnchorToken = breakToken;
            return result;
        }

        public Node StatementReturn()
        {
            var returnToken = Expect(TokenCategory.RETURN);
            var expr_ = Expr();
            Expect(TokenCategory.SEMICOLON);
            var result = new StatementReturn() { expr_ };
            result.AnchorToken = returnToken;
            return result;
        }

        public Node StatementSemiColon()
        {
            var result = new Empty();
            {
                Expect(TokenCategory.SEMICOLON);
            }
            return result;
        }

        public Node ExprList()
        {
            var exprList = new Expr_List();
            if (firstOfSuperExpression.Contains(CurrentToken))
            {
                var expr_ = Expr();
                exprList.Add(expr_);
                while (CurrentToken == TokenCategory.COMMA)
                {
                    Expect(TokenCategory.COMMA);
                    var newExpr = Expr();
                    exprList.Add(newExpr);
                }
            }
            return exprList;
        }

        public Node Expr()
        {
            var result = ExprOr();
            return result;
        }

        public Node ExprOr()
        {
            var result = ExprAnd();
            while (firstOfBinaryExpr.Contains(CurrentToken))
            {
                switch (CurrentToken)
                {

                    case TokenCategory.OR:
                        var newOr = new Or();
                        var idToken = Expect(TokenCategory.OR);
                        newOr.Add(result);
                        newOr.Add(ExprAnd());
                        newOr.AnchorToken = idToken;
                        result = newOr;
                        break;

                    case TokenCategory.XOR:
                        var newXor = new Xor();
                        var idToken2 = Expect(TokenCategory.XOR);
                        newXor.Add(result);
                        newXor.Add(ExprAnd());
                        newXor.AnchorToken = idToken2;
                        result = newXor;
                        break;

                    default:
                        throw new SyntaxError(firstOfBinaryExpr,
                                    tokenStream.Current);
                }
            }
            return result;
        }

        //public Node Op_Or(){}

        public Node ExprAnd()
        {
            var result = ExprComp();
            while (CurrentToken == TokenCategory.AND)
            {
                var idToken = Expect(TokenCategory.AND);
                var newAnd = new And();
                newAnd.Add(result);
                newAnd.Add(ExprComp());
                newAnd.AnchorToken = idToken;
                result = newAnd;
            }
            return result;
        }

        public Node ExprComp()
        {
            var expr_ = ExprRel();
            while (firstOfCompare.Contains(CurrentToken))
            {
                var exprb = OpComp();
                exprb.Add(expr_);
                exprb.Add(ExprRel());
                expr_ = exprb;
            }
            return expr_;
        }

        public Node OpComp()
        {
            switch (CurrentToken)
            {

                case TokenCategory.COMPARE:
                    return new Compare()
                    {
                        AnchorToken = Expect(TokenCategory.COMPARE)
                    };

                case TokenCategory.DIFFERENT:
                    return new Different()
                    {
                        AnchorToken = Expect(TokenCategory.DIFFERENT)
                    };

                default:
                    throw new SyntaxError(firstOfCompare,
                                        tokenStream.Current);
            }
        }

        public Node ExprRel()
        {
            var expr_ = ExprAdd();
            while (firstOfRelation.Contains(CurrentToken))
            {
                var exprb = OpRel();
                exprb.Add(expr_);
                exprb.Add(ExprAdd());
                expr_ = exprb;
            }
            return expr_;
        }

        public Node OpRel()
        {
            switch (CurrentToken)
            {

                case TokenCategory.MORE:
                    return new More_Than()
                    {
                        AnchorToken = Expect(TokenCategory.MORE)
                    };

                case TokenCategory.MORE_EQUAL:
                    return new More_Equal()
                    {
                        AnchorToken = Expect(TokenCategory.MORE_EQUAL)
                    };

                case TokenCategory.LESS:
                    return new Less_Than()
                    {
                        AnchorToken = Expect(TokenCategory.LESS)
                    };

                case TokenCategory.LESS_EQUAL:
                    return new Less_Equal()
                    {
                        AnchorToken = Expect(TokenCategory.LESS_EQUAL)
                    };

                default:
                    throw new SyntaxError(firstOfRelation,
                                        tokenStream.Current);
            }
        }

        public Node ExprAdd()
        {
            var expr_ = ExprMul();
            while (firstOfAddition.Contains(CurrentToken))
            {
                var exprb = OpAdd();
                exprb.Add(expr_);
                exprb.Add(ExprMul());
                expr_ = exprb;
            }
            return expr_;
        }

        public Node OpAdd()
        {
            switch (CurrentToken)
            {

                case TokenCategory.PLUS:
                    return new Plus()
                    {
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };

                case TokenCategory.MINUS:
                    return new Minus()
                    {
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };

                default:
                    throw new SyntaxError(firstOfAddition,
                                        tokenStream.Current);
            }
        }

        public Node ExprMul()
        {
            var expr_ = ExprUnary();
            while (firstOfMultiplication.Contains(CurrentToken))
            {
                var exprb = OpMul();
                exprb.Add(expr_);
                exprb.Add(ExprUnary());
                expr_ = exprb;
            }
            return expr_;
        }

        public Node OpMul()
        {
            switch (CurrentToken)
            {

                case TokenCategory.MUL:
                    return new Multiply()
                    {
                        AnchorToken = Expect(TokenCategory.MUL)
                    };

                case TokenCategory.DIV:
                    return new Div()
                    {
                        AnchorToken = Expect(TokenCategory.DIV)
                    };

                case TokenCategory.MOD:
                    return new Mod()
                    {
                        AnchorToken = Expect(TokenCategory.MOD)
                    };

                default:
                    throw new SyntaxError(firstOfMultiplication,
                                        tokenStream.Current);
            }
        }

        public Node ExprUnary()
        {
            if (firstOfUnary.Contains(CurrentToken))
            {
                var result = OpUnary();
                result.Add(ExprUnary());
                return result;

            }
            else if (firstOfSuperExpression.Contains(CurrentToken))
            {
                var result = ExprPrimary();
                return result;

            }
            else
            {
                throw new SyntaxError(firstOfUnary,
                                    tokenStream.Current);
            }
        }

        public Node OpUnary()
        {
            switch (CurrentToken)
            {

                case TokenCategory.PLUS:
                    return new Positive()
                    {
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };

                case TokenCategory.MINUS:
                    return new Negative()
                    {
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };

                case TokenCategory.NOT:
                    return new Not()
                    {
                        AnchorToken = Expect(TokenCategory.NOT)
                    };

                default:
                    throw new SyntaxError(firstOfUnary,
                                        tokenStream.Current);
            }
        }

        public Node ExprPrimary()
        {
            switch (CurrentToken)
            {

                case TokenCategory.IDENTIFIER:
                    var idToken = Expect(TokenCategory.IDENTIFIER);
                    if (CurrentToken == TokenCategory.PAR_LEFT)
                    {
                        var result = new Fun_Call();
                        result.AnchorToken = idToken;
                        Expect(TokenCategory.PAR_LEFT);
                        result.Add(ExprList());
                        Expect(TokenCategory.PAR_RIGHT);
                        return result;
                    }
                    else
                    {
                        var result = new Var_Refer();
                        result.AnchorToken = idToken;
                        return result;
                    }


                case TokenCategory.SQUARE_BRACE_LEFT:
                    return Array();

                case TokenCategory.TRUE:
                    return new True
                    {
                        AnchorToken = Expect(TokenCategory.TRUE)
                    };

                case TokenCategory.FALSE:
                    return new False
                    {
                        AnchorToken = Expect(TokenCategory.FALSE)
                    };

                case TokenCategory.INT_LITERAL:
                    return new Int_Literal
                    {
                        AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };

                case TokenCategory.CHAR:
                    return new CharLit
                    {
                        AnchorToken = Expect(TokenCategory.CHAR)
                    };

                case TokenCategory.STRING:
                    return new String_Lit
                    {
                        AnchorToken = Expect(TokenCategory.STRING)
                    };

                case TokenCategory.PAR_LEFT:
                    Expect(TokenCategory.PAR_LEFT);
                    var result_c = Expr();
                    Expect(TokenCategory.PAR_RIGHT);
                    return result_c;

                default:
                    throw new SyntaxError(firstOfPrimaryExpr,
                                        tokenStream.Current);
            }
        }

        public Node Array()
        {
            Expect(TokenCategory.SQUARE_BRACE_LEFT);
            var result_b = new Array();
            result_b.Add(ExprList());
            Expect(TokenCategory.SQUARE_BRACE_RIGHT);
            return result_b;
        }

    }
}