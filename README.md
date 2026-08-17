# SpeedRim

Mod para **RimWorld 1.6** que adiciona as velocidades de jogo **5x**, **10x** e **20x** aos
controles de tempo originais (pausa / 1x / 2x / 3x).

*A RimWorld 1.6 mod that adds **5x**, **10x** and **20x** game speeds. English notes are at the
bottom of this file.*

![Preview](About/Preview.png)

## O que o mod faz

- **Tres botoes extras** ao lado dos controles de tempo do jogo, no canto inferior direito.
- **Atalhos de teclado**: `5`, `6` e `7` por padrao, reconfiguraveis em *Opcoes -> Configuracao de teclado*.
- As teclas originais de **acelerar/desacelerar** continuam funcionando e agora sobem ate 20x.
- **Configuracoes do mod** (*Opcoes -> Configuracoes de mods -> SpeedRim*):
  - multiplicador de cada botao, de 2x a 100x;
  - mostrar ou esconder a fileira de botoes;
  - deslocar a fileira na horizontal e na vertical (util quando outro mod ocupa o mesmo canto);
  - respeitar ou nao os momentos em que o jogo forca a velocidade normal (chegada de ataques, por exemplo);
  - estender ou nao as teclas de acelerar/desacelerar.

## Velocidade real

Os multiplicadores sao **alvos**, nao garantias. O RimWorld processa quantos ticks couberem no
quadro e para assim que a taxa de quadros minima seria ultrapassada, entao colonias grandes ou
processadores mais lentos rodam abaixo do numero mostrado no botao — o jogo fica mais lento que o
alvo, mas nao trava.

## Instalacao

1. Instale o [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) (dependencia obrigatoria).
2. Copie a pasta deste repositorio para dentro da pasta `Mods` do RimWorld, de modo que fique assim:

   ```
   RimWorld/Mods/SpeedRim/About/About.xml
   RimWorld/Mods/SpeedRim/1.6/Assemblies/SpeedRim.dll
   ```

3. Ative **SpeedRim** na lista de mods, depois do Harmony.

O `SpeedRim.dll` ja vem compilado no repositorio, entao nao e necessario compilar nada para jogar.

## Compilando do codigo-fonte

Requer o [.NET SDK](https://dotnet.microsoft.com/download) (8.0 ou mais recente). As referencias do
RimWorld 1.6 vem do pacote NuGet `Krafs.Rimworld.Ref`, ou seja, nao e preciso ter o jogo instalado
para compilar — e funciona em Windows, Linux e macOS:

```bash
cd Source/SpeedRim
dotnet build -c Release
```

O resultado e gravado direto em `1.6/Assemblies/SpeedRim.dll`.

As texturas dos botoes e a imagem de preview sao geradas por script (requer `pillow`):

```bash
python3 Source/Tools/generate_textures.py
```

## Estrutura do repositorio

```
About/                      About.xml e Preview.png
Defs/                       KeyBindingDefs dos atalhos 5 / 6 / 7
Languages/                  Textos em ingles e portugues (Brasil)
Textures/SpeedRim/          Texturas dos tres botoes
1.6/Assemblies/SpeedRim.dll Assembly compilada, carregada pelo jogo
Source/SpeedRim/            Codigo-fonte C# e o projeto .csproj
Source/Tools/               Script que gera as texturas
```

## Como funciona por dentro

A `TimeSpeed` original do RimWorld vai de `Paused` (0) a `Ultrafast` (4). O mod usa os valores 5, 6
e 7 dessa mesma enumeracao para as velocidades novas e responde por elas em tres patches Harmony:

| Patch | Alvo | Papel |
| --- | --- | --- |
| `TickManager_TickRateMultiplier_Patch` | `TickManager.TickRateMultiplier` | Devolve o multiplicador configurado para as velocidades novas; as originais seguem intocadas. Respeita a pausa, a velocidade normal forcada e o ritmo acelerado do mapa-mundi. |
| `TimeControls_DoTimeControlsGUI_Patch` | `TimeControls.DoTimeControlsGUI` | Desenha os tres botoes a esquerda da fileira original e trata os atalhos (o prefixo roda antes do jogo consumir as teclas). |
| `TickManager_ExposeData_Patch` | `TickManager.ExposeData` | Grava uma velocidade original no arquivo salvo e devolve a velocidade extra logo em seguida. |

Guardar a velocidade ativa dentro do proprio `curTimeSpeed` do jogo faz com que pausar/despausar,
salvar/carregar e outros mods que mudam a velocidade continuem funcionando sem estado paralelo. O
patch em `ExposeData` e o que mantem os saves compativeis com o jogo sem o mod: um save feito em 20x
carrega em velocidade Super-rapida caso o SpeedRim seja removido, em vez de ficar com um valor
desconhecido.

## Compatibilidade

- Pode ser adicionado e removido de partidas em andamento.
- Nao substitui nenhum metodo do jogo por completo: os patches sao prefixos/postfixos que deixam o
  caminho original intacto para as velocidades originais.
- Funciona junto com outros mods que mexem na velocidade, desde que eles nao substituam
  `TickRateMultiplier` inteiro.

---

## English

SpeedRim adds **5x**, **10x** and **20x** game speeds to RimWorld 1.6, as three extra buttons next
to the vanilla time controls plus hotkeys `5`, `6` and `7` (rebindable). Every multiplier is
configurable between 2x and 100x in the mod settings, the button row can be hidden or moved, and the
vanilla speed-up/slow-down keys climb all the way to the new top speed.

The multipliers are targets: RimWorld stops ticking a frame once it would fall below its minimum
framerate, so a heavy colony runs slower than the number on the button instead of freezing.

**Install**: install [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077),
copy this folder into `RimWorld/Mods/`, enable SpeedRim after Harmony. The compiled
`1.6/Assemblies/SpeedRim.dll` is committed, so no build step is needed to play.

**Build**: `cd Source/SpeedRim && dotnet build -c Release` — RimWorld references come from the
`Krafs.Rimworld.Ref` NuGet package, so the game does not need to be installed and the build works on
Windows, Linux and macOS.

**Safe to remove**: saves store a vanilla speed value, so a save made at 20x still loads correctly
without the mod.
