📜 Game Design Document (GDD): Twins Defense

## 1. Visão Geral do Projeto

**Título:** Twins Defense

**Gênero:** Roguelite Survival / Auto-Battler 2D (Inspirado em Vampire Survivors)

**Plataforma Inicial:** PC (Steam)

**Motor Gráfico:** Unity 2D

**Estilo Visual:** Cartoon Sombrio/Expressivo (Inspirado em Don't Starve)

**Perspectiva:** Top-Down 2D

**Premissa Temática:** Hordas de monstros e fantasmas invadem infinitamente uma arena, e a única defesa é o trio dinâmico composto por Izzy, Court e o cachorro Ralph, que utilizam feitiços, disfarces e habilidades especiais para sobreviver o máximo de tempo possível.

> ⚠️ Mudança de escopo (v2): O jogo deixou de ser um Tower Defense de fases fixas e virou um Survival Roguelite de arena aberta, com progressão infinita, cartas de upgrade aleatórias e evolução de personagem in-run.

---

## 2. Fluxo de Telas

1. **Main Menu:** `PLAY` — `SETTINGS` — `EXIT`
2. **Character Select:** Exibe os ícones desenhados dos personagens. Na primeira versão, apenas as formas **BASE** de Izzy, Court e Ralph estão disponíveis para seleção (as evoluções são desbloqueadas jogando).
3. **Arena Run:** Gameplay principal — sobrevivência com spawn contínuo de inimigos.
4. **Level Up (Pausa + 3 Cartas):** Ao subir de nível, o jogo pausa e apresenta 3 cartas aleatórias de upgrade.
5. **Boss Encounter:** A cada 10 levels dentro da run.
6. **Fim de Run (Morte ou Extração):** Contabiliza Boss Points ganhos.
7. **Meta Progression (Star System):** Tela fora da run para gastar Boss Points e estrelar personagens.

---

## 3. Core Gameplay Loop

### 3.1 Estrutura da Arena
- **Tipo de Mapa:** Arena aberta (sem caminho fixo). O jogador controla diretamente o movimento do personagem principal.
- **Composição de Time:** O jogador é **SOLO**. Na Character Select, escolhe **1 entre Izzy, Court ou Ralph** e enfrenta a arena sozinho contra a horda — não há companions automáticos em campo. Cada personagem precisa ser autossuficiente (ataque próprio + kit de sobrevivência), inclusive o Ralph, que deixa de ser um "suporte puro para aliados" e passa a ter ataque básico próprio (ver seção 4.3).

### 3.2 Progressão Dentro da Run
- Inimigos derrotados dropam **XP** (substituindo as Gems como recurso primário de progresso em partida).
- Ao subir de nível: jogo pausa → aparecem **3 cartas aleatórias** de upgrade, ex:
  - `+ATK` (dano base)
  - `+Projectiles` (número de projéteis)
  - `+MoveSpeed`
  - `+HP` (vida máxima)
  - `+AOE` (raio de área de efeito)
  - `+AttackSpeed`
  - `+Pickup Radius` (raio de coleta de XP/drops)
  - `+Crit Chance` / `+Crit Damage`

### 3.3 Escalonamento de Dificuldade
- A cada level dentro da run:
  - `spawnRate` dos inimigos aumenta (menor intervalo entre spawns).
  - `enemyHP` recebe multiplicador crescente.
- **A cada 10 levels:** Spawna um **Boss** com barra de vida própria e mecânica única.
  - Ao derrotar o Boss: jogador ganha **Boss Points** (moeda meta, usada fora da run).

### 3.4 Evolução de Personagem (In-Run)
Sistema de evolução **linear e sequencial**, disparado ao atingir level 10 na forma atual:

| Personagem | Cadeia de Evolução |
|---|---|
| **Izzy** | Base (lvl 10) → Izzy Fire (lvl 10) → Izzy Ranger (lvl 10) → Izzy Popstar |
| **Court** | Base (lvl 10) → Court Ice (lvl 10) → Court Megabrain (lvl 10) → Court Evil |
| **Ralph** | Base (lvl 10) → Ralph Priest (lvl 10) → Ralph Paladin (lvl 10) → Ralph Cute |

Cada evolução troca o kit de ataque, o passivo e o poder especial do personagem (ver seção 5), mantendo a progressão de level acumulada.

---

## 4. As Torres/Personagens & Roster

### 4.1 Izzy (Attacker / Ofensiva Flexível)
- **Base:** Ataques básicos de dano físico/mágico à distância.
- **Evoluções (em ordem):**
  - **Izzy Fire:** Dano de Fogo em Área (AoE) e queimadura contínua (DoT).
  - **Izzy Ranger:** Alcance massivo, tiros perfurantes em alvo único/linha.
  - **Izzy Popstar:** Atordoamento (Charm/Stun) e dano em cadeia.

### 4.2 Court (Attacker / Controle de Multidões & Tática)
- **Base:** Ataques táticos de médio alcance com marcação de alvo.
- **Evoluções (em ordem):**
  - **Court Ice:** Congelamento, estilhaçamento de gelo e desaceleração de grupos.
  - **Court Megabrain:** Rajadas elétricas que saltam entre inimigos, dano proporcional à vida.
  - **Court Evil:** Drenagem e amplificação de dano recebido pelos monstros.

### 4.3 Ralph, the Dog (Self-Sustain / Short-Range Brawler)
- **Base:** Personagem solo que também atira projéteis (Bark Shot), porém com **range bem menor** que Izzy/Court — estilo "melee" de Realm of the Mad God, onde o personagem ainda dispara à distância, mas precisa ficar mais perto dos inimigos pra acertar. Aura passiva buffa a si mesmo.
- **Evoluções (em ordem):**
  - **Ralph Priest:** Regeneração de HP própria contínua + buff temporário de tudo (era cura de aliados, agora self-heal).
  - **Ralph Paladin:** Reflete dano recebido (Thorns) + invulnerabilidade temporária.
  - **Ralph Cute:** Encanta e causa slow extremo em inimigos próximos (crowd control em área, não depende de aliados).

**Total de Variantes:** 12 personagens (3 bases + 9 evoluções desbloqueáveis in-run).

---

## 5. Poderes Especiais (Passivos + Ultimates)

| Personagem | Estilo de Ataque | Passivo | Poder Especial |
|---|---|---|---|
| Izzy (Base) | Bolt mágico que ricocheteia entre inimigos próximos | Chance de ricochete extra por acerto | **Arcane Overload** — rajada periódica de bolts em todas as direções |
| Izzy Fire | Bola de fogo com DoT | Inimigos queimando espalham fogo por contato | **Wildfire Nova** — pulso de fogo em área ao redor da Izzy |
| Izzy Ranger | Tiro perfurante em linha reta | Dano aumenta com a distância percorrida | **Piercing Volley** — saraivada de flechas que atravessa a tela |
| Izzy Popstar | Ataques com chance de stun | Stun pode "encantar" o inimigo contra os outros | **Encore** — stun em área + charme em cadeia |
| Court (Base) | Ataque tático que marca o alvo | Alvo marcado recebe dano bônus de todas as fontes | **Tactical Strike** — alvo marcado explode ao morrer e marca o próximo |
| Court Ice | Projéteis com slow acumulativo | 3 stacks de slow = congelamento total | **Absolute Zero** — congela todos os inimigos num raio grande |
| Court Megabrain | Raio que salta entre inimigos | Dano do salto escala com % da vida do alvo | **Overload Circuit** — descarga elétrica que percorre toda a tela por alguns segundos |
| Court Evil | Drena vida e amaldiçoa (amplifica dano recebido) | Vida drenada cura a própria Court | **Void Pact** — amaldiçoa todos os inimigos na tela por X segundos |
| Ralph (Base) | Bark Shot — projétil de curto alcance ("melee" estilo Realm of the Mad God: range bem menor que Izzy/Court, mas ainda é à distância) | Aura própria: quanto mais inimigos por perto, mais dano/velocidade de ataque ele ganha | **Loyal Bark** — latido em área que empurra inimigos e dobra o próprio dano por alguns segundos |
| Ralph Priest | Bark Shot com alcance levemente maior (ainda curto) | Regenera HP próprio continuamente | **Blessing Howl** — cura grande parte da vida máxima e concede buff total temporário em si mesmo |
| Ralph Paladin | Bark Shot de curto alcance | **Thorns** — reflete parte do dano recebido de volta pra quem acertou ele | **Guardian Stance** — fica invulnerável/imortal por alguns segundos |
| Ralph Cute | Bark Shot fraco, foco total em CC | Inimigos enfeitiçados por perto não atacam | **Puppy Eyes** — pulso gigante que enfeitiça/paralisa quase toda a tela |

---

## 6. Economia e Progressão

### 6.1 Dentro da Run (In-Game)
**Recurso Único — XP:**
- Dropado por todos os monstros/fantasmas derrotados.
- Usado exclusivamente para leveling automático (não há compra manual de upgrades — os upgrades vêm das 3 cartas por level up).

### 6.2 Fora da Run (Meta-Progresso)
**Boss Points:**
- Ganhos ao derrotar Bosses (a cada 10 levels dentro da run).
- Usados na tela de Meta Progression para **estrelar** (Star System) os personagens — aumentando seus stats-base para as próximas runs.

**Star System (1–5 estrelas por personagem):**
- Substitui a economia de Gems da versão anterior do GDD.
- Aplica-se aos stats base de cada personagem (Izzy, Court, Ralph), refletindo em todas as suas evoluções dentro da run.

---

## 7. Arquitetura Técnica Recomendada (Unity)

Para garantir flexibilidade com o Claude e facilidade no Git:

### Data-Driven Design (ScriptableObjects):
- `CharacterData.cs`: Dano, FireRate, Alcance, Prefab Visual, Passivo, Poder Especial, e referência à próxima evolução da cadeia.
- `EvolutionData.cs`: Define o gatilho (level da forma atual) e qual `CharacterData` desbloqueia.
- `SkillCardData.cs`: Define os upgrades possíveis nas cartas de level up (+ATK, +Projectiles, +MoveSpeed, +HP, +AOE, etc.) e seus valores de escala.
- `WaveScalingConfig.cs`: Curvas de `spawnRate` e multiplicador de `enemyHP` por level dentro da run.
- `BossData.cs`: ScriptableObject com stats, mecânica única e recompensa de Boss Points.
- `StarProgressionData.cs`: Persiste o nível de estrela de cada personagem entre runs.

### Grid & Movimento:
- Arena aberta sem Tilemap de caminho fixo — jogador se move livremente.
- Inimigos usam pathing simples (ex: seguir o jogador via `NavMeshAgent2D` ou steering behavior) em vez de Waypoints fixos.
