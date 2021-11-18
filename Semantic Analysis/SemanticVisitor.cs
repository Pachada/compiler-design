/*
    Falak compiler 
    Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Falak
{

    class SemanticVisitor
    {

        public HashSet<string> GlobalVariableTable;
        public IDictionary<string, Type> GlobalFunctionTable;

        public int loops, pass;
        public string FunName;

        public bool body;

        void AddApiFunctions()
        {
            GlobalFunctionTable["printi"] = new Type(true, 1);
            GlobalFunctionTable["printc"] = new Type(true, 1);
            GlobalFunctionTable["prints"] = new Type(true, 1);
            GlobalFunctionTable["println"] = new Type(true, 0);
            GlobalFunctionTable["readi"] = new Type(true, 0);
            GlobalFunctionTable["reads"] = new Type(true, 0);
            GlobalFunctionTable["new"] = new Type(true, 1);
            GlobalFunctionTable["size"] = new Type(true, 1);
            GlobalFunctionTable["add"] = new Type(true, 2);
            GlobalFunctionTable["get"] = new Type(true, 2);
            GlobalFunctionTable["set"] = new Type(true, 3);

        }

        public SemanticVisitor()
        {
            this.pass = 1;
            this.loops = 0;
            this.body = false;

            this.GlobalVariableTable = new HashSet<string>();
            this.GlobalFunctionTable = new SortedDictionary<string, Type>();

            AddApiFunctions();
        }

        public void Visit(Program node)
        {

            // Primer pasada
            // Visitar el AST y registar funciones y variables globales; Omitimos el cuerpo de las funciones
            VisitChildren(node);
            if (!GlobalFunctionTable.ContainsKey("main"))
            {
                throw new SemanticError("No main function ", node.AnchorToken);
            }
            var tableRow = GlobalFunctionTable["main"];
            if (tableRow.getArguments() > 0)
            {
                throw new SemanticError("Main function can´t have parameters ", node.AnchorToken);
            }
            // Segundo pasada
            pass = 2;

            VisitChildren(node);
        }

        public void Visit(DefList node)
        {
            body = true;
            VisitChildren(node);
            body = false;
        }

        public void Visit(Var_Def node)
        {
            var variableName = node.AnchorToken.Lexeme;
            if (pass == 1)
            {
                if (GlobalVariableTable.Contains(variableName))
                {
                    throw new SemanticError($"Variable * {variableName} * is already declare ", node.AnchorToken);
                }
                else
                {
                    GlobalVariableTable.Add(variableName);
                }
            }
            else
            {
                var tableRow = GlobalFunctionTable[FunName];
                if (!GlobalVariableTable.Contains(variableName))
                {
                    if (tableRow.localTable.Contains(variableName))
                    {
                        throw new SemanticError($"Variable * {variableName} * is already declared inside the function * {FunName} * ", node.AnchorToken);
                    }
                    else
                    {
                        tableRow.localTable.Add(variableName);
                    }

                }
                else
                {
                    if (body)
                    {
                        tableRow.localTable.Add(variableName);
                    }
                }
            }
        }

        public void Visit(VarList node)
        {
            VisitChildren(node);
        }

        public void Visit(FunDef node)
        {
            FunName = node.AnchorToken.Lexeme;
            if (pass == 1)
            {
                if (GlobalFunctionTable.ContainsKey(FunName))
                {
                    throw new SemanticError($"* {FunName} * is already declare ", node.AnchorToken);
                }
                else
                {
                    int arguments = 0;
                    List<string> numlist = new List<string>();
                    foreach (var id in node[0])
                    {   
                        foreach (var num in id)
                        {
                            if (numlist.Contains(num.AnchorToken.Lexeme)){
                                 throw new SemanticError($"Duplicate parameter * {num.AnchorToken.Lexeme} * in function * {FunName} *", node.AnchorToken);
                            }
                            numlist.Add(num.AnchorToken.Lexeme);

                            arguments++;
                        }
                    }
                    GlobalFunctionTable[FunName] = new Type(false, arguments);
                }
            }
            else
            {
                VisitChildren(node);
            }
        }


        public void Visit(Var_DefList node)
        {
            VisitChildren(node);
        }


        public void Visit(ParamList node)
        {
            VisitChildren(node);
        }


        public void Visit(StatementList node)
        {
            VisitChildren(node);
        }


        public void Visit(StatementAssign node)
        {
            var variableName = node.AnchorToken.Lexeme;
            var tableRow = GlobalFunctionTable[FunName];

            if (tableRow.localTable.Contains(variableName))
            {
                VisitChildren(node);
            }
            else if (GlobalVariableTable.Contains(variableName))
            {
                VisitChildren(node);
            }
            else
            {
                throw new SemanticError($"* {variableName} * is not declare", node.AnchorToken);

            }
        }


        public void Visit(StatementInc node) {}

        public void Visit(StatementDec node) {}

        public void Visit(StatementIf node)
        {
            VisitChildren(node);
        }

        public void Visit(Else_If node)
        {
            VisitChildren(node);
        }

        public void Visit(Node_Else node)
        {
            VisitChildren(node);
        }

        public void Visit(StatementWhile node)
        {
            loops++;
            VisitChildren(node);
            loops--;
        }

        public void Visit(StatementDo node)
        {
            loops++;
            VisitChildren(node);
            loops--;
        }

        public void Visit(StatementBreak node)
        {
            if (loops <= 0)
            {
                throw new SemanticError("Break statement was used outside a loop " + node.AnchorToken);
            }
        }

        public void Visit(StatementReturn node)
        {
            VisitChildren(node);
        }

        public void Visit(Empty node) { }

        public void Visit(Expr_List node)
        {
            VisitChildren(node);
        }

        public void Visit(Or node)
        {
            VisitChildren(node);
        }

        public void Visit(Xor node)
        {
            VisitChildren(node);
        }

        public void Visit(And node)
        {
            VisitChildren(node);
        }

        public void Visit(Compare node)
        {
            VisitChildren(node);

        }

        public void Visit(Different node)
        {
            VisitChildren(node);

        }

        public void Visit(Less_Than node)
        {
            VisitChildren(node);

        }

        public void Visit(Less_Equal node)
        {
            VisitChildren(node);

        }

        public void Visit(More_Than node)
        {
            VisitChildren(node);

        }

        public void Visit(More_Equal node)
        {
            VisitChildren(node);

        }

        public void Visit(Plus node)
        {
            VisitChildren(node);

        }

        public void Visit(Minus node)
        {
            VisitChildren(node);

        }

        public void Visit(Multiply node)
        {
            VisitChildren(node);

        }

        public void Visit(Div node)
        {
            VisitChildren(node);

        }

        public void Visit(Mod node)
        {
            VisitChildren(node);

        }


        public void Visit(Positive node)
        {
            VisitChildren(node);
        }

        public void Visit(Negative node)
        {
            VisitChildren(node);
        }

        public void Visit(Not node)
        {
            VisitChildren(node);
        }

        public void Visit(Fun_Call node)
        {
            var functionName = node.AnchorToken.Lexeme;
            if (!GlobalFunctionTable.ContainsKey(functionName))
            {
                throw new SemanticError($"* {functionName} * function is not declare", node.AnchorToken);
            }
            
            if (node[0].getCount() != GlobalFunctionTable[functionName].getArguments())
            {
                throw new SemanticError($"* {functionName} * function takes a different number of arguments", node.AnchorToken);
            }
            VisitChildren(node);
        }

        public void Visit(Var_Refer node)
        {
            var varName = node.AnchorToken.Lexeme;
            var tableRow = GlobalFunctionTable[FunName];
            if (!(GlobalVariableTable.Contains(varName)) && !(tableRow.localTable.Contains(varName)))
            {
                throw new SemanticError($"* {varName} * is not declare", node.AnchorToken);
            }
        }

        public void Visit(True node)
        {
            var lexeme = node.AnchorToken.Lexeme;
            bool value;

            if (!Boolean.TryParse(lexeme, out value)) {
                throw new SemanticError($"Boolean literal overflow * {lexeme} *", node.AnchorToken);
            }

        }


        public void Visit(False node)
        {
            var lexeme = node.AnchorToken.Lexeme;
            bool value;

            if (!Boolean.TryParse(lexeme, out value)) {
                throw new SemanticError($"Boolean literal overflow * {lexeme} *", node.AnchorToken);
            }
        }


        public void Visit(Int_Literal node)
        {
            var lexeme = node.AnchorToken.Lexeme;
            int value;

            if (!Int32.TryParse(lexeme, out value)) {
                throw new SemanticError($"Integer overflow * {lexeme} *", node.AnchorToken);
            }
        }


        public void Visit(CharLit node)
        {
            var lexeme = node.AnchorToken.Lexeme;
            char value;

            if (!Char.TryParse(lexeme, out value)) {
                throw new SemanticError($"Char literal overflow * {lexeme} *", node.AnchorToken);
            }
        }

        public void Visit(String_Lit node)
        {
            var lexeme = node.AnchorToken.Lexeme;
            try {}
            catch (OverflowException)
            {
                throw new SemanticError($"String literal overflow * {lexeme} *", node.AnchorToken);
            }

        }

        public void Visit(Array node) {}

        void VisitChildren(Node node)
        {
            foreach (var n in node)
            {
                Visit((dynamic)n);
            }
        }

    }
}
