using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace BabelTrialRemover
{
    class Program
    {
        static ModuleDefMD asm;

        static void removeTrial()
        {
            foreach (TypeDef type in asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    Local dateTimeVar = null;
                    bool nextMethod = false;
                    if (methods.Body.Instructions[0].OpCode.Equals(OpCodes.Ldloca_S) && ((Local)methods.Body.Instructions[0].Operand).Type.FullName == "System.DateTime") { dateTimeVar = (Local)methods.Body.Instructions[0].Operand; }
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count; x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        switch (inst.OpCode.Code)
                        {
                            case Code.Call:
                                if (inst.Operand.ToString().Contains("System.TimeSpan::get_TotalDays()"))
                                {
                                    if (methods.Body.Instructions[x + 1].OpCode.Equals(OpCodes.Ldc_R8) && methods.Body.Instructions[x + 1].Operand.ToString() == "0" && methods.Body.Instructions[x + 2].Operand is Instruction)
                                    {
                                        Instruction endifVar = methods.Body.Instructions[x + 2];
                                    endifVar_:
                                        if (endifVar.OpCode.Equals(OpCodes.Bge_Un_S) || endifVar.OpCode.Equals(OpCodes.Br) || endifVar.OpCode.Equals(OpCodes.Br_S))
                                        {
                                            endifVar = (Instruction)endifVar.Operand;
                                            goto endifVar_;
                                        }
                                    //List<Instruction> keepInsts = new List<Instruction>();
                                        //for (int x_ = x + 3; x_ < methods.Body.Instructions.IndexOf(endifVar); x_++)
                                        //{
                                        //    keepInsts.Add(methods.Body.Instructions[x_]);
                                        //}
                                        int temp_int = methods.Body.Instructions.IndexOf(endifVar);
                                        for (int x_ = 0; x_ < temp_int; x_++)
                                        {
                                            methods.Body.Instructions.RemoveAt(0);
                                        }
                                        //keepInsts.Reverse();
                                        //foreach (Instruction inst_ in keepInsts)
                                        //{
                                        //    methods.Body.Instructions.Insert(0, inst_);
                                        //}
                                        nextMethod = true;
                                    }
                                }
                                break;
                        }
                        if (nextMethod) { break; }
                    }
                }
            }
        }

        static void disableTrial(bool notrial)
        {
            foreach (TypeDef type in asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    Local dateTimeVar = null;
                    bool nextMethod = false;
                    bool inverted = false;
                    if (methods.Body.Instructions[0].OpCode.Equals(OpCodes.Ldloca_S) && ((Local)methods.Body.Instructions[0].Operand).Type.FullName == "System.DateTime") { dateTimeVar = (Local)methods.Body.Instructions[0].Operand; }
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count; x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        switch (inst.OpCode.Code)
                        {
                            case Code.Call:
                                if (inst.Operand.ToString().Contains("System.DateTime::get_Now()"))
                                {
                                    if (!methods.Body.Instructions[x + 1].OpCode.Equals(OpCodes.Call))
                                    {
                                        inverted = true;
                                    }
                                }
                                if (inst.Operand.ToString().Contains("System.TimeSpan::get_TotalDays()"))
                                {
                                    if (methods.Body.Instructions[x + 1].OpCode.Equals(OpCodes.Ldc_R8) && methods.Body.Instructions[x + 1].Operand.ToString() == "0" && methods.Body.Instructions[x + 2].Operand is Instruction)
                                    {
                                        if (notrial)
                                        {
                                            if (inverted) { methods.Body.Instructions[x + 1].Operand = double.PositiveInfinity; }
                                            else { methods.Body.Instructions[x + 1].Operand = double.NegativeInfinity; }
                                        }
                                        else { methods.Body.Instructions[x + 1].Operand = Convert.ToDouble(0); }
                                        nextMethod = true;
                                    }
                                }
                                break;
                        }
                        if (nextMethod) { break; }
                    }
                }
            }
        }

        static Local getLdlocLocal(Instruction inst, MethodDef methods)
        {
            switch (inst.OpCode.Code)
            {
                case Code.Ldloc_0:
                    return methods.Body.Variables[0];
                case Code.Ldloc_1:
                    return methods.Body.Variables[1];
                case Code.Ldloc_2:
                    return methods.Body.Variables[2];
                case Code.Ldloc_3:
                    return methods.Body.Variables[3];
                default:
                    try { return (Local)inst.Operand; }
                    catch { return null; }
            }
        }

        static void PrintOption(char number, string info, string notes, ConsoleColor infoColor, ConsoleColor notesColor)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   ╠═ ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(number);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = infoColor;
            Console.Write(info);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ╠═ ");
            Console.ForegroundColor = notesColor;
            Console.WriteLine(notes);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void Main(string[] args)
        {
            str:
            Console.Title = "BabelTrialRemover";
            Console.WriteLine();
            Console.WriteLine(" BabelTrialRemover | Remove/Disable Babel Obfuscator 30 Day limit from apps");
            Console.WriteLine("  |- https://github.com/miso-xyz/BabelTrialRemover");
            Console.WriteLine();
            try { asm = ModuleDefMD.Load(args[0]); }
            catch { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(" Invalid file, make sure it is a valid .NET Application!"); goto end; }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Please Select:");
            PrintOption('1', "Remove 30 Day Limit ", "Unstable, will likely brick input assembly", ConsoleColor.Magenta, ConsoleColor.DarkRed);
            //Console.WriteLine("   |--------------------------|-ENABLE/DISABLE-30-DAY-LIMIT--------------------");
            Console.WriteLine("   ╠══════════════════════════╬═ENABLE/DISABLE-30-DAY-LIMIT═══════════════════►");
            PrintOption('2', "Disable 30 Day Limit", "Stable, works perfectly", ConsoleColor.DarkCyan, ConsoleColor.DarkGreen);
            PrintOption('3', "Enable 30 Day Limit ", "Stable, works perfectly", ConsoleColor.DarkRed, ConsoleColor.DarkGreen);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("   Note: Only works if the trial code is still present");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.Write("  > ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            ConsoleKey key_ = Console.ReadKey().Key;
            Console.ForegroundColor = ConsoleColor.White;
            if (key_ != ConsoleKey.NumPad1 && key_ != ConsoleKey.D1 && key_ != ConsoleKey.NumPad2 && key_ != ConsoleKey.D2 && key_ != ConsoleKey.NumPad3 && key_ != ConsoleKey.D3)
            {
                Console.ResetColor();
                Console.Clear();
                goto str;
            }
            Console.WriteLine();
            string split = " ◄";
            for (int x = 2; x < Console.WindowWidth - 2; x++)
            {
                split += "═";
            }
            split += "►";
            Console.WriteLine();
            Console.WriteLine(split);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            switch (key_)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Console.WriteLine(" Removing 30 Day Trial Limit...");
                    removeTrial();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Console.WriteLine(" Disabling 30 Day Trial Limit...");
                    disableTrial(true);
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    Console.WriteLine(" Enabling 30 Day Trial Limit...");
                    disableTrial(false);
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Finished!");
            Console.WriteLine();
            ModuleWriterOptions moduleWriterOptions = new ModuleWriterOptions(asm);
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            NativeModuleWriterOptions nativeModuleWriterOptions = new NativeModuleWriterOptions(asm, true);
            nativeModuleWriterOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            nativeModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" Now saving '" + Path.GetFileNameWithoutExtension(args[0]) + "-BabelTrialRemover" + Path.GetExtension(args[0]) + "'!");
            if (asm.IsILOnly) { asm.Write(Path.GetFileNameWithoutExtension(args[0]) + "-BabelTrialRemover" + Path.GetExtension(args[0]), moduleWriterOptions); } else { asm.NativeWrite(Path.GetFileNameWithoutExtension(args[0]) + "-BabelTrialRemover" + Path.GetExtension(args[0])); }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Successfully saved '" + Path.GetFileNameWithoutExtension(args[0]) + "-BabelTrialRemover" + Path.GetExtension(args[0]) + "'!");
        end:
            Console.ResetColor();
            Console.WriteLine();
            Console.Write(" Press any key to exit...");
            Console.ReadKey();
        }
    }
}
