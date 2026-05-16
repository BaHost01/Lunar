-- ============================================================
--  LUNAR ENGINE: 3D PARKOUR DEMO
-- ============================================================

print("🌙 Carregando Parkour Demo...")

-- ─── Configurações do Nível ──────────────────────────────
local PLATFORM_COUNT = 15
local START_POS = { x = 0, y = 0, z = 0 }
local GOAL_POS = { x = 0, y = 0, z = 0 }

-- ─── Criar Ponto de Partida ─────────────────────────────
local spawnPoint = new("Part", nil, nil, {Type="Spawn"}, {Color="Green", Transparency=0.2})
spawnPoint:MoveTo(START_POS.x, START_POS.y, START_POS.z)

-- ─── Gerar Plataformas Procedurais ──────────────────────
local lastX, lastY, lastZ = START_POS.x, START_POS.y, START_POS.z

for i = 1, PLATFORM_COUNT do
    local platform = new("Part", nil, nil, {ID=i}, {Color="Blue"})
    
    -- Lógica de salto (distância aleatória mas alcançável)
    local dx = math.random(5, 12)
    local dy = math.random(-2, 4)
    local dz = math.random(-5, 5)
    
    lastX = lastX + dx
    lastY = lastY + dy
    lastZ = lastZ + dz
    
    platform:MoveTo(lastX, lastY, lastZ)
    print("  [Plataforma " .. i .. "] Criada em " .. lastX .. ", " .. lastY .. ", " .. lastZ)
end

GOAL_POS = { x = lastX + 10, y = lastY, z = lastZ }

-- ─── Criar Objetivo Final ──────────────────────────────
local goal = new("Part", nil, nil, {Type="Goal"}, {Color="Gold"})
goal:MoveTo(GOAL_POS.x, GOAL_POS.y, GOAL_POS.z)

-- ─── Som de Início ─────────────────────────────────────
local startSound = new("Sound")
startSound.Properties.SoundId = "lunar://assets/start_race.mp3"
startSound:Play()

-- ─── Monitoramento de Progresso (Thread) ───────────────
spawn(function()
    print("🏁 Corrida Iniciada! Chegue ao bloco Dourado.")
    local timer = 0
    while true do
        timer = timer + 1
        -- Simulação de verificação de vitória
        if timer > 10 then
            warn("Tempo correndo: " .. timer .. "s")
        end
        if timer == 20 then
            print("🏆 Jogador chegou ao objetivo!")
            local winSound = new("Sound")
            winSound.Properties.SoundId = "lunar://assets/win.mp3"
            winSound:Play()
            break
        end
    end
end)

print("✅ Mapa de Parkour gerado com sucesso!")
