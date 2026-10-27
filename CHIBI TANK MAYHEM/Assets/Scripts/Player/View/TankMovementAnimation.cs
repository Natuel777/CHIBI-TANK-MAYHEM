using UnityEngine;

//Vista de las orugas del tanque. Traduce el avance y el giro REALES del modelo (PlayerMovement) a los parámetros del
//Animator "TankMovement": un Blend Tree 2D (Freeform Cartesian) con los clips Forward / Backwards / TurningLeft / TurningRight.
//
//POR QUÉ SE LEE EL MODELO Y NO EL INPUT
//Una oruga es una cinta que rueda sin deslizar sobre el piso. Para que no "patine", la superficie de cada oruga tiene que
//moverse (respecto del casco) exactamente a la velocidad con la que ese lado del tanque avanza sobre el suelo. Esa
//velocidad sale del avance v (m/s, + adelante) y del giro w (rad/s, + a la derecha) del tanque:
//    oruga izquierda   uL = v + w*d
//    oruga derecha     uR = v - w*d        (d = mitad de la trocha: del centro del tanque al eje de cada oruga)
//Con el input crudo esos valores no se pueden reconstruir: el tanque acelera y frena con rampas (PlayerMovement),
//un choque puede cancelar el avance, etc. PlayerMovement ya tiene el valor real, así que la vista solo lo lee.
//
//CÓMO SE ARMA LA MEZCLA
//Los 4 clips del FBX son las 4 combinaciones de "cada oruga hacia adelante o hacia atrás" a la MISMA velocidad:
//    clip            (izquierda, derecha)
//    Forward           (+, +)
//    Backwards         (-, -)
//    TurningLeft       (-, +)
//    TurningRight      (+, -)
//Si Forward/Backwards entran con peso f y TurningRight/TurningLeft con peso t (con |f| + |t| = 1), la oruga izquierda
//queda con (f + t) y la derecha con (f - t), todo multiplicado por la velocidad de reproducción. Se necesita entonces:
//    f = v / N       t = w*d / N       N = |v| + |w*d|              (así |f| + |t| = 1)
//    velocidad de reproducción = N / trackSurfaceSpeed               (cuánto avanza la oruga con el clip a velocidad 1x)
//y queda uL = N*(f + t) = v + w*d  y  uR = N*(f - t) = v - w*d, que es lo pedido.
//
//El Blend Tree es Freeform CARTESIAN a propósito: sobre el rombo |X| + |Y| = 1 reparte los pesos linealmente entre clips
//vecinos (Freeform Directional no: a 25% de giro daba 20/80). Parámetros del Animator:
//    MoveX     = t  (giro: + derecha)         MoveY = f  (avance: + adelante)         MoveSpeed = velocidad de reproducción
public sealed class TankMovementAnimation
{
    //Por debajo de esta demanda (m/s de oruga) el tanque se considera detenido: la animación frena y queda la última dirección.
    private const float StoppedThreshold = 0.05f;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

    private readonly Animator _animator;
    private readonly PlayerMovement _movement;
    private readonly float _trackSurfaceSpeed, _trackHalfGauge, 
                            _directionDamping, _speedDamping;

    public TankMovementAnimation(Animator animator, PlayerMovement movement,
                                float trackSurfaceSpeed, float trackHalfGauge,
                                float directionDamping, float speedDamping)
    {
        _animator = animator;
        _movement = movement;
        _trackSurfaceSpeed = trackSurfaceSpeed;
        _trackHalfGauge = trackHalfGauge;
        _directionDamping = directionDamping;
        _speedDamping = speedDamping;
    }

    public void ArtificialUpdate()
    {
        bool moving = GetTrackBlend(_movement.CurrentSpeed, _movement.CurrentTurnRate, _trackHalfGauge, _trackSurfaceSpeed,
                                    out float turn, out float forward, out float playbackSpeed);

        //Detenido: la dirección se conserva (el Blend Tree no pasa por (0,0), donde sus pesos no están definidos)
        //y solo baja la velocidad de reproducción, así las orugas frenan en la pose en la que estaban.
        if(moving)
        {
            _animator.SetFloat(MoveXHash, turn, _directionDamping, Time.deltaTime);
            _animator.SetFloat(MoveYHash, forward, _directionDamping, Time.deltaTime);
        }

        _animator.SetFloat(MoveSpeedHash, playbackSpeed, _speedDamping, Time.deltaTime);
    }

    //Matemática pura de la mezcla (ver cabecera). forwardSpeed en m/s, turnRate en grados/s (+ derecha), halfGauge en m y
    //surfaceSpeed en m/s. Devuelve false si el tanque está detenido (x e y quedan en 0 y no hay dirección que mezclar).
    public static bool GetTrackBlend(float forwardSpeed, float turnRate, float halfGauge, float surfaceSpeed,
                                    out float x, out float y, out float playbackSpeed)
    {
        float turnSpeed = turnRate * Mathf.Deg2Rad * halfGauge; //m/s de diferencia entre orugas por el giro (w*d)
        float demand = Mathf.Abs(forwardSpeed) + Mathf.Abs(turnSpeed);

        if(demand < StoppedThreshold)
        {
            x = y = playbackSpeed = 0f;
            return false;
        }

        x = turnSpeed / demand;
        y = forwardSpeed / demand;
        playbackSpeed = demand / Mathf.Max(surfaceSpeed, 0.0001f);

        return true;
    }
}
