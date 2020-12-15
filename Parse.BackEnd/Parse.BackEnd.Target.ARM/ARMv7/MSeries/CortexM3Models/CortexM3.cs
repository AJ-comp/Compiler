using System;

namespace Parse.BackEnd.Target.ARMv7.MSeries.CortexM3Models
{
    public class CortexM3 : ARMv7_M
    {
        public string DataSheet { get; protected set; }
		public override MemoryInfo FlashMemory { get; } = new MemoryInfo(0x8000000, 384000);
		public override MemoryInfo RAM { get; } = new MemoryInfo(0x20000000, 64000);
        public MemoryInfo EEPROM { get; set; }

        public override string StartUpCode
        {
			get
            {
				return "\t.cpu cortex-m3" + Environment.NewLine +
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
						"\t.global Reset_Handler" + Environment.NewLine +
						"\t.arch armv7-m" + Environment.NewLine +
						"\t.syntax unified" + Environment.NewLine +
						"\t.thumb" + Environment.NewLine +
						"\t.thumb_func" + Environment.NewLine +
						"\t.fpu softvfp" + Environment.NewLine +

						"\t.weak Reset_Handler" + Environment.NewLine +
						"\t.type Reset_Handler, % function" + Environment.NewLine +
						"Reset_Handler:" + Environment.NewLine +
						"\t@ args = 0, pretend = 0, frame = 0" + Environment.NewLine +
						"\t@ frame_needed = 1, uses_anonymous_args = 0" + Environment.NewLine +
						"\tpush    { r7, lr}" + Environment.NewLine +
						"\tadd r7, sp, #0" + Environment.NewLine +
						"\tmovs    r1, #0" + Environment.NewLine +
						"\tmovs r0, #0" + Environment.NewLine +
						"\tbl  main" + Environment.NewLine +
						"\tnop" + Environment.NewLine +
						"\tpop { r7, pc}" + Environment.NewLine +
						"\t.size Reset_Handler, .-Reset_Handler" + Environment.NewLine +
						"\t.global exception_table" + Environment.NewLine +
						"\t.section\t.vectors.table,\"aw\"" + Environment.NewLine +
						"\t.align  2" + Environment.NewLine +
						"\t.type   exception_table, % object" + Environment.NewLine +
						"\t.size   exception_table, 8" + Environment.NewLine +
						Environment.NewLine +

						DefaultIRQCode("NMI_Handler") +
						DefaultIRQCode("HardFault_Handler") +
						DefaultIRQCode("MemManage_Handler") +
						DefaultIRQCode("BusFault_Handler") +
						DefaultIRQCode("UsageFault_Handler") +
						DefaultIRQCode("SVC_Handler") +
						DefaultIRQCode("DebugMon_Handler") +
						DefaultIRQCode("PendSV_Handler") +
						DefaultIRQCode("SysTick_Handler") +

						"exception_table:" + Environment.NewLine +
						"\t.word   _ram_end" + Environment.NewLine +
						"\t.word Reset_Handler" + Environment.NewLine +
						"\t.word NMI_Handler" + Environment.NewLine +
						"\t.word HardFault_Handler" + Environment.NewLine +
						"\t.word MemManage_Handler" + Environment.NewLine +
						"\t.word BusFault_Handler" + Environment.NewLine +
						"\t.word UsageFault_Handler" + Environment.NewLine +
						"\t.word 0" + Environment.NewLine +
						"\t.word 0" + Environment.NewLine +
						"\t.word 0" + Environment.NewLine +
						"\t.word 0" + Environment.NewLine +
						"\t.word SVC_Handler" + Environment.NewLine +
						"\t.word DebugMon_Handler" + Environment.NewLine +
						"\t.word 0" + Environment.NewLine +
						"\t.word PendSV_Handler" + Environment.NewLine +
						"\t.word SysTick_Handler" + Environment.NewLine +
						"\t.ident  \"GCC: (GNU Arm Embedded Toolchain 9-2020-q2-update) 9.3.1 20200408(release)\"";
			}
        }

        public int seq = 0;

        public CortexM3()
        {
            this.Name = GetType().Name;
        }


		private string DefaultIRQCode(string handlerName)
        {
			if (handlerName.Length == 0) return string.Empty;

			return string.Format("\t.weak {0}" + Environment.NewLine +
										  "\t.type {0}, % function" + Environment.NewLine +
										  "{0}:" + Environment.NewLine +
										  "\tb {0}" + Environment.NewLine, handlerName);
		}
    }
}
