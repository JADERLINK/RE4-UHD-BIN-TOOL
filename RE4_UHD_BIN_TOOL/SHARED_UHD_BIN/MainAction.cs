﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SimpleEndianBinaryIO;

namespace SHARED_UHD_BIN
{
    public static class MainAction
    {
        public static void MainContinue(string[] args, bool isPS4NS, Endianness endianness) 
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.WriteLine(Shared.HeaderText());

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4-UHD-BIN-TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else if (args.Length >= 1 && File.Exists(args[0]))
            {
                try
                {
                    //FileInfo
                    FileInfo fileInfo1 = new FileInfo(args[0]);
                    FileInfo fileInfo2 = null;

                    //extension
                    string file1Extension = fileInfo1.Extension.ToUpperInvariant();
                    string file2Extension = null;

                    Console.WriteLine("File1: " + fileInfo1.Name);

                    //verrifica o file2
                    if (args.Length >= 2 && File.Exists(args[1]))
                    {
                        fileInfo2 = new FileInfo(args[1]);
                        file2Extension = fileInfo2.Extension.ToUpperInvariant();
                        Console.WriteLine("File2: " + fileInfo2.Name);
                    }

                    Actions(fileInfo1, file1Extension, fileInfo2, file2Extension, isPS4NS, endianness);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error1: " + ex);
                }

            }
            else
            {
                Console.WriteLine("File specified does not exist.");
            }

            Console.WriteLine("Finished!!!");

        }

