-- ============================================================
--  LUNAR THREAD TEST
-- ============================================================

print("Iniciando teste de threads...")

local function tarefa(nome, tempo)
    print("  [Thread " .. nome .. "] Iniciada")
    -- Simular trabalho (MoonSharp coroutines não são threads reais de SO, 
    -- mas funcionam como cooperativas)
    for i = 1, 5 do
        print("  [Thread " .. nome .. "] Passo " .. i)
    end
    print("  [Thread " .. nome .. "] Finalizada")
end

-- Spawnar threads
spawn(function() tarefa("Alfa", 100) end)
spawn(function() tarefa("Beta", 200) end)

print("Threads spawnadas! Use a opção 'Monitorar Threads' no menu.")
