# RenderDoc Python console, powered by python 3.6.4.
# The 'pyrenderdoc' object is the current CaptureContext instance.
# The 'renderdoc' and 'qrenderdoc' modules are available.
# Documentation is available: https://renderdoc.org/docs/python_api/index.html

import sys
import os
import csv
import struct

# Import renderdoc if not already imported (e.g. in the UI)
if 'renderdoc' not in sys.modules and '_renderdoc' not in sys.modules:
	import renderdoc

# Alias renderdoc for legibility
rd = renderdoc

useInout = False

def callback(controller):
	global useInout

	state = controller.GetPipelineState()
	pipe = state.GetGraphicsPipelineObject()
	entry = state.GetShaderEntryPoint(rd.ShaderStage.Pixel)
	ps = state.GetShaderReflection(rd.ShaderStage.Pixel)

	useInout = False

	targets = controller.GetDisassemblyTargets(True)
	target = targets[0]

	# txt = controller.DisassembleShader(pipe, ps, target)	
	# print(txt[0:128])

	# print(ps.encoding)
	# print(ps.rawBytes.decode('utf-8'))
	# print(ps.debugInfo.encoding)
	# print(ps.debugInfo.files[0].filename)
	# print(ps.debugInfo.files[0].contents)
	
	if ps.encoding == rd.ShaderEncoding.GLSL:
		txt = ps.rawBytes.decode('utf-8')
	else:
		txt = controller.DisassembleShader(pipe, ps, target)
	if txt.find("inout") >= 0:
		useInout = True	

def UseInout():
	pyrenderdoc.Replay().BlockInvoke(callback)
	return useInout

lastEvtId = pyrenderdoc.GetLastDrawcall().eventId
evtId = 0
psHash = {}
for evtId in range(lastEvtId + 1):
	d = pyrenderdoc.GetDrawcall(evtId)
	evtId += 1
	if d is None:
		continue
	if d.flags == rd.DrawFlags.PushMarker or d.flags == rd.DrawFlags.Clear:
		continue
	pyrenderdoc.SetEventID([], pyrenderdoc.CurEvent(), d.eventId, False)
	state = pyrenderdoc.CurPipelineState()
	ps = state.GetShaderReflection(rd.ShaderStage.Pixel)
	if ps is None:
		continue
	glState = pyrenderdoc.CurGLPipelineState()
	glShader = glState.fragmentShader
	if glShader is None:
		continue
	shaderRes = pyrenderdoc.GetResource(glShader.shaderResourceId)
	progRes = pyrenderdoc.GetResource(glShader.programResourceId)
	
	shaderName = ""
	progName = ""
	if shaderRes is not None:
		shaderName = shaderRes.name
	if progRes is not None:
		progName = progRes.name

	useInout = UseInout()
	if useInout:
		ct = psHash.get(progName)
		if ct is None:
			ct = 1
		else:
			ct += 1
		psHash[progName] = ct

		# Print this action
		print('%s%d,%d,%s: ps:%s|%s ct:%d inout:%d' % (0, d.eventId, d.drawcallId, d.flags.name, shaderName, progName, ct, useInout))

print('======')
for k, v in psHash.items():
	print(k, v)