        private static void Actions(FileInfo fileInfo1, string file1Extension, FileInfo fileInfo2, string file2Extension, bool isPS4NS, Endianness endianness)
        {
            //diretorio, e nome do arquivo
            string baseDirectory = fileInfo1.DirectoryName + "\\";
            string baseName = Path.GetFileNameWithoutExtension(fileInfo1.Name);

            //stream
            Stream binFile = null;
            Stream tplFile = null;

            Stream idxuhdtplFile = null;
            Stream idxmaterialFile = null;

            Stream idxuhdBinFile = null;
            Stream objFile = null;
            Stream smdFile = null;
            Stream mtlFile = null;


            // verifica arquivos e posibiliades

            // arquivo 1
            switch (file1Extension)
            {
                case ".BIN":
                    binFile = fileInfo1.OpenRead();
                    break;
                case ".TPL":
                    tplFile = fileInfo1.OpenRead();
                    break;
                case ".OBJ":
                    objFile = fileInfo1.OpenRead();
                    break;
                case ".SMD":
                    smdFile = fileInfo1.OpenRead();
                    break;
                case ".MTL":
                    mtlFile = fileInfo1.OpenRead();
                    break;
                case ".IDXUHDBIN":
                case ".IDXUHDBINBIG":
                    Console.WriteLine("Pass the .OBJ or .SMD file as a parameter.");
                    return;
                case ".IDXUHDTPL":
                    idxuhdtplFile = fileInfo1.OpenRead();
                    break;
                case ".IDXMATERIAL":
                    idxmaterialFile = fileInfo1.OpenRead();
                    break;
                default:
                    Console.WriteLine("the file format is invalid: " + fileInfo1.Name);
                    return;
            }

            //arquivo 2

            if (file2Extension != null)
            {
                switch (file2Extension)
                {
                    case ".BIN":
                    case ".OBJ":
                    case ".SMD":
                        Console.WriteLine("pass this file as the first parameter: " + fileInfo2.Name);
                        return;
                    case ".IDXUHDBIN":
                    case ".IDXUHDBINBIG":
                        Console.WriteLine("Pass the .OBJ or .SMD file as a parameter.");
                        return;
                    case ".TPL":
                        if (idxuhdtplFile != null)
                        {
                            Console.WriteLine("A .Tpl and .IdxUhdTpl file together per parameter cannot be passed.");
                            return;
                        }
                        else if (tplFile == null)
                        {
                            tplFile = fileInfo2.OpenRead();
                        }
                        else
                        {
                            Console.WriteLine("Cannot have two .TPL files");
                            return;
                        }
                        break;
                    case ".IDXUHDTPL":
                        if (tplFile != null)
                        {
                            Console.WriteLine("A .Tpl and .IdxUhdTpl file together per parameter cannot be passed.");
                            return;
                        }
                        if (idxuhdtplFile == null)
                        {
                            idxuhdtplFile = fileInfo2.OpenRead();
                        }
                        else
                        {
                            Console.WriteLine("Cannot have two .IdxUhdTpl files.");
                            return;
                        }
                        break;
                    case ".IDXMATERIAL":
                        if (binFile != null)
                        {
                            Console.WriteLine("You cannot pass an .IdxMaterial together with a .Bin file.");
                            return;
                        }
                        else if (mtlFile != null)
                        {
                            Console.WriteLine("You cannot pass an .IdxMaterial together with a .Mtl file.");
                            return;
                        }
                        else if (idxmaterialFile == null)
                        {
                            idxmaterialFile = fileInfo2.OpenRead();
                        }
                        else
                        {
                            Console.WriteLine("Cannot have two .IdxMaterial files.");
                            return;
                        }
                        break;
                    case ".MTL":
                        if (binFile != null)
                        {
                            Console.WriteLine("You cannot pass an .MTL together with a .Bin file.");
                            return;
                        }
                        else if (idxmaterialFile != null)
                        {
                            Console.WriteLine("You cannot pass an .MTL together with a .IdxMaterial file.");
                            return;
                        }
                        else if (mtlFile == null)
                        {
                            mtlFile = fileInfo2.OpenRead();
                        }
                        else
                        {
                            Console.WriteLine("Cannot have two .MTL files.");
                            return;
                        }
                        break;
                    default:
                        Console.WriteLine("the file format is invalid: " + fileInfo2.Name);
                        return;
                }
            }


            //carregando arquivos adicionais
            switch (file1Extension)
            {
                case ".BIN":
                    // tenta caregar um tpl de mesmo nome, caso não tenha sido declarado um
                    // se não tiver fica sem mesmo.
                    if (tplFile == null && idxuhdtplFile == null)
                    {
                        string tplFilePath = baseDirectory + baseName + ".tpl";
                        if (File.Exists(tplFilePath))
                        {
                            Console.WriteLine("Load File: " + baseName + ".tpl");
                            tplFile = new FileInfo(tplFilePath).OpenRead();
                        }
                    }
                    break;
                case ".OBJ":
                case ".SMD":
                    // modo repack, tem que carregar o IDXUHDBIN
                    // requisita tambem o .MTL, caso não tenha sido declarado um .IdxMaterial
                    // (removido essa função) opcionalmente caso seja um mtl, pode carregar o IDXUHDTPL (a não ser tenha sido pasado um .tpl como segundo parametro)

                    string idxbinFormat = ".idxuhdbin";
                    if (endianness == Endianness.BigEndian)
                    {
                        idxbinFormat = ".idxuhdbinbig";
                    }
                    string idxbinFilePath = baseDirectory + baseName + idxbinFormat;
                    if (File.Exists(idxbinFilePath))
                    {
                        Console.WriteLine("Load File: " + baseName + idxbinFormat);
                        idxuhdBinFile = new FileInfo(idxbinFilePath).OpenRead();
                    }
                    else
                    {
                        Console.WriteLine($"{idxbinFormat} file does not exist, it is necessary to repack the .Bin");
                        return;
                    }

                    // versão com mtl 
                    if (idxmaterialFile == null)
                    {
                        if (mtlFile == null)
                        {
                            string mtlFilePath = baseDirectory + baseName + ".mtl";
                            if (File.Exists(mtlFilePath))
                            {
                                Console.WriteLine("Load File: " + baseName + ".mtl");
                                mtlFile = new FileInfo(mtlFilePath).OpenRead();
                            }
                            else
                            {
                                Console.WriteLine(".MTL file does not exist, it is necessary to repack the .Bin or pass the .idxmaterial file as the second parameter");
                                return;
                            }

                        }

                        /*
                        if (tplFile == null && idxuhdtplFile == null)
                        {
                            string idxtplFilePath = baseDirectory + baseName + ".IdxUhdTpl";
                            if (File.Exists(idxtplFilePath))
                            {
                                Console.WriteLine("Load File: " + baseName + ".IdxUhdTpl");
                                idxuhdtplFile = new FileInfo(idxtplFilePath).OpenRead();
                            }

                        } 
                        */

                    }


                    break;
                case ".IDXMATERIAL":
                    // não pode ter um idxmaterial sozinho
                    if (file2Extension == null)
                    {
                        Console.WriteLine("Only the .IdxMaterial file cannot be passed as a parameter, also pass an .Idxuhdtpl or .tpl file");
                        return;
                    }
                    break;
                default:
                    break;
            }

            //-----------
            //carrega os objetos arquivos.


            EXTRACT.UhdBIN uhdBIN = null;
            EXTRACT.UhdTPL uhdTPL = null;
            ALL.IdxMaterial material = null;

            REPACK.IdxUhdBin idxbin = null;

            ALL.IdxMtl idxMtl = null;

            if (binFile != null) //.BIN
            {
                uhdBIN = EXTRACT.UhdBinDecoder.Decoder(binFile, 0, out _, isPS4NS, endianness);
                material = ALL.IdxMaterialParser.Parser(uhdBIN);
                binFile.Close();
            }

            if (tplFile != null) //.TPL
            {
                uhdTPL = EXTRACT.UhdTplDecoder.Decoder(tplFile, 0, out _, isPS4NS, endianness);
                tplFile.Close();
            }

            if (idxuhdtplFile != null) // .IDXUHDTPL
            {
                uhdTPL = ALL.IdxUhdTplLoad.Load(idxuhdtplFile);
                idxuhdtplFile.Close();
            }

            if (idxmaterialFile != null) //.IDXMATERIAL
            {
                material = ALL.IdxMaterialLoad.Load(idxmaterialFile);
                idxmaterialFile.Close();
            }

            if (idxuhdBinFile != null) // .IDXUHDBIN
            {
                idxbin = REPACK.IdxUhdBinLoad.Load(idxuhdBinFile, endianness);
                idxuhdBinFile.Close();
            }

            if (mtlFile != null) // .MTL
            {
                REPACK.MtlLoad.Load(mtlFile, out idxMtl);
            }

            // cria arquivos


            if (file1Extension == ".BIN") // modo extract
            {
                EXTRACT.OutputFiles.CreateSMD(uhdBIN, baseDirectory, baseName);
                EXTRACT.OutputFiles.CreateOBJ(uhdBIN, baseDirectory, baseName);
                EXTRACT.OutputFiles.CreateIdxUhdBin(uhdBIN, baseDirectory, baseName, endianness);
                EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseName);

                if (tplFile != null) // cria somente um arquivo .idxuhdtpl somente se a origem for .tpl
                {
                    EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseName);
                }
                if (uhdTPL != null) // caso tiver o conteudo de .tpl/.idxuhdtpl, é criado o .mtl
                {
                    var _idxMtl = ALL.IdxMtlParser.Parser(material, uhdTPL, isPS4NS);
                    EXTRACT.OutputMaterial.CreateMTL(_idxMtl, baseDirectory, baseName);
                }
            }
            else if (file1Extension == ".OBJ") //repack with obj
            {
                if (idxMtl != null)
                {
                    new REPACK.MtlConverter(baseDirectory).Convert(idxMtl, ref uhdTPL, out material);

                    EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseName + ".Repack");
                    EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseName + ".Repack");
                }

                REPACK.FinalBoneLine[] boneLines = idxbin.Bones;
                REPACK.Structures.FinalStructure final = null;

                {
                    int ObjFileUseBone = idxbin.ObjFileUseBone;
                    bool CompressVertices = idxbin.CompressVertices;
                    REPACK.Structures.IntermediaryStructure intermediaryStructure = null;
                    REPACK.BinRepack.RepackOBJ(objFile, CompressVertices, ObjFileUseBone, out intermediaryStructure, idxbin.UseExtendedNormals, idxbin.UseVertexColor);
                    REPACK.Structures.IntermediaryLevel2 level2 = REPACK.BinRepack.MakeIntermediaryLevel2(intermediaryStructure);
                    final = REPACK.BinRepack.MakeFinalStructure(level2);
                }

                //checa limite de vertives
                if (final.Vertex_Position_Array.Length > ushort.MaxValue)
                {
                    Console.WriteLine("Warning: Number of vertices greater than the limit: " + final.Vertex_Position_Array.Length + ";");
                    Console.WriteLine("The limit is: " + ushort.MaxValue + ";");
                    Console.WriteLine("If this .BIN is not used in a Scenario SMD, it will not work correctly;");
                }

                // cria arquivos
                string binFilePath = baseDirectory + baseName + ".bin";
                Stream binstream = File.Open(binFilePath, FileMode.Create);
                REPACK.BINmakeFile.MakeFile(binstream, 0, out _, final, boneLines, material,
                    idxbin.BonePairLines, idxbin.UseExtendedNormals, idxbin.UseWeightMap, idxbin.EnableBonepairTag, 
                    idxbin.EnableAdjacentBoneTag, idxbin.UseVertexColor, isPS4NS, endianness);
                binstream.Close();

                if (uhdTPL != null)
                {
                    string tplFilePath = baseDirectory + baseName + ".tpl";
                    Stream tplstream = File.Open(tplFilePath, FileMode.Create);
                    REPACK.TPLmakeFile.MakeFile(uhdTPL, tplstream, 0, out _, isPS4NS, endianness);
                    tplstream.Close();
                }
            }
            else if (file1Extension == ".SMD") //repack with smd
            {
                if (idxMtl != null)
                {
                    new REPACK.MtlConverter(baseDirectory).Convert(idxMtl, ref uhdTPL, out material);

                    EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseName + ".Repack");
                    EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseName + ".Repack");
                }

                REPACK.FinalBoneLine[] boneLines = null;
                REPACK.Structures.FinalStructure final = null;

                {
                    bool CompressVertices = idxbin.CompressVertices;
                    REPACK.Structures.IntermediaryStructure intermediaryStructure = null;
                    REPACK.BinRepack.RepackSMD(smdFile, CompressVertices, out intermediaryStructure, out boneLines, idxbin.UseExtendedNormals, endianness);
                    REPACK.Structures.IntermediaryLevel2 level2 = REPACK.BinRepack.MakeIntermediaryLevel2(intermediaryStructure);
                    final = REPACK.BinRepack.MakeFinalStructure(level2);
                }

                //checa limite de vertives
                if (final.Vertex_Position_Array.Length > ushort.MaxValue)
                {
                    Console.WriteLine("Warning: Number of vertices greater than the limit: " + final.Vertex_Position_Array.Length + ";");
                    Console.WriteLine("The limit is: " + ushort.MaxValue + ";");
                    Console.WriteLine("If this .BIN is not used in a Scenario SMD, it will not work correctly;");
                }

                // checa limite da combinações de pesos (WeightMap)
                if (final.WeightMaps.Length > byte.MaxValue)
                {
                    Console.WriteLine("Warning: Number of WeightMap combinations greater than limit: " + final.WeightMaps.Length + ";");
                    Console.WriteLine("The limit is: " + byte.MaxValue + ";");
                    Console.WriteLine("This BIN file in the base game will crash.");
                    Console.WriteLine("It will only work if you are using the Qingsheng DLL (X3DAudio1_7.dll),");
                    Console.WriteLine("with \"Allocate more memory for bones\" enabled.");
                }

                // cria arquivos
                string binFilePath = baseDirectory + baseName + ".bin";
                Stream binstream = File.Open(binFilePath, FileMode.Create);
                REPACK.BINmakeFile.MakeFile(binstream, 0, out _, final, boneLines, material,
                    idxbin.BonePairLines, idxbin.UseExtendedNormals, idxbin.UseWeightMap, idxbin.EnableBonepairTag,
                    idxbin.EnableAdjacentBoneTag, false, isPS4NS, endianness);
                binstream.Close();

                if (uhdTPL != null)
                {
                    string tplFilePath = baseDirectory + baseName + ".tpl";
                    Stream tplstream = File.Open(tplFilePath, FileMode.Create);
                    REPACK.TPLmakeFile.MakeFile(uhdTPL, tplstream, 0, out _, isPS4NS, endianness);
                    tplstream.Close();
                }

            }
            else if (file1Extension == ".TPL" && file2Extension == null) // extrai so o arquivo tpl
            {
                EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseName);
            }
            else if (file1Extension == ".IDXUHDTPL" && file2Extension == null) // faz repack do arquivo .idxuhdtpl
            {
                string tplFilePath = baseDirectory + baseName + ".tpl";
                Stream stream = File.Open(tplFilePath, FileMode.Create);
                REPACK.TPLmakeFile.MakeFile(uhdTPL, stream, 0, out _, isPS4NS, endianness);
                stream.Close();
            }

            // outras situações
            else if (uhdTPL != null && material != null) // cria um .mtl com o tpl e idxmaterial
            {
                var _idxMtl = ALL.IdxMtlParser.Parser(material, uhdTPL, isPS4NS);
                EXTRACT.OutputMaterial.CreateMTL(_idxMtl, baseDirectory, baseName);
            }
            else if (idxMtl != null) // cria idxMaterial derivado do .mtl (pode usar o .tpl/.idxuhdtpl)
            {
                new REPACK.MtlConverter(baseDirectory).Convert(idxMtl, ref uhdTPL, out material);

                EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseName + ".Repack");
                EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseName + ".Repack");
            }


            // stream.Close()
            // objFile, smdFile, mtlFile são fechados dentro dos metodos.

        }

    }
}
