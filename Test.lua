-- ============================================================
--  LUNAR TEST SCRIPT — teste completo de features Lua
-- ============================================================

print("=== LUNAR TEST SCRIPT ===")
print("")

-- ─── 1. Variáveis e tipos ─────────────────────────────────
print("-- [1] Variáveis e Tipos --")

local inteiro   = 42
local flutuante = 3.14159
local texto     = "Lunar Engine"
local booleano  = true
local nulo      = nil

print("Inteiro:   " .. inteiro)
print("Flutuante: " .. flutuante)
print("Texto:     " .. texto)
print("Booleano:  " .. tostring(booleano))
print("Nulo:      " .. tostring(nulo))
print("")

-- ─── 2. Operações matemáticas ────────────────────────────
print("-- [2] Matemática --")

local a, b = 15, 4

print("Soma:       " .. (a + b))
print("Subtração:  " .. (a - b))
print("Multiplicação: " .. (a * b))
print("Divisão:    " .. (a / b))
print("Módulo:     " .. (a % b))
print("Potência:   " .. (a ^ b))
print("Divisão int:" .. math.floor(a / b))
print("Raiz de 144: " .. math.sqrt(144))
print("Abs(-99):   " .. math.abs(-99))
print("Max(3,7,1): " .. math.max(3, 7, 1))
print("Min(3,7,1): " .. math.min(3, 7, 1))
print("Pi:         " .. math.pi)
print("")

-- ─── 3. Strings ───────────────────────────────────────────
print("-- [3] Strings --")

local s = "  Hello, Lunar World!  "

