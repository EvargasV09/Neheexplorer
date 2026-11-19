# Project Overview

## Cambio de alcance aprobado por el usuario
Se implementa ahora un prototipo completo con cuadrados de colores y letras/nombres, sin canasta ni generación de ilustraciones. Esto sustituye las decisiones de arte y cancela los pasos 2 y 3 de generación. Los prefabs conservarán campos Sprite opcionales para añadir arte después. El resto de la mecánica, integración y verificación se mantiene. El catálogo y las fichas son editables; si no se consigue validar fuentes locales, se indicará que la selección regional queda pendiente de revisión, sin inventar exclusividad ni origen nativo.
- **Game Title:** Nehexplorer — Frutas de las Islas (nombre propuesto del minijuego).
- **High-Level Concept:** Un pequeño explorador con canasta sigue el puntero del mouse por un jardín isleño. Al acercarse a las frutas las recoge automáticamente y descubre fichas sobre su diversidad, usos y relación con la cultura y el ambiente insular.
- **Players:** Un jugador, experiencia local sin servicios en línea.
- **Inspiration / Reference Games:** Colecciones exploratorias y álbumes de descubrimientos; no se copiará un juego concreto.
- **Tone / Art Direction:** Educativo, amable y tropical; personaje caricaturesco de cuerpo completo, frutas reconocibles, vegetación y mar. Ilustración no pixel art, sin emojis como sustituto de las frutas finales.
- **Target Platform:** Windows PC, conforme al objetivo actual StandaloneWindows64.
- **Screen Orientation / Resolution:** Horizontal, Canvas de referencia 1920 × 1080, escalable a diferentes ventanas.
- **Render Pipeline:** Universal Render Pipeline, Unity 6000.6.0f1.

## Decisiones confirmadas
- Colección sin tiempo, sin vidas ni condición de derrota.
- Ficha con pausa al descubrir cada variedad.
- Ilustraciones tropicales para personaje y frutas.
- La recolección se realiza moviendo el mouse; no exige clics sobre las frutas.
- Propuesta de alcance inicial: seis variedades, una recogida por variedad para completar el álbum. El número final dependerá de verificar la pertinencia regional del catálogo.

# Game Mechanics
## Core Gameplay Loop
1. Entrar desde Insular en MapaHub y leer una instrucción breve.
2. Mover el mouse dentro del jardín; el personaje sigue el destino con velocidad limitada y desplazamiento suave.
3. Acercarse a una fruta para recogerla automáticamente; actualizar el álbum y el contador de variedades.
4. Pausar solo la mecánica del minijuego, mostrar una ficha de la fruta y esperar al botón Continuar.
5. Repetir hasta descubrir todas las variedades y leer sus fichas.
6. Mostrar el panel educativo final y permitir reclamar el fragmento de sabiduría y volver al mapa.

No hay cronómetro ni recorrido de 1200 metros: este nuevo minijuego se completa por diversidad descubierta, no por distancia. No se modifican los recorridos de Amazonas y Orinoquía.

El catálogo aparecerá completo y al alcance del jugador, con ubicaciones distribuidas sin superposiciones. No se dependerá de azar para que aparezca una variedad necesaria. La partida acaba después de cerrar la última ficha, no antes de poder leerla.

### Enfoques técnicos considerados
- **Canvas uGUI con detección geométrica:** recomendado y elegido para el plan. Sigue la arquitectura de los minijuegos existentes y mantiene coordenadas, escala y fichas en un mismo sistema. No requiere Rigidbody2D ni gravedad; resulta menos adecuado para futuros escenarios con física compleja.
- **Escena de mundo 2D con SpriteRenderer y triggers:** válida para exploración con obstáculos físicos y cámaras. Requiere más configuración de cámara, conversiones y colisiones, sin ventaja clara para esta colección sencilla.

La colección sin presión favorece autonomía y exploración (SDT). La mecánica de descubrir variedades genera una dinámica de curiosidad y una experiencia de aprendizaje (MDA); la pausa evita competir por atención entre movimiento y lectura.

## Controls and Input Methods
- Backend verificado: New Input System (`activeInputHandler: 1`).
- Crear una InputAction de tipo Value/Vector2 con binding `<Mouse>/position`, gestionar `performed`, activar y desactivar la acción según el ciclo de vida del minijuego y eliminar suscripciones al destruirlo.
- Convertir coordenadas de pantalla al RectTransform del jardín con la cámara adecuada al Canvas; limitar el destino a la zona jugable dejando margen por el tamaño del personaje.
- El sprite se desplaza hacia el destino; puede orientarse horizontalmente y tener un balanceo discreto de desplazamiento y un pequeño efecto de recogida. No se requieren spritesheets ni un controlador de plataformas.
- Evaluar la cercanía a la fruta en el mismo espacio local; usar distancia al segmento del movimiento para no omitir recogidas entre frames. Dar de baja una fruta una sola vez.
- Durante fichas o resultados, detener movimiento y recogidas. Mantener operativo el EventSystem para Continuar y Volver al mapa. No usar `Time.timeScale = 0` para evitar efectos globales.
- Al continuar, restablecer el destino en la posición del personaje y esperar un nuevo movimiento del mouse dentro del jardín; el clic en la ficha no provoca una recogida involuntaria.
- Mantener el cursor visible y sin bloquear. Fuera del jardín, el personaje permanece dentro de sus límites; al perder el foco no se fuerzan movimientos ni penalizaciones.

