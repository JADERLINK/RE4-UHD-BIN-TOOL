﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHARED_UHD_BIN;

namespace RE4_UHD_BIN_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAction.MainContinue(args, false, SimpleEndianBinaryIO.Endianness.LittleEndian);
        }
    }
}
