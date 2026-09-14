# command script import e:\lldb_bpcmd.py
# script lldb_bpcmd.print_thread_stacks("")

# command script add -o -f lldb_bpcmd.bt_cont bt_cont

# watchpoint set expression -w write -s 8 -- (0xb400007262a9a0b0+0xc8)
# memory read -s 1 -c 0x20000 -o e:\tlsf_crash_mem.txt --force 0x00000071813c0000

# breakpoint set -f VKDescriptorState.cpp -l 247 -C 'p/x *(long*)(*(long*)((char*)m_UpdateTemplate+0x10)+8)' -C 'var m_Bindings->buffer' -C 'c'
# breakpoint set -y VKDescriptorState.cpp:247 -C 'p/x *(long*)(*(long*)((char*)m_UpdateTemplate+0x10)+8)' -C 'var m_Bindings->buffer' -C 'c'
# breakpoint set -C 'var e' -C 'c' -y GpuProgramsVK.cpp:622

import lldb
import os

file_path = 'e:/malloc.txt'
file_object = None

def del_file():
	global file_path
	if os.path.exists(file_path):
		os.remove(file_path)

def open_file():
	global file_object
	file_object = open(file_path, 'a')

def flush_file():
	global file_object
	if file_object is not None:
		file_object.flush()

def close_file():
	global file_object
	if file_object is not None:
		file_object.close()

def write_to_file(content):
	global file_object
	if file_object is None:
		open_file()
	if file_object is not None:
		file_object.write(content)
	else:
		print("Error: File is not open")

def get_regs(frame, kind):
	for reg_set in frame.regs:
		if kind.lower() in reg_set.name.lower():
			return reg_set
	return None

def get_GPRs(frame):
	return get_regs(frame, 'general purpose')

def get_FPRs(frame):
	return get_regs(frame, 'floating point')

def get_ESRs(frame):
	return get_regs(frame, 'exception state')

def print_modules(key):
	debugger = lldb.debugger
	target = debugger.GetSelectedTarget()
	for module in target.module_iter():
		if key in module.file.GetFilename().lower():
			addr = module.GetObjectFileHeaderAddress()
			entry = module.GetObjectFileEntryPointAddress()
			print("Module Addr:load 0x{:016x} file 0x{:016x} Entry:load 0x{:016x} file 0x{:016x} File:{}".format(addr.load_addr, addr.file_addr, entry.load_addr, entry.file_addr, module))
			for section in module.section_iter():
				print("  Section: ", section)

def print_threads(key):
	debugger = lldb.debugger
	process = debugger.GetSelectedTarget().GetProcess()
	for thread in process:
		if key in thread.name.lower():
			print(thread)

def print_breakpoints(key):
	debugger = lldb.debugger
	target = debugger.GetSelectedTarget()
	for breakpoint in target.breakpoint_iter():
		for location in breakpoint:
			symbol = location.GetAddress().GetSymbol()
			if symbol.IsValid() and key in symbol.GetName().lower():
				print(breakpoint)

def print_threads_stacks(key):
	dbg = lldb.debugger
	result = lldb.SBCommandReturnObject();
	ci = dbg.GetCommandInterpreter()
	debugger = lldb.debugger
	process = debugger.GetSelectedTarget().GetProcess()
	for thread in process:
		if key in thread.name.lower():
			print(thread)
			for f in thread.frames:
				print(f)
	
def bt_cont(debugger, command, result, internal_dict):
	target = debugger.GetSelectedTarget()
	process = target.GetProcess()
	thread = process.GetSelectedThread()
	print(thread, result)
	if len(thread.frames) > 0:
		for reg in get_GPRs(thread.frames[0]):
			if reg.name[0] != 'w':
				reg_info = "  {}: {}".format(reg.GetName(), reg.GetValue())
				print(reg_info)
	for frame in thread.frames:
		print(frame.reg)
		print(frame, result)
	process.Continue()
	
def on_bp(frame, bp_loc, extra_args, internal_dict):
	thread = frame.GetThread()
	process = thread.GetProcess()
	tinfo = str(thread)
	write_to_file(tinfo)
	write_to_file('\n')
	print(tinfo)
	if len(thread.frames) > 0:
		for reg in get_GPRs(thread.frames[0]):
			if reg.name[0] != 'w':
				reg_info = "  {}: {}".format(reg.GetName(), reg.GetValue())
				write_to_file(reg_info)
				write_to_file('\n')
				print(reg_info)
	for f in thread.frames:
		finfo = str(f)
		write_to_file(finfo)
		write_to_file('\n')
		print(finfo)
	process.Continue()
	
def test():
	dbg = lldb.debugger
	result = lldb.SBCommandReturnObject();
	ci = dbg.GetCommandInterpreter()
	ci.HandleCommand('bt', result)
	for reg in get_GPRs(lldb.frame):
		if reg.name[0] != 'w':
			print("  {}: {}".format(reg.GetName(), reg.GetValue()))
	print(result.GetOutput())
	target = dbg.GetSelectedTarget()
	process = target.GetProcess()
	thread = process.GetSelectedThread()
	frame = thread.GetSelectedFrame()
	process.Continue()
	
def start():
	close_file()
	del_file()
	debugger = lldb.debugger
	target = debugger.GetSelectedTarget()
	for module in target.module_iter():
		if "unity" in module.file.GetFilename().lower():
			addr = module.GetObjectFileHeaderAddress()
			entry = module.GetObjectFileEntryPointAddress()
			minfo = "Module Addr:load 0x{:016x} file 0x{:016x} Entry:load 0x{:016x} file 0x{:016x} File:{}".format(addr.load_addr, addr.file_addr, entry.load_addr, entry.file_addr, module)
			write_to_file(minfo)
			write_to_file('\n')
			print(minfo)
			for section in module.section_iter():
				sinfo = "  Section: {}".format(section)
				write_to_file(sinfo)
				write_to_file('\n')
				print(sinfo)
	process = target.GetProcess()
	for thread in process:
		if "unitygfx" in thread.name.lower():
			tinfo = str(thread)
			write_to_file(tinfo)
			write_to_file('\n')
			print(tinfo)
			breakpoint = target.BreakpointCreateByName("malloc")
			breakpoint.SetThreadID(thread.id)
			#breakpoint.SetCommandLineCommands(['reg read','bt','c'])
			breakpoint.SetScriptCallbackFunction("lldb_bpcmd.on_bp")
	process.Continue()