# UI
Mantener el sistema uGUI ya observado: Canvas Screen Space Overlay, CanvasScaler y TextMeshPro para etiquetas. Adaptar colores turquesa, arena, verde y acentos de fruta al tema insular del hub.

```text
+------------------------------------------------------------------+
| [← MAPA PRINCIPAL]       FRUTAS DE LAS ISLAS       Variedades 0 / 6 |
| Mueve el mouse: tu explorador recogerá las frutas al acercarse.    |
+---------------------------------------------+--------------------+
| Jardín tropical, mar al fondo                | Álbum de frutas    |
|    [fruta]                 [fruta]           | [descubierta]      |
|                 [explorador + canasta]      | [por descubrir]    |
| [fruta]             [fruta]                  | ...                |
+---------------------------------------------+--------------------+
```

Ficha modal centrada: ilustración, nombre común, nombre científico cuando se verifique, un dato breve sobre diversidad/ecología y otro sobre contexto cultural o uso local, más botón Continuar. El fondo modal bloquea interacciones del jardín, pero no debe impedir volver al mapa; el botón superior permanece accesible por jerarquía/orden de Canvas.

Panel final: variedades descubiertas, síntesis educativa y botón Reclamar sabiduría y volver. Conservar el flujo de guardado existente de MinigameBase.

No reconstruir el hub ni cambiar fuentes de otros minijuegos. Ilustraciones no interactivas con `raycastTarget = false`; botones con gráficos habilitados para raycast. Exactamente un EventSystem con InputSystemUIInputModule en la nueva escena.

# Key Asset & Context
## Estado observado
- No existe escena ni controlador de minijuego Insular entre las escenas habilitadas.
- `Assets/Scripts/Nehexplorer/RegionWheelController.cs`: `OnClickExplore()` carga Caribe, Andina, Orinoquía y Amazónica; falta la rama Insular.
- `Assets/Scenes/MapaHub.unity`: contiene región Insular y datos serializados que anuncian buceo y rompecabezas; deben actualizarse para reflejar este nuevo reto.
- `Assets/Scripts/Minigames/MinigameBase.cs`: `StartGame()`, `EndGame(bool won)`, callback final `OnReturnToHubClicked()` y navegación superior `ReturnToHubImmediately()` ya existen. La referencia heredada `backToHubTopButton` debe asignarse expresamente.
- `GameManager` reconoce `Insular` en el progreso. Aprovechar `CompleteRegion` y el guardado con PlayerPrefs; no crear otra persistencia.
- Canvas del hub: Overlay, referencia 1920 × 1080. Hay ilustraciones de rueda y de Amazonía, pero no se encontró un conjunto de frutas/personaje para Insular.
- Inspección de dependencias del hub realizada mediante AssetDatabase.GetDependencies. Antes de implementar, revisar también dependientes de los archivos compartidos y volver a verificar el estado del proyecto.

## Archivos nuevos
- `Assets/Scripts/Minigames/InsularFruitGame.cs`: controlador derivado de MinigameBase; seguimiento del puntero, estado jugable/ficha/resultados, colección única y finalización.
- `Assets/Scripts/Minigames/InsularFruitDataSet.cs`: ScriptableObject con catálogo, sprites, textos y fuentes, parámetros de movimiento y recogida.
- `Assets/Data/Minigames/Insular_FruitData.asset`: datos educativos y referencias de arte.
- `Assets/Scenes/Minigame_Insular.unity`: nueva escena funcional.
- `Assets/Prefabs/Minigames/Insular/FruitItem.prefab`: Image y etiqueta de fruta, reutilizable por el catálogo.
- `Assets/Prefabs/Minigames/Insular/Collector.prefab`: explorador con canasta en UI; sin física ni GroundCheck.
- `Assets/Sprites/Minigames/Insular/`: concepto del explorador, ilustraciones individuales de frutas y fondo tropical.

Datos por variedad: id único, nombre común, nombre científico opcional, Sprite, descripción breve, dato de diversidad, uso/contexto local y enlaces de las fuentes consultadas.

## Contenido educativo y precisión
- Centrarse en San Andrés, Providencia y Santa Catalina para el contexto frutal y cultural; no extender automáticamente sus cultivos a Malpelo o Gorgona.
- Candidatos que deben contrastarse antes de cerrar el catálogo: coco, fruta de pan, mango, papaya, tamarindo y guanábana. Sustituir cualquier candidato cuya asociación regional no pueda respaldarse.
- Consultar fuentes de entidades locales, ambientales, agrícolas o académicas para cada ficha. No afirmar que una fruta es endémica, nativa o exclusiva de las islas sin evidencia.
- Distinguir diversidad de especies y variedades cultivadas; presentar usos de forma respetuosa con la cultura raizal, sin inventar denominaciones locales ni afirmaciones médicas.
- Redactar textos breves en español y conservar bibliografía editable en los datos.

