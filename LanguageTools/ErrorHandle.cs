﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IBMiCmd.IBMiTools;

namespace IBMiCmd.LanguageTools
{
    class ErrorHandle
    {
        private static string _name = "";
        private static string[] _Lines;

        private static int _FileID;
        private static Dictionary<int, string> _FileIDs;
        private static Dictionary<int, List<LineError>> _Errors;
        private static Dictionary<int, List<expRange>> _Expansions;

        public static void getErrors(string lib, string obj)
        {
            string filetemp = Path.GetTempFileName();

            List<string> commands = new List<string>();

            lib = lib.Trim().ToUpper();
            obj = obj.Trim().ToUpper();

            if (lib == "*CURLIB") lib = IBMi.GetConfig("curlib");

            commands.Add("ASCII");
            commands.Add("quote type b 1"); //SJIS
            commands.Add("cd /QSYS.lib");
            commands.Add("recv \"" + lib + ".lib/EVFEVENT.file/" + obj + ".mbr\" \"" + filetemp + "\"");

            IBMi.RunCommands(commands.ToArray());

            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("shift_jis");   //SJIS
            ErrorHandle.doName(lib.ToUpper() + '/' + obj.ToUpper());
            ErrorHandle.setLines(File.ReadAllLines(filetemp, enc)); //SJIS
        }

        public static string doName(string newName = "")
        {
            if (newName != "") _name = newName;

            return _name;
        }

        public static void setLines(string[] data)
        {
            _Lines = data;
            wrkErrors();
        }

        public static void wrkErrors()
        {
            _FileIDs = new Dictionary<int, string>();
            _Errors = new Dictionary<int, List<LineError>>();
            _Expansions = new Dictionary<int, List<expRange>>();

            string err;
            int sev;
            int linenum, column, sqldiff;
            
            string[] pieces;
            string curtype;

            foreach (string line in _Lines)
            {
                if (line == null) continue;
                err = line.PadRight(150);
                pieces = err.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                curtype = err.Substring(0, 10).TrimEnd();
                _FileID = int.Parse(line.Substring(13, 3));
                switch (curtype)
                {
                    case "FILEID":
                        if (_FileIDs.ContainsKey(_FileID))
                        {
                            //_FileIDs[_FileID] = pieces[5];
                        }
                        else
                        {
                            _FileIDs.Add(_FileID, pieces[5]);
                            _Errors.Add(_FileID, new List<LineError>());
                            _Expansions.Add(_FileID, new List<expRange>());
                        }
                        break;

                    case "EXPANSION":
                        _Expansions[_FileID].Add(new expRange(int.Parse(pieces[6]), int.Parse(pieces[7])));
                        break;

                    case "ERROR":
                        sev = int.Parse(err.Substring(58, 2));
                        linenum = int.Parse(err.Substring(37, 6));
                        column = int.Parse(err.Substring(33, 3));
                        sqldiff = 0;

                        if (sev >= 20)
                        {
                            foreach (expRange range in _Expansions[_FileID])
                            {
                                if (range.inRange(linenum))
                                {
                                    sqldiff += range.getVal();
                                }
                            }
                        }

                        if (sqldiff > 0)
                        {
                            linenum -= sqldiff;
                        }

                        _Errors[_FileID].Add(new LineError(sev, linenum, column, err.Substring(65), err.Substring(48, 7)));
                        break;
                }
            }
        }

        public static int[] getFileIDs()
        {
            return _FileIDs.Keys.ToArray();
        }

        public static string getFilePath(int fileid)
        {
            return _FileIDs[fileid];
        }

        public static LineError[] getErrors(int fileid)
        {
            return _Errors[fileid].ToArray();
        }
    }

    class expRange
    {
        public int _low;
        public int _high;

        public expRange(int low, int high)
        {
            _low = low;
            _high = high;
        }

        public bool inRange(int num)
        {
            return (num >= _high);
        }

        public int getVal()
        {
            return (_high - _low) + 1;
        }
    }
}
