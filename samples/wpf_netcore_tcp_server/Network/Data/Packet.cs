﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingNetwork.Data
{
    [Serializable]
    public partial class Packet
    {
        public int From;
        public int To;
        public byte[] Data;
    }
}
