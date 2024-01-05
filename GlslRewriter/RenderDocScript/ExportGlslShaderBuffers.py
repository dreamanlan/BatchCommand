# RenderDoc Python console, powered by python 3.6.4.
# The 'pyrenderdoc' object is the current CaptureContext instance.
# The 'renderdoc' and 'qrenderdoc' modules are available.
# Documentation is available: https://renderdoc.org/docs/python_api/index.html

import sys
import os
import csv
import struct

subDir = ""
vaoOutputPath = "d:/uc/VAO_lighttex"
uniformOutputPath = "d:/uc/lighttex"
vaoRecordNum = 0

def bytesToStr(bytes):
	str0 = ""
	for byte in bytes:
		str0 += f'{byte:x} '
	return str0

pipestate = pyrenderdoc.CurPipelineState();
glstate = pyrenderdoc.CurGLPipelineState();

print("topology:", pipestate.GetPrimitiveTopology())

ib = pipestate.GetIBuffer()
if ib is not None:
	resdesc = pyrenderdoc.GetResource(ib.resourceId)
	if resdesc is not None:
		print("ib:", ib.byteOffset, "size:", ib.byteSize, "stride:", ib.byteStride, resdesc.name, ib.resourceId)

		buf = pyrenderdoc.GetBuffer(ib.resourceId)
		if buf is not None:
			print(buf.resourceId, "gpu addr:", buf.gpuAddress, "len:", buf.length, buf.creationFlags)

vbs = pipestate.GetVBuffers()
if vbs is not None:
	for vb in vbs:
		resdesc = pyrenderdoc.GetResource(vb.resourceId)
		if resdesc is not None:
			print("vb:", vb.byteOffset, "size:", vb.byteSize, "stride:", vb.byteStride, resdesc.name, vb.resourceId)

			buf = pyrenderdoc.GetBuffer(vb.resourceId)
			if buf is not None:
				print(buf.resourceId, "gpu addr:", buf.gpuAddress, "len:", buf.length, buf.creationFlags)

vis = pipestate.GetVertexInputs()
if vis is not None:
	for vi in vis:
		print("vi:", vi.name, "offset:", vi.byteOffset, "format:", vi.format.Name(), "instanceRate:", vi.instanceRate, "perInstance:", vi.perInstance, "used:", vi.used, "vertexBuffer:", vi.vertexBuffer)

ots = pipestate.GetOutputTargets()
if ots is not None:
	for ot in ots:
		resdesc = pyrenderdoc.GetResource(ot.resourceId)
		if resdesc is not None:
			print("output:", resdesc.name, ot.resourceId)

			buf = pyrenderdoc.GetBuffer(ot.resourceId)
			if buf is not None:
				print(buf.resourceId, "gpu addr:", buf.gpuAddress, "len:", buf.length, buf.creationFlags)

