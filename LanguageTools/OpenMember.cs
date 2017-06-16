﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBMiCmd.LanguageTools
{
    class OpenMembers
    {
        private static Dictionary<string, OpenMember> _Members = new Dictionary<string, OpenMember>();

        public static void AddMember(string Local, string Lib, string Obj, string Mbr)
        {
            if (!Contains(Local))
            {
                _Members.Add(Local, new OpenMember(Local, Lib, Obj, Mbr));
            }
        }

        public static Boolean Contains(String Local)
        {
            return _Members.ContainsKey(Local);
        }

        public static OpenMember GetMember(String Local)
        {
            return _Members[Local];
        }
    }

    class OpenMember
    {
        private string _Local;
        private string _Lib;
        private string _Obj;
        private string _Mbr;

        public OpenMember(string Local, string Lib, string Obj, string Mbr)
        {
            this._Local = Local;
            this._Lib = Lib;
            this._Obj = Obj;
            this._Mbr = Mbr;
        }

        public string GetLocalFile()
        {
            return this._Local;
        }

        public string GetLibrary()
        {
            return this._Lib;
        }

        public string GetObject()
        {
            return this._Obj;
        }

        public string GetMember()
        {
            return this._Mbr;
        }
    }
}