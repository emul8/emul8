if request.isInit:
	lastVal = 0
else:
	lastVal = 1 - lastVal
	request.value = lastVal * 0xFFFFFFFF
self.NoisyLog("%s FLIPFLOP 0x%x val 0x%x" % (str(request.type), request.offset, request.value))
