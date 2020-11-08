using System;
using System.IO;

namespace ApplicationLayer.Common
{
    public class BootsTrapGenerator
    {
        public static void CreateVectorTable(string path, string fileName)
        {
			string code = "\t.cpu cortex-m3" + Environment.NewLine +
								"\t.eabi_attribute 20, 1" + Environment.NewLine +
								"\t.eabi_attribute 21, 1" + Environment.NewLine +
								"\t.eabi_attribute 23, 3" + Environment.NewLine +
								"\t.eabi_attribute 24, 1" + Environment.NewLine +
								"\t.eabi_attribute 25, 1" + Environment.NewLine +
								"\t.eabi_attribute 26, 1" + Environment.NewLine +
								"\t.eabi_attribute 30, 6" + Environment.NewLine +
								"\t.eabi_attribute 34, 1" + Environment.NewLine +
								"\t.eabi_attribute 18, 4" + Environment.NewLine +
								"\t.file   \"vector.c\"" + Environment.NewLine +
								"\t.text" + Environment.NewLine +
								"\t.section\t.vectors.code,\"ax\",% progbits" + Environment.NewLine +
								"\t.align  1" + Environment.NewLine +
								"\t.global Reset_handler" + Environment.NewLine +
								"\t.arch armv7-m" + Environment.NewLine +
								"\t.syntax unified" + Environment.NewLine +
								"\t.thumb" + Environment.NewLine +
								"\t.thumb_func" + Environment.NewLine +
								"\t.fpu softvfp" + Environment.NewLine +
								"\t.type Reset_handler, % function" + Environment.NewLine +
								"Reset_handler:" + Environment.NewLine +
								"\t@ args = 0, pretend = 0, frame = 0" + Environment.NewLine +
								"\t@ frame_needed = 1, uses_anonymous_args = 0" + Environment.NewLine +
								"\tpush    { r7, lr}" + Environment.NewLine +
								"\tadd r7, sp, #0" + Environment.NewLine +
								"\tmovs    r1, #0" + Environment.NewLine +
								"\tmovs r0, #0" + Environment.NewLine +
								"\tbl  main" + Environment.NewLine +
								"\tnop" + Environment.NewLine +
								"\tpop { r7, pc}" + Environment.NewLine +
								"\t.size Reset_handler, .-Reset_handler" + Environment.NewLine +
								"\t.global exception_table" + Environment.NewLine +
								"\t.section\t.vectors.table,\"aw\"" + Environment.NewLine +
								"\t.align  2" + Environment.NewLine +
								"\t.type   exception_table, % object" + Environment.NewLine +
								"\t.size   exception_table, 8" + Environment.NewLine +
								"exception_table:" + Environment.NewLine +
								"\t.word   _ram_end" + Environment.NewLine +
								"\t.word Reset_handler" + Environment.NewLine +
								"\t.ident  \"GCC: (GNU Arm Embedded Toolchain 9-2020-q2-update) 9.3.1 20200408(release)\"";

			File.WriteAllText(Path.Combine(path, fileName), code);
		}
    }
}
