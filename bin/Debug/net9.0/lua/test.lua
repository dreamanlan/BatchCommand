function inc(v)
    return v+1
end
function get(arr, index)
    return arr[index]
end
function set(arr, index, v)
    arr[index]=v
end

function test()
    local t1 = os.time()
    local v = 0
    for i = 1, 100000000 do
        v = v + 1 --inc(v)
    end
    local t2 = os.time()
    print(os.difftime(t2,t1), v)
end

test()