## Archivos existentes a modificar
- `RegionWheelController.cs`: rama de carga `Minigame_Insular` y descripción de minijuego por defecto para Insular.
- `MapaHub.unity`: actualizar únicamente datos serializados del reto Insular; preservar rueda, selecciones y progreso.
- Configuración de escenas de build/perfil activo: añadir `Minigame_Insular` sin eliminar ni deshabilitar escenas existentes.
- No modificar MinigameBase o GameManager salvo que la inspección revele una incompatibilidad directamente necesaria; proponer revisión del plan en ese caso.

# Implementation Steps
1. **Validación de contexto y contenido.** Revisar dependencias y escenas actuales, catálogo y fuentes; definir ids y texto de fichas. Confirmar el sistema de build activo y configuración de pruebas. Dependencias: aprobación del plan.
2. **Concepto visual.** Preparar un explorador tropical de cuerpo completo con canasta y presentar la imagen para aprobación antes de producir arte definitivo relacionado. No atribuir representación cultural específica sin validarla. Usar transparencia y una referencia coherente para las frutas; comprobar formas botánicas. Si no hay herramientas de generación disponibles, solicitar imágenes al usuario o acordar otra vía antes de continuar; no reemplazar silenciosamente las ilustraciones aprobadas por emojis. Dependencias: 1 y aprobación del concepto.
3. **Producción y preparación de arte.** Crear/importar frutas y fondo coherentes con el concepto aprobado. Comprobar transparencia, recorte, tamaños visibles y legibilidad a escala de juego. Separar producción/importación de imágenes y compilación de scripts para no interrumpir procesos de generación. Dependencias: 2.
4. **Datos y lógica.** Crear InsularFruitDataSet e InsularFruitGame. Implementar InputAction de posición, conversión y límites, desplazamiento, recogida por segmento, ids únicos, pausa local y final tras cerrar la última ficha. Manejar referencias y catálogo inválidos con mensajes claros, sin marcar victoria falsa. Dependencias: 1; sprites finales de 3 para poblar el asset.
5. **Prefabs y escena.** Crear Collector, FruitItem y Minigame_Insular con Canvas, EventSystem, jardín, álbum, ficha modal y panel final. Asignar todas las referencias heredadas y nuevas; usar `regionId = "Insular"`. No reutilizar la escena de otro minijuego con su controlador activo. Dependencias: 3–4.
6. **Integración desde el hub.** Modificar rama Insular de RegionWheelController, datos por defecto y datos serializados del hub. Añadir escena al build/perfil activo. Verificar entrada y regreso desde estados de juego, ficha y victoria. Dependencias: 5.
7. **Verificación funcional y visual.** Ejecutar la matriz de pruebas siguiente; inspeccionar capturas en ejecución y ajustar solamente tamaño, orden y espaciado de los elementos nuevos. Si la configuración no permite automatizar Play Mode, no cambiar ajustes globales sin autorización y documentar las pruebas manuales pendientes. Dependencias: 4–6.

# Verification & Testing
- Compilación limpia y nueva escena incluida una sola vez en el build activo.
- Desde MapaHub seleccionar Insular, pulsar Explorar y comprobar que abre el nuevo minijuego, no uno existente.
- Movimiento con mouse lento/rápido, bordes y esquinas; el sprite permanece dentro del jardín y no salta al pulsar botones fuera de él.
- Sin clics sobre frutas, cada ejemplar se recoge una sola vez; contador y álbum sincronizados.
- Todas las variedades aparecen y son accesibles; no se bloquea la victoria por aleatoriedad o superposición.
- Al abrir ficha se detienen movimiento y recogidas. El texto permanece hasta Continuar. Cerrar la última ficha muestra la victoria con todas las variedades.
- Volver al mapa funciona durante juego y fichas, sin registrar una región no completada. Reclamar después de victoria registra `Insular` una vez y actualiza fragmentos del hub.
- Reentrar tras abandonar o completar reinicia la partida visual y lógica, sin eventos duplicados ni frutas anteriores; no borrar progreso previamente guardado.
- Mouse ausente, pérdida de foco, catálogo vacío, ids repetidos, Sprite ausente y referencias sin asignar: comportamiento controlado y sin excepciones silenciosas.
- Verificar escalado a 1920 × 1080 y 1280 × 720; fichas legibles, botones accesibles, sin fruta bajo paneles o cabeceras.
- Capturas de ejecución del jardín, ficha y resultado para comprobar arte visible, transparencia y orden visual, no solo referencias asignadas.
- Regresión: entradas y botón de regreso de Caribe, Andina, Orinoquía y Amazónica siguen operativos; Amazonas y Orinoquía mantienen sus 1200 metros.
- Revisar texto y fuentes de cada fruta antes de declarar listo el contenido educativo. No afirmar validación runtime si solo se verificaron assets en Edit Mode.
