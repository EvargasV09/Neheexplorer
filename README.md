# Nehexplorer

Videojuego educativo desarrollado en **Unity y C#** para explorar las regiones naturales de Colombia y aprender sobre su geografía, ecosistemas y biodiversidad mediante minijuegos interactivos.

## Objetivo

Promover el aprendizaje a través del juego, combinando retos de memoria, preguntas y exploración con información educativa sobre Colombia.

## Funcionalidades

- Navegación entre un mapa principal y las escenas de los minijuegos.
- Selección de personaje y almacenamiento local de la selección.
- Seguimiento de las regiones completadas y guardado del progreso con `PlayerPrefs`.
- Paneles educativos al finalizar los retos.
- Contenido y reglas de minijuegos configurables mediante `ScriptableObjects`.

## Minijuegos incluidos

El proyecto contiene escenas y scripts para cuatro regiones:

| Región | Minijuego | Enfoque |
| --- | --- | --- |
| Caribe | Juego de memoria | Asociación de parejas mediante tarjetas. |
| Andina | Trivia de las Cordilleras | Preguntas sobre geografía, páramos y biodiversidad de los Andes colombianos. |
| Orinoquía | Runner | Retos de avance y evasión de obstáculos. |
| Amazónica | Expedición en canoa | Fotografía de especies y evasión de obstáculos en un recorrido por el río. |

El modelo de datos también contempla las regiones Pacífica, Insular y una categoría Espiritual; no se presentan aquí como minijuegos implementados.

## Tecnologías

- **Unity 6000.6.0f1 (Unity 6)**: motor de desarrollo.
- **C#**: lógica de juego y gestión del estado.
- **Universal Render Pipeline (URP)**: renderizado.
- **Unity UI (uGUI) y TextMeshPro**: interfaces y textos.
- **New Input System**: sistema de entrada configurado en el proyecto.
- **ScriptableObjects**: configuración de preguntas, especies y reglas.
- **PlayerPrefs**: persistencia local del personaje seleccionado y las regiones completadas.

Plataforma objetivo configurada: **Windows de 64 bits**.

## Organización del proyecto

```text
Assets/
├── Scenes/
│   ├── MapaHub.unity
│   ├── Minigame_Caribe.unity
│   ├── Minigame_Andina.unity
│   ├── Minigame_Orinoquia.unity
│   └── Minigame_Amazonica.unity
├── Scripts/
│   ├── Core/          # Gestión global y persistencia del progreso
│   ├── Nehexplorer/   # Datos y navegación de regiones
│   └── Minigames/     # Lógica y configuración de los minijuegos
└── Data/
    └── Minigames/     # Assets de configuración educativa
```

## Cómo ejecutar

1. Descarga o clona el proyecto completo.
2. En **Unity Hub**, selecciona **Add / Agregar** y elige la carpeta raíz que contiene `Assets`, `Packages` y `ProjectSettings`.
3. Abre el proyecto con **Unity 6000.6.0f1** para mantener la compatibilidad con su versión actual.
4. Espera a que Unity restaure los paquetes e importe los assets.
5. Abre `Assets/Scenes/MapaHub.unity`.
6. Presiona **Play** para iniciar desde el mapa principal.

### Controles de la expedición amazónica

- **W / S** o **flechas arriba / abajo**: dirigir la canoa.
- **Espacio** o botón **FOTOGRAFIAR**: fotografiar especies.

Los demás minijuegos presentan sus instrucciones e interacciones en sus respectivas interfaces.

## Configuración del contenido educativo

Los datos de los minijuegos se separan de su lógica mediante assets de configuración. Por ejemplo:

- `AndeanTriviaDataSet`: preguntas, opciones, explicaciones, tiempo por pregunta y puntaje requerido.
- `AmazonExpeditionDataSet`: catálogo de especies, obstáculos, distancia del recorrido y condiciones de victoria.

Estas configuraciones se pueden editar desde el Inspector de Unity sin modificar la lógica de los minijuegos.

## Progreso local

El personaje seleccionado y las regiones completadas se guardan con `PlayerPrefs` en el equipo del jugador. Esta persistencia es local: no equivale a un sistema de cuentas ni a guardado en la nube.

## Descripción breve para portafolio

> Videojuego educativo desarrollado en Unity y C# para explorar las regiones y la biodiversidad de Colombia mediante minijuegos interactivos, con interfaces de usuario, contenido configurable y persistencia local del progreso.