for stage in renderdoc.ShaderStage:
	print()

	print("<===", stage, "===>")
	bindpointMapping = pipestate.GetBindpointMapping(stage)
	if bindpointMapping is not None:
		for cb in bindpointMapping.constantBlocks:
			print("constant block bind (bind, bindset):", cb.bind, cb.bindset, "array size:", cb.arraySize, cb.used)
		for input in bindpointMapping.inputAttributes:
			print("input bind:", input)
		for rres in bindpointMapping.readOnlyResources:
			print("readonly bind (bind, bindset):", rres.bind, rres.bindset, "array size:", rres.arraySize, rres.used)
		for rwres in bindpointMapping.readWriteResources:
			print("readwrite bind (bind, bindset):", rwres.bind, rwres.bindset, "array size:", rwres.arraySize, rwres.used)
		for sam in bindpointMapping.samplers:
			print("sampler bind (bind, bindset):", sam.bind, sam.bindset, "array size:", sam.arraySize, sam.used)

	print()

	shader = pipestate.GetShaderReflection(stage)
	if shader is not None:
		cbIx = 0
		for cb in shader.constantBlocks:
			print("constant block:", cb.name, "bind point:", cb.bindPoint, "size:", cb.byteSize)
			for constant in cb.variables:
				print("==>name:", constant.name, "bitFieldOffset:", constant.bitFieldOffset, "bitFieldSize:", constant.bitFieldSize, "byteOffset:", constant.byteOffset, "defval:", constant.defaultValue,
"base type:", constant.type.baseType, "elements:", constant.type.elements, "cols:", constant.type.columns, "rows:", constant.type.rows,
"type name:", constant.type.name, "arrayByteStride:", constant.type.arrayByteStride, "matrixByteStride:", constant.type.matrixByteStride,
"pointerTypeID:", constant.type.pointerTypeID, "member count:", len(constant.type.members))
			cbBuf = pipestate.GetConstantBuffer(stage, cbIx, 0)
			if cbBuf is not None:
				print("==>", cbBuf.resourceId, "offset:", cbBuf.byteOffset, "size:", cbBuf.byteSize, "inlineData:", bytesToStr(cbBuf.inlineData));
			cbIx = cbIx + 1
		for input in shader.inputSignature:
			print("input:", input.varName, input.varType, "comp count:", input.compCount, "reg index:", input.regIndex, "semantic index:", input.semanticIndex, "semantic name:", input.semanticName, "stream:", input.stream, input.systemValue)
		for output in shader.outputSignature:
			print("output:", output.varName, output.varType, "comp count:", output.compCount, "reg index:", output.regIndex, "semantic index:", output.semanticIndex, "semantic name:", output.semanticName, "stream:", output.stream, output.systemValue)
		for rres in shader.readOnlyResources:
			print("readonly:", rres.name, "bind point:", rres.bindPoint, rres.resType,
"base type:", rres.variableType.baseType, "elements:", rres.variableType.elements, "cols:", rres.variableType.columns, "rows:", rres.variableType.rows,
"type name:", rres.variableType.name, "arrayByteStride:", rres.variableType.arrayByteStride, "matrixByteStride:", rres.variableType.matrixByteStride,
"pointerTypeID:", rres.variableType.pointerTypeID, "member count:", len(rres.variableType.members))
		for rwres in shader.readWriteResources:
			print("readwrite:", rwres.name, "bind point:", rwres.bindPoint, rwres.resType,
"base type:", rwres.variableType.baseType, "elements:", rwres.variableType.elements, "cols:", rwres.variableType.columns, "rows:", rwres.variableType.rows,
"type name:", rwres.variableType.name, "arrayByteStride:", rwres.variableType.arrayByteStride, "matrixByteStride:", rwres.variableType.matrixByteStride,
"pointerTypeID:", rwres.variableType.pointerTypeID, "member count:", len(rwres.variableType.members))
		for sam in shader.samplers:
			print("sampler:", sam.name, "bind point:", sam.bindPoint)

	print()

	bindings = pipestate.GetReadOnlyResources(stage)
	for binding in bindings:
		bp = binding.bindPoint
		firstIndex = binding.firstIndex
		dynamicallyUsedCount = binding.dynamicallyUsedCount
		printBind = False
		for res in binding.resources:
			resdesc = pyrenderdoc.GetResource(res.resourceId)
			if resdesc is not None:
				printBind = True
				print("readonly:", stage, resdesc.name, res.resourceId)

				buf = pyrenderdoc.GetBuffer(res.resourceId)
				if buf is not None:
					print("==>", buf.resourceId, "gpu addr:", buf.gpuAddress, "len:", buf.length, buf.creationFlags)
		if printBind:
			print("<== readonly bind (bind, bindset):", bp.bind, bp.bindset, "array size:", bp.arraySize, "first index:", firstIndex, "used count:", dynamicallyUsedCount)

	print()

	bindings = pipestate.GetReadWriteResources(stage)
	for binding in bindings:
		bp = binding.bindPoint
		firstIndex = binding.firstIndex
		dynamicallyUsedCount = binding.dynamicallyUsedCount
		printBind = False
		for res in binding.resources:
			resdesc = pyrenderdoc.GetResource(res.resourceId)
			if resdesc is not None:
				printBind = True
				print("readwrite:", stage, resdesc.name, res.resourceId)

				buf = pyrenderdoc.GetBuffer(res.resourceId)
				if buf is not None:
					print("==>", buf.resourceId, "gpu addr:", buf.gpuAddress, "len:", buf.length, buf.creationFlags)
		if printBind:
			print("<== readwrite bind (bind, bindset):", bp.bind, bp.bindset, "array size:", bp.arraySize, "first index:", firstIndex, "used count:", dynamicallyUsedCount)

	print()

	bindings = pipestate.GetSamplers(stage)
	for binding in bindings:
		bp = binding.bindPoint
		firstIndex = binding.firstIndex
		dynamicallyUsedCount = binding.dynamicallyUsedCount
		printBind = False
		for res in binding.resources:
			resdesc = pyrenderdoc.GetResource(res.resourceId)
			if resdesc is not None:
				printBind = True
				print("sampler:", stage, resdesc.name, res.resourceId)

				buf = pyrenderdoc.GetBuffer(res.resourceId)
				if buf is not None:
					print("==>", buf.resourceId, "gpu addr:", buf.gpuAddress, "len:", buf.length, buf.creationFlags)
		if printBind:
			print("<== sampler bind (bind, bindset):", bp.bind, bp.bindset, "array size:", bp.arraySize, "first index:", firstIndex, "used count:", dynamicallyUsedCount)

