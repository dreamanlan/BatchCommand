# command script import e:\lldb_bpdata.py
# script lldb_bpdata.print_threads_stacks("")
# target stop-hook add -P lldb_bpdata.MyStopHook

# watchpoint set expression -w write -s 8 -- (0xb400007262a9a0b0+0xc8)
# memory read -s 1 -c 0x20000 -o e:\tlsf_crash_mem.txt --force 0x00000071813c0000

# breakpoint set -f VKDescriptorState.cpp -l 247 -C 'p/x *(long*)(*(long*)((char*)m_UpdateTemplate+0x10)+8)' -C 'var m_Bindings->buffer' -C 'c'
# breakpoint set -y VKDescriptorState.cpp:247 -C 'p/x *(long*)(*(long*)((char*)m_UpdateTemplate+0x10)+8)' -C 'var m_Bindings->buffer' -C 'c'
# breakpoint set -C 'var e' -C 'c' -y GpuProgramsVK.cpp:622

import lldb
import os
import struct

file_path = 'e:/databp.txt'
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
	file_object = None

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
	debugger.HandleCommand("target stop-hook delete")
	debugger.HandleCommand("target stop-hook add -P lldb_bpdata.MyStopHook")
	process.Continue()

def resume():
	debugger = lldb.debugger
	target = debugger.GetSelectedTarget()
	process = target.GetProcess()
	debugger.HandleCommand("target stop-hook delete")
	debugger.HandleCommand("target stop-hook add -P lldb_bpdata.MyStopHook")
	process.Continue()

class MyStopHook(object):

	def __init__(self, target, extra_args, internal_dict):
		self.target = target

	def handle_stop(self, exe_ctx, stream):
		target = self.target
		frame = exe_ctx.frame
		thread = frame.GetThread()
		process = thread.GetProcess()
		tinfo = str(thread)
		write_to_file(tinfo)
		write_to_file('\n')
		stream.Print(tinfo)
		stream.Print('\n')

		if thread.stop_reason == lldb.eStopReasonWatchpoint:
			if len(thread.frames) > 0:
				gprs = get_GPRs(thread.frames[0])
				if gprs is not None:
					for reg in gprs:
						if reg.name[0] != 'w':
							reg_info = "  {}: {}".format(reg.GetName(), reg.GetValue())
							write_to_file(reg_info)
							write_to_file('\n')
							stream.Print(reg_info)
							stream.Print('\n')

			for f in thread.frames:
				finfo = str(f)
				write_to_file(finfo)
				write_to_file('\n')
				stream.Print(finfo)
				stream.Print('\n')

		cmd = target.EvaluateExpression("g_WatchPointCommandInfo.cmd").signed
		flag = target.EvaluateExpression("g_WatchPointCommandInfo.flag").signed
		size = target.EvaluateExpression("g_WatchPointCommandInfo.size").signed
		addr = target.EvaluateExpression("g_WatchPointCommandInfo.addr").signed
		tid = target.EvaluateExpression("g_WatchPointCommandInfo.tid").signed

		cmd_info = ""
		if addr!=0 and size>0:
			mem_error = lldb.SBError()
			mem_val = process.ReadMemory(addr,size,mem_error)
			if mem_error.Success():
				#val = struct.unpack('q',bytearray(mem_val))
				val = int.from_bytes(bytearray(mem_val), byteorder='little')
				cmd_info = "cmd:{} flag:{} size:{} addr:{:016x} value:{:016x} tid:{}".format(cmd,flag,size,addr,val,tid)
			else:
				cmd_info = "cmd:{} flag:{} size:{} addr:{:016x} value:(failed) tid:{}".format(cmd,flag,size,addr,tid)
		else:
			cmd_info = "cmd:{} flag:{} size:{} addr:{:016x} value:(unread) tid:{}".format(cmd,flag,size,addr,tid)

		write_to_file(cmd_info)
		write_to_file('\n')
		stream.Print(cmd_info)
		stream.Print('\n')

		if thread.stop_reason == lldb.eStopReasonSignal and thread.id == tid:
			sig = thread.GetStopReasonDataAtIndex(0)
			if sig == 5:
				if cmd == 1:
					while target.num_watchpoints>=4:
						wp = target.watchpoint[0]
						wp.SetEnabled(False)
						target.DeleteWatchpoint(wp.GetID())
					wp_error = lldb.SBError()
					target.WatchAddress(addr,size,False,True,wp_error)
					target.EvaluateExpression("g_WatchPointCommandInfo.cmd=0")
				elif cmd == 2:
					for i in range(target.num_watchpoints):
						wp = target.watchpoint[i]
						if wp.GetWatchAddress() == addr:
							wp.SetEnabled(False)
							target.DeleteWatchpoint(wp.GetID())
							target.EvaluateExpression("g_WatchPointCommandInfo.cmd=0")
							break

		wp_info = "num_watchpoints:{}".format(target.num_watchpoints)
		write_to_file(wp_info)
		write_to_file('\n')
		stream.Print(wp_info)
		stream.Print('\n')

		ret = False
		if thread.stop_reason == lldb.eStopReasonWatchpoint:
			wp_id = thread.GetStopReasonDataAtIndex(0)
			wp = target.FindWatchpointByID(wp_id)
			if wp is not None:
				wp_addr = wp.GetWatchAddress()
				wp_size = wp.GetWatchSize()
				wp_spec = wp.GetWatchSpec()
				wp_error = lldb.SBError()
				mem_val = process.ReadMemory(wp_addr,wp_size,wp_error)
				if wp_error.Success():
					#wp_val = struct.unpack('q',bytearray(mem_val))
					wp_val = int.from_bytes(bytearray(mem_val), byteorder='little')
					wp_info = "watch point addr:{:016x} size:{} spec:{} value:{:016x}".format(wp_addr,wp_size,wp_spec,wp_val)
					write_to_file(wp_info)
					write_to_file('\n')
					stream.Print(wp_info)
					stream.Print('\n')
		elif thread.stop_reason == lldb.eStopReasonSignal:
			sig = thread.GetStopReasonDataAtIndex(0)
			sig_info = "sig:{}".format(sig)
			write_to_file(sig_info)
			write_to_file('\n')
			stream.Print(sig_info)
			stream.Print('\n')
			if sig == 5:
				pass
			else:
				ret = True
		elif thread.stop_reason == lldb.eStopReasonException or thread.stop_reason == lldb.eStopReasonThreadExiting:
			ret = True
		else:
			pass

		flush_file()
		return ret