📜 Game Design Document (GDD): Twins Defense
1. Visão Geral do Projeto
Título: Twins Defense

Gênero: Tower Defense 2D / Strategy / Puzzle

Plataforma Inicial: PC (Steam)

Motor Gráfico: Unity 2D

Estilo Visual: Cartoon Sombrio/Expressivo (Inspirado em Don't Starve)

Perspectiva: Top-Down 2D

Premissa Temática: Hordas de monstros e fantasmas estão invadindo, e a única defesa é o trio dinâmico composto por Izzy, Court e o cachorro Ralph, que utilizam feitiços, disfarces e habilidades especiais para proteger o caminho.

2. Mecânicas Principais & Gameplay Loop
2.1 Estrutura de Mapa e Posicionamento
Tipo de Mapa: Caminho Fixo (Estilo Bloons TD 6). Os monstros/fantasmas seguem rotas predefinidas até o objetivo final.

Regra de Posição:

O jogador possui no máximo 3 unidades em campo por vez: 1 Izzy, 1 Court e 1 Ralph.

Cada fase exige encontrar a combinação/configuração ideal das 3 torres para lidar com os tipos específicos de inimigos e chefes.

2.2 Estrutura de Ondas (Waves) e Bosses
Cada fase contém múltiplas ondas de monstros e fantasmas normais/rápidos/resistentes.

Mid-Boss (Meio da Fase): Um mini-chefe surge para testar a sinergia inicial da equipe.

Final Boss (Fim da Fase): Um chefe com mecânicas únicas e grande quantidade de vida. Ao ser derrotado, conclui a fase.

3. As Torres & Roster de Personagens
3.1 Izzy (DPS / Ofensiva Flexível)
Base: Izzy (Ataques básicos de dano físico/mágico à distância).

Evoluções / Subclasses (Desbloqueadas na Árvore de Talentos):

Izzy Ranger: Foco em alcance massivo, tiros rápidos e dano perfurante em alvo único.

Izzy Fire Witch: Foco em dano de Fogo em Área (AoE) e efeitos de queimadura contínua (DoT).

Izzy Pop Star: Foco em atordoamento temporário (Charm/Stun) e danos múltiplos em cadeia.

3.2 Court (DPS / Controle de Multidões & Tática)
Base: Court (Ataques táticos de médio alcance).

Evoluções / Subclasses (Desbloqueadas na Árvore de Talentos):

Court Ice Witch: Foco em congelamento, estilhaçamento de gelo e desaceleração de grandes grupos.

Court Megabrain: Foco em rajadas elétricas/raios que saltam entre inimigos e dano proporcional à vida.

Court Evil: Foco em magias de trevas, drenagem e amplificação de dano recebido pelos monstros.

3.3 Ralph, the Dog (Suporte Fixo)
Base: Ralph (Unidade de Suporte focada em auras ao seu redor).

Evoluções / Subclasses (Desbloqueadas na Árvore de Talentos):

Ralph the Priest: Aura de aumento de alcance e dano total para Izzy e Court quando posicionadas próximas.

Ralph the Paladin: Aura de aumento de velocidade de ataque (Attack Speed) e buffs defensivos para o mapa.

Ralph the Cute: Usa o olhar fofo estilo Gato de Botas para encantar e causar lentidão extrema (Slow) nos monstros que passam perto.

Total de Variantes: 12 Torres (3 bases + 9 ramificações desbloqueáveis).

4. Economia e Progressão
4.1 Economia Dentro da Partida (In-Game)
Recurso Único — Gems (Gemas):

Derrubadas por todos os monstros e fantasmas derrotados durante as ondas.

Utilizadas durante a partida para invocar, melhorar status (Upgrades) e alternar as variantes de Izzy, Court e Ralph.

4.2 Economia Fora da Partida (Meta-Progresso)
Pontos de Talento:

Recompensa obtida ao derrotar os Bosses no final das fases.

Árvore de Talentos (3 Ramificações Principais):

Ramo da Izzy: Desbloqueia Ranger, Fire Witch e Pop Star + bônus passivos de dano.

Ramo da Court: Desbloqueia Ice Witch, Megabrain e Evil Court + bônus passivos de utilidade.

Ramo do Ralph: Desbloqueia The Priest, The Paladin e The Cute + bônus de alcance de aura e eficiência de suporte.

5. Arquitetura Técnica Recomendada (Unity)
Para garantir flexibilidade com o Claude e facilidade no Git:

Data-Driven Design (ScriptableObjects):

TowerData.cs: ScriptableObject contendo Dano, FireRate, Alcance, Prefab Visual, Custo em Gemas e Efeitos (Fogo, Gelo, Aura).

WaveData.cs: ScriptableObject com a lista de inimigos por onda, tempo de spawner e recompensas de Gemas.

Grid & Pathing:

Uso do Tilemap 2D nativo da Unity para estradas e Nodes de Posicionamento predefinidos para as 3 torres.

Inimigos utilizam Waypoints simples para navegação ao longo do caminho.
