/*
    Falak compiler 
    Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/

using System;
using System.Text;
using System.Collections.Generic;

namespace Falak
{

    class Type
    {
        public bool primitive;

        public int arguments;
        public HashSet<string> localTable;

        public Type(bool primitive, int arguments)
        {
            this.primitive = primitive;
            this.arguments = arguments;
            this.localTable = new HashSet<string>();

        }

        public void setPrimitive(bool primitive)
        {
            this.primitive = primitive;
        }

        public void setArguments(int arguments)
        {
            this.arguments = arguments;
        }

        public bool isPrimitive()
        {
            return this.primitive;
        }

        public int getArguments()
        {
            return this.arguments;
        }

        public HashSet<string> getLocalTable()
        {
            return this.localTable;
        }

    }
}
