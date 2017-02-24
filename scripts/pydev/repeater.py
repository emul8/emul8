if request.isInit:
    lastVal = 0
elif request.isRead:
    request.value = lastVal
elif request.isWrite:
    lastVal = request.value
