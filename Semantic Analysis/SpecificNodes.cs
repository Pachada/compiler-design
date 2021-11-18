/*
  Falak compiler - Token categories for the scanner.
  Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM

*/

namespace Falak
{

    class Program : Node { }

    class DefList : Node { }

    class FunDef : Node { }

    class Var_DefList : Node { }

    class ParamList : Node { }

    class VarList : Node { }

    class Var_Def : Node { }

    class StatementList : Node { }

    class StatementAssign : Node { }

    class StatementInc : Node { }

    class StatementDec : Node { }

    class StatementIf : Node { }

    class Else_If : Node { }

    class Node_Else : Node { }

    class StatementWhile : Node { }

    class StatementDo : Node { }

    class StatementBreak : Node { }

    class StatementReturn : Node { }

    class Empty : Node { }

    class Expr_List : Node { }

    class Or : Node { }

    class Xor : Node { }

    class And : Node { }

    class Compare : Node { }

    class Different : Node { }

    class More_Than : Node { }

    class More_Equal : Node { }

    class Less_Than : Node { }

    class Less_Equal : Node { }

    class Multiply : Node { }

    class Div : Node { }

    class Mod : Node { }

    class Plus : Node { }

    class Minus : Node { }

    class Positive : Node { }

    class Negative : Node { }

    class Not : Node { }

    class Fun_Call : Node { }

    class Var_Refer : Node { }

    class True : Node { }

    class False : Node { }

    class Int_Literal : Node { }

    class CharLit : Node { }

    class String_Lit : Node { }

    class Array : Node { }

}