print()

for buf in pyrenderdoc.GetBuffers():
    print("buf %s is %s gpu addr:%x len:%d %s" % (buf.resourceId, pyrenderdoc.GetResourceName(buf.resourceId), buf.gpuAddress, buf.length, buf.creationFlags))
	
for glBuf in glstate.shaderStorageBuffers:
	if glBuf.resourceId != renderdoc.ResourceId.Null():
		print("glBuffer %s byteOffset:%d size:%d" % (glBuf.resourceId, glBuf.byteOffset, glBuf.byteSize))

print()

def saveTexture(fileName, resourceId, controller):
	texsave = renderdoc.TextureSave()
	texsave.resourceId = resourceId
	if texsave.resourceId == renderdoc.ResourceId.Null():
		return False

	filename = str(int(texsave.resourceId))
	# texsave.alpha = renderdoc.AlphaMapping.BlendToCheckerboard
	# Most formats can only display a single image per file, so we select the
	# first mip and first slice
	texsave.mip = 0
	texsave.slice.sliceIndex = 0
	texsave.alpha = renderdoc.AlphaMapping.Preserve
	texsave.destType = renderdoc.FileType.EXR
	
	basePath = uniformOutputPath
	if subDir != "" and not os.path.exists("{0}{1}".format(basePath, subDir)):
		os.makedirs("{0}{1}".format(basePath, subDir))

	outTexPath = "{0}{1}/{2}.exr".format(basePath, subDir, fileName)
	controller.SaveTexture(texsave, outTexPath)

	print("save texture {0}".format(outTexPath))
	return True

def writeCsv(fileName, csvArray, isVAO):
	basePath = vaoOutputPath if isVAO else uniformOutputPath
	if subDir != "" and not os.path.exists("{0}{1}".format(basePath, subDir)):
		os.makedirs("{0}{1}".format(basePath, subDir))

	outPath = "{0}{1}/{2}.csv".format(basePath, subDir, fileName)
	csvFile = open(outPath, "w", newline='')
	writer = csv.writer(csvFile)
	
	writer.writerows(csvArray)
	csvFile.close()

	print("save csv {0}".format(outPath))