print("Original:   '" .. s .. "'")
print("Upper:      " .. string.upper(s))
print("Lower:      " .. string.lower(s))
print("Trim:       '" .. string.match(s, "^%s*(.-)%s*$") .. "'")
print("Length:     " .. #s)
print("Sub(3,7):   " .. string.sub(s, 3, 7))
print("Rep x3:     " .. string.rep("Lua!", 3, " | "))
print("Reverse:    " .. string.reverse("Lunar"))
print("Find 'World': " .. tostring(string.find(s, "World")))
print("Replace:    " .. string.gsub(s, "World", "Engine"))

-- Format
local fmt = string.format("Nome: %-10s | HP: %03d | XP: %.2f", "Guerreiro", 87, 1523.7)
print("Format:     " .. fmt)
print("")

-- ─── 4. Tabelas ───────────────────────────────────────────
print("-- [4] Tabelas --")

local frutas = {"maçã", "banana", "laranja", "uva", "melão"}

print("Tamanho: " .. #frutas)

for i, v in ipairs(frutas) do
    print("  [" .. i .. "] " .. v)
end

-- Inserir / remover
table.insert(frutas, "kiwi")
table.insert(frutas, 2, "pera")
table.remove(frutas, 4)

print("Após edições:")
print("  " .. table.concat(frutas, ", "))

-- Ordenar
table.sort(frutas)
print("Ordenado: " .. table.concat(frutas, ", "))
print("")

-- ─── 5. Tabelas como dicionário ───────────────────────────
print("-- [5] Dicionário --")

local jogador = {
    nome    = "Guerreiro",
    classe  = "Tank",
    nivel   = 15,
    hp      = 100,
    mana    = 40,
    stats   = { forca = 18, agilidade = 9, inteligencia = 5 }
}

print("Nome:    " .. jogador.nome)
print("Classe:  " .. jogador.classe)
print("Nível:   " .. jogador.nivel)
print("Stats:")
for k, v in pairs(jogador.stats) do
    print("  " .. k .. " = " .. v)
end
print("")

-- ─── 6. Condicionais ──────────────────────────────────────
print("-- [6] Condicionais --")

local hp = 35

if hp >= 80 then
    print("Status: Saudável")
elseif hp >= 50 then
    print("Status: Levemente ferido")
elseif hp >= 20 then
    print("Status: Ferido grave [HP: " .. hp .. "]")
else
    print("Status: CRÍTICO — quase morto!")
end

-- Ternário via and/or
local status = hp > 0 and "Vivo" or "Morto"
print("Vivo ou morto: " .. status)
print("")

-- ─── 7. Loops ─────────────────────────────────────────────
print("-- [7] Loops --")

-- for numérico
io.write("For numérico: ")
for i = 1, 10 do io.write(i .. " ") end
print("")

-- for step
io.write("Step de 2:    ")
for i = 0, 20, 2 do io.write(i .. " ") end
print("")

-- for reverso
io.write("Reverso:      ")
for i = 5, 1, -1 do io.write(i .. " ") end
print("")

-- while
local n = 1
io.write("While (2^n):  ")
while n <= 512 do
    io.write(n .. " ")
    n = n * 2
end
print("")

-- repeat until
local x = 100
io.write("Repeat:       ")
repeat
    io.write(x .. " ")
    x = x - 15
until x <= 0
print("")
print("")

-- ─── 8. Funções ───────────────────────────────────────────
print("-- [8] Funções --")

-- Função simples
local function fatorial(n)
    if n <= 1 then return 1 end
    return n * fatorial(n - 1)
end

for i = 0, 10 do
    print("  " .. i .. "! = " .. fatorial(i))
end
print("")

-- Múltiplos retornos
local function minMax(t)
    local mn, mx = t[1], t[1]
    for _, v in ipairs(t) do
        if v < mn then mn = v end
        if v > mx then mx = v end
    end
    return mn, mx
end

local nums = {5, 3, 8, 1, 9, 2, 7, 4, 6}
local mn, mx = minMax(nums)
print("Números: " .. table.concat(nums, ", "))
print("Min: " .. mn .. " | Max: " .. mx)
print("")

-- Varargs
local function somar(...)
    local total = 0
    for _, v in ipairs({...}) do total = total + v end
    return total
end
print("Soma varargs: " .. somar(10, 20, 30, 40, 50))
print("")

-- ─── 9. Closures ──────────────────────────────────────────
print("-- [9] Closures --")

local function criarAcumulador(inicio)
    local total = inicio or 0
    return function(n)
        total = total + n
        return total
    end
end

local acc = criarAcumulador(0)
print("Acumulador:")
print("  +10 = " .. acc(10))
print("  +25 = " .. acc(25))
print("  +5  = " .. acc(5))
print("  -8  = " .. acc(-8))
print("")

-- ─── 10. OOP com metatables ───────────────────────────────
print("-- [10] OOP / Metatables --")

local Personagem = {}
Personagem.__index = Personagem

function Personagem.new(nome, classe, hp, mana)
    return setmetatable({
        nome   = nome,
        classe = classe,
        hp     = hp,
        mana   = mana,
        nivel  = 1,
        xp     = 0
    }, Personagem)
end

function Personagem:atacar(alvo, dano)
    alvo.hp = alvo.hp - dano
    print("  " .. self.nome .. " atacou " .. alvo.nome .. " por " .. dano .. " de dano!")
    if alvo.hp <= 0 then
        print("  " .. alvo.nome .. " foi derrotado!")
        self.xp = self.xp + 50
        if self.xp >= self.nivel * 100 then
            self.nivel = self.nivel + 1
            print("  " .. self.nome .. " subiu para nível " .. self.nivel .. "!")
        end
    end
end

function Personagem:status()
    return string.format("[%s | %s | Nv.%d | HP:%d | Mana:%d | XP:%d]",
        self.nome, self.classe, self.nivel, self.hp, self.mana, self.xp)
end

local guerreiro = Personagem.new("Kael",    "Guerreiro", 120, 30)
local mago      = Personagem.new("Sylas",   "Mago",       70, 100)
local arqueiro  = Personagem.new("Faryn",   "Arqueiro",   90, 60)
local monstro   = Personagem.new("Goblin",  "Inimigo",    40, 0)
local chefe     = Personagem.new("DragonLord", "Boss",   200, 0)

print("Personagens criados:")
print("  " .. guerreiro:status())
print("  " .. mago:status())
print("  " .. arqueiro:status())
print("")

print("Batalha:")
guerreiro:atacar(monstro, 15)
mago:atacar(monstro, 20)
arqueiro:atacar(monstro, 12)
print("")

print("Status após batalha:")
print("  " .. guerreiro:status())
print("  " .. mago:status())
print("  " .. arqueiro:status())
print("")

-- ─── 11. Herança ──────────────────────────────────────────
print("-- [11] Herança --")

local Mago = setmetatable({}, { __index = Personagem })
Mago.__index = Mago

function Mago.new(nome)
    local self = Personagem.new(nome, "Mago", 70, 120)
    return setmetatable(self, Mago)
end

function Mago:lancarFeitico(alvo, feitico)
    local danos = { Bola_de_Fogo = 45, Gelo = 30, Raio = 55 }
    local dano = danos[feitico] or 20
    if self.mana >= 20 then
        self.mana = self.mana - 20
        print("  " .. self.nome .. " usou " .. feitico .. " em " .. alvo.nome .. "! (-" .. dano .. " HP)")
        alvo.hp = alvo.hp - dano
    else
        print("  " .. self.nome .. " não tem mana suficiente!")
    end
end

local magaLuna = Mago.new("Luna")
print("Mago criado: " .. magaLuna:status())
magaLuna:lancarFeitico(chefe, "Bola_de_Fogo")
magaLuna:lancarFeitico(chefe, "Raio")
magaLuna:atacar(chefe, 10)
print("Chefe após ataques: " .. chefe:status())
print("")

-- ─── 12. Tabelas avançadas ────────────────────────────────
print("-- [12] Algoritmos com Tabelas --")

-- Bubble sort
local function bubbleSort(t)
    local n = #t
    for i = 1, n do
        for j = 1, n - i do
            if t[j] > t[j+1] then
                t[j], t[j+1] = t[j+1], t[j]
            end
        end
    end
    return t
end

local arr = {64, 34, 25, 12, 22, 11, 90, 45, 3, 77}
print("Antes:  " .. table.concat(arr, ", "))
bubbleSort(arr)
print("Depois: " .. table.concat(arr, ", "))
print("")

-- Fibonacci com tabela de memoização
local memo = {}
local function fib(n)
    if memo[n] then return memo[n] end
    if n <= 1 then return n end
    memo[n] = fib(n-1) + fib(n-2)
    return memo[n]
end

io.write("Fibonacci (0-15): ")
for i = 0, 15 do
    io.write(fib(i) .. " ")
end
print("")
print("")

-- ─── 13. Manipulação de strings avançada ─────────────────
print("-- [13] Parsing de Strings --")

local csv = "Guerreiro,100,18,9,5;Mago,70,5,8,20;Arqueiro,90,10,18,7"

print("CSV parseado:")
for linha in string.gmatch(csv, "[^;]+") do
    local campos = {}
    for campo in string.gmatch(linha, "[^,]+") do
        table.insert(campos, campo)
    end
    print(string.format("  Nome: %-10s HP: %-4s FOR: %-3s AGI: %-3s INT: %s",
        campos[1], campos[2], campos[3], campos[4], campos[5]))
end
print("")

-- ─── 14. Pcall / Error handling ───────────────────────────
print("-- [14] Tratamento de Erros --")

local function dividir(a, b)
    if b == 0 then error("divisão por zero!", 2) end
    return a / b
end

local ok, resultado = pcall(dividir, 10, 2)
print("10 / 2 = " .. (ok and resultado or "ERRO: " .. resultado))

local ok2, err = pcall(dividir, 5, 0)
print("5 / 0 = " .. (ok2 and resultado or "ERRO: " .. err))
print("")

-- ─── 15. Funções de alta ordem ────────────────────────────
print("-- [15] Map / Filter / Reduce --")

local function map(t, fn)
    local r = {}
    for i, v in ipairs(t) do r[i] = fn(v) end
    return r
end

local function filter(t, fn)
    local r = {}
    for _, v in ipairs(t) do
        if fn(v) then table.insert(r, v) end
    end
    return r
end

local function reduce(t, fn, acc)
    for _, v in ipairs(t) do acc = fn(acc, v) end
    return acc
end

local valores = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}

local dobrados  = map(valores, function(v) return v * 2 end)
local pares     = filter(valores, function(v) return v % 2 == 0 end)
local somaTotal = reduce(valores, function(a, b) return a + b end, 0)

print("Original: " .. table.concat(valores, ", "))
print("Dobrados: " .. table.concat(dobrados, ", "))
print("Pares:    " .. table.concat(pares, ", "))
print("Soma:     " .. somaTotal)
print("")

-- ─── Fim ──────────────────────────────────────────────────
print("=== TESTE CONCLUÍDO COM SUCESSO ===")
