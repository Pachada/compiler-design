/*
    Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/

using System;
using System.Collections.Generic;

namespace Falak {
    
    class Parser{

        static readonly ISet<TokenCategory> firstOfDef = new HashSet<TokenCategory>(){
            TokenCategory.VAR,
            TokenCategory.IDENTIFIER
        };

        static readonly ISet<TokenCategory> firstOfStatement = new HashSet<TokenCategory>(){
            TokenCategory.IDENTIFIER,
            TokenCategory.IF,
            TokenCategory.WHILE,
            TokenCategory.DO,
            TokenCategory.BREAK,
            TokenCategory.RETURN,
            TokenCategory.INC,
            TokenCategory.DEC,
            TokenCategory.SEMICOLON
        };

        static readonly ISet<TokenCategory> firstOfComp = new HashSet<TokenCategory>(){
            TokenCategory.COMPARE,
            TokenCategory.DIFFERENT
        };

        static readonly ISet<TokenCategory> firstOfRel = new HashSet<TokenCategory>(){
            TokenCategory.LESS,
            TokenCategory.LESS_EQUAL,
            TokenCategory.MORE,
            TokenCategory.MORE_EQUAL
        };

        static readonly ISet<TokenCategory> firstOfAdd = new HashSet<TokenCategory>(){
            TokenCategory.PLUS,
            TokenCategory.MINUS

        };

        static readonly ISet<TokenCategory> firstOfMul = new HashSet<TokenCategory>(){
            TokenCategory.MUL,
            TokenCategory.DIV,
            TokenCategory.MOD
        };

        static readonly ISet<TokenCategory> firstOfUnary = new HashSet<TokenCategory>(){
            TokenCategory.PLUS,
            TokenCategory.MINUS,
            TokenCategory.NOT

        };

        static readonly ISet<TokenCategory> firstOfLit = new HashSet<TokenCategory>(){
            TokenCategory.TRUE,
            TokenCategory.FALSE,
            TokenCategory.CHAR,
            TokenCategory.STRING,
            TokenCategory.INT_LITERAL
        };

        static readonly ISet<TokenCategory> expressions = new HashSet<TokenCategory>(){
            TokenCategory.OR,
            TokenCategory.AND,
            //TokenCategory.XOR,
            TokenCategory.COMPARE,
            TokenCategory.DIFFERENT,
            TokenCategory.LESS_EQUAL,
            TokenCategory.LESS,
            TokenCategory.MORE_EQUAL,
            TokenCategory.MORE,
            TokenCategory.PLUS,
            TokenCategory.MINUS,
            TokenCategory.MUL,
            TokenCategory.DIV,
            TokenCategory.MOD,
            TokenCategory.NOT,
            TokenCategory.PAR_LEFT,
            TokenCategory.SQUARE_BRACE_LEFT,
            TokenCategory.TRUE,
            TokenCategory.FALSE,
            TokenCategory.CHAR,
            TokenCategory.STRING,
            TokenCategory.INT_LITERAL,
            TokenCategory.IDENTIFIER
        };

        static readonly ISet<TokenCategory> firstOflist = new HashSet<TokenCategory>(){
            TokenCategory.TRUE,
            TokenCategory.FALSE,
            TokenCategory.CHAR,
            TokenCategory.STRING,
            TokenCategory.INT_LITERAL,
            TokenCategory.PAR_LEFT, 
            TokenCategory.CURLY_LEFT, 
            TokenCategory.IDENTIFIER
        };

        IEnumerator<Token> tokenStream;

        public Parser(IEnumerator<Token> tokenStream) {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext();
        }

        public TokenCategory CurrentToken {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect(TokenCategory category) {
            if (CurrentToken == category) {
                Token current = tokenStream.Current;
                tokenStream.MoveNext();
                return current;
            } else {
                throw new SyntaxError(category, tokenStream.Current);
            }
        }

        public Node Program(){
            var program = DefList();
            Expect(TokenCategory.EOF);
            return program;
        }

        public Node DefList(){
            var program_node = new Program();
            while (firstOfDef.Contains(CurrentToken)){
                program_node.Add(Def());
            }
            return program_node;
        }

        public Node Def(){
            switch (CurrentToken){
                case TokenCategory.VAR:
                    return VarDef();
                case TokenCategory.IDENTIFIER:
                    return FunDef();
                default:
                    throw new SyntaxError(firstOfDef, tokenStream.Current);
            }
        }

        public Node VarDef(){
            var var_def = new Var_Def() {
                AnchorToken = Expect(TokenCategory.VAR)
            };
            var_def.Add(VarList());
            Expect(TokenCategory.SEMICOLON);
            return var_def;
        }

        public Node VarList(){
            return IdList();
        }

        public Node IdList(){
            var id_list = new Id_List();
            var id_node = new Identifier(){
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };
            id_list.Add(id_node);
            while(CurrentToken == TokenCategory.COMMA){
                Expect(TokenCategory.COMMA);
                var node_list = new Identifier(){
                        AnchorToken = Expect(TokenCategory.IDENTIFIER)
                    };
                    id_list.Add(node_list);
            }
            return id_list;
        }

        public Node FunDef(){
            var fun_def = new Fun_Def(){
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };
            Expect(TokenCategory.PAR_LEFT);
            fun_def.Add(ParamLists());
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURLY_LEFT);
            fun_def.Add(VarDefList());
            fun_def.Add(StatementList());
            Expect(TokenCategory.CURLY_RIGHT);

            return fun_def;
        }

        public Node ParamLists(){
            var param_list = new Param_List();
            if(CurrentToken == TokenCategory.IDENTIFIER){
                param_list.Add(IdList());
            }

            return param_list;
        }

        public Node VarDefList(){
            var var_def_list = new Var_Def_List();
            while(CurrentToken == TokenCategory.VAR){
                var_def_list.Add(VarDef());
            }
            return var_def_list;
        }

        public Node StatementList(){
            var statement_list = new StatementList();
            while(firstOfStatement.Contains(CurrentToken)){
                statement_list.Add(Statement());
            }
            
            return statement_list;
        }

        public Node Statement(){
            switch(CurrentToken){
                case TokenCategory.IDENTIFIER:
                var temp = Expect(TokenCategory.IDENTIFIER);;

                    switch(CurrentToken){
                        case TokenCategory.ASSIGN:
                            return StatementAssign(temp);
                        case TokenCategory.PAR_LEFT:
                            return StatementFunCall(temp);
                        
                        default:
                            throw new SyntaxError(firstOfStatement, tokenStream.Current);

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
                var statement_break = new StatementBreak(){
                    AnchorToken = Expect(TokenCategory.BREAK)
                };
                    Expect(TokenCategory.SEMICOLON);
                    return statement_break;
                

                case TokenCategory.RETURN:
                    var statement_return = new StatementReturn(){
                        AnchorToken = Expect(TokenCategory.RETURN)
                    };
                    statement_return.Add(expr());
                    Expect(TokenCategory.SEMICOLON);
                    return statement_return;

                case TokenCategory.SEMICOLON:
                var semicolon = new StatementSemiColon(){
                    AnchorToken = Expect(TokenCategory.SEMICOLON)
                };
                    Expect(TokenCategory.SEMICOLON);
                    return semicolon;

            default:
                throw new SyntaxError(firstOfStatement, tokenStream.Current);

            }
        }

        public Node StatementAssign(Token temp){
            var statement_assign = new StatementAssign(){
                AnchorToken = temp
            };
            Expect(TokenCategory.ASSIGN);
            statement_assign.Add(expr());
            Expect(TokenCategory.SEMICOLON);

            return statement_assign;
        }

        public Node StatementInc(){
            var statement_inc = new StatementInc(){
                AnchorToken = Expect(TokenCategory.INC)
            };
            var statement_id = new Identifier(){
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            };
            statement_inc.Add(statement_id);
            Expect(TokenCategory.SEMICOLON);

            return statement_inc;
        }

        public Node StatementDec(){
            var statment_dec = new StatementDec(){
                AnchorToken = Expect(TokenCategory.DEC)
            };
            
            var statement_id = new Identifier(){
                AnchorToken = Expect(TokenCategory.IDENTIFIER)
            }; 
            
            statment_dec.Add(statement_id);
            Expect(TokenCategory.SEMICOLON);

            return statment_dec;
        }

        public Node StatementFunCall(Token temp){
            var statement_fun_call = FunCall(temp);
            Expect(TokenCategory.SEMICOLON);
            return statement_fun_call;
        }

        public Node FunCall(Token temp){      //C
            var statement_fun_call = new Fun_Call(){
                AnchorToken = temp
            };
            Expect(TokenCategory.PAR_LEFT);
            statement_fun_call.Add(ExprList());
            Expect(TokenCategory.PAR_RIGHT);

            return statement_fun_call;
        }

        public Node ExprList(){     // Checked
            var expresion_list = new Expr_List();
            if(expressions.Contains(CurrentToken)){
                expresion_list.Add(expr());
                while(CurrentToken == TokenCategory.COMMA){
                    Expect(TokenCategory.COMMA);
                    expresion_list.Add(expr());
                }
            }
            return expresion_list;
        }

        public Node StatementIf(){ //Checked
            var statement_if = new StatementIf(){
                AnchorToken = Expect(TokenCategory.IF)
            };
            
            Expect(TokenCategory.PAR_LEFT);
            statement_if.Add(expr());
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURLY_LEFT);
            statement_if.Add(StatementList());
            Expect(TokenCategory.CURLY_RIGHT);
            statement_if.Add(ElseIf());
            statement_if.Add(Else());

            return statement_if;
        }

        public Node ElseIf(){ // Checked
            var elif = new Else_If();
            while (CurrentToken == TokenCategory.ELSEIF){
                var nested = new Nested_Elseif(){
                    AnchorToken = Expect(TokenCategory.ELSEIF)
                };
                Expect(TokenCategory.PAR_LEFT);
                nested.Add(expr());
                Expect(TokenCategory.PAR_RIGHT);
                Expect(TokenCategory.CURLY_LEFT);
                nested.Add(StatementList());
                Expect(TokenCategory.CURLY_RIGHT);
                elif.Add(nested);
            }

            return elif;
        }

        public Node Else(){         // checked
            if(CurrentToken == TokenCategory.ELSE){
                var node_else = new Node_Else(){
                    AnchorToken = Expect(TokenCategory.ELSE)
                };
                Expect(TokenCategory.CURLY_LEFT);
                node_else.Add(StatementList());
                Expect(TokenCategory.CURLY_RIGHT);
                return node_else;
            }
            else return new Node_Else();
            }
        

        public Node StatementWhile(){           // checked 
            var statement_while = new StatementWhile(){
                AnchorToken = Expect(TokenCategory.WHILE)
            };
            Expect(TokenCategory.PAR_LEFT);
            statement_while.Add(expr());
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.CURLY_LEFT);
            statement_while.Add(StatementList());
            Expect(TokenCategory.CURLY_RIGHT);

            return statement_while;
        }

        
        public Node StatementDo(){          // checked
            var statement_do_while = new StatementDo();
            Expect(TokenCategory.DO);
            Expect(TokenCategory.CURLY_LEFT);
            statement_do_while.Add(StatementList());
            Expect(TokenCategory.CURLY_RIGHT);
            statement_do_while.AnchorToken = Expect(TokenCategory.WHILE);
            Expect(TokenCategory.PAR_LEFT);
            statement_do_while.Add(expr());
            Expect(TokenCategory.PAR_RIGHT);
            Expect(TokenCategory.SEMICOLON);
            
            return statement_do_while;
        }

        public Node expr(){
            return ExprOr();
        }

        public Node ExprOr(){

            var expresion = ExprAnd();
            while (CurrentToken == TokenCategory.OR){
                var or_node = new Or(){
                AnchorToken = Expect(TokenCategory.OR)
            };
            or_node.Add(expresion);
            or_node.Add(ExprAnd());
            expresion = or_node;
        }
        return expresion;
    }

        public Node ExprAnd(){
            var expresion = ExprComp();
            while(CurrentToken == TokenCategory.AND){
                var and_node = new And(){
                    AnchorToken = Expect(TokenCategory.AND)
                };
                and_node.Add(expresion);
                and_node.Add(ExprComp());
                expresion = and_node;
            }
            return expresion;
        }

        public Node ExprComp(){
            
            var expresion = ExprRel();
            while(firstOfComp.Contains(CurrentToken)){
                var expresion_op = OpComp();
                expresion_op.Add(expresion);
                expresion_op.Add(ExprRel());
                expresion = expresion_op;
            }
            return expresion;
        }

        public Node OpComp(){
            switch (CurrentToken)
            {
                case TokenCategory.COMPARE:
                var comparar = new Compare(){
                    AnchorToken = Expect(TokenCategory.COMPARE)
                };
                return comparar;
                case TokenCategory.DIFFERENT:
                    var different = new Different(){
                        AnchorToken = Expect(TokenCategory.DIFFERENT)
                    };
                    return different;                   
                
                default:
                    throw new SyntaxError(firstOfComp, tokenStream.Current);
            }
        }

        public Node ExprRel(){
            var res = ExprAdd();
            while(firstOfRel.Contains(CurrentToken)){
                var rel = OpRel();
                rel.Add(res);
                rel.Add(ExprAdd());
                res = rel;
            }
            return res;
        }

        public Node OpRel(){
            switch (CurrentToken)
            {
                case TokenCategory.LESS:
                    var less = new Less_Than(){
                        AnchorToken = Expect(TokenCategory.LESS)
                    };
                return less;
                case TokenCategory.LESS_EQUAL:
                    var less_equal = new Less_Equal(){
                        AnchorToken = Expect(TokenCategory.LESS_EQUAL)
                    };
                    return less_equal;
                case TokenCategory.MORE:
                    var more_than = new More_Than(){
                    AnchorToken = Expect(TokenCategory.MORE)
                    };
                    return more_than;
                case TokenCategory.MORE_EQUAL:
                    var more_equal = new More_Equal(){
                        AnchorToken = Expect(TokenCategory.MORE_EQUAL)
                    };
                    return more_equal;
                
                default:
                    throw new SyntaxError(firstOfRel, tokenStream.Current);
            }
        }

        public Node ExprAdd(){
            var res = ExprMul();
            while(firstOfAdd.Contains(CurrentToken)){
                var add = OpAdd();
                add.Add(res);
                add.Add(ExprMul());
                res = add;
            }
            return res;
        }

        public Node OpAdd(){
            switch (CurrentToken)
            {
                case TokenCategory.PLUS:
                    var plus = new Plus(){
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };
                    return plus;
                case TokenCategory.MINUS:
                    var minus = new Minus(){
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };
                    return minus;
                
                default:
                    throw new SyntaxError(firstOfAdd, tokenStream.Current);
            }
        }

        public Node ExprMul(){
            var expresion = ExprUnary();
            while(firstOfMul.Contains(CurrentToken)){
                var op_expr = OpMul();
                op_expr.Add(expresion);
                op_expr.Add(ExprUnary());
                expresion = op_expr;
            }
            return expresion;
        }

        public Node OpMul(){
            switch (CurrentToken){
                case TokenCategory.MUL:
                    var multiply = new Multiply(){
                        AnchorToken = Expect(TokenCategory.MUL)
                    };
                    return multiply;
                case TokenCategory.DIV:
                    var div = new Div(){
                        AnchorToken = Expect(TokenCategory.DIV)
                    };
                    return div;
                case TokenCategory.MOD:
                    var mod = new Mod(){
                        AnchorToken = Expect(TokenCategory.MOD)
                    };
                    return mod;
                
                default:
                    throw new SyntaxError(firstOfMul, tokenStream.Current);
            }
        }

        public Node ExprUnary(){

            if (firstOfUnary.Contains(CurrentToken)){
                var res = OpUnary();
                res.Add(ExprUnary());
                return res;
            }
            else return ExprPrimary();
        }

        public Node OpUnary(){
            switch (CurrentToken)
            {
                case TokenCategory.PLUS:
                    var plus = new Plus(){
                        AnchorToken = Expect(TokenCategory.PLUS)
                    };
                    return plus;
                case TokenCategory.MINUS:
                    var minus = new Minus(){
                        AnchorToken = Expect(TokenCategory.MINUS)
                    };
                    return minus;
                case TokenCategory.NOT:
                    var not = new Not(){
                        AnchorToken = Expect(TokenCategory.NOT)
                    };
                    return not;
                
                default:
                    throw new SyntaxError(firstOfUnary, tokenStream.Current);
            }
        }

        public Node ExprPrimary(){
            switch (CurrentToken) 
            {
                case TokenCategory.IDENTIFIER:
                    var temp = Expect(TokenCategory.IDENTIFIER);
                    if(CurrentToken == TokenCategory.PAR_LEFT){
                        return FunCall(temp);
                    } else {
                        return new Var_Refer(){
                            AnchorToken = temp
                        };
                    }
                case TokenCategory.SQUARE_BRACE_LEFT:
                    var node_array = Array();
                    return node_array;
                case TokenCategory.TRUE:
                case TokenCategory.FALSE:
                case TokenCategory.INT_LITERAL:
                case TokenCategory.CHAR:
                case TokenCategory.STRING:
                    return Lit();
                
                case TokenCategory.PAR_LEFT:
                    Expect(TokenCategory.PAR_LEFT);
                    var res = expr();
                    Expect(TokenCategory.PAR_RIGHT);
                    return res;
                
                default:
                    throw new SyntaxError(firstOflist, tokenStream.Current);
            }
        }

        public Node Array(){            // checked
            Expect(TokenCategory.SQUARE_BRACE_LEFT);
            var res = new Array(){
                ExprList()
            };
            Expect(TokenCategory.SQUARE_BRACE_RIGHT);

            return res;
        }

        public Node Lit(){
            switch (CurrentToken)
            {
                case TokenCategory.TRUE:
                    var node_true = new True(){
                    AnchorToken = Expect(TokenCategory.TRUE)
                    };
                    return node_true;
                case TokenCategory.FALSE:
                    var node_false = new False(){
                    AnchorToken = Expect(TokenCategory.FALSE)
                    };
                    return node_false;
                case TokenCategory.INT_LITERAL:
                    var node_int = new Int_Literal(){
                    AnchorToken = Expect(TokenCategory.INT_LITERAL)
                    };
                    return node_int;
                case TokenCategory.CHAR:
                    var node_char = new CharLit(){
                    AnchorToken = Expect(TokenCategory.CHAR)
                    };
                    return node_char;
                case TokenCategory.STRING:
                    var node_string = new String_Lit(){
                    AnchorToken = Expect(TokenCategory.STRING)
                    };
                    return node_string;

                default:
                    throw new SyntaxError(firstOfLit, tokenStream.Current);
            }
        }

    }
}