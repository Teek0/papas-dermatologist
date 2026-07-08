# Historial de cambios

Este archivo resume cambios importantes y, sobre todo, por que se hicieron. La idea es que podamos volver despues y entender el camino del proyecto sin depender solo de los mensajes de commit.

## 2026-07-08

### Se separo la reproduccion de musica por escena

**Problema:** al pasar de recepcion a camilla, la musica de recepcion seguia sonando y se mezclaba con la musica de camilla.

**Cambio:** se agrego `SceneMusicPlayer` para que cada escena controle su propia musica y solo reproduzca cuando su escena esta activa.

**Justificacion:** la musica no deberia depender del menu lateral ni de objetos compartidos entre escenas. Cada escena debe decidir su ambiente musical.

### Se dejo de cargar Camilla y SideMenu como escenas aditivas

**Problema:** la carga aditiva estaba provocando responsabilidades mezcladas: mas de un `AudioListener`, mas de un `EventSystem`, musica duplicada y objetos de UI viviendo fuera de la escena que realmente los usaba.

**Cambio:** el flujo recepcion -> camilla paso a usar `SceneManager.LoadScene(SceneNames.Camilla)`. El menu lateral dejo de depender de `SideMenu.unity` como escena aditiva.

**Justificacion:** el juego es simple y no necesita mantener varias escenas activas para este flujo. Cargar una escena principal a la vez reduce warnings y hace mas facil razonar sobre que objetos existen.

### Se agregaron menus laterales como prefabs de escena

**Problema:** el menu lateral intentaba funcionar como escena compartida, pero eso duplicaba sistemas de Unity y hacia dificil saber quien controlaba la UI.

**Cambio:** `CanvasMenu` quedo como prefab base del menu lateral. `CanvasMenu Camilla` quedo como variante para camilla, con la opcion extra de volver a recepcion.

**Justificacion:** un prefab base permite compartir la UI comun. Una variante permite diferencias por escena sin duplicar todo ni romper la configuracion general.

### Se bloqueo pintar mientras el menu esta abierto

**Problema:** al abrir el menu en camilla, el tiempo se pausaba pero aun era posible seguir pintando.

**Cambio:** las entradas de pintura ahora revisan el estado de pausa antes de permitir pintar.

**Justificacion:** pausar el juego debe pausar tambien las acciones del jugador, no solo el temporizador.

### Se agrego volver a recepcion desde el minimenu

**Problema:** camilla necesitaba una salida rapida hacia recepcion desde el menu lateral.

**Cambio:** `IngameMenuController` recibio la accion publica `actionReception()`, pensada para conectarse desde un boton del prefab variante de camilla.

**Justificacion:** volver a recepcion es una accion distinta de volver al menu principal, asi que debe existir como funcion propia.

### Se empezo a separar la transicion de escenas del menu

**Problema:** `IngameMenuController` estaba mezclando demasiadas responsabilidades: abrir/cerrar menu, pausar, manejar settings, hacer fade, tocar el mixer y cargar escenas.

**Cambio:** se creo `SceneTransitionService` para centralizar fade visual, fade de audio, reset del mixer y carga de escenas.

**Justificacion:** el menu debe pedir una transicion, no conocer todos los detalles de como se hace. Esto deja el codigo mas facil de mantener y prepara el terreno para reutilizar transiciones en otros flujos.

### Se reutilizo SceneTransitionService desde el menu inicial

**Problema:** `UIController` tenia una copia de la logica de fade y ademas delegaba el cambio de escena a `mainMenu_Music`.

**Cambio:** `UIController` ahora reproduce el sonido de inicio y usa `SceneTransitionService` para hacer fade visual, fade de audio, reset del mixer y carga de recepcion.

**Justificacion:** la transicion de escenas debe vivir en un solo lugar. Esto evita duplicacion y separa mejor la musica del menu inicial de la navegacion entre escenas.

### Se saco SideMenu de Build Settings

**Problema:** `SideMenu.unity` ya no forma parte del flujo principal, pero seguia incluido en Build Settings.

**Cambio:** Build Settings quedo solo con `mainMenu_UI`, `ReceptionScene` y `CamillaScene`.

**Justificacion:** si el menu lateral ahora vive como prefab por escena, la escena `SideMenu` no deberia entrar al build. La escena se conserva por ahora como referencia, sin borrarla.

### Se corrigio el golpe de volumen al iniciar juego

**Problema:** al pasar del menu inicial a recepcion, el mixer se restauraba antes de cargar la nueva escena. Eso hacia que la musica subiera de golpe justo antes del corte.

**Cambio:** `SceneTransitionService` ahora restaura el mixer cuando la nueva escena ya termino de cargar. Ademas, `UIController` vuelve a hacer fade visual hacia negro al iniciar el juego.

**Justificacion:** la transicion debe bajar el audio sin volver a subirlo mientras la escena anterior sigue sonando.
