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

lastEvtId = pyrenderdoc.GetLastAction().eventId
evtId = 0
psHash = {}
for evtId in range(lastEvtId + 1):
	d = pyrenderdoc.GetAction(evtId)
	evtId += 1
	if d is None:
		continue
	pyrenderdoc.SetEventID([], pyrenderdoc.CurEvent(), d.eventId, False)
	state = pyrenderdoc.CurPipelineState()
	ps = state.GetShaderReflection(rd.ShaderStage.Pixel)
	if ps is None:
		continue
	shaderRes = pyrenderdoc.GetResource(ps.resourceId)
	shaderName = ""
	if shaderRes is not None:
		shaderName = shaderRes.name

	ct = psHash.get(shaderName)
	if ct is None:
		ct = 1
	else:
		ct += 1
	psHash[shaderName] = ct

	# Print this action
	print('%s%d: ps:%s ct:%d' % (0, d.eventId, shaderName, ct))

print('======')
for k, v in psHash.items():
	print(k, v)