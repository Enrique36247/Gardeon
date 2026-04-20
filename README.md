# Informe de Proyecto — Desarrollo de Videojuego 2D
 
---
 
**Nombre y apellidos:** Enrique Pérez García 
**Centro:** IES El Rincón  
**Ciclo Formativo:** Grado Superior — Desarrollo de Aplicaciones Multiplataforma  
**Curso:** 2º Curso  
**Título del proyecto:** Gardeon
 
---
 
## Índice
 
1. [Descripción del juego](#1-descripción-del-juego)
2. [Temática y objetivo del juego](#2-temática-y-objetivo-del-juego)
3. [Mecánicas y escenas implementadas](#3-mecánicas-y-escenas-implementadas)
4. [Mejoras y creatividad introducidas](#4-mejoras-y-creatividad-introducidas)
5. [Capturas del juego](#5-capturas-del-juego)
6. [Conclusiones](#6-conclusiones)
7. [Referencias](#7-referencias)
---
 
## 1. Descripción del juego
 
El proyecto consiste en el desarrollo de un videojuego 2D de género **roguelike dungeon crawler** desarrollado con el motor **Unity 6**, utilizando el lenguaje de programación **C#** y el nuevo sistema de entrada **Unity Input System**.
 
El juego sitúa al jugador en un entorno de mazmorras minimalistas con perspectiva cenital (*top-down*), donde debe avanzar a través de una serie de salas enfrentándose a distintos tipos de enemigos hasta llegar a una sala final con un jefe. La estética del juego es deliberadamente minimalista, empleando formas geométricas simples y una paleta de colores reducida para centrar el foco en la jugabilidad.
 
El principal referente del juego es **Enter the Gungeon** (Dodge Roll, 2016), del que toma prestadas sus mecánicas principales: el movimiento en ocho direcciones, el sistema de esquiva con invulnerabilidad temporal y el combate basado en proyectiles.
 
---
 
## 2. Temática y objetivo del juego
 
El juego no presenta una narrativa explícita. Su propuesta es puramente jugable: el jugador debe superar tres salas de combate con enemigos progresivamente más difíciles y derrotar a un jefe final para completar la run.
 
**Objetivo principal:** eliminar a todos los enemigos de cada sala, avanzar a la siguiente y derrotar al jefe final.
 
**Progresión:**
 
| Sala | Contenido |
|------|-----------|
| Sala 1 | Enemigos básicos (Grunt) |
| Sala 2 | Grunts y Sniper |
| Sala 3 | Grunts, Sniper y Spinner |
| Sala 4 — Boss | Jefe con dos fases y cuatro patrones de ataque |
 
La dificultad escala progresivamente: cada sala introduce un nuevo tipo de enemigo con comportamientos más complejos, culminando en un jefe que combina todos los patrones de ataque vistos anteriormente con mayor densidad de proyectiles.
 
---
 
## 3. Mecánicas y escenas implementadas
 
### 3.1 Escenas
 
El juego cuenta con tres escenas diferenciadas:
 
- **Menú principal:** pantalla de inicio con botón para comenzar la partida y botón para salir del juego.
- **Escena de juego:** escena principal donde transcurre toda la acción.
- **Pantalla de Game Over:** muestra las estadísticas de la run (salas completadas y enemigos eliminados) con opciones para reintentar o volver al menú.
En caso de victoria contra el jefe, el juego regresa directamente al menú principal tras un fundido a negro.
 
### 3.2 Mecánicas del jugador
 
**Movimiento:**
- Desplazamiento en ocho direcciones con teclado (WASD / flechas).
- Rotación continua hacia el cursor del ratón.
- Implementado con `Rigidbody2D` y el nuevo `Input System` de Unity.
**Sistema de esquiva (Dash/Roll):**
- Activado con `Shift` o `Espacio`.
- El jugador se desplaza rápidamente en la dirección del movimiento.
- Durante el dash el jugador es **invulnerable**: las balas enemigas lo atraviesan gracias a `Physics2D.IgnoreLayerCollision`.
- Cooldown de 0,7 segundos para evitar el abuso.
- Feedback visual: el sprite se vuelve semitransparente durante el roll.
**Sistema de disparo:**
- Disparo continuo con clic izquierdo del ratón.
- Cambio de arma con la rueda del ratón.
- Implementado con **Object Pooling** para optimizar el rendimiento.
**Sistema de vida e invulnerabilidad:**
- El jugador tiene 6 puntos de vida.
- Al recibir daño se activan **iframes** (invulnerabilidad temporal) con feedback visual de parpadeo rojo.
- Al recibir daño se eliminan automáticamente todas las balas enemigas presentes en la escena.
- Sistema de armadura implementado como capa adicional de protección (disponible como mejora futura).
### 3.3 Sistema de armas
 
Se han implementado tres armas con características distintas que modifican sustancialmente el estilo de juego:
 
| Arma | Cadencia | Daño | Especial |
|------|----------|------|---------|
| Pistola | Alta (0,15s) | 1 | Bala única y precisa |
| Escopeta | Baja (0,6s) | 1 × 6 balas | Dispersión aleatoria en abanico |
| Rifle | Muy baja (0,8s) | 5 | Bala penetrante que atraviesa enemigos |
 
Las armas están implementadas como **ScriptableObjects** (`WeaponData`), lo que permite configurar y añadir nuevas armas sin modificar el código.
 
### 3.4 Tipos de enemigos
 
Se han desarrollado tres tipos de enemigos con comportamientos diferenciados, todos heredando de una clase base común (`EnemyBase`):
 
**Grunt:**
- Se acerca al jugador y mantiene una distancia óptima para disparar.
- Retrocede si el jugador se acerca demasiado.
- Disparo directo hacia el jugador.
**Sniper:**
- Mantiene una distancia elevada del jugador (10 unidades).
- Se mueve lateralmente en su distancia ideal para ser difícil de acertar.
- Telegrafía el disparo con parpadeo naranja antes de ejecutarlo.
- Balas rápidas con daño de 2.
**Spinner:**
- Se mueve de forma lenta y errática.
- Dispara ráfagas circulares con ángulos aleatorios que llenan la sala de proyectiles.
- Alterna ráfagas con pausas para crear ritmo en el combate.
### 3.5 Jefe final (Boss)
 
El jefe es el enemigo más complejo del juego. Sus características son:
 
- **60 puntos de vida** divididos en dos fases.
- **Movimiento errático** similar al Spinner pero más lento.
- **Cuatro patrones de ataque** que se alternan aleatoriamente cada 1,5 segundos sin repetir el mismo dos veces seguidas:
  - *Espiral:* ráfaga circular que rota progresivamente.
  - *Burst:* oleadas de balas con dispersión aleatoria hacia el jugador.
  - *Aimed:* balas rápidas directas al jugador.
  - *Cruz:* patrones en cruz y diagonal que rotan entre oleadas.
- **Fase 2** al llegar a 30 de vida: aumenta velocidad, reduce el intervalo de ataque, cambia de color y lanza más balas por patrón.
- **Secuencia de muerte** con efecto de parpadeo antes de cargar la pantalla de victoria.
### 3.6 Sistema de habitaciones
 
Las habitaciones se generan mediante código (`RoomModule`) sin necesidad de Tilemap:
 
- Paredes, suelo y puertas construidos con `SpriteRenderer` en modo *Sliced* y `BoxCollider2D` con tamaño definido directamente, evitando problemas de escala.
- Las puertas están **bloqueadas físicamente** mientras quedan enemigos en la sala.
- Al limpiar la sala, las puertas se vuelven visibles y funcionales.
- Los enemigos **spawnean con un retraso de 2 segundos** con indicadores visuales parpadeantes en sus posiciones de aparición.
- Las posiciones de spawn se generan aleatoriamente respetando una distancia mínima al jugador y entre enemigos.
- Las transiciones entre salas se realizan con un fundido a negro.
---
 
## 4. Mejoras y creatividad introducidas
 
### 4.1 Sistema de esquiva con física real
 
A diferencia de implementaciones simples, el dash utiliza `Physics2D.IgnoreLayerCollision` para deshabilitar físicamente las colisiones entre el jugador y las balas enemigas durante el roll, en lugar de simplemente marcar una variable booleana. Esto garantiza que ninguna bala impacte al jugador independientemente de la velocidad del proyectil.
 
### 4.2 Feedback de daño con limpieza de proyectiles
 
Al recibir daño, el juego elimina automáticamente todas las balas enemigas presentes en la escena. Esta decisión de diseño reduce la frustración del jugador al evitar que un único impacto encadene múltiples daños por proyectiles que ya estaban en vuelo.
 
### 4.3 Telegrafía de enemigos
 
El enemigo Sniper avisa visualmente con un parpadeo naranja antes de disparar, dando al jugador tiempo de reacción. Este principio de *telegrafía* hace que el juego sea difícil pero justo.
 
### 4.4 Indicadores de spawn
 
Antes de que los enemigos aparezcan en la sala, se muestran indicadores visuales parpadeantes que se aceleran conforme se acerca el momento del spawn. Esto avisa al jugador de dónde aparecerán los enemigos y le da tiempo para posicionarse.
 
### 4.5 Generación procedural de posiciones de spawn
 
Las posiciones de los enemigos se calculan aleatoriamente en cada sala respetando un margen mínimo con las paredes, una distancia mínima al jugador y una distancia mínima entre enemigos, garantizando que ningún enemigo aparezca en una posición injusta.
 
### 4.6 ScriptableObjects para las armas
 
El uso de `ScriptableObjects` para definir las armas permite añadir nuevas armas al juego simplemente creando un nuevo asset en el editor, sin tocar ningún script. Esto sigue el principio de diseño **Open/Closed** (abierto para extensión, cerrado para modificación).
 
### 4.7 Object Pooling
 
Las balas del jugador utilizan un sistema de *object pooling* que reutiliza instancias existentes en lugar de instanciar y destruir objetos continuamente, mejorando el rendimiento especialmente en situaciones de disparo intensivo como la escopeta o el boss.
 
### 4.8 Escalado de dificultad del boss en dos fases
 
El jefe cambia de comportamiento al llegar a la mitad de su vida, aumentando su velocidad y la densidad de proyectiles. Esta transición se acompaña de un cambio de color y un efecto de flash que comunica claramente al jugador que el combate ha cambiado.
 
---
 
## 5. Capturas del juego
 
> ![MainMenu](image.png)
 
> ![Enemies](image-1.png)
 
> ![MoreEnemies](image-2.png)
 
> ![boss](image-4.png)
 
> ![gameover](image-3.png)
 
---
 
## 6. Conclusiones
 
El desarrollo de este proyecto ha permitido aplicar de forma práctica los conocimientos adquiridos durante el ciclo formativo, especialmente en lo relativo a la programación orientada a objetos, la arquitectura de software y el uso de herramientas profesionales de desarrollo.
 
Desde el punto de vista técnico, el proyecto ha supuesto un reto significativo en varios aspectos:
 
- La gestión del nuevo **Input System** de Unity, que difiere notablemente del sistema clásico y requiere una comprensión más profunda de la arquitectura de eventos.
- El diseño de un sistema de habitaciones generado por código, que evita la dependencia de herramientas visuales como Tilemap y ofrece mayor flexibilidad.
- La implementación de **patrones de diseño** como ScriptableObjects, Object Pooling y herencia de clases para estructurar el código de forma mantenible y extensible.
- El equilibrado de la dificultad, que requiere una iteración constante de valores y pruebas de jugabilidad.
Como líneas de mejora futuras se identifican:
 
- Implementación de un sistema de items y power-ups entre salas.
- Generación procedural de la disposición de salas.
- Añadir efectos de partículas y sonido para mejorar el *game feel*.
- Ampliar el número de tipos de enemigos y patrones de ataque del jefe.
- Implementar un sistema de puntuación y tabla de récords.
En conclusión, el proyecto ha resultado una experiencia completa de desarrollo de software aplicada a la creación de videojuegos, abarcando desde la arquitectura del sistema hasta las decisiones de diseño de jugabilidad.
 
---
 
## 7. Referencias
 
- Unity Technologies. (2024). *Unity 6 Documentation*. https://docs.unity3d.com
- Unity Technologies. (2024). *Input System Package Documentation*. https://docs.unity3d.com/Packages/com.unity.inputsystem@latest
- Dodge Roll. (2016). *Enter the Gungeon* [Videojuego]. Devolver Digital.
- Microsoft. (2024). *C# Documentation*. https://learn.microsoft.com/es-es/dotnet/csharp/
- Nystrom, R. (2014). *Game Programming Patterns*. https://gameprogrammingpatterns.com
---
 
*Documento generado como parte del proyecto final del módulo correspondiente al Ciclo Formativo de Grado Superior en Desarrollo de Aplicaciones Multiplataforma — IES El Rincón — Curso 2º.*