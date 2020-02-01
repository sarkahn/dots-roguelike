using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
#endif

/// <summary>
/// Interaction for Unity's Input System that repeats a button with
/// the given delay and rate while it's being actuated.
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class RepeatedPressInteraction : IInputInteraction
{
    // ------ Plumbing ------

    static RepeatedPressInteraction()
    {
        InputSystem.RegisterInteraction<RepeatedPressInteraction>();
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInPlayer() { }

    // ------ Properties ------

    /// <summary>
    /// The initial delay before the input starts repeating (in seconds).
    /// </summary>
    public float delay = 0.5f;

    /// <summary>
    /// The rate at wich the input is repeated after the delay (in seconds).
    /// </summary>
    public float rate = 0.1f;

    /// <summary>
    /// Amount of actuation required before a control is considered pressed.
    /// </summary>
    /// <remarks>
    /// If zero (default), defaults to <see cref="InputSettings.defaultButtonPressPoint"/>.
    /// </remarks>
    public float pressPoint;

    private float pressPointOrDefault => pressPoint > 0 ? pressPoint : InputSystem.settings.defaultButtonPressPoint;

    private bool m_delayPassed;
    private double m_lastPerformTime;

    public void Process(ref InputInteractionContext context)
    {
        var isActuated = context.ControlIsActuated(pressPointOrDefault);

        switch (context.phase)
        {
            case InputActionPhase.Waiting:
                if (isActuated)
                {
                    context.Started();
                    context.PerformedAndStayPerformed();
                    m_lastPerformTime = context.time;
                    context.SetTimeout(delay);
                }
                break;
            case InputActionPhase.Performed:
                if (!isActuated)
                {
                    context.Canceled();
                }
                else
                {
                    var trigger = false;
                    if (!m_delayPassed)
                    {
                        trigger = context.time - m_lastPerformTime >= delay;
                        if (trigger) m_delayPassed = true;
                    }
                    else
                    {
                        trigger = context.time - m_lastPerformTime >= rate;
                    }
                    if (trigger)
                    {
                        context.PerformedAndStayPerformed();
                        m_lastPerformTime = context.time;
                        context.SetTimeout(rate);
                    }
                }
                break;
        }
    }

    public void Reset()
    {
        m_delayPassed = false;
        m_lastPerformTime = 0;
    }
}
