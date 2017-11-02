if request.isInit:
	lastVal = 0
else:
        if lastVal == 0:
            lastVal = 1
        else:
            lastVal = (lastVal << 1) & 0xFFFFFFFF
        request.value = lastVal