def sampleCode(controller):
	curAction = pyrenderdoc.CurAction()
	if curAction is None:
		return
	print("cur action:", curAction.eventId, "numIndices:", curAction.numIndices, "numInstances:", curAction.numInstances, curAction.flags) 
	print("==>", curAction.drawIndex, "baseVertex:", curAction.baseVertex, "vertexOffset:", curAction.vertexOffset, "indexOffset:", curAction.indexOffset, "instanceOffset:", curAction.instanceOffset) 
	
	ib = pipestate.GetIBuffer()
	if ib is not None:
		resdesc = pyrenderdoc.GetResource(ib.resourceId)
		if resdesc is not None:
			print("ib:", ib.byteOffset, "size:", ib.byteSize, "stride:", ib.byteStride, resdesc.name, ib.resourceId)

			buf = pyrenderdoc.GetBuffer(ib.resourceId)
			if buf is not None:
				bufData = controller.GetBufferData(ib.resourceId, ib.byteOffset, buf.length)
				if bufData is not None:
					csvArray = []
					fileHeader = ["Element", "indices"]

					csvArray.append(fileHeader)

					for i in range(min(buf.length // ib.byteStride, curAction.numIndices)):
						off =  (curAction.indexOffset + i) * ib.byteStride
						bytes = struct.unpack_from("H", bufData, off)
						row = [i, bytes[0]]
						csvArray.append(row)
					writeCsv("vertex_index", csvArray, True)

	vbs = pipestate.GetVBuffers()
	if vbs is not None:
		for vb in vbs:
			resdesc = pyrenderdoc.GetResource(vb.resourceId)
			if resdesc is not None:
				print("vb:", vb.byteOffset, "size:", vb.byteSize, "stride:", vb.byteStride, resdesc.name, vb.resourceId)

				buf = pyrenderdoc.GetBuffer(vb.resourceId)
				if buf is not None:
					print(buf.resourceId, "gpu addr:", buf.gpuAddress, "len:", buf.length, buf.creationFlags)

	vis = pipestate.GetVertexInputs()
	if vis is not None:
		for vi in vis:
			print("vi:", vi.name, "offset:", vi.byteOffset, "format:", vi.format.Name(), "instanceRate:", vi.instanceRate, "perInstance:", vi.perInstance, "used:", vi.used, "vertexBuffer:", vi.vertexBuffer)
			if vi.format.Name() == "R32G32B32A32_FLOAT":
				vb = vbs[vi.vertexBuffer]
				buf = pyrenderdoc.GetBuffer(vb.resourceId)
				if buf is not None:
					bufData = controller.GetBufferData(vb.resourceId, vb.byteOffset, buf.length)
					if bufData is not None:
						csvArray = []
						fileHeader = ["Element"]
						formatXYZE = [".x", ".y", ".z", ".w"]
						headFormat = "vertex.{0}{1}"
						for comp in formatXYZE:
							newStr = headFormat.format(vi.name, comp)
							fileHeader.append(newStr)

						csvArray.append(fileHeader)

						for i in range(min(buf.length // vb.byteStride, max(vaoRecordNum, curAction.numIndices))):
							off =  vi.byteOffset + i * vb.byteStride
							bytes = struct.unpack_from("4f", bufData, off)
							row = [i, bytes[0], bytes[1], bytes[2], bytes[3]]
							csvArray.append(row)
						writeCsv("vertex_" + vi.name, csvArray, True)
						
	for stage in renderdoc.ShaderStage:
		bindpointMapping = pipestate.GetBindpointMapping(stage)
		rwBindings = pipestate.GetReadWriteResources(stage)
		bindings = pipestate.GetReadOnlyResources(stage)
		shader = pipestate.GetShaderReflection(stage)
		if shader is not None:
			cbIx = 0
			for cb in shader.constantBlocks:
				print("constant block:", cb.name, "bind point:", cb.bindPoint, "size:", cb.byteSize)
				if len(cb.variables)==1:
					constant = cb.variables[0]
					
					if constant.type.name == "uvec4":
						stride = constant.type.arrayByteStride
						cbBuf = pipestate.GetConstantBuffer(stage, cbIx, 0)
						if cbBuf is not None:
							bufData = controller.GetBufferData(cbBuf.resourceId, cbBuf.byteOffset, cbBuf.byteSize)
							if bufData is not None:
								csvArray = []
								fileHeader = ["Name", "Value", "Byte Offset", "Type"]
								csvArray.append(fileHeader)

								csvArray.append([constant.name, "", 0, "uint4[" + str(cbBuf.byteSize // stride) + "]"]);

								for i in range(cbBuf.byteSize // stride):
									off =  i * stride
									bytes = struct.unpack_from("4I", bufData, off)
									row = [constant.name+"["+str(i)+"]", '{0}, {1}, {2}, {3}'.format(bytes[0], bytes[1], bytes[2], bytes[3]), off, "uint4"]
									csvArray.append(row)
								writeCsv(constant.name, csvArray, False)
							
				cbIx = cbIx + 1

			for rwres in shader.readWriteResources:
				print("readwrite:", rwres.name, "bind point:", rwres.bindPoint, rwres.resType,
	"base type:", rwres.variableType.baseType, "elements:", rwres.variableType.elements, "cols:", rwres.variableType.columns, "rows:", rwres.variableType.rows,
	"type name:", rwres.variableType.name, "arrayByteStride:", rwres.variableType.arrayByteStride, "matrixByteStride:", rwres.variableType.matrixByteStride,
	"pointerTypeID:", rwres.variableType.pointerTypeID, "member count:", len(rwres.variableType.members))
				if rwres.resType == renderdoc.TextureType.Buffer:
					if bindpointMapping is not None:
						readWriteBindpoint = bindpointMapping.readWriteResources[rwres.bindPoint]
						bind = readWriteBindpoint.bind
						bindset = readWriteBindpoint.bindset

						for binding in rwBindings:
							bp = binding.bindPoint
							if bp.bind == bind and bp.bindset == bindset:
								ssboIx = 0
								for res in binding.resources:
									csvName = rwres.name
									if ssboIx != 0:
										csvName = rwres.name + "_" + str(ssboIx)
									
									for glBuf in glstate.shaderStorageBuffers:
										if glBuf.resourceId != renderdoc.ResourceId.Null() and glBuf.resourceId == res.resourceId:
											print("glBuffer %s byteOffset:%d size:%d" % (glBuf.resourceId, glBuf.byteOffset, glBuf.byteSize))
											
											bufData = controller.GetBufferData(res.resourceId, glBuf.byteOffset, glBuf.byteSize)
											if bufData is not None:
												if rwres.variableType.baseType == renderdoc.VarType.UInt:
													csvArray = []
													fileHeader = ["Element", rwres.name]
													csvArray.append(fileHeader)

													for i in range(glBuf.byteSize // 4):
														off =  i * 4
														bytes = struct.unpack_from("1I", bufData, off)
														row = [i, bytes[0]]
														csvArray.append(row)
													writeCsv(csvName, csvArray, False)
											break
									
									ssboIx = ssboIx + 1
				
			for rres in shader.readOnlyResources:
				print("readonly:", rres.name, "bind point:", rres.bindPoint, rres.resType,
"base type:", rres.variableType.baseType, "elements:", rres.variableType.elements, "cols:", rres.variableType.columns, "rows:", rres.variableType.rows,
"type name:", rres.variableType.name, "arrayByteStride:", rres.variableType.arrayByteStride, "matrixByteStride:", rres.variableType.matrixByteStride,
"pointerTypeID:", rres.variableType.pointerTypeID, "member count:", len(rres.variableType.members))
				if rres.resType == renderdoc.TextureType.Texture2D:
					if bindpointMapping is not None:
						readOnlyBindpoint = bindpointMapping.readOnlyResources[rres.bindPoint]
						bind = readOnlyBindpoint.bind
						bindset = readOnlyBindpoint.bindset

						for binding in bindings:
							bp = binding.bindPoint
							if bp.bind == bind and bp.bindset == bindset:
								texIx = 0
								for res in binding.resources:
									if texIx == 0:
										saveTexture(rres.name, res.resourceId, controller)
									else:
										saveTexture(rres.name + "_" + str(texIx), res.resourceId, controller)
									texIx = texIx + 1

pyrenderdoc.Replay().BlockInvoke(sampleCode)