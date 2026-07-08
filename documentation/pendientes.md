# Pendientes

Lista viva de tareas tecnicas y decisiones de diseno pendientes. La idea es actualizarla cuando cerremos algo o aparezca una deuda nueva.

## Arquitectura

- Separar mejor `IngameMenuController`: dejarlo solo con abrir/cerrar menu, pausa y botones.
- Evaluar un controlador propio para settings/volumen, separado del menu lateral.
- Renombrar `mainMenu_Music` a un nombre mas claro, por ejemplo `MainMenuMusicPlayer`.

## Escenas

- Revisar si `SideMenu.unity` sigue siendo necesario. Ya no esta en Build Settings; si todo sigue estable, eliminarlo en un commit futuro.
- Mantener el flujo principal con una escena activa a la vez, salvo que aparezca una razon fuerte para volver a cargas aditivas.

## Audio

- Confirmar que cada escena tenga exactamente un responsable de musica.
- Revisar que no vuelvan warnings de `AudioListener` duplicado o faltante.
- Revisar que el mixer no quede silenciado despues de transiciones.

## Gameplay

- Definir que significa "volver a recepcion" desde camilla:
  - si cancela el tratamiento,
  - si limpia el paciente actual,
  - si conserva o descarta progreso,
  - si afecta dinero o evaluacion.
- Revisar si `GameSession.CurrentCustomer` debe limpiarse al volver a recepcion.

## Limpieza

- Revisar referencias rotas despues de borrar placeholders y reemplazar assets.
- Mantener `CanvasMenu` como prefab base y `CanvasMenu Camilla` como variante de camilla.
- Crear una checklist manual de pruebas antes de cada commit importante.

## Checklist manual sugerida

1. Menu inicial -> Recepcion.
2. Recepcion -> Camilla.
3. Abrir menu en Camilla y confirmar que no se puede pintar.
4. Camilla -> Recepcion desde minimenu.
5. Recepcion -> menu inicial desde minimenu.
6. Revisar consola sin warnings de `AudioListener`, `EventSystem` o referencias faltantes.
