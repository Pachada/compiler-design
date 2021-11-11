using System;
using System.Collections.Generic;
using System.Linq;

namespace Falak {

    class SemanticVisitor{
        public GlobalTable globalTable{
            get;
            private set;
        }
        public FunTable funTable{
            get;
            private set;
        }
        public LocalTable localTable{
            get;
            private set;
        }

        public IDictionary<string, LocalTable> localSymbol = new Dictionary<string, LocalTable>();

        private int loops; 
        private bool firstVisit;
        private bool ParamIs;
        private bool inside; 

        public SemanticVisitor() {
            loops = 0;
            firstVisit = true;
            ParamIs = false;
            globalTable = new GlobalTable();
            funTable = new FunTable();
            localTable = new LocalTable();

            //API
            funTable["printi"] = new List<object>(){true, 1, "null"};
            funTable["printc"] = new List<object>(){true, 1, "null"};
            funTable["prints"] = new List<object>(){true, 1, "null"};
            funTable["println"] = new List<object>(){true, 0, "null"};
            funTable["readi"] = new List<object>(){true, 0, "null"};
            funTable["reads"] = new List<object>(){true, 0, "null"};
            funTable["new"] = new List<object>(){true, 1, "null"};
            funTable["size"] = new List<object>(){true, 1, "null"};
            funTable["add"] = new List<object>(){true, 2, "null"};
            funTable["get"] = new List<object>(){true, 2, "null"};
            funTable["set"] = new List<object>(){true, 3, "null"};

        }

        public void Visit(Program node){
            VisitChildren(node);
            firstVisit = false;
            if (!funTable.Contains("main")){
                throw new SemanticError("Please add a main function");
            }

            var mainArity = funTable["main"].ElementAt(1);
            if ((int) mainArity>0){
                throw new SemanticError("Main does not have parameters");
            }
            VisitChildren(node);
        }

        public void Visit(Var_Def node) {
            VisitChildren(node);
        }

        public void Visit (Fun_Def node){
            var name = node.AnchorToken.Lexeme;
            if(firstVisit) {
                int arity = 0;
                try{
                    foreach (var attribute in node[0][0]){
                        arity++;
                    }
                }catch (Exception e){
                    arity = 0;
                }
                if(funTable.Contains(name)){
                    throw new SemanticError("Duplicate function: " + name, node.AnchorToken);
                } else {
                    funTable[name] = new List<object>(){false, arity, " " + "Function" + name};
                }
            } else {
                localTable = new LocalTable();
                Visit((dynamic) node[0]);
                Visit((dynamic) node[1]);
                inside = true;
                Visit((dynamic) node[2]);
                localSymbol[name] = localTable;
                localTable = new LocalTable();
                inside = false;
            }
        }

        public void Visit(Var_Def_List node){
            VisitChildren(node);
        }

        public void Visit(Param_List node){
            ParamIs = true;
            VisitChildren(node);
            ParamIs = false;
        }

        public void Visit(StatementList node){
            VisitChildren(node);
        }

        public void Visit(StatementAssign node){
            var name = node.AnchorToken.Lexeme;
            if (!localTable.Contains(name) && !globalTable.Contains(name)){
                throw new SemanticError("You didn't declared the variable: " + name, node.AnchorToken);

            }else{
                VisitChildren(node);
            }
        }

        public void Visit(StatementReturn node){
            var name = node.AnchorToken.Lexeme;
            if(!inside){
                throw new SemanticError("Not in a function", node.AnchorToken);

            }else{
                VisitChildren(node);
            }
        }

        public void Visit(StatementWhile node){
            loops++;
            Visit((dynamic) node[0]);
            Visit((dynamic) node[1]);
            loops--;
        }

        public void Visit(StatementDo node) {
            loops++;
            Visit((dynamic) node[0]);
            Visit((dynamic) node[1]);
            loops--;
        }

        public void Visit(StatementIf node) {
            Visit((dynamic) node[0]);
            Visit((dynamic) node[1]);
            Visit((dynamic) node[2]);
            Visit((dynamic) node[3]);
        }

