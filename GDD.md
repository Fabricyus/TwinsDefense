📜 Game Design Document (GDD): Twins Defense

## 1. Visão Geral do Projeto

**Título:** Twins Defense

**Gênero:** Roguelite Survival / Auto-Battler 2D (Inspirado em Vampire Survivors)

**Plataforma Inicial:** PC (Steam)

**Motor Gráfico:** Unity 2D

**Estilo Visual:** Cartoon Sombrio/Expressivo (Inspirado em Don't Starve)

**Perspectiva:** Top-Down 2D

**Premissa Temática:** Hordas de monstros e fantasmas invadem infinitamente uma arena, e a única defesa é o trio dinâmico composto por Izzy, Court e o cachorro Ralph, que utilizam feitiços, disfarces e habilidades especiais para sobreviver o máximo de tempo possível.

> ⚠️ Histórico de mudanças de escopo:
> - **v2:** O jogo deixou de ser um Tower Defense de fases fixas e virou um Survival Roguelite de arena aberta, com progressão infinita, cartas de upgrade aleatórias e evolução de personagem in-run.
> - **v3 (atual):** A evolução automática in-run (troca de forma ao bater level 10 dentro da run) foi **removida**. As 12 formas de personagem (3 bases + 9 variantes) são desbloqueadas por **conquistas** e escolhidas na **Character Select**, cada uma com sua própria trilha de estrelas comprada com Coin — dentro da run, a única progressão é leveling + cartas. **Boss Points** e **Poderes Especiais/Ultimates** foram removidos do escopo. O Star System não tem tela própria: vive dentro da Character Select.

---

## 2. Fluxo de Telas

1. **Main Menu:** `PLAY` — `SETTINGS` — `EXIT`
2. **Character Select:** Exibe os ícones desenhados dos personagens. As 12 formas (Base + evoluções de Izzy, Court e Ralph) aparecem como slots independentes, desbloqueados por conquistas e evoluídos com estrelas (1–5) compradas com Coin acumulada em runs anteriores. **É aqui, e só aqui, que a meta-progressão acontece** — não existe uma tela separada de Meta Progression.
3. **Arena Run:** Gameplay principal — sobrevivência com spawn contínuo de inimigos.
4. **Level Up (Pausa + 3 Cartas):** Ao subir de nível, o jogo pausa e apresenta 3 cartas aleatórias de upgrade. É a única forma de progressão dentro da run.
5. **Boss Encounter:** A cada 10 levels dentro da run, com pelo menos 3 bosses distintos no roster.
6. **Fim de Run (Morte ou Extração):** Contabiliza a Coin ganha na run (kills, XP, level atingido) e progresso de conquistas.

---

## 3. Core Gameplay Loop

### 3.1 Estrutura da Arena
- **Tipo de Mapa:** Arena aberta (sem caminho fixo). O jogador controla diretamente o movimento do personagem principal.
- **Composição de Time:** O jogador é **SOLO**. Na Character Select, escolhe **1 forma entre as desbloqueadas de Izzy, Court ou Ralph** e enfrenta a arena sozinho contra a horda — não há companions automáticos em campo. Cada personagem precisa ser autossuficiente (ataque próprio + kit de sobrevivência), inclusive o Ralph, que deixa de ser um "suporte puro para aliados" e passa a ter ataque básico próprio (ver seção 4.3).

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
- **Leveling + cartas é o único mecanismo de progressão dentro da run.** A forma de personagem escolhida na Character Select fica fixa a run inteira (ver 3.4).

### 3.3 Escalonamento de Dificuldade
- A cada level dentro da run:
  - `spawnRate` dos inimigos aumenta (menor intervalo entre spawns).
  - `enemyHP` recebe multiplicador crescente.
  - ⚠️ Curvas ainda em balanceamento ativo — prioridade atual antes do MVP fechar (ver `WaveScalingConfig.cs`, seção 7).
- **A cada 10 levels:** Spawna um **Boss** com barra de vida própria e mecânica única. Roster mínimo de **3 bosses distintos** (não o mesmo boss reciclado em cópias).
  - Ao derrotar o Boss: jogador ganha Coin, contabilizada junto com o resto do placar da run — **não existe mais uma moeda separada de Boss** (Boss Points foi removido).

### 3.4 Formas de Personagem (Meta-Progressão, fora da Run)
As formas dos personagens **não evoluem automaticamente dentro da run** — a evolução in-run foi removida do escopo. Cada forma é uma variante selecionável independente na Character Select:

| Personagem | Formas Disponíveis |
|---|---|
| **Izzy** | Base, Izzy Fire, Izzy Ranger, Izzy Popstar |
| **Court** | Base, Court Ice, Court Megabrain, Court Evil |
| **Ralph** | Base, Ralph Priest, Ralph Paladin, Ralph Cute |

- **Desbloqueio:** conquistas (achievements) completadas jogando.
- **Progressão:** cada forma tem sua própria trilha de 1–5 estrelas, comprada com Coin na Character Select — estrelas de uma forma **não** cascateiam para as outras formas do mesmo personagem.
- **Dentro da run:** a forma escolhida na Character Select permanece fixa; a única progressão é leveling (XP) + cartas de upgrade (seção 3.2).

---

## 4. As Torres/Personagens & Roster

### 4.1 Izzy (Attacker / Ofensiva Flexível)
- **Base:** Ataques básicos de dano físico/mágico à distância.
- **Formas (desbloqueáveis por conquista):**
  - **Izzy Fire:** Dano de Fogo em Área (AoE) e queimadura contínua (DoT).
  - **Izzy Ranger:** Alcance massivo, tiros perfurantes em alvo único/linha.
  - **Izzy Popstar:** Atordoamento (Charm/Stun) e dano em cadeia.

### 4.2 Court (Attacker / Controle de Multidões & Tática)
- **Base:** Ataques táticos de médio alcance com marcação de alvo.
- **Formas (desbloqueáveis por conquista):**
  - **Court Ice:** Congelamento, estilhaçamento de gelo e desaceleração de grupos.
  - **Court Megabrain:** Rajadas elétricas que saltam entre inimigos, dano proporcional à vida.
  - **Court Evil:** Drenagem e amplificação de dano recebido pelos monstros.

### 4.3 Ralph, the Dog (Self-Sustain / Short-Range Brawler)
- **Base:** Personagem solo que também atira projéteis (Bark Shot), porém com **range bem menor** que Izzy/Court — estilo "melee" de Realm of the Mad God, onde o personagem ainda dispara à distância, mas precisa ficar mais perto dos inimigos pra acertar. Aura passiva buffa a si mesmo.
- **Formas (desbloqueáveis por conquista):**
  - **Ralph Priest:** Regeneração de HP própria contínua + buff temporário de tudo (era cura de aliados, agora self-heal).
  - **Ralph Paladin:** Reflete dano recebido (Thorns) + invulnerabilidade temporária.
  - **Ralph Cute:** Encanta e causa slow extremo em inimigos próximos (crowd control em área, não depende de aliados).

**Nota de produção:** os prefabs de projétil do Ralph permanecem os atuais para todas as formas (não há orçamento de arte para novos sprites de projétil agora) — a diferenciação visual entre formas deve vir de partículas/VFX distintos por cima do mesmo projétil base.

**Total de Variantes:** 12 personagens (3 bases + 9 formas), cada um desbloqueado por conquista e evoluído com estrelas próprias na Character Select — não há evolução automática dentro da run (ver 3.4).

---

## 5. Estilo de Ataque e Passivos

Poderes especiais/ultimates foram removidos do escopo. A identidade mecânica de cada forma vem do kit de ataque + passivo abaixo, combinado com as cartas de upgrade (seção 3.2).

| Personagem | Estilo de Ataque | Passivo |
|---|---|---|
| Izzy (Base) | Bolt mágico que ricocheteia entre inimigos próximos | Chance de ricochete extra por acerto |
| Izzy Fire | Bola de fogo com DoT | Inimigos queimando espalham fogo por contato |
| Izzy Ranger | Tiro perfurante em linha reta | Dano aumenta com a distância percorrida |
| Izzy Popstar | Ataques com chance de stun | Stun pode "encantar" o inimigo contra os outros |
| Court (Base) | Ataque tático que marca o alvo | Alvo marcado recebe dano bônus de todas as fontes |
| Court Ice | Projéteis com slow acumulativo | 3 stacks de slow = congelamento total |
| Court Megabrain | Raio que salta entre inimigos | Dano do salto escala com % da vida do alvo |
| Court Evil | Drena vida e amaldiçoa (amplifica dano recebido) | Vida drenada cura a própria Court |
| Ralph (Base) | Bark Shot — projétil de curto alcance ("melee" estilo Realm of the Mad God: range bem menor que Izzy/Court, mas ainda é à distância) | Aura própria: quanto mais inimigos por perto, mais dano/velocidade de ataque ele ganha |
| Ralph Priest | Bark Shot com alcance levemente maior (ainda curto) | Regenera HP próprio continuamente |
| Ralph Paladin | Bark Shot de curto alcance | **Thorns** — reflete parte do dano recebido de volta pra quem acertou ele |
| Ralph Cute | Bark Shot fraco, foco total em CC | Inimigos enfeitiçados por perto não atacam |

---

## 6. Economia e Progressão

### 6.1 Dentro da Run (In-Game)
- **XP:** dropado por todos os monstros/fantasmas derrotados. Usado exclusivamente para leveling automático (não há compra manual de upgrades — os upgrades vêm das 3 cartas por level up).
- **Coin:** acumulada ao longo da run (kills, XP coletado, level atingido). Não é gasta dentro da run — só contabilizada no fim para a meta-progressão.

### 6.2 Fora da Run (Meta-Progresso) — só na Character Select
- **Conquistas (Achievements):** completadas jogando, desbloqueiam as 9 formas evoluídas (Izzy Fire, Court Ice, Ralph Priest, etc.) para seleção.
- **Coin:** acumulada nas runs, gasta na Character Select para comprar estrelas.
- **Star System (1–5 estrelas por forma):** cada uma das **12 formas** (não cada personagem-base) tem sua própria trilha de estrelas, aumentando os stats-base daquela forma especificamente. Estrelas **não** cascateiam entre formas do mesmo personagem.
- **Boss Points foi removido:** derrotar Boss não dá moeda diferenciada, só contribui pro placar de Coin da run como qualquer outro progresso.

---

## 7. Arquitetura Técnica Recomendada (Unity)

Para garantir flexibilidade com o Claude e facilidade no Git:

### Data-Driven Design (ScriptableObjects):
- `CharacterData.cs`: Dano, FireRate, Alcance, Prefab Visual, Passivo. Cada forma é uma entrada independente — sem referência a "próxima evolução" (não há mais cadeia automática in-run).
- `CharacterUnlockCondition.cs`: Condição de conquista que desbloqueia cada forma para seleção na Character Select.
- `SkillCardData.cs`: Define os upgrades possíveis nas cartas de level up (+ATK, +Projectiles, +MoveSpeed, +HP, +AOE, etc.) e seus valores de escala.
- `WaveScalingConfig.cs`: Curvas de `spawnRate` e multiplicador de `enemyHP` por level dentro da run. **Prioridade atual: balanceamento real das curvas.**
- `BossData.cs`: Stats e mecânica única por boss. Recompensa é Coin, igual ao resto da run. Mínimo de 3 bosses distintos no roster.
- `StarProgressionData.cs`: Persiste o nível de estrela de **cada forma** (não por personagem-base) entre runs, comprado com Coin na Character Select.

### Grid & Movimento:
- Arena aberta sem Tilemap de caminho fixo — jogador se move livremente.
- Inimigos usam pathing simples (ex: seguir o jogador via `NavMeshAgent2D` ou steering behavior) em vez de Waypoints fixos.
