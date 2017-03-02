if request.isInit:
    lastVal = -1
else:
    request.value = lastVal + 1
    lastVal += 1

self.DebugLog("%s COUNTER 0x%x val 0x%x" % (str(request.type), request.offset, request.value))
