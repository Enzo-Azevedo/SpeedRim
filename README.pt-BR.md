# SpeedRim

Mod para **RimWorld 1.6** que adiciona as velocidades de jogo **5x**, **10x** e **20x** aos
controles de tempo originais (pausa / 1x / 2x / 3x).

📖 **Read in English: [README.md](README.md)**

![Preview](About/Preview.png)

## O que o mod faz

- **Três botões extras** ao lado dos controles de tempo do jogo, no canto inferior direito.
- **Atalhos de teclado**: `5`, `6` e `7` por padrão, reconfiguráveis em *Opções -> Configuração de teclado*.
- As teclas originais de **acelerar/desacelerar** continuam funcionando e agora sobem até 20x.
- **Configurações do mod** (*Opções -> Configurações de mods -> SpeedRim*):
  - o multiplicador de cada botão, de 2x a 100x;
  - mostrar ou esconder a fileira de botões;
  - deslocar a fileira na horizontal e na vertical, útil quando outro mod ocupa o mesmo canto;
  - respeitar ou não os momentos em que o jogo força a velocidade normal (a chegada de um ataque, por exemplo);
  - estender ou não as teclas de acelerar/desacelerar.

## Velocidade real

Os multiplicadores são **alvos**, não garantias. O RimWorld processa quantos ticks couberem no
quadro e para assim que a taxa de quadros mínima seria ultrapassada, então colônias grandes ou
processadores mais lentos rodam abaixo do número mostrado no botão — o jogo fica mais lento que o
alvo, mas não trava.

## Instalação

1. Instale o [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) (dependência obrigatória).
2. Copie a pasta deste repositório para dentro da pasta `Mods` do RimWorld, de modo que fique assim:

   ```
   RimWorld/Mods/SpeedRim/About/About.xml
   RimWorld/Mods/SpeedRim/1.6/Assemblies/SpeedRim.dll
   ```

3. Ative o **SpeedRim** na lista de mods, depois do Harmony.

O `SpeedRim.dll` já vem compilado no repositório, então não é necessário compilar nada para jogar.

## Compilando do código-fonte

Requer o [.NET SDK](https://dotnet.microsoft.com/download) (8.0 ou mais recente). As referências do
RimWorld 1.6 vêm do pacote NuGet `Krafs.Rimworld.Ref`, ou seja, não é preciso ter o jogo instalado
para compilar — e funciona em Windows, Linux e macOS:

```bash
cd Source/SpeedRim
dotnet build -c Release
```

O resultado é gravado direto em `1.6/Assemblies/SpeedRim.dll`.

As texturas dos botões e a imagem de preview são geradas por script (requer `pillow`):

```bash
python3 Source/Tools/generate_textures.py
```

## Estrutura do repositório

```
About/                      About.xml e Preview.png
Defs/                       KeyBindingDefs dos atalhos 5 / 6 / 7
Languages/                  Textos em inglês e português do Brasil
Textures/SpeedRim/          As texturas dos três botões
1.6/Assemblies/SpeedRim.dll Assembly compilada, carregada pelo jogo
Source/SpeedRim/            Código-fonte C# e o projeto .csproj
Source/Tools/               Script que gera as texturas
```

## Como funciona por dentro

A `TimeSpeed` original do RimWorld vai de `Paused` (0) a `Ultrafast` (4). O mod usa os valores 5, 6
e 7 dessa mesma enumeração para as velocidades novas e responde por elas em três patches Harmony:

| Patch | Alvo | Papel |
| --- | --- | --- |
| `TickManager_TickRateMultiplier_Patch` | `TickManager.TickRateMultiplier` | Devolve o multiplicador configurado para as velocidades novas; as originais seguem intocadas. Respeita a pausa, a velocidade normal forçada e o ritmo acelerado do mapa-múndi. |
| `TimeControls_DoTimeControlsGUI_Patch` | `TimeControls.DoTimeControlsGUI` | Desenha os três botões à esquerda da fileira original e trata os atalhos (o prefixo roda antes de o jogo consumir as teclas). |
| `TickManager_ExposeData_Patch` | `TickManager.ExposeData` | Grava uma velocidade original no arquivo salvo e devolve a velocidade extra logo em seguida. |

Guardar a velocidade ativa dentro do próprio `curTimeSpeed` do jogo faz com que pausar/despausar,
salvar/carregar e outros mods que mudam a velocidade continuem funcionando sem estado paralelo. O
patch em `ExposeData` é o que mantém os saves compatíveis com o jogo sem o mod: um save feito em 20x
carrega em velocidade Super-rápida caso o SpeedRim seja removido, em vez de ficar com um valor
desconhecido.

## Compatibilidade

- Pode ser adicionado e removido de partidas em andamento.
- Não substitui nenhum método do jogo por completo: os patches são prefixos/postfixos que deixam o
  caminho original intacto para as velocidades originais.
- Funciona junto com outros mods que mexem na velocidade, desde que eles não substituam
  `TickRateMultiplier` inteiro.