        public void Visit(Else_If node) {
            VisitChildren(node);
        }

        
        public void Visit(Nested_Elseif node) {
            VisitChildren(node);
        }

        
        public void Visit(Node_Else node) {
            VisitChildren(node);
        }

        public void Visit(Id_List node){
            VisitChildren(node);
            
        }

        public void Visit(StatementBreak node) {
            if(loops <= 0){
                throw new SemanticError("Can't break if not looping.", node.AnchorToken);
            }
        }

        public void Visit(Fun_Call node) {
            var name = node.AnchorToken.Lexeme;
            if(!funTable.Contains(name)){
                throw new SemanticError("Not a declared function: " + name, node.AnchorToken);
            } else {
                int arity = 0;
                foreach(var atribute in node [0]){
                    arity++;
                }
                var usedFun = funTable[name];
                if(arity != (int) usedFun.ElementAt(1)){
                    throw new SemanticError("Incorrect match of parameters: " + name, node.AnchorToken);
                }
            }
            Visit((dynamic) node[0]);
        }

        public void Visit(Expr_List node) {
            VisitChildren(node);
        }

        public void Visit(Identifier node) {
            var name = node.AnchorToken.Lexeme;
            if (firstVisit){
                if(globalTable.Contains(name)){
                    throw new SemanticError("Duplicated global variable: " + name, node.AnchorToken);
                } else {
                    
                    globalTable[name] = name;
                }
            } else {
                if (localTable.Contains(name)){
                    
                    throw new SemanticError("Duplicated local variable: " + name, node.AnchorToken);
                } else {
                    
                    localTable[name] = ParamIs;
                }
            }
        }

        public void Visit(Int_Literal node) {
            var intText = node.AnchorToken.Lexeme;
            try{
                Convert.ToInt32(intText);
            } catch (OverflowException) {
                throw new SemanticError("Too big: " + intText, node.AnchorToken);
            }
        }

        public void Visit(Not node) {
            VisitChildren(node);
        }

        public void Visit(And node) {
            VisitChildren(node);
        }

        public void Visit(Or node) {
            VisitChildren(node);
        }
        

        public void Visit(More_Than node) {
            VisitChildren(node);
        }

        public void Visit(More_Equal node) {
            VisitChildren(node);
        }

        public void Visit(Less_Than node) {
            VisitChildren(node);
        }

        public void Visit(Less_Equal node) {
            VisitChildren(node);
        }

        public void Visit(Different node) {
            VisitChildren(node);
        }

        public void Visit(Compare node) {
            VisitChildren(node);
        }

        public void Visit(StatementInc node) {
            Visit((dynamic) node[0]);
        }

        public void Visit(StatementDec node) {
            Visit((dynamic) node[0]);
        }

        public void Visit(Plus node) {
            VisitChildren(node);
        }

        public void Visit(Minus node) {
            VisitChildren(node);
        }
        public void Visit(Multiply node) {
            VisitChildren(node);
        }
        public void Visit(Mod node) {
            VisitChildren(node);
        }
        public void Visit(Div node) {
            VisitChildren(node);
        }

        public void Visit(Var_Refer node){
            var name = node.AnchorToken.Lexeme;
            if(!localTable.Contains(name) && !globalTable.Contains(name)){
                throw new SemanticError("Undeclared variable reference: " + name, node.AnchorToken);
            }
        }

        public void Visit(Array node) {
            VisitChildren(node);
        }

        void VisitChildren(Node node) {
            foreach (var n in node) {
                Visit((dynamic) n);
            }
        }

        //Jared Fogle safe
        public void Visit(CharLit node) {
            //just consumes
        }

        public void Visit(String_Lit node) {
            //just consumes
        }

        public void Visit(True node) {
            // just consumes
        }

        public void Visit(False node) {
            // just consumes
        }

        public void Visit(StatementSemiColon node) {
            // just consumes
        }

    }
}
