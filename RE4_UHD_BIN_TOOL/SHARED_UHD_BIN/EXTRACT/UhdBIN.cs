﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHARED_UHD_BIN.ALL;
using SimpleEndianBinaryIO;

namespace SHARED_UHD_BIN.EXTRACT
{
    public class UhdBIN
    {
        public UhdBinHeader Header;

        //----
        public (float vx, float vy, float vz)[] Vertex_Position_Array;
        public (float nx, float ny, float nz)[] Vertex_Normal_Array;
        public (float tu, float tv)[] Vertex_UV_Array;
        public (byte a, byte r, byte g, byte b)[] Vertex_Color_Array;
        //------

        public Bone[] Bones; //sempre tem

        public WeightMap[] WeightMaps; // nem sempre tem

        //Weight index
        public ushort[] WeightIndex; // nem sempre tem
        public ushort[] Weight2Index; // nem sempre tem,  segundo byte é sempre zero, em geral é igual ao de cima

        // é igual ao bin de ps2, mas não sei oque é isso.
        public byte[][] bonepairLines; // nem sempre tem

        //adjacent_bone, não sei oque é isso (é em big endian)
        public ushort[] adjacent_bone;

        //material
        public MaterialBin[] Materials;
    }


    public class MaterialBin
    {
        public MaterialPart material;

        public (int i1, int i2, int i3)[] face_index_array;
    }

    public class WeightMap
    {
        // correção: é sempre byte
        // o jogo base so aceita so 255 combinações desse objeto(WeightMap)
        // para ter mais que 255 tem que usar a DLL do qingsheng
        // X3DAudio1_7.dll
        // no "X3DAudio1_7.ini" mudar "Allocate more memory for bones" de 0 para 1
        // atenção: nos arquivos r10c_27.BIN r22a_27.BIN, os bones são ushort, mas essa logica não funciona para os outros arquivos.
        public byte boneId1; //pode ser ushort ou byte
        public byte boneId2; //pode ser ushort ou byte
        public byte boneId3; //pode ser ushort ou byte
        public byte count;   //pode ser ushort ou byte
        public byte weight1;
        public byte weight2;
        public byte weight3;
        public byte unk018;
    }

    public class Bone
    {
        private Endianness endianness;

        public Bone(Endianness endianness) 
        {
            this.endianness = endianness;
        }

        // new byte[16];
        public byte[] boneLine;

        public byte BoneID { get { return boneLine[0x0]; } }
        public byte BoneParent { get { return boneLine[0x1]; } }

        public float PositionX { get { return EndianBitConverter.ToSingle(boneLine, 0x4, endianness); } }
        public float PositionY { get { return EndianBitConverter.ToSingle(boneLine, 0x8, endianness); } }
        public float PositionZ { get { return EndianBitConverter.ToSingle(boneLine, 0xC, endianness); } }

    }


    public class UhdBinHeader
    {
        public uint bone_offset; // magic: 60 00 00 00
        public uint unknown_x04; //--zeros
        public uint unknown_x08; // offset: 50 00 00 00 // address is present when adjacent_meshinfo_addr is present {but address goes to blank area}
        public uint vertex_colour_offset; //colors


        public uint vertex_texcoord_offset; // UV
        public uint weight_offset;
        public byte weight_count;
        public byte bone_count;
        public ushort material_count;
        public uint material_offset;

        public ushort texture1_flags;
        public ushort texture2_flags;
        public uint TPL_count; // quantidade de id de tpl usados
        public byte vertex_scale; // a descobrir oque é
        public byte unknown_x29;// a verificar
        public ushort weight2_count; //--same as weightcount
        public uint morph_offset; // não foi feito nada com isso ainda.

        public uint vertex_position_offset;
        public uint vertex_normal_offset;
        public ushort vertex_position_count;
        public ushort vertex_normal_count;
        public uint version_flags;

        public uint bonepair_offset; //unk005_addr
        public uint adjacent_offset; //adjacent_addr
        public uint vertex_weight_index_offset;  //  //--vertex weights id's array (2 words ) * numvertex
        public uint vertex_weight2_index_offset; //  //--vertex weights array  (2 words ) * numvertex
    